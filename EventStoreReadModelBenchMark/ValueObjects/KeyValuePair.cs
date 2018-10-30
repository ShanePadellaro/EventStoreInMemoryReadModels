
namespace EventStoreReadModelBenchMark.ValueObjects
{
    public class KeyValuePair 
    {
        public KeyValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}