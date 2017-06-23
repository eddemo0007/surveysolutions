using System;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Services
{
    public interface IInterviewImportService
    {
        AssignmentImportStatus Status { get; }
        void ImportAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, Guid? supervisorId, Guid headquartersId, PreloadedContentType mode);

        void VerifyAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId);
    }
}