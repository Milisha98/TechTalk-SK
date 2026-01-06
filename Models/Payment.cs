namespace SK.Models;

public class Payment
{
    public string   PaymentID     { get; set; } = string.Empty;
    public string   CustomerID    { get; set; } = string.Empty;
    public decimal  Amount        { get; set; }
    public DateTime Date          { get; set; }
}
