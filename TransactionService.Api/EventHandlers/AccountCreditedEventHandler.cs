using System.Linq;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using TransactionService.Api.Events;

namespace TransactionService.Api.EventHandlers
{
    internal class AccountCreditedEventHandler : IDomainEventHandler
    {
        public State Execute(State state)
        {
            if (state.EventType != DomainEventTypes.AccountCredited)
                return state;

            var @event =
                JsonConvert.DeserializeObject<AccountDebitedEvent>(state.Event,new JsonSerializerSettings(){ContractResolver = new JsonContractResolver(new JsonMediaTypeFormatter())});

            state.Account.Balance += @event.Transaction.Amount;
            state.Account.CurrentStatement?.MakePayment(@event.Transaction.Amount);
            
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