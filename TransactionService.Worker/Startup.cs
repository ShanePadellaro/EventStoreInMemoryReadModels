using System;
using System.Net;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace TransactionService.Worker
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;

        public Startup(IConfiguration configuration,IHostingEnvironment environment)
        {
            _environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            ValidateConfiguration(Configuration);
            
            var userName = Configuration["Config:EventstoreUsername"];
            var password = Configuration["Config:EventstorePassword"];
            var host = Configuration["Config:EventstoreSingleNodeHost"];
            var port = Configuration["Config:EventstoreSingleNodePort"];
            var mongoConnectionString = Configuration["Config:MongoDbConnectionString"];
          
            var userCredentials = new UserCredentials(userName,
                password);
            
            services.AddSingleton(Configuration);

            if (_environment.IsProduction())
            {
                services.AddSingleton(f =>
                {
                    var conn = EventStoreConnection.Create(
                        ConnectionSettings.Create()
                            .SetDefaultUserCredentials(userCredentials)
                            .KeepReconnecting(),
                        ClusterSettings.Create().DiscoverClusterViaGossipSeeds().SetGossipSeedEndPoints(new[]
                            {
                                new IPEndPoint(IPAddress.Parse("52.151.78.42"), 2113),
                                new IPEndPoint(IPAddress.Parse("52.151.79.84"), 2113),
                                new IPEndPoint(IPAddress.Parse("51.140.14.214"), 2113)
                            })
                            .SetGossipTimeout(TimeSpan.FromMilliseconds(500)).Build(), "TransactionService");

                    conn.ConnectAsync().Wait();
                    return conn;
                });
            }
            else
            {
                services.AddSingleton(f =>
                {
                    var conn = EventStoreConnection.Create(
                        ConnectionSettings.Create()
                            .SetDefaultUserCredentials(userCredentials)
                            .KeepReconnecting(),
                        new Uri(
                            $"tcp://{userName}:{password}@{host}:{port}"));
                    conn.ConnectAsync().Wait();
                    return conn;
                });
            }
            
            services.AddTransient<IMongoDatabase>(s =>
                {
                    var conventionPack = new ConventionPack
                        {new CamelCaseElementNameConvention(), new EnumRepresentationConvention(BsonType.String)};
                    ConventionRegistry.Register("default", conventionPack, t => true);
                    var url = new MongoUrl(mongoConnectionString);
                    var client = new MongoClient(url);
                    return client.GetDatabase("transactions");
                }
            );
            
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddHangfire(g =>
            {
                g.UseMongoStorage(mongoConnectionString, "hangfire");
            });

            services.AddHostedService<TransactionRequestListener>();
        }

        private void ValidateConfiguration(IConfiguration configuration)
        {
            if(string.IsNullOrWhiteSpace(Configuration["Config:EventstoreUsername"]) 
               || string.IsNullOrWhiteSpace(Configuration["Config:EventstorePassword"]) 
               || string.IsNullOrWhiteSpace(Configuration["Config:EventstoreSingleNodeHost"])
               || string.IsNullOrWhiteSpace(Configuration["Config:EventstoreSingleNodePort"]) 
               || string.IsNullOrWhiteSpace(Configuration["Config:MongoDbConnectionString"])
               || string.IsNullOrWhiteSpace(Configuration["Config:RabbitMqPort"])
               || string.IsNullOrWhiteSpace(Configuration["Config:RabbitMqHost"])
               || string.IsNullOrWhiteSpace(Configuration["Config:RabbitMqUsername"])
               || string.IsNullOrWhiteSpace(Configuration["Config:RabbitMqPassword"]))
                    throw new Exception("A configuration value is missing, check the configuration.");
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