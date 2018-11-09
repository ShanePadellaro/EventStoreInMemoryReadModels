using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TransactionService.Api.Events;
using TransactionService.External;
using TransactionService.External.ValueObjects;

namespace TransactionServiceWriterSomething
{
        public class TransactionRequestListener : RabbitConsumer, IHostedService
        {
            private readonly IEventStoreConnection _eventStoreConnection;
            private readonly RabbitProducer _producer;

            public TransactionRequestListener(IEventStoreConnection eventStoreConnection)
                : base("transactionExchange", "transactions", "transactions.new", new RabbitConfig())
            {
                _eventStoreConnection = eventStoreConnection;
                _producer = new RabbitProducer(new RabbitConfig());
            }

            protected override async Task<bool> Handle(string message)
            {
                try
                {
                    var request =  Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionRequest>(message);
//                    var jsonBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new AccountCreditedEvent(transaction),new JsonSerializerSettings() {ContractResolver = new CamelCasePropertyNamesContractResolver()}));
//                    var e = new EventData(Guid.NewGuid(), "accountCredited", true,
//                        jsonBytes, null);
//                    var conn = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113")
//                    );
//                    await conn.ConnectAsync();

                    var @event = request.Transaction.TransactionType == TransactionType.Credit
                        ? (IDomainEvent<Transaction>)new AccountCreditedEvent(request.Transaction)
                        : (IDomainEvent<Transaction>)new AccountDebitedEvent(request.Transaction);

                    var d = @event.EventData;
                    
                    await _eventStoreConnection.AppendToStreamAsync(request.Transaction.AccountId, ExpectedVersion.Any,@event.EventData);
                }
                catch (Exception exception)
                {
                    _producer.Send("transactionExchange", 
                        $"Message: {message} \nException:{exception.Message}", 
                        "transactions.failed",
                        "transactions.failed");
                    return false;
                }

                return true;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
                //throw new NotImplementedException();
            }
        }
    }
