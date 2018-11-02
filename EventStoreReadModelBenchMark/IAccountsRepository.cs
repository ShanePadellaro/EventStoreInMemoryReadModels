using System.Collections.Generic;

namespace EventStoreReadModelBenchMark
{
    public interface IAccountsRepository
    {
        Dictionary<string, Account> GetAccounts();
    }
}