using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EventStoreReadModelBenchMark.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly IAccountsRepository _accountsRepository;

        public AccountController(IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
        }
        
        [HttpGet("api/v1/accounts")]
        public async Task<IDictionary<string,Account>> GetAccountsAsync()
        {
            var accounts = _accountsRepository.GetAccounts();
            return accounts;
        }
        
        [HttpGet("api/v1/accounts/{accountId}")]
        public async Task<ActionResult<Account>> GetAccountAsync(string accountId)
        {
            var accounts = _accountsRepository.GetAccounts();
            if (!accounts.TryGetValue(accountId, out var account))
                return NotFound();
            
            return account;
        }
    }
}