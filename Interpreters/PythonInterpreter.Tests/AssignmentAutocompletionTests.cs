using NUnit.Framework;
using QuakeConsole.Tests.Utilities;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class AssignmentAutocompletionTestses : TestsBase
    {
        private const string FirstInstanceName = "instance";
        private const string StaticClassName = "Podobranchia";
        private const string TargetFieldName = FirstInstanceName + ".Cymidine";
        private const string TargetBooleanType = StaticClassName + ".Gymnogen";
        private const string StringInstanceNameAndValue = "instance_c";

        public override void Setup()
        {
            base.Setup();
            Interpreter.AddVariable(FirstInstanceName, new Kickup(), int.MaxValue);
            Interpreter.AddVariable(StringInstanceNameAndValue, StringInstanceNameAndValue, int.MaxValue);
            Interpreter.AddType(typeof(Podobranchia), int.MaxValue);
        }

        [Test]
        public void DerivedInstanceAssignableToBaseInstanceType()
        {
            var baseVar = new Base();
            var derivedVar = new Derived();
            Interpreter.AddVariable("base", baseVar);
            Interpreter.AddVariable("derived", derivedVar);
            Input.Value = "base=";
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true); // =base
            Interpreter.Autocomplete(Input, true); // =derived

            Assert.AreEqual("base=derived", Input.Value);
        }

        [Test]
        public void DerivedStaticAssignableToBaseInstanceType() // Python ctor syntax
        {
            var baseVar = new Base();
            var derivedVar = new Derived();
            Interpreter.AddVariable("base", baseVar);
            Interpreter.AddVariable("derived", derivedVar);
            Input.Value = "base=";
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true); // =base
            Interpreter.Autocomplete(Input, true); // =derived
            Interpreter.Autocomplete(Input, true); // =Base
            Interpreter.Autocomplete(Input, true); // =Derived

            Assert.AreEqual("base=Derived", Input.Value);
        }

        [Test]
        public void InstanceStringFieldInput_Assignment_CaretAtEnd_Autocomplete_StringInstanceSelected()
        {
            Input.Value = TargetFieldName + Assignment;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetFieldName + Assignment + StringInstanceNameAndValue, Input.Value);
        }

        [Test]
        public void InstanceStringFieldInput_Assignment_CaretAtEnd_AutocompleteTwice_StringTypeSelected()
        {
            Input.Value = TargetFieldName + Assignment;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);
            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetFieldName + Assignment + "String", Input.Value);
        }

        [Test]
        public void InstanceStringFieldInput_Assignment_Space_CaretAtEnd_Autocomplete_StringInstanceSelected()
        {
            Input.Value = TargetFieldName + Assignment + Space;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetFieldName + Assignment + Space + StringInstanceNameAndValue, Input.Value);
        }

        [Test]
        public void BoolType_Assignment_CaretAtEnd_Autocomplete_PredefinedTypeSelected()
        {
            Input.Value = TargetBooleanType + Assignment;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetBooleanType + Assignment + "False", Input.Value);
        }

        [Test]
        public void NewVariable_Assignment_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            Input.Value = "x" + Assignment;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual("x" + Assignment + FirstInstanceName, Input.Value);
        }
    }    

    public class Base
    {        
    }

    public class Derived : Base
    {        
    }
}
