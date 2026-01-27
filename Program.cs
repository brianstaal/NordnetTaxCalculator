using System.Text;
using Spectre.Console;
using NordnetTaxCalculator.Services;

// Register encoding provider just in case
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

AnsiConsole.Clear();

// 1. Show Banner
AnsiConsole.Write(
    new FigletText("Nordnet Parser")
        .LeftJustified()
        .Color(Color.Lime));

// Find CSV file
var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");

if (!Directory.Exists(appDataPath))
{
    // Create it if missing to avoid crash, but warn user
    Directory.CreateDirectory("AppData");
    AnsiConsole.MarkupLine("[red]AppData folder not found. Created 'AppData' folder. Please place .csv file there.[/]");
    return;
}

var csvFile = Directory.GetFiles(appDataPath, "*.csv").FirstOrDefault();

if (csvFile == null)
{
    AnsiConsole.MarkupLine($"[red]No CSV file found in {appDataPath}[/]");
    return;
}

AnsiConsole.MarkupLine($"[green]Found file:[/] {Path.GetFileName(csvFile)}");

try
{
    // Dependency Composition
    NordnetCsvReader csvReader = new();
    TransactionAnalyzer analyzer = new();
    ConsoleReportPrinter printer = new();

    // Execution
    var transactions = csvReader.ReadTransactions(csvFile);

    if (transactions.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No transactions parsed.[/]");
        return;
    }

    var summary = analyzer.Analyze(transactions);
    printer.PrintReport(summary);
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
}
