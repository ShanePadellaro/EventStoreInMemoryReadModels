using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Api.Domain;
using TransactionService.Api.Events;
using TransactionService.Api.ValueObjects;

namespace TransactionService.Api.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IEventStoreConnection _eventStoreConnection;

        public AccountController(IAccountsRepository accountsRepository,IEventStoreConnection eventStoreConnection)
        {
            _accountsRepository = accountsRepository;
            _eventStoreConnection = eventStoreConnection;
        }
        
        [HttpGet("api/v1/accounts")]
        public async Task<IDictionary<string,Account>> GetAccountsAsync()
        {
            var accounts = _accountsRepository.GetAccounts();
            return accounts;
        }
        
        [HttpPost("api/v1/accounts")]
        public async Task<ActionResult<Account>> CreateAccountAsync()
        {

            var s = AccountId.New;
            var accountDetails = new AccountDetails(s.Id,"catalystExternalId", "Apiaccount", "GBR", "GBP", 0);
            var @event = new AccountOpenedEvent(accountDetails);
            
            await _eventStoreConnection.AppendToStreamAsync(@event.Data.AccountId, ExpectedVersion.Any,
                @event.EventData);
            
            return new Account(accountDetails);
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