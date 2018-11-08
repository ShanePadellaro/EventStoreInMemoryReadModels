using System;
using MongoDB.Bson;

namespace TransactionService.Api.ReadModels
{
    public class FinanceReadModelBase
    {
        public DateTime CreatedOn { get; set; }
        public ObjectId _id { get; set; }
        public DateTime BillingDate { get; set; }
        public string TransactionType { get; set; }
        public string TransactionId { get; set; }
        public string ExternalId { get; set; }
        public FinanceType Type { get; set; }
    }
}