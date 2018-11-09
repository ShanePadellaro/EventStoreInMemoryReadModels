using System;

namespace TransactionService.Api.Controllers
{
    public class AccountId
    {
        public string Id { get; }

        public AccountId(string accountId)
        {
            Id = accountId;
        }

        public static AccountId New => new AccountId($"Account-{Guid.NewGuid()}");
    }
}