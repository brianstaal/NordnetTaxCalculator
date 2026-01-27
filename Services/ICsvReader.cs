using NordnetTaxCalculator.Entities;

namespace NordnetTaxCalculator.Services;

public interface ICsvReader
{
    List<Transaction> ReadTransactions(string filePath);
}
