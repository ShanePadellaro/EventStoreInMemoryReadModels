using TransactionService.Api.ValueObjects;

namespace TransactionService.Api.Events
{
    public class AccountCreditedEvent
    {
        public Transaction Transaction { get; }

        public AccountCreditedEvent(Transaction transaction)
        {
            Transaction = transaction;
        }
        public static string Name => DomainEventTypes.AccountCredited.ToString();

    }
}