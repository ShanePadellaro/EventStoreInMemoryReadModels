using Newtonsoft.Json;
using TransactionService.Api.Domain;
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
                JsonConvert.DeserializeObject<StatementDetails>(state.Event);

            var statement = new Statement(@event.BillingDate, @event.IncomingBalance)
            {
                BillingDate = @event.BillingDate, IncomingBalance = @event.IncomingBalance,
                CurrentBalance = @event.IncomingBalance
            };

            if (state.Account.CurrentStatement != null)
            {
                var currentStatement = state.Account.CurrentStatement;
                currentStatement.Close();
                state.Account.Statements.Add(currentStatement);
            }

            state.Account.CurrentStatement = statement;


            return state;
        }
    }
}