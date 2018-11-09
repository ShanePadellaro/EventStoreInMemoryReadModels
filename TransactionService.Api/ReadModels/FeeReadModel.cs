using System;
using TransactionService.Api.ValueObjects;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.ReadModels
{
    public class FeeReadModel:FinanceReadModelBase
    {
        public FeeReadModel(Fee fee, in DateTime billingDate, TransactionType transactionType, Guid transactionId,
            string externalId, DateTime createdOn)
        {
            BillingDate = billingDate;
            TransactionType = transactionType;
            ExternalId = externalId;
            CreatedOn = createdOn;
            TransactionId = transactionId.ToString();
            Label = fee.Label;
            FeeId = fee.FeeId;
            FeeAmount = fee.Amount;
            Type = FinanceType.Fee;
        }

        public string Label { get; set; }
        public string FeeId { get; set; }
        public long FeeAmount { get; set; }
    }
}