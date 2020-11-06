﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Events;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorCalendarEventsHandler : IHandleCommunicationMessage
    {
        private readonly ICalendarEventStorage calendarEventStorage;
        private readonly IEnumeratorEventStorage eventStore;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly ILiteEventBus eventBus;
        private readonly IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog;


        public SupervisorCalendarEventsHandler(ICalendarEventStorage calendarEventStorage,
            IEnumeratorEventStorage eventStore,
            ICommandService commandService,
            ILogger logger,
            IJsonAllTypesSerializer serializer,
            ILiteEventBus eventBus,
            IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog)
        {
            this.calendarEventStorage = calendarEventStorage;
            this.eventStore = eventStore;
            this.commandService = commandService;
            this.logger = logger;
            this.serializer = serializer;
            this.eventBus = eventBus;
            this.receivedPackagesLog = receivedPackagesLog;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<PostCalendarEventsRequest, OkResponse>(Handle);
            requestHandler.RegisterHandler<GetCalendarEventsRequest, GetCalendarEventsResponse>(GetCalendarEvents);
            requestHandler.RegisterHandler<GetCalendarEventDetailsRequest, GetCalendarEventDetailsResponse>(Handle);
            requestHandler.RegisterHandler<UploadCalendarEventRequest, OkResponse>(UploadCalendarEvent);
        }

        private Task<GetCalendarEventDetailsResponse> Handle(GetCalendarEventDetailsRequest arg)
        {
            var events = this.eventStore.Read(arg.CalendarEventId, 0).ToList();

            return Task.FromResult(new GetCalendarEventDetailsResponse
            {
                Events = events
            });
        }

        private Task<OkResponse> Handle(PostCalendarEventsRequest arg)
        {
            eventBus.PublishCommittedEvents(arg.Events);
            eventStore.StoreEvents(new CommittedEventStream(arg.CalendarEventId, arg.Events));

            return Task.FromResult(new OkResponse());
        }

        private Task<GetCalendarEventsResponse> GetCalendarEvents(GetCalendarEventsRequest arg)
        {
            var calendarEvents = calendarEventStorage.GetNotSynchedCalendarEvents(arg.UserId);

            List<CalendarEventApiView> response = calendarEvents.Select(x => new CalendarEventApiView
            {
                CalendarEventId = x.Id,
                Sequence = eventStore.GetMaxSequenceForAnyEvent(x.Id, EventsThatAssignInterviewToResponsibleProvider.GetTypeNames()),
                ResponsibleId = x.UserId,
                LastEventId = x.LastEventId,
                //IsMarkedAsReceivedByInterviewer = x.ReceivedByInterviewerAtUtc != null,
            }).ToList();

            return Task.FromResult(new GetCalendarEventsResponse()
            {
                CalendarEvents = response
            });
        }
        
        private Task<OkResponse> UploadCalendarEvent(UploadCalendarEventRequest request)
        {
            var calendarEvent = request.CalendarEvent;

            this.logger.Info($"Uploading of calendar event {calendarEvent.CalendarEventId} started.");

            var innerwatch = Stopwatch.StartNew();

            try
            {
                var calendarEventStream = this.serializer.Deserialize<CommittedEvent[]>(calendarEvent.Events);

                var firstEvent = calendarEventStream.FirstOrDefault();

                if (firstEvent != null && eventStore.HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(firstEvent.EventSequence - 1,
                    calendarEvent.CalendarEventId, 
                    EventsThatChangeAnswersStateProvider.GetTypeNames()))
                {
                    throw new ArgumentException("Provided calendar event package is outdated. New events were given to the calendar event while interviewer had calendar event on a tablet");
                }

                var isPackageDuplicated = IsPackageDuplicated(calendarEventStream);
                if (isPackageDuplicated)
                    return Task.FromResult(new OkResponse()); // ignore

                this.logger.Debug($"Calendar events by {calendarEvent.CalendarEventId} deserialized. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();

                var committedEventStream = new CommittedEventStream(calendarEvent.CalendarEventId, calendarEventStream);
                eventStore.StoreEvents(committedEventStream);
                eventBus.PublishCommittedEvents(calendarEventStream);
  
                RecordProcessedPackageInfo(calendarEventStream);
            }
            catch (Exception exception)
            {
                this.logger.Error($"Calendar event by {calendarEvent.CalendarEventId} processing failed. Reason: '{exception.Message}'", exception);
                innerwatch.Restart();
                throw;
            }

            innerwatch.Stop();

            return Task.FromResult(new OkResponse());
        }
        
        private bool IsPackageDuplicated(CommittedEvent[] events)
        {
            if (events.Length > 0)
            {
                var firstEvent = events[0];
                var lastEvent = events[^1];

                var existingReceivedPackageLog = this.receivedPackagesLog.Where(
                    x => x.FirstEventId == firstEvent.EventIdentifier &&
                         x.FirstEventTimestamp == firstEvent.EventTimeStamp &&
                         x.LastEventId == lastEvent.EventIdentifier &&
                         x.LastEventTimestamp == lastEvent.EventTimeStamp).Count;

                if (existingReceivedPackageLog > 0)
                {
                    return true; // Package already received and processed
                }
            }

            return false;
        }

        private void RecordProcessedPackageInfo(CommittedEvent[] events)
        {
            if (events.Length > 0)
            {
                var firstEvent = events[0];
                var lastEvent = events[^1];
                this.receivedPackagesLog.Store(new SuperivsorReceivedPackageLogEntry
                {
                    FirstEventId = firstEvent.EventIdentifier,
                    FirstEventTimestamp = firstEvent.EventTimeStamp,
                    LastEventId = lastEvent.EventIdentifier,
                    LastEventTimestamp = lastEvent.EventTimeStamp
                });
            }
        }
    }
}