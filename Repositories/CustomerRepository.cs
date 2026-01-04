using System.Globalization;
using SK.Models;

namespace SK.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _filePath;
    private List<Customer> _customers = new();

    public CustomerRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task LoadDataAsync()
    {
        _customers.Clear();
        
        var lines = await File.ReadAllLinesAsync(_filePath);
        
        // Skip header row
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length >= 4)
            {
                _customers.Add(new Customer
                {
                    CustomerID = parts[0].Trim(),
                    Name = parts[1].Trim(),
                    ABN = parts[2].Trim(),
                    Region = parts[3].Trim()
                });
            }
        }
    }

    public IQueryable<Customer> FetchAll() => 
        _customers.AsQueryable();
    

    public Customer? FetchByName(string name) =>
        _customers.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    

    public Customer? FetchById(string customerId) =>
        _customers.FirstOrDefault(c => c.CustomerID == customerId);

}
