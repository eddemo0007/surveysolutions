﻿using Android.Gms.Nearby.Connection;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.UI.Shared.Enumerator.OfflineSync.Services.Entities;
using Payload = WB.UI.Shared.Enumerator.OfflineSync.Services.Entities.Payload;

namespace WB.UI.Shared.Enumerator.OfflineSync.Entities
{
    internal class OnPayloadCallback : PayloadCallback
    {
        private readonly NearbyPayloadCallback callback;

        public OnPayloadCallback(NearbyPayloadCallback callback)
        {
            this.callback = callback;
        }

        public override void OnPayloadReceived(string endpointId, Android.Gms.Nearby.Connection.Payload payload)
        {
            var wbPayload = new Payload(payload);
        
            callback.OnPayloadReceived(endpointId, wbPayload);
        }

        public override void OnPayloadTransferUpdate(string endpointId, PayloadTransferUpdate update)
        {
            var transferUpdate = new NearbyPayloadTransferUpdate
            {
                Id = update.PayloadId,
                TotalBytes = update.TotalBytes,
                BytesTransferred = update.BytesTransferred
            };

            switch (update.TransferStatus)
            {
                case PayloadTransferUpdate.Status.InProgress: transferUpdate.Status = TransferStatus.InProgress; break;
                case PayloadTransferUpdate.Status.Success: transferUpdate.Status = TransferStatus.Success; break;
                case PayloadTransferUpdate.Status.Failure: transferUpdate.Status = TransferStatus.Failure; break;
            }

            callback.OnPayloadTransferUpdate(endpointId, transferUpdate);
        }
    }
}
