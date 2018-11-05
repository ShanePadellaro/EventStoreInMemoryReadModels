using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStoreReadModelBenchMark.ReadModels;
using EventStoreReadModelBenchMark.Repository;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EventStoreReadModelBenchMark.Controllers
{
    public class TaxController : ControllerBase
    {
        private readonly ITaxLedgerRepository _taxLedgerRepository;
        private readonly IMongoDatabase _mongoDatabase;

        public TaxController(ITaxLedgerRepository taxLedgerRepository,IMongoDatabase mongoDatabase)
        {
            _taxLedgerRepository = taxLedgerRepository;
            _mongoDatabase = mongoDatabase;
        }
        [HttpGet("api/v1/taxes")]
        public async Task<ActionResult<TaxLedger>> GetTaxesAsync()
        {
            return _taxLedgerRepository.GetTaxLedger();
        }
        
        [HttpGet("api/v1/taxes/{year}/{month}/transactions")]
        public async Task<ActionResult<IEnumerable<TaxReadModel>>> GetTaxesTransactionsAsync(int year,int month,[FromQuery] int page,[FromQuery]int pageSize)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            var transactions = await _mongoDatabase.GetCollection<TaxReadModel>("finance")
                .AsQueryable()
                .Where(t=>t.CreatedOn >= firstDayOfMonth && t.CreatedOn <= lastDayOfMonth && t.Type == FinanceType.Tax)
                .Skip(page <= 1?0:(page-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return transactions;
        }
    }
}