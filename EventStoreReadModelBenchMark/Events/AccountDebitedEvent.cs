using EventStoreReadModelBenchMark.ValueObjects;

namespace EventStoreReadModelBenchMark.Events
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

