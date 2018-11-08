namespace TransactionService.Api.EventHandlers
{
    internal interface IDomainEventHandler
    {
        State Execute(State state);
    }
}