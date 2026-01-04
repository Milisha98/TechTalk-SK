using SK.Models;

namespace SK.Repositories;

public interface IPaymentRepository
{
    Task LoadDataAsync();
    IQueryable<Payment> FetchAll();
    IQueryable<Payment> FetchByCustomerId(string customerId);
}
