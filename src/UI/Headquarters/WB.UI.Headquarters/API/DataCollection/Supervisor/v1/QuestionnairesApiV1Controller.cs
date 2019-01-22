﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(UserRoles.Supervisor)]
    public class QuestionnairesApiV1Controller : QuestionnairesControllerBase
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository;

        public QuestionnairesApiV1Controller(
            IQuestionnaireAssemblyAccessor questionnareAssemblyFileAccessor,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            ISerializer serializer,
            IQuestionnaireStorage questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository) : base(
            questionnaireStorage: questionnaireStorage,
            questionnaireRepository: questionnaireRepository,
            questionnareAssemblyFileAccessor: questionnareAssemblyFileAccessor,
            questionnaireBrowseViewFactory: questionnaireBrowseViewFactory,
            serializer: serializer)
        {
            this.questionnaireRepository = questionnaireRepository;
        }

        [HttpGet]
        public override HttpResponseMessage List() => base.List();
        [HttpGet]
        public override HttpResponseMessage Get(Guid id, int version, long contentVersion) => base.Get(id, version, contentVersion);
        [HttpGet]
        public override HttpResponseMessage GetAssembly(Guid id, int version) => base.GetAssembly(id, version);
        [HttpPost]
        public override HttpResponseMessage LogQuestionnaireAsSuccessfullyHandled(Guid id, int version) => base.LogQuestionnaireAsSuccessfullyHandled(id, version);
        [HttpPost]
        public override HttpResponseMessage LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version) => base.LogQuestionnaireAssemblyAsSuccessfullyHandled(id, version);
        [HttpGet]
        public override HttpResponseMessage GetAttachments(Guid id, int version) => base.GetAttachments(id, version);

        [HttpGet]
        public List<string> GetDeletedQuestionnaireList()
        {
            var list = questionnaireRepository.Query(_ => _.Where(q => q.IsDeleted == true).ToList())
                .Select(l => l.Id).ToList();
            return list;
        }
    }
}
