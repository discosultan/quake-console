using NUnit.Framework;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class ArrayAutocompletionTests
    {
        private IConsoleInput _consoleInput;
        private PythonInterpreter _interpreter;

        [SetUp]
        public void Setup()
        {
            _consoleInput = new FakeConsoleInput();
            _interpreter = new PythonInterpreter();
        }        

        [Test]
        public void ArrayUnderlyingTypeLoaded()
        {
            var target = new int[0];
            _interpreter.AddVariable("target", target);            
            _consoleInput.Value = "In";
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual("Int32", _consoleInput.Value);
        }

        [Test]
        public void ArrayIndexerAutocomplete()
        {
            var target = new int[0];
            _interpreter.AddVariable("target", target);
            _consoleInput.Value = "target[0].";
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual("target[0].CompareTo", _consoleInput.Value);
        }

        [Test]
        public void InstanceArrayIndexerAutocomplete()
        {
            var target = new A { B = new int[0] };
            _interpreter.AddVariable("target", target);
            _consoleInput.Value = "target.B[0].";
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual("target.B[0].CompareTo", _consoleInput.Value);
        }
    }

    public class A
    {
        public int[] B;
    }
}
