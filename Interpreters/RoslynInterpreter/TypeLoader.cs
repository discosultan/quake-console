using System;
using System.Collections.Generic;
using System.Dynamic;

namespace QuakeConsole
{
    internal class TypeLoader
    {
        private readonly RoslynInterpreter _interpreter;        

        public TypeLoader(RoslynInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public void AddVariable<T>(string name, T obj, int recursionLevel)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (recursionLevel < 0)
                throw new ArgumentOutOfRangeException(nameof(recursionLevel), "Recursion level must be positive.");

            ((IDictionary<string, object>) _interpreter.Globals).Add(name, obj);            

            //_interpreter.Globals.Add(name, obj);

            //if (_interpreter.Instances.ContainsKey(name))
            //    throw new InvalidOperationException("Variable with the name " + name + " already exists.");

            //Type type = typeof(T);
            //if (!type.IsPublic)
            //    throw new InvalidOperationException("Only variables of public type can be added.");
            //if (type.DeclaringType != null)
            //    throw new InvalidOperationException("Nested types are not supported.");

            //_interpreter.ScriptScope.SetVariable(name, obj);

            //// Add instance.
            //_interpreter.Instances.Add(name, new Member { Name = name, Type = type });
            //_interpreter.InstancesAndStaticsDirty = true;

            //if (_interpreter.InstanceMembers.ContainsKey(type))
            //    return;

            //AddTypeImpl(type, recursionLevel);
        }

        public void Reset() { }
    }
}
