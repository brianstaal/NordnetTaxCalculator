using System;
using System.Collections.Generic;
using System.Text;

namespace NordnetTaxCalculator.Entities;

public class Overview
{

    public decimal Inserted { get; set; }

    public decimal Withdrawn { get; set; }

    public decimal NetAmount => Inserted - Withdrawn;

    public List<Stock> Stocks { get; set; } = [];
}
