using NordnetTaxCalculator.Entities;

namespace NordnetTaxCalculator.Interfaces;

public interface IReportPrinter
{
    void PrintReport(List<TransactionSummary> summaries);
}
