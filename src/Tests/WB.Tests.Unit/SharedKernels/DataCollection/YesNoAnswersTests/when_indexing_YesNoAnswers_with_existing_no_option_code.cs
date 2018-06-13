using FluentAssertions;

using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.V5.CustomFunctions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.YesNoAnswersTests
{
    internal class when_indexing_YesNoAnswers_with_existing_no_option_code
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answers = Create.Entity.YesNoAnswers(allCodes, new YesNoAnswersOnly(selectedYes, selectedNo));
            BecauseOf();
        }

        public void BecauseOf() =>
            result = answers[10].IsNo();

        [NUnit.Framework.Test] public void should_return_true () =>
            result.Should().BeTrue();

        private static YesNoAnswers answers;
        private static bool result;
        private static readonly decimal[] allCodes = new decimal[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private static readonly decimal[] selectedYes = new decimal[] { 1, 2, 8 };
        private static readonly decimal[] selectedNo = new decimal[] { 10, 6, 3 };
    }
}
