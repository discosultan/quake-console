using NUnit.Framework;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class OverwriteInputAutocompletionTests
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
        public void CaretAtEndOfTargetValue_OverwriteFollowingInputUponAutocomplete()
        {            
            _interpreter.AddVariable("variable", new object());
            _consoleInput.Value = "va random gibberish";
            _consoleInput.CaretIndex = 2;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual("variable", _consoleInput.Value);
        }

        [Test]
        public void CaretAtTargetValue_OverwriteFollowingInputUponAutocomplete()
        {
            _interpreter.AddVariable("variable", new object());
            _consoleInput.Value = "va random gibberish";
            _consoleInput.CaretIndex = 1;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.AreEqual("variable", _consoleInput.Value);
        }
    }        
}
