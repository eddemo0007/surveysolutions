﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using Ncqrs.Eventing.Storage;

using Raven.Abstractions.Data;
using Raven.Client.Document;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide
{
    internal class RavenReadSideService : IReadSideStatusService, IReadSideAdministrationService
    {
        private const int MaxAllowedFailedEvents = 100;
        private const string ViewsDatabaseName = "Views";

        private static readonly object RebuildAllViewsLockObject = new object();
        private static readonly object ErrorsLockObject = new object();

        private static bool areViewsBeingRebuiltNow = false;
        private static bool shouldStopViewsRebuilding = false;

        private static string statusMessage;
        private static List<Tuple<DateTime, string, Exception>> errors = new List<Tuple<DateTime,string,Exception>>();

        private readonly IStreamableEventStore eventStore;
        private readonly IViewConstructorEventBus eventBus;
        private readonly DocumentStore ravenStore;
        private readonly ILogger logger;
        private readonly IRavenReadSideRepositoryWriterRegistry writerRegistry;
        private readonly IReadSideRepositoryCleanerRegistry cleanerRegistry;

        static RavenReadSideService()
        {
            UpdateStatusMessage("No administration operations were performed so far.");
        }

        public RavenReadSideService(IStreamableEventStore eventStore, IViewConstructorEventBus eventBus, DocumentStore ravenStore, ILogger logger, IRavenReadSideRepositoryWriterRegistry writerRegistry,
        IReadSideRepositoryCleanerRegistry cleanerRegistry)
        {
            this.eventStore = eventStore;
            this.eventBus = eventBus;
            this.ravenStore = ravenStore;
            this.logger = logger;
            this.writerRegistry = writerRegistry;
            this.cleanerRegistry = cleanerRegistry;
        }

        #region IReadLayerStatusService implementation

        public bool AreViewsBeingRebuiltNow()
        {
            return areViewsBeingRebuiltNow;
        }

        #endregion // IReadLayerStatusService implementation

        #region IReadLayerAdministrationService implementation

        public string GetReadableStatus()
        {
            return string.Format("{1}{0}{0}Are views being rebuilt now: {2}{0}{0}{3}{0}{0}{4}",
                Environment.NewLine,
                statusMessage,
                areViewsBeingRebuiltNow ? "Yes" : "No",
                this.GetReadableListOfRepositoryWriters(),
                GetReadableErrors());
        }

        public void RebuildAllViewsAsync()
        {
            new Task(this.RebuildAllViews).Start();
        }

        public void RebuildViewsAsync(string[] handlerNames)
        {
            new Task(() => this.RebuildViews(handlerNames)).Start();
        }

        public void StopAllViewsRebuilding()
        {
            if (!areViewsBeingRebuiltNow)
                return;

            shouldStopViewsRebuilding = true;
        }

        public IEnumerable<EventHandlerDescription> GetAllAvailableHandlers()
        {
            return
                this.eventBus.GetAllRegistredEventHandlers()
                    .Select(
                        h =>
                        new EventHandlerDescription(h.Name, h.UsesViews.Select(u => u.Name).ToArray(),
                                                    h.BuildsViews.Select(b => b.Name).ToArray()))
                    .ToList();
        }

        #endregion // IReadLayerAdministrationService implementation

        private void RebuildViews(string[] handlerNames)
        {
            if (!areViewsBeingRebuiltNow)
            {
                lock (RebuildAllViewsLockObject)
                {
                    if (!areViewsBeingRebuiltNow)
                    {
                        this.RebuildViewsImpl(handlerNames);
                    }
                }
            }
        }

        private void RebuildAllViews()
        {
            if (!areViewsBeingRebuiltNow)
            {
                lock (RebuildAllViewsLockObject)
                {
                    if (!areViewsBeingRebuiltNow)
                    {
                        this.RebuildAllViewsImpl();
                    }
                }
            }
        }

        private void RebuildViewsImpl(string[] handlerNames)
        {
            try
            {
                areViewsBeingRebuiltNow = true;

                var handlers = GetListOfEventHandlersForRebuild(handlerNames);
                
                var viewTypes = GetAllViewsBuildByHandlers(handlers);

                this.DeleteViews(viewTypes);

                var writers = GetListOfWritersForEnableCache(viewTypes);

                string republishDetails = "<<NO DETAILS>>";

                try
                {
                    this.EnableCacheInRepositoryWriters(writers);

                    republishDetails = this.RepublishAllEvents(handlers);
                }
                finally
                {
                    this.DisableCacheInRepositoryWriters(writers);

                    UpdateStatusMessage("Rebuild specific views succeeded." + Environment.NewLine + republishDetails);
                }
            }
            catch (Exception exception)
            {
                this.SaveErrorForStatusReport("Unexpected error occurred", exception);
                UpdateStatusMessage(string.Format("Unexpectedly failed. Last status message:{0}{1}",
                    Environment.NewLine, statusMessage));
                throw;
            }
            finally
            {
                areViewsBeingRebuiltNow = false;
            }
        }

        private Type[] GetAllViewsBuildByHandlers(IEventHandler[] handlers)
        {
            return handlers
                    .SelectMany(h => h.BuildsViews)
                    .Distinct()
                    .ToArray();
        }

        private IEventHandler[] GetListOfEventHandlersForRebuild(string[] handlerNames)
        {
            var allHandlers = this.eventBus.GetAllRegistredEventHandlers();
            var result = new List<IEventHandler>();
            foreach (var eventHandler in allHandlers)
            {
                if(handlerNames.Contains(eventHandler.Name))
                    result.Add(eventHandler);
            }
            return result.ToArray();
        }

        private IEnumerable<IRavenReadSideRepositoryWriter> GetListOfWritersForEnableCache(Type[] viewTypes)
        {
            return this.writerRegistry.GetAll().Where(w => viewTypes.Contains(w.ViewType)).ToArray();
        }

        private void RebuildAllViewsImpl()
        {
            try
            {
                areViewsBeingRebuiltNow = true;

                this.DeleteAllViews();
                this.CleanUpAllWriters();

                string republishDetails = "<<NO DETAILS>>";

                try
                {
                    this.EnableCacheInAllRepositoryWriters();

                    republishDetails = this.RepublishAllEvents(eventBus.GetAllRegistredEventHandlers());
                }
                finally
                {
                    this.DisableCacheInAllRepositoryWriters();

                    UpdateStatusMessage("Rebuild all views succeeded." + Environment.NewLine + republishDetails);
                }
            }
            catch (Exception exception)
            {
                this.SaveErrorForStatusReport("Unexpected error occurred", exception);
                UpdateStatusMessage(string.Format("Unexpectedly failed. Last status message:{0}{1}",
                    Environment.NewLine, statusMessage));
                throw;
            }
            finally
            {
                areViewsBeingRebuiltNow = false;
            }
        }

        private void CleanUpAllWriters()
        {
            foreach (var cleaner in this.cleanerRegistry.GetAll())
            {
                cleaner.Clear();
            }
        }

        private void DeleteViews(Type[] viewTypes)
        {
            foreach (var viewType in viewTypes)
            {
                ThrowIfShouldStopViewsRebuilding();
                UpdateStatusMessage(string.Format("Deleting {0}", viewType.Name));
                var query = string.Format("Tag: *{0}*", viewType.Name);

                this.ravenStore
                    .DatabaseCommands
                    .ForDatabase("Views")
                    .DeleteByIndex("Raven/DocumentsByEntityName", new IndexQuery
                        {
                            Query = query
                        });
                    UpdateStatusMessage(string.Format("{0} view was deleted.", viewType.Name));


                int resultViewCount = this.ravenStore
                                          .DatabaseCommands
                                          .ForDatabase("Views").Query("Raven/DocumentsByEntityName", new IndexQuery
                                              {
                                                  Query = query
                                              }, new string[0]).Results.Count;

                if (resultViewCount > 0)
                    throw new Exception(string.Format(
                        "Failed to delete all views. Remaining {0} count: {1}.", resultViewCount, viewType.Name));
            }
        }

        private void DeleteAllViews()
        {
            ThrowIfShouldStopViewsRebuilding();

            UpdateStatusMessage(string.Format("Delete database {0}", ViewsDatabaseName));

            this.ravenStore.DeleteDatabase(ViewsDatabaseName, true);

            ThrowIfShouldStopViewsRebuilding();

            UpdateStatusMessage(string.Format("Create database {0}", ViewsDatabaseName));

            this.ravenStore.CreateDatabase(ViewsDatabaseName);
        }

        private void EnableCacheInRepositoryWriters(IEnumerable<IRavenReadSideRepositoryWriter> writers)
        {
            UpdateStatusMessage("Enabling cache in repository writers.");

            foreach (IRavenReadSideRepositoryWriter writer in writers)
            {
                writer.EnableCache();
            }

            UpdateStatusMessage("Cache in repository writers enabled.");
        }

        private void EnableCacheInAllRepositoryWriters()
        {
            EnableCacheInRepositoryWriters(this.writerRegistry.GetAll());
        }

        private void DisableCacheInAllRepositoryWriters()
        {
            DisableCacheInRepositoryWriters(this.writerRegistry.GetAll());
        }

        private void DisableCacheInRepositoryWriters(IEnumerable<IRavenReadSideRepositoryWriter> writers)
        {
            UpdateStatusMessage("Disabling cache in repository writers.");

            foreach (IRavenReadSideRepositoryWriter writer in writers)
            {
                UpdateStatusMessage(string.Format(
                    "Disabling cache in repository writer for entity {0}.",
                    GetRepositoryEntityName(writer)));

                try
                {
                    writer.DisableCache();
                }
                catch (Exception exception)
                {
                    this.SaveErrorForStatusReport(
                        string.Format("Failed to disable cache and store data to repository for writer {0}.",
                            writer.GetType()),
                        exception);
                }
            }

            UpdateStatusMessage("Cache in repository writers disabled.");
        }

        private string RepublishAllEvents(IEnumerable<IEventHandler> handlers)
        {
            int processedEventsCount = 0;
            int failedEventsCount = 0;

            ThrowIfShouldStopViewsRebuilding();

            logger.Info("Starting rebuild Read Layer");

            UpdateStatusMessage("Determining count of events to be republished.");

            int allEventsCount = this.eventStore.CountOfAllEvents();

            DateTime republishStarted = DateTime.Now;
            UpdateStatusMessage(
                "Acquiring first portion of events. "
                + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount));

            foreach (CommittedEvent[] eventBulk in this.eventStore.GetAllEvents())
            {
                foreach (CommittedEvent @event in eventBulk)
                {
                    ThrowIfShouldStopViewsRebuilding();

                    UpdateStatusMessage(
                        string.Format("Publishing event {0}. ", processedEventsCount + 1)
                        + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount));

                    try
                    {
                        this.eventBus.PublishEventsToHandlers(@event, handlers);
                    }
                    catch (Exception exception)
                    {
                        this.SaveErrorForStatusReport(
                            string.Format("Failed to publish event {0} of {1} ({2})",
                                processedEventsCount + 1, allEventsCount, @event.EventIdentifier),
                            exception);

                        failedEventsCount++;
                    }

                    processedEventsCount++;

                    if (failedEventsCount >= MaxAllowedFailedEvents)
                        throw new Exception(string.Format("Failed to rebuild read layer. Too many events failed: {0}.", failedEventsCount));
                }

                UpdateStatusMessage(
                    "Acquiring next portion of events. "
                    + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount));
            }

            logger.Info(String.Format("Processed {0} events, failed {1}", processedEventsCount, failedEventsCount));

            UpdateStatusMessage(string.Format("All events were republished. "
                + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount)));

            return GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount);
        }

        private static string GetReadablePublishingDetails(DateTime republishStarted,
            int processedEventsCount, int allEventsCount, int failedEventsCount)
        {
            TimeSpan republishTimeSpent = DateTime.Now - republishStarted;

            int speedInEventsPerMinute = (int)(
                republishTimeSpent.TotalSeconds == 0
                ? 0
                : 60 * processedEventsCount / republishTimeSpent.TotalSeconds);

            TimeSpan estimatedTotalRepublishTime = TimeSpan.FromMilliseconds(
                processedEventsCount == 0
                ? 0
                : republishTimeSpent.TotalMilliseconds / processedEventsCount * allEventsCount);

            return string.Format(
                "Processed events: {1}. Total events: {2}. Failed events: {3}.{0}Time spent republishing: {4}. Speed: {5} events per minute. Estimated time: {6}.",
                Environment.NewLine,
                processedEventsCount, allEventsCount, failedEventsCount,
                republishTimeSpent.ToString(@"hh\:mm\:ss"), speedInEventsPerMinute, estimatedTotalRepublishTime.ToString(@"hh\:mm\:ss"));
        }

        private static void ThrowIfShouldStopViewsRebuilding()
        {
            if (shouldStopViewsRebuilding)
            {
                shouldStopViewsRebuilding = false;
                throw new Exception("Views rebuilding stopped by request.");
            }
        }

        private static void UpdateStatusMessage(string newMessage)
        {
            statusMessage = string.Format("{0}: {1}", DateTime.Now, newMessage);
        }

        private string GetReadableListOfRepositoryWriters()
        {
            List<IRavenReadSideRepositoryWriter> writers = this.writerRegistry.GetAll().ToList();

            bool areThereNoWriters = writers.Count == 0;
            #warning to Tolik: calls to dictionary (writer cache) from other thread rais exceptions because Dictionary is not thread safe
            return areThereNoWriters
                ? "Registered writers: None"
                : string.Format(
                    "Registered writers: {1}{0}{2}",
                    Environment.NewLine,
                    writers.Count,
                    string.Join(
                        Environment.NewLine,
                        writers
                            .Select(writer => string.Format("{0,-40} ({1})", GetRepositoryEntityName(writer), writer.GetReadableStatus()))
                            .OrderBy(_ => _)
                            .ToArray()));
        }

        private static string GetRepositoryEntityName(IRavenReadSideRepositoryWriter writer)
        {
            return writer.GetType().GetGenericArguments().Single().Name;
        }

        #region Error reporting methods

        private void SaveErrorForStatusReport(string message, Exception exception)
        {
            lock (ErrorsLockObject)
            {
                errors.Add(Tuple.Create(DateTime.Now, message, exception));
            }

            this.logger.Error(message, exception);
        }

        private static string GetReadableErrors()
        {
            lock (ErrorsLockObject)
            {
                bool areThereNoErrors = errors.Count == 0;

                return areThereNoErrors
                    ? "Errors: None"
                    : string.Format(
                        "Errors: {1}{0}{0}{2}",
                        Environment.NewLine,
                        errors.Count,
                        string.Join(
                            Environment.NewLine + Environment.NewLine,
                            ReverseList(errors)
                                .Select((error, index) => GetReadableError(error, shouldShowStackTrace: index < 10))
                                .ToArray()));
            }
        }

        private static IEnumerable<T> ReverseList<T>(List<T> list)
        {
            for (int indexOfElement = list.Count - 1; indexOfElement >= 0; indexOfElement--)
                yield return list[indexOfElement];
        }

        private static string GetReadableError(Tuple<DateTime, string, Exception> error, bool shouldShowStackTrace)
        {
            return string.Format("{1}: {2}{0}{3}", Environment.NewLine,
                error.Item1,
                error.Item2,
                shouldShowStackTrace ? error.Item3.ToString() : error.Item3.Message);
        }

        #endregion // Error reporting methods
    }
}