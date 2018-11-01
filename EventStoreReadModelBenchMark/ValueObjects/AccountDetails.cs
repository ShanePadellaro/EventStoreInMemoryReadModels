using Newtonsoft.Json;

namespace EventStoreReadModelBenchMark.ValueObjects
{
    public class AccountDetails 
    {
        public string Externalid { get; }
        public string Name { get; }
        public string CountryCode { get; }
        public string CurrencyCode { get; }
        public int StartingBalance { get; }

        [JsonConstructor]
        public AccountDetails(string externalid, string name, string countryCode, string currencyCode,
            int startingBalance)
        {
            Externalid = externalid;
            Name = name;
            CountryCode = countryCode;
            CurrencyCode = currencyCode;
            StartingBalance = startingBalance;
        }
    }
}