using System;

namespace TransactionService.Api.Events
{
    public class StatementCreatedEvent:DomainEvent<StatementDetails>
    {
        public override string Name => DomainEventTypes.StatementCreated.ToCamelCase();
        public override StatementDetails Data { get; }
        
        public StatementCreatedEvent(long incomingBalance,DateTime billingDate)
        {
            Data = new StatementDetails() {BillingDate = billingDate, IncomingBalance = incomingBalance};
            MetaData = new EventMetaData() {EventVersion = "1"};
        }
    }
    
    public class StatementDetails
    {
        public long IncomingBalance { get; set; }
        public DateTime BillingDate { get; set; }    
    }
}