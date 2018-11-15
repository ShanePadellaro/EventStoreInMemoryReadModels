using System;
using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;
using MongoDB.Driver;
using TransactionService.Api.Events;
using TransactionService.Api.ReadModels;

namespace TransactionService.Worker
{
    public class StatementCreator
    {
        private readonly IMongoDatabase _database;
        private readonly IEventStoreConnection _conn;

        public StatementCreator(IMongoDatabase database, IEventStoreConnection conn)
        {
            _database = database;
            _conn = conn;
        }

        public void Run()
        {
            var result = _database.ListCollections();
            var collections = result.ToList();
            var names = collections.Select(x => x["name"]?.AsString)
                .Where(x => x.Contains("Account-", StringComparison.OrdinalIgnoreCase)).ToList();

            var accountBalances = new Dictionary<string, long>();

#if DEBUG
                var todaysDate = DateTime.UtcNow.AddMonths(1);
#else
                var todaysDate = DateTime.UtcNow;
#endif
            
            var firstDayOfMonth = new DateTime(todaysDate.Year, todaysDate.AddMonths(-1).Month, 1, 23, 59, 59);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            foreach (var name in names)
            {
                var transaction = _database.GetCollection<TransactionReadModel>(name).AsQueryable()
                    .Where(x => x.MetaData.EventCreated <= lastDayOfMonth)
                    .OrderByDescending(x => x.MetaData.EventCreated)
                    .FirstOrDefault();

                accountBalances.Add(
                    name.TrimEnd('t', 'r', 'a', 'n', 's', 'a', 'c', 't', 'i', 'o', 'n', 's').TrimEnd('-'),
                    transaction?.AccountBalance == null || transaction.AccountBalance > -1
                        ? 0
                        : transaction.AccountBalance);
            }

            foreach (var account in accountBalances)
            {
                var statementCreatedEvent = new StatementCreatedEvent(account.Value, lastDayOfMonth);
                _conn.AppendToStreamAsync(account.Key, ExpectedVersion.Any, statementCreatedEvent.EventData).Wait();
            }
        }
    }
}