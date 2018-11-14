using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using TransactionService.Api.Domain;
using TransactionService.Api.EventHandlers;
using TransactionService.Api.Events;
using TransactionService.Api.ReadModels;
using TransactionService.Api.Repository;
using TransactionService.Api.ValueObjects;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api
{
    class ReadModelComposerBackgroundService : BackgroundService
    {
        private readonly IAccountsRepository _accountsRepository;
        private static IMongoDatabase _database;
        private readonly IHostingEnvironment _env;
        private static string _streamName;
        private static IEventStoreConnection _conn;
        private static UserCredentials _adminCredentials;
        private static Dictionary<string, Account> _accounts;
        private static Dictionary<string, long> _accountBalances;
        private static long _startPosition;
        private static TaxLedger _taxLedger;
        private static FeeLedger _feeLedger;
        private static long _lastEventNumber;

        public ReadModelComposerBackgroundService(IAccountsRepository accountsRepository, IMongoDatabase mongoDatabase
            , IHostingEnvironment env,ITaxLedgerRepository taxLedgerRepository,IFeeLedgerRepository feeLedgerRepository,
            IEventStoreConnection eventStoreConnection,UserCredentials userCredentials)
        {
            _accountsRepository = accountsRepository;
            _database = mongoDatabase;
            _env = env;
            _taxLedger = taxLedgerRepository.GetTaxLedger();
            _feeLedger = feeLedgerRepository.GetFeeLedger();
            _conn = eventStoreConnection;
            _adminCredentials = userCredentials;
        }

        private async Task Start()
        {
            _accounts = _accountsRepository.GetAccounts();
            _accountBalances = new Dictionary<string, long>();
            _startPosition = 0;
            _streamName = "$ce-Account";

            await CreatePersistenSubscription();

            SubscribeCatchup();
            SubscribePersisten();

        }

        private async Task GotEvent(EventStorePersistentSubscriptionBase sub, ResolvedEvent evt, int? value)
        {
            
                if (!Enum.TryParse(evt.Event.EventType, true, out DomainEventTypes eventType))
                    return;

                if (eventType != DomainEventTypes.AccountCredited && eventType != DomainEventTypes.AccountDebited && eventType != DomainEventTypes.AccountOpened)
                    return;

                var accountId = evt.Event.EventStreamId.Contains("Account", StringComparison.OrdinalIgnoreCase)
                    ? evt.Event.EventStreamId
                    : throw new Exception("Can't parse streamId");


                var lastTransaction = _database.GetCollection<TransactionReadModel>($"{accountId}-transactions")
                    .AsQueryable()
                    .OrderByDescending(x => x._id).FirstOrDefault();

                if (evt.OriginalEventNumber <= (lastTransaction?.MetaData?.OriginalEventNumber ?? 0))
                    return;

                var accountBalance = lastTransaction?.AccountBalance ?? 0L;


                var eventJson = Encoding.UTF8.GetString(evt.Event.Data);
                Transaction transaction = null;
                
                switch (eventType)
                {
                    case DomainEventTypes.AccountDebited:
                    {
                        transaction=
                            JsonConvert.DeserializeObject<Transaction>(eventJson);
                        accountBalance -= transaction.Amount;
                    }
                        break;
                    case DomainEventTypes.AccountCredited:
                    {
                        transaction =
                            JsonConvert.DeserializeObject<Transaction>(eventJson);
                        accountBalance += transaction.Amount;
                    }
                        break;
                    case DomainEventTypes.AccountOpened:
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountDetails>(eventJson);
                        var account = new Account(@event);
                        await _database.GetCollection<Account>("accounts").InsertOneAsync(account);
                        
                        return;
                    }
                    default:
                        throw new Exception("Wrong EvenType, should not be here");
                }
                
                var readModel = new TransactionReadModel(transaction,evt, accountBalance);
                var taxReadModel = new TaxReadModel(readModel).ToBsonDocument();
                var feeModels = readModel.Transaction.TransactionItems.SelectMany(x => x.SubFees
                    .Select(y=>new FeeReadModel(y,transaction,evt).ToBsonDocument())).ToList();
                feeModels.Add(taxReadModel);
                
                await _database.GetCollection<TransactionReadModel>($"{accountId}-transactions")
                    .InsertOneAsync(readModel);
                await _database.GetCollection<BsonDocument>("finance")
                    .InsertManyAsync(feeModels.AsEnumerable());
           
        }

        private async Task GotEvent(EventStoreCatchUpSubscription sub, ResolvedEvent e)
        {
         
                var eventData = Encoding.UTF8.GetString(e.Event.Data);

                if (!Enum.TryParse(e.Event.EventType, true, out DomainEventTypes eventType))
                    return;

                var accountId = e.Event.EventStreamId;
                if (!_accounts.TryGetValue(accountId, out var account))
                    _accounts.Add(accountId, account);


                if (account == null && eventType != DomainEventTypes.AccountOpened)
                    return;


                var state = new State(account, _taxLedger, _feeLedger, eventType, eventData, e.Event.EventStreamId);
                state = new AccountOpenedEventHandler()
                    .Execute(new AccountDebitedEventHandler()
                        .Execute(new AccountCreditedEventHandler()
                            .Execute(new StatementCreatedEventHandler()
                                .Execute(state))));

                _accounts[accountId] = state.Account;


                if (_env.IsDevelopment())
                {
                    Console.WriteLine(e.OriginalEventNumber);
                    Console.WriteLine(e.Event.EventNumber);
                    Console.WriteLine(eventType);
                    Console.WriteLine(JsonConvert.SerializeObject(_accounts[accountId]));
                    Console.WriteLine(JsonConvert.SerializeObject(_taxLedger));
                    Console.WriteLine(JsonConvert.SerializeObject(_feeLedger));
                }

                _lastEventNumber = e.OriginalEventNumber;
          
        }

        private void Dropped(EventStoreCatchUpSubscription sub, SubscriptionDropReason reason, Exception ex)
        {
//            SubscribeCatchup(_lastEventNumber);
            Console.WriteLine($"Catchup sub dropped {reason}");
        }

        private void SubscribeCatchup(long lastEventNumber = -1)
        {
            var position = lastEventNumber == -1 ? 
                _startPosition - 1 < 0 ? 0:_startPosition -1 
                : lastEventNumber;
            _conn.SubscribeToStreamFrom(_streamName, position,
                new CatchUpSubscriptionSettings(10000, 1000, true, true, "ReadModel"), GotEvent,
                subscriptionDropped: Dropped);
        }

        private void Dropped(EventStorePersistentSubscriptionBase sub, SubscriptionDropReason reason,
            Exception ex)
        {
            Console.WriteLine($"Persisten sub dropped: {reason}");
            SubscribePersisten();

        }

        private void SubscribePersisten()
        {
            _conn.ConnectToPersistentSubscription(_streamName, "transactionReadModelWriter", GotEvent,
                subscriptionDropped: Dropped);
        }

        private async Task CreatePersistenSubscription()
        {
            PersistentSubscriptionSettings settings = PersistentSubscriptionSettings.Create()
                .ResolveLinkTos()
                .StartFrom(_startPosition);
            try
            {
                await _conn.CreatePersistentSubscriptionAsync(_streamName, "transactionReadModelWriter", settings,
                    _adminCredentials);
                Console.WriteLine("PersistentSubscription created");
            }
            catch (Exception e)
            {
                Console.WriteLine("PersistentSubscription already exists, using existing one.");
            }
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Start();
        }
    }
}