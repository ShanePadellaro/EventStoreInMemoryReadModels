using System;
using MongoDB.Bson;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.ReadModels
{
    public class FinanceReadModelBase
    {
        public DateTime CreatedOn { get; set; }
        public ObjectId Id { get; set; }
        public DateTime BillingDate { get; set; }
        public TransactionType TransactionType { get; set; }
        public string TransactionId { get; set; }
        public string ExternalId { get; set; }
        public FinanceType Type { get; set; }
        public string CountryCode { get; set; }
    }
}