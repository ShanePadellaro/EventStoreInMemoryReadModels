using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TransactionService.Api.ReadModels;
using TransactionService.Api.Repository;

namespace TransactionService.Api.Controllers
{
    public class FeeController : ControllerBase
    {
        private readonly IFeeLedgerRepository _feeLedgerRepository;
        private readonly IMongoDatabase _mongoDatabase;

        public FeeController(IFeeLedgerRepository feeLedgerRepository,IMongoDatabase mongoDatabase)
        {
            _feeLedgerRepository = feeLedgerRepository;
            _mongoDatabase = mongoDatabase;
        }
        [HttpGet("api/v1/fees")]
        public async Task<ActionResult<FeeLedger>> GetFeesAsync()
        {
            return _feeLedgerRepository.GetFeeLedger();
        }
        
        [HttpGet("api/v1/fees/{feeId}/{year}/{month}/transactions")]
        public async Task<ActionResult<IEnumerable<FeeReadModel>>> GetFeesTransactionsAsync(string feeid,int year,int month,[FromQuery] int page,[FromQuery]int pageSize)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            var transactions = await _mongoDatabase.GetCollection<FeeReadModel>("finance")
                .AsQueryable()
                .Where(t=>t.CreatedOn >= firstDayOfMonth && t.CreatedOn <= lastDayOfMonth && t.Type == FinanceType.Fee && t.FeeId == feeid)
                .Skip(page <= 1?0:(page-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return transactions;
        }
    }
}