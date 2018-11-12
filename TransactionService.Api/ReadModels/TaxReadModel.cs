using System;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.ReadModels
{
    public class TaxReadModel : FinanceReadModelBase
    {
        public long TransactionAmount { get; set; }
        public long TaxAmount { get; set; }
        public decimal TaxRate { get; set; }

        public TaxReadModel(TransactionReadModel transactionReadModel)
        {
            BillingDate = transactionReadModel.Transaction.BillingDate;
            TransactionType = transactionReadModel.Transaction.TransactionType;
            TransactionId = transactionReadModel.Transaction.ExternalId;
            ExternalId = transactionReadModel.Transaction.ExternalId;
            TransactionAmount = transactionReadModel.Transaction.Amount;
            TaxAmount = transactionReadModel.Transaction.Tax;
            TaxRate = transactionReadModel.Transaction.Taxrate;
            CreatedOn = transactionReadModel.CreatedOn;
            Type = FinanceType.Tax;
            CountryCode = transactionReadModel.Transaction.CountryCode;
        }

        
    }
}