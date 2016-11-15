using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public interface ICommandPreProcessor<in TAggregateRoot, in TCommand> 
        where TAggregateRoot : IAggregateRoot
        where TCommand : ICommand
    {
        void Process(TAggregateRoot aggregate, TCommand command);
    }
}