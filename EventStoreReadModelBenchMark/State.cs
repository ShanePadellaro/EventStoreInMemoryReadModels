using EventStoreReadModelBenchMark.Events;

namespace EventStoreReadModelBenchMark
{
    internal class State
    {
        public TaxLedger TaxLedger { get; }
        public string Event;
        public Account Account;
        public DomainEventTypes EventType;
        public string EventStreamId;


        public State( Account account, TaxLedger taxLedger, DomainEventTypes eventType,string @event,
            string eventEventStreamId)
        {
            TaxLedger = taxLedger;
            Event = @event;
            Account = account;
            EventType = eventType;
            EventStreamId = eventEventStreamId;
        }

        
    }
}