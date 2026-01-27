using NordnetTaxCalculator.Entities;
using NordnetTaxCalculator.Interfaces;

namespace NordnetTaxCalculator.Services;

public class TransactionAnalyzer : ITransactionAnalyzer
{
    public List<Stock> SummarizeStocks(List<Transaction> transactions)
    {
        var stockDict = new Dictionary<string, Stock>();

        foreach (var transaction in transactions)
        {
            if (stockDict.TryGetValue(transaction.ISIN, out var stock))
            {
                stock.Transactions.Add(transaction);
            } else
            {
                stock = new Stock
                {
                    ISIN = transaction.ISIN,
                    Name = transaction.Stock,
                    Transactions = [transaction]
                };
                stockDict[transaction.ISIN] = stock;
            }
        }
        return [.. stockDict.Values];
    }

    public List<TransactionSummary> Analyze(List<Transaction> transactions)
    {
        return transactions
            .Where(t => t.TransactionType.Equals("KØBT", StringComparison.OrdinalIgnoreCase) || 
                        t.TransactionType.Equals("SOLGT", StringComparison.OrdinalIgnoreCase))
            .GroupBy(t => t.Stock)
            .Select(g => new TransactionSummary
            {
                Stock = g.Key,
                // KØBT entries are usually negative cost. We want positive Total Bought.
                Bought = g.Where(t => t.TransactionType.Equals("KØBT", StringComparison.OrdinalIgnoreCase))
                          .Sum(t => Math.Abs(t.Amount)),
                
                // SOLGT entries are positive proceeds.
                Sold = g.Where(t => t.TransactionType.Equals("SOLGT", StringComparison.OrdinalIgnoreCase))
                        .Sum(t => t.Amount),
            })
            .OrderBy(s => s.Stock)
            .ToList();
    }
}