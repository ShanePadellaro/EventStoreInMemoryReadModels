using System.Linq;
using System.Net.Http.Formatting;
using Airfi.Transactions.Client.ValueObjects;
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
                JsonConvert.DeserializeObject<Transaction>(state.Event,new JsonSerializerSettings(){ContractResolver = new JsonContractResolver(new JsonMediaTypeFormatter())});

            state.Account.Balance += @event.Amount;
            state.Account.CurrentStatement?.MakePayment(@event.Amount);
            
            var countryCode = @event.CountryCode;
            var billingDate = @event.BillingDate;
            var tax = @event.Tax;
            state.TaxLedger.RecordTax(tax, countryCode,billingDate);

            var fees = @event.TransactionItems.SelectMany(x => x.SubFees);
            state.FeeLedger.RecordFees(fees, billingDate,countryCode);

            return state;
        }
    }
}