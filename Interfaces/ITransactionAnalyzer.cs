using NordnetTaxCalculator.Entities;

namespace NordnetTaxCalculator.Interfaces;

public interface ITransactionAnalyzer
{
    List<Stock> SummarizeStocks(List<Transaction> transactions);
}
