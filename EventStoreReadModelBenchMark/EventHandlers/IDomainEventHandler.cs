namespace EventStoreReadModelBenchMark.EventHandlers
{
    internal interface IDomainEventHandler
    {
        SomethingEventTuple Execute(SomethingEventTuple thingy);
    }
}