﻿using System;
using System.Linq;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class Assignment : IReadSideRepositoryEntity
    {
        public Assignment()
        {
            this.Answers = new List<InterviewAnswer>();
            this.IdentifyingData = new List<IdentifyingAnswer>();
            this.InterviewSummaries = new HashSet<InterviewSummary>();
            this.ProtectedVariables = new List<string>();
        }

        internal Assignment(QuestionnaireIdentity questionnaireId,
            Guid responsibleId, 
            int? quantity,
            bool isAudioRecordingEnabled,
            string email,
            string password,
            bool? webMode) : this()
        {
            this.ResponsibleId = responsibleId;
            this.Quantity = quantity;
            this.QuestionnaireId = questionnaireId;
            this.IsAudioRecordingEnabled = isAudioRecordingEnabled;
            this.Email = email;
            this.Password = password;
            this.WebMode = webMode;
        }

        public virtual Guid AggregateRootId { get; set; }

        public virtual int Id { get; set; }

        public virtual Guid ResponsibleId { get; set; }

        public virtual ReadonlyUser Responsible { get; protected set; }

        public virtual int? Quantity { get; set; }
                
        public virtual bool Archived { get; set; }

        public virtual DateTime CreatedAtUtc { get; set; }

        public virtual DateTime UpdatedAtUtc { get; set; }

        public virtual DateTime? ReceivedByTabletAtUtc { get; set; }

        public virtual QuestionnaireIdentity QuestionnaireId { get; set; }

        public virtual bool IsAudioRecordingEnabled { get; set; }

        public virtual string Email { get; set; }
        public virtual string Password { get; set; }
        public virtual bool? WebMode { get; set; }

        public virtual IList<IdentifyingAnswer> IdentifyingData { get; protected set; }

        public virtual IList<InterviewAnswer> Answers { get; protected set; }

        public virtual QuestionnaireLiteViewItem Questionnaire { get; set; }

        public virtual ISet<InterviewSummary> InterviewSummaries { get; protected set; }

        public virtual List<string> ProtectedVariables { get; set; }

        public virtual int InterviewsProvided =>
            InterviewSummaries.Count(i => i.Status == InterviewStatus.InterviewerAssigned ||
                                          i.Status == InterviewStatus.Completed ||
                                          i.Status == InterviewStatus.RejectedBySupervisor);

        public virtual int? InterviewsNeeded => this.Quantity.HasValue
            ? this.Quantity - this.InterviewSummaries.Count
            : null;

        public virtual bool IsCompleted => this.InterviewsNeeded <= 0;


        public virtual void SetAudioRecordingEnabled(bool enabled, DateTime utcDateTime)
        {
            this.IsAudioRecordingEnabled = enabled;
            this.UpdatedAtUtc = utcDateTime;
        }

        public virtual void UpdateQuantity(int? quantity, DateTime utcDateTime)
        {
            this.Quantity = quantity == -1 ? null : quantity;
            this.UpdatedAtUtc = utcDateTime;
        }

        public virtual void Reassign(Guid responsibleId, DateTime utcDateTime)
        {
            this.ResponsibleId = responsibleId;
            this.UpdatedAtUtc = utcDateTime;
            this.ReceivedByTabletAtUtc = null;
        }

        public virtual void SetIdentifyingData(IList<IdentifyingAnswer> identifyingAnswers)
        {
            this.IdentifyingData = identifyingAnswers;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void SetAnswers(IList<InterviewAnswer> answers)
        {
            this.Answers = answers;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void Unarchive()
        {
            this.Archived = false;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void SetProtectedVariables(List<string> protectedVariables)
        {
            this.ProtectedVariables = protectedVariables;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual void MarkAsReceivedByTablet()
        {
            this.ReceivedByTabletAtUtc = DateTime.UtcNow;
        }

        public virtual void UpdateEmail(string email)
        {
            this.Email = email;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }
        public virtual void UpdatePassword(string password)
        {
            this.Password = password?.ToUpperInvariant();
            this.UpdatedAtUtc = DateTime.UtcNow;
        }
        public virtual void UpdateMode(bool? mode)
        {
            this.WebMode = mode;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }

        public virtual bool InPrivateWebMode()
        {
            return (WebMode == null || WebMode == true) && Quantity == 1;
        }

        public static Assignment PrefillFromInterview(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            Assignment result = new Assignment();

            result.QuestionnaireId = interview.QuestionnaireIdentity;
            var prefilledQuestions = questionnaire.GetPrefilledQuestions();
            foreach (var prefilledQuestion in prefilledQuestions)
            {
                var questionIdentity = new Identity(prefilledQuestion, RosterVector.Empty);

                var question = interview.GetQuestion(questionIdentity);
                if (question.IsAnswered())
                {
                    var answer = interview.GetAnswerAsString(questionIdentity);
                    if (question.IsSingleFixedOption)
                    {
                        answer = question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue.ToString();
                    }

                    result.IdentifyingData.Add(IdentifyingAnswer.Create(result, questionnaire, answer, questionIdentity));

                    result.Answers.Add(
                        new InterviewAnswer
                        {
                            Identity = questionIdentity,
                            Answer = question.InterviewQuestion.Answer
                        });
                }
            }

            return result;
        }
    }

    public class QuestionnaireLiteViewItem
    {
        public virtual string Title { get; set; }
        public virtual string Id { get; set; }
        public virtual bool? IsAudioRecordingEnabled { get; set; }
    }
}
