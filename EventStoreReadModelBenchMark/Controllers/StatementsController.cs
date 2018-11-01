using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStoreReadModelBenchMark.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EventStoreReadModelBenchMark.Controllers
{
    public class StatementsController : ControllerBase
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IMongoDatabase _mongoDatabase;

        public StatementsController(IAccountsRepository accountsRepository,IMongoDatabase mongoDatabase)
        {
            _accountsRepository = accountsRepository;
            _mongoDatabase = mongoDatabase;
        }
        
        [HttpGet("api/accounts/{accountId}/statements")]
        public async Task<ActionResult<IList<Statement>>> GetStatementsAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return BadRequest("Invalid AccountId");
            
            var accounts = _accountsRepository.GetAccounts();
            if (!accounts.TryGetValue(accountId, out var account))
                return NotFound();
            
            return account.Statements;
        }
        
        [HttpGet("api/accounts/{accountId}/statements/{year}/{month}/transactions")]
        public async Task<ActionResult<IList<TransactionReadModel>>> GetTransactionsAsync(string accountId,int year,int month,[FromQuery] int page,[FromQuery]int pageSize)
        {
             
            if (string.IsNullOrWhiteSpace(accountId))
                return BadRequest("Invalid AccountId");
            
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            var transactions = await _mongoDatabase.GetCollection<TransactionReadModel>($"{accountId}-transactions")
                .AsQueryable()
                .Where(t=>t.MetaData.EventCreated >= firstDayOfMonth && t.MetaData.EventCreated <= lastDayOfMonth)
                .Skip(page <= 1?0:(page-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return transactions;
        }
    }
}