using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Hosting;
using Varus.Paradox.Console.Interpreters.Python.Utilities;

namespace Varus.Paradox.Console.Interpreters.Python
{
    /// <summary>
    /// Runs <see cref="ConsoleShell"/> commands through an IronPython parser. Supports loading .NET types
    /// and provides autocomplete for them.
    /// </summary>
    public class PythonInterpreter : ICommandInterpreter
    {
        private const StringComparison StringComparisonMethod = StringComparison.Ordinal;

        private readonly ScriptEngine _scriptEngine = IronPython.Hosting.Python.CreateEngine();
        private ScriptScope _scriptScope;

        private readonly HashSet<string> _referencedAssemblies = new HashSet<string>();
        private readonly HashSet<Type> _addedTypes = new HashSet<Type>();        

        private bool _initialized;

        // Autocomplete entries.
        private readonly Dictionary<Type, MemberCollection> _staticMembers = new Dictionary<Type, MemberCollection>();
        private readonly Dictionary<Type, MemberCollection> _instanceMembers = new Dictionary<Type, MemberCollection>();
        private readonly Dictionary<string, Type> _instances = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _statics = new Dictionary<string, Type>();        
        private readonly Dictionary<Type, string[]> _instancesAndStaticsForTypes = new Dictionary<Type,string[]>();
        private bool _instancesAndStaticsDirty = true;
        private string[] _instancesAndStatics;

        // Members starting with these names will not be included in autocomplete entries.
        private static readonly string[] AutocompleteFilters =
        {
            ".ctor", // Constructor.
            "op_", // Operators.
            "add_", "remove_", // Events.
            "get_", "set_" // Properties.
        };

        private static readonly string[] TypeFilters =
        {
            "Void"
        };
        private static readonly char[] Operators =
        {
            '+', '-', '*', '/', '%'
        };
        private static readonly char[] AutocompleteBoundaryDenoters =
        {
            '(', ')', '[', ']', '{', '}', '/', '=', '.'
        };

        private const char AccessorSymbol = '.';
        private const char AssignmentSymbol = '=';
        private const char SpaceSymbol = ' ';
        private const char FunctionStartSymbol = '(';
        private const char FunctionParamSeparatorSymbol = ',';

        /// <summary>
        /// Constructs a new instance of <see cref="PythonInterpreter"/>.
        /// </summary>
        public PythonInterpreter()
        {
            Reset();            
            EchoEnabled = true;
        }

        /// <summary>
        /// Gets or sets if the user entered command should be shown in the output.
        /// </summary>
        public bool EchoEnabled { get; set; }

        /// <summary>
        /// Executes a command by running it through the IronPython parser.
        /// </summary>
        /// <param name="outputBuffer">Console output buffer to append any output messages.</param>
        /// <param name="command">Command to execute.</param>
        public void Execute(IOutputBuffer outputBuffer, string command)
        {
            if (!_initialized)
            {
                var memStream = new MemoryStream();
                var pythonWriter = new OutputBufferWriter(memStream, outputBuffer);
                _scriptEngine.Runtime.IO.SetOutput(memStream, pythonWriter);
                _scriptEngine.Runtime.IO.SetErrorOutput(memStream, pythonWriter);
                _initialized = true;
            }

            if (EchoEnabled) outputBuffer.Append(command);

            string resultStr;            
            try
            {                
                dynamic result = RunScript(command);
                resultStr = result == null ? null : result.ToString();                
            }
            catch (Exception ex)
            {
                resultStr = ex.Message;
            }
            
            outputBuffer.Append(resultStr);
        }

        /// <summary>
        /// Adds a search path for the IronPython engine to look for when importing modules.
        /// </summary>
        /// <param name="path">Path to add.</param>
        public void AddSearchPath(string path)
        {
            string dir = Path.GetDirectoryName(path);

            if (string.IsNullOrWhiteSpace(dir)) return;

            ICollection<string> paths = _scriptEngine.GetSearchPaths();
            paths.Add(dir);             
            _scriptEngine.SetSearchPaths(paths);
        }

        /// <summary>
        /// Runs a script straight on IronPython engine.
        /// </summary>
        /// <param name="script">Script to run.</param>
        /// <returns>Value returned by the IronPython engine.</returns>
        public dynamic RunScript(string script)
        {
            return _scriptEngine.CreateScriptSourceFromString(script).Compile().Execute(_scriptScope);
        }

        /// <summary>
        /// Resets the IronPython engine scope, clears any imported modules and .NET types.
        /// </summary>
        public void Reset()
        {
            _scriptScope = _scriptEngine.CreateScope();
            _referencedAssemblies.Clear();
            _addedTypes.Clear();
            _instanceMembers.Clear();
            _staticMembers.Clear();
            _instances.Clear();
            _statics.Clear();
            _instancesAndStatics = null;
            _instancesAndStaticsDirty = true;
            RunScript("import clr");
        }


        #region Autocomplete

        /// <summary>
        /// Tries to autocomplete the current input value in the <see cref="ConsoleShell"/> <see cref="InputBuffer"/>.
        /// </summary>
        /// <param name="inputBuffer">Console input.</param>
        /// <param name="isNextValue">True if user wants to autocomplete to the next value; false if to the previous value.</param>
        public void Autocomplete(IInputBuffer inputBuffer, bool isNextValue)
        {
            int autocompleteBoundaryIndices = FindBoundaryIndices(inputBuffer, inputBuffer.Caret.Index);
            int startIndex = autocompleteBoundaryIndices & 0xff;
            int length = autocompleteBoundaryIndices >> 16;
            string command = inputBuffer.Substring(startIndex, length);
            AutocompletionType completionType = FindAutocompleteType(inputBuffer, startIndex);

            if (completionType == AutocompletionType.Regular)
            {
                if (_instancesAndStaticsDirty)
                {
                    _instancesAndStatics =
                        _instances.Select(x => x.Key)
                            .OrderBy(x => x)
                            .Union(_statics.Select(x => x.Key).OrderBy(x => x))
                            .ToArray();
                    _instancesAndStaticsForTypes.Clear(); // TODO: Maybe populate it here already? Currently deferred.
                    _instancesAndStaticsDirty = false;
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
                            _instanceMembers.TryGetValue(lastChainLink.Type, out autocompleteValues);
                        else
                            _staticMembers.TryGetValue(lastChainLink.Type, out autocompleteValues);
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
            if (_instancesAndStaticsDirty || !_instancesAndStaticsForTypes.TryGetValue(type, out results))
            {
                results = _instances.Where(x => x.Value == type)
                    .Union(_statics.Where(x => x.Value == type))
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
            if (_instances.TryGetValue(link, out type))
            {                                
                memberType = new Member { IsInstance = true, Type = type };
            }
            else if (_statics.TryGetValue(link, out type))
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
                    if (!_instanceMembers.TryGetValue(memberType.Type, out memberInfo)) return _members;
                }
                else // static type
                {
                    if (!_staticMembers.TryGetValue(memberType.Type, out memberInfo)) return _members;
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
            int index = autocompleteEntries.IndexOf(x => x.Equals(command, StringComparisonMethod));            
            if (index == -1 || inputBuffer.LastAutocompleteEntry == null) inputBuffer.LastAutocompleteEntry = command;
            
            string inputEntry = inputBuffer.LastAutocompleteEntry;
            Func<string, bool> predicate = x => x.StartsWith(inputEntry, StringComparisonMethod);
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
        
        #endregion


        #region Type Loading

        /// <summary>
        /// Adds a variable to the IronPython environment.
        /// </summary>
        /// <typeparam name="T">Type of variable to add.</typeparam>
        /// <param name="name">Name of the variable.</param>
        /// <param name="obj">Object to add.</param>
        public void AddVariable<T>(string name, T obj)
        {
            if (name == null) throw new ArgumentException("name");
            if (obj == null) throw new ArgumentException("obj");

            if (_instances.ContainsKey(name))
                throw new InvalidOperationException("Variable with the name " + name + " already exists.");

            Type type = typeof(T);
            if (!type.IsPublic)
                throw new InvalidOperationException("Only variables of public type can be added.");
            if (type.DeclaringType != null)
                throw new InvalidOperationException("Nested types are not supported.");

            _scriptScope.SetVariable(name, obj);
            
            // Add instance.
            _instances.Add(name, type);
            _instancesAndStaticsDirty = true;

            if (_instanceMembers.ContainsKey(type)) return;
            
            AddType(type, true);
        }

        /// <summary>
        /// Loads types to IronPython.
        /// </summary>
        /// <param name="types">Types to load.</param>
        public void AddTypes(params Type[] types)
        {
            if (types == null) throw new ArgumentException("types");

            types.ForEach(x => AddType(x));
        }

        /// <summary>
        /// Loads all the public non-nested types from the assembly to IronPython.
        /// </summary>
        /// <param name="assembly">Assembly to get types from.</param>
        public void AddAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentException("assembly");

            AddTypes(assembly.GetTypes());
        }
        
        private bool AddType(Type type, bool includeSubTypes = false)
        {
            if (type == null) return false;

            // Load type and stop if it is already loaded.
            if (!LoadTypeInPython(type)) return false;

            // Add static.
            if (!_statics.ContainsKey(type.Name))
            {
                _statics.Add(type.Name, type);
                _instancesAndStaticsDirty = true;                
            }
            // Add static members.
            AddMembers(_staticMembers, type, BindingFlags.Static | BindingFlags.Public, includeSubTypes);            
            // Add instance members.
            AddMembers(_instanceMembers, type, BindingFlags.Instance | BindingFlags.Public, includeSubTypes);
            
            return true;
        }

        private void AddMembers(Dictionary<Type, MemberCollection> dict, Type type, BindingFlags flags, bool includeSubTypes)
        {
            if (!dict.ContainsKey(type))
            {                
                MemberCollection memberInfo = AutocompleteMembersQuery(type.GetMembers(flags));
                dict.Add(type, memberInfo);
                if (includeSubTypes)
                {
                    memberInfo.UnderlyingTypes.ForEach(x => AddType(x));
                }
            }
        }

        private bool LoadTypeInPython(Type type)
        {
            if (type == null || // Not null.
                type.IsGenericType || // Not a generic type (requires special handling).
                !type.IsPublic || // Not a public type.
                type.DeclaringType != null || // IronPython does not support importing nested classes.
                TypeFilters.Any(x => x.Equals(type.Name, StringComparisonMethod)) || // Not filtered.
                !_addedTypes.Add(type)) // Not already added.                 
            {
                return false;
            }                        

            var assemblyName = type.Assembly.GetName().Name;
            if (_referencedAssemblies.Add(assemblyName))
                RunScript("clr.AddReference('" + assemblyName + "')");            

            RunScript("from " + type.Namespace + " import " + type.Name);

            return true;
        }

        private static MemberCollection AutocompleteMembersQuery(IEnumerable<MemberInfo> members)
        {
            var result = new MemberCollection();

            var ordered = members.Where(x => !AutocompleteFilters.Any(y => x.Name.StartsWith(y, StringComparisonMethod))) // Filter.
                //.DistinctBy(x => x.Name) // Distinctly named values only.
                                 .OrderBy(x => x.Name); // Order alphabetically.
            ordered.ForEach(x =>
            {
                ParameterInfo[] parameters = null;
                if (x.MemberType == MemberTypes.Method)
                {
                    parameters = ((MethodInfo)x).GetParameters();
                }
                result.Add(x.Name, x.GetUnderlyingType(), x.MemberType, parameters);
            });

            return result;
        }

        #endregion        
    }
}
