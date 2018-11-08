using System;
using System.Collections.Generic;

namespace TransactionService.Api
{
    public class TaxLedger
    {
        public Dictionary<string,Dictionary<int,Dictionary<int,long>>> Countries { get; private set; }

        public TaxLedger()
        {
            Countries=new Dictionary<string, Dictionary<int, Dictionary<int, long>>>();
            
        }
        public void RecordTax(long tax, string countryCode, in DateTime billingDate)
        {
            if (!Countries.ContainsKey(countryCode))
                Countries.Add(countryCode,new Dictionary<int, Dictionary<int, long>>());
            
            if(!Countries[countryCode].ContainsKey(billingDate.Year))
                Countries[countryCode].Add(billingDate.Year,new Dictionary<int, long>());
            
            if(!Countries[countryCode][billingDate.Year].ContainsKey(billingDate.Month))
                Countries[countryCode][billingDate.Year].Add(billingDate.Month,0);

            Countries[countryCode][billingDate.Year][billingDate.Month] += tax;
        }
    }
}