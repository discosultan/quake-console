using NUnit.Framework;
using QuakeConsole.Input;

namespace QuakeConsole.Tests.Utilities
{    
    public class TestsBase
    {
        public const string Accessor = ".";
        protected const string Assignment = "=";
        protected const string Space = " ";

        [SetUp]
        public virtual void Setup()
        {
            Input = new FakeConsoleInput();
            Interpreter = new PythonInterpreter();
        }

        protected IConsoleInput Input { get; private set; }
        protected PythonInterpreter Interpreter { get; private set; }
    }
}
