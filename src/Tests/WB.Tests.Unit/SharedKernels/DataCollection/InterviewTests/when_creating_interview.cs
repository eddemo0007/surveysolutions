﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestFixture]
    internal class when_creating_interview : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            eventContext = new EventContext();

            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            responsibleSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA00");
            questionnaireVersion = 18;
            answersToFeaturedQuestions = new Dictionary<Guid, AbstractAnswer>();
            
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.Version == questionnaireVersion);

            command = Create.Command.CreateInterviewCommand(questionnaireId: questionnaireId,
                questionnaireVersion: questionnaireVersion,
                responsibleSupervisorId: responsibleSupervisorId,
                answersToFeaturedQuestions: answersToFeaturedQuestions, 
                userId: userId);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);

            //Act
            interview.CreateInterview(command);
        }

        [Test]
        public void should_raise_InterviewCreated_event() =>
            eventContext.ShouldContainEvent<InterviewCreated>();

        [Test]
        public void should_provide_questionnaire_id_in_InterviewCreated_event() =>
            eventContext.GetEvent<InterviewCreated>()
                .QuestionnaireId.Should().Be(questionnaireId);

        [Test]
        public void should_provide_questionnaire_verstion_in_InterviewCreated_event() =>
            eventContext.GetEvent<InterviewCreated>()
                .QuestionnaireVersion.Should().Be(questionnaireVersion);

        [OneTimeTearDown]
        public void TearDown()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private EventContext eventContext;
        private Guid questionnaireId;
        private long questionnaireVersion;
        private Guid userId;
        private Guid responsibleSupervisorId;
        private Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions;
        private Interview interview;
        private CreateInterviewCommand command;
    }
}