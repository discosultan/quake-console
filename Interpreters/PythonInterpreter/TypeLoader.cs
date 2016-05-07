using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QuakeConsole
{
    internal class TypeLoader
    {
        // Members starting with these names will not be included.
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

        internal void AddVariable<T>(string name, T obj, int recursionLevel)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

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

            if (_interpreter.InstanceMembers.ContainsKey(type))
                return;

            AddTypeImpl(type, recursionLevel);
        }

        internal bool RemoveVariable(string name)
        {
            _interpreter.Instances.Remove(name);
            _interpreter.InstancesAndStaticsDirty = true;
            return _interpreter.ScriptScope.RemoveVariable(name);
        }

        internal void AddType(Type type, int recursionLevel)
        {
            if (type == null)
                throw new ArgumentException("type");

            AddTypeImpl(type, recursionLevel);            
        }

        internal void AddAssembly(Assembly assembly, int recursionLevel)
        {
            if (assembly == null)
                throw new ArgumentException("assembly");

            assembly.GetTypes().ForEach(x => AddTypeImpl(x, recursionLevel));
        }

        internal void Reset()
        {
            _referencedAssemblies.Clear();
            _addedTypes.Clear();            
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
            if (!LoadTypeInPython(type))
                return false;            

            // Add static.
            if (!_interpreter.Statics.ContainsKey(type.Name))
            {
                _interpreter.Statics.Add(type.Name, new Member { Name = type.Name, Type = type });
                _interpreter.InstancesAndStaticsDirty = true;
            }

            if (recursionLevel-- > 0)
            { 
                // Add static members.
                AddMembers(_interpreter.StaticMembers, type, BindingFlags.Static | BindingFlags.Public, recursionLevel);
                // Add instance members.
                AddMembers(_interpreter.InstanceMembers, type, BindingFlags.Instance | BindingFlags.Public, recursionLevel);
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

                    // NB! There seems to be some unexpected behavior on Mono (v4.2.3.4) using null propagation on extension methods.
                    // Therefore, regular null checks and foreach statements are used!
                    // Issue: https://github.com/discosultan/quake-console/issues/6#issuecomment-217608599

                    //memberInfo.ParamInfos[i]?.ForEach(overload =>
                    //    overload?.ForEach(parameter => AddTypeImpl(parameter.ParameterType, recursionLevel)));

                    ParameterInfo[][] paramInfos = memberInfo.ParamInfos[i];
                    if (paramInfos != null)
                        foreach (ParameterInfo[] paramInfo in paramInfos)
                            if (paramInfo != null)
                                foreach (ParameterInfo param in paramInfo)
                                    AddTypeImpl(param.ParameterType, recursionLevel);                    
                }               
            }
        }
        
        private bool LoadTypeInPython(Type type)
        {
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
