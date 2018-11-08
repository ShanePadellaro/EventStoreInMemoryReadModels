using System.Linq;
using Newtonsoft.Json;
using TransactionService.Api.Events;

namespace TransactionService.Api.EventHandlers
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

            var countryCode = @event.Transaction.CountryCode;
            var billingDate = @event.Transaction.BillingDate;
            var tax = @event.Transaction.Tax;
            state.TaxLedger.RecordTax(tax, countryCode,billingDate);

            var fees = @event.Transaction.TransactionItems.SelectMany(x => x.SubFees);
            state.FeeLedger.RecordFees(fees, billingDate);

            return state;
        }
    }
}