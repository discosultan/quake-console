using System;
using System.Collections.Generic;

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

            ((IDictionary<string, object>) _interpreter.Globals.globals).Add(name, obj);
        }

        public void Reset() { }
    }
}
