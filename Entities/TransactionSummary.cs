namespace NordnetTaxCalculator.Entities;

public class TransactionSummary
{
    public string Stock { get; set; } = "";
    public decimal Bought { get; set; }
    public decimal Sold { get; set; }
    public decimal Profit => Sold - Bought;
}
