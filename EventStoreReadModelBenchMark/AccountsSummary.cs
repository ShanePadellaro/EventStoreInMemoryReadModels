using System.Collections.Generic;

namespace EventStoreReadModelBenchMark
{
    public class AccountsSummary
    {
        public string _id => "5bd9c1fddfd1ea0dcece6f26";
        public Dictionary<string, long> AccountBalances { get; set; }
        public long EventNumber { get; set; }

        public AccountsSummary(Dictionary<string, long> accountBalances)
        {
            AccountBalances = accountBalances;
        }

        public AccountsSummary()
        {
            AccountBalances = new Dictionary<string, long>();
        }
    }
}