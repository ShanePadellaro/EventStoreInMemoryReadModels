
using TransactionService.Api.ValueObjects;

namespace TransactionService.Api.Events
{
    
    public class AccountOpenedEvent:DomainEvent<AccountDetails>
    {

        public AccountOpenedEvent(AccountDetails accountDetails)
        {
            Data = accountDetails;
            MetaData = new EventMetaData() {EventVersion = "1"};
        }
        public override string Name => DomainEventTypes.AccountOpened.ToCamelCase();
        public override AccountDetails Data { get; }
    }
}