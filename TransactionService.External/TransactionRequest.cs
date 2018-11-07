using TransactionService.External.ValueObjects;

namespace TransactionService.External
{
    public class TransactionRequest
    {
        public string APIVersion { get; set; }
        public Transaction Transaction { get; set; }
    }
}