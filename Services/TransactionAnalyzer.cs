using NordnetTaxCalculator.Entities;

namespace NordnetTaxCalculator.Services;

public class TransactionAnalyzer : ITransactionAnalyzer
{
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
