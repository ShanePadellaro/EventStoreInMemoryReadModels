namespace EventStoreReadModelBenchMark
{
    internal interface IDomainEventHandler
    {
        SomethingEventTuple Execute(SomethingEventTuple thingy);
    }
}