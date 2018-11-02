using EventStoreReadModelBenchMark.Events;

namespace EventStoreReadModelBenchMark
{
    internal class State
    {
        public TaxLedger TaxLedger { get; }
        public FeeLedger FeeLedger { get; }
        public string Event { get; }
        public Account Account { get; set; }
        public DomainEventTypes EventType { get; }
        public string EventStreamId { get; }


        public State(Account account, TaxLedger taxLedger, FeeLedger feeLedger, DomainEventTypes eventType,
            string @event,
            string eventEventStreamId)
        {
            TaxLedger = taxLedger;
            FeeLedger = feeLedger;
            Event = @event;
            Account = account;
            EventType = eventType;
            EventStreamId = eventEventStreamId;
        }

        
    }
}