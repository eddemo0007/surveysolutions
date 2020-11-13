using Main.Core.Entities.SubEntities;
using System;
using System.Linq;
using Dapper;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFactory : IInterviewFactory
    {
        private readonly IUnitOfWork sessionProvider;
        private readonly IWorkspaceNameProvider workspaceNameProvider;

        public InterviewFactory(IUnitOfWork sessionProvider,
            IWorkspaceNameProvider workspaceNameProvider)
        {
            this.sessionProvider = sessionProvider;
            this.workspaceNameProvider = workspaceNameProvider;
        }

        public Identity[] GetFlaggedQuestionIds(Guid interviewId)
            => this.sessionProvider.Session.Query<InterviewFlag>()
                .Where(y => y.InterviewId == interviewId.ToString("N"))
                .Select(x => x.QuestionIdentity).ToArray()
                .Select(Identity.Parse).ToArray();

        public void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged)
        {
            var sInterviewId = interviewId.ToString("N");
            var interview = this.sessionProvider.Session.Query<InterviewSummary>()
                .Where(x => x.SummaryId == sInterviewId)
                .Select(x => new { ReceivedByInterviewer = x.ReceivedByInterviewerAtUtc.HasValue, x.Status})
                .FirstOrDefault();

            if (interview == null)
                throw new InterviewException($"Interview {interviewId} not found.");
            if (interview.ReceivedByInterviewer)
                throw new InterviewException($"Can't modify Interview {interviewId} on server, because it received by interviewer.");
            if (interview.Status == InterviewStatus.ApprovedByHeadquarters)
                throw new InterviewException($"Interview was approved by Headquarters and cannot be edited. InterviewId: {interviewId}");

            if (flagged)
            {
                this.sessionProvider.Session.SaveOrUpdate(new InterviewFlag
                {
                    InterviewId = sInterviewId,
                    QuestionIdentity = questionIdentity.ToString()
                });
            }
            else
            {
                this.sessionProvider.Session.Query<InterviewFlag>()
                    .Where(y => y.InterviewId == sInterviewId && y.QuestionIdentity == questionIdentity.ToString())
                    .Delete();
            }
        }

        public void RemoveInterview(Guid interviewId)
        {
            var conn = sessionProvider.Session.Connection;
            conn.Execute($@"DELETE FROM {this.workspaceNameProvider.CurrentWorkspace()}.report_statistics i
                     WHERE i.interview_id = s.id AND s.interview_id = @InterviewId",
            new { InterviewId = interviewId });
        }

        private static readonly InterviewStatus[] DisabledStatusesForGps =
        {
            InterviewStatus.ApprovedBySupervisor,
            InterviewStatus.ApprovedByHeadquarters
        };

        public InterviewGpsAnswer[] GetGpsAnswers(Guid questionnaireId, long? questionnaireVersion,
            string gpsQuestionVariableName, int? maxAnswersCount, Guid? supervisorId)
        {
            var gpsQuery = QueryGpsAnswers()
                .Where(x => x.Answer.IsEnabled &&
                            x.QuestionnaireItem.StataExportCaption == gpsQuestionVariableName &&
                            x.InterviewSummary.QuestionnaireId == questionnaireId);

            if (questionnaireVersion.HasValue)
            {
                gpsQuery = gpsQuery
                    .Where(x => x.InterviewSummary.QuestionnaireVersion == questionnaireVersion.Value);
            }

            if (supervisorId.HasValue)
            {
                gpsQuery = gpsQuery
                    .Where(x => x.InterviewSummary.SupervisorId == supervisorId.Value 
                                && !DisabledStatusesForGps.Contains(x.InterviewSummary.Status));
            }

            var result = gpsQuery.Select(x => new InterviewGpsAnswer
            {
                InterviewId = x.InterviewSummary.InterviewId,
                RosterVector = x.Answer.RosterVector,
                Latitude = x.Answer.Latitude,
                Longitude = x.Answer.Longitude
            });

            if (maxAnswersCount.HasValue)
            {
                result = result.Take(maxAnswersCount.Value);
            }

            return result.ToArray();
        }
        
        public InterviewGpsAnswerWithTimeStamp[] GetGpsAnswersForInterviewer(Guid interviewerId) =>
            QueryGpsAnswers()
                .Where(x => x.Answer.IsEnabled 
                            && x.InterviewSummary.ResponsibleId == interviewerId 
                            && x.QuestionnaireItem.QuestionScope == QuestionScope.Interviewer)
                .Select(x => new InterviewGpsAnswerWithTimeStamp
                {
                    EntityId = x.Answer.QuestionId,
                    InterviewId = x.InterviewSummary.InterviewId,
                    Latitude = x.Answer.Latitude,
                    Longitude = x.Answer.Longitude,
                    Timestamp = x.Answer.Timestamp,
                    Idenifying = x.QuestionnaireItem.Featured == true,
                    Status = x.InterviewSummary.Status
                })
                .ToArray();

        private IQueryable<GpsAnswerQuery> QueryGpsAnswers()
        {
            return this.sessionProvider.Session
                .Query<InterviewGps>()
                .Join(this.sessionProvider.Session.Query<InterviewSummary>(),
                    gps => gps.InterviewSummary.Id,
                    interview => interview.Id,
                    (gps, interview) => new  { gps, interview})
                .Join(this.sessionProvider.Session.Query<QuestionnaireCompositeItem>(),
                    interview_gps => new { interview_gps.interview.QuestionnaireIdentity, interview_gps.gps.QuestionId },
                    questionnaireItem => new { questionnaireItem.QuestionnaireIdentity, QuestionId = questionnaireItem.EntityId },
                    (interview_gps, questionnaireItem) => new GpsAnswerQuery
                    {
                        InterviewSummary = interview_gps.interview,
                        Answer = interview_gps.gps,
                        QuestionnaireItem = questionnaireItem
                    });
        }

        public bool HasAnyGpsAnswerForInterviewer(Guid interviewerId) =>
            QueryGpsAnswers()
                .Any(x => x.Answer.IsEnabled 
                            && x.InterviewSummary.ResponsibleId == interviewerId 
                            && x.QuestionnaireItem.QuestionScope == QuestionScope.Interviewer);

        private class GpsAnswerQuery
        {
            public InterviewGps Answer { get; set; }
            public InterviewSummary InterviewSummary { get; set; }
            public QuestionnaireCompositeItem QuestionnaireItem { get; set; }
        }
    }
}
