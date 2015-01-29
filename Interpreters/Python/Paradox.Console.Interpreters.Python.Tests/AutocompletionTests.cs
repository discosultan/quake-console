using NUnit.Framework;

namespace Varus.Paradox.Console.Interpreters.Python.Tests
{
    [TestFixture]
    public class AutocompletionTests
    {
        private const string InstancePrefix = "instance";
        private const string FirstInstanceName = "instance_a";
        private const string SecondInstanceName = "instance_b";
        private const string StringInstanceNameAndValue = "instance_c";
        private const string LastStaticName = "Type";
        private const string Accessor = ".";
        private const string Assignment = "=";
        private const string Space = " ";
        private const string MethodParamSeparator = ",";
        private const string MethodStart = "(";
        private const string MethodEnd = ")";
        private const string TargetFirstMemberName = "Cymidine";
        private const string TargetSecondMemberName = "Equals";
        private const string TargetLastMemberName = "ToString";
        private const string TargetStringFieldName = "instance_a.Cymidine";
        private const string TargetMethodName = "instance_a.SetBehen";
        private const string EnumTypeName = "Behen";
        private const string EnumFirstMemberName = "Razor";
        private const string TargetMethodSecondParamTypeName = "Pauciloquent";
        private const string StaticTypeName = "Kickup";
        private const string StaticTypeFirstMemberName = "Eider";
        private const string StaticTypeFirstMemberFirstMemberName = "B";

        private IInputBuffer _inputBuffer;
        private ICaret _caret;
        private PythonInterpreter _interpreter;

        private Kickup _target;        

        [SetUp]
        public void Setup()
        {
            _inputBuffer = new FakeInputBuffer();
            _caret = _inputBuffer.Caret;
            _interpreter = new PythonInterpreter();
            _target = new Kickup();
            // Should be automatically ordered by names.
            _interpreter.AddVariable(SecondInstanceName, _target);
            _interpreter.AddVariable(FirstInstanceName, _target);
            _interpreter.AddVariable(StringInstanceNameAndValue, StringInstanceNameAndValue);
        }

        // Loaded instance_a, instance_z, Behen, Eider, Kickup, Pauciloquent, String, Type


        #region General        

        [Test]
        public void NoInput_AutocompleteOnce_FirstInstanceSelected()
        {
            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName, _inputBuffer.Value);
        }

        [Test]
        public void NoInput_AutocompleteTwice_SecondInstanceSelected()
        {
            _interpreter.Autocomplete(_inputBuffer, true);
            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(SecondInstanceName, _inputBuffer.Value);
        }

        [Test]
        public void NoInput_AutocompleteTwiceForwardOnceBackward_FirstInstanceSelected()
        {
            _interpreter.Autocomplete(_inputBuffer, true);
            _interpreter.Autocomplete(_inputBuffer, true);
            _interpreter.Autocomplete(_inputBuffer, false);

            Assert.AreEqual(FirstInstanceName, _inputBuffer.Value);
        }

        [Test]
        public void NoInput_AutocomplateBackward_WrappedToEnd()
        {
            _interpreter.Autocomplete(_inputBuffer, false);

            Assert.AreEqual(LastStaticName, _inputBuffer.Value);
        }

        [Test]
        public void NoInput_AutocompleteBackwardOnceForwardOnce_WrappedToBeginning()
        {
            _interpreter.Autocomplete(_inputBuffer, false);
            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName, _inputBuffer.Value);
        }

        [Test]
        public void FirstInstanceInput_CaretAtZero_Autocomplete_DidNothing()
        {
            _inputBuffer.Value = FirstInstanceName;
            _caret.Index = 0;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName, _inputBuffer.Value);
        }        

        [Test]
        public void InstancePrefix_CaretAtZero_Autocomplete_FirstInstanceSelected()
        {
            _inputBuffer.Value = InstancePrefix;
            _caret.Index = 0;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName, _inputBuffer.Value);
        }

        [Test]
        public void InstancePrefix_CaretAtZero_AutocompleteTwice_SecondInstanceSelected()
        {
            _inputBuffer.Value = InstancePrefix;
            _caret.Index = 0;

            _interpreter.Autocomplete(_inputBuffer, true);
            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(SecondInstanceName, _inputBuffer.Value);
        }

        [Test]
        public void InstancePrefix_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            _inputBuffer.Value = InstancePrefix;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName, _inputBuffer.Value);
        }

        [Test]
        public void FirstInstanceInput_CaretAtEnd_Autocomplete_DidNothing()
        {
            _inputBuffer.Value = FirstInstanceName;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName, _inputBuffer.Value);
        }

        #endregion


        #region Accessor

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            _inputBuffer.Value = FirstInstanceName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetFirstMemberName, _inputBuffer.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_AutocompleteTwice_SecondMemberSelected()
        {
            _inputBuffer.Value = FirstInstanceName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);
            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetSecondMemberName, _inputBuffer.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_AutocompleteBackward_WrappedToEnd()
        {
            _inputBuffer.Value = FirstInstanceName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, false);            

            Assert.AreEqual(FirstInstanceName + Accessor + TargetLastMemberName, _inputBuffer.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_AutocompleteOnceBackwardOnceForward_WrappedToBeginning()
        {
            _inputBuffer.Value = FirstInstanceName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, false);
            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetFirstMemberName, _inputBuffer.Value);
        }

        [Test]
        public void Space_FirstInstanceInput_Accessor_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            _inputBuffer.Value = Space + FirstInstanceName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);


            Assert.AreEqual(Space + FirstInstanceName + Accessor + TargetFirstMemberName, _inputBuffer.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_Space_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            _inputBuffer.Value = FirstInstanceName + Accessor + Space;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(FirstInstanceName + Accessor + Space + TargetFirstMemberName, _inputBuffer.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_AutocompleteToOverloadedMember_CaretAtEnd_Autocomplete_OverloadSkipped()
        {
            _inputBuffer.Value = FirstInstanceName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true); // Cymidine
            _interpreter.Autocomplete(_inputBuffer, true); // Equals            
            _interpreter.Autocomplete(_inputBuffer, true); // GetHashcode
            _interpreter.Autocomplete(_inputBuffer, true); // GetType
            _interpreter.Autocomplete(_inputBuffer, true); // Pauciloquent
            _interpreter.Autocomplete(_inputBuffer, true); // SetBehen            
            _interpreter.Autocomplete(_inputBuffer, true); // ToString

            Assert.AreEqual(FirstInstanceName + Accessor + "ToString", _inputBuffer.Value);
        }

        [Test]
        public void StaticTypeInput_Accessor_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            _inputBuffer.Value = StaticTypeName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(StaticTypeName + Accessor + StaticTypeFirstMemberName, _inputBuffer.Value);
        }

        [Test]
        public void StaticTypeMember_Accessor_CaretAtEnd_Autocomplete_MemberMemberSelected()
        {
            _inputBuffer.Value = StaticTypeName + Accessor + StaticTypeFirstMemberName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(StaticTypeName + Accessor + StaticTypeFirstMemberName + Accessor + StaticTypeFirstMemberFirstMemberName, _inputBuffer.Value);
        }

        #endregion


        #region Assignment

        [Test]
        public void InstanceStringFieldInput_Assignment_CaretAtEnd_Autocomplete_StringInstanceSelected()
        {
            _inputBuffer.Value = TargetStringFieldName + Assignment;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetStringFieldName + Assignment + StringInstanceNameAndValue, _inputBuffer.Value);
        }

        [Test]
        public void InstanceStringFieldInput_Assignment_CaretAtEnd_AutocompleteTwice_StringTypeSelected()
        {
            _inputBuffer.Value = TargetStringFieldName + Assignment;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);
            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetStringFieldName + Assignment + "String", _inputBuffer.Value);
        }

        [Test]
        public void InstanceStringFieldInput_Assignment_Space_CaretAtEnd_Autocomplete_StringInstanceSelected()
        {
            _inputBuffer.Value = TargetStringFieldName + Assignment + Space;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetStringFieldName + Assignment + Space + StringInstanceNameAndValue, _inputBuffer.Value);
        }

        #endregion


        #region Methods

        [Test]
        public void InstanceEnumMethodInput_MethodStart_CaretAtEnd_Autocomplete_FirstParamSelected()
        {
            _inputBuffer.Value = TargetMethodName + MethodStart;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName, _inputBuffer.Value);
        }

        [Test]
        public void InstanceEnumMethodInput_MethodStart_Space_CaretAtEnd_Autocomplete_FirstParamSelected()
        {
            _inputBuffer.Value = TargetMethodName + MethodStart + Space;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetMethodName + MethodStart + Space + EnumTypeName, _inputBuffer.Value);
        }

        [Test]
        public void InstanceEnumMethodInput_MethodStart_Enum_Assignment_CaretAtEnd_Autocomplete_EnumMemberSelected()
        {
            _inputBuffer.Value = TargetMethodName + MethodStart + EnumTypeName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName, _inputBuffer.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_FirstParam_ParamSeparator_CaretAtEnd_Autocomplete_SecondParamSelected()
        {
            _inputBuffer.Value = TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName + MethodParamSeparator;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName + MethodParamSeparator + TargetMethodSecondParamTypeName, _inputBuffer.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_ParamSeparator_MethodEnd_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            _inputBuffer.Value = TargetMethodName + MethodStart + MethodParamSeparator + MethodEnd;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetMethodName + MethodStart + MethodParamSeparator + MethodEnd + FirstInstanceName, _inputBuffer.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_InstancePrefix_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            _inputBuffer.Value = TargetMethodName + MethodStart + InstancePrefix;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetMethodName + MethodStart + FirstInstanceName, _inputBuffer.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_FirstParamType_Accessor_CaretAtEnd_Autocomplete_FirstParamValueSelected()
        {
            _inputBuffer.Value = TargetMethodName + MethodStart + EnumTypeName + Accessor;
            _caret.Index = _inputBuffer.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName, _inputBuffer.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_MethodParamSeparator_CaretAtSeparator_Autocomplete_FirstParamTypeSelected()
        {
            _inputBuffer.Value = TargetMethodName + MethodStart + Accessor;
            _caret.Index = _inputBuffer.Length - Accessor.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName, _inputBuffer.Value);
        }

        [Test]
        [Ignore(@"Currently interpreter is expected to cut input buffer ending after autocompleted value 
        (Command Prompt Style). We might want to change that (to Powershell style for example).")]
        public void InstanceMethodInput_MethodStart_Space_MethodParamSeparator_CaretAtSpace_Autocomplete_FirstParamTypeSelected()
        {
            _inputBuffer.Value = TargetMethodName + MethodStart + Space + Accessor;
            _caret.Index = _inputBuffer.Length - Accessor.Length - Space.Length;

            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + MethodParamSeparator, _inputBuffer.Value);
        }

        #endregion
    }
}
