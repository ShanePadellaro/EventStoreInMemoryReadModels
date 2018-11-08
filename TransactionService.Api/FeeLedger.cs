using System;
using System.Collections.Generic;
using TransactionService.Api.ValueObjects;

namespace TransactionService.Api
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
                
                if (!Fees.ContainsKey(fee.CompanyId))
                    Fees.Add(fee.CompanyId,new Dictionary<int, Dictionary<int, long>>());
            
                if(!Fees[fee.CompanyId].ContainsKey(billingDate.Year))
                    Fees[fee.CompanyId].Add(billingDate.Year,new Dictionary<int, long>());
            
                if(!Fees[fee.CompanyId][billingDate.Year].ContainsKey(billingDate.Month))
                    Fees[fee.CompanyId][billingDate.Year].Add(billingDate.Month,0);

                Fees[fee.CompanyId][billingDate.Year][billingDate.Month] += fee.Amount;
            }
            
        }

    }
}