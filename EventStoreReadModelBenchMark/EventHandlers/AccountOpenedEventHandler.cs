using EventStoreReadModelBenchMark.Events;
using Newtonsoft.Json;

namespace EventStoreReadModelBenchMark.EventHandlers
{
    internal class AccountOpenedEventHandler : IDomainEventHandler
    {
        public State Execute(State state)
        {
            if (state.EventType != DomainEventTypes.AccountOpened)
                return state;

            var @event =
                JsonConvert.DeserializeObject<AccountOpenedEvent>(state.Event);

            state.Account = new Account()
                {Balance = @event.AccountDetails.StartingBalance, Id = state.EventStreamId};

            return state;
        }
    }
}