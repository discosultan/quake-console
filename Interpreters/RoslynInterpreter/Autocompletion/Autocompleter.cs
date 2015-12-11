using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QuakeConsole
{
    // Following the W.E.T principle with this one (from PythonInterpreter).    
    internal class Autocompleter
    {
        private const string NewKeyword = "new ";
        private const char AccessorSymbol = '.';
        private const char AssignmentSymbol = '=';
        private const char SpaceSymbol = ' ';
        private const char FunctionStartSymbol = '(';
        private const char FunctionEndSymbol = ')';
        private const char FunctionParamSeparatorSymbol = ',';
        private const char ArrayStartSymbol = '[';
        private const char ArrayEndSymbol = ']';
        private static readonly char[] Operators =
        {
            '+', '-', '*', '/', '%'
        };
        private static readonly char[] AutocompleteBoundaryDenoters =
        {
            '(', ')', '{', '}', '/', '=', '.', ',', '+', '-', '*', '%', ' ' //, '[', ']'
        };
        private static readonly Dictionary<Type, string[]> PredefinedAutocompleteEntries = new Dictionary<Type, string[]>
        {
            { typeof(bool), new[] { "false", "true" } }
        };

        internal readonly Dictionary<Type, string[]> _instancesAndStaticsForTypes = new Dictionary<Type, string[]>();
        internal readonly List<string> _instancesAndStatics = new List<string>();

        private readonly TypeLoader _typeLoader;

        public Autocompleter(TypeLoader typeLoader)
        {
            _typeLoader = typeLoader;
        }

        public void Autocomplete(IConsoleInput consoleInput, bool isNextValue)
        {
            // Which context we in, method or regular.
            AutocompletionContextResult contextResult = FindAutocompletionContext(consoleInput);
            Type typeToPrefer = null;
            if (contextResult.Context == AutocompletionContext.Method)
            {
                long newCommandLength_whichParamAt_newStartIndex_numParams = FindParamIndexNewStartIndexAndNumParams(consoleInput, contextResult.StartIndex);
                int chainEndIndex = FindPreviousLinkEndIndex(consoleInput, contextResult.StartIndex - 1);
                if (chainEndIndex >= 0)
                {
                    Stack<string> accessorChain = FindAccessorChain(consoleInput, chainEndIndex);
                    Member lastChainLink = FindLastChainLinkMember(accessorChain);
                    if (lastChainLink?.ParameterInfo != null)
                    {
                        var numParams = (int)(newCommandLength_whichParamAt_newStartIndex_numParams & 0xff);
                        ParameterInfo[] overload = null;
                        for (int i = numParams; i <= lastChainLink.ParameterInfo.Max(x => x.Length); i++)
                        {
                            ParameterInfo[] overloadCandidate = lastChainLink.ParameterInfo.FirstOrDefault(x => x.Length == i);
                            if (overloadCandidate != null)
                            {
                                overload = overloadCandidate;
                                break;
                            }
                        }
                        if (overload != null)
                        {
                            var paramIndex = newCommandLength_whichParamAt_newStartIndex_numParams >> 32 & 0xff;
                            if (overload.Length > paramIndex)
                                typeToPrefer = overload[paramIndex].ParameterType;
                        }
                    }
                }
            }

            int autocompleteBoundaryIndices = FindBoundaryIndices(consoleInput, consoleInput.CaretIndex);
            int startIndex = autocompleteBoundaryIndices & 0xff;
            int length = autocompleteBoundaryIndices >> 16;
            string command = consoleInput.Substring(startIndex, length);
            AutocompletionType completionType = FindAutocompleteType(consoleInput, startIndex);

            if (completionType == AutocompletionType.Regular)
            {
                if (typeToPrefer == null || !string.IsNullOrWhiteSpace(command))
                    FindAutocompleteForEntries(consoleInput, InstancesAndStatics, command, startIndex, isNextValue, completionType);
                else
                    FindAutocompleteForEntries(consoleInput, GetAvailableNamesForType(typeToPrefer), command, startIndex, isNextValue, completionType);
            }
            else // Accessor or assignment or method.
            {
                // We also need to find the value for whatever was before the type accessor.
                int chainEndIndex = FindPreviousLinkEndIndex(consoleInput, startIndex - 1);
                if (chainEndIndex < 0)
                    return;

                Stack<string> accessorChain = FindAccessorChain(consoleInput, chainEndIndex);
                Member lastChainLink = FindLastChainLinkMember(accessorChain);
                // If no types were found, that means we are assigning a new variable.
                // Provide all autocomplete entries in that scenario.
                if (lastChainLink == null)
                {
                    FindAutocompleteForEntries(consoleInput, InstancesAndStatics, command, startIndex, isNextValue, completionType);
                    return;
                }

                switch (completionType)
                {
                    case AutocompletionType.Accessor:
                        MemberCollection autocompleteValues;
                        if (lastChainLink.IsInstance)
                            _typeLoader.InstanceMembers.TryGetValue(lastChainLink.Type, out autocompleteValues);
                        else
                            _typeLoader.StaticMembers.TryGetValue(lastChainLink.Type, out autocompleteValues);
                        if (autocompleteValues == null) break;
                        FindAutocompleteForEntries(consoleInput, autocompleteValues.Names, command, startIndex, isNextValue, completionType);
                        break;
                    case AutocompletionType.Assignment:
                        FindAutocompleteForEntries(
                            consoleInput,
                            GetAvailableNamesForType(lastChainLink.Type),
                            command,
                            startIndex,
                            isNextValue,
                            completionType);
                        break;
                }
            }
        }

        public void Reset()
        {
            _instancesAndStatics.Clear();
            _instancesAndStaticsForTypes.Clear();
        }

        private List<string> InstancesAndStatics
        {
            get
            {
                if (_typeLoader.InstancesAndStaticsDirty)
                {
                    Reset();
                    _instancesAndStatics.AddRange(_typeLoader.Instances.Select(x => x.Key)
                        .OrderBy(x => x)
                        .Union(_typeLoader.Statics.OrderBy(x => x.Key).SelectMany(x => new[] { NewKeyword + x.Key, x.Key })));
                    _typeLoader.InstancesAndStaticsDirty = false;
                }
                return _instancesAndStatics;
            }
        }

        private static AutocompletionContextResult FindAutocompletionContext(IConsoleInput consoleInput)
        {
            var result = new AutocompletionContextResult { StartIndex = 0 };
            for (int i = Math.Min(consoleInput.CaretIndex, consoleInput.Length - 1); i >= 0; i--)
            {
                if (consoleInput[i] == FunctionEndSymbol)
                {
                    result.Context = AutocompletionContext.Regular;
                    break;
                }
                if (consoleInput[i] == FunctionStartSymbol)
                {
                    result.Context = AutocompletionContext.Method;
                    result.StartIndex = i + 1;
                    break;
                }
            }
            return result;
        }

        // returns slices of 16 bit values. [X] are not used and should be removed.
        // [X] new command length | which param we at | [X] new start index  | num params
        private static long FindParamIndexNewStartIndexAndNumParams(IConsoleInput consoleInput, int startIndex)
        {
            int whichParamWeAt = 0;
            int numParams = 1;
            int newStartIndex = startIndex;
            int newCommandLength = 0;
            for (int i = startIndex; i < consoleInput.Length; i++)
            {
                if (consoleInput[i] == FunctionEndSymbol)
                    break;
                if (consoleInput[i] == FunctionParamSeparatorSymbol)
                {
                    newStartIndex = i + 1;
                    newCommandLength = 0;
                    numParams++;
                }
                else
                {
                    newCommandLength++;
                }
                if (consoleInput.CaretIndex == i + 1) whichParamWeAt = (numParams - 1);
            }
            return ((long)newCommandLength << 48) + ((long)whichParamWeAt << 32) + (newStartIndex << 16) + numParams;
        }

        private string[] GetAvailableNamesForType(Type type)
        {
            string[] results;
            if (!_instancesAndStaticsForTypes.TryGetValue(type, out results))
            {
                IEnumerable<string> resultsQuery = _typeLoader.Instances.Where(x => type.IsAssignableFrom(x.Value.Type))
                    .Union(_typeLoader.Statics.Where(x => type.IsAssignableFrom(x.Value.Type)))
                    .Select(x => x.Key);

                string[] predefined;
                if (PredefinedAutocompleteEntries.TryGetValue(type, out predefined))
                    resultsQuery = predefined.Union(resultsQuery);

                results = resultsQuery.ToArray();

                _instancesAndStaticsForTypes.Add(type, results);
            }
            return results;
        }

        private static int FindBoundaryIndices(IConsoleInput consoleInput, int lookupIndex)
        {
            if (consoleInput.Length == 0)
                return 0;

            int previousIndex = lookupIndex - 1;

            // Find start index.
            for (int i = previousIndex; i >= 0; i--)
            {
                if (AutocompleteBoundaryDenoters.Any(x => x == consoleInput[i]))
                    break;
                lookupIndex = i;
            }

            // Find length.
            int length = 0;
            if (previousIndex >= 0)
            {
                for (int i = lookupIndex; i < consoleInput.Length; i++)
                {
                    if (AutocompleteBoundaryDenoters.Any(x => x == consoleInput[i]) || consoleInput[i] == SpaceSymbol)
                        break;
                    length++;
                }
            }

            return lookupIndex + (length << 16);
        }

        private static int FindPreviousLinkEndIndex(IConsoleInput consoleInput, int startIndex)
        {
            int chainEndIndex = -1;
            for (int i = startIndex; i >= 0; i--)
                if (consoleInput[i] == AccessorSymbol ||
                    consoleInput[i] == AssignmentSymbol ||
                    consoleInput[i] == FunctionStartSymbol)
                {
                    chainEndIndex = i - 1;
                    break;
                }
            return chainEndIndex;
        }

        private static AutocompletionType FindAutocompleteType(IConsoleInput consoleInput, int startIndex)
        {
            if (startIndex == 0)
                return AutocompletionType.Regular;
            startIndex--;

            // Does not take into account what was before the accessor or assignment symbol.
            for (int i = startIndex; i >= 0; i--)
            {
                char c = consoleInput[i];
                if (c == SpaceSymbol) continue;
                if (c == AccessorSymbol) return AutocompletionType.Accessor;
                if (c == AssignmentSymbol)
                {
                    if (i <= 0) return AutocompletionType.Assignment;
                    // If we have for example == or += instead of =, use regular autocompletion.
                    char prev = consoleInput[i - 1];
                    return prev == AssignmentSymbol || Operators.Any(x => x == prev)
                        ? AutocompletionType.Regular
                        : AutocompletionType.Assignment;
                }
                return AutocompletionType.Regular;
            }
            return AutocompletionType.Regular;
        }

        private readonly Stack<string> _accessorChain = new Stack<string>();
        private Stack<string> FindAccessorChain(IConsoleInput consoleInput, int chainEndIndex)
        {
            _accessorChain.Clear();
            while (true)
            {
                int indices = FindBoundaryIndices(consoleInput, chainEndIndex);
                int startIndex = indices & 0xff;
                int length = indices >> 16;

                string chainLink = consoleInput.Substring(startIndex, length).Trim();
                _accessorChain.Push(chainLink);

                int previousLinkEndIndex = FindPreviousLinkEndIndex(consoleInput, startIndex - 1);
                if (chainEndIndex < 0)
                    return _accessorChain;

                AutocompletionType chainType = FindAutocompleteType(consoleInput, startIndex);
                if (chainType == AutocompletionType.Accessor)
                {
                    chainEndIndex = previousLinkEndIndex;
                    continue;
                }
                break;
            }
            return _accessorChain;
        }

        private Member FindLastChainLinkMember(Stack<string> accessorChain)
        {
            if (accessorChain.Count == 0)
                return null;

            string link = accessorChain.Pop();

            bool isArrayIndexer = IsArrayIndexer(link, out link);

            Member member;
            if (_typeLoader.Instances.TryGetValue(link, out member))
                member.IsInstance = true;
            else if (_typeLoader.Statics.TryGetValue(link, out member))
                member.IsInstance = false;
            else
                return null;

            if (isArrayIndexer)
                member = ResolveIndexerType(member);

            if (accessorChain.Count == 0)
                return member;

            while (true)
            {
                link = accessorChain.Pop();
                MemberCollection membersCollection;
                if (member.IsInstance)
                {
                    if (!_typeLoader.InstanceMembers.TryGetValue(member.Type, out membersCollection))
                        return null;
                }
                else // static type
                {
                    if (!_typeLoader.StaticMembers.TryGetValue(member.Type, out membersCollection))
                        return null;
                }

                isArrayIndexer = IsArrayIndexer(link, out link);

                member = membersCollection.TryGetMemberByName(link, true);
                if (member == null)
                    return null;

                if (isArrayIndexer)
                    member = ResolveIndexerType(member);

                if (accessorChain.Count == 0)
                    return member;
            }
        }

        private static bool IsArrayIndexer(string link, out string subLink)
        {
            bool insideArrayAccessor = false;
            for (int i = link.Length - 1; i >= 0; i--)
            {
                char c = link[i];
                if (c == ArrayEndSymbol)
                {
                    insideArrayAccessor = true;
                    continue;
                }

                if (insideArrayAccessor && c == ArrayEndSymbol)
                    break;

                if (insideArrayAccessor && c == ArrayStartSymbol)
                {
                    subLink = link.Substring(0, i);
                    return true;
                }
            }
            subLink = link;
            return false;
        }

        private Member ResolveIndexerType(Member member)
        {
            var type = member.Type.GetElementType();
            if (type != null && _typeLoader.Statics.TryGetValue(type.Name, out member))
                member.IsInstance = true;
            return member;
        }

        private static void FindAutocompleteForEntries(IConsoleInput consoleInput, IList<string> autocompleteEntries, 
            string command, int startIndex, bool isNextValue, AutocompletionType completionType)
        {
            int index = autocompleteEntries.IndexOf(x => x.Equals(command, StringComparison.Ordinal));
            if (index == -1 || consoleInput.LastAutocompleteEntry == null)
                consoleInput.LastAutocompleteEntry = command;

            string inputEntry = consoleInput.LastAutocompleteEntry;
            Func<string, bool> predicate = x => x.StartsWith(inputEntry, StringComparison.Ordinal);
            int firstIndex = autocompleteEntries.IndexOf(predicate);
            if (firstIndex == -1)
                return;
            int lastIndex = autocompleteEntries.LastIndexOf(predicate);
            if (index == -1)
                index = firstIndex - 1;

            if (isNextValue)
            {
                index++;
                if (index > lastIndex)
                    index = firstIndex;
            }
            else
            {
                index--;
                if (index < firstIndex)
                    index = lastIndex;                
            }
            string autocompleteValue = autocompleteEntries[index];
            
            //if (completionType == AutocompletionType.Regular)
            //    autocompleteValue = nameof(ExpandoWrapper.globals) + AccessorSymbol + autocompleteValue;

            SetAutocompleteValue(consoleInput, startIndex, autocompleteValue);
        }

        private static void SetAutocompleteValue(IConsoleInput consoleInput, int startIndex, string autocompleteEntry)
        {
            consoleInput.Remove(startIndex, consoleInput.Length - startIndex);
            consoleInput.Append(autocompleteEntry);
        }
    }
}
