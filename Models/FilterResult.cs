namespace SK.Models;

public class FilterResult
{
    // Customer information
    public string CustomerID { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    
    // Basic aggregation - outstanding balance
    public decimal OutstandingBalance { get; set; }
    
    // Raw data for LLM analysis
    public List<InvoiceInfo> Invoices { get; set; } = new();
    public List<PaymentInfo> Payments { get; set; } = new();
}

public class InvoiceInfo
{
    public string InvoiceID { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public int? DaysLate { get; set; }
}

public class PaymentInfo
{
    public string PaymentID { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}
