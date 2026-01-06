namespace SK.Models;

public class Invoice
{
    public string   InvoiceID   { get; set; } = string.Empty;
    public string   CustomerID  { get; set; } = string.Empty;
    public decimal  Amount      { get; set; }
    public DateTime DueDate     { get; set; }
    public DateTime? PaidDate   { get; set; }
}
