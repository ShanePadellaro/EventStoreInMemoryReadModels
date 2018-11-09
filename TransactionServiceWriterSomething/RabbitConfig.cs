using Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;

namespace TransactionServiceWriterSomething
{
    public class RabbitConfig : IRabbitConfig
    {
        public int Port { get; }
        public string Host { get; }
        public string Username { get; }
        public string Password { get; }

        public RabbitConfig(IConfigurationRoot configurationRoot)
        {
            Port = int.Parse(configurationRoot["RabbitMqPort"]);
            Host = configurationRoot["RabbitMqHost"];
            Username = configurationRoot["RabbitMqUsername"];
            Password = configurationRoot["RabbitMqPassword"];
        }

        public RabbitConfig()
        {
            Port = 5672;
            Host = "127.0.0.1";
            Username = "guest";
            Password = "guest";
        }
    }
}