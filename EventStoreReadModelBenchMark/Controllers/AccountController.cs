using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EventStoreReadModelBenchMark.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountsRepository _accountsRepository;

        public AccountController(IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
        }
        
        [HttpGet("api/accounts")]
        public async Task<IDictionary<string,Account>> GetAccounts()
        {
            var accounts = _accountsRepository.GetAccounts();
            return accounts;
        }
    }
}