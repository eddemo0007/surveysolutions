﻿using System;
using System.Linq;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class NewAddQuestionTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitle_is_not_empty_Then_event_contains_the_same_answer_title(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                var questionnaireKey = Guid.NewGuid();
                var groupKey = Guid.NewGuid();
                var notEmptyAnswerTitle1 = "title";
                var notEmptyAnswerTitle2 = "title1";
                Guid responsibleId = Guid.NewGuid();
                // arrange
                Questionnaire questionnsire = CreateQuestionnaireWithOneGroup(questionnaireId: questionnaireKey, groupId: groupKey, responsibleId: responsibleId);
                Option[] options = new Option[2] { new Option(Guid.NewGuid(), "1", notEmptyAnswerTitle1), new Option(Guid.NewGuid(), "2", notEmptyAnswerTitle2) };

                // act
                questionnsire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, options, Order.AsIs, null, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);
                // assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual(notEmptyAnswerTitle1, risedEvent.Answers[0].AnswerText);
                Assert.AreEqual(notEmptyAnswerTitle2, risedEvent.Answers[1].AnswerText);
            }
        }


        [Test]
        public void NewAddQuestion_When_question_is_featured_but_inside_propagated_group_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid autoGroupId = Guid.NewGuid();
            bool isFeatured = true;
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(groupId: autoGroupId, responsibleId: responsibleId);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(Guid.NewGuid(), autoGroupId, "What is your last name?", QuestionType.Text, "name", false,
                                                            isFeatured,
                                                            false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AsIs, 0, new Guid[0],
                                                            responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionIsFeaturedButNotInsideNonPropagateGroup));
        }

        [Test]
        public void NewAddQuestion_When_question_is_featured_and_inside_non_propagated_group_Then_raised_NewQuestionAdded_event_contains_the_same_featured_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid groupId = Guid.NewGuid();
                bool isFeatured = true;
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneNonPropagatedGroup(groupId: groupId, responsibleId: responsibleId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?",
                                             QuestionType.Text, "name",
                                             false, isFeatured, false, QuestionScope.Interviewer, "", "", "", "",
                                             new Option[0], Order.AsIs, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Featured, Is.EqualTo(isFeatured));
            }
        }

        [Test]
        public void NewAddQuestion_When_Title_is_not_empty_Then_NewQuestionAdded_event_contains_the_same_title_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var questionnaireKey = Guid.NewGuid();
                var groupKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: questionnaireKey, groupId: groupKey, responsibleId: responsibleId);

                var notEmptyTitle = "any not empty title";

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, notEmptyTitle, QuestionType.Text, "test", false,
                                             false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                             string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

                // Assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.That(risedEvent.QuestionText, Is.EqualTo(notEmptyTitle));
            }
        }

        [Test]
        public void NewAddQuestion_When_Title_is_empty_Then_DomainException_should_be_thrown()
        {
            // arrange
            var questionnaireKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: questionnaireKey, groupId: groupKey, responsibleId: responsibleId);

            // act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "", QuestionType.Text, "test", false, false,
                                                            false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                            string.Empty, new Option[0], Order.AZ, null, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: false);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionTitleRequired));
        }

        [Ignore("Validation about options count is temporary turned off. Should be turned on with new clone questionnaire feature implementation")]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_there_is_only_one_option_in_categorical_question_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            var questionnaireKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            // arrange
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: questionnaireKey, groupId: groupKey, responsibleId: responsibleId);

            Option[] oneOption = new Option[1] { new Option(Guid.NewGuid(), "1", "title") };
            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, oneOption, Order.AsIs, null, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: false);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TooFewOptionsInCategoryQuestion));
        }

        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_there_are_two_options_in_categorical_question_Then_raised_NewQuestionAdded_event_contains_the_same_options_count(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                var questionnaireKey = Guid.NewGuid();
                var groupKey = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                // arrange
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: questionnaireKey, groupId: groupKey, responsibleId: responsibleId);

                const int answerOptionsCount = 2;

                Option[] options = new Option[answerOptionsCount]{ new Option(Guid.NewGuid(), "1", "title"), new Option(Guid.NewGuid(), "2", "title1") };
                // act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                                  false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                                  string.Empty,
                                                  string.Empty, options, Order.AsIs, null, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

                // assert
                var raisedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.That(raisedEvent.Answers.Length, Is.EqualTo(answerOptionsCount));
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitle_is_absent_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            var questionnaireKey = Guid.NewGuid();
            var groupKey = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            // arrange
            Questionnaire questionnsire = CreateQuestionnaireWithOneGroup(questionnaireId: questionnaireKey, groupId: groupKey, responsibleId: responsibleId);

            Option[] options = new Option[2] { new Option(Guid.NewGuid(), "1", string.Empty), new Option(Guid.NewGuid(), "2", string.Empty) };
            // act
            TestDelegate act =
                () =>
                questionnsire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, options, Order.AsIs, null, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorTextRequired));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitleIsNotUnique_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            var groupKey = Guid.NewGuid();
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupKey, responsibleId: responsibleId);

            Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "title"), new Option(Guid.NewGuid(), "2", "title") };

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, options, Order.AsIs, null, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);
            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorTextNotUnique));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_AnswerTitleIsUnique_Then_event_contains_the_same_answer_titles(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                var groupKey = Guid.NewGuid();
                // arrange
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupKey, responsibleId: responsibleId);

                Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "title1"), new Option(Guid.NewGuid(), "2", "title2") };
                // act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupKey, "test", questionType, "alias", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, options, Order.AsIs, null, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);
                // assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                for (int i = 0; i < options.Length; i++)
                {
                    Assert.IsTrue(options[i].Title == risedEvent.Answers[i].AnswerText);
                }
            }
        }

        [Test]
        public void NewAddQuestion_When_question_is_head_of_propagated_group_but_inside_non_propagated_group_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid groupId = Guid.NewGuid();
            bool isHeadOfPropagatedGroup = true;
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                                            "name", false, false,
                                                            isHeadOfPropagatedGroup,
                                                            QuestionScope.Interviewer, "", "", "", "",
                                                            new Option[0], Order.AsIs, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionIsHeadOfGroupButNotInsidePropagateGroup));
        }

        [Test]
        public void NewAddQuestion_When_question_is_head_of_propagated_group_and_inside_propagated_group_Then_raised_NewQuestionAdded_event_contains_the_same_header_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid autoGroupId = Guid.NewGuid();
                bool isHeadOfPropagatedGroup = true;
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(groupId: autoGroupId, responsibleId: responsibleId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), autoGroupId, "What is your last name?",
                                             QuestionType.Text, "name",
                                             false, false, isHeadOfPropagatedGroup, QuestionScope.Interviewer, "", "", "", "",
                                             new Option[0], Order.AsIs, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Capital, Is.EqualTo(isHeadOfPropagatedGroup));
            }
        }

        [Test]
        public void NewAddQuestion_When_capital_parameter_is_true_Then_in_NewQuestionAdded_event_capital_property_should_be_set_in_true_too()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                Guid groupId = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(groupId: groupId, responsibleId: responsibleId);
                bool capital = true;

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text, "name",
                    false, false, capital, QuestionScope.Interviewer, "", "", "", "", new Option[] { }, Order.AZ, null, new Guid[] { }, responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

                // Assert
                var risedEvent = GetSingleEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual(capital, risedEvent.Capital);
            }
        }

        [TestCase("ma_name38")]
        [TestCase("__")]
        [TestCase("_123456789012345678901234567890_")]
        public void NewAddQuestion_When_variable_name_is_valid_Then_rised_NewQuestionAdded_event_contains_the_same_stata_caption(string validVariableName)
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                             validVariableName,
                                             false, false, false, QuestionScope.Interviewer,
                                             "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);


                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).StataExportCaption, Is.EqualTo(validVariableName));
            }
        }

        [Test]
        public void NewAddQuestion_When_variable_name_has_trailing_spaces_and_is_valid_Then_rised_NewQuestionAdded_event_should_contains_trimed_stata_caption()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: Guid.NewGuid(), groupId: groupId, responsibleId: responsibleId);
                string longVariableName = " my_name38  ";

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                             longVariableName,
                                             false, false, false, QuestionScope.Interviewer,
                                             "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).StataExportCaption, Is.EqualTo(longVariableName.Trim()));
            }
        }

        [TestCase("this")]
        public void NewAddQuestion_When_variable_name_mathes_with_keyword_Then_DomainException_should_be_thrown(string variableNameMatchedWithKeyword)
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  variableNameMatchedWithKeyword,
                                                                  false, false, false, QuestionScope.Interviewer,
                                                                  "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameShouldNotMatchWithKeywords));
        }

        [TestCase("This")]
        [TestCase("tHis")]
        public void NewAddQuestion_When_variable_name_mathes_with_keyword_in_lowercase_only_Then_DomainException_should_be_thrown(string variableNameMatchedWithKeyword)
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  variableNameMatchedWithKeyword,
                                                                  false, false, false, QuestionScope.Interviewer,
                                                                  "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameShouldNotMatchWithKeywords));
        }

        [Test]
        public void NewAddQuestion_When_variable_name_has_33_chars_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            string longVariableName = "".PadRight(33, 'A');

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  longVariableName,
                                                                  false, false, false, QuestionScope.Interviewer,
                                                                  "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameMaxLength));
        }

        [Test]
        public void NewAddQuestion_When_variable_name_starts_with_digit_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            string stataExportCaptionWithFirstDigit = "1aaaa";

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  stataExportCaptionWithFirstDigit,
                                                                  false, false, false, QuestionScope.Interviewer,
                                                                  "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameStartWithDigit));
        }

        [Test]
        public void NewAddQuestion_When_variable_name_is_empty_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            string emptyVariableName = string.Empty;

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  emptyVariableName,
                                                                  false, false, false, QuestionScope.Interviewer,
                                                                  "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0], responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);


            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameRequired));
        }

        [Test]
        public void NewAddQuestion_When_variable_name_contains_any_non_underscore_letter_or_digit_character_Then_DomainException_should_be_thrown()
        {
            // Arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            string nonValidVariableNameWithBannedSymbols = "aaa:_&b";

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), Guid.NewGuid(), "What is your last name?", QuestionType.Text,
                                                                  nonValidVariableNameWithBannedSymbols,
                                                                  false, false, false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0],
                                                                  responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableNameSpecialCharacters));
        }

        [Test]
        public void NewAddQuestion_When_questionnaire_has_another_question_with_same_variable_name_Then_DomainException_should_be_thrown()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var duplicateVariableName = "text";
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroupAndQuestionInIt(questionId: Guid.NewGuid(), groupId: groupId, responsibleId: responsibleId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?", QuestionType.Text,
                                                                  duplicateVariableName,
                                                                  false, false, false, QuestionScope.Interviewer, "", "", "", "", new Option[0], Order.AZ, 0, new Guid[0],
                                                                  responsibleId, linkedToQuestionId: null, isInteger: null);


            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VarialbeNameNotUnique));
        }

        [Ignore("Validation about options count is temporary turned off. Should be turned on with new clone questionnaire feature implementation")]
        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_QuestionType_is_option_type_and_answer_options_list_is_empty_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // Arrange
            var emptyAnswersList = new Option[] { };
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "What is your last name?",
                                                            questionType, "name", false, false, false, QuestionScope.Interviewer, "", "", "", "",
                                                            emptyAnswersList, Order.AZ, null, new Guid[] { }, responsibleId: responsibleId, linkedToQuestionId: null, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TooFewOptionsInCategoryQuestion));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
#warning Roma: when part is incorrect should be something like when answer option value contains not number
        public void NewAddQuestion_When_answer_option_is_not_numeric_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            // Act
            var notNumericAnswerValue1 = "some value";
            var notNumericAnswerValue2 = "some value";

            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(
                                   questionId: Guid.NewGuid(),
                                   groupId: Guid.NewGuid(),
                                   title: "What is your last name?",
                                   type: questionType,
                                   alias: "name",
                                   isMandatory: false,
                                   isFeatured: false,
                                   isHeaderOfPropagatableGroup: false,
                                   scope: QuestionScope.Interviewer,
                                   condition: string.Empty,
                                   validationExpression: string.Empty,
                                   validationMessage: string.Empty,
                                   instructions: string.Empty,
                                   optionsOrder: Order.AsIs,
                                   maxValue: 0,
                                   triggedGroupIds: new Guid[0],
                                   options: new Option[2]
                                       {
                                           new Option(id: Guid.NewGuid(), value: notNumericAnswerValue1, title: "text"),
                                           new Option(id: Guid.NewGuid(), value: notNumericAnswerValue2, title: "text1")
                                       },
                                   responsibleId: responsibleId,
                                   linkedToQuestionId: null, isInteger: null);

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorValueSpecialCharacters));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_value_contains_only_numbers_Then_raised_NewQuestionAdded_event_contains_question_answer_the_same_answer_values(
            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                string answerValue1 = "10";
                string answerValue2 = "100";
                Guid autoGroupId = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(groupId: autoGroupId, responsibleId: responsibleId);

                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: autoGroupId,
                    title: "What is your last name?",
                    type: questionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: 0,
                    triggedGroupIds: new Guid[0],
                    options: new Option[2]
                        {
                            new Option(id: Guid.NewGuid(), value: answerValue1, title: "text1"),
                            new Option(id: Guid.NewGuid(), value: answerValue2, title: "text2")
                        },
                    responsibleId: responsibleId,
                    linkedToQuestionId: null, isInteger: null);


                // assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Answers[0].AnswerValue, Is.EqualTo(answerValue1));
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Answers[1].AnswerValue, Is.EqualTo(answerValue2));
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        [TestCase(QuestionType.Numeric)]
        [TestCase(QuestionType.Text)]
        [TestCase(QuestionType.DateTime)]
        [TestCase(QuestionType.AutoPropagate)]
        [TestCase(QuestionType.GpsCoordinates)]
        public void NewAddQuestion_When_question_type_is_allowed_Then_raised_NewQuestionAdded_event_with_same_question_type(
            QuestionType allowedQuestionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "What is your last name?",
                    type: allowedQuestionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: null,
                    triggedGroupIds: new Guid[] {},
                    options: AreOptionsRequiredByQuestionType(allowedQuestionType) ? CreateTwoOptions() : null,
                    responsibleId: responsibleId,
                    linkedToQuestionId: null, isInteger: IsPrecisionRequiredByQuestionType(allowedQuestionType) ? (bool?) true : null);

                // assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).QuestionType, Is.EqualTo(allowedQuestionType));
            }
        }

        [Test]
        [TestCase(QuestionType.DropDownList)]
        //[TestCase(QuestionType.GpsCoordinates)]
        [TestCase(QuestionType.YesNo)]
        public void NewAddQuestion_When_question_type_is_not_allowed_Then_DomainException_with_type_NotAllowedQuestionType_should_be_thrown(
            QuestionType notAllowedQuestionType)
        {
            // arrange
            Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

            // act
            TestDelegate act = () => questionnaire.NewAddQuestion(
                questionId: Guid.NewGuid(),
                groupId: groupId,
                title: "What is your last name?",
                type: notAllowedQuestionType,
                alias: "name",
                isMandatory: false,
                isFeatured: false,
                isHeaderOfPropagatableGroup: false,
                scope: QuestionScope.Interviewer,
                condition: string.Empty,
                validationExpression: string.Empty,
                validationMessage: string.Empty,
                instructions: string.Empty,
                optionsOrder: Order.AsIs,
                maxValue: null,
                triggedGroupIds: null,
                options: null,
                responsibleId: responsibleId,
                linkedToQuestionId: null, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotAllowedQuestionType));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_value_is_required_Then_DomainException_should_be_thrown(QuestionType questionTypeWithOptionsExpected)
        {
            // arrange
            Guid groupId = Guid.NewGuid();
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

            // Act
            TestDelegate act = () =>
                               questionnaire.NewAddQuestion(
                                   questionId: Guid.NewGuid(),
                                   groupId: groupId,
                                   title: "What is your last name?",
                                   type: questionTypeWithOptionsExpected,
                                   alias: "name",
                                   isMandatory: false,
                                   isFeatured: false,
                                   isHeaderOfPropagatableGroup: false,
                                   scope: QuestionScope.Interviewer,
                                   condition: string.Empty,
                                   validationExpression: string.Empty,
                                   validationMessage: string.Empty,
                                   instructions: string.Empty,
                                   optionsOrder: Order.AZ,
                                   maxValue: 0,
                                   triggedGroupIds: new Guid[] { },
                                   options: new Option[2]
                                       {
                                           new Option(id: Guid.NewGuid(), value: null, title: "text"),
                                           new Option(id: Guid.NewGuid(), value: "1", title: "text2")
                                       },
                                   responsibleId: responsibleId,
                                   linkedToQuestionId: null, isInteger: null);

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorValueRequired));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_value_is_not_null_or_empty_Then_raised_NewQuestionAdded_event_contains_not_null_and_not_empty_question_answer(
            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                string answerValue1 = "10";
                string answerValue2 = "100";
                Guid autoGroupId = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(groupId: autoGroupId, responsibleId: responsibleId);

                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: autoGroupId,
                    title: "What is your last name?",
                    type: questionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: 0,
                    triggedGroupIds: new Guid[0],
                    options: new Option[2]
                        {
                            new Option(id: Guid.NewGuid(), value: answerValue1, title: "text1") , 
                            new Option(id: Guid.NewGuid(), value: answerValue2, title: "text2")
                        },
                    responsibleId: responsibleId,
                    linkedToQuestionId: null, isInteger: null);


                // assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Answers[0].AnswerValue, !Is.Empty);
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Answers[1].AnswerValue, !Is.Empty);
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_values_not_unique_in_options_scope_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            var questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            // Act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: Guid.NewGuid(),
                    title: "What is your last name?",
                    type: questionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: 0,
                    triggedGroupIds: new Guid[0],
                    options:
                        new Option[2]
                            {
                                new Option(id: Guid.NewGuid(), value: "1", title: "text 1"),
                                new Option(id: Guid.NewGuid(), value: "1", title: "text 2")
                            },
                    responsibleId: responsibleId,
                    linkedToQuestionId: null, isInteger: null);

            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.SelectorValueNotUnique));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_answer_option_values_unique_in_options_scope_Then_raised_NewQuestionAdded_event_contains_only_unique_values_in_answer_values_scope(
            QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid autoGroupId = Guid.NewGuid();
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneAutoPropagatedGroup(groupId: autoGroupId, responsibleId: responsibleId);

                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: autoGroupId,
                    title: "What is your last name?",
                    type: questionType,
                    alias: "name",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    optionsOrder: Order.AsIs,
                    maxValue: 0,
                    triggedGroupIds: new Guid[0],
                    options:
                        new Option[2]
                            {
                                new Option(id: Guid.NewGuid(), title: "text 1", value: "1"),
                                new Option(id: Guid.NewGuid(), title: "text 2", value: "2")
                            },
                    responsibleId: responsibleId,
                    linkedToQuestionId: null, isInteger: null);


                // assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Answers.Select(x => x.AnswerValue).Distinct().Count(),
                            Is.EqualTo(2));
            }
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_is_null_Then_rised_NewQuestionAdded_event_should_contains_null_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                Guid[] emptyTriggedGroupIds = null;
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, emptyTriggedGroupIds, responsibleId: responsibleId,
                                             linkedToQuestionId: null, isInteger: true);


                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Triggers, Is.Null);
            }
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_is_empty_Then_rised_NewQuestionAdded_event_should_contains_empty_list_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var groupId = Guid.NewGuid();
                var autoPropagate = QuestionType.AutoPropagate;
                var emptyTriggedGroupIds = new Guid[0];
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null, emptyTriggedGroupIds, responsibleId: responsibleId,
                                             linkedToQuestionId: null, isInteger: true);


                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Triggers, Is.Empty);
            }
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_contains_absent_group_id_Then_DomainException_of_type_TriggerLinksToNotExistingGroup_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;

            var groupId = Guid.NewGuid();
            var absentGroupId = Guid.NewGuid();
            var triggedGroupIdsWithAbsentGroupId = new[] { absentGroupId };

            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                                                  false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                                  string.Empty, new Option[0], Order.AZ, null, triggedGroupIdsWithAbsentGroupId, responsibleId: responsibleId,
                                                                  linkedToQuestionId: null, isInteger: true);


            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotExistingGroup));
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_precision_is_not_integer_Then_DomainException_of_type_AutoPropagateQuestionMarkedAsNotInteger_should_be_thrown()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var autoPropagate = QuestionType.AutoPropagate;
            var emptyTriggedGroupIds = new Guid[0];
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithOneGroup(groupId: groupId, responsibleId: responsibleId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                string.Empty, new Option[0], Order.AZ, null, emptyTriggedGroupIds, responsibleId: responsibleId,
                linkedToQuestionId: null, isInteger: false);


            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.AutoPropagateQuestionMarkedAsNotInteger));
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_contains_non_propagate_group_id_Then_DomainException_of_type_TriggerLinksToNotPropagatedGroup_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;
            var nonPropagateGroupId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var triggedGroupIdsWithNonPropagateGroupId = new[] { nonPropagateGroupId };
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaireWithTwoGroups(firstGroup: nonPropagateGroupId, secondGroup: groupId, responsibleId: responsibleId);

            // Act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false, false,
                                                                  false, QuestionScope.Interviewer, string.Empty, string.Empty, string.Empty,
                                                                  string.Empty, new Option[0], Order.AZ, null, triggedGroupIdsWithNonPropagateGroupId, responsibleId: responsibleId,
                                                                  linkedToQuestionId: null, isInteger: true);


            // Assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.TriggerLinksToNotPropagatedGroup));
        }

        [Test]
        public void NewAddQuestion_When_question_is_AutoPropagate_and_list_of_triggers_contains_propagate_group_id_Then_rised_NewQuestionAdded_event_should_contains_that_group_id_in_triggers_field()
        {
            using (var eventContext = new EventContext())
            {
                // Arrange
                var autoPropagate = QuestionType.AutoPropagate;
                var autoPropagateGroupId = Guid.NewGuid();
                var groupId = Guid.NewGuid();
                var triggedGroupIdsWithAutoPropagateGroupId = new[] { autoPropagateGroupId };
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaireWithAutoGroupAndRegularGroup(autoGroupPublicKey: autoPropagateGroupId, secondGroup: groupId, responsibleId: responsibleId);

                // Act
                questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false,
                                             false,
                                             false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                             string.Empty,
                                             string.Empty, new Option[0], Order.AZ, null,
                                             triggedGroupIdsWithAutoPropagateGroupId, responsibleId: responsibleId,
                                             linkedToQuestionId: null, isInteger: true);

                // Assert
                Assert.That(GetSingleEvent<NewQuestionAdded>(eventContext).Triggers, Contains.Item(autoPropagateGroupId));
            }
        }

        [Test]
        public void NewAddQuestion_When_User_Doesnot_Have_Permissions_For_Edit_Questionnaire_Then_DomainException_should_be_thrown()
        {
            // Arrange
            var autoPropagate = QuestionType.AutoPropagate;
            var autoPropagateGroupId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var triggedGroupIdsWithAutoPropagateGroupId = new[] { autoPropagateGroupId };
            Questionnaire questionnaire = CreateQuestionnaireWithAutoGroupAndRegularGroup(autoGroupPublicKey: autoPropagateGroupId, secondGroup: groupId, responsibleId: Guid.NewGuid());
            // act
            TestDelegate act = () => questionnaire.NewAddQuestion(Guid.NewGuid(), groupId, "Question", autoPropagate, "test", false,
                                         false,
                                         false, QuestionScope.Interviewer, string.Empty, string.Empty,
                                         string.Empty,
                                         string.Empty, new Option[0], Order.AZ, null,
                                         triggedGroupIdsWithAutoPropagateGroupId, responsibleId: Guid.NewGuid(),
                                         linkedToQuestionId: null, isInteger: false);
            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.DoesNotHavePermissionsForEdit));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_categorical_question_with_linked_question_that_does_not_exist_in_questionnaire_questions_scope_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid autoQuestionId = Guid.Parse("00000000-1111-0000-2222-111000000000");
            Guid questionId = Guid.Parse("00000000-1111-0000-2222-000000000000");
            Guid autoGroupId = Guid.Parse("00000000-1111-0000-3333-111000000000");
            Guid groupId = Guid.Parse("00000000-1111-0000-3333-000000000000");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire =
                CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
                    autoGroupPublicKey: autoGroupId,
                    secondGroup: groupId,
                    autoQuestionId: autoQuestionId,
                    questionId: questionId,
                    responsibleId: responsibleId,
                    questionType: questionType);

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "Question",
                    type: questionType,
                    alias: "test",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    options: null,
                    optionsOrder: Order.AZ,
                    maxValue: null,
                    triggedGroupIds: new Guid[0],
                    responsibleId: responsibleId,
                    linkedToQuestionId: Guid.NewGuid(), isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.LinkedQuestionDoesNotExist));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_categorical_question_with_linked_question_that_exist_in_autopropagated_group_questions_scope(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid autoQuestionId = Guid.Parse("00000000-1111-0000-2222-111000000000");
                Guid questionId = Guid.Parse("00000000-1111-0000-2222-000000000000");
                Guid autoGroupId = Guid.Parse("00000000-1111-0000-3333-111000000000");
                Guid groupId = Guid.Parse("00000000-1111-0000-3333-000000000000");
                Guid responsibleId = Guid.NewGuid();

                Questionnaire questionnaire =
                    CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
                        autoGroupPublicKey: autoGroupId,
                        secondGroup: groupId,
                        autoQuestionId: autoQuestionId,
                        questionId: questionId,
                        responsibleId: responsibleId,
                        questionType: questionType);

                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "Question",
                    type: questionType,
                    alias: "test",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    options: null,
                    optionsOrder: Order.AZ,
                    maxValue: null,
                    triggedGroupIds: new Guid[0],
                    responsibleId: responsibleId,
                    linkedToQuestionId: autoQuestionId, isInteger: null);

                // assert
                var risedEvent = GetLastEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual(autoQuestionId, risedEvent.LinkedToQuestionId);
            }
        }

        [Test]
        [TestCase(QuestionType.AutoPropagate)]
        [TestCase(QuestionType.DateTime)]
        [TestCase(QuestionType.GpsCoordinates)]
        [TestCase(QuestionType.Numeric)]
        [TestCase(QuestionType.Text)]
        public void NewAddQuestion_When_non_categorical_question_with_linked_question_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid autoQuestionId = Guid.Parse("00000000-1111-0000-2222-111000000000");
            Guid questionId = Guid.Parse("00000000-1111-0000-2222-000000000000");
            Guid autoGroupId = Guid.Parse("00000000-1111-0000-3333-111000000000");
            Guid groupId = Guid.Parse("00000000-1111-0000-3333-000000000000");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire =
                CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
                    autoGroupPublicKey: autoGroupId,
                    secondGroup: groupId,
                    autoQuestionId: autoQuestionId,
                    questionId: questionId,
                    responsibleId: responsibleId,
                    questionType: QuestionType.MultyOption);

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "Question",
                    type: questionType,
                    alias: "test",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    options: null,
                    optionsOrder: Order.AZ,
                    maxValue: null,
                    triggedGroupIds: new Guid[0],
                    responsibleId: responsibleId,
                    linkedToQuestionId: autoQuestionId, isInteger: false);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotCategoricalQuestionLinkedToAnoterQuestion));
        }


        [Test]
        [TestCase(QuestionType.DateTime)]
        [TestCase(QuestionType.Numeric)]
        [TestCase(QuestionType.Text)]
        public void NewAddQuestion_When_categorical_question_with_linked_question_with_number_or_text_or_datetime_type(QuestionType questionType)
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid autoQuestionId = Guid.Parse("00000000-1111-0000-2222-111000000000");
                Guid questionId = Guid.Parse("00000000-1111-0000-2222-000000000000");
                Guid autoGroupId = Guid.Parse("00000000-1111-0000-3333-111000000000");
                Guid groupId = Guid.Parse("00000000-1111-0000-3333-000000000000");
                Guid responsibleId = Guid.NewGuid();

                Questionnaire questionnaire =
                    CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
                        autoGroupPublicKey: autoGroupId,
                        secondGroup: groupId,
                        autoQuestionId: autoQuestionId,
                        questionId: questionId,
                        responsibleId: responsibleId,
                        questionType: QuestionType.MultyOption,
                        autoQuestionType: questionType);


                // act
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "Question",
                    type: QuestionType.MultyOption,
                    alias: "test",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    options: null,
                    optionsOrder: Order.AZ,
                    maxValue: null,
                    triggedGroupIds: new Guid[0],
                    responsibleId: responsibleId,
                    linkedToQuestionId: autoQuestionId, isInteger: null);

                // assert
                var risedEvent = GetLastEvent<NewQuestionAdded>(eventContext);
                Assert.AreEqual(autoQuestionId, risedEvent.LinkedToQuestionId);
            }
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_categorical_question_with_linked_question_that_not_of_type_text_or_number_or_datetime_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid autoQuestionId = Guid.Parse("00000000-1111-0000-2222-111000000000");
            Guid questionId = Guid.Parse("00000000-1111-0000-2222-000000000000");
            Guid autoGroupId = Guid.Parse("00000000-1111-0000-3333-111000000000");
            Guid groupId = Guid.Parse("00000000-1111-0000-3333-000000000000");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire =
                CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
                    autoGroupPublicKey: autoGroupId,
                    secondGroup: groupId,
                    autoQuestionId: autoQuestionId,
                    questionId: questionId,
                    responsibleId: responsibleId,
                    questionType: questionType,
                    autoQuestionType: QuestionType.GpsCoordinates);

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "Question",
                    type: questionType,
                    alias: "test",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    options: null,
                    optionsOrder: Order.AZ,
                    maxValue: null,
                    triggedGroupIds: new Guid[0],
                    responsibleId: responsibleId,
                    linkedToQuestionId: autoQuestionId, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.NotSupportedQuestionForLinkedQuestion));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_categorical_question_have_answers_and_linked_question_in_the_same_time_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid autoQuestionId = Guid.Parse("00000000-1111-0000-2222-111000000000");
            Guid questionId = Guid.Parse("00000000-1111-0000-2222-000000000000");
            Guid autoGroupId = Guid.Parse("00000000-1111-0000-3333-111000000000");
            Guid groupId = Guid.Parse("00000000-1111-0000-3333-000000000000");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire =
                CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
                    autoGroupPublicKey: autoGroupId,
                    secondGroup: groupId,
                    autoQuestionId: autoQuestionId,
                    questionId: questionId,
                    responsibleId: responsibleId,
                    questionType: questionType);

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "Question",
                    type: questionType,
                    alias: "test",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    options: new []{new Option(Guid.NewGuid(), "1", "auto"), }, 
                    optionsOrder: Order.AZ,
                    maxValue: null,
                    triggedGroupIds: new Guid[0],
                    responsibleId: responsibleId,
                    linkedToQuestionId: autoQuestionId, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.ConflictBetweenLinkedQuestionAndOptions));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_categorical_question_with_linked_question_that_does_not_exist_in_questions_scope_from_autopropagate_groups_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid autoQuestionId = Guid.Parse("00000000-1111-0000-2222-111000000000");
            Guid questionId = Guid.Parse("00000000-1111-0000-2222-000000000000");
            Guid questionThatLinkedButNotFromPropagateGroupId = Guid.Parse("00000000-1111-0000-2222-222000000000");
            Guid autoGroupId = Guid.Parse("00000000-1111-0000-3333-111000000000");
            Guid groupId = Guid.Parse("00000000-1111-0000-3333-000000000000");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire =
                CreateQuestionnaireWithAutoAndRegularGroupsAnd1QuestionInAutoGroupAnd2QuestionsInRegular(
                    autoGroupPublicKey: autoGroupId,
                    secondGroup: groupId,
                    autoQuestionId: autoQuestionId,
                    questionId: questionId,
                    responsibleId: responsibleId,
                    questionType: questionType,
                    questionThatLinkedButNotFromPropagateGroup: questionThatLinkedButNotFromPropagateGroupId);

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "Question",
                    type: questionType,
                    alias: "test",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    options: null,
                    optionsOrder: Order.AZ,
                    maxValue: null,
                    triggedGroupIds: new Guid[0],
                    responsibleId: responsibleId,
                    linkedToQuestionId: questionThatLinkedButNotFromPropagateGroupId, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.LinkedQuestionIsNotInPropagateGroup));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_categorical_question_with_linked_question_that_has_featured_status_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid autoQuestionId = Guid.Parse("00000000-1111-0000-2222-111000000000");
            Guid questionId = Guid.Parse("00000000-1111-0000-2222-000000000000");
            Guid autoGroupId = Guid.Parse("00000000-1111-0000-3333-111000000000");
            Guid groupId = Guid.Parse("00000000-1111-0000-3333-000000000000");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire =
                CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
                    autoGroupPublicKey: autoGroupId,
                    secondGroup: groupId,
                    autoQuestionId: autoQuestionId,
                    questionId: questionId,
                    responsibleId: responsibleId,
                    questionType: questionType);

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "Question",
                    type: questionType,
                    alias: "test",
                    isMandatory: false,
                    isFeatured: true,
                    isHeaderOfPropagatableGroup: false,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    options: null,
                    optionsOrder: Order.AZ,
                    maxValue: null,
                    triggedGroupIds: new Guid[0],
                    responsibleId: responsibleId,
                    linkedToQuestionId: autoQuestionId, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionWithLinkedQuestionCanNotBeFeatured));
        }

        [Test]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.MultyOption)]
        public void NewAddQuestion_When_categorical_question_with_linked_question_that_has_head_status_Then_DomainException_should_be_thrown(QuestionType questionType)
        {
            // arrange
            Guid autoQuestionId = Guid.Parse("00000000-1111-0000-2222-111000000000");
            Guid questionId = Guid.Parse("00000000-1111-0000-2222-000000000000");
            Guid autoGroupId = Guid.Parse("00000000-1111-0000-3333-111000000000");
            Guid groupId = Guid.Parse("00000000-1111-0000-3333-000000000000");
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire =
                CreateQuestionnaireWithAutoGroupAndRegularGroupAndQuestionsInThem(
                    autoGroupPublicKey: autoGroupId,
                    secondGroup: groupId,
                    autoQuestionId: autoQuestionId,
                    questionId: questionId,
                    responsibleId: responsibleId,
                    questionType: questionType);

            // act
            TestDelegate act =
                () =>
                questionnaire.NewAddQuestion(
                    questionId: Guid.NewGuid(),
                    groupId: groupId,
                    title: "Question",
                    type: questionType,
                    alias: "test",
                    isMandatory: false,
                    isFeatured: false,
                    isHeaderOfPropagatableGroup: true,
                    scope: QuestionScope.Interviewer,
                    condition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    options: null,
                    optionsOrder: Order.AZ,
                    maxValue: null,
                    triggedGroupIds: new Guid[0],
                    responsibleId: responsibleId,
                    linkedToQuestionId: autoQuestionId, isInteger: null);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionWithLinkedQuestionCanNotBeHead));
        }

    }
}