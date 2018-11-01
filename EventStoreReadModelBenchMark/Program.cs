using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStoreReadModelBenchMark.EventHandlers;
using EventStoreReadModelBenchMark.Events;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventStoreReadModelBenchMark
{
    class Program
    {
        private static IMongoDatabase _database;
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
            _adminCredentials = new UserCredentials("admin", "Airfi2018Airfi2018");
            _streamName = "$ce-Account";

            var url = new MongoUrl("mongodb://localhost:27017");
            var client = new MongoClient(url);
            _database = client.GetDatabase("concurrency");

//            await GetBalances();

            await CreateEventStoreConnection();
            await CreatePersistenSubscription();

            SubscribeCatchup();
            SubscribePersisten();


            while (true)
            {
                Console.WriteLine("waiting for events. press enter to exit");
                var input = Console.ReadLine();
                if (input.Equals("s"))
                {
                    Console.WriteLine("Creating Statements");
                    await GetBalances();
                }
            }
        }

        private static async Task GetBalances()
        {
            var result = await _database.ListCollectionsAsync();
            var collections = await result.ToListAsync();
            var names = collections.Select(x => x["name"]?.AsString)
                .Where(x => x.Contains("Account", StringComparison.OrdinalIgnoreCase)).ToList();

            var accountBalances = new Dictionary<string, long>();

            foreach (var name in names)
            {
                var transaction = _database.GetCollection<TransactionReadModel>(name).AsQueryable()
                    .Where(x => x.MetaData.EventCreated <= DateTime.Now).OrderByDescending(x => x.MetaData.EventCreated)
                    .FirstOrDefault();

                accountBalances.Add(
                    name.TrimEnd('t', 'r', 'a', 'n', 's', 'a', 'c', 't', 'i', 'o', 'n', 's').TrimEnd('-'),
                    transaction?.AccountBalance ?? 0);
            }

            foreach (var account in accountBalances)
            {
                var statementCreatedEvent = new StatementCreatedEvent(account.Value, DateTime.UtcNow);
                var json = JsonConvert.SerializeObject(statementCreatedEvent,
                    new JsonSerializerSettings() {ContractResolver = new CamelCasePropertyNamesContractResolver()});
                var jsonBytes = UTF8Encoding.ASCII.GetBytes(json);
                var e = new EventData(Guid.NewGuid(), "statementCreated", true,
                    jsonBytes, null);

                await _conn.AppendToStreamAsync(account.Key, ExpectedVersion.Any, e);
            }
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
                    : throw new Exception("Can't parse streamId");


                var lastTransaction = _database.GetCollection<TransactionReadModel>($"{accountId}-transactions")
                    .AsQueryable()
                    .OrderByDescending(x => x._id).FirstOrDefault();

                if (evt.OriginalEventNumber <= (lastTransaction?.MetaData?.OriginalEventNumber ?? 0))
                    return;

                var accountBalance = lastTransaction?.AccountBalance ?? 0L;


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
                readModel.MetaData.EventCreated = evt.Event.Created;

                await _database.GetCollection<TransactionReadModel>($"{accountId}-transactions")
                    .InsertOneAsync(readModel);
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

                if (!Enum.TryParse(e.Event.EventType, true, out DomainEventTypes eventType))
                    return;

                var accountId = e.Event.EventStreamId;
                if (!_accounts.TryGetValue(accountId, out var account))
                    _accounts.Add(accountId, account);


                if (account == null && eventType != DomainEventTypes.AccountOpened)
                    return;

                var thingy = new SomethingEventTuple(eventData, account, eventType, e.Event.EventStreamId);
                thingy = new AccountOpenedEventHandler()
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

        private static async Task CreatePersistenSubscription()
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

        private static async Task CreateEventStoreConnection()
        {
            _conn = EventStoreConnection.Create(
                ConnectionSettings.Create().KeepReconnecting(),
                ClusterSettings.Create().DiscoverClusterViaGossipSeeds().SetGossipSeedEndPoints(new[]
                    {
                        new IPEndPoint(IPAddress.Parse("52.151.78.42"), 2113),
                        new IPEndPoint(IPAddress.Parse("52.151.79.84"), 2113),
                        new IPEndPoint(IPAddress.Parse("51.140.14.214"), 2113)
                    })
                    .SetGossipTimeout(TimeSpan.FromMilliseconds(500)).Build());
            await _conn.ConnectAsync();
        }
    }
}