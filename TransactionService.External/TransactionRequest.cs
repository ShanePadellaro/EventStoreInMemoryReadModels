using TransactionService.External.ValueObjects;

namespace TransactionService.External
{
    public class TransactionRequest
    {
        public string ApiVersion { get; set; }
        public Transaction Transaction { get; set; }
    }
}