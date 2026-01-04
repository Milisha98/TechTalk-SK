using SK.Models;

namespace SK.Repositories;

public interface IInvoiceRepository
{
    Task LoadDataAsync();
    IQueryable<Invoice> FetchAll();
    IQueryable<Invoice> FetchByCustomerId(string customerId);
    IQueryable<Invoice> FetchUnpaidByCustomerId(string customerId);
}
