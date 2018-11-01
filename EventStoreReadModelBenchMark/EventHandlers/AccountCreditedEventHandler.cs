using EventStoreReadModelBenchMark.Events;
using Newtonsoft.Json;

namespace EventStoreReadModelBenchMark.EventHandlers
{
    internal class AccountCreditedEventHandler : IDomainEventHandler
    {
        public SomethingEventTuple Execute(SomethingEventTuple thingy)
        {
            if (thingy.EventType != DomainEventTypes.AccountCredited)
                return thingy;

            var @event =
                JsonConvert.DeserializeObject<AccountDebitedEvent>(thingy.Event);

            thingy.Account.Balance += @event.Transaction.Amount;
            thingy.Account.CurrentStatement?.MakePayment(@event.Transaction.Amount);


            return thingy;
        }
    }
}