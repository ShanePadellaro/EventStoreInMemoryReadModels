using EventStoreReadModelBenchMark.Events;
using Newtonsoft.Json;

namespace EventStoreReadModelBenchMark.EventHandlers
{
    internal class AccountCreditedEventHandler : IDomainEventHandler
    {
        public State Execute(State state)
        {
            if (state.EventType != DomainEventTypes.AccountCredited)
                return state;

            var @event =
                JsonConvert.DeserializeObject<AccountDebitedEvent>(state.Event);

            state.Account.Balance += @event.Transaction.Amount;
            state.Account.CurrentStatement?.MakePayment(@event.Transaction.Amount);
            
            var tax = @event.Transaction.Tax;
            state.TaxLedger.AddTax(tax, @event.Transaction.CountryCode,@event.Transaction.BillingDate);

            return state;
        }
    }
}