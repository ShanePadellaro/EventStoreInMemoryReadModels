using System;
using System.Collections.Generic;
using TransactionService.Api.ValueObjects;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.Domain
{
    public class FeeLedger
    {
        public Dictionary<string,Dictionary<string,Dictionary<int, Dictionary<int, long>>>> Fees { get; set; }
        public FeeLedger()
        {
            Fees=new Dictionary<string,Dictionary<string,Dictionary<int, Dictionary<int, long>>>>();
        }

        public void RecordFees(IEnumerable<Fee> fees, DateTime billingDate, string countryCode)
        {
            foreach (var fee in fees)
            {

                if (!Fees.ContainsKey(fee.FeeId))
                    Fees.Add(fee.FeeId, new Dictionary<string, Dictionary<int, Dictionary<int, long>>>());
                
                if(!Fees[fee.FeeId].ContainsKey(countryCode))
                    Fees[fee.FeeId].Add(countryCode,new Dictionary<int, Dictionary<int, long>>());
                
                if(!Fees[fee.FeeId][countryCode].ContainsKey(billingDate.Year))
                    Fees[fee.FeeId][countryCode].Add(billingDate.Year,new Dictionary<int, long>());
            
                if(!Fees[fee.FeeId][countryCode][billingDate.Year].ContainsKey(billingDate.Month))
                    Fees[fee.FeeId][countryCode][billingDate.Year].Add(billingDate.Month,0);

                Fees[fee.FeeId][countryCode][billingDate.Year][billingDate.Month] += fee.Amount;
            }
            
        }

    }
}