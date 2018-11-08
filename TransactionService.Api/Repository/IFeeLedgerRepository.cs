using TransactionService.Api.Domain;

namespace TransactionService.Api.Repository
{
    public interface IFeeLedgerRepository
    {
        FeeLedger GetFeeLedger();
    }
}