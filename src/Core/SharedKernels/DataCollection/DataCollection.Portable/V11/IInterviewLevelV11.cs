using System;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public interface IInterviewLevelV11
    {
        Func<bool> GetConditionExpressionResult(Identity entitIdentity);
    }
}