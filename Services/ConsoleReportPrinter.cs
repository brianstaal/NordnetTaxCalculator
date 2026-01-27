using System.Globalization;
using NordnetTaxCalculator.Entities;
using Spectre.Console;

namespace NordnetTaxCalculator.Services;

public class ConsoleReportPrinter : IReportPrinter
{
    public void PrintReport(List<TransactionSummary> summary)
    {
        var danishCulture = new CultureInfo("da-DK");

        // Render Table
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("Stock");
        table.AddColumn(new TableColumn("Bought").RightAligned());
        table.AddColumn(new TableColumn("Sold").RightAligned());
        table.AddColumn(new TableColumn("Result").RightAligned());

        decimal totalBought = 0;
        decimal totalSold = 0;
        decimal totalResult = 0;

        foreach (var item in summary)
        {
            var resultColor = item.Profit >= 0 ? "green" : "red";
            // Only show if there is activity
            if (item.Bought == 0 && item.Sold == 0) continue;

            table.AddRow(
                item.Stock, 
                item.Bought.ToString("N2", danishCulture), 
                item.Sold.ToString("N2", danishCulture), 
                $"[{resultColor}]{item.Profit.ToString("N2", danishCulture)}[/]"
            );
            
            totalBought += item.Bought;
            totalSold += item.Sold;
            totalResult += item.Profit;
        }

        table.AddRow(new Rule());
        var totalColor = totalResult >= 0 ? "green" : "red";
        table.AddRow(
            "[bold]TOTAL[/]", 
            $"[bold]{totalBought.ToString("N2", danishCulture)}[/]", 
            $"[bold]{totalSold.ToString("N2", danishCulture)}[/]", 
            $"[bold {totalColor}]{totalResult.ToString("N2", danishCulture)}[/]"
        );

        AnsiConsole.Write(table);
    }
}
