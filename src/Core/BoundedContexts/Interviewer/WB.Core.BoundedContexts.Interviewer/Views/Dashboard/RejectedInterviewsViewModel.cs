using System;
using System.Linq.Expressions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class RejectedInterviewsViewModel : BaseInterviewsViewModel
    {
        public override DashboardGroupType DashboardType => DashboardGroupType.InvalidInterviews;
        public override string TabTitle => EnumeratorUIResources.Dashboard_RejectedLinkText;
        public override string TabDescription => EnumeratorUIResources.Dashboard_RejectedTabText;
        protected override Expression<Func<InterviewView, bool>> GetDbQuery()
        {
            var interviewerId = Principal.CurrentUserIdentity.UserId;

            return interview => interview.ResponsibleId == interviewerId &&
                                (interview.Mode == InterviewMode.CAPI || interview.Mode == null) && 
                                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.RejectedBySupervisor;
        }
        
        public RejectedInterviewsViewModel(IPlainStorage<InterviewView> interviewViewRepository, 
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IPrincipal principal) : base(viewModelFactory, interviewViewRepository, identifyingQuestionsRepo, principal)
        {
        }
    }
}
