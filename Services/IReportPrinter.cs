using NordnetTaxCalculator.Entities;

namespace NordnetTaxCalculator.Services;

public interface IReportPrinter
{
    void PrintReport(List<TransactionSummary> summaries);
}
