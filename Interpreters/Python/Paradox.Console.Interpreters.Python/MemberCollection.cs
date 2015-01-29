using System;
using System.Collections.Generic;
using System.Reflection;

namespace Varus.Paradox.Console.Interpreters.Python
{
    internal class MemberCollection
    {
        public List<string> Names { get; private set; }
        public List<Type> UnderlyingTypes { get; private set; }
        public List<MemberTypes> MemberTypes { get; private set; }
        public List<ParameterInfo[][]> ParamInfos { get; private set; }
        private readonly List<Member> _members = new List<Member>();
 
        public MemberCollection()
        {
            Names = new List<string>();
            UnderlyingTypes = new List<Type>();
            MemberTypes = new List<MemberTypes>();
            ParamInfos = new List<ParameterInfo[][]>();
        }

        public void Add(string name, Type type, MemberTypes memberType, ParameterInfo[][] methodParamTypes)
        {
            Names.Add(name);
            UnderlyingTypes.Add(type);
            MemberTypes.Add(memberType);
            ParamInfos.Add(methodParamTypes);
            _members.Add(new Member
            {
                Name = name,
                Type = type,
                MemberType = memberType,
                ParameterInfo = methodParamTypes
            });
        }

        public Member TryGetMemberByName(string name, bool isInstance)
        {
            int index = Names.IndexOf(name);
            if (index < 0)
            {
                return null;                
            }
            return GetMemberByIndex(index, isInstance);                        
        }

        private Member GetMemberByIndex(int index, bool isInstance)
        {
            Member member = _members[index];
            member.IsInstance = isInstance;
            return member;
        }
    }
}
