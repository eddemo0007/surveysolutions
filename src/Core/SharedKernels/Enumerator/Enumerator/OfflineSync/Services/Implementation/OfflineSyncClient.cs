﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class OfflineSyncClient : IOfflineSyncClient
    {
        private readonly INearbyCommunicator communicator;
        private readonly INearbyConnection nearbyConnection;
        
        public OfflineSyncClient(INearbyCommunicator communicator, INearbyConnection nearbyConnection)
        {
            this.communicator = communicator;
            this.nearbyConnection = nearbyConnection;
            this.nearbyConnection.RemoteEndpoints.CollectionChanged += (sender, args) =>
            {
                Endpoint = this.nearbyConnection.RemoteEndpoints.FirstOrDefault()?.Enpoint;
            };
        }

        public static string Endpoint { get; set; }

        public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken, IProgress<TransferProgress> progress = null) 
            where TRequest : ICommunicationMessage
            where TResponse : ICommunicationMessage
        {
            return this.communicator.SendAsync<TRequest, TResponse>(this.nearbyConnection, Endpoint, request, progress, cancellationToken);
        }

        public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken, IProgress<TransferProgress> progress = null)
            where TRequest : ICommunicationMessage
        {
            await this.communicator.SendAsync<TRequest, OkResponse>(this.nearbyConnection, Endpoint, request, progress, cancellationToken);
        }
    }
}
