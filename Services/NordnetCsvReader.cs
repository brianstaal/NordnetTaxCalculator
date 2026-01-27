using System.Globalization;
using System.Text;
using NordnetTaxCalculator.Entities;
using SoftCircuits.CsvParser;

namespace NordnetTaxCalculator.Services;

public class NordnetCsvReader : ICsvReader
{
    public List<Transaction> ReadTransactions(string filePath)
    {
        var transactions = new List<Transaction>();
        var danishCulture = new CultureInfo("da-DK");

        // Detect encoding? Nordnet often uses UTF-16LE or ANSI. 
        // Let's assume default unless BOM. SoftCircuits handles streams nicely.
        // We'll try to auto-detect delimiter.
        var settings = new CsvSettings
        {
            ColumnDelimiter = '\t', // Start with Tab as seen in error output previously
        };

        // Check first line to confirm delimiter before creating reader if needed, 
        // but let's try reading one line with likely delimiters if one fails? 
        // Actually, let's just inspect the first line manually quickly to set the delimiter correctly
        // as SoftCircuits doesn't auto-detect delimiter by itself in the constructor without hints.

        string firstLine = File.ReadLines(filePath).FirstOrDefault() ?? "";
        if (firstLine.Contains(';') && !firstLine.Contains('\t')) settings.ColumnDelimiter = ';';
        else if (firstLine.Contains(',') && !firstLine.Contains('\t')) settings.ColumnDelimiter = ',';

        // Use CsvReader with explicit Encoding.Unicode (UTF-16LE) and settings
        // Assuming constructor: CsvReader(string path, Encoding encoding, CsvSettings settings)
        using (var reader = new CsvReader(filePath, Encoding.Unicode, settings))
        {
            string[]? headers = null;
            if (reader.ReadRow(ref headers))
            {
                // Find indices manually to handle duplicate headers (like "Valuta") safely
                var headerList = headers.Select(h => h.Trim()).ToList();

                var dateColIndex = headerList.FindIndex(h => h.Equals("Valørdag", StringComparison.OrdinalIgnoreCase));
                var stockColIndex = headerList.FindIndex(h => h.Equals("Værdipapirer", StringComparison.OrdinalIgnoreCase));
                var transactionTypeColIndex = headerList.FindIndex(h => h.Equals("Transaktionstype", StringComparison.OrdinalIgnoreCase));
                var quantityColIndex = headerList.FindIndex(h => h.Equals("Antal", StringComparison.OrdinalIgnoreCase));
                var rateColIndex = headerList.FindIndex(h => h.Equals("Kurs", StringComparison.OrdinalIgnoreCase));
                var feeColIndex = headerList.FindIndex(h => h.Equals("Samlede afgifter", StringComparison.OrdinalIgnoreCase));
                var amountColIndex = headerList.FindIndex(h => h.Equals("Beløb", StringComparison.OrdinalIgnoreCase));

                if (stockColIndex == -1 || transactionTypeColIndex == -1 || amountColIndex == -1)
                {
                    throw new InvalidOperationException($"Could not find required columns (Værdipapirer, Transaktionstype, Beløb). Headers found: {string.Join(", ", headers)}");
                }

                string[]? columns = null;
                while (reader.ReadRow(ref columns))
                {
                    if (columns.Length <= dateColIndex ||
                        columns.Length <= stockColIndex ||
                        columns.Length <= transactionTypeColIndex ||
                        columns.Length <= quantityColIndex ||
                        columns.Length <= rateColIndex ||
                        columns.Length <= feeColIndex ||
                        columns.Length <= amountColIndex)
                        continue;

                    var dateStr = columns[dateColIndex].Trim();
                    var stock = columns[stockColIndex].Trim();
                    var transactionType = columns[transactionTypeColIndex].Trim();
                    var quantity = columns[quantityColIndex].Trim();
                    var rateStr = columns[rateColIndex].Trim();
                    var feeStr = columns[feeColIndex].Trim();
                    var amountStr = columns[amountColIndex].Trim();

                    // Clean up amount (spaces, non-breaking spaces)
                    rateStr = rateStr.Replace(" ", "").Replace("\u00A0", "");
                    feeStr = feeStr.Replace(" ", "").Replace("\u00A0", "");
                    amountStr = amountStr.Replace(" ", "").Replace("\u00A0", "");

                    if (decimal.TryParse(amountStr, NumberStyles.Any, danishCulture, out decimal amount))
                    {
                        DateTime.TryParseExact(dateStr, "yyyy-MM-dd", danishCulture, DateTimeStyles.None, out DateTime transactionDate);
                        int.TryParse(quantity, NumberStyles.Any, danishCulture, out int qty);
                        decimal.TryParse(rateStr, NumberStyles.Any, danishCulture, out decimal rate);
                        decimal.TryParse(feeStr, NumberStyles.Any, danishCulture, out decimal fee);

                        transactions.Add(new Transaction { 
                            TransactionDate = transactionDate,
                            Stock = stock, 
                            TransactionType = transactionType, 
                            Quantity = qty,
                            Rate = rate,
                            Fee = fee,
                            Amount = amount
                        });
                    }
                }
            }
        }
        return transactions;
    }
}
