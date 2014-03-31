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
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answer_on_multy_option_question_increases_roster_size : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");

            multyOptionRosterSizeId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                                                        => _.HasQuestion(multyOptionRosterSizeId) == true
                                                        && _.GetQuestionType(multyOptionRosterSizeId) == QuestionType.MultyOption
                                                        && _.GetRosterGroupsByRosterSizeQuestion(multyOptionRosterSizeId) == new Guid[] { rosterGroupId }
                                                        && _.GetAnswerOptionsAsValues(multyOptionRosterSizeId) == new decimal[] { 1,2,3}

                                                        && _.HasGroup(rosterGroupId) == true
                                                        && _.GetRosterLevelForGroup(rosterGroupId) == 1
                                                        && _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(rosterGroupId) == new Guid[] { rosterGroupId }
                                                        && _.GetRostersFromTopToSpecifiedGroup(rosterGroupId) == new Guid[] { rosterGroupId });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.SynchronizeInterview(userId,
                new InterviewSynchronizationDto(interview.EventSourceId, InterviewStatus.RejectedBySupervisor, userId, questionnaireId,
                    questionnaire.Version,
                    new[]
                    { new AnsweredQuestionSynchronizationDto(multyOptionRosterSizeId, new decimal[0], new decimal[] { 1 }, string.Empty) },
                    new HashSet<InterviewItemId>(),
                    new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), null,
                    new Dictionary<InterviewItemId, RosterSynchronizationDto[]>
                    {
                        {
                            new InterviewItemId(rosterGroupId, new decimal[0]), new[]
                            {
                                new RosterSynchronizationDto(rosterGroupId, new decimal[0], 1.0m, null, string.Empty),
                            }
                        }
                    }, true));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[] { }, DateTime.Now, new decimal[]{1,2});

        It should_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 2));

        It should_not_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 1));

        It should_not_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 1));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid multyOptionRosterSizeId;
        private static Guid rosterGroupId;
    }
}
