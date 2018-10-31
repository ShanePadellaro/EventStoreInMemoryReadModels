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

        static async Task Main(string[] args)
        {

            _accounts = new Dictionary<string, Account>();
            _accountBalances = new Dictionary<string, long>();

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
                .StartFrom(4352482);


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


//            conn.ConnectToPersistentSubscription(streamName, "examplegroup", (_, x) =>
//            {
////                var data = Encoding.ASCII.GetString(x.Event.Data);
//                var amountString = (string) JObject.Parse(Encoding.UTF8.GetString(x.Event.Data))["Balance"];
//                if (int.TryParse(amountString, out var amount))
//                {
//                    total += amount;
//                }
//
//                Console.WriteLine("Received: " + x.Event.EventStreamId + ":" + x.Event.EventNumber);
//                Console.WriteLine(total);
//            }, (sub, reason, ex) => { }, adminCredentials);

            Console.WriteLine("waiting for events. press enter to exit");

            Console.ReadLine();
        }

        private static async Task GotEvent(EventStorePersistentSubscriptionBase sub, ResolvedEvent evt, int? value)
        {
            try
            {
                if (!Enum.TryParse(evt.Event.EventType,true,out DomainEventTypes eventType))
                    return;
                
                if(eventType != DomainEventTypes.AccountCredited && eventType != DomainEventTypes.AccountDebited)
                    return;
                
                
                var accountId = evt.Event.EventStreamId.Contains("Account", StringComparison.OrdinalIgnoreCase)
                    ? evt.Event.EventStreamId
                    : throw new Exception("Can't parse streanId");
                
                var summary = _database.GetCollection<AccountsSummary>("accountSummarys")
                    .Find(s=>s._id.Equals("5bd9c1fddfd1ea0dcece6f26"))
                    .ToList().FirstOrDefault() ?? new AccountsSummary();
                
                if(evt.OriginalEventNumber <= summary.EventNumber)
                    return;
                
                var accountBalance = 0L;
                if (summary.AccountBalances.TryGetValue(accountId, out var balance))
                {
                    accountBalance = balance;
                }
                else
                {
                    summary.AccountBalances.Add(accountId,0);
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
                        readModel = new TransactionReadModel(@event.Transaction,accountBalance);
                    }
                        break;
                    case DomainEventTypes.AccountCredited:
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountCreditedEvent>(eventJson);
                        accountBalance += @event.Transaction.Amount;
                        readModel = new TransactionReadModel(@event.Transaction,accountBalance);

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
                await _database.GetCollection<AccountsSummary>("accountSummarys").ReplaceOneAsync<AccountsSummary>(x => x._id.Equals(summary._id),
                    summary,new UpdateOptions() {IsUpsert = true});
                
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
                
                if (!Enum.TryParse(e.Event.EventType,true,out DomainEventTypes eventType))
                    return;


                var account = eventType == DomainEventTypes.AccountOpened
                    ? null
                    : _accounts[e.Event.EventStreamId]; 
                    
                
                if (account == null && eventType != DomainEventTypes.AccountOpened)
                    return;


                switch (eventType)
                {
                    case DomainEventTypes.AccountOpened:
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountOpenedEvent>(eventData);
                        _accounts.Add(e.Event.EventStreamId,
                            new Account()
                                {Balance = @event.AccountDetails.StartingBalance, Id = e.Event.EventStreamId});
                        Console.WriteLine(JsonConvert.SerializeObject(@event));
                    }
                        break;
                    case DomainEventTypes.AccountDebited:
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountDebitedEvent>(eventData);
                        account.Balance -= @event.Transaction.Amount;
                        Console.WriteLine(JsonConvert.SerializeObject(@event));
                    }
                        break;
                    case DomainEventTypes.AccountCredited:
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountCreditedEvent>(eventData);
                        account.Balance += @event.Transaction.Amount;
                        account.CurrentStatement?.MakePayment(@event.Transaction.Amount);
                        Console.WriteLine(JsonConvert.SerializeObject(@event));
                    }
                        break;
                    case DomainEventTypes.StatementCreated:
                    {
                        var @event =
                            JsonConvert.DeserializeObject<StatementCreatedEvent>(eventData);
                        var statement = new Statement(@event.BillingDate, @event.IncomingBalance)
                        {
                            BillingDate = @event.BillingDate, IncomingBalance = @event.IncomingBalance,
                            CurrentBalance = @event.IncomingBalance
                        };
                        if (account.CurrentStatement != null)
                            account.Statements.Add(account.CurrentStatement);

                        account.CurrentStatement = statement;
                        Console.WriteLine(JsonConvert.SerializeObject(@event));
                    }
                        break;
                }
                Console.WriteLine(e.OriginalEventNumber);
                Console.WriteLine(e.Event.EventNumber);
                Console.WriteLine(JsonConvert.SerializeObject(account));
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
            _conn.SubscribeToStreamFrom(_streamName, 4352482,
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
            _conn.ConnectToPersistentSubscription(_streamName, "transactionReadModelWriter", GotEvent,userCredentials:_adminCredentials, subscriptionDropped: Dropped);
        }

    }

    public class AccountsSummary
    {
        public string _id => "5bd9c1fddfd1ea0dcece6f26";
        public Dictionary<string, long> AccountBalances { get; set; }
        public long EventNumber { get; set; }

        public AccountsSummary(Dictionary<string,long> accountBalances)
        {
            AccountBalances = accountBalances;
        }

        public AccountsSummary()
        {
           AccountBalances=new Dictionary<string, long>();
        }
    }
}