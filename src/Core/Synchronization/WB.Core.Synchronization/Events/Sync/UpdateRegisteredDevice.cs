﻿using System;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.Synchronization
{
    /// <summary>
    /// The new device registered
    /// </summary>
    [Serializable]
    public class UpdateRegisteredDevice : ILiteEvent
    {
        #region PublicProperties

        /// <summary>
        /// Gets or sets TabletId.
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Gets or sets Registrator.
        /// </summary>
        public Guid Registrator { get; set; }

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public byte[] SecretKey { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        #endregion
    }
}