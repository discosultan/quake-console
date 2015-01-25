using NUnit.Framework;

namespace Varus.Paradox.Console.Interpreters.Python.Tests
{
    [TestFixture]
    public class AutocompletionTests
    {
        private IInputBuffer _inputBuffer;
        private PythonInterpreter _interpreter;

        private Kickup _target;
        private const string TargetName = "target";

        [SetUp]
        public void Setup()
        {
            _inputBuffer = new FakeInputBuffer();
            _interpreter = new PythonInterpreter();
            _target = new Kickup();
            _interpreter.AddVariable(TargetName, _target);
        }

        [Test]
        public void NoInput_FirstInstanceSelected()
        {
            _interpreter.Autocomplete(_inputBuffer, true);

            Assert.AreEqual(TargetName, _inputBuffer.Get());
        }
    }
}
