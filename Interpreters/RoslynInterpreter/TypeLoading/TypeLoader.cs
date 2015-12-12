using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QuakeConsole
{
    // Following the W.E.T principle with this one (from PythonInterpreter).
    internal class TypeLoader
    {
        // Members starting with these names will not be loaded.
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

        private readonly RoslynInterpreter _interpreter;

        private readonly HashSet<string> _referencedAssemblies = new HashSet<string>();
        private readonly HashSet<string> _imports = new HashSet<string>();
        private readonly HashSet<Type> _addedTypes = new HashSet<Type>();

        public TypeLoader(RoslynInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public Dictionary<Type, MemberCollection> StaticMembers { get; } = new Dictionary<Type, MemberCollection>();
        public Dictionary<Type, MemberCollection> InstanceMembers { get; } = new Dictionary<Type, MemberCollection>();
        public Dictionary<string, Member> Instances { get; } = new Dictionary<string, Member>();
        public Dictionary<string, Member> Statics { get; } = new Dictionary<string, Member>();
        public bool InstancesAndStaticsDirty { get; set; }

        public void AddVariable<T>(string name, T obj, int recursionLevel)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (recursionLevel < 0)
                throw new ArgumentOutOfRangeException(nameof(recursionLevel), "Recursion level must be positive.");

            if (Instances.ContainsKey(name))
                throw new InvalidOperationException("Variable with the name " + name + " already exists.");

            Type type = typeof(T);
            if (!type.IsPublic)
                throw new InvalidOperationException("Only variables of public type can be added.");            

            ((IDictionary<string, object>)_interpreter.Globals.globals).Add(name, obj);

            // Add instance.
            Instances.Add(name, new Member { Name = name, Type = type });
            InstancesAndStaticsDirty = true;

            if (InstanceMembers.ContainsKey(type))
                return;

            AddTypeImpl(type, recursionLevel);
        }

        public bool RemoveVariable(string name)
        {
            Instances.Remove(name);
            InstancesAndStaticsDirty = true;
            return ((IDictionary<string, object>) _interpreter.Globals.globals).Remove(name);            
        }

        public void Reset()
        {
            _referencedAssemblies.Clear();
            _addedTypes.Clear();
            _imports.Clear();
        }

        public void AddAssembly(Assembly assembly, int recursionLevel)
        {                        
            assembly.GetTypes().ForEach(type => AddTypeImpl(type, recursionLevel));
        }

        public void AddType(Type type, int recursionLevel)
        {
            AddTypeImpl(type, recursionLevel);
        }

        private bool AddTypeImpl(Type type, int recursionLevel)
        {
            if (type == null)
                return false;

            if (type.IsArray)
            {
                AddTypeImpl(type.GetElementType(), recursionLevel);
                return false;
            }

            // Load type and stop if it is already loaded.
            if (!LoadTypeToScriptContext(type))
                return false;

            // Add static.
            if (!Statics.ContainsKey(type.Name))
            {
                Statics.Add(type.Name, new Member { Name = type.Name, Type = type });
                InstancesAndStaticsDirty = true;
            }

            if (recursionLevel-- > 0)
            {
                // Add static members.
                AddMembers(StaticMembers, type, BindingFlags.Static | BindingFlags.Public, recursionLevel);
                // Add instance members.
                AddMembers(InstanceMembers, type, BindingFlags.Instance | BindingFlags.Public, recursionLevel);
            }

            return true;
        }

        private void AddMembers(IDictionary<Type, MemberCollection> dict, Type type, BindingFlags flags, int recursionLevel)
        {
            if (!dict.ContainsKey(type))
            {
                MemberCollection memberInfo = AutocompleteMembersQuery(type.GetMembers(flags));
                dict.Add(type, memberInfo);
                for (int i = 0; i < memberInfo.Names.Count; i++)
                {
                    AddTypeImpl(memberInfo.UnderlyingTypes[i], recursionLevel);
                    if (memberInfo.ParamInfos[i] != null)
                    {
                        memberInfo.ParamInfos[i].ForEach(overload =>
                        {
                            overload?.ForEach(parameter => AddTypeImpl(parameter.ParameterType, recursionLevel));
                        });
                    }
                }
            }
        }

        private bool LoadTypeToScriptContext(Type type)
        {
            if (type.IsGenericType || // Not a generic type (requires special handling).
                !type.IsPublic || // Not a public type.                
                TypeFilters.Any(x => x.Equals(type.Name, StringComparison.Ordinal)) || // Not filtered.
                !_addedTypes.Add(type)) // Not already added.                 
            {
                return false;
            }

            string assemblyName = type.Assembly.GetName().Name;
            if (_referencedAssemblies.Add(assemblyName))
                _interpreter.ScriptOptions = _interpreter.ScriptOptions.AddReferences(type.Assembly);

            string namespaceName = type.Namespace;
            if (_imports.Add(namespaceName))
                _interpreter.ScriptOptions = _interpreter.ScriptOptions.AddImports(namespaceName);

            return true;
        }

        private static MemberCollection AutocompleteMembersQuery(IEnumerable<MemberInfo> members)
        {
            var result = new MemberCollection();

            var ordered = members.Where(x => !AutocompleteFilters
                .Any(y => x.Name.StartsWith(y, StringComparison.Ordinal))) // Filter.
                .GroupBy(x => x.Name) // Distinctly named values only.
                .OrderBy(x => x.Key) // Order alphabetically.            
                .Select(group => // Pick member from first, param overloads from all
                {
                    MemberInfo firstMember = group.First();
                    return new
                    {
                        firstMember.Name,
                        Type = firstMember.GetUnderlyingType(),
                        firstMember.MemberType,
                        Parameters =
                            firstMember.MemberType == MemberTypes.Method
                                ? group.Select(x => ((MethodInfo)x).GetParameters()).ToArray()
                                : null
                    };
                });
            ordered.ForEach(x => result.Add(x.Name, x.Type, x.MemberType, x.Parameters));

            return result;
        }
    }
}
