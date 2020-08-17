using NUnit.Framework;
using QuakeConsole.Tests.Utilities;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class GeneralAutocompletionTests : TestsBase
    {
        private const string InstancePrefix = "instance";
        private const string FirstInstanceName = "instance_a";
        private const string SecondInstanceName = "instance_b";
        private const string LastStaticName = "YieldAwaitable";
        private const string StaticClassName = "Podobranchia";
        private const string StaticClassPrefix = "Podo";

        private Kickup _target;

        public override void Setup()
        {
            base.Setup();
            _target = new Kickup();
            Interpreter.AddVariable(FirstInstanceName, _target, int.MaxValue);
            Interpreter.AddVariable(SecondInstanceName, _target, int.MaxValue);
            Interpreter.AddType(typeof (Podobranchia), int.MaxValue);
        }

        [Test]
        public void NoInput_AutocompleteOnce_FirstInstanceSelected()
        {
            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName, Input.Value);
        }

        [Test]
        public void NoInput_AutocompleteTwice_SecondInstanceSelected()
        {
            Interpreter.Autocomplete(Input, true);
            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(SecondInstanceName, Input.Value);
        }

        [Test]
        public void NoInput_AutocompleteTwiceForwardOnceBackward_FirstInstanceSelected()
        {
            Interpreter.Autocomplete(Input, true);
            Interpreter.Autocomplete(Input, true);
            Interpreter.Autocomplete(Input, false);

            Assert.AreEqual(FirstInstanceName, Input.Value);
        }

        [Test]
        public void NoInput_AutocomplateBackward_WrappedToEnd()
        {
            Interpreter.Autocomplete(Input, false);

            Assert.AreEqual(LastStaticName, Input.Value);
        }

        [Test]
        public void NoInput_AutocompleteBackwardOnceForwardOnce_WrappedToBeginning()
        {
            Interpreter.Autocomplete(Input, false);
            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName, Input.Value);
        }

        [Test]
        public void FirstInstanceInput_CaretAtZero_Autocomplete_DidNothing()
        {
            Input.Value = FirstInstanceName;
            Input.CaretIndex = 0;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName, Input.Value);
        }

        [Test]
        public void InstancePrefix_CaretAtZero_Autocomplete_FirstInstanceSelected()
        {
            Input.Value = InstancePrefix;
            Input.CaretIndex = 0;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName, Input.Value);
        }

        [Test]
        public void InstancePrefix_CaretAtZero_AutocompleteTwice_SecondInstanceSelected()
        {
            Input.Value = InstancePrefix;
            Input.CaretIndex = 0;

            Interpreter.Autocomplete(Input, true);
            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(SecondInstanceName, Input.Value);
        }

        [Test]
        public void InstancePrefix_CaretAtEnd_Autocomplete_FirstInstanceSelected()
        {
            Input.Value = InstancePrefix;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName, Input.Value);
        }

        [Test]
        public void FirstInstanceInput_CaretAtEnd_Autocomplete_DidNothing()
        {
            Input.Value = FirstInstanceName;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName, Input.Value);
        }

        [Test]
        public void StaticClassPrefixInput_CaretAtEnd_Autocomplete_StaticTypeSelected()
        {
            Input.Value = StaticClassPrefix;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(StaticClassName, Input.Value);
        }
    }
}
