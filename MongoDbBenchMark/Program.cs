using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDbBenchMark
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = new MongoUrl("mongodb://localhost:27017");
            var client = new MongoClient(new MongoClientSettings()
            {
                Server = url.Server,
                MaxConnectionPoolSize = 500,
                WaitQueueSize = 20000
            });
            
            var database = client.GetDatabase("concurrency");
           
//            var doc = new MongTest(){_id = "1"};
//
//            database.GetCollection<MongTest>("mongTest").InsertOne(doc);
            
            var watch = new Stopwatch();
            watch.Start();
            for (int y = 0; y < 10000; y++)
            {
                var tasks = new List<Task>();
                for (int i = 0; i < 150; i++)
                {
//                tasks.Add(new Task(()=>database.GetCollection<MongTest>("mongTest").FindOneAndUpdate<MongTest>(p => p._id == "1",
//                    new UpdateDefinitionBuilder<MongTest>().Inc(x => x.value, value: 1),
//                    new FindOneAndUpdateOptions<MongTest> {IsUpsert = true, ReturnDocument = ReturnDocument.After})));
//                
                    var task = Task.Run(async () => await
                        database.GetCollection<MongTest>("insertBenchmark").InsertOneAsync(new MongTest()).ConfigureAwait(false));
                    tasks.Add(task);
                }
                
                Task.WaitAll(tasks.ToArray());
            }
            
            
            
//            while (tasks.Count != 0)
//            {
////                Thread.Sleep(TimeSpan.FromSeconds(1));
//                var ts = tasks.Take(2).ToList();
//                ts.ForEach(t => t.Start());
//                Task.WaitAll(ts.ToArray());
//
//                tasks.RemoveAll(t => ts.Contains(t));
//            }

            
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            Console.ReadLine();
        
        }
    }
    
    public class MongTest
    {
//        public string _id { get; set; }
        public int value { get; set; }
    }
}