﻿namespace WB.Core.SharedKernels.DataCollection
{
    public class SyncProtocolVersionProvider : ISyncProtocolVersionProvider
    {
        //previous values: 5962, 7018, 7034, 7050, 7060
        public int GetProtocolVersion() => 7070;

        public int GetLastNonUpdatableVersion() => 7000;
    }
}
