using System.Collections.Generic;
using TransactionService.Api.Domain;

namespace TransactionService.Api.Repository
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