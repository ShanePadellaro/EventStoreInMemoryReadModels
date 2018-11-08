using Newtonsoft.Json;
using TransactionService.Api.Events;

namespace TransactionService.Api.EventHandlers
{
    internal class StatementCreatedEventHandler : IDomainEventHandler
    {
        public State Execute(State state)
        {
            if (state.EventType != DomainEventTypes.StatementCreated)
                return state;

            var @event =
                JsonConvert.DeserializeObject<StatementCreatedEvent>(state.Event);

            var statement = new Statement(@event.BillingDate, @event.IncomingBalance)
            {
                BillingDate = @event.BillingDate, IncomingBalance = @event.IncomingBalance,
                CurrentBalance = @event.IncomingBalance
            };
            
            if (state.Account.CurrentStatement != null)
                state.Account.Statements.Add(state.Account.CurrentStatement);

            state.Account.CurrentStatement = statement;


            return state;
        }
    }
}