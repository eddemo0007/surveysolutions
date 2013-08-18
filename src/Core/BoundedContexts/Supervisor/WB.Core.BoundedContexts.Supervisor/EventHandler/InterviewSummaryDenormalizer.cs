﻿using System;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewSummaryDenormalizer : IEventHandler,
                                                IEventHandler<InterviewCreated>,
                                                IEventHandler<InterviewStatusChanged>,
                                                IEventHandler<SupervisorAssigned>,
                                                IEventHandler<TextQuestionAnswered>,
                                                IEventHandler<MultipleOptionsQuestionAnswered>,
                                                IEventHandler<SingleOptionQuestionAnswered>,
                                                IEventHandler<NumericQuestionAnswered>,
                                                IEventHandler<DateTimeQuestionAnswered>,
                                                IEventHandler<InterviewerAssigned>,
                                                IEventHandler<InterviewDeleted>
        

    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviews;
        private readonly IReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires;
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public InterviewSummaryDenormalizer(IReadSideRepositoryWriter<UserDocument> users,
                                            IReadSideRepositoryWriter<InterviewSummary> interviews,
                                            IReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires)
        {
            this.users = users;
            this.interviews = interviews;
            this.questionnaires = questionnaires;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new[] {typeof (UserDocument), typeof (QuestionnaireBrowseItem)}; }
        }

        public Type[] BuildsViews
        {
            get { return new[] {typeof (InterviewSummary)}; }
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer.ToString("d", CultureInfo.InvariantCulture));
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            UserDocument responsible = this.users.GetById(evnt.Payload.UserId);
            var interview = new InterviewSummary(this.questionnaires.GetById(evnt.Payload.QuestionnaireId))
                {
                    InterviewId = evnt.EventSourceId,
                    UpdateDate = evnt.EventTimeStamp,
                    QuestionnaireId = evnt.Payload.QuestionnaireId,
                    QuestionnaireVersion = evnt.Payload.QuestionnaireVersion,
                    QuestionnaireTitle = questionnaires.GetById(evnt.Payload.QuestionnaireId).Title,
                    ResponsibleId = evnt.Payload.UserId, // Creator is responsible
                    ResponsibleName = this.users.GetById(evnt.Payload.UserId).UserName,
                    ResponsibleRole = responsible.Roles.FirstOrDefault()
                };
            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            InterviewSummary interview = this.interviews.GetById(evnt.EventSourceId);

            interview.Status = evnt.Payload.Status;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                                string.Join(",", evnt.Payload.SelectedValues.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray()));
        }

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer.ToString(CultureInfo.InvariantCulture));
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                                evnt.Payload.SelectedValue.ToString(CultureInfo.InvariantCulture));
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            InterviewSummary interview = this.interviews.GetById(evnt.EventSourceId);

            var supervisorName = this.users.GetById(evnt.Payload.SupervisorId).UserName;

            interview.ResponsibleId = evnt.Payload.SupervisorId;
            interview.ResponsibleName = supervisorName;
            interview.ResponsibleRole = UserRoles.Supervisor;
            interview.TeamLeadId = evnt.Payload.SupervisorId;
            interview.TeamLeadName = supervisorName;

            this.interviews.Store(interview, interview.InterviewId);
        }


        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            InterviewSummary interview = this.interviews.GetById(evnt.EventSourceId);

            var interviewerName = this.users.GetById(evnt.Payload.InterviewerId).UserName;

            interview.ResponsibleId = evnt.Payload.InterviewerId;
            interview.ResponsibleName = interviewerName;
            interview.ResponsibleRole = UserRoles.Operator;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            InterviewSummary interview = this.interviews.GetById(evnt.EventSourceId);

            interview.IsDeleted = true;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }

        private void AnswerQuestion(Guid interviewId, Guid questionId, string answer)
        {
            InterviewSummary interview = this.interviews.GetById(interviewId);

            if (!interview.AnswersToFeaturedQuestions.ContainsKey(questionId))
                return;

            interview.AnswersToFeaturedQuestions[questionId].Answer = answer;

            this.interviews.Store(interview, interview.InterviewId);
        }
    }
}