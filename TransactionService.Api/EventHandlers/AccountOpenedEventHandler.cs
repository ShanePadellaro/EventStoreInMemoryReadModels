using Newtonsoft.Json;
using TransactionService.Api.Domain;
using TransactionService.Api.Events;
using TransactionService.Api.ValueObjects;

namespace TransactionService.Api.EventHandlers
{
    internal class AccountOpenedEventHandler : IDomainEventHandler
    {
        public State Execute(State state)
        {
            if (state.EventType != DomainEventTypes.AccountOpened)
                return state;

            var @event =
                JsonConvert.DeserializeObject<AccountDetails>(state.Event);

            state.Account = new Account()
            {
                Balance = @event.StartingBalance,
                Id = state.EventStreamId,
                Name = @event.Name,
                ExternalId = @event.Externalid,
                CurrencyCode = @event.CurrencyCode,
                CountryCode = @event.CountryCode
            };

            return state;
        }
    }
}