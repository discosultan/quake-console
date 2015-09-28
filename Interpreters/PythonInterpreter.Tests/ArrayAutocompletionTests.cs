using System.Collections.Generic;
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

        [Test]
        public void ListIndexerAutocomplete()
        {
            var target = new List<int>();
            _interpreter.AddVariable("target", target);
            _consoleInput.Value = "target[0].";
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.Ignore("Need to find a solution to reflect generic collections (other than Array) element type.");
            Assert.AreEqual("target[0].CompareTo", _consoleInput.Value);
        }

        [Test]
        public void InstanceArrayDoubleIndexerAutocomplete()
        {
            var target = new A { C = new[] { new int[0] } };
            _interpreter.AddVariable("target", target);
            _consoleInput.Value = "target.C[0][0].";
            _consoleInput.CaretIndex = _consoleInput.Length;

            _interpreter.Autocomplete(_consoleInput, true);

            Assert.Ignore("More than single indexer access autocompletion not implemented.");
            Assert.AreEqual("target.C[0][0].CompareTo", _consoleInput.Value);
        }
    }    

    public class A
    {
        public int[][] C;
        public int[] B;
    }
}
