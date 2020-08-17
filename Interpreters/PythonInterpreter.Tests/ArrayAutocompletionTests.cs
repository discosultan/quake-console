using System.Collections.Generic;
using NUnit.Framework;
using QuakeConsole.Tests.Utilities;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class ArrayAutocompletionTestses : TestsBase
    {
        [Test]
        public void ArrayUnderlyingTypeLoaded()
        {
            var target = new int[0];
            Interpreter.AddVariable("target", target);
            Input.Value = "In";
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual("Int32", Input.Value);
        }

        [Test]
        public void ArrayIndexerAutocomplete()
        {
            var target = new int[0];
            Interpreter.AddVariable("target", target);
            Input.Value = "target[0].";
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual("target[0].CompareTo", Input.Value);
        }

        [Test]
        public void InstanceArrayIndexerAutocomplete()
        {
            var target = new A { B = new int[0] };
            Interpreter.AddVariable("target", target);
            Input.Value = "target.B[0].";
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual("target.B[0].CompareTo", Input.Value);
        }

        [Test]
        public void ListIndexerAutocomplete()
        {
            var target = new List<int>();
            Interpreter.AddVariable("target", target);
            Input.Value = "target[0].";
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.Ignore("Need to find a solution to reflect generic collections (other than Array) element type.");
            Assert.AreEqual("target[0].CompareTo", Input.Value);
        }

        [Test]
        public void InstanceArrayDoubleIndexerAutocomplete()
        {
            var target = new A { C = new[] { new int[0] } };
            Interpreter.AddVariable("target", target);
            Input.Value = "target.C[0][0].";
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.Ignore("More than single indexer access autocompletion not implemented.");
            Assert.AreEqual("target.C[0][0].CompareTo", Input.Value);
        }
    }

    public class A
    {
        public int[][] C;
        public int[] B;
    }
}
