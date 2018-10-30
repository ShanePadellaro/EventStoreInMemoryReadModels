﻿using EventStoreReadModelBenchMark.ValueObjects;

namespace EventStoreReadModelBenchMark
{
    public class AccountCreditedEvent
    {
        public Transaction Transaction { get; }

        public AccountCreditedEvent(Transaction transaction)
        {
            Transaction = transaction;
        }
    }
}