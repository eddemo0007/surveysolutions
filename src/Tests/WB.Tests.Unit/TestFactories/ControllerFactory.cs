﻿using System;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1;
using WB.Core.SharedKernels.SurveyManagement.Services;
using AttachmentsController = WB.Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController;

namespace WB.Tests.Unit.TestFactories
{
    internal class ControllerFactory
    {
        public AttachmentsController AttachmentsController(IAttachmentContentService attachmentContentService)
            => new AttachmentsController(attachmentContentService);

        public QuestionnairesApiV1Controller InterviewerQuestionnaires(
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            ISerializer serializer = null,
            QuestionnaireDocument questionnaire = null,
            QuestionnaireBrowseItem questionnaireBrowseItem = null)
            => new QuestionnairesApiV1Controller(
                questionnareAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                questionnaireBrowseViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
                serializer ?? Mock.Of<ISerializer>(),
                Mock.Of<IPlainQuestionnaireRepository>(_
                    => _.GetQuestionnaireDocument(It.IsAny<Guid>(), It.IsAny<long>()) == questionnaire),
                Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(_
                    => _.GetById(It.IsAny<object>()) == questionnaireBrowseItem))
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
    }
}
