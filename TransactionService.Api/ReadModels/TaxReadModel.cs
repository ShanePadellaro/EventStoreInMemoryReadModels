using System;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.ReadModels
{
    public class TaxReadModel : FinanceReadModelBase
    {
        public TaxReadModel(DateTime billingDate, TransactionType transactionType, string transactionId, string externalId,
            long transactionAmount, long taxAmount, string taxRate)
        {
            BillingDate = billingDate;
            TransactionType = transactionType;
            TransactionId = transactionId ?? throw new ArgumentNullException(nameof(transactionId));
            ExternalId = externalId;
            TransactionAmount = transactionAmount;
            TaxAmount = taxAmount;
            TaxRate = taxRate ?? throw new ArgumentNullException(nameof(taxRate));
        }

        public TaxReadModel(TransactionReadModel transactionReadModel)
        {
            BillingDate = transactionReadModel.Transaction.BillingDate;
            TransactionType = transactionReadModel.Transaction.TransactionType;
            TransactionId = transactionReadModel.Transaction.ExternalId;
            ExternalId = transactionReadModel.Transaction.ExternalId;
            TransactionAmount = transactionReadModel.Transaction.Amount;
            TaxAmount = transactionReadModel.Transaction.Tax;
            TaxRate = transactionReadModel.Transaction.Taxrate.ToString();
            CreatedOn = transactionReadModel.CreatedOn;
            Type = FinanceType.Tax;
        }

        public long TransactionAmount { get; set; }
        public long TaxAmount { get; set; }
        public string TaxRate { get; set; }
    }
}