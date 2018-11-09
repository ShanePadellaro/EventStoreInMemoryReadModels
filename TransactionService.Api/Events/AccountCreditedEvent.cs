using EventStore.ClientAPI;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.Events
{
    public class AccountCreditedEvent:DomainEvent<Transaction>
    {
        public override Transaction Data { get; }
        public override string Name => DomainEventTypes.AccountCredited.ToCamelCase();

        public AccountCreditedEvent(Transaction transaction)
        {
            Data = transaction;
            MetaData = new EventMetaData() {EventVersion = "1"};
        }

    }
}