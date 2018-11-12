using System;

namespace TransactionService.Api.Domain
{
    public class Statement
    {
        public long IncomingBalance { get; set;}
        public long CurrentBalance { get; set; }
        public DateTime BillingDate { get; set; }
        public long ClosingBalance { get; set; }
        public StatementStatus Status { get; set; }
        
        public Statement(DateTime billingDate, long incomingBalance)
        {
            BillingDate = billingDate;
            IncomingBalance = incomingBalance;
            Status = StatementStatus.Open;
        }




        public void MakePayment(long amount)
        {
            if (CurrentBalance + amount > 0)
            {
                CurrentBalance = 0;
                return;
            }

            CurrentBalance += amount;
        }

        public void Close()
        {
            ClosingBalance = CurrentBalance;
            CurrentBalance = 0;
            Status = StatementStatus.Closed;
        }

    }
}