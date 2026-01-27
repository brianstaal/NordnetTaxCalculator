using NordnetTaxCalculator.Entities;

namespace NordnetTaxCalculator.Services;

public interface ITransactionAnalyzer
{
    List<TransactionSummary> Analyze(List<Transaction> transactions);
}
