namespace EventStoreReadModelBenchMark
{
    internal class SomethingEventTuple
    {
        public string Event;
        public Account Account;
        public DomainEventTypes EventType;
        public string EventStreamId;


        public SomethingEventTuple(string @event, Account account,DomainEventTypes eventType, string eventEventStreamId)
        {
            Event = @event;
            Account = account;
            EventType = eventType;
            EventStreamId = eventEventStreamId;
        }

        
    }
}