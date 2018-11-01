using Newtonsoft.Json;

namespace EventStoreReadModelBenchMark
{
    internal class AccountDebitedEventHandler : IDomainEventHandler
    {
        public SomethingEventTuple Execute(SomethingEventTuple thingy)
        {
            if (thingy.EventType != DomainEventTypes.AccountDebited)
                return thingy;

            var @event =
                JsonConvert.DeserializeObject<AccountDebitedEvent>(thingy.Event);

            thingy.Account.Balance -= @event.Transaction.Amount;


            return thingy;
        }
    }
}