﻿using System;
using System.Threading.Tasks;
using MvvmCross.Logging;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.ViewModels
{
    public enum ConnectionStatus
    {
        WaitingForGoogleApi,
        StartDiscovering,
        StartAdvertising,
        Discovering,
        Connecting,
        Sync,
        Done,
        Connected,
        Advertising
    }

    public abstract class BaseOfflineSyncViewModel : BaseViewModel
    {
        private readonly IPermissionsService permissions;
        private readonly INearbyConnection nearbyConnection;

        protected BaseOfflineSyncViewModel(
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IPermissionsService permissions,
            INearbyConnection nearbyConnection
        )
            : base(principal, viewModelNavigationService)
        {
            this.permissions = permissions;
            this.nearbyConnection = nearbyConnection;
            SetStatus(ConnectionStatus.WaitingForGoogleApi);
            this.nearbyConnection.Events.Subscribe(HandleConnectionEvents);
        }

        protected void HandleConnectionEvents(INearbyEvent @event)
        {
            switch (@event)
            {
                case NearbyEvent.InitiatedConnection iniConnection:
                    this.nearbyConnection.AcceptConnection(iniConnection.Endpoint);
                    break;
                case NearbyEvent.Connected connected:
                    SetStatus(ConnectionStatus.Connected, "Connected to " + connected.Endpoint);
                    Connected(connected.Endpoint);
                    break;
                case NearbyEvent.Disconnected disconnected:
                    SetStatus(ConnectionStatus.Discovering, "Disconnected from " + disconnected.Endpoint);
                    break;
                case NearbyEvent.EndpointFound endpointFound:
                    this.OnFound(endpointFound.Endpoint, endpointFound.EndpointInfo);
                    break;
                case NearbyEvent.EndpointLost endpointLost:
                    break;
            }
        }

        protected void SetStatus(ConnectionStatus connectionStatus, string details = null)
        {
            this.Status = connectionStatus.ToString();
            this.StatusDetails = details ?? String.Empty;
        }

        protected virtual void Connected(string connectedEndpoint)
        {

        }
    

        protected virtual async void OnFound(string endpoint, NearbyDiscoveredEndpointInfo info)
        {
            SetStatus(ConnectionStatus.Connecting,
                $"Found endpoint: {info.EndpointName} [{endpoint}]. Requesting conection");
            Log.Trace("OnFound {0} - {1}", endpoint, info.EndpointName);
            await this.nearbyConnection.RequestConnection(this.principal.CurrentUserIdentity.Name, endpoint);

            SetStatus(ConnectionStatus.Connecting,
                $"Requested conection from endpoint: {info.EndpointName} [{endpoint}]");
        }

        //protected virtual void OnDisconnected(string endpoint)
        //{
        //    Log.Trace("OnDisconnected {0}", endpoint);
        //    // stop all network activity
        //}

        //protected virtual void OnConnectionResult(string endpoint, NearbyConnectionResolution resolution)
        //{
        //    Log.Trace("OnConnectionResult {0}, Success: {1}, Code: {2}", endpoint, resolution.IsSuccess,
        //        resolution.StatusCode);
        //    SetStatus(ConnectionStatus.Connected, $"Connected to endpoint [{endpoint}]");
        //}

        //protected virtual async void OnInitiatedConnection(string endpoint, NearbyConnectionInfo info)
        //{
        //    Log.Trace("OnInitiatedConnection {0} - {1}", endpoint, info.EndpointName);
        //    SetStatus(ConnectionStatus.Connecting, $"Accepting connection from {info.EndpointName} [{endpoint}]");
        //    await this.nearbyConnection.AcceptConnection(endpoint);
        //}

        protected async Task StartDiscovery()
        {
            await permissions.AssureHasPermission(Permission.Location);

            SetStatus(ConnectionStatus.StartDiscovering, $"Starting discovery");
            await this.nearbyConnection.StartDiscovery(GetServiceName());
            SetStatus(ConnectionStatus.Discovering, $"Searching for supervisor");
        }

        protected Task StopDiscovery()
        {
            return this.nearbyConnection.StopDiscovery();
        }
        
        protected Task StopAdvertising()
        {
            return this.nearbyConnection.StopAdvertising();
        }

        protected abstract string GetServiceName();

        protected async Task StartAdvertising()
        {
            await permissions.AssureHasPermission(Permission.Location);

            Log.Trace("StartAdvertising");

            SetStatus(ConnectionStatus.StartAdvertising, $"Starting advertising");
            await this.nearbyConnection.StartAdvertising(GetServiceName(), this.principal.CurrentUserIdentity.Name);

            SetStatus(ConnectionStatus.Advertising, "Waiting for interviewers connections");
        }

        protected virtual async void OnConnection(string endpoint, NearbyConnectionInfo info)
        {
            SetStatus(ConnectionStatus.Connecting, $"Accepting connection from {endpoint} {info.EndpointName}");
            Log.Trace("OnConnection from " + endpoint + " => " + info.EndpointName);
            await this.nearbyConnection.AcceptConnection(endpoint);
            Log.Trace("Accept connection from " + endpoint + " => " + info.EndpointName);
            SetStatus(ConnectionStatus.Connected, $"Connected to {endpoint} {info.EndpointName}");
        }

        private string status;
        private string statusDetails;

        public string Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        public string StatusDetails
        {
            get => statusDetails;
            set => SetProperty(ref statusDetails, value);
        }
    }
}
