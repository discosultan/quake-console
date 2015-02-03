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

        internal void AddVariable<T>(string name, T obj, bool fullyRecursive)
        {
            if (name == null) throw new ArgumentException("name");
            if (obj == null) throw new ArgumentException("obj");

            if (_interpreter.Instances.ContainsKey(name))
                throw new InvalidOperationException("Variable with the name " + name + " already exists.");

            Type type = typeof(T);
            if (!type.IsPublic)
                throw new InvalidOperationException("Only variables of public type can be added.");
            if (type.DeclaringType != null)
                throw new InvalidOperationException("Nested types are not supported.");

            _interpreter.ScriptScope.SetVariable(name, obj);

            // Add instance.
            _interpreter.Instances.Add(name, new Member { Name = name, Type = type });
            _interpreter.InstancesAndStaticsDirty = true;

            if (_interpreter.InstanceMembers.ContainsKey(type)) return;

            AddTypeImpl(type, fullyRecursive);
        }

        internal void AddType(Type type, bool fullyRecursive)
        {
            if (type == null) throw new ArgumentException("type");

            AddTypeImpl(type, fullyRecursive);            
        }

        internal void AddAssembly(Assembly assembly, bool fullyRecursive)
        {
            if (assembly == null) throw new ArgumentException("assembly");

            assembly.GetTypes().ForEach(x => AddTypeImpl(x, fullyRecursive));
        }

        internal void Reset()
        {
            _referencedAssemblies.Clear();
            _addedTypes.Clear();            
        }

        private bool AddTypeImpl(Type type, bool fullyRecursive)
        {
            if (type == null) return false;

            // Load type and stop if it is already loaded.
            if (!LoadTypeInPython(type)) return false;            

            // Add static.
            if (!_interpreter.Statics.ContainsKey(type.Name))
            {
                _interpreter.Statics.Add(type.Name, new Member { Name = type.Name, Type = type });
                _interpreter.InstancesAndStaticsDirty = true;
            }
            // Add static members.
            AddMembers(_interpreter.StaticMembers, type, BindingFlags.Static | BindingFlags.Public, fullyRecursive);
            // Add instance members.
            AddMembers(_interpreter.InstanceMembers, type, BindingFlags.Instance | BindingFlags.Public, fullyRecursive);

            return true;
        }

        private void AddMembers(IDictionary<Type, MemberCollection> dict, Type type, BindingFlags flags, bool fullyRecursive)
        {
            if (!dict.ContainsKey(type))
            {
                MemberCollection memberInfo = AutocompleteMembersQuery(type.GetMembers(flags));
                dict.Add(type, memberInfo);
                if (fullyRecursive)
                {
                    for (int i = 0; i < memberInfo.Names.Count; i++)
                    {                        
                        AddTypeImpl(memberInfo.UnderlyingTypes[i], true);
                        if (memberInfo.ParamInfos[i] != null)
                        {
                            memberInfo.ParamInfos[i].ForEach(overload =>
                            {
                                if (overload != null)
                                {
                                    overload.ForEach(parameter => AddTypeImpl(parameter.ParameterType, true));
                                }
                            });
                        }
                    }
                }
            }
        }
        
        private bool LoadTypeInPython(Type type)
        {
            if (type == null) return false;

            bool isArray = type.IsArray;
            while (type.IsArray)
            {
                type = type.GetElementType();
            }

            if (type.IsGenericType || // Not a generic type (requires special handling).
                !type.IsPublic || // Not a public type.
                //type.IsAbstract && !type.IsSealed || // Not an abstract type. We check for IsSealed because a static class is considered to be abstract AND sealed.
                type.DeclaringType != null || // IronPython does not support importing nested classes.
                TypeFilters.Any(x => x.Equals(type.Name, PythonInterpreter.StringComparisonMethod)) || // Not filtered.
                !_addedTypes.Add(type)) // Not already added.                 
            {
                return false;
            }

            var assemblyName = type.Assembly.GetName().Name;
            if (_referencedAssemblies.Add(assemblyName))
                _interpreter.RunScript("clr.AddReference('" + assemblyName + "')");

            string script = "from " + type.Namespace + " import " + type.Name;            
            _interpreter.RunScript(script);

            return !isArray;
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
