using NUnit.Framework;
using QuakeConsole.Tests.Utilities;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class MethodAutocompletionTests : TestsBase
    {
        private const string FirstInstanceName = "instance";
        private const string MethodParamSeparator = ",";
        private const string MethodStart = "(";
        private const string MethodEnd = ")";
        private const string TargetMethodName = FirstInstanceName + ".SetBehen";
        private const string EnumTypeName = "Behen";
        private const string EnumFirstMemberName = "Razor";
        private const string TargetMethodSecondParamTypeName = "Pauciloquent";
        private const string StaticTypeMethodName = TargetMethodSecondParamTypeName + ".Horopter";
        private const string StaticTypeMethodParamName = "Behen";

        public override void Setup()
        {
            base.Setup();
            Interpreter.AddVariable(FirstInstanceName, new Kickup(), int.MaxValue);
        }

        [Test]
        public void InstanceEnumMethodInput_MethodStart_CaretAtEnd_Autocomplete_FirstParamSelected()
        {
            Input.Value = TargetMethodName + MethodStart;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName, Input.Value);
        }

        [Test]
        public void InstanceEnumMethodInput_MethodStart_Space_CaretAtEnd_Autocomplete_FirstParamSelected()
        {
            Input.Value = TargetMethodName + MethodStart + Space;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetMethodName + MethodStart + Space + EnumTypeName, Input.Value);
        }

        [Test]
        public void InstanceEnumMethodInput_MethodStart_Enum_Assignment_CaretAtEnd_Autocomplete_EnumMemberSelected()
        {
            Input.Value = TargetMethodName + MethodStart + EnumTypeName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName, Input.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_FirstParam_ParamSeparator_CaretAtEnd_Autocomplete_SecondParamSelected()
        {
            Input.Value = TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName + MethodParamSeparator;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName + MethodParamSeparator + TargetMethodSecondParamTypeName, Input.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_ParamSeparator_MethodEnd_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            Input.Value = TargetMethodName + MethodStart + MethodParamSeparator + MethodEnd;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetMethodName + MethodStart + MethodParamSeparator + MethodEnd + FirstInstanceName, Input.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_InstancePrefix_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            Input.Value = TargetMethodName + MethodStart + FirstInstanceName.Substring(0, FirstInstanceName.Length - 1);
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetMethodName + MethodStart + FirstInstanceName, Input.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_FirstParamType_Accessor_CaretAtEnd_Autocomplete_FirstParamValueSelected()
        {
            Input.Value = TargetMethodName + MethodStart + EnumTypeName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName, Input.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_MethodParamSeparator_CaretAtSeparator_Autocomplete_FirstParamTypeSelected()
        {
            Input.Value = TargetMethodName + MethodStart + Accessor;
            Input.CaretIndex = Input.Length - Accessor.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName, Input.Value);
        }

        [Test]
        [Ignore(@"Currently interpreter is expected to cut input buffer ending after autocompleted value
        (Command Prompt Style). We might want to change that (to Powershell style for example).")]
        public void InstanceMethodInput_MethodStart_Space_MethodParamSeparator_CaretAtSpace_Autocomplete_FirstParamTypeSelected()
        {
            Input.Value = TargetMethodName + MethodStart + Space + Accessor;
            Input.CaretIndex = Input.Length - Accessor.Length - Space.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + MethodParamSeparator, Input.Value);
        }

        [Test]
        public void TwoParamMethod_MethodStart_CaretAtEnd_Autocomplete_FirstParamSelected()
        {
            Input.Value = StaticTypeMethodName + MethodStart;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(StaticTypeMethodName + MethodStart + StaticTypeMethodParamName, Input.Value);
        }
    }
}
