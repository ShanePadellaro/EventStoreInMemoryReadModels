using System;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace TransactionService.Api.Events
{
    public interface IDomainEvent<T>
    {
        EventMetaData MetaData { get; set; }
        string Name { get; }
        T Data { get; }
        EventData EventData { get; }
    }

    public abstract class DomainEvent<T> : IDomainEvent<T>
    {
        public EventMetaData MetaData { get; set; }
        public abstract string Name { get; }
        public abstract T Data { get; }

        public EventData EventData
        {
            get
            {
                var settings = new JsonSerializerSettings()
                    {ContractResolver = new CamelCasePropertyNamesContractResolver()};
                settings.Converters.Add(new StringEnumConverter());

                var data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this.Data,
                    settings));
                var metaData = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this.MetaData,
                    settings));
                return new EventData(Guid.NewGuid(), this.Name, true,
                    data, metaData);
            }
        }
    }
}