﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireListViewDenormalizerTests
{
    internal class when_questionnaire_changing_event_recived : QuestionnaireListViewItemDenormalizerTestContext
    {
        Establish context = () =>
        {
            var eventsToPublish = new List<IEvent>
            {
                new NewGroupAdded(),
                new GroupCloned(),
                new QuestionnaireItemMoved(),
                new QuestionDeleted(),
                new GroupDeleted(),
                new GroupUpdated(),
                new GroupBecameARoster(Guid.NewGuid(), Guid.NewGuid()),
                new RosterChanged(Guid.NewGuid(), Guid.NewGuid())
                {
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    FixedRosterTitles = new FixedRosterTitle[0],
                    RosterTitleQuestionId = null
                },
                new GroupStoppedBeingARoster(Guid.NewGuid(), Guid.NewGuid()),
                new TextListQuestionCloned(),
                new TextListQuestionChanged(),
                new QRBarcodeQuestionUpdated(),
                new QRBarcodeQuestionCloned(),
                new MultimediaQuestionUpdated(),
                Create.Event.StaticTextAdded(),
                Create.Event.StaticTextUpdated(),
                Create.Event.StaticTextCloned(),
                new StaticTextDeleted()
            };

            eventAndResultView = eventsToPublish.ToDictionary(e => CreateCommittedEvent(e, questionnaireId, RandomDate()),
                e => (DateTime?)null);

            questionnaireListViewItemInMemoryWriter.Store(
                new QuestionnaireListViewItem(questionnaireId, "test", DateTime.Now, DateTime.Now, null, true), questionnaireId);

            questionnaireListViewItemDenormalizer = CreateQuestionnaireListViewItemDenormalizer(questionnaireListViewItemInMemoryWriter);
        };

        Because of = () =>
        {
            foreach (var eventToPublish in eventAndResultView.Keys.ToList())
            {
                var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(eventToPublish.Payload.GetType());
                var handleMethod = typeof(QuestionnaireListViewItemDenormalizer).GetMethod("Handle", new[] { publishedEventClosedType });

                var publishedEvent =
                    (PublishedEvent)
                        Activator.CreateInstance(publishedEventClosedType, eventToPublish);

                handleMethod.Invoke(questionnaireListViewItemDenormalizer, new object[] { publishedEvent });

                eventAndResultView[eventToPublish] = questionnaireListViewItemInMemoryWriter.GetById(questionnaireId).LastEntryDate;
            }
        };

        It should_update_LastEntryDate_each_time_when_event_piblished = () =>
            eventAndResultView.ShouldEachConformTo(eventWithView=>eventWithView.Key.EventTimeStamp==eventWithView.Value);

        private static DateTime RandomDate()
        {
            DateTime start = new DateTime(1984, 4, 18);
            Random gen = new Random();

            int range = (DateTime.Today - start).Days;
            return start.AddDays(gen.Next(range));
        }

        private static QuestionnaireListViewItemDenormalizer questionnaireListViewItemDenormalizer;

        private static TestInMemoryWriter<QuestionnaireListViewItem> questionnaireListViewItemInMemoryWriter =
            Stub.ReadSideRepository<QuestionnaireListViewItem>();

        private static Dictionary<CommittedEvent, DateTime?> eventAndResultView =
            new Dictionary<CommittedEvent, DateTime?>();

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
    }
}
