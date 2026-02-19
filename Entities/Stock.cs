namespace NordnetTaxCalculator.Entities;

public class Stock
{
    public string ISIN { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<Transaction> Transactions { get; set; } = [];


    public int TotalQuantityBought => Transactions
        .Where(t => t.TransactionType.Equals("KØBT", StringComparison.OrdinalIgnoreCase))
        .Sum(t => t.Quantity);

    public int TotalQuantitySold => Transactions
        .Where(t => t.TransactionType.Equals("SOLGT", StringComparison.OrdinalIgnoreCase))
        .Sum(t => t.Quantity);

    public int NetQuantity => TotalQuantityBought - TotalQuantitySold;



    // By using Fifo method to calculate realized gains/losses per year
    public Dictionary<int, decimal> RealizedGainsPerYear()
    {
        var gainsPerYear = new Dictionary<int, decimal>();
        var buyQueue = new Queue<(int Quantity, decimal Price)>();
        foreach (var transaction in Transactions.OrderBy(t => t.TransactionDate))
        {
            if (transaction.TransactionType.Equals("KØBT", StringComparison.OrdinalIgnoreCase))
            {
                buyQueue.Enqueue((transaction.Quantity, Math.Abs(transaction.Amount) / transaction.Quantity));
            }
            else if (transaction.TransactionType.Equals("SOLGT", StringComparison.OrdinalIgnoreCase))
            {
                int quantityToSell = transaction.Quantity;
                decimal salePricePerUnit = transaction.Amount / transaction.Quantity;
                decimal totalGain = 0m;
                while (quantityToSell > 0 && buyQueue.Count > 0)
                {
                    var (buyQuantity, buyPricePerUnit) = buyQueue.Peek();
                    if (buyQuantity <= quantityToSell)
                    {
                        totalGain += buyQuantity * (salePricePerUnit - buyPricePerUnit);
                        quantityToSell -= buyQuantity;
                        buyQueue.Dequeue();
                    }
                    else
                    {
                        totalGain += quantityToSell * (salePricePerUnit - buyPricePerUnit);
                        buyQueue.Enqueue((buyQuantity - quantityToSell, buyPricePerUnit));
                        quantityToSell = 0;
                    }
                }
                int year = transaction.TransactionDate.Year;
                if (!gainsPerYear.ContainsKey(year))
                {
                    gainsPerYear[year] = 0m;
                }
                gainsPerYear[year] += totalGain;
            }
        }
        return gainsPerYear;
    }
}
