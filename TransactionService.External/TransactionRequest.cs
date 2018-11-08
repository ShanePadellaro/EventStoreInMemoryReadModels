using TransactionService.External.ValueObjects;

namespace TransactionService.External
{
    public class TransactionRequest
    {
        public string ApiVersion = "1";
        public Transaction Transaction { get; set; }
    }
}