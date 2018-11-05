using System;

namespace EventStoreReadModelBenchMark.ReadModels
{
    public class TaxReadModel : FinanceReadModelBase
    {
        public TaxReadModel(DateTime billingDate, string transactionType, string transactionId, string externalId,
            long transactionAmount, long taxAmount, string taxRate)
        {
            BillingDate = billingDate;
            TransactionType = transactionType ?? throw new ArgumentNullException(nameof(transactionType));
            TransactionId = transactionId ?? throw new ArgumentNullException(nameof(transactionId));
            ExternalId = externalId;
            TransactionAmount = transactionAmount;
            TaxAmount = taxAmount;
            TaxRate = taxRate ?? throw new ArgumentNullException(nameof(taxRate));
        }

        public TaxReadModel(TransactionReadModel transactionReadModel)
        {
            BillingDate = transactionReadModel.Transaction.BillingDate;
            TransactionType = transactionReadModel.Transaction.Type;
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