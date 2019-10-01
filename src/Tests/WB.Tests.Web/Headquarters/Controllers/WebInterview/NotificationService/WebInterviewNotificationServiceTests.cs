﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;
using WB.Tests.Web;
using static WB.Enumerator.Native.WebInterview.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.NotificationService
{
    public class WebInterviewNotificationServiceTests
    {
        private StatefulInterview interview;
        private Mock<IWebInterviewInvoker> hubMock;
        private string prefilledSectionId;
        private WebInterviewNotificationService NotificationService { get; set; }
        private readonly Identity subGroup = Id.IdentityA;
        private readonly Identity textQuestion = Id.Identity1;
        private readonly Identity prefilledTextQuestion = Id.Identity2;

        [SetUp]
        public void Setup()
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.Group(subGroup.Id, children: new IComposite[] {
                    Create.Entity.TextQuestion(textQuestion.Id),
                    Create.Entity.TextQuestion(prefilledTextQuestion.Id, variable:"text2",
                    preFilled: true)})
            });

            this.interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaireDocument);

            this.hubMock = new Mock<IWebInterviewInvoker>();
            this.prefilledSectionId = GetConnectedClientPrefilledSectionKey(interview.Id);

            var repo = Mock.Of<IStatefulInterviewRepository>(s => s.Get(interview.Id.FormatGuid()) == interview);
            var questionnaireStorage = Create.Storage.QuestionnaireStorage(questionnaireDocument);

            this.NotificationService = new WebInterviewNotificationService(repo, questionnaireStorage, hubMock.Object);
        }

        public class WebInterviewHubMock<TClientContract> where TClientContract : class
        {
            private Mock<TClientContract> coverGroup;
            public Mock<TClientContract> GetCoverGroup(Guid interviewId) =>
                coverGroup ?? (coverGroup = new Mock<TClientContract>());
        }

        [Test]
        public void should_notify_on_prefilled_question_at_cover_section()
        {
            this.NotificationService.RefreshEntities(interview.Id, textQuestion, prefilledTextQuestion);

            this.hubMock.Verify(v => v.RefreshEntities(prefilledSectionId, new[] { prefilledTextQuestion.ToString() }), Times.Once);
        }

        [Test]
        public void should_notify_on_prefilled_question_sections_with_affected_question()
        {
            this.NotificationService.RefreshEntities(interview.Id, textQuestion, prefilledTextQuestion);

            var sectionKey = GetConnectedClientSectionKey(subGroup, interview.Id);

            this.hubMock.Verify(g => g.RefreshEntities(
                sectionKey,
                new[] { textQuestion.ToString(), prefilledTextQuestion.ToString() }
            ), Times.Once);
        }

        [Test]
        public void should_refresh_client_section_state()
        {
            this.NotificationService.RefreshEntities(interview.Id, textQuestion, prefilledTextQuestion);

            this.hubMock.Verify(g => g.RefreshSectionState(interview.Id), Times.Once);
        }

        [Test]
        public void should_notify_parent_group_if_it_has_flat_roster()
        {
            var sectionId = Id.g1;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: sectionId, new IComposite[]
            {
                Create.Entity.Roster(Id.g2,
                    displayMode: RosterDisplayMode.Flat,
                    children: new List<IComposite>
                    {
                        Create.Entity.TextQuestion(Id.gA)
                    })
            });

            var localInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire, shouldBeInitialized: true);
            var localHubMock = new Mock<IWebInterviewInvoker>();

            var service = Create.Service.WebInterviewNotificationService(Create.Storage.InterviewRepository(localInterview), 
                Create.Storage.QuestionnaireStorage(questionnaire), localHubMock.Object);

            // act
            var question = Create.Identity(Id.gA, 1);
            service.RefreshEntities(localInterview.Id, question);

            // Assert
            var identity = Create.Identity(sectionId);
            var sectionKey = GetConnectedClientSectionKey(identity, localInterview.Id);

            localHubMock.Verify(g => g.RefreshEntities(
                sectionKey,
                It.Is<string[]>(m => m.Contains(question.ToString()))
            ), Times.Once);
        }
    }
}
