using NUnit.Framework;
using QuakeConsole.Tests.Utilities;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class OverwriteInputAutocompletionTests : TestsBase
    {
        [Test]
        public void CaretAtEndOfTargetValue_OverwriteFollowingInputUponAutocomplete()
        {            
            Interpreter.AddVariable("variable", new object());
            Input.Value = "va gibberish";
            Input.CaretIndex = 2;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual("variable", Input.Value);
        }

        [Test]
        public void CaretAtTargetValue_OverwriteFollowingInputUponAutocomplete()
        {
            Interpreter.AddVariable("variable", new object());
            Input.Value = "va gibberish";
            Input.CaretIndex = 1;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual("variable", Input.Value);
        }

        [Test]
        public void CaretAtBeginningOfInput_OverwriteInput()
        {
            Interpreter.AddVariable("variable", new object());
            Input.Value = "gibberish";
            Input.CaretIndex = 0;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual("variable", Input.Value);
        }
    }        
}
