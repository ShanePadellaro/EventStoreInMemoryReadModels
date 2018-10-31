using EventStoreReadModelBenchMark.ValueObjects;

namespace EventStoreReadModelBenchMark
{
    public class TransactionReadModel
    {
        public TransactionReadModel(Transaction transaction, long accountBalance)
        {
            Transaction = transaction;
            AccountBalance = accountBalance;
            MetaData = new MetaData();
        }

        public Transaction Transaction { get; set; }
        public long AccountBalance { get; set; }
        public MetaData MetaData { get; set; }
    }

    public class MetaData
    {
        public long OriginalEventNumber { get; set; }
        public long EventNumber { get; set; }
    }
}