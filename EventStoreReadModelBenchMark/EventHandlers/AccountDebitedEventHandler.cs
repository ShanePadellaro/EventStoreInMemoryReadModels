using EventStoreReadModelBenchMark.Events;
using Newtonsoft.Json;

namespace EventStoreReadModelBenchMark.EventHandlers
{
    internal class AccountDebitedEventHandler : IDomainEventHandler
    {
        public State Execute(State state)
        {
            if (state.EventType != DomainEventTypes.AccountDebited)
                return state;

            var @event =
                JsonConvert.DeserializeObject<AccountDebitedEvent>(state.Event);

            state.Account.Balance -= @event.Transaction.Amount;
            var tax = @event.Transaction.Tax;
            state.TaxLedger.AddTax(tax, @event.Transaction.CountryCode,@event.Transaction.BillingDate);

            return state;
        }
    }
}