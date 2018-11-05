using EventStoreReadModelBenchMark.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace EventStoreReadModelBenchMark
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IAccountsRepository, AccountsRepository>();
            services.AddSingleton<ITaxLedgerRepository, TaxLedgerRepository>();
            services.AddSingleton<IFeeLedgerRepository, FeeLedgerRepository>();
            
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
            services.AddSingleton<IHostedService, ReadModelComposerBackgroundService>();

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
        }
    }
}