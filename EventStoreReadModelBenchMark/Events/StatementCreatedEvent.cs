using System;

namespace EventStoreReadModelBenchMark.Events
{
    public class StatementCreatedEvent
    {
        public long IncomingBalance { get; set; }
        public DateTime BillingDate { get; set; }
    }
}