﻿using System.Linq;
using Newtonsoft.Json;
using TransactionService.Api.Events;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.EventHandlers
{
    internal class AccountDebitedEventHandler : IDomainEventHandler
    {
        public State Execute(State state)
        {
            if (state.EventType != DomainEventTypes.AccountDebited)
                return state;

            var @event =
                JsonConvert.DeserializeObject<Transaction>(state.Event);

            state.Account.Balance -= @event.Amount;

            var countryCode = @event.CountryCode;
            var billingDate = @event.BillingDate;
            var tax = @event.Tax;
            state.TaxLedger.RecordTax(tax, countryCode,billingDate);

            var fees = @event.TransactionItems.SelectMany(x => x.SubFees);
            state.FeeLedger.RecordFees(fees, billingDate);

            return state;
        }
    }
}