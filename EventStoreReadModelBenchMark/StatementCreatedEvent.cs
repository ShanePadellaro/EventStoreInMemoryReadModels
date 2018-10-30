using System;

namespace EventStoreReadModelBenchMark
{
    public class StatementCreatedEvent
    {
        public long IncomingBalance { get; set; }
        public DateTime BillingDate { get; set; }
    }
}