using NordnetTaxCalculator.Entities;
using NordnetTaxCalculator.Interfaces;

namespace NordnetTaxCalculator.Services;

public class TransactionAnalyzer : ITransactionAnalyzer
{
    public Overview GetOverview(List<Transaction> transactions)
    {
        var rtn = new Overview();
        
        //HÆVNING + INDSÆTTELSE
        var otherTransactions = transactions.Where(t => string.IsNullOrEmpty(t.Stock)).ToList();

        foreach (var transaction in otherTransactions)
        {
            rtn.FirstYear = Math.Min(rtn.FirstYear, transaction.TransactionDate.Year);

            var amount = transaction.Amount;

            if (transaction.TransactionType.Equals("HÆVNING", StringComparison.OrdinalIgnoreCase))
            {
                //if (amount < 0)
                //    amount = -amount;
                rtn.Withdrawn += amount;
            }
            else if (transaction.TransactionType.Equals("INDBETALING", StringComparison.OrdinalIgnoreCase) || 
                     transaction.TransactionType.Equals("INDSÆTTELSE", StringComparison.OrdinalIgnoreCase))
            {
                rtn.Inserted += amount;
            } else
            {
                // Interest
                rtn.Interest += amount;
            }

        }

        // Seperate list of transactions
        var trades = transactions.Where(t => !string.IsNullOrEmpty(t.Stock) && t.Amount != 0).ToList();
        rtn.Stocks = SummarizeStocks(trades);

        return rtn;

    }

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
}