namespace EventStoreReadModelBenchMark.Repository
{
    public class FeeLedgerRepository : IFeeLedgerRepository
    {
        private static FeeLedger _feeLedger;

        public FeeLedgerRepository()
        {
            _feeLedger = new FeeLedger();
        }
        public FeeLedger GetFeeLedger()
        {
            return _feeLedger;
        }
    }
}