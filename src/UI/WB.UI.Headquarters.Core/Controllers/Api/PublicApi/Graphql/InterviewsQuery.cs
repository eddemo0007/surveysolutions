using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class InterviewsQuery
    {
        public IQueryable<InterviewSummary> GetInterviews([Service] IUnitOfWork unitOfWork, [Service]IAuthorizedUser user)
        {
            unitOfWork.DiscardChanges();

            var interviewSummaries = unitOfWork.Session.Query<InterviewSummary>();
            if (user.IsSupervisor)
            {
                interviewSummaries = interviewSummaries.Where(x => x.TeamLeadId == user.Id || x.ResponsibleId == user.Id);
            }

            if (user.IsInterviewer)
            {
                interviewSummaries = interviewSummaries.Where(x => x.ResponsibleId == user.Id);
            }
            
            return interviewSummaries;
        }
    }
}
