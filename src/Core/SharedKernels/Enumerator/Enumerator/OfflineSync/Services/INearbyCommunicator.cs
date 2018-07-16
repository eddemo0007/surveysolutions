﻿using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface INearbyCommunicator
    {
        Task<TResponse> SendAsync<TRequest, TResponse>(INearbyConnection connection,
            string endpoint, TRequest message, IProgress<TransferProgress> progress,
            CancellationToken cancellationToken)
            where TRequest : ICommunicationMessage
            where TResponse : ICommunicationMessage;
        
        Task RecievePayloadAsync(INearbyConnection nearbyConnection, string endpoint, IPayload payload);
        void RecievePayloadTransferUpdate(INearbyConnection nearbyConnection, string endpoint, NearbyPayloadTransferUpdate update);

        IObservable<IncomingDataInfo> IncomingInfo { get; }
    }
}
