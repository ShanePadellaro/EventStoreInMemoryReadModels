using System;
using System.Threading;
using System.Threading.Tasks;
using Airfi.Transactions.Client;
using Airfi.Transactions.Client.ValueObjects;
using EventStore.ClientAPI;
using Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TransactionService.Api.Domain;
using TransactionService.Api.Events;

namespace TransactionService.Worker
{
        public class TransactionRequestListener : RabbitConsumer, IHostedService
        {
            private readonly IEventStoreConnection _eventStoreConnection;
            private readonly IMongoDatabase _mongoDatabase;
            private readonly RabbitProducer _producer;

            public TransactionRequestListener(IEventStoreConnection eventStoreConnection,IMongoDatabase mongoDatabase,
                IConfiguration configuration)
                : base("transactionsExchange", "transactionsQueue", "transactions.new", new RabbitConfig(configuration))
            {
                _eventStoreConnection = eventStoreConnection;
                _mongoDatabase = mongoDatabase;
                _producer = new RabbitProducer(new RabbitConfig(configuration));
            }

            protected async Task<bool> Handle(string message)
            {
                try
                {
                    var request =  Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionRequest>(message);
                    
                    if(!request.ApiVersion.Equals("1"))
                        throw new Exception("Api version");
                    
                    var account = await _mongoDatabase.GetCollection<Account>("accounts").AsQueryable()
                        .Where(x => x.Id == request.Transaction.AccountId).FirstOrDefaultAsync();
                    
                    if(account == null)
                        throw new Exception("AccountId does not exist");
                    
                    var @event = request.Transaction.TransactionType == TransactionType.Credit
                        ? (IDomainEvent<Transaction>)new AccountCreditedEvent(request.Transaction)
                        : (IDomainEvent<Transaction>)new AccountDebitedEvent(request.Transaction);

                    await _eventStoreConnection.AppendToStreamAsync(request.Transaction.AccountId, ExpectedVersion.Any,@event.EventData);
                }
                catch (Exception exception)
                {
                    _producer.Send("transactionExchange", 
                        $"Message: {message} \nException:{exception.Message}", 
                        "transactions.failed",
                        "transactions.failed",DeliveryMode.Persistent);
                    return false;
                }

                return true;
            }


            public async Task StartAsync(CancellationToken cancellationToken)
            {
                await StartAsync(Handle);
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
