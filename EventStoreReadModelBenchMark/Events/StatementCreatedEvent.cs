using System;

namespace EventStoreReadModelBenchMark.Events
{
    public class StatementCreatedEvent
    {
        public StatementCreatedEvent(long incomingBalance,DateTime billingDate)
        {
            IncomingBalance = incomingBalance;
            BillingDate = billingDate;
        }

        public long IncomingBalance { get; set; }
        public DateTime BillingDate { get; set; }
    }
}