using System.Collections.Generic;
using TransactionService.Api.ValueObjects;

namespace TransactionService.Api.Domain
{
    public class Account
    {
        public string Id { get; set; }
        public long Balance { get; set; }
        public Statement CurrentStatement { get; set; }
        public List<Statement> Statements { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public string CurrencyCode { get; set; }
        public string CountryCode { get; set; }

        public Account(AccountDetails accountDetails)
        {
            Statements=new List<Statement>();
            Id = accountDetails.AccountId;
            Balance = accountDetails.StartingBalance;
            Name = accountDetails.Name;
            ExternalId = accountDetails.Externalid;
            CurrencyCode = accountDetails.CurrencyCode;
            CountryCode = accountDetails.CountryCode;
        }
        
        public Account()
        {
            Statements=new List<Statement>();
        }

    }
}