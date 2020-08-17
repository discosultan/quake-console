using NUnit.Framework;
using QuakeConsole.Tests.Utilities;

namespace QuakeConsole.Tests
{
    [TestFixture]
    public class AccessorAutocompletionTests : TestsBase
    {
        private const string FirstInstanceName = "instance";
        private const string TargetFirstMemberName = "Cymidine";
        private const string TargetSecondMemberName = "Equals";
        private const string TargetLastMemberName = "ToString";
        private const string TargetRecursiveFieldName = FirstInstanceName + ".Gusher.Nolt";
        private const string TargetRecursiveFieldTypeMemberName = "Apprehender";
        private const string StaticTypeName = "Kickup";
        private const string StaticTypeFirstMemberName = "Eider";
        private const string StaticTypeFirstMemberFirstMemberName = "B";

        public override void Setup()
        {
            base.Setup();
            Interpreter.AddVariable(FirstInstanceName, new Kickup(), int.MaxValue);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            Input.Value = FirstInstanceName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetFirstMemberName, Input.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_AutocompleteTwice_SecondMemberSelected()
        {
            Input.Value = FirstInstanceName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);
            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetSecondMemberName, Input.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_AutocompleteBackward_WrappedToEnd()
        {
            Input.Value = FirstInstanceName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, false);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetLastMemberName, Input.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_CaretAtEnd_AutocompleteOnceBackwardOnceForward_WrappedToBeginning()
        {
            Input.Value = FirstInstanceName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, false);
            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName + Accessor + TargetFirstMemberName, Input.Value);
        }

        [Test]
        public void Space_FirstInstanceInput_Accessor_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            Input.Value = Space + FirstInstanceName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);


            Assert.AreEqual(Space + FirstInstanceName + Accessor + TargetFirstMemberName, Input.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_Space_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            Input.Value = FirstInstanceName + Accessor + Space;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(FirstInstanceName + Accessor + Space + TargetFirstMemberName, Input.Value);
        }

        [Test]
        public void FirstInstanceInput_Accessor_AutocompleteToOverloadedMember_CaretAtEnd_Autocomplete_OverloadSkipped()
        {
            Input.Value = FirstInstanceName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true); // Cymidine
            Interpreter.Autocomplete(Input, true); // Equals
            Interpreter.Autocomplete(Input, true); // GetHashcode
            Interpreter.Autocomplete(Input, true); // GetType
            Interpreter.Autocomplete(Input, true); // Gusher
            Interpreter.Autocomplete(Input, true); // Pauciloquent
            Interpreter.Autocomplete(Input, true); // SetBehen
            Interpreter.Autocomplete(Input, true); // ToString

            Assert.AreEqual(FirstInstanceName + Accessor + "ToString", Input.Value);
        }

        [Test]
        public void StaticTypeInput_Accessor_CaretAtEnd_Autocomplete_FirstMemberSelected()
        {
            Input.Value = StaticTypeName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(StaticTypeName + Accessor + StaticTypeFirstMemberName, Input.Value);
        }

        [Test]
        public void StaticTypeMember_Accessor_CaretAtEnd_Autocomplete_MemberMemberSelected()
        {
            Input.Value = StaticTypeName + Accessor + StaticTypeFirstMemberName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(StaticTypeName + Accessor + StaticTypeFirstMemberName + Accessor + StaticTypeFirstMemberFirstMemberName, Input.Value);
        }

        [Test]
        public void InstanceTypeMembersMember_Accessor_CaretAtEnd_Autocomplete_TypeLoaderWasRecursive()
        {
            Input.Value = TargetRecursiveFieldName + Accessor;
            Input.CaretIndex = Input.Length;

            Interpreter.Autocomplete(Input, true);

            Assert.AreEqual(TargetRecursiveFieldName + Accessor + TargetRecursiveFieldTypeMemberName, Input.Value);
        }
    }
}
