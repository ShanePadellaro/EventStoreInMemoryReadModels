using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TransactionService.External.ValueObjects;

namespace TransactionServiceWriterSomething
{
        public class TransactionRequestListener : RabbitConsumer, IHostedService
        {
            private readonly RabbitProducer _producer;

            public TransactionRequestListener(IConfigurationRoot configurationRoot)
                : base("transactionExchange", "transactions", "transactions.new", new RabbitConfig(configurationRoot))
            {
                _producer = new RabbitProducer(new RabbitConfig(configurationRoot));
            }

            protected override async Task<bool> Handle(string message)
            {
//                try
//                {
//                    var transaction =  Newtonsoft.Json.JsonConvert.DeserializeObject<Transaction>(message);
//                    var commandBus = Startup.ServiceProvider.GetService<ICommandBus>();
//
//                    var command = new CreditAccountCommand(new AccountId("account-439fa12b-18ac-4c82-87c6-ddc74f591284"),
//                        transaction);
//
//                    var result = await commandBus.PublishAsync(command,CancellationToken.None);
//                
//                }
//                catch (Exception exception)
//                {
//                    _producer.Send("transactionExchange", 
//                        $"Message: {message} \nException:{exception.Message}", 
//                        "transactions.failed",
//                        "transactions.failed");
//                    return false;
//                }

                return true;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
                //throw new NotImplementedException();
            }
        }
    }
