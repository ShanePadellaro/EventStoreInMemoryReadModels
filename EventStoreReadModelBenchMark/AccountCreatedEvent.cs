namespace EventStoreReadModelBenchMark
{
    
    public class AccountCreatedEvent 
    {
        public AccountDetails AccountDetails { get; }

        public AccountCreatedEvent(AccountDetails accountDetails)
        {
            AccountDetails = accountDetails;
        }
    }
}