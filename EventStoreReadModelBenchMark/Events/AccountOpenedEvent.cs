using EventStoreReadModelBenchMark.ValueObjects;

namespace EventStoreReadModelBenchMark.Events
{
    
    public class AccountOpenedEvent 
    {
        public AccountDetails AccountDetails { get; }

        public AccountOpenedEvent(AccountDetails accountDetails)
        {
            AccountDetails = accountDetails;
        }
        public static string Name => DomainEventTypes.AccountOpened.ToString();

    }
}