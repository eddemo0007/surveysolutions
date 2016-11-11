using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class WarningsTests
    {
        [Test]
        public void no_current_time_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.DateTimeQuestion(),
                })
                .ExpectWarning("WB0221");

        [Test]
        public void current_time_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.DateTimeQuestion(isCurrentTime: true),
                })
                .ExpectNoWarning("WB0221");

        [Test]
        public void no_prefilled_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectWarning("WB0216");

        [Test]
        public void prefilled_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(isPrefilled: true),
                })
                .ExpectNoWarning("WB0216");

        [Test]
        public void variable_label_length_121()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(variableLabel: new string(Enumerable.Range(1, 121).Select(x => 'a').ToArray())),
                })
                .ExpectWarning("WB0217");

        [Test]
        public void variable_label_length_120()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(variableLabel: new string(Enumerable.Range(1, 120).Select(x => 'a').ToArray())),
                })
                .ExpectNoWarning("WB0217");

        [Test]
        public void no_gps_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectWarning("WB0211")
                .AndNoWarning("WB0264");

        [Test]
        public void gps_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.GpsCoordinateQuestion(),
                })
                .ExpectNoWarning("WB0211")
                .AndWarning("WB0264");

        [Test]
        public void no_single_option_prefilled_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectNoWarning("WB0222");

        [Test]
        public void single_option_prefilled_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.SingleOptionQuestion(isPrefilled: true),
                })
                .ExpectWarning("WB0222");

        [Test]
        public void no_barcode_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectNoWarning("WB0267");

        [Test]
        public void barcode_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.QRBarcodeQuestion(),
                })
                .ExpectWarning("WB0267");

        [Test]
        public void less_than_50_percent_questions_with_validations()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                    Create.Question(),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition() }),
                })
                .ExpectWarning("WB0208");

        [Test]
        public void more_than_50_percent_questions_with_validations()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition() }),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition() }),
                })
                .ExpectNoWarning("WB0208");

        [Test]
        public void more_than_30_percent_questions_are_text()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.TextQuestion(),
                    Create.TextQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.NumericIntegerQuestion(),
                })
                .ExpectWarning("WB0265");

        [Test]
        public void less_than_30_percent_questions_are_text()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.TextQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.NumericIntegerQuestion(),
                })
                .ExpectNoWarning("WB0265");

        [Test]
        public void questions_with_same_title()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(title: "Question"),
                    Create.Question(title: "Question"),
                })
                .ExpectWarning("WB0266");

        [Test]
        public void questions_with_different_titles()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(title: "Question 1"),
                    Create.Question(title: "Question 2"),
                })
                .ExpectNoWarning("WB0266");

        [Test]
        public void consecutive_questions_with_same_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                })
                .ExpectWarning("WB0218");

        [Test]
        public void consecutive_questions_with_same_empty_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "   "),
                    Create.Question(enablementCondition: "   "),
                    Create.Question(enablementCondition: "   "),
                })
                .ExpectNoWarning("WB0218");

        [Test]
        public void independent_questions_with_same_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(),
                    Create.Question(enablementCondition: "x > 10"),
                })
                .ExpectNoWarning("WB0218");

        [Test]
        public void consecutive_questions_with_different_enablements()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "y > 10"),
                    Create.Question(enablementCondition: "z > 10"),
                })
                .ExpectNoWarning("WB0218");

        [Test]
        public void consecutive_unconditional_single_option_questions_with_2_options()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 1, 2 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 10, 20 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 100, 200 }),
                })
                .ExpectWarning("WB0219");

        [Test]
        public void consecutive_unconditional_single_option_questions_with_3_options()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 1, 2, 3 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 10, 20, 30 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 100, 200, 300 }),
                })
                .ExpectNoWarning("WB0219");

        [Test]
        public void independent_unconditional_single_option_questions_with_2_options()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 1, 2 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 10, 20 }),
                    Create.Question(),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 100, 200 }),
                })
                .ExpectNoWarning("WB0219");

        [Test]
        public void consecutive_conditional_single_option_questions_with_2_options()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 1, 2 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 10, 20 }, enablementCondition: "x > 0"),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 100, 200 }),
                })
                .ExpectNoWarning("WB0219");

        [Test]
        public void used_rowindex_inside_multioption_based_roster()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.MultipleOptionsQuestion(questionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111"), children: new []
                    {
                        Create.Question(enablementCondition: "@rowindex > 2"),
                    }),
                })
                .ExpectWarning("WB0220");

        [Test]
        public void used_rowcode_inside_multioption_based_roster()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.MultipleOptionsQuestion(questionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111"), children: new []
                    {
                        Create.Question(enablementCondition: "@rowcode > 2"),
                    }),
                })
                .ExpectNoWarning("WB0220");

        [Test]
        public void section_with_4_questions_and_other_big_section()
            => Create.QuestionnaireDocument(children: new IComposite[]
                {
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                })
                .ExpectWarning("WB0223");

        [Test]
        public void section_with_5_questions_and_other_big_section()
            => Create.QuestionnaireDocument(children: new IComposite[]
                {
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                })
                .ExpectNoWarning("WB0223");

        [Test]
        public void section_with_4_questions_and_no_other_sections()
            => Create.QuestionnaireDocument(children: new IComposite[]
                {
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                })
                .ExpectNoWarning("WB0223");

        [Test]
        public void section_with_10_subsections_at_one_level()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                })
                .ExpectWarning("WB0224");

        [Test]
        public void section_with_10_subsections_at_one_level_inside_other_subsection()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(children: new []
                    {
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                    }),
                })
                .ExpectWarning("WB0224");

        [Test]
        public void section_with_9_subsections_at_one_level_inside_other_subsection()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(children: new []
                    {
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                    }),
                })
                .ExpectNoWarning("WB0224");

        [Test]
        public void section_with_10_subsections_at_different_levels()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(children: new []
                    {
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                    }),
                    Create.Subsection(children: new []
                    {
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                    }),
                })
                .ExpectNoWarning("WB0224");

        [Test]
        public void section_with_9_subsections_at_one_level()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                })
                .ExpectNoWarning("WB0224");

        [Test]
        public void single_option_with_9_options_in_combobox_mode()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(isComboBox: true, answers: new List<Answer>
                    {
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                    }),
                })
                .ExpectWarning("WB0225");

        [Test]
        public void single_option_with_9_options_in_regular_mode()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(isComboBox: false, answers: new List<Answer>
                    {
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                    }),
                })
                .ExpectNoWarning("WB0225");

        [Test]
        public void single_option_with_10_options_in_combobox_mode()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(isComboBox: true, answers: new List<Answer>
                    {
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                        Create.Answer(),
                    }),
                })
                .ExpectNoWarning("WB0225");
    }
}