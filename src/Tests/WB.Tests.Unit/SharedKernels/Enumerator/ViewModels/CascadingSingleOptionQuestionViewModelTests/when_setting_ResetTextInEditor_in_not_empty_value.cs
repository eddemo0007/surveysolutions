using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_setting_ResetTextInEditor_in_not_empty_value: CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();

            var childAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 3);
            var parentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 1);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireIdentity == questionnaireId
                   && _.GetSingleOptionAnswer(questionIdentity) == childAnswer
                   && _.GetSingleOptionAnswer(parentIdentity) == parentOptionAnswer
                   && _.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1) == new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 }
                   && _.GetFilteredOptionsForQuestion(questionIdentity, 1, "a") == Options.Where(x => x.ParentValue == 1).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var optionsRepository = SetupOptionsRepositoryForQuestionnaire(questionIdentity.Id);

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            cascadingModel.FilterText = "a";
        };

        Because of = () =>
            cascadingModel.ResetTextInEditor = "hello";

        It should_not_set_selected_object = () =>
            cascadingModel.SelectedObject.ShouldNotBeNull();

        It should_not_set_filter_text = () =>
            cascadingModel.FilterText.ShouldNotBeNull();

        It should_set_empty_list_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.ShouldNotBeEmpty();

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
    }
}