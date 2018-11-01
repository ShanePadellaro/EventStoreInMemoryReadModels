using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStoreReadModelBenchMark.ValueObjects;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Formatting = System.Xml.Formatting;

namespace EventStoreReadModelBenchMark
{
    class Program
    {
        private static IMongoDatabase _database;
        private static int _total;
        private static string _streamName;
        private static IEventStoreConnection _conn;
        private static UserCredentials _adminCredentials;
        private static Dictionary<string, Account> _accounts;
        private static Dictionary<string, long> _accountBalances;
        private static long _startPosition;

        static async Task Main(string[] args)
        {
            _accounts = new Dictionary<string, Account>();
            _accountBalances = new Dictionary<string, long>();
            _startPosition = 4352483;

            var url = new MongoUrl("mongodb://localhost:27017");
            var client = new MongoClient(url);
            _database = client.GetDatabase("concurrency");


            Console.WriteLine("Hello World!");
            _total = 0;
            _conn = EventStoreConnection.Create(
                ConnectionSettings.Create().KeepReconnecting(),
                ClusterSettings.Create().DiscoverClusterViaGossipSeeds().SetGossipSeedEndPoints(new[]
                {
                    new IPEndPoint(IPAddress.Parse("52.151.78.42"), 2113),
                    new IPEndPoint(IPAddress.Parse("52.151.79.84"), 2113),
                    new IPEndPoint(IPAddress.Parse("51.140.14.214"), 2113)
                }).SetGossipTimeout(TimeSpan.FromMilliseconds(500)).Build());
            await _conn.ConnectAsync();

            PersistentSubscriptionSettings settings = PersistentSubscriptionSettings.Create()
                .ResolveLinkTos()
                .StartFrom(_startPosition);


            _adminCredentials = new UserCredentials("admin", "Airfi2018Airfi2018");
            _streamName = "$ce-Account";

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


            var stopwatch = new Stopwatch();
            SubscribeCatchup();
            SubscribePersisten();

            Console.WriteLine("waiting for events. press enter to exit");

            Console.ReadLine();
        }

        private static async Task GotEvent(EventStorePersistentSubscriptionBase sub, ResolvedEvent evt, int? value)
        {
            try
            {
                if (!Enum.TryParse(evt.Event.EventType, true, out DomainEventTypes eventType))
                    return;

                if (eventType != DomainEventTypes.AccountCredited && eventType != DomainEventTypes.AccountDebited)
                    return;


                var accountId = evt.Event.EventStreamId.Contains("Account", StringComparison.OrdinalIgnoreCase)
                    ? evt.Event.EventStreamId
                    : throw new Exception("Can't parse streanId");

                var summary = _database.GetCollection<AccountsSummary>("accountSummarys")
                                  .Find(s => s._id.Equals("5bd9c1fddfd1ea0dcece6f26"))
                                  .ToList().FirstOrDefault() ?? new AccountsSummary();

                if (evt.OriginalEventNumber <= summary.EventNumber)
                    return;

                var accountBalance = 0L;
                if (summary.AccountBalances.TryGetValue(accountId, out var balance))
                {
                    accountBalance = balance;
                }
                else
                {
                    summary.AccountBalances.Add(accountId, 0);
                }

                var eventJson = Encoding.UTF8.GetString(evt.Event.Data);
                TransactionReadModel readModel = null;

                switch (eventType)
                {
                    case DomainEventTypes.AccountDebited:
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountDebitedEvent>(eventJson);
                        accountBalance -= @event.Transaction.Amount;
                        readModel = new TransactionReadModel(@event.Transaction, accountBalance);
                    }
                        break;
                    case DomainEventTypes.AccountCredited:
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountCreditedEvent>(eventJson);
                        accountBalance += @event.Transaction.Amount;
                        readModel = new TransactionReadModel(@event.Transaction, accountBalance);
                    }
                        break;
                    default:
                        throw new Exception("Wrong EvenType, should not be here");
                }

                readModel.MetaData.EventNumber = evt.Event.EventNumber;
                readModel.MetaData.OriginalEventNumber = evt.OriginalEventNumber;

                await _database.GetCollection<TransactionReadModel>($"{accountId}-transactions")
                    .InsertOneAsync(readModel);

                summary.AccountBalances[accountId] = accountBalance;
                summary.EventNumber = evt.OriginalEventNumber;
                await _database.GetCollection<AccountsSummary>("accountSummarys").ReplaceOneAsync<AccountsSummary>(
                    x => x._id.Equals(summary._id),
                    summary, new UpdateOptions() {IsUpsert = true});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task GotEvent(EventStoreCatchUpSubscription sub, ResolvedEvent e)
        {
            try
            {
                var eventData = Encoding.UTF8.GetString(e.Event.Data);
                var eventMeta = Encoding.UTF8.GetString(e.Event.Metadata);

                if (!Enum.TryParse(e.Event.EventType, true, out DomainEventTypes eventType))
                    return;


//                var account = eventType == DomainEventTypes.AccountOpened
//                    ? null
//                    : _accounts[e.Event.EventStreamId];
                var accountId = e.Event.EventStreamId;
                if(!_accounts.TryGetValue(accountId, out var account))
                    _accounts.Add(accountId,account);


                if (account == null && eventType != DomainEventTypes.AccountOpened)
                    return;
                
                var thingy = new SomethingEventTuple(eventData,account,eventType,e.Event.EventStreamId);
                var accountState = new AccountOpenedEventHandler()
                    .Execute(new AccountDebitedEventHandler()
                    .Execute(new AccountCreditedEventHandler()
                    .Execute(new StatementCreatedEventHandler()
                    .Execute(thingy))));

                _accounts[accountId] = thingy.Account;

                Console.WriteLine(e.OriginalEventNumber);
                Console.WriteLine(e.Event.EventNumber);
                Console.WriteLine(eventType);
                Console.WriteLine(JsonConvert.SerializeObject(_accounts[accountId]));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static void Dropped(EventStoreCatchUpSubscription sub, SubscriptionDropReason reason, Exception ex)
        {
            SubscribeCatchup();
        }

        private static void SubscribeCatchup()
        {
            _conn.SubscribeToStreamFrom(_streamName, _startPosition - 1,
                new CatchUpSubscriptionSettings(500, 500, true, true, "ReadModel"), GotEvent,
                subscriptionDropped: Dropped, userCredentials: _adminCredentials);
        }

        private static void Dropped(EventStorePersistentSubscriptionBase sub, SubscriptionDropReason reason,
            Exception ex)
        {
            SubscribePersisten();
        }

        private static void SubscribePersisten()
        {
            _conn.ConnectToPersistentSubscription(_streamName, "transactionReadModelWriter", GotEvent,
                userCredentials: _adminCredentials, subscriptionDropped: Dropped);
        }
    }
}