using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewFactory
    {
        Identity[] GetQuestionsWithFlagBySectionId(QuestionnaireIdentity questionnaireId, Guid interviewId, Identity sectionId);
        Identity[] GetFlaggedQuestionIds(Guid interviewId);
        void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged);
        void RemoveInterview(Guid interviewId);

        InterviewStringAnswer[] GetMultimediaAnswersByQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            Guid[] multimediaQuestionIds);

        InterviewStringAnswer[] GetAudioAnswersByQuestionnaire(QuestionnaireIdentity questionnaireIdentity);
        Guid[] GetAnsweredGpsQuestionIdsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity);

        InterviewGpsAnswer[] GetGpsAnswersByQuestionIdAndQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            Guid gpsQuestionId, int maxAnswersCount, double northEastCornerLatitude,
            double southWestCornerLatitude, double northEastCornerLongtitude, double southWestCornerLongtitude);

        string[] GetQuestionnairesWithAnsweredGpsQuestions();

        string[] GetQuestionnairesWithAnsweredGpsQuestionsBySupervisor(Guid supervisorId);

        Guid[] GetAnsweredGpsQuestionIdsByQuestionnaireAndSupervisor(QuestionnaireIdentity questionnaireIdentity, Guid supervisorId);

        InterviewGpsAnswer[] GetGpsAnswersByQuestionIdAndQuestionnaireAndSupervisor(QuestionnaireIdentity questionnaireIdentity,
            Guid gpsQuestionId, int maxAnswersCount, double northEastCornerLatitude,
            double southWestCornerLatitude, double northEastCornerLongtitude, double southWestCornerLongtitude, Guid supervisorId);

        List<InterviewEntity> GetInterviewEntities(QuestionnaireIdentity questionnaireId, Guid interviewId);
        Dictionary<string, InterviewLevel> GetInterviewDataLevels(QuestionnaireIdentity questionnaireId, List<InterviewEntity> interviewEntities);
        void Save(InterviewState interviewState);
    }
}