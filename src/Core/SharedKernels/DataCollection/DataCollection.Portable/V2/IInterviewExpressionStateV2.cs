using System;

namespace WB.Core.SharedKernels.DataCollection.V2
{
    public interface IInterviewExpressionStateV2 : IInterviewExpressionState
    {
        void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle);
        new IInterviewExpressionStateV2 Clone();
    }
}