﻿using System;
using Raven.Abstractions.Replication;

namespace WB.Core.Infrastructure.Storage.Raven
{
    public class RavenConnectionSettings
    {
        public RavenConnectionSettings(string storagePath,
            string username = null, string password = null, string eventsDatabase = "Events", string viewsDatabase = "Views",
            string plainDatabase = "PlainStorage", string failoverBehavior = null, string activeBundles = null)
        {
            this.Username = username;
            this.Password = password;
            this.StoragePath = storagePath;
            this.EventsDatabase = eventsDatabase;
            this.ViewsDatabase = viewsDatabase;
            this.PlainDatabase = plainDatabase;

            FailoverBehavior failoverBehaviorValue;
            if (string.IsNullOrEmpty(failoverBehavior) || !Enum.TryParse(failoverBehavior, out failoverBehaviorValue))
                failoverBehaviorValue = FailoverBehavior.FailImmediately;

            this.FailoverBehavior = failoverBehaviorValue;
            this.ActiveBundles = activeBundles;
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
        public string StoragePath { get; private set; }
        public string EventsDatabase { get; private set; }
        public string ViewsDatabase { get; private set; }
        public string PlainDatabase { get; private set; }
        public FailoverBehavior FailoverBehavior { get; private set; }
        public string ActiveBundles { get; private set; }
    }
}