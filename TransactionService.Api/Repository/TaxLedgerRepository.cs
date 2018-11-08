using TransactionService.Api.Domain;

namespace TransactionService.Api.Repository
{
    public class TaxLedgerRepository : ITaxLedgerRepository
    {
        private static TaxLedger _taxLedger;

        public TaxLedgerRepository()
        {
            _taxLedger = new TaxLedger();
        }
        public TaxLedger GetTaxLedger()
        {
            return _taxLedger;
        }
    }
}