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

    [KernelFunction, Description("Fetches customer financial data for analysis")]
    public Task<FilterResult> ApplyFilterAsync(
        [Description("Filter specification from user query")] FilterSpec spec)
    {
        // Fetch customer by name
        var customer = _customerRepository.FetchByName(spec.CustomerName ?? string.Empty);
        
        if (customer is null)
        {
            return Task.FromResult(new FilterResult
            {
                CustomerName = spec.CustomerName ?? "Unknown"
            });
        }

        // Calculate date range (last N months)
        var cutoffDate = DateTime.Now.AddMonths(-spec.Months);

        // Fetch invoices for the time period
        var invoices = _invoiceRepository
            .FetchByCustomerId(customer.CustomerID)
            .Where(i => i.DueDate >= cutoffDate)
            .OrderBy(i => i.DueDate)
            .ToList();

        // Fetch payments for the time period
        var payments = _paymentRepository
            .FetchByCustomerId(customer.CustomerID)
            .Where(p => p.Date >= cutoffDate)
            .OrderBy(p => p.Date)
            .ToList();

        // Calculate outstanding balance (basic aggregation)
        var outstandingBalance = invoices
            .Where(i => i.PaidDate is null)
            .Sum(i => i.Amount);

        // Map to InvoiceInfo with calculated DaysLate
        var invoiceInfos = invoices.Select(i => new InvoiceInfo
        {
            InvoiceID = i.InvoiceID,
            Amount = i.Amount,
            DueDate = i.DueDate,
            PaidDate = i.PaidDate,
            DaysLate = i.PaidDate is not null 
                ? (i.PaidDate.Value - i.DueDate).Days 
                : null
        }).ToList();

        // Map to PaymentInfo
        var paymentInfos = payments.Select(p => new PaymentInfo
        {
            PaymentID = p.PaymentID,
            Amount = p.Amount,
            Date = p.Date
        }).ToList();

        var result = new FilterResult
        {
            CustomerID = customer.CustomerID,
            CustomerName = customer.Name,
            Region = customer.Region,
            OutstandingBalance = outstandingBalance,
            Invoices = invoiceInfos,
            Payments = paymentInfos
        };
        
        return Task.FromResult(result);
    }
}
