using System;
using EventStore.ClientAPI;
using TransactionService.Api.ValueObjects;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.ReadModels
{
    public class FeeReadModel : FinanceReadModelBase
    {
        public string Label { get; set; }
        public string FeeId { get; set; }
        public long FeeAmount { get; set; }


        public FeeReadModel(Fee fee, Transaction transaction, ResolvedEvent evt)
        {
            BillingDate = transaction.BillingDate;
            TransactionType = transaction.TransactionType;
            ExternalId = transaction.ExternalId;
            CreatedOn = evt.Event.Created;
            TransactionId = evt.Event.EventId.ToString();
            Label = fee.Label;
            FeeId = fee.FeeId;
            FeeAmount = fee.Amount;
            Type = FinanceType.Fee;
            CountryCode = transaction.CountryCode;
        }
    }
}