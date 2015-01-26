using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Varus.Paradox.Console.Interpreters.Python.Utilities;

namespace Varus.Paradox.Console.Interpreters.Python
{
    internal class TypeLoader
    {
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

        private readonly PythonInterpreter _interpreter;
        private readonly HashSet<string> _referencedAssemblies = new HashSet<string>();
        private readonly HashSet<Type> _addedTypes = new HashSet<Type>();

        internal TypeLoader(PythonInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        internal void AddVariable<T>(string name, T obj)
        {
            if (name == null) throw new ArgumentException("name");
            if (obj == null) throw new ArgumentException("obj");

            if (_interpreter._instances.ContainsKey(name))
                throw new InvalidOperationException("Variable with the name " + name + " already exists.");

            Type type = typeof(T);
            if (!type.IsPublic)
                throw new InvalidOperationException("Only variables of public type can be added.");
            if (type.DeclaringType != null)
                throw new InvalidOperationException("Nested types are not supported.");

            _interpreter._scriptScope.SetVariable(name, obj);

            // Add instance.
            _interpreter._instances.Add(name, type);
            _interpreter._instancesAndStaticsDirty = true;

            if (_interpreter._instanceMembers.ContainsKey(type)) return;

            AddType(type, true);
        }

        internal void AddTypes(params Type[] types)
        {
            if (types == null) throw new ArgumentException("types");

            types.ForEach(x => AddType(x));
        }

        internal void AddAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentException("assembly");

            AddTypes(assembly.GetTypes());
        }

        internal void Reset()
        {
            _referencedAssemblies.Clear();
            _addedTypes.Clear();
        }

        private bool AddType(Type type, bool includeSubTypes = false)
        {
            if (type == null) return false;

            // Load type and stop if it is already loaded.
            if (!LoadTypeInPython(type)) return false;

            // Add static.
            if (!_interpreter._statics.ContainsKey(type.Name))
            {
                _interpreter._statics.Add(type.Name, type);
                _interpreter._instancesAndStaticsDirty = true;
            }
            // Add static members.
            AddMembers(_interpreter._staticMembers, type, BindingFlags.Static | BindingFlags.Public, includeSubTypes);
            // Add instance members.
            AddMembers(_interpreter._instanceMembers, type, BindingFlags.Instance | BindingFlags.Public, includeSubTypes);

            return true;
        }

        private void AddMembers(IDictionary<Type, MemberCollection> dict, Type type, BindingFlags flags, bool includeSubTypes)
        {
            if (!dict.ContainsKey(type))
            {
                MemberCollection memberInfo = AutocompleteMembersQuery(type.GetMembers(flags));
                dict.Add(type, memberInfo);
                if (includeSubTypes)
                {
                    for (int i = 0; i < memberInfo.Names.Count; i++)
                    {
                        AddType(memberInfo.UnderlyingTypes[i]);
                        if (memberInfo.ParamInfos[i] != null)
                        {
                            memberInfo.ParamInfos[i].ForEach(overload =>
                            {
                                if (overload != null)
                                {
                                    overload.ForEach(parameter => AddType(parameter.ParameterType));
                                }
                            });
                        }
                    }
                }
            }
        }

        private bool LoadTypeInPython(Type type)
        {
            if (type == null || // Not null.
                type.IsGenericType || // Not a generic type (requires special handling).
                !type.IsPublic || // Not a public type.
                type.DeclaringType != null || // IronPython does not support importing nested classes.
                TypeFilters.Any(x => x.Equals(type.Name, PythonInterpreter.StringComparisonMethod)) || // Not filtered.
                !_addedTypes.Add(type)) // Not already added.                 
            {
                return false;
            }

            var assemblyName = type.Assembly.GetName().Name;
            if (_referencedAssemblies.Add(assemblyName))
                _interpreter.RunScript("clr.AddReference('" + assemblyName + "')");

            _interpreter.RunScript("from " + type.Namespace + " import " + type.Name);

            return true;
        }

        private static MemberCollection AutocompleteMembersQuery(IEnumerable<MemberInfo> members)
        {
            var result = new MemberCollection();

            var ordered = members.Where(x => !AutocompleteFilters
                .Any(y => x.Name.StartsWith(y, PythonInterpreter.StringComparisonMethod))) // Filter.
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
                                ? group.Select(x => ((MethodInfo) x).GetParameters()).ToArray()
                                : null
                    };
                });                
            ordered.ForEach(x => result.Add(x.Name, x.Type, x.MemberType, x.Parameters));

            return result;
        }
    }
}
