using System.Globalization;
using SK.Models;

namespace SK.Repositories;

public interface IInvoiceRepository
{
    Task LoadDataAsync();
    IQueryable<Invoice> FetchAll();
    IQueryable<Invoice> FetchByCustomerId(string customerId);
    IQueryable<Invoice> FetchUnpaidByCustomerId(string customerId);
}

public class InvoiceRepository : IInvoiceRepository
{
    private readonly string _filePath;
    private List<Invoice> _invoices = new();

    public InvoiceRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task LoadDataAsync()
    {
        _invoices.Clear();
        
        var lines = await File.ReadAllLinesAsync(_filePath);
        
        // Skip header row
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length >= 5)
            {
                _invoices.Add(new Invoice
                {
                    InvoiceID = parts[0].Trim(),
                    CustomerID = parts[1].Trim(),
                    Amount = decimal.Parse(parts[2].Trim(), CultureInfo.InvariantCulture),
                    DueDate = DateTime.Parse(parts[3].Trim(), CultureInfo.InvariantCulture),
                    PaidDate = string.IsNullOrWhiteSpace(parts[4]) 
                        ? null 
                        : DateTime.Parse(parts[4].Trim(), CultureInfo.InvariantCulture)
                });
            }
        }
    }

    public IQueryable<Invoice> FetchAll() => 
        _invoices.AsQueryable();

    public IQueryable<Invoice> FetchByCustomerId(string customerId) => 
        _invoices.Where(i => i.CustomerID == customerId).AsQueryable();

    public IQueryable<Invoice> FetchUnpaidByCustomerId(string customerId) => 
        _invoices.Where(i => i.CustomerID == customerId && i.PaidDate is null).AsQueryable();
}
