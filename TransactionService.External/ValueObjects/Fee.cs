

namespace TransactionService.External.ValueObjects
{
    public class Fee
    {
        public Fee(string feeId, string label,long amount, string originalCurrency, string conversionRate, long tax,
            long taxRate)
        {
            FeeId = feeId;
            Label = label;
            Amount = amount;
            OriginalCurrency = originalCurrency;
            ConversionRate = conversionRate;
            Tax = tax;
            TaxRate = taxRate;
        }

        public string FeeId { get; private set; }
        public string Label { get; private set; }
        public string OriginalCurrency { get; private set; }
        public string ConversionRate { get; private set; }
        public long Tax { get; private set; }
        public long Amount { get; private set; }
        public long TaxRate { get; private set; }
    }
}