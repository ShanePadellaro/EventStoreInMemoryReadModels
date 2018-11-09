using System;
using System.Collections.Generic;
using TransactionService.Api.ValueObjects;
using TransactionService.External.ValueObjects;

namespace TransactionService.Api.Domain
{
    public class FeeLedger
    {
        public Dictionary<string, Dictionary<int, Dictionary<int, long>>> Fees { get; set; }
        public FeeLedger()
        {
            Fees=new Dictionary<string, Dictionary<int, Dictionary<int, long>>>();
        }

        public void RecordFees(IEnumerable<Fee> fees, DateTime billingDate)
        {
            foreach (var fee in fees)
            {
                
                if (!Fees.ContainsKey(fee.FeeId))
                    Fees.Add(fee.FeeId,new Dictionary<int, Dictionary<int, long>>());
            
                if(!Fees[fee.FeeId].ContainsKey(billingDate.Year))
                    Fees[fee.FeeId].Add(billingDate.Year,new Dictionary<int, long>());
            
                if(!Fees[fee.FeeId][billingDate.Year].ContainsKey(billingDate.Month))
                    Fees[fee.FeeId][billingDate.Year].Add(billingDate.Month,0);

                Fees[fee.FeeId][billingDate.Year][billingDate.Month] += fee.Amount;
            }
            
        }

    }
}