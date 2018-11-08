using System.Collections.Generic;
using TransactionService.Api.Domain;

namespace TransactionService.Api
{
    public interface IAccountsRepository
    {
        Dictionary<string, Account> GetAccounts();
    }
}