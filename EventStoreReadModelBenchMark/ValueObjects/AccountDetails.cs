//using EventFlow.ValueObjects;
//using Newtonsoft.Json;
//using TransactionsCQRS.API.Controllers.Accounts.InputModels;
//
//namespace TransactionsCQRS.API.Domain.Account.ValueObjects
//{
//    public class AccountDetails
//    {
//        public string Externalid { get; }
//        public string Name { get; }
//        public string CountryCode { get; }
//        public string CurrencyCode { get; }
//        public int StartingBalance { get; }
//
//        [JsonConstructor]
//        public AccountDetails(string externalid, string name, string countryCode, string currencyCode,
//            int startingBalance)
//        {
//            Externalid = externalid;
//            Name = name;
//            CountryCode = countryCode;
//            CurrencyCode = currencyCode;
//            StartingBalance = startingBalance;
//        }
//        
//        public AccountDetails(AccountDetailsInputModel accountDetails)
//        {
//            Externalid = accountDetails.Externalid;
//            Name = accountDetails.Name;
//            CountryCode = accountDetails.CountryCode;
//            CurrencyCode = accountDetails.CurrencyCode;
//            StartingBalance = accountDetails.StartingBalance;
//        }
//    }
//}