using SK.Models;

namespace SK.Repositories;

public interface ICustomerRepository
{
    Task LoadDataAsync();
    IQueryable<Customer> FetchAll();
    Customer? FetchByName(string name);
    Customer? FetchById(string customerId);
}
