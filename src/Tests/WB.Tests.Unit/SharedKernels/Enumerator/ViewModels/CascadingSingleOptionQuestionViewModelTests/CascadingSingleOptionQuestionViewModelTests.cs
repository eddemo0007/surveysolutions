using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class CascadingSingleOptionQuestionViewModelTests : CascadingSingleOptionQuestionViewModelTestContext
    {
        [Test]
        public void when_initializing_cascading_view_model_and_child_and_parent_question_are_answered_and_question_2_level_roster()
        {
            CascadingSingleOptionQuestionViewModel cascadingModel;
            Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();
            int answerOnChildQuestion = 3;

            SetUp();
            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ =>
                _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(answerOnChildQuestion));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ =>
                _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            StatefulInterviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);

            StatefulInterviewMock.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1))
                .Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });

            StatefulInterviewMock.Setup(x =>
                    x.GetTopFilteredOptionsForQuestion(questionIdentity, 1, "3", Moq.It.IsAny<int>()))
                .Returns(Options.Where(x => x.Value == 3).ToList());

            var interviewRepository =
                Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == StatefulInterviewMock.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>()
            {
                new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 }
            });

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository, filteredOptionsViewModel: filteredOptionsViewModel);

            //act
            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            //Assert
            StatefulInterviewMock.Verify(x => x.GetSingleOptionQuestion(parentIdentity), Times.Once);
            StatefulInterviewMock.Verify(x => x.GetSingleOptionQuestion(parentIdentity), Times.Once);
            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);
            EventRegistry.Verify(x => x.Subscribe(cascadingModel, Moq.It.IsAny<string>()), Times.Once);

        }

        [Test]
        public void when_handling_SingleOptionQuestionAnswered_for_parent_question()
        {
            List<CategoricalOption> OptionsIfParentAnswerIs2 = Options.Where(x => x.ParentValue == 2).ToList();
            CascadingSingleOptionQuestionViewModel cascadingModel;
            Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();
            int answerOnChildQuestion = 3;

            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ =>
                _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(answerOnChildQuestion));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ =>
                _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));
            var secondParentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ =>
                _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(2));

            StatefulInterviewMock.Setup(x => x.Id).Returns(interviewGuid);
            StatefulInterviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);

            StatefulInterviewMock.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1))
                .Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });

            StatefulInterviewMock.Setup(x =>
                    x.GetTopFilteredOptionsForQuestion(questionIdentity, 2, string.Empty, Moq.It.IsAny<int>()))
                .Returns(Options.Where(x => x.ParentValue == 2).ToList());

            StatefulInterviewMock.Setup(x =>
                    x.GetTopFilteredOptionsForQuestion(questionIdentity, 1, "3", Moq.It.IsAny<int>()))
                .Returns(Options.Where(x => x.ParentValue == 1).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x =>
                x.Get(interviewGuid.FormatGuid()) == StatefulInterviewMock.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(Options.Where(x => x.ParentValue == 2).ToList());

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository, 
                filteredOptionsViewModel: filteredOptionsViewModel);

            cascadingModel.Init(interviewGuid.FormatGuid(), questionIdentity, navigationState);

            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity))
                .Returns(secondParentOptionAnswer);
            
            //aa
            cascadingModel.Handle(Create.Event.SingleOptionQuestionAnswered(parentIdentity.Id, parentIdentity.RosterVector, 2));

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            combo.AutoCompleteSuggestions.Should().NotBeEmpty();
            combo.AutoCompleteSuggestions.Count.Should().Be(3);
            combo.AutoCompleteSuggestions.Select(x => x.Title).Should()
                .BeEquivalentTo(OptionsIfParentAnswerIs2.Select(x => x.Title));
        }

        [Test]
        public void when_initializing_cascading_view_model_and_parent_question_is_answered_and_question_2_level_roster()
        {
            CascadingSingleOptionQuestionViewModel cascadingModel;
            Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();
            List<CategoricalOption> OptionsIfParentAnswerIs1 = Options.Where(x => x.ParentValue == 1).ToList();

            SetUp();
            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            StatefulInterviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(singleOptionAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);

            StatefulInterviewMock.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1))
                .Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });

            StatefulInterviewMock.Setup(x => x.GetTopFilteredOptionsForQuestion(questionIdentity, 1, string.Empty, Moq.It.IsAny<int>()))
                .Returns(Options.Where(x => x.ParentValue == 1).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == StatefulInterviewMock.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(Options.Where(x => x.ParentValue == 1).ToList());

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository, filteredOptionsViewModel: filteredOptionsViewModel);
            //act
            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            //assert
            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);
            EventRegistry.Verify(x => x.Subscribe(cascadingModel, Moq.It.IsAny<string>()), Times.Once);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            combo.FilterText.Should().BeNullOrEmpty();
            combo.AutoCompleteSuggestions.Should().NotBeEmpty();
            combo.AutoCompleteSuggestions.Count.Should().Be(3);
            combo.AutoCompleteSuggestions.Select(x => x.Title).Should().BeEquivalentTo(OptionsIfParentAnswerIs1.Select(x => x.Title));
        }

        [Test]
        public void when_handling_SingleOptionQuestionAnswered_for_parent_question_as_first_answer()
        {

            List<CategoricalOption> OptionsIfParentAnswerIs2 = Options.Where(x => x.ParentValue == 2).ToList();
            CascadingSingleOptionQuestionViewModel cascadingModel;
            Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();

            SetUp();
            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);
            var secondParentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(2));

            StatefulInterviewMock.Setup(x => x.Id).Returns(interviewGuid);
            StatefulInterviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);

            StatefulInterviewMock.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1))
                .Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });

            StatefulInterviewMock.Setup(x => x.GetTopFilteredOptionsForQuestion(questionIdentity, 2, string.Empty, Moq.It.IsAny<int>()))
                .Returns(Options.Where(x => x.ParentValue == 2).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewGuid.FormatGuid()) == StatefulInterviewMock.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(Options.Where(x => x.ParentValue == 2).ToList());

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository, filteredOptionsViewModel : filteredOptionsViewModel);

            cascadingModel.Init(interviewGuid.FormatGuid(), questionIdentity, navigationState);

            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(secondParentOptionAnswer);

            //act
            cascadingModel.Handle(Create.Event.SingleOptionQuestionAnswered(parentIdentity.Id, parentIdentity.RosterVector, 2));
            

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;
            //assert
            combo.AutoCompleteSuggestions.Should().NotBeEmpty();
            combo.AutoCompleteSuggestions.Count.Should().Be(3);
            combo.AutoCompleteSuggestions.Select(x => x.Title).Should().BeEquivalentTo(OptionsIfParentAnswerIs2.Select(x => x.Title));

        }

        [Test]
        public void when_initializing_cascading_view_model_and_there_is_no_answers_in_interview_and_question_2_level_roster()
        {
            CascadingSingleOptionQuestionViewModel cascadingModel;

            SetUp();

            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer
                   && _.GetSingleOptionQuestion(parentIdentity) == parentOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>());

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository, 
                filteredOptionsViewModel: filteredOptionsViewModel);

            //act
            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);
            EventRegistry.Verify(x => x.Subscribe(cascadingModel, Moq.It.IsAny<string>()), Times.Once);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;
            combo.FilterText.Should().BeNullOrEmpty();
            combo.AutoCompleteSuggestions.Should().BeEmpty();
        }

        [Test]
        public void when_setting_FilterText_and_there_are_match_options()
        {
            CascadingSingleOptionQuestionViewModel cascadingModel;
            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            interview.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => Options.Where(x => x.ParentValue == value && x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>()
            {
                new CategoricalOption() { Title = "title abc 1", Value = 3, ParentValue = 1 }
            });

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository, filteredOptionsViewModel: filteredOptionsViewModel);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            //act
            combo.FilterCommand.Execute("a");

            combo.FilterText.Should().Be("a");
            combo.AutoCompleteSuggestions.Should().NotBeEmpty();
            combo.AutoCompleteSuggestions.Should().HaveCount(1);
            combo.AutoCompleteSuggestions.Select(x => x.Title).Should().HaveElementAt(0, "title abc 1");
        }

        [Test]
        public void when_setting_FilterText_to_empty_and_was_not_empty()
        {
            CascadingSingleOptionQuestionViewModel cascadingModel;
            List<CategoricalOption> OptionsIfParentAnswerIs1 = Options.Where(x => x.ParentValue == 1).ToList();

            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            interview.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => Options.Where(x => x.ParentValue == value && x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(Options.Where(x => x.ParentValue == 1 && x.Title.IndexOf("", StringComparison.OrdinalIgnoreCase) >= 0).ToList());

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository, filteredOptionsViewModel: filteredOptionsViewModel);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            combo.FilterCommand.Execute("a");
            combo.FilterCommand.Execute(string.Empty);

            combo.FilterText.Should().BeEmpty();
            combo.AutoCompleteSuggestions.Count.Should().Be(3);
            combo.AutoCompleteSuggestions.Select(x => x.Title).Should().BeEquivalentTo(OptionsIfParentAnswerIs1.Select(x => x.Title));

        }

        
        [Test]
        public void Should_show_first_items_in_list()
        {
            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            interview.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => Options.Where(x => x.ParentValue == value && x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(
                Options.Where(x => x.ParentValue == 1 && x.Title.IndexOf("", StringComparison.OrdinalIgnoreCase) >= 0).ToList()
                );

            var cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository, filteredOptionsViewModel: filteredOptionsViewModel);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;
            // Act
            combo.FilterCommand.Execute(string.Empty);

            // Assert
            combo.FilterText.Should().BeEmpty();
            combo.AutoCompleteSuggestions.Should().NotBeEmpty();
            combo.AutoCompleteSuggestions.Should().HaveCount(3);

            List<CategoricalOption> optionsIfParentAnswerIs1 = Options.Where(x => x.ParentValue == 1).ToList();
            combo.AutoCompleteSuggestions.Select(x => x.Title).Should().Contain(optionsIfParentAnswerIs1.Select(x => x.Title));
        }


        [Test]
        public void when_invoking_ValueChangeCommand_and_there_is_match_in_options_list()
        {

            CascadingSingleOptionQuestionViewModel cascadingModel;
            int answerOnChildQuestion = 2;
            Mock<ValidityViewModel> ValidityModelMock = new Mock<ValidityViewModel>();

            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(answerOnChildQuestion));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            interview.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 2, 1)).Returns(new CategoricalOption() { Title = "2", Value = 2, ParentValue = 1 });
            interview.Setup(x => x.GetOptionForQuestionWithFilter(questionIdentity, Moq.It.IsAny<string>(), 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => Options.Where(x => x.ParentValue == value && x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList());


            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            QuestionStateMock.Setup(x => x.Validity).Returns(ValidityModelMock.Object);

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(
                Options.Where(x => x.ParentValue == 1 && x.Title.IndexOf("", StringComparison.OrdinalIgnoreCase) >= 0).ToList()
            );

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            combo.FilterCommand.Execute("o");

            combo.SaveAnswerBySelectedOptionCommand.Execute(Create.Entity.OptionWithSearchTerm(3));

            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerSingleOptionQuestionCommand>()), Times.Exactly(1));

        }

        [Test]
        public void when_selecting_object_in_cascading_view_model_set_in_null()
        {
            CascadingSingleOptionQuestionViewModel cascadingModel;
            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            interview.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => Options.Where(x => x.ParentValue == value && x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList());


            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(
                Options.Where(x => x.ParentValue == 1 && x.Title.IndexOf("", StringComparison.OrdinalIgnoreCase) >= 0).ToList()
            );

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            combo.FilterCommand.Execute("oooo");
            combo.ShowErrorIfNoAnswerCommand.Execute(null);

            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommandAsync(Moq.It.IsAny<AnswerSingleOptionQuestionCommand>()), Times.Never);
            QuestionStateMock.Verify(x => x.Validity.MarkAnswerAsNotSavedWithMessage(Moq.It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void should_set_suggestion_list_to_empty()
        {
            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(3));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            interview.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            interview.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => Options.Where(x => x.ParentValue == value && x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>()
            {
                new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 }
            });

            var cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                filteredOptionsViewModel: filteredOptionsViewModel);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            // Act
            combo.FilterCommand.Execute("ebw");

            // Assert
            combo.FilterText.Should().Be("ebw");
            combo.AutoCompleteSuggestions.Should().BeEmpty();
        }

        [Test]
        public void when_setting_FilterText_and_text_contains_special_regex_characters()
        {
            CascadingSingleOptionQuestionViewModel cascadingModel;
            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            interview.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            interview.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            interview.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => new List<CategoricalOption>(new[]
                {
                            new CategoricalOption
                            {
                                Title = "G.C.E. (O Level) / C.X.C/ (+ General), 1 or 2 Subjects (+ Basic) and Technical Training",
                                Value = 3,
                                ParentValue = 1
                            }
                }));

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>(new[]
            {
                new CategoricalOption
                {
                    Title = "G.C.E. (O Level) / C.X.C/ (+ General), 1 or 2 Subjects (+ Basic) and Technical Training",
                    Value = 3,
                    ParentValue = 1
                }
            }));


            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository, filteredOptionsViewModel: filteredOptionsViewModel);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            Assert.DoesNotThrow( () => combo.FilterCommand.Execute(@"(+"));
        }

        [Test]
        public void when_handling_AnswersRemoved_for_cascading_question()
        {
            CascadingSingleOptionQuestionViewModel cascadingModel;
            Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();
            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));

            StatefulInterviewMock.Setup(x => x.Id).Returns(interviewGuid);
            StatefulInterviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);
            StatefulInterviewMock.Setup(x => x.GetOptionForQuestionWithFilter(questionIdentity, "3", 1)).Returns(new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 });
            StatefulInterviewMock.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => Options.Where(x => x.ParentValue == value && (filter == null || x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewGuid.FormatGuid()) == StatefulInterviewMock.Object);

            var optionsRepository = SetupOptionsRepositoryForQuestionnaire(questionIdentity.Id);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion(optionsRepository);

            var filteredOptionsViewModel = Abc.SetUp.FilteredOptionsViewModel(new List<CategoricalOption>()
            {
                new CategoricalOption() { Title = "3", Value = 3, ParentValue = 1 }
            });

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                filteredOptionsViewModel:filteredOptionsViewModel);

            cascadingModel.Init(interviewGuid.FormatGuid(), questionIdentity, navigationState);

            var combo = cascadingModel.Children[1] as CategoricalComboboxAutocompleteViewModel;

            combo.SaveAnswerBySelectedOptionCommand.Execute(Create.Entity.OptionWithSearchTerm(3));

            cascadingModel.Handle(Create.Event.AnswersRemoved(questionIdentity));

            combo.FilterText.Should().BeNullOrEmpty();
        }
    }
}