using NUnit.Framework;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class AutocompletionTests
    {
        private const string InstancePrefix = "instance";
        private const string FirstInstanceName = "instance_a";
        private const string SecondInstanceName = "instance_b";
        private const string StringInstanceNameAndValue = "instance_c";
        private const string LastStaticName = "YieldAwaitable";
        private const string Accessor = ".";
        private const string Assignment = "=";
        private const string Space = " ";
        private const string MethodParamSeparator = ",";
        private const string MethodStart = "(";
        private const string MethodEnd = ")";
        private const string TargetFirstMemberName = "Cymidine";
        private const string TargetSecondMemberName = "Equals";
        private const string TargetLastMemberName = "ToString";
        private const string TargetFieldName = FirstInstanceName + ".Cymidine";
        private const string TargetRecursiveFieldName = FirstInstanceName + ".Gusher.Nolt";
        private const string TargetRecursiveFieldTypeMemberName = "Apprehender";
        private const string TargetMethodName = FirstInstanceName + ".SetBehen";
        private const string EnumTypeName = "Behen";
        private const string EnumFirstMemberName = "Razor";
        private const string TargetMethodSecondParamTypeName = "Pauciloquent";
        private const string StaticTypeName = "Kickup";
        private const string StaticTypeFirstMemberName = "Eider";
        private const string StaticTypeFirstMemberFirstMemberName = "B";
        private const string StaticTypeMethodName = TargetMethodSecondParamTypeName + ".Horopter";
        private const string StaticTypeMethodParamName = "Behen";
        private const string StaticClassName = "Podobranchia";
        private const string StaticClassPrefix = "Podo";
        private const string TargetBooleanType = StaticClassName + ".Gymnogen";

        private IConsoleInput _consoleInput;        
        private PythonInterpreter _interpreter;

        private Kickup _target;        

        [SetUp]
        public void Setup()
        {
            _consoleInput = new FakeConsoleInput();            
            _interpreter = new PythonInterpreter();
            _target = new Kickup();
            // Should be automatically ordered by names.
            _interpreter.AddVariable(SecondInstanceName, _target, int.MaxValue);
            _interpreter.AddVariable(FirstInstanceName, _target, int.MaxValue);
            _interpreter.AddVariable(StringInstanceNameAndValue, StringInstanceNameAndValue, int.MaxValue);
            _interpreter.AddType(typeof (Podobranchia), int.MaxValue);
        }

        // Loaded instance_a, instance_z, Behen, Eider, Kickup, Pauciloquent, String, Type


        #region General        

        [Test]
        public void NoInput_AutocompleteOnce_FirstInstanceSelected()
        {
            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName, _consoleInput.Value);
        }

        [Test]
        public void NoInput_AutocompleteTwice_SecondInstanceSelected()
        {
            _interpreter.Autocomplete(_consoleInput, true);
            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(SecondInstanceName, _consoleInput.Value);
        }

        [Test]
        public void NoInput_AutocompleteTwiceForwardOnceBackward_FirstInstanceSelected()
        {
            _interpreter.Autocomplete(_consoleInput, true);
            _interpreter.Autocomplete(_consoleInput, true);
            _interpreter.Autocomplete(_consoleInput, false);

            Assert.AreEqual(FirstInstanceName, _consoleInput.Value);
        }

        [Test]
        public void NoInput_AutocomplateBackward_WrappedToEnd()
        {
            _interpreter.Autocomplete(_consoleInput, false);

            Assert.AreEqual(LastStaticName, _consoleInput.Value);
        }

        [Test]
        public void NoInput_AutocompleteBackwardOnceForwardOnce_WrappedToBeginning()
        {
            _interpreter.Autocomplete(_consoleInput, false);
            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName, _consoleInput.Value);
        }

        [Test]
        public void FirstInstanceInput_CaretAtZero_Autocomplete_DidNothing()
        {
            _consoleInput.Value = FirstInstanceName;
            _consoleInput.CaretIndex = 0;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName, _consoleInput.Value);
        }        

        [Test]
        public void InstancePrefix_CaretAtZero_Autocomplete_FirstInstanceSelected()
        {
            _consoleInput.Value = InstancePrefix;
            _consoleInput.CaretIndex = 0;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName, _consoleInput.Value);
        }

        [Test]
        public void InstancePrefix_CaretAtZero_AutocompleteTwice_SecondInstanceSelected()
        {
            _consoleInput.Value = InstancePrefix;
            _consoleInput.CaretIndex = 0;

            _interpreter.Autocomplete(_consoleInput, true);
            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(SecondInstanceName, _consoleInput.Value);
        }

        [Test]
        public void InstancePrefix_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            _consoleInput.Value = InstancePrefix;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName, _consoleInput.Value);
        }

        [Test]
        public void FirstInstanceInput_CaretAtEnd_Autocomplete_DidNothing()
        {
            _consoleInput.Value = FirstInstanceName;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName, _consoleInput.Value);
        }

        [Test]
        public void StaticClassPrefixInput_CaretAtEnd_Autocomplete_StaticTypeSelected()
        {
            _consoleInput.Value = StaticClassPrefix;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(StaticClassName, _consoleInput.Value);
        }

        #endregion


        #region Accessor

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            _consoleInput.Value = FirstInstanceName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetFirstMemberName, _consoleInput.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_AutocompleteTwice_SecondMemberSelected()
        {
            _consoleInput.Value = FirstInstanceName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);
            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetSecondMemberName, _consoleInput.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_AutocompleteBackward_WrappedToEnd()
        {
            _consoleInput.Value = FirstInstanceName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, false);            

            Assert.AreEqual(FirstInstanceName + Accessor + TargetLastMemberName, _consoleInput.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_AutocompleteOnceBackwardOnceForward_WrappedToBeginning()
        {
            _consoleInput.Value = FirstInstanceName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, false);
            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetFirstMemberName, _consoleInput.Value);
        }

        [Test]
        public void Space_FirstInstanceInput_Accessor_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            _consoleInput.Value = Space + FirstInstanceName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);


            Assert.AreEqual(Space + FirstInstanceName + Accessor + TargetFirstMemberName, _consoleInput.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_Space_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            _consoleInput.Value = FirstInstanceName + Accessor + Space;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(FirstInstanceName + Accessor + Space + TargetFirstMemberName, _consoleInput.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_AutocompleteToOverloadedMember_CaretAtEnd_Autocomplete_OverloadSkipped()
        {
            _consoleInput.Value = FirstInstanceName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true); // Cymidine
            _interpreter.Autocomplete(_consoleInput, true); // Equals            
            _interpreter.Autocomplete(_consoleInput, true); // GetHashcode
            _interpreter.Autocomplete(_consoleInput, true); // GetType
            _interpreter.Autocomplete(_consoleInput, true); // Gusher
            _interpreter.Autocomplete(_consoleInput, true); // Pauciloquent
            _interpreter.Autocomplete(_consoleInput, true); // SetBehen            
            _interpreter.Autocomplete(_consoleInput, true); // ToString

            Assert.AreEqual(FirstInstanceName + Accessor + "ToString", _consoleInput.Value);
        }

        [Test]
        public void StaticTypeInput_Accessor_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            _consoleInput.Value = StaticTypeName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(StaticTypeName + Accessor + StaticTypeFirstMemberName, _consoleInput.Value);
        }

        [Test]
        public void StaticTypeMember_Accessor_CaretAtEnd_Autocomplete_MemberMemberSelected()
        {
            _consoleInput.Value = StaticTypeName + Accessor + StaticTypeFirstMemberName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(StaticTypeName + Accessor + StaticTypeFirstMemberName + Accessor + StaticTypeFirstMemberFirstMemberName, _consoleInput.Value);
        }

        [Test]
        public void InstanceTypeMembersMember_Accessor_CaretAtEnd_Autocomplete_TypeLoaderWasRecursive()
        {
            _consoleInput.Value = TargetRecursiveFieldName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetRecursiveFieldName + Accessor + TargetRecursiveFieldTypeMemberName, _consoleInput.Value);
        }

        #endregion


        #region Assignment

        [Test]
        public void InstanceStringFieldInput_Assignment_CaretAtEnd_Autocomplete_StringInstanceSelected()
        {
            _consoleInput.Value = TargetFieldName + Assignment;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetFieldName + Assignment + StringInstanceNameAndValue, _consoleInput.Value);
        }

        [Test]
        public void InstanceStringFieldInput_Assignment_CaretAtEnd_AutocompleteTwice_StringTypeSelected()
        {
            _consoleInput.Value = TargetFieldName + Assignment;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);
            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetFieldName + Assignment + "String", _consoleInput.Value);
        }

        [Test]
        public void InstanceStringFieldInput_Assignment_Space_CaretAtEnd_Autocomplete_StringInstanceSelected()
        {
            _consoleInput.Value = TargetFieldName + Assignment + Space;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetFieldName + Assignment + Space + StringInstanceNameAndValue, _consoleInput.Value);
        }

        [Test]
        public void BoolType_Assignment_CaretAtEnd_Autocomplete_PredefinedTypeSelected()
        {
            _consoleInput.Value = TargetBooleanType + Assignment;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetBooleanType + Assignment + "False", _consoleInput.Value);
        }

        [Test]
        public void NewVariable_Assignment_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            _consoleInput.Value = "x" + Assignment;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual("x" + Assignment + FirstInstanceName, _consoleInput.Value);
        }

        #endregion


        #region Methods

        [Test]
        public void InstanceEnumMethodInput_MethodStart_CaretAtEnd_Autocomplete_FirstParamSelected()
        {
            _consoleInput.Value = TargetMethodName + MethodStart;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName, _consoleInput.Value);
        }

        [Test]
        public void InstanceEnumMethodInput_MethodStart_Space_CaretAtEnd_Autocomplete_FirstParamSelected()
        {
            _consoleInput.Value = TargetMethodName + MethodStart + Space;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetMethodName + MethodStart + Space + EnumTypeName, _consoleInput.Value);
        }

        [Test]
        public void InstanceEnumMethodInput_MethodStart_Enum_Assignment_CaretAtEnd_Autocomplete_EnumMemberSelected()
        {
            _consoleInput.Value = TargetMethodName + MethodStart + EnumTypeName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName, _consoleInput.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_FirstParam_ParamSeparator_CaretAtEnd_Autocomplete_SecondParamSelected()
        {
            _consoleInput.Value = TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName + MethodParamSeparator;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName + MethodParamSeparator + TargetMethodSecondParamTypeName, _consoleInput.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_ParamSeparator_MethodEnd_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            _consoleInput.Value = TargetMethodName + MethodStart + MethodParamSeparator + MethodEnd;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetMethodName + MethodStart + MethodParamSeparator + MethodEnd + FirstInstanceName, _consoleInput.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_InstancePrefix_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            _consoleInput.Value = TargetMethodName + MethodStart + InstancePrefix;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetMethodName + MethodStart + FirstInstanceName, _consoleInput.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_FirstParamType_Accessor_CaretAtEnd_Autocomplete_FirstParamValueSelected()
        {
            _consoleInput.Value = TargetMethodName + MethodStart + EnumTypeName + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + Accessor + EnumFirstMemberName, _consoleInput.Value);
        }

        [Test]
        public void InstanceMethodInput_MethodStart_MethodParamSeparator_CaretAtSeparator_Autocomplete_FirstParamTypeSelected()
        {
            _consoleInput.Value = TargetMethodName + MethodStart + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length - Accessor.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName, _consoleInput.Value);
        }

        [Test]
        [Ignore(@"Currently interpreter is expected to cut input buffer ending after autocompleted value 
        (Command Prompt Style). We might want to change that (to Powershell style for example).")]
        public void InstanceMethodInput_MethodStart_Space_MethodParamSeparator_CaretAtSpace_Autocomplete_FirstParamTypeSelected()
        {
            _consoleInput.Value = TargetMethodName + MethodStart + Space + Accessor;
            _consoleInput.CaretIndex = _consoleInput.Length - Accessor.Length - Space.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(TargetMethodName + MethodStart + EnumTypeName + MethodParamSeparator, _consoleInput.Value);
        }

        [Test]
        public void TwoParamMethod_MethodStart_CaretAtEnd_Autocomplete_FirstParamSelected()
        {
            _consoleInput.Value = StaticTypeMethodName + MethodStart;
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual(StaticTypeMethodName + MethodStart + StaticTypeMethodParamName, _consoleInput.Value);
        }

        #endregion
    }
}
