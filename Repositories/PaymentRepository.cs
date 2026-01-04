using System.Globalization;
using SK.Models;

namespace SK.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly string _filePath;
    private List<Payment> _payments = new();

    public PaymentRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task LoadDataAsync()
    {
        _payments.Clear();
        
        var lines = await File.ReadAllLinesAsync(_filePath);
        
        // Skip header row
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length >= 4)
            {
                _payments.Add(new Payment
                {
                    PaymentID = parts[0].Trim(),
                    CustomerID = parts[1].Trim(),
                    Amount = decimal.Parse(parts[2].Trim(), CultureInfo.InvariantCulture),
                    Date = DateTime.Parse(parts[3].Trim(), CultureInfo.InvariantCulture)
                });
            }
        }
    }

    public IQueryable<Payment> FetchAll() => 
        _payments.AsQueryable();

    public IQueryable<Payment> FetchByCustomerId(string customerId) => 
        _payments.Where(p => p.CustomerID == customerId).AsQueryable();
}
