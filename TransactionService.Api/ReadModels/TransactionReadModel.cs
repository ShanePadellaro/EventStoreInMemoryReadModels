using System;
using MongoDB.Bson;
using TransactionService.Api.ValueObjects;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.ReadModels
{
    public class TransactionReadModel
    {
        public TransactionReadModel(Transaction transaction, DateTime createdOn, long accountBalance)
        {
            Transaction = transaction;
            CreatedOn = createdOn;
            AccountBalance = accountBalance;
            MetaData = new MetaData();
        }

        public Transaction Transaction { get; set; }
        public DateTime CreatedOn { get; }
        public long AccountBalance { get; set; }
        public MetaData MetaData { get; set; }
        public ObjectId _id { get; set; }
    }

    public class MetaData
    {
        public long OriginalEventNumber { get; set; }
        public long EventNumber { get; set; }
        public DateTime EventCreated { get; set; }
    }
}