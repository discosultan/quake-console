using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QuakeConsole.Utilities;

namespace QuakeConsole
{
    internal class Autocompleter
    {
        private const char AccessorSymbol = '.';
        private const char AssignmentSymbol = '=';
        private const char SpaceSymbol = ' ';
        private const char FunctionStartSymbol = '(';
        private const char FunctionEndSymbol = ')';
        private const char FunctionParamSeparatorSymbol = ',';
        private static readonly char[] Operators =
        {
            '+', '-', '*', '/', '%'
        };
        private static readonly char[] AutocompleteBoundaryDenoters =
        {
            '(', ')', '[', ']', '{', '}', '/', '=', '.', ','
        };
        private static readonly Dictionary<Type, string[]> PredefinedAutocompleteEntries = new Dictionary<Type, string[]>
        {
            { typeof(bool), new[] { "False", "True" } }
        };

        private readonly PythonInterpreter _interpreter;

        internal readonly Dictionary<Type, string[]> _instancesAndStaticsForTypes = new Dictionary<Type, string[]>();
        internal readonly List<string> _instancesAndStatics = new List<string>();

        internal Autocompleter(PythonInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        internal List<string> InstancesAndStatics
        {
            get
            {
                if (_interpreter.InstancesAndStaticsDirty)
                {
                    Reset();
                    _instancesAndStatics.AddRange(_interpreter.Instances.Select(x => x.Key)
                        .OrderBy(x => x)
                        .Union(_interpreter.Statics.Select(x => x.Key).OrderBy(x => x)));
                    _interpreter.InstancesAndStaticsDirty = false;
                }
                return _instancesAndStatics;
            }
        }

        internal void Reset()
        {
            _instancesAndStatics.Clear();
            _instancesAndStaticsForTypes.Clear();
        }

        internal void Autocomplete(IInputBuffer inputBuffer, bool isNextValue)
        {
            // Which context we in, method or regular.
            AutocompletionContextResult contextResult = FindAutocompletionContext(inputBuffer, inputBuffer.Caret);
            Type typeToPrefer = null;
            if (contextResult.Context == AutocompletionContext.Method)
            {
                long newCommandLength_whichParamAt_newStartIndex_numParams = FindParamIndexNewStartIndexAndNumParams(inputBuffer, contextResult.StartIndex);
                int chainEndIndex = FindPreviousLinkEndIndex(inputBuffer, contextResult.StartIndex - 1);
                if (chainEndIndex >= 0)
                {
                    Stack<string> accessorChain = FindAccessorChain(inputBuffer, chainEndIndex);
                    Member lastChainLink = FindLastChainLinkMember(accessorChain);                    
                    if (lastChainLink != null && lastChainLink.ParameterInfo != null)
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
                            {
                                typeToPrefer = overload[paramIndex].ParameterType;
                            }                            
                        }
                    }
                }
            }

            int autocompleteBoundaryIndices = FindBoundaryIndices(inputBuffer, inputBuffer.Caret.Index, removeSpaces: false);
            int startIndex = autocompleteBoundaryIndices & 0xff;
            int length = autocompleteBoundaryIndices >> 16;
            string command = inputBuffer.Substring(startIndex, length);
            AutocompletionType completionType = FindAutocompleteType(inputBuffer, startIndex);

            if (completionType == AutocompletionType.Regular)
            {
                if (typeToPrefer == null || !string.IsNullOrWhiteSpace(command))
                {
                    FindAutocompleteForEntries(inputBuffer, InstancesAndStatics, command, startIndex, isNextValue);
                }
                else
                {
                    FindAutocompleteForEntries(inputBuffer, GetAvailableNamesForType(typeToPrefer), command, startIndex, isNextValue);
                }
            }
            else // Accessor or assignment or method.
            {
                // We also need to find the value for whatever was before the type accessor.
                int chainEndIndex = FindPreviousLinkEndIndex(inputBuffer, startIndex - 1);                
                if (chainEndIndex < 0) return;                

                Stack<string> accessorChain = FindAccessorChain(inputBuffer, chainEndIndex);
                Member lastChainLink = FindLastChainLinkMember(accessorChain);
                // If no types were found, that means we are assigning a new variable.
                // Provide all autocomplete entries in that scenario.
                if (lastChainLink == null)
                {
                    FindAutocompleteForEntries(inputBuffer, InstancesAndStatics, command, startIndex, isNextValue);
                    return;
                }

                switch (completionType)
                {
                    case AutocompletionType.Accessor:                        
                        MemberCollection autocompleteValues;
                        if (lastChainLink.IsInstance)
                            _interpreter.InstanceMembers.TryGetValue(lastChainLink.Type, out autocompleteValues);
                        else
                            _interpreter.StaticMembers.TryGetValue(lastChainLink.Type, out autocompleteValues);
                        if (autocompleteValues == null) break;
                        FindAutocompleteForEntries(inputBuffer, autocompleteValues.Names, command, startIndex, isNextValue);
                        break;
                    case AutocompletionType.Assignment:
                        FindAutocompleteForEntries(
                            inputBuffer,
                            GetAvailableNamesForType(lastChainLink.Type),
                            command,
                            startIndex,
                            isNextValue);
                        break;
                }
            }
        }
                
        private static AutocompletionContextResult FindAutocompletionContext(IInputBuffer inputBuffer, ICaret caret)
        {
            var result = new AutocompletionContextResult { StartIndex = 0 };
            for (int i = Math.Min(caret.Index, inputBuffer.Length - 1); i >= 0; i--)
            {
                if (inputBuffer[i] == FunctionEndSymbol)
                {
                    result.Context = AutocompletionContext.Regular;
                    break;
                }
                if (inputBuffer[i] == FunctionStartSymbol)
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
        private static long FindParamIndexNewStartIndexAndNumParams(IInputBuffer inputBuffer, int startIndex)
        {
            int whichParamWeAt = 0;
            int numParams = 1;
            int newStartIndex = startIndex;
            int newCommandLength = 0;
            for (int i = startIndex; i < inputBuffer.Length; i++)
            {                
                if (inputBuffer[i] == FunctionEndSymbol)
                    break;
                if (inputBuffer[i] == FunctionParamSeparatorSymbol)
                {
                    newStartIndex = i + 1;
                    newCommandLength = 0;
                    numParams++;
                }
                else
                {
                    newCommandLength++;
                }
                if (inputBuffer.Caret.Index == i + 1) whichParamWeAt = (numParams - 1);
            }
            return ((long)newCommandLength << 48) + ((long)whichParamWeAt << 32) + (newStartIndex << 16) + numParams;
        }

        private string[] GetAvailableNamesForType(Type type)
        {
            string[] results;
            if (!_instancesAndStaticsForTypes.TryGetValue(type, out results))
            {                                    
                IEnumerable<string> resultsQuery = _interpreter.Instances.Where(x => x.Value.Type == type)
                    .Union(_interpreter.Statics.Where(x => x.Value.Type == type))
                    .Select(x => x.Key);

                string[] predefined;
                if (PredefinedAutocompleteEntries.TryGetValue(type, out predefined))
                    resultsQuery = predefined.Union(resultsQuery);

                results = resultsQuery.ToArray();               

                _instancesAndStaticsForTypes.Add(type, results);
            }
            return results;
        }

        private static int FindBoundaryIndices(IInputBuffer inputBuffer, int lookupIndex, bool removeSpaces = false)
        {
            if (inputBuffer.Length == 0) return 0;

            // Find start index.
            for (int i = lookupIndex; i >= 0; i--)
            {
                if (i >= inputBuffer.Length) continue;
                if (!removeSpaces && (AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i]) || inputBuffer[i] == SpaceSymbol) ||
                    removeSpaces && AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i]))
                    break;
                lookupIndex = i;
            }

            // Find length.
            int length = 0;
            for (int i = lookupIndex; i < inputBuffer.Length; i++)            
            {                
                if (AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i]) || inputBuffer[i] == SpaceSymbol)
                    break;
                length++;
            }

            return lookupIndex + (length << 16);
        }

        private static int FindPreviousLinkEndIndex(IInputBuffer inputBuffer, int startIndex)
        {
            int chainEndIndex = -1;
            for (int i = startIndex; i >= 0; i--)
                if (inputBuffer[i] == AccessorSymbol ||
                    inputBuffer[i] == AssignmentSymbol ||
                    inputBuffer[i] == FunctionStartSymbol)
                {
                    chainEndIndex = i - 1;
                    break;
                }
            return chainEndIndex;
        }

        private static AutocompletionType FindAutocompleteType(IInputBuffer inputBuffer, int startIndex)
        {
            if (startIndex == 0) return AutocompletionType.Regular;
            startIndex--;

            // Does not take into account what was before the accessor or assignment symbol.
            for (int i = startIndex; i >= 0; i--)
            {
                char c = inputBuffer[i];
                if (c == SpaceSymbol) continue;
                if (c == AccessorSymbol) return AutocompletionType.Accessor;                
                if (c == AssignmentSymbol)
                {
                    if (i <= 0) return AutocompletionType.Assignment;
                    // If we have for example == or += instead of =, use regular autocompletion.
                    char prev = inputBuffer[i - 1];
                    return prev == AssignmentSymbol || Operators.Any(x => x == prev)
                        ? AutocompletionType.Regular
                        : AutocompletionType.Assignment;
                }
                return AutocompletionType.Regular;
            }
            return AutocompletionType.Regular;
        }

        private readonly Stack<string> _accessorChain = new Stack<string>();
        private Stack<string> FindAccessorChain(IInputBuffer inputBuffer, int chainEndIndex)
        {
            _accessorChain.Clear();
            while (true)
            {
                int indices = FindBoundaryIndices(inputBuffer, chainEndIndex, removeSpaces: false);
                int startIndex = indices & 0xff;
                int length = indices >> 16;

                string chainLink = inputBuffer.Substring(startIndex, length).Trim();
                _accessorChain.Push(chainLink);

                int previousLinkEndIndex = FindPreviousLinkEndIndex(inputBuffer, startIndex - 1);
                if (chainEndIndex < 0) return _accessorChain;

                AutocompletionType chainType = FindAutocompleteType(inputBuffer, startIndex);
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
            // This expressions should never be true.
            if (accessorChain.Count == 0) return null;

            string link = accessorChain.Pop();
            Member member;            
            if (_interpreter.Instances.TryGetValue(link, out member))
            {
                member.IsInstance = true;                
            }
            else if (_interpreter.Statics.TryGetValue(link, out member))
            {
                member.IsInstance = false;                
            }
            else
            {
                return null;
            }

            if (accessorChain.Count == 0)
            {                
                return member;
            }

            while (true)
            {
                link = accessorChain.Pop();
                MemberCollection membersCollection;
                if (member.IsInstance)
                {
                    if (!_interpreter.InstanceMembers.TryGetValue(member.Type, out membersCollection)) return null;
                }
                else // static type
                {
                    if (!_interpreter.StaticMembers.TryGetValue(member.Type, out membersCollection)) return null;
                }

                member = membersCollection.TryGetMemberByName(link, true);
                if (member == null) return null;                

                if (accessorChain.Count == 0)
                {
                    return member;
                }
            }
        }

        private static void FindAutocompleteForEntries(IInputBuffer inputBuffer, IList<string> autocompleteEntries, string command, int startIndex, bool isNextValue)
        {
            int index = autocompleteEntries.IndexOf(x => x.Equals(command, PythonInterpreter.StringComparisonMethod));            
            if (index == -1 || inputBuffer.LastAutocompleteEntry == null) inputBuffer.LastAutocompleteEntry = command;

            string inputEntry = inputBuffer.LastAutocompleteEntry;
            Func<string, bool> predicate = x => x.StartsWith(inputEntry, PythonInterpreter.StringComparisonMethod);
            int firstIndex = autocompleteEntries.IndexOf(predicate);
            if (firstIndex == -1) return;
            int lastIndex = autocompleteEntries.LastIndexOf(predicate);
            if (index == -1) index = firstIndex - 1;

            if (isNextValue)
            {
                index++;
                if (index > lastIndex) index = firstIndex;
                SetAutocompleteValue(inputBuffer, startIndex, autocompleteEntries[index]);
            }
            else
            {
                index--;
                if (index < firstIndex) index = lastIndex;
                SetAutocompleteValue(inputBuffer, startIndex, autocompleteEntries[index]);
            }
        }

        private static void SetAutocompleteValue(IInputBuffer inputBuffer, int startIndex, string autocompleteEntry)
        {
            inputBuffer.Remove(startIndex, inputBuffer.Length - startIndex);
            inputBuffer.Write(autocompleteEntry);
        }       
    }
}
