namespace NordnetTaxCalculator.Entities;

public class Overview
{
    public int FirstYear { get; set; }

    public decimal Inserted { get; set; }

    public decimal Withdrawn { get; set; }
    
    public decimal Interest { get; set; }
    
    public decimal Fees { get; set; }
    
    public decimal Yield { get; set; }
    
    public decimal YieldTax { get; set; }



    public decimal NetAmount => Inserted + Withdrawn;

    public List<Stock> Stocks { get; set; } = [];
}
