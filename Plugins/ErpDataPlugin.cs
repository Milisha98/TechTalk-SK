using System.ComponentModel;
using Microsoft.SemanticKernel;
using SK.Models;
using SK.Repositories;

namespace SK.Plugins;

public class ErpDataPlugin
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public ErpDataPlugin(
        ICustomerRepository customerRepository,
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        _customerRepository = customerRepository;
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task LoadDataAsync()
    {
        await _customerRepository.LoadDataAsync();
        await _invoiceRepository.LoadDataAsync();
        await _paymentRepository.LoadDataAsync();
    }

    [KernelFunction]
    [Description("Looks up a customer by their business name. Returns customer details if found, null otherwise.")]
    public Customer? GetCustomerByName(
        [Description("The name of the customer business to look up")] string customerName) =>
        _customerRepository.FetchByName(customerName);

    [KernelFunction]
    [Description("Gets all invoices for a customer within a specified time period. Includes invoice amounts, due dates, and payment dates.")]
    public List<Invoice> GetInvoicesForCustomer(
        [Description("The name of the customer")] string customerName,
        [Description("Number of months to look back (e.g., 6 for last 6 months)")] int months = 6)
    {
        var customer = _customerRepository.FetchByName(customerName);
        if (customer is null) return new List<Invoice>();

        var cutoffDate = DateTime.Now.AddMonths(-months);
        
        return _invoiceRepository
            .FetchByCustomerId(customer.CustomerID)
            .Where(i => i.DueDate >= cutoffDate)
            .OrderBy(i => i.DueDate)
            .ToList();
    }

    [KernelFunction]
    [Description("Gets all payments made by a customer within a specified time period. Includes payment amounts and dates.")]
    public List<Payment> GetPaymentsForCustomer(
        [Description("The name of the customer")] string customerName,
        [Description("Number of months to look back (e.g., 6 for last 6 months)")] int months = 6)
    {
        var customer = _customerRepository.FetchByName(customerName);
        if (customer is null) return new List<Payment>();

        var cutoffDate = DateTime.Now.AddMonths(-months);
        
        return _paymentRepository
            .FetchByCustomerId(customer.CustomerID)
            .Where(p => p.Date >= cutoffDate)
            .OrderBy(p => p.Date)
            .ToList();
    }

    [KernelFunction]
    [Description("Calculates the current outstanding balance for a customer (sum of all unpaid invoices).")]
    public decimal CalculateOutstandingBalance(
        [Description("The name of the customer")] string customerName)
    {
        var customer = _customerRepository.FetchByName(customerName);
        if (customer is null) return 0m;

        return _invoiceRepository
            .FetchUnpaidByCustomerId(customer.CustomerID)
            .Sum(i => i.Amount);
    }

    [KernelFunction]
    [Description("Gets a list of all customer names in the system.")]
    public List<string> GetAllCustomerNames() =>
        _customerRepository.FetchAll().Select(c => c.Name).ToList();

    [KernelFunction]
    [Description("Gets outstanding balances for all customers. Returns a dictionary with customer names as keys and their outstanding balances as values.")]
    public Dictionary<string, decimal> GetAllOutstandingBalances()
    {
        var result = new Dictionary<string, decimal>();
        var customers = _customerRepository.FetchAll();
        
        foreach (var customer in customers)
        {
            var balance = _invoiceRepository
                .FetchUnpaidByCustomerId(customer.CustomerID)
                .Sum(i => i.Amount);
            
            result[customer.Name] = balance;
        }
        
        return result;
    }
}
