﻿
namespace EventStoreBenchMark.ValueObjects
{
    public class TransactionId 
    {
        public string Value { get; }

        public TransactionId(string value)
        {
            Value = value;
        }
    }
}