namespace SK.Models;

public class FilterSpec
{
    public string? CustomerName { get; set; }
    public int Months { get; set; }
    public bool IncludeOutstanding { get; set; }
    public bool IncludeTrends { get; set; }
    public bool IncludeAnomalies { get; set; }
    public string Scope { get; set; } = "singleCustomer";
}
