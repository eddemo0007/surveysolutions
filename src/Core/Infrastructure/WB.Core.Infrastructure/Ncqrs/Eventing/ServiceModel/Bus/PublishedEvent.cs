﻿using System;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    /// <summary>
    /// An interface that represents an event during its publishing and handling. At this stage event objects are genericaly typed
    /// to the actual payload type.
    /// </summary>
    /// <remarks>
    /// This interface is internal and is not mean to be implemented in user code. It is necessary because "out" type parameters can
    /// only be declared by interfaces (not classes). <see cref="IEventHandler{TEvent}"/> needs to declare "in" type parameter so
    /// <see cref="IPublishedEvent{TEvent}"/> have to have "out" modifier.
    /// </remarks>
    /// <typeparam name="TEvent">Type of the payload.</typeparam>
    public interface IPublishedEvent<out TEvent> : IPublishableEvent
    {        
        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        new TEvent Payload { get;}
    }

    /// <summary>
    /// Provides default <see cref="IPublishedEvent{TEvent}"/> interface implementation.
    /// </summary>
    /// <typeparam name="TEvent">Type of the event.</typeparam>
    public class PublishedEvent<TEvent> : PublishedEvent, IPublishedEvent<TEvent>
    {
        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        public new TEvent Payload
        {
            get { return (TEvent)base.Payload; }
        }

        public PublishedEvent(IPublishableEvent evnt) : base(evnt)
        {
        }
    }

    /// <summary>
    /// Base clas for representing published events. Can be used when type of the event payload does not matter.
    /// </summary>
    public abstract class PublishedEvent : IPublishableEvent
    {
        private readonly object _payload;
        private readonly int _eventSequence;
        private readonly Guid _eventIdentifier;
        private readonly DateTime _eventTimeStamp;
        private readonly Guid _eventSourceId;
        private readonly Guid _commitId;
        private readonly string _origin;
        private readonly long globalSequence;

        /// <summary>
        /// Id of the commit this event belongs to (usually corresponds to command id).
        /// </summary>
        public Guid CommitId
        {
            get { return _commitId; }
        }

        public string Origin
        {
            get { return _origin; }
        }

        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        public object Payload
        {
            get { return _payload; }
        }

        /// <summary>
        /// Gets the unique identifier for this event.
        /// </summary>
        public Guid EventIdentifier
        {
            get { return _eventIdentifier; }
        }

        /// <summary>
        /// Gets the time stamp for this event.
        /// </summary>
        /// <value>a <see cref="DateTime"/> UTC value that represents the point
        /// in time where this event occurred.</value>
        public DateTime EventTimeStamp
        {
            get { return _eventTimeStamp; }
        }

        /// <summary>
        /// Gets the id of the event source that caused the event.
        /// </summary>
        /// <value>The id of the event source that caused the event.</value>
        public Guid EventSourceId
        {
            get { return _eventSourceId; }
        }

        /// <summary>
        /// Gets the event sequence number.
        /// </summary>
        /// <remarks>
        /// An sequence of events always starts with <c>1</c>. So the first event in a sequence has the <see cref="EventSequence"/> value of <c>1</c>.
        /// </remarks>
        /// <value>A number that represents the order of where this events occurred in the sequence.</value>
        public int EventSequence
        {
            get { return _eventSequence; }
        }

        public long GlobalSequence
        {
            get { return globalSequence; }
        }

        protected PublishedEvent(IPublishableEvent evnt)            
        {            
            _payload = evnt.Payload;           
            _eventSourceId = evnt.EventSourceId;
            _eventSequence = evnt.EventSequence;
            _eventIdentifier = evnt.EventIdentifier;
            _eventTimeStamp = evnt.EventTimeStamp;
            _commitId = evnt.CommitId;
            _origin = evnt.Origin;
            globalSequence = evnt.GlobalSequence;
        }
    }
}