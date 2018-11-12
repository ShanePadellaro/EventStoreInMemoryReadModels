using System;
using MongoDB.Bson;
using TransactionService.Api.ReadModels;

namespace TransactionService.Api.Controllers
{
    public class FeeViewModel
    {
        public string Label { get; set; }
        public string FeeId { get; set; }
        public long FeeAmount { get; set; }
        public DateTime CreatedOn { get; set; }
        public ObjectId Id { get; set; }
        public DateTime BillingDate { get; set; }
        public string TransactionType { get; set; }
        public string ExternalTransactionType { get; set; }
        public string TransactionId { get; set; }
        public string ExternalId { get; set; }
        public string Type { get; set; }
        public string CountryCode { get; set; }
        
        public FeeViewModel(FeeReadModel feeReadModel)
        {
            Label = feeReadModel.Label;
            FeeId = feeReadModel.FeeId;
            FeeAmount = feeReadModel.FeeAmount;
            CreatedOn = feeReadModel.CreatedOn;
            Id = feeReadModel.Id;
            BillingDate = feeReadModel.BillingDate;
            TransactionType = feeReadModel.TransactionType.ToString();
            ExternalTransactionType = feeReadModel.ExternalTransactionType;
            TransactionId = feeReadModel.TransactionId;
            ExternalId = feeReadModel.ExternalId;
            Type = feeReadModel.Type.ToString();
            CountryCode = feeReadModel.CountryCode;

        }
    }
}