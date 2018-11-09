using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace TransactionServiceWriterSomething
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddSingleton(f =>
            {
                var conn = EventStoreConnection.Create(
                    ConnectionSettings.Create().KeepReconnecting(),
                    ClusterSettings.Create().DiscoverClusterViaGossipSeeds().SetGossipSeedEndPoints(new[]
                        {
                            new IPEndPoint(IPAddress.Parse("52.151.78.42"), 2113),
                            new IPEndPoint(IPAddress.Parse("52.151.79.84"), 2113),
                            new IPEndPoint(IPAddress.Parse("51.140.14.214"), 2113)
                        })
                        .SetGossipTimeout(TimeSpan.FromMilliseconds(500)).Build(),"TransactionService");

                conn.ConnectAsync().Wait();
                return conn;
            });
            
            services.AddTransient<IMongoDatabase>(s =>
                {
                    var conventionPack = new  ConventionPack {new CamelCaseElementNameConvention()
                        ,new EnumRepresentationConvention(BsonType.String)};
                    ConventionRegistry.Register("default", conventionPack, t => true);
                    var url = new MongoUrl("mongodb://localhost:27017");
                    var client = new MongoClient(url);
                    return client.GetDatabase("transactions");
                }
            );
            
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddHangfire(g =>
            {
                g.UseMongoStorage("mongodb://localhost:27017", "hangfire");
            });

            services.AddHostedService<TransactionRequestListener>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            

            app.UseHttpsRedirection();
            app.UseMvc();
            
            app.UseHangfireServer();
            app.UseHangfireDashboard();
            
            Hangfire.RecurringJob.AddOrUpdate<StatementCreator>(j=>j.Run(),Cron.Monthly(1,4));

        }
    }
}