using System;
using System.Reflection;

namespace QuakeConsole.Interpreters
{
    // TODO: pool it?
    internal class Member
    {        
        public Type Type { get; set; }                
        public string Name { get; set; }
        public MemberTypes MemberType { get; set; }
        public ParameterInfo[][] ParameterInfo { get; set; }
        public bool IsInstance { get; set; }
    }
}
