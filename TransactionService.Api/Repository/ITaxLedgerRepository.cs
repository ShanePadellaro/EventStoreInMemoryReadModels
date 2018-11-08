using TransactionService.Api.Domain;

namespace TransactionService.Api.Repository
{
    public interface ITaxLedgerRepository
    {
        TaxLedger GetTaxLedger();
    }
}