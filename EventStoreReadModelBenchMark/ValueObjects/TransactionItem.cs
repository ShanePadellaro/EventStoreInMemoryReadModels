
using System.Collections.Generic;

namespace EventStoreReadModelBenchMark.ValueObjects
{
    public class TransactionItem
    {
        public TransactionItem(long amount, string type, long units, List<KeyValuePair> keyValuePairs = null, List<Fee> subFees = null)
        {
            Amount = amount;
            Type = type;
            Units = units;
            KeyValuePairs = keyValuePairs;
            SubFees = subFees;
        }

        public long Amount { get; private set; }
        public string Type { get; private set; }
        public long Units { get; private set; }
        public List<Fee> SubFees { get; private set; }
        public List<KeyValuePair> KeyValuePairs { get; private set; }

    }
}