using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Api.Controllers.InputModels;
using TransactionService.Api.Domain;
using TransactionService.Api.Events;
using TransactionService.Api.ValueObjects;

namespace TransactionService.Api.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IEventStoreConnection _eventStoreConnection;

        public AccountController(IAccountsRepository accountsRepository, IEventStoreConnection eventStoreConnection)
        {
            _accountsRepository = accountsRepository;
            _eventStoreConnection = eventStoreConnection;
        }

        [HttpGet("api/v1/accounts")]
        public async Task<IDictionary<string, Account>> GetAccountsAsync()
        {
            var accounts = _accountsRepository.GetAccounts();
            return accounts;
        }

        [HttpPost("api/v1/accounts")]
        public async Task<ActionResult<Account>> CreateAccountAsync([FromBody] AccountInputModel inputModel)
        {
            if (!TryValidateModel(inputModel))
                return BadRequest();

            var accountDetails = new AccountDetails(AccountId.New.Id, inputModel.Externalid, inputModel.Name,
                inputModel.CountryCode, inputModel.CurrencyCode, inputModel.StartingBalance);
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