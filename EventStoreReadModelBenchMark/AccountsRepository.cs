using System.Collections.Generic;

namespace EventStoreReadModelBenchMark
{
    public class AccountsRepository : IAccountsRepository
    {
        private static Dictionary<string, Account> _accounts;

        public AccountsRepository()
        {
            _accounts = new Dictionary<string, Account>();
        }

        public Dictionary<string, Account> GetAccounts()
        {
            return _accounts;
        }
    }
}