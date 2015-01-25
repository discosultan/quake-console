using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Varus.Paradox.Console.Interpreters.Python.Utilities;

namespace Varus.Paradox.Console.Interpreters.Python
{
    internal class Autocompleter
    {
        private const char AccessorSymbol = '.';
        private const char AssignmentSymbol = '=';
        private const char SpaceSymbol = ' ';
        private const char FunctionStartSymbol = '(';
        private const char FunctionParamSeparatorSymbol = ',';
        private static readonly char[] Operators =
        {
            '+', '-', '*', '/', '%'
        };
        private static readonly char[] AutocompleteBoundaryDenoters =
        {
            '(', ')', '[', ']', '{', '}', '/', '=', '.'
        }; 

        private readonly PythonInterpreter _interpreter;

        internal readonly Dictionary<Type, string[]> _instancesAndStaticsForTypes = new Dictionary<Type, string[]>();
        internal readonly List<string> _instancesAndStatics = new List<string>();

        internal Autocompleter(PythonInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        internal void Reset()
        {
            _instancesAndStatics.Clear();
        }

        internal void Autocomplete(IInputBuffer inputBuffer, bool isNextValue)
        {
            int autocompleteBoundaryIndices = FindBoundaryIndices(inputBuffer, inputBuffer.Caret.Index);
            int startIndex = autocompleteBoundaryIndices & 0xff;
            int length = autocompleteBoundaryIndices >> 16;
            string command = inputBuffer.Substring(startIndex, length);
            AutocompletionType completionType = FindAutocompleteType(inputBuffer, startIndex);

            if (completionType == AutocompletionType.Regular)
            {
                if (_interpreter._instancesAndStaticsDirty)
                {
                    _instancesAndStatics.AddRange(_interpreter._instances.Select(x => x.Key)
                        .OrderBy(x => x)
                        .Union(_interpreter._statics.Select(x => x.Key).OrderBy(x => x)));
                    _instancesAndStaticsForTypes.Clear(); // TODO: Maybe populate it here already? Currently deferred.
                    _interpreter._instancesAndStaticsDirty = false;
                }
                FindAutocompleteForEntries(inputBuffer, _instancesAndStatics, command, startIndex, isNextValue);
            }
            else // Accessor or assignment or method.
            {
                // We also need to find the value for whatever was before the type accessor.
                int chainEndIndex = FindPreviousLinkEndIndex(inputBuffer, startIndex - 1);
                if (chainEndIndex < 0) return;
                Stack<string> accessorChain = FindAccessorChain(inputBuffer, chainEndIndex);
                List<Member> lastChainLinks = FindLastChainLinkMembers(accessorChain);
                if (lastChainLinks.Count == 0) return;

                switch (completionType)
                {
                    case AutocompletionType.Accessor:
                        Member lastChainLink = lastChainLinks[0];
                        MemberCollection autocompleteValues;
                        if (lastChainLink.IsInstance)
                            _interpreter._instanceMembers.TryGetValue(lastChainLink.Type, out autocompleteValues);
                        else
                            _interpreter._staticMembers.TryGetValue(lastChainLink.Type, out autocompleteValues);
                        if (autocompleteValues == null) break;
                        FindAutocompleteForEntries(inputBuffer, autocompleteValues.Names, command, startIndex, isNextValue);
                        break;
                    case AutocompletionType.Assignment:
                        FindAutocompleteForEntries(
                            inputBuffer,
                            GetAvailableNamesForType(lastChainLinks[0].Type),
                            command,
                            startIndex,
                            isNextValue);
                        break;
                    case AutocompletionType.Method:
                        // Find number of params from current input.
                        long newCommandLength_whichParamAt_newStartIndex_numParams = FindParamIndexNewStartIndexAndNumParams(inputBuffer, startIndex);
                        // Match member with that number of params.
                        Member paramsMember = lastChainLinks.FirstOrDefault(x => x.ParameterInfo.Length == (newCommandLength_whichParamAt_newStartIndex_numParams & 0xff));
                        if (paramsMember == null) break;
                        ParameterInfo[] parameters = paramsMember.ParameterInfo;
                        // Find which param we are at.                        
                        // Profit.                                            
                        var newStartIndex = (int)(newCommandLength_whichParamAt_newStartIndex_numParams >> 16 & 0xff);
                        FindAutocompleteForEntries(
                            inputBuffer,
                            GetAvailableNamesForType(parameters[newCommandLength_whichParamAt_newStartIndex_numParams >> 32 & 0xff].ParameterType),
                            inputBuffer.Substring(newStartIndex, (int)(newCommandLength_whichParamAt_newStartIndex_numParams >> 48)),
                            newStartIndex,
                            isNextValue);
                        break;
                }
            }
        }

        // returns slices of 16 bit values: new command length | which param we at | new start index  | num params
        private static long FindParamIndexNewStartIndexAndNumParams(IInputBuffer inputBuffer, int startIndex)
        {
            int whichParamWeAt = 0;
            int numParams = 1;
            int newStartIndex = startIndex;
            int newCommandLength = 0;
            for (int i = startIndex; i < inputBuffer.Length; i++)
            {

                if (AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i])) break;
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
            if (_interpreter._instancesAndStaticsDirty || !_instancesAndStaticsForTypes.TryGetValue(type, out results))
            {
                results = _interpreter._instances.Where(x => x.Value == type)
                    .Union(_interpreter._statics.Where(x => x.Value == type))
                    .Select(x => x.Key)
                    .ToArray();
                _instancesAndStaticsForTypes.Add(type, results);
            }
            return results;
        }

        private static int FindBoundaryIndices(IInputBuffer inputBuffer, int lookupIndex, bool allowSpaces = false)
        {
            if (inputBuffer.Length == 0) return 0;

            // Find start index.
            for (int i = lookupIndex; i >= 0; i--)
            {
                if (i >= inputBuffer.Length) continue;
                if (!allowSpaces && AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i] || x == SpaceSymbol) ||
                    allowSpaces && AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i]))
                    break;
                lookupIndex = i;
            }

            // Find length.
            int length = 0;
            for (int i = lookupIndex; i < inputBuffer.Length; i++)
            {
                if (!allowSpaces && AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i] || x == SpaceSymbol) ||
                    allowSpaces && AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i]))
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
                if (c == FunctionStartSymbol || c == FunctionParamSeparatorSymbol) return AutocompletionType.Method;
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
                int indices = FindBoundaryIndices(inputBuffer, chainEndIndex, true);
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

        private readonly List<Member> _members = new List<Member>();
        // Returns a collection because last chain link might very well be a method with overloads,
        // where each overload is represented by a different member.
        private List<Member> FindLastChainLinkMembers(Stack<string> accessorChain)
        {
            _members.Clear();

            // This expressions should never be true.
            if (accessorChain.Count == 0) return _members;

            string link = accessorChain.Pop();
            Member memberType;
            Type type;
            if (_interpreter._instances.TryGetValue(link, out type))
            {
                memberType = new Member { IsInstance = true, Type = type };
            }
            else if (_interpreter._statics.TryGetValue(link, out type))
            {
                memberType = new Member { IsInstance = false, Type = type };
            }
            else
            {
                return _members;
            }

            if (accessorChain.Count == 0)
            {
                _members.Add(memberType);
                return _members;
            }

            while (true)
            {
                link = accessorChain.Pop();
                MemberCollection memberInfo;
                if (memberType.IsInstance)
                {
                    if (!_interpreter._instanceMembers.TryGetValue(memberType.Type, out memberInfo)) return _members;
                }
                else // static type
                {
                    if (!_interpreter._staticMembers.TryGetValue(memberType.Type, out memberInfo)) return _members;
                }

                if (!memberInfo.TryGetMemberByName(link, memberType.IsInstance, out memberType))
                    return _members;

                if (accessorChain.Count == 0)
                {
                    if (memberType.MemberType == MemberTypes.Method)
                    {
                        _members.AddRange(memberInfo.GetMembersForOverloads(link, memberType.IsInstance));
                    }
                    else
                    {
                        _members.Add(memberType);
                    }
                    return _members;
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
