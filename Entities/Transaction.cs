namespace NordnetTaxCalculator.Entities;

public class Transaction
{
    public DateTime TransactionDate { get; set; }

    public string Stock { get; set; } = "";

    public string TransactionType { get; set; } = "";

    public int Quantity { get; set; }

    public decimal Rate { get; set; }
    
    public decimal Fee { get; set; }

    public decimal Amount { get; set; }
}