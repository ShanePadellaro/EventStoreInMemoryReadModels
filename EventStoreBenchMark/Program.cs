using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using TransactionService.Api.ValueObjects;
using TransactionService.Api;
using TransactionService.Api.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TransactionService.External.ValueObjects;
using KeyValuePair = System.Collections.Generic.KeyValuePair;

namespace EventStoreBenchMark
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var connectionString = "ConnectTo=tcp://admin:Airfi2018Airfi2018@http://52.151.85.8:1113; HeartBeatTimeout=500";
//            var conn = EventStoreConnection.Create(new Uri("tcp://admin:changeit@http://52.151.85.8:1113")
//            var conn = EventStoreConnection.Create(new Uri("tcp://admin:changeit@52.151.85.8:1113")
//
//            );
            
            var conn = EventStoreConnection.Create(
                ConnectionSettings.Create().KeepReconnecting(),
                ClusterSettings.Create().DiscoverClusterViaGossipSeeds().SetGossipSeedEndPoints(new []
                    {
                        new IPEndPoint(IPAddress.Parse("52.151.78.42"), 2113),
                        new IPEndPoint(IPAddress.Parse("52.151.79.84"), 2113),
                        new IPEndPoint(IPAddress.Parse("51.140.14.214"), 2113)
                    }).SetGossipTimeout(TimeSpan.FromMilliseconds(500)).Build());
                
                 
            await conn.ConnectAsync();
//            var accountId = $"Account-{Guid.NewGuid()}";
            var accountId = "Account-4d4158cd-a6ac-49cf-a41e-d11f86ccbbf6";
//            var accountId = "test1";


//            await conn.DeleteStreamAsync("Transactions", ExpectedVersion.Any,true);

            var watch = new Stopwatch();
            watch.Start();
            var tasks = new List<Task>();
            var rnd = new Random();
            
            var props = new List<Dictionary<string, object>>();
//            props.Add(new Dictionary<string, object>() {{"item1", "value1"}});
//            props.Add(new Dictionary<string, object>() {{"item2", "value2"}});

            var subfee1 = new TransactionService.External.ValueObjects.KeyValuePair("myKey", "MyValue");
            var companyId = "b3e4bf26-c93b-41f6-adf1-27b85fa82c91";
            var subfee2 = new Fee(companyId, "MyLabel", 50, "USD", "0.9", 0, 0);
            var subfees = new List<Fee>() {subfee2};
            var keyValueParis = new List<TransactionService.External.ValueObjects.KeyValuePair>() {subfee1};
            var item = new TransactionItem(100, "B2C Renewal", 1, keyValueParis, subfees);

            var transaction = new Transaction("T-00001", Guid.NewGuid().ToString(), "Transaction", "B2C Renewal", 100,
                            50,
                            new DateTime(rnd.Next(2015, 2018), rnd.Next(1, 12), rnd.Next(1, 28)), 20, "GBR", "GBP",
                            new List<TransactionItem>() {item});

                        var data = new Data() {Balance = 100, Id = Guid.NewGuid().ToString()};
            var jsonBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new AccountCreditedEvent(transaction),new JsonSerializerSettings() {ContractResolver = new CamelCasePropertyNamesContractResolver()}));
             
            
            for (int j = 0; j < 100; j++)
            {
                for (int i = 0; i < 100; i++)
                {
                   

                    tasks.Add(Task.Run(async () =>
                    {
                        var e = new EventData(Guid.NewGuid(), "accountCredited", true,
                            jsonBytes, null);
//                    var conn = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113")
//                    );
//                    await conn.ConnectAsync();
                        await conn.AppendToStreamAsync(accountId, ExpectedVersion.Any, e);
                    }));
                }

//            while (tasks.Count != 0)
//            {
////                Thread.Sleep(TimeSpan.FromSeconds(1));
//                var ts = tasks.Take(50).ToList();
//                ts.ForEach(t => t.Start());
//                Task.WaitAll(ts.ToArray());
//
//                tasks.RemoveAll(t => ts.Contains(t));
//            }

                Task.WaitAll(tasks.ToArray());
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            Console.ReadLine();
        }
    }

    public class Data
    {
        public int Balance { get; set; }
        public string Id { get; set; }
    }
}