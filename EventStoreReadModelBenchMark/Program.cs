using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
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

        static async Task Main(string[] args)
        {
            _accounts = new Dictionary<string, Account>();
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
                .DoNotResolveLinkTos()
                .StartFromBeginning();


            _adminCredentials = new UserCredentials("admin", "Airfi2018Airfi2018");
            _streamName = "$ce-Account";
//            await conn.CreatePersistentSubscriptionAsync(streamName, "examplegroup", settings,
//                adminCredentials);
            var stopwatch = new Stopwatch();
            SubscribeCatchup();
//            SubscribePersisten();


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
                var amountString = (string) JObject.Parse(Encoding.UTF8.GetString(evt.Event.Data))["Balance"];
                var id = (string) JObject.Parse(Encoding.UTF8.GetString(evt.Event.Data))["Id"];

                if (!int.TryParse(amountString, out var amount))
                    return;

                await _database.GetCollection<Data>("Transactions")
                    .InsertOneAsync(new Data() {Balance = amount, Id = id});
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
                var eventType = e.Event.EventType;
                
                var account = !e.Event.EventType.Equals("accountOpened",StringComparison.OrdinalIgnoreCase) ? _accounts[e.Event.EventStreamId]: null;
                if(!e.Event.EventType.Equals("accountOpened",StringComparison.OrdinalIgnoreCase) && account == null)
                    return;
                    
                Console.WriteLine("Event:");

                switch (eventType)
                {
                    case "accountOpened":
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountCreatedEvent>(eventData);
                        _accounts.Add(e.Event.EventStreamId,
                            new Account()
                                {Balance = @event.AccountDetails.StartingBalance, Id = e.Event.EventStreamId});
                        Console.WriteLine(JsonConvert.SerializeObject(@event));

                    }
                        break;
                    case "accountDebited":
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountDebitedEvent>(eventData);
                        account.Balance -= @event.Transaction.Amount;
                        Console.WriteLine(JsonConvert.SerializeObject(@event));

                    }
                        break;
                    case "accountCredited":
                    {
                        var @event =
                            JsonConvert.DeserializeObject<AccountCreditedEvent>(eventData);
                        account.Balance += @event.Transaction.Amount;
                        account.CurrentStatement?.MakePayment(@event.Transaction.Amount);
                        Console.WriteLine(JsonConvert.SerializeObject(@event));


                    }
                        break;
                    case "statementCreated":
                    {
                        var @event =
                            JsonConvert.DeserializeObject<StatementCreatedEvent>(eventData);
                        var statement = new Statement(@event.BillingDate,@event.IncomingBalance)
                            {BillingDate = @event.BillingDate, IncomingBalance = @event.IncomingBalance,CurrentBalance = @event.IncomingBalance};
                        if(account.CurrentStatement != null)
                            account.Statements.Add(account.CurrentStatement);

                        account.CurrentStatement = statement;
                        Console.WriteLine(JsonConvert.SerializeObject(@event));

                    }
                        break;
                }

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
            _conn.SubscribeToStreamFrom(_streamName, 4352453,
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
            _conn.ConnectToPersistentSubscription(_streamName, "examplegroup", GotEvent, subscriptionDropped: Dropped);
        }

        public class Data
        {
            public int Balance { get; set; }
            public string Id { get; set; }
        }
    }
}