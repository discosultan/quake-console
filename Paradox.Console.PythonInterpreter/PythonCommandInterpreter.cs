using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Varus.Paradox.Console.PythonInterpreter.Utilities;

namespace Varus.Paradox.Console.PythonInterpreter
{
    public class PythonCommandInterpreter : IPythonInterpreter
    {
        private readonly ScriptEngine _scriptEngine = Python.CreateEngine();
        private ScriptScope _scriptScope;

        private readonly HashSet<string> _referencedAssemblies = new HashSet<string>();
        private readonly HashSet<Type> _addedTypes = new HashSet<Type>();
        private readonly Stack<string> _accessorChain = new Stack<string>();

        private bool _initialized;

        // Autocomplete entries.
        private readonly Dictionary<Type, MemberTypeInfoCollection> _staticMembers = new Dictionary<Type, MemberTypeInfoCollection>();
        private readonly Dictionary<Type, MemberTypeInfoCollection> _instanceMembers = new Dictionary<Type, MemberTypeInfoCollection>();
        private readonly Dictionary<string, Type> _instances = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _statics = new Dictionary<string, Type>();
        private bool _instancesAndStaticsDirty = true;
        private string[] _instancesAndStatics;        

        // Members starting with these names will not be included in autocomplete entries.
        private readonly string[] AutocompleteFilters =
        {
            ".ctor", // Constructor.
            "op_", // Operators.
            "add_", "remove_", // Events.
            "get_", "set_" // Properties.
        };

        private readonly string[] TypeFilters =
        {
            "Void"
        };

        private readonly char[] AutocompleteBoundaryDenoters =
        {
            ' ', '(', ')', '[', ']', '{', '}', '/', '=', '.'
        };
        private const char AccessorSymbol = '.';
        private const char AssignmentSymbol = '=';
        private const char SpaceSymbol = ' ';

        public PythonCommandInterpreter()
        {
            Reset();            
            EchoEnabled = true;
        }

        public bool EchoEnabled { get; set; }

        public void Execute(OutputBuffer viewBuffer, string command)
        {
            if (!_initialized)
            {
                var memStream = new MemoryStream();
                var pythonWriter = new PythonOutputBufferWriter(memStream, viewBuffer);
                _scriptEngine.Runtime.IO.SetOutput(memStream, pythonWriter);
                _scriptEngine.Runtime.IO.SetErrorOutput(memStream, pythonWriter);
                _initialized = true;
            }

            if (EchoEnabled) viewBuffer.Append(command);

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
            
            viewBuffer.Append(resultStr);
        }

        public void AddSearchPath(string path)
        {
            string dir = Path.GetDirectoryName(path);
            ICollection<string> paths = _scriptEngine.GetSearchPaths();
            if (!string.IsNullOrWhiteSpace(dir)) paths.Add(dir);             
            _scriptEngine.SetSearchPaths(paths);
        }

        public dynamic RunScript(string script)
        {
            return _scriptEngine.CreateScriptSourceFromString(script).Compile().Execute(_scriptScope);
        }

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

        public void Autocomplete(InputBuffer inputBuffer, bool isNextValue)
        {
            int autocompleteBoundaryIndices = FindBoundaryIndices(inputBuffer, inputBuffer.Caret.Index);
            int startIndex = autocompleteBoundaryIndices & 0xff;
            int length = autocompleteBoundaryIndices >> 16;
            var command = inputBuffer.Substring(startIndex, length);
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
                    _instancesAndStaticsDirty = false;
                }
                FindAutocompleteForEntries(inputBuffer, _instancesAndStatics, command, startIndex, isNextValue);
            }
            else // Accessor or assignment.
            {
                // We also need to find the value for whatever was before the type accessor.
                int chainEndIndex = FindPreviousLinkEndIndex(inputBuffer, startIndex - 1);
                if (chainEndIndex < 0) return;
                _accessorChain.Clear();
                FindAccessorChain(inputBuffer, chainEndIndex);
                MemberTypeInfo? lastChainLink = FindMemberTypeInfo();
                if (!lastChainLink.HasValue) return;

                if (completionType == AutocompletionType.Accessor)
                {
                    MemberTypeInfoCollection autocompleteValues;
                    if (lastChainLink.Value.IsInstance)
                        _instanceMembers.TryGetValue(lastChainLink.Value.Type, out autocompleteValues);
                    else
                        _staticMembers.TryGetValue(lastChainLink.Value.Type, out autocompleteValues);
                    if (autocompleteValues == null) return;
                    FindAutocompleteForEntries(inputBuffer, autocompleteValues.Names, command, startIndex, isNextValue);
                }
                else // Assignment.
                {
                    FindAutocompleteForEntries(
                        inputBuffer,
                        _instances.Where(x => x.Value == lastChainLink.Value.Type)
                            .Union(_statics.Where(x => x.Value == lastChainLink.Value.Type))
                            .Select(x => x.Key).ToArray(),
                        command, 
                        startIndex, 
                        isNextValue);
                }
            }
        }        

        private int FindBoundaryIndices(InputBuffer inputBuffer, int lookupIndex, bool allowSpaces = false)
        {
            if (inputBuffer.Length == 0) return 0;

            // Find start index.
            for (int i = lookupIndex; i >= 0; i--)
            {                
                if (i >= inputBuffer.Length) continue;
                if (!allowSpaces && AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i]) ||
                    allowSpaces && AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i] && x != SpaceSymbol))                
                    break;                
                lookupIndex = i;
            }

            // Find length.
            int length = 0;
            for (int i = lookupIndex; i < inputBuffer.Length; i++)
            {
                if (!allowSpaces && AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i]) ||
                    allowSpaces && AutocompleteBoundaryDenoters.Any(x => x == inputBuffer[i] && x != SpaceSymbol))
                    break;                
                length++;
            }

            return lookupIndex + (length << 16);
        }

        private int FindPreviousLinkEndIndex(InputBuffer inputBuffer, int startIndex)
        {
            int chainEndIndex = -1;
            for (int i = startIndex; i >= 0; i--)
                if (inputBuffer[i] == AccessorSymbol || inputBuffer[i] == AssignmentSymbol)
                {
                    chainEndIndex = i - 1;
                    break;
                }
            return chainEndIndex;
        }

        private static AutocompletionType FindAutocompleteType(InputBuffer inputBuffer, int startIndex)
        {
            if (startIndex == 0) return AutocompletionType.Regular;            
            startIndex--;            

            // Does not take into account what was before the accessor or assignment symbol.
            for (int i = startIndex; i >= 0; i--)
            {
                char c = inputBuffer[i];
                if (c == SpaceSymbol) continue;                
                if (c == AccessorSymbol) return AutocompletionType.Accessor;
                if (c == AssignmentSymbol) return AutocompletionType.Assignment;
                return AutocompletionType.Regular;
            }
            return AutocompletionType.Regular;
        }

        private void FindAccessorChain(InputBuffer inputBuffer, int chainEndIndex)
        {
            while (true)
            {
                int indices = FindBoundaryIndices(inputBuffer, chainEndIndex, true);
                int startIndex = indices & 0xff;
                int length = indices >> 16;

                var chainLink = inputBuffer.Substring(startIndex, length).Trim();
                _accessorChain.Push(chainLink);

                int previousLinkEndIndex = FindPreviousLinkEndIndex(inputBuffer, startIndex - 1);
                if (chainEndIndex < 0) return;

                AutocompletionType chainType = FindAutocompleteType(inputBuffer, startIndex);
                if (chainType == AutocompletionType.Accessor)
                {
                    chainEndIndex = previousLinkEndIndex;
                    continue;
                }
                break;
            }
        }

        private MemberTypeInfo? FindMemberTypeInfo()
        {
            // This expressions should never be true.
            if (_accessorChain.Count == 0) return null;

            string link = _accessorChain.Pop();
            Type type;
            if (_instances.TryGetValue(link, out type))
            {                
                return FindMemberTypeInfo(new MemberTypeInfo { IsInstance = true, Type = type });
            }
            if (_statics.TryGetValue(link, out type))
            {                
                return FindMemberTypeInfo(new MemberTypeInfo { IsInstance = false, Type = type });
            }

            return null;
        }

        private MemberTypeInfo? FindMemberTypeInfo(MemberTypeInfo previous)
        {
            while (true)
            {
                if (_accessorChain.Count == 0) return previous;

                string link = _accessorChain.Pop();
                MemberTypeInfoCollection memberInfo;
                if (previous.IsInstance)
                {
                    if (!_instanceMembers.TryGetValue(previous.Type, out memberInfo)) return null;
                }
                else // static type
                {
                    if (!_staticMembers.TryGetValue(previous.Type, out memberInfo)) return null;
                }

                int indexOfLink = memberInfo.Names.IndexOf(link);
                if (indexOfLink == -1) return null;

                Type type = memberInfo.UnderlyingTypes[indexOfLink];
                previous = new MemberTypeInfo { IsInstance = true, Type = type };
            }
        }

        private static void FindAutocompleteForEntries(InputBuffer inputBuffer, IList<string> autocompleteEntries, string command, int startIndex, bool isNextValue)
        {
            int index = autocompleteEntries.IndexOf(x => x.Equals(command, StringComparison.Ordinal));            
            if (index == -1 || inputBuffer.LastAutocompleteEntry == null) inputBuffer.LastAutocompleteEntry = command;
            
            string inputEntry = inputBuffer.LastAutocompleteEntry;
            Func<string, bool> predicate = x => x.StartsWith(inputEntry, StringComparison.Ordinal);
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

        private static void SetAutocompleteValue(InputBuffer inputBuffer, int startIndex, string autocompleteEntry)
        {            
            inputBuffer.Remove(startIndex, inputBuffer.Length - startIndex);            
            inputBuffer.Write(autocompleteEntry);
        }
        
        #endregion


        #region Type Loading

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

        public void AddTypes(params Type[] types)
        {
            if (types == null) throw new ArgumentException("types");

            types.ForEach(x => AddType(x));
        }

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
            AddMembers(_staticMembers, type, BindingFlags.Public | BindingFlags.Static, includeSubTypes);            
            // Add instance members.
            AddMembers(_instanceMembers, type, BindingFlags.Instance | BindingFlags.Public, includeSubTypes);
            
            return true;
        }

        private void AddMembers(Dictionary<Type, MemberTypeInfoCollection> dict, Type type, BindingFlags flags, bool includeSubTypes)
        {
            if (!dict.ContainsKey(type))
            {                
                MemberTypeInfoCollection memberTypeInfo = AutocompleteMembersQuery(type.GetMembers(flags));
                dict.Add(type, memberTypeInfo);
                if (includeSubTypes)
                {
                    memberTypeInfo.UnderlyingTypes.ForEach(x => AddType(x));
                }
            }
        }

        private bool LoadTypeInPython(Type type)
        {
            if (type == null || // Not null.
                type.IsGenericType || // Not a generic type (requires special handling).
                !type.IsPublic || // Not a public type.
                type.DeclaringType != null || // IronPython does not support importing nested classes.
                TypeFilters.Any(x => x.Equals(type.Name, StringComparison.Ordinal)) || // Not filtered.
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

        private MemberTypeInfoCollection AutocompleteMembersQuery(IEnumerable<MemberInfo> members)
        {
            var result = new MemberTypeInfoCollection();

            var ordered = members.Where(x => !AutocompleteFilters.Any(y => x.Name.StartsWith(y, StringComparison.Ordinal))) // Filter.
                                 .DistinctBy(x => x.Name) // Distinctly named values only.
                                 .OrderBy(x => x.Name); // Order alphabetically.
            ordered.ForEach(x => result.Add(x.Name, x.GetUnderlyingType(), x.MemberType));

            return result;
        }

        #endregion        
    }
}
