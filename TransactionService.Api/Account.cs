using System;
using System.Collections.Generic;

namespace TransactionService.Api
{
    public class Account
    {
        public string Id { get; set; }
        public long Balance { get; set; }
        public Statement CurrentStatement { get; set; }
        public List<Statement> Statements { get; set; }

        public Account()
        {
            Statements=new List<Statement>();
        }

    }

    public class Statement
    {
        public Statement(DateTime billingDate, long incomingBalance)
        {
            BillingDate = billingDate;
            IncomingBalance = incomingBalance;
        }

        public long IncomingBalance { get; set;}
        public long CurrentBalance { get; set; }
        public DateTime BillingDate { get; set; }

        public void MakePayment(long amount)
        {
            if (CurrentBalance + amount > 0)
            {
                CurrentBalance = 0;
                return;
            }

            CurrentBalance += amount;
        }
    }
}