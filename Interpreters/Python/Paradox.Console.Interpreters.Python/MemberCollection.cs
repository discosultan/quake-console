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
        }

        public bool TryGetMemberByName(string name, bool isInstance, out Member memberType)
        {
            int index = Names.IndexOf(name);
            if (index < 0)
            {
                memberType = default(Member);
                return false;
            }
            memberType = GetMemberByIndex(index, isInstance);
            return true;
        }

        private Member GetMemberByIndex(int index, bool isInstance)
        {
            return new Member
            {
                IsInstance = isInstance,
                Name = Names[index],
                MemberType = MemberTypes[index],
                Type = UnderlyingTypes[index],
                ParameterInfo = ParamInfos[index]
            };
        }
    }
}
