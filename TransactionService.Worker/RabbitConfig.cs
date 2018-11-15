using Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;

namespace TransactionService.Worker
{
    public class RabbitConfig : IRabbitConfig
    {
        public int Port { get; }
        public string Host { get; }
        public string Username { get; }
        public string Password { get; }

        public RabbitConfig(IConfiguration configurationRoot)
        {
            Port = int.Parse(configurationRoot["Config:RabbitMqPort"]);
            Host = configurationRoot["Config:RabbitMqHost"];
            Username = configurationRoot["Config:RabbitMqUsername"];
            Password = configurationRoot["Config:RabbitMqPassword"];
        }

    }
}