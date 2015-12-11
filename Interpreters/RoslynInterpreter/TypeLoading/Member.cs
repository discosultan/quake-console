using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuakeConsole
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

    internal class MemberCollection
    {
        public List<string> Names { get; } = new List<string>();
        public List<Type> UnderlyingTypes { get; } = new List<Type>();
        public List<MemberTypes> MemberTypes { get; } = new List<MemberTypes>();
        public List<ParameterInfo[][]> ParamInfos { get; } = new List<ParameterInfo[][]>();
        private readonly List<Member> _members = new List<Member>();

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
                return null;
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
