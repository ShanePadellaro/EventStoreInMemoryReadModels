using System;
using System.Collections.Generic;

namespace TransactionService.External.ValueObjects
{
    public class Transaction 
    {
        public Transaction(TransactionType type,string externalId, string accountId, string description, string externalTransactionType, long amount, long tax,
            DateTime billingDate, long taxrate, string countryCode, string currencyCode,
            List<TransactionItem> transactionItems, List<Dictionary<string, object>> properties = null)
        {
            ExternalId = externalId;
            AccountId = accountId;
            Description = description;
            ExternalTransactionType = externalTransactionType;
            Properties = properties;
            Amount = amount;
            Tax = tax;
            BillingDate = billingDate;
            Taxrate = taxrate;
            CountryCode = countryCode;
            CurrencyCode = currencyCode;
            TransactionItems = transactionItems;
            TransactionType = type;
        }

        public string ExternalId { get;  set; }
        public string AccountId { get;  set; }
        public string Description { get;  set; }
        public string ExternalTransactionType { get;  set; }
        public TransactionType TransactionType { get; set; }
        public List<Dictionary<string, object>> Properties { get; }
        public long Amount { get;  set; }
        public long Tax { get;  set; }
        public DateTime BillingDate { get; }
        public long Taxrate { get;  set; }
        public string CountryCode { get;  set; }
        public string CurrencyCode { get;  set; }
        public List<TransactionItem> TransactionItems { get;  set; }
    }
}