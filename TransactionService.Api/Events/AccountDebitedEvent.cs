using TransactionService.Api.ValueObjects;

namespace TransactionService.Api.Events
{
    public class AccountDebitedEvent
    {
        public Transaction Transaction { get; }

        public AccountDebitedEvent(Transaction transaction)
        {
            Transaction = transaction;
        }

        public static string Name => DomainEventTypes.AccountDebited.ToString();
    }
}

