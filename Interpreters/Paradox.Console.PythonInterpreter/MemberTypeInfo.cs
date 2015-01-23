using System;
using System.Collections.Generic;
using System.Reflection;

namespace Varus.Paradox.Console.PythonInterpreter
{
    internal class MemberTypeInfoCollection
    {
        public List<string> Names { get; private set; }
        public List<Type> UnderlyingTypes { get; private set; }
        public List<MemberTypes> MemberTypes { get; private set; }
        public List<ParameterInfo[]> MethodParamTypes { get; private set; }

        public MemberTypeInfoCollection()
        {
            Names = new List<string>();
            UnderlyingTypes = new List<Type>();
            MemberTypes = new List<MemberTypes>();
            MethodParamTypes = new List<ParameterInfo[]>();
        }

        public void Add(string name, Type type, MemberTypes memberType, ParameterInfo[] methodParamTypes)
        {
            Names.Add(name);
            UnderlyingTypes.Add(type);
            MemberTypes.Add(memberType);
            MethodParamTypes.Add(methodParamTypes);
        }
    }

    internal struct MemberTypeInfo
    {        
        public Type Type { get; set; }        
        public bool IsInstance { get; set; }
        public string Name { get; set; }
        public MemberTypes MemberType { get; set; }
        public ParameterInfo[] ParameterInfo { get; set; }
    }
}
