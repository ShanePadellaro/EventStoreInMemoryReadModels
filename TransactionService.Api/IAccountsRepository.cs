using System.Collections.Generic;

namespace TransactionService.Api
{
    public interface IAccountsRepository
    {
        Dictionary<string, Account> GetAccounts();
    }
}