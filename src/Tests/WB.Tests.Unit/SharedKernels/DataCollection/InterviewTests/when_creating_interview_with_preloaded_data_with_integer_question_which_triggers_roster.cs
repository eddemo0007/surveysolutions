﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_preloaded_data_with_integer_question_which_triggers_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCD");
            prefilledQuestionAnswer = 2;
            preloadedDataDto = new PreloadedDataDto(
                new[]
                {
                    new PreloadedLevelDto(new decimal[0], new Dictionary<Guid, object> { { prefilledQuestionId, prefilledQuestionAnswer } }),
                });
            answersTime = new DateTime(2013, 09, 01);

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.GetQuestionType(prefilledQuestionId) == QuestionType.Numeric
                   && _.HasQuestion(prefilledQuestionId) == true
                   && _.IsQuestionInteger(prefilledQuestionId) == true
                   && _.GetRosterGroupsByRosterSizeQuestion(prefilledQuestionId) == new Guid[] { rosterGroupId }

                   && _.HasGroup(rosterGroupId) == true
                   && _.GetRosterLevelForGroup(rosterGroupId) == 1
                   && _.GetRostersFromTopToSpecifiedGroup(rosterGroupId) == new Guid[] {rosterGroupId});

            var questionnaireRepository = Mock.Of<IPlainQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, 1) == questionaire);

            eventContext = new EventContext();

            interview = Create.Interview(questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            interview.CreateInterviewWithPreloadedData(questionnaireId, 1, preloadedDataDto, supervisorId, answersTime, userId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_InterviewFromPreloadedDataCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewFromPreloadedDataCreated>();

        It should_raise_NumericIntegerQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>(@event
                => @event.Answer == prefilledQuestionAnswer && @event.QuestionId == prefilledQuestionId);

        It should_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.All(i => i.GroupId == rosterGroupId));


        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid prefilledQuestionId;
        private static Guid rosterGroupId;
        private static int prefilledQuestionAnswer;
        private static Interview interview;
    }
}
