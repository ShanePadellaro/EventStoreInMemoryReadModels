
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.Events
{
    public class AccountDebitedEvent:DomainEvent<Transaction>
    {

        public AccountDebitedEvent(Transaction transaction)
        {
            Data = transaction;
            MetaData = new EventMetaData() {EventVersion = "1"};
        }

        public override string Name => DomainEventTypes.AccountDebited.ToCamelCase();
        public override Transaction Data { get; }
    }
}

