using System.Text;
using Spectre.Console;
using NordnetTaxCalculator.Services;
using NordnetTaxCalculator.Entities;

// Register encoding provider just in case
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

AnsiConsole.Clear();

// 1. Show Banner
AnsiConsole.Write(
    new FigletText("Nordnet Skatteberegner")
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


    var overview = analyzer.GetOverview(transactions);

    var tableOverview = new Table()
        .RoundedBorder()
        .BorderColor(Color.Blue);
    tableOverview.AddColumn("Indsat", col => col.RightAligned());
    tableOverview.AddColumn("Hævet", col => col.RightAligned());
    tableOverview.AddColumn("Balance", col => col.RightAligned());
    tableOverview.AddRow($"{overview.Inserted:N}", $"{overview.Withdrawn:N}", $"{overview.NetAmount:N}");
    AnsiConsole.Write(tableOverview);

    var tableFees = new Table()
        .RoundedBorder()
        .BorderColor(Color.Blue);
    tableFees.AddColumn("Renter", col => col.RightAligned());
    tableFees.AddColumn("Gebyr", col => col.RightAligned());
    tableFees.AddColumn("Udbytte", col => col.RightAligned());
    tableFees.AddColumn("Udbytteskat", col => col.RightAligned());
    tableFees.AddRow($"{overview.Interest:N}", $"{overview.Fees:N}", $"{overview.Yield:N}", $"{overview.YieldTax:N}");
    AnsiConsole.Write(tableFees);

    foreach (var stock in overview.Stocks.OrderBy(x => x.Name))
    {
        var message = new Markup($"[bold blue]{stock.Name}[/]\n{stock.ISIN}");
        AnsiConsole.Write(message);
        AnsiConsole.WriteLine();


        var table = new Table()
        .RoundedBorder()
        .BorderColor(Color.Blue);
        table.AddColumn("Tekst");
        table.AddColumn("Dato", col => col.RightAligned());
        table.AddColumn("Antal", col => col.RightAligned());
        table.AddColumn("Købskurs", col => col.RightAligned());
        table.AddColumn("Salgskurs", col => col.RightAligned());
        table.AddColumn("Tab/Gevinst", col => col.RightAligned());

        var bought = new List<Transaction>();
        var totalGain = 0m;
        foreach (var t in stock.Transactions.OrderBy(t => t.TransactionDate))
        {
            if (t.TransactionType.Equals("KØBT", StringComparison.OrdinalIgnoreCase))
            {
                bought.Add(t);
            }
            else
            {
                var soldQuantity = 0;
                table.AddRow("Solgt", $"{t.TransactionDate:yyyy-MM-dd}", $"{t.Quantity}", $"-", $"{t.Rate:N}", $"[bold blue]{t.Amount:N}[/]");
                
                foreach (var b in bought.Where(x => x.Quantity > 0))
                {
                    if (t.Quantity >= soldQuantity)
                    {
                        var rest = t.Quantity - soldQuantity;
                        var quantitySold = Math.Min(rest, b.Quantity);

                        var gain = (t.Rate * quantitySold) - (b.Rate * quantitySold);
                        totalGain += gain;
                        table.AddRow("Købt", $"{b.TransactionDate:yyyy-MM-dd}", $"{quantitySold}", $"{b.Rate:N}", $"{t.Rate:N}", $"{gain:N}");

                        soldQuantity += quantitySold;
                        if (rest <= b.Quantity)
                        {
                            // Update the bought transaction with the remaining quantity
                            b.Quantity -= quantitySold;
                            //table.AddRow($"Tab/Gevinst", "", "", "", $"{totalGain:N}");

                            // Add footers
                            table.Columns[0].Footer = new Text("Tab/Gevinst", new Style(foreground: Color.Yellow));
                            if (totalGain < 0)
                                table.Columns[5].Footer = new Text($"{totalGain:N}", new Style(foreground: Color.Red, decoration: Decoration.Bold));
                            else
                                table.Columns[5].Footer = new Text($"{totalGain:N}", new Style(foreground: Color.Green, decoration: Decoration.Bold));

                            break;
                        }
                        else
                        {
                            b.Quantity = 0; // All quantity from this bought transaction is sold
                        }
                    }


                    //var gain = (t.Amount / t.Quantity) - (b.Amount / b.Quantity);
                    //table.AddRow($"[grey]{b.TransactionDate:yyyy-MM-dd}[/]", $"[grey]{b.TransactionType} {b.Quantity} @ {Math.Abs(b.Amount) / b.Quantity:N}[/]", $"[grey]Netto: {b.Amount:N}, Gain: {gain:N}[/]");
                }



                //table.AddRow($"[grey]{t.TransactionDate:yyyy-MM-dd}[/]", $"[grey]{t.TransactionType} {t.Quantity} @ {Math.Abs(t.Amount) / t.Quantity:N}[/]", $"[grey]Netto: {t.Amount:N}[/]");
            }
        }



        //foreach (var rgpy in stock.RealizedGainsPerYear().OrderBy(x => x.Key))
        //{
        //    table.AddRow($"[grey]{rgpy.Key}[/]", $"[grey]Realiseret gevinst[/]", $"[grey]{rgpy.Value:N}[/]");
        //}
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }


    //var summary = analyzer.Analyze(transactions);
    //printer.PrintReport(summary);
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
}
