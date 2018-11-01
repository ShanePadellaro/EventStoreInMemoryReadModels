using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task<ActionResult<IList<Statement>>> GetTransactionsAsync(string accountId)
        {
            var lastTransaction = _mongoDatabase.GetCollection<TransactionReadModel>($"{accountId}-transactions")
                .AsQueryable()
                .OrderByDescending(x => x._id).FirstOrDefault();
            
            if (string.IsNullOrWhiteSpace(accountId))
                return BadRequest("Invalid AccountId");
            
            var accounts = _accountsRepository.GetAccounts();
            if (!accounts.TryGetValue(accountId, out var account))
                return NotFound();
            
            return account.Statements;
        }
    }
}