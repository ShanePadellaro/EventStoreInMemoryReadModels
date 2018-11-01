using Newtonsoft.Json;

namespace EventStoreReadModelBenchMark
{
    internal class AccountOpenedEventHandler : IDomainEventHandler
    {
        public SomethingEventTuple Execute(SomethingEventTuple thingy)
        {
            if (thingy.EventType != DomainEventTypes.AccountOpened)
                return thingy;

            var @event =
                JsonConvert.DeserializeObject<AccountOpenedEvent>(thingy.Event);

            thingy.Account = new Account()
                {Balance = @event.AccountDetails.StartingBalance, Id = thingy.EventStreamId};

            return thingy;
        }
    }
}