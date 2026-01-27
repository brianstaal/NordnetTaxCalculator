using NordnetTaxCalculator.Entities;

namespace NordnetTaxCalculator.Interfaces;

public interface ICsvReader
{
    List<Transaction> ReadTransactions(string filePath);
}
