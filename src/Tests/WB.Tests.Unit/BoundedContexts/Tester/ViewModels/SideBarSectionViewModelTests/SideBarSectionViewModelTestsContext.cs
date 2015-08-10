﻿using Cirrious.MvvmCross.Plugins.Messenger;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.SideBarSectionViewModelTests
{
    public class SideBarSectionViewModelTestsContext
    {
        protected static SideBarSectionViewModel CreateViewModel(QuestionnaireModel questionnaire = null,
            IStatefulInterview interview = null)
        {
            Mock<IStatefulInterviewRepository> interviewRepository = new Mock<IStatefulInterviewRepository>();
            interviewRepository.SetReturnsDefault(interview);
            Mock<IPlainKeyValueStorage<QuestionnaireModel>> questionnaireRepository = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            questionnaireRepository.SetReturnsDefault(questionnaire);

            return new SideBarSectionViewModel(interviewRepository.Object, 
                questionnaireRepository.Object, 
                Create.SubstitutionService(), 
                Create.LiteEventRegistry(), 
                Stub.MvxMainThreadDispatcher(), 
                Stub.SideBarSectionViewModelsFactory(),
                Mock.Of<IMvxMessenger>());
        }
    }
}