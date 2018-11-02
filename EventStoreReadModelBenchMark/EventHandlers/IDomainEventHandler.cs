namespace EventStoreReadModelBenchMark.EventHandlers
{
    internal interface IDomainEventHandler
    {
        State Execute(State state);
    }
}