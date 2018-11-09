using Newtonsoft.Json;
using TransactionService.Api.Controllers;

namespace TransactionService.Api.ValueObjects
{
    public class AccountDetails 
    {
        public string AccountId { get; }
        public string Externalid { get; }
        public string Name { get; }
        public string CountryCode { get; }
        public string CurrencyCode { get; }
        public int StartingBalance { get; }

        [JsonConstructor]
        public AccountDetails(string accountId, string externalid, string name, string countryCode,
            string currencyCode,
            int startingBalance)
        {
            AccountId = accountId;
            Externalid = externalid;
            Name = name;
            CountryCode = countryCode;
            CurrencyCode = currencyCode;
            StartingBalance = startingBalance;
        }
    }
}