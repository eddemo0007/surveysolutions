using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;

namespace WB.UI.Tester.Infrastructure.Internals
{
    internal class DesignerApiService : IDesignerApiService
    {
        private readonly IRestService restService;
        private readonly IUserIdentity userIdentity;

        public DesignerApiService(
            IRestService restService, 
            IUserIdentity userIdentity)
        {
            this.restService = restService;
            this.userIdentity = userIdentity;
        }

        public async Task<bool> Authorize(string login, string password)
        {
            await this.restService.GetAsync(
                url: "login",
                credentials: new RestCredentials()
                {
                    Login = login,
                    Password = password
                });

            return true;
        }

        public async virtual Task<IList<QuestionnaireListItem>> GetQuestionnairesAsync(bool isPublic, CancellationToken token)
        {
            var pageIndex = 1;
            var serverQuestionnaires = new List<QuestionnaireListItem>();

            QuestionnaireListItem[] batchOfServerQuestionnaires;
            do
            {
                batchOfServerQuestionnaires = await this.GetPageOfQuestionnairesAsync(isPublic: isPublic, pageIndex: pageIndex++, token: token);
                serverQuestionnaires.AddRange(batchOfServerQuestionnaires);

            } while (batchOfServerQuestionnaires.Any());

            return serverQuestionnaires;
        }

        public async Task<Questionnaire> GetQuestionnaireAsync(QuestionnaireListItem selectedQuestionnaire, Action<decimal> downloadProgress, CancellationToken token)
        {
            Questionnaire downloadedQuestionnaire = null;

            downloadedQuestionnaire = await this.restService.GetWithProgressAsync<Questionnaire>(
                url: string.Format("questionnaires/{0}", selectedQuestionnaire.Id),
                credentials:
                    new RestCredentials
                    {
                        Login = this.userIdentity.Name,
                        Password = this.userIdentity.Password
                    },
                progressPercentage: downloadProgress, token: token);

            return downloadedQuestionnaire;
        }

        private async Task<QuestionnaireListItem[]> GetPageOfQuestionnairesAsync(bool isPublic, int pageIndex, CancellationToken token)
        {
            var  batchOfServerQuestionnaires= await this.restService.GetAsync<Core.SharedKernels.SurveySolutions.Api.Designer.QuestionnaireListItem[]>(
                url: "questionnaires",
                token: token,
                credentials: 
                    new RestCredentials
                    {
                        Login = this.userIdentity.Name,
                        Password = this.userIdentity.Password
                    },
                queryString: new { pageIndex = pageIndex, isPublic = isPublic });

            return batchOfServerQuestionnaires.Select(questionnaireListItem => new QuestionnaireListItem()
            {
                Id = questionnaireListItem.Id,
                Title = questionnaireListItem.Title,
                LastEntryDate = questionnaireListItem.LastEntryDate,
                IsPublic = isPublic,
                OwnerName = isPublic ? string.Empty : this.userIdentity.Name
            }).ToArray();
        }
    }
}