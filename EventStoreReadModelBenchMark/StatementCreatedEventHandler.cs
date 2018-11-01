using Newtonsoft.Json;

namespace EventStoreReadModelBenchMark
{
    internal class StatementCreatedEventHandler : IDomainEventHandler
    {
        public SomethingEventTuple Execute(SomethingEventTuple thingy)
        {
            if (thingy.EventType != DomainEventTypes.StatementCreated)
                return thingy;

            var @event =
                JsonConvert.DeserializeObject<StatementCreatedEvent>(thingy.Event);

            var statement = new Statement(@event.BillingDate, @event.IncomingBalance)
            {
                BillingDate = @event.BillingDate, IncomingBalance = @event.IncomingBalance,
                CurrentBalance = @event.IncomingBalance
            };
            
            if (thingy.Account.CurrentStatement != null)
                thingy.Account.Statements.Add(thingy.Account.CurrentStatement);

            thingy.Account.CurrentStatement = statement;


            return thingy;
        }
    }
}