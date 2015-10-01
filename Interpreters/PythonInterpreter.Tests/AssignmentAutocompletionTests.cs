using NUnit.Framework;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class AssignmentAutocompletionTests
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
        public void DerivedInstanceAssignableToBaseInstanceType()
        {
            var baseVar = new Base();
            var derivedVar = new Derived();
            _interpreter.AddVariable("base", baseVar);
            _interpreter.AddVariable("derived", derivedVar);
            _consoleInput.Value = "base=";
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true); // =base
            _interpreter.Autocomplete(_consoleInput, true); // =derived

            Assert.AreEqual("base=derived", _consoleInput.Value);
        }

        [Test]
        public void DerivedStaticAssignableToBaseInstanceType() // Python ctor syntax
        {
            var baseVar = new Base();
            var derivedVar = new Derived();
            _interpreter.AddVariable("base", baseVar);
            _interpreter.AddVariable("derived", derivedVar);
            _consoleInput.Value = "base=";
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true); // =base
            _interpreter.Autocomplete(_consoleInput, true); // =derived
            _interpreter.Autocomplete(_consoleInput, true); // =Base
            _interpreter.Autocomplete(_consoleInput, true); // =Derived

            Assert.AreEqual("base=Derived", _consoleInput.Value);
        }
    }    

    public class Base
    {        
    }

    public class Derived : Base
    {        
    }
}
