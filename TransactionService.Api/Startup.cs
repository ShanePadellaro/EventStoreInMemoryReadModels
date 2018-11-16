using System;
using System.Net;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.Swagger;
using TransactionService.Api.Repository;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace TransactionService.Api
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _environment = environment;
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            var userName = Configuration["Config:EventstoreUsername"];
            var password = Configuration["Config:EventstorePassword"];
            var host = Configuration["Config:EventstoreSingleNodeHost"];
            var port = Configuration["Config:EventstoreSingleNodePort"];
            var mongoConnectionString = Configuration["Config:MongoDbConnectionString"];
            
            if(string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(host)
               || string.IsNullOrWhiteSpace(port) || string.IsNullOrWhiteSpace(mongoConnectionString))
                throw new Exception("A configuration value is missing, check the configuration.");

            var userCredentials = new UserCredentials(userName,
                password);
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IAccountsRepository, AccountsRepository>();
            services.AddSingleton<ITaxLedgerRepository, TaxLedgerRepository>();
            services.AddSingleton<IFeeLedgerRepository, FeeLedgerRepository>();
            services.AddTransient(s => userCredentials);

            if (_environment.IsProduction())
            {
                services.AddSingleton(f =>
                {
                    var conn = EventStoreConnection.Create(
                        ConnectionSettings.Create()
                            .SetDefaultUserCredentials(userCredentials)
                            .KeepReconnecting().KeepRetrying(),
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
                            .KeepReconnecting()
                        .KeepRetrying().UseConsoleLogger().Build(),
                        new Uri(
                            $"tcp://{userName}:{password}@{host}:{port}"));
                    conn.ConnectAsync().Wait();
                    return conn;
                });
                
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = "TransactionService API", Version = "v1" });
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


            services.AddSingleton<IHostedService, ReadModelComposerBackgroundService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TransactionService API V1");
                });
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}