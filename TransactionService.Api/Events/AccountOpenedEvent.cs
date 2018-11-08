using TransactionService.Api.ValueObjects;

namespace TransactionService.Api.Events
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