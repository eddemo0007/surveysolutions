﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewEntityViewFactory
    {
        private readonly ISubstitutionService substitutionService;

        public InterviewEntityViewFactory(ISubstitutionService substitutionService, IVariableToUIStringService variableToUiStringService)
        {
            this.substitutionService = substitutionService;
        }

        public InterviewQuestionView BuildInterviewLinkedQuestionView(IQuestion question, 
            InterviewQuestion answeredQuestion,
            Dictionary<string, string> answersForTitleSubstitution,
            Dictionary<decimal[], string> availableOptions, 
            bool isParentGroupDisabled, 
            decimal[] rosterVector, 
            InterviewStatus interviewStatus)
        {
            var questionView = new InterviewQuestionView();
            this.FillInterviewQuestionView(questionView, question, answeredQuestion, answersForTitleSubstitution, isParentGroupDisabled, rosterVector, interviewStatus);

            questionView.Options = availableOptions
                .Select(a => new QuestionOptionView { Value = a.Key, Label = a.Value })
                .ToList();

            if (answeredQuestion != null)
                questionView.Answer = answeredQuestion.Answer;

            return questionView;
        }

        public InterviewQuestionView BuildInterviewQuestionView(IQuestion question, 
            InterviewQuestion answeredQuestion, 
            Dictionary<string, string> answersForTitleSubstitution, 
            bool isParentGroupDisabled, 
            decimal[] rosterVector,
            InterviewStatus interviewStatus)
        {
            var questionView = new InterviewQuestionView();
            this.FillInterviewQuestionView(questionView, question, answeredQuestion, answersForTitleSubstitution, isParentGroupDisabled, rosterVector, interviewStatus);
            return questionView;
        }

        private void FillInterviewQuestionView(InterviewQuestionView questionView, 
            IQuestion question, 
            InterviewQuestion answeredQuestion,
            Dictionary<string, string> answersForTitleSubstitution, 
            bool isParentGroupDisabled, 
            decimal[] rosterVector,
            InterviewStatus interviewStatus)
        {
            questionView.Id = question.PublicKey;
            questionView.RosterVector = rosterVector;
            questionView.Title = this.GetTextWithSubstitutedVariables(question.QuestionText, answersForTitleSubstitution);
            questionView.QuestionType = question.QuestionType;
            questionView.IsFeatured = question.Featured;
            questionView.Variable = question.StataExportCaption;
            questionView.IsValid = true;
            questionView.IsEnabled = (question.QuestionScope == QuestionScope.Supervisor) || (answeredQuestion == null) && !isParentGroupDisabled;
            questionView.IsReadOnly = !(question.QuestionScope == QuestionScope.Supervisor && interviewStatus < InterviewStatus.ApprovedByHeadquarters);
            questionView.Scope = question.QuestionScope;

            if (question.Answers != null)
            {
                questionView.Options = question.Answers.Select(a => new QuestionOptionView
                {
                    Value = decimal.Parse(a.AnswerValue),
                    Label = a.AnswerText
                }).ToList();
            }

            var numericQuestion = question as INumericQuestion;
            if (numericQuestion != null)
            {
                questionView.Settings = new NumericQuestionSettings
                {
                    IsInteger = numericQuestion.IsInteger,
                    CountOfDecimalPlaces = numericQuestion.CountOfDecimalPlaces,
                    UseFormating = numericQuestion.UseFormatting
                };
            }

            var categoricalMultiQuestion = question as MultyOptionsQuestion;
            if (categoricalMultiQuestion != null)
            {
                questionView.Settings = new MultiQuestionSettings
                {
                    YesNoQuestion = categoricalMultiQuestion.YesNoView,
                    AreAnswersOrdered = categoricalMultiQuestion.AreAnswersOrdered,
                    MaxAllowedAnswers = categoricalMultiQuestion.MaxAllowedAnswers,
                    IsLinked =
                        (categoricalMultiQuestion.LinkedToQuestionId.HasValue ||
                         categoricalMultiQuestion.LinkedToRosterId.HasValue)
                };
            }

            var categoricalSingleQuestion = question as SingleQuestion;
            if (categoricalSingleQuestion != null)
            {
                questionView.Settings = new SingleQuestionSettings
                {
                    IsFilteredCombobox = categoricalSingleQuestion.IsFilteredCombobox ?? false,
                    IsCascade = categoricalSingleQuestion.CascadeFromQuestionId.HasValue,
                    IsLinked =
                        (categoricalSingleQuestion.LinkedToQuestionId.HasValue ||
                         categoricalSingleQuestion.LinkedToRosterId.HasValue)
                };
            }

            var textQuestion = question as TextQuestion;
            if (textQuestion != null)
            {
                questionView.Settings = new TextQuestionSettings
                {
                    Mask = textQuestion.Mask
                };
            }

            var dateTimeQuestion = question as DateTimeQuestion;
            if (dateTimeQuestion != null)
            {
                questionView.Settings = new DateTimeQuestionSettings
                {
                    IsTimestamp = question.IsTimestamp
                };
            }

            if (answeredQuestion == null) return;

            questionView.IsAnswered = answeredQuestion.IsAnswered();
            questionView.IsEnabled = !isParentGroupDisabled && !answeredQuestion.IsDisabled();
            questionView.IsFlagged = answeredQuestion.IsFlagged();
            questionView.Comments =
                (answeredQuestion.Comments ?? new List<InterviewQuestionComment>()).Select(x => new InterviewQuestionCommentView
                {
                    Id = x.Id,
                    Text = x.Text,
                    CommenterId = x.CommenterId,
                    CommenterName = x.CommenterName,
                    Date = x.Date
                }).ToList();
            questionView.Answer = answeredQuestion.Answer;

            var textListQuestion = question as ITextListQuestion;
            if (textListQuestion != null)
            {
                questionView.Answer = null;
                var typedAnswer = answeredQuestion.Answer as InterviewTextListAnswers;
                if (typedAnswer != null)
                {
                    questionView.Options = typedAnswer.Answers.Select(a => new QuestionOptionView
                    {
                        Value = a.Value,
                        Label = a.Answer
                    }).ToList();
                }
            }

            bool shouldBeValidByConvention = !questionView.IsEnabled;

            questionView.IsValid = shouldBeValidByConvention || !answeredQuestion.IsInvalid();
            questionView.AnswerString = this.FormatAnswerAsString(questionView, answeredQuestion.Answer);

            questionView.FailedValidationMessages = answeredQuestion.FailedValidationConditions
                .Select(x => question.ValidationConditions[x.FailedConditionIndex])
                .Select(vc => new ValidationCondition()
                {
                    Expression = vc.Expression,
                    Message = this.GetTextWithSubstitutedVariables(vc.Message, answersForTitleSubstitution)
                })
                .ToList();
        }

        public InterviewStaticTextView BuildInterviewStaticTextView(IStaticText staticText, 
            InterviewStaticText interviewStaticText,
            Dictionary<string, string> answersForTitleSubstitution,
            InterviewAttachmentViewModel attachment) 
        {
            var staticTextView = new InterviewStaticTextView();
            staticTextView.Id = staticText.PublicKey;
            staticTextView.Text = this.GetTextWithSubstitutedVariables(staticText.Text, answersForTitleSubstitution); 
            staticTextView.Attachment = attachment;

            if (interviewStaticText != null)
            {
                staticTextView.IsEnabled = interviewStaticText.IsEnabled;
                staticTextView.IsValid = !interviewStaticText.IsInvalid;
                staticTextView.FailedValidationMessages = interviewStaticText.FailedValidationConditions
                    .Select(x => staticText.ValidationConditions[x.FailedConditionIndex])
                    .Select(vc => new ValidationCondition()
                    {
                        Expression = vc.Expression,
                        Message = this.GetTextWithSubstitutedVariables(vc.Message, answersForTitleSubstitution)
                    })
                    .ToList();
            }

            return staticTextView;
        }

        public InterviewAttachmentViewModel BuildInterviewAttachmentViewModel(string contentId, string contentType, string contentName)
        {
            var view = new InterviewAttachmentViewModel();
            view.ContentId = contentId;
            view.ContentType = contentType;
            view.ContentName = contentName;
            return view;
        }


        private string FormatAnswerAsString(InterviewQuestionView questionView, object answer)
        {
            if (answer == null) return "";
            switch (questionView.QuestionType)
            {
                case QuestionType.SingleOption:
                    if (questionView.Settings != null && (questionView.Settings as SingleQuestionSettings).IsLinked)
                    {
                        return AnswerUtils.AnswerToString(answer);
                    }
                    else
                    {
                        return AnswerUtils.AnswerToString(answer, x => questionView.Options.First(o => (decimal)o.Value == x).Label);
                    }

                case QuestionType.MultyOption:
                    if ((questionView.Settings as MultiQuestionSettings).IsLinked)
                    {
                        return AnswerUtils.AnswerToString(answer);
                    }
                    else
                    {
                        return AnswerUtils.AnswerToString(answer, x => questionView.Options.First(o => (decimal)o.Value == x).Label);
                    }
                case QuestionType.DateTime:
                    if (answer is DateTime)
                    {
                        var date = (DateTime)answer;

                        return ((DateTimeQuestionSettings)questionView.Settings).IsTimestamp ? date.ToString("t") : date.ToString("u");
                    }
                    break;
                case QuestionType.Numeric:
                    var intAnswer = answer as int?;
                    var longAnswer = intAnswer ??  (answer as long?);
                    var decimalAnswer = longAnswer ?? (answer as decimal?);

                    return decimalAnswer.FormatDecimal();
                case QuestionType.GpsCoordinates:
                case QuestionType.TextList:
                case QuestionType.Text:
                case QuestionType.QRBarcode:
                case QuestionType.Multimedia:
                    return AnswerUtils.AnswerToString(answer);
            }
            return "";
        }

        private string GetTextWithSubstitutedVariables(string title, Dictionary<string, string> answersForTitleSubstitution)
        {
            IEnumerable<string> usedVariables = this.substitutionService.GetAllSubstitutionVariableNames(title);

            foreach (string usedVariable in usedVariables)
            {
                string escapedVariable = $"%{usedVariable}%";
                string actualAnswerOrDots = answersForTitleSubstitution.ContainsKey(usedVariable) ? answersForTitleSubstitution[usedVariable] : "[...]";

                title = title.Replace(escapedVariable, actualAnswerOrDots);
            }

            return title;
        }
    }
}