using System.ComponentModel;
using Microsoft.SemanticKernel;
using SK.Models;
using SK.Repositories;

namespace SK.Plugins;

/// <summary>
/// Semantic Kernel plugin that provides access to ERP financial data.
/// Each method is a KernelFunction that the LLM can automatically call.
/// The LLM analyzes the user's question and autonomously decides which functions to invoke.
/// </summary>
public class ErpDataPlugin
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IInvoiceRepository  _invoiceRepository;
    private readonly IPaymentRepository  _paymentRepository;

    public ErpDataPlugin(ICustomerRepository customerRepository,
                         IInvoiceRepository  invoiceRepository,
                         IPaymentRepository  paymentRepository)
    {
        _customerRepository = customerRepository;
        _invoiceRepository  = invoiceRepository;
        _paymentRepository  = paymentRepository;
    }

    /// <summary>
    /// KernelFunction that looks up a customer by business name.
    /// The Description attribute tells the LLM when to call this function.
    /// </summary>
    [KernelFunction]
    [Description("Looks up a customer by their business name. Returns customer details if found, null otherwise.")]
    public Customer? GetCustomerByName(
        [Description("The name of the customer business to look up")] string customerName) =>
        _customerRepository.FetchByName(customerName);

    /// <summary>
    /// KernelFunction that looks up a customer by their CustomerID (also known as MemberID).
    /// Useful when the user references a customer by their ID number.
    /// </summary>
    [KernelFunction]
    [Description("Looks up a customer by their CustomerID or MemberID. Returns customer details if found, null otherwise.")]
    public Customer? GetCustomerById(
        [Description("The CustomerID or MemberID to look up")] string customerId) =>
        _customerRepository.FetchById(customerId);

    /// <summary>
    /// KernelFunction that retrieves invoices for a specific customer.
    /// Uses LINQ to filter invoices by date range and sort by due date.
    /// Returns structured Invoice objects that the LLM can analyze.
    /// </summary>
    [KernelFunction]
    [Description("Gets all invoices for a customer within a specified time period. Includes invoice amounts, due dates, and payment dates.")]
    public List<Invoice> GetInvoicesForCustomer(
        [Description("The name of the customer")] string customerName,
        [Description("Number of months to look back (e.g., 6 for last 6 months)")] int months = 6)
    {
        var customer = _customerRepository.FetchByName(customerName);
        if (customer is null) return new List<Invoice>();

        // Calculate cutoff date for filtering invoices
        var cutoffDate = DateTime.Now.AddMonths(-months);
        
        // LINQ query: filter by customer and date, then sort chronologically
        return _invoiceRepository
            .FetchByCustomerId(customer.CustomerID)
            .Where(i => i.DueDate >= cutoffDate)
            .OrderBy(i => i.DueDate)
            .ToList();
    }

    /// <summary>
    /// KernelFunction that retrieves payment history for a specific customer.
    /// The LLM uses this to analyze payment patterns and timeliness.
    /// </summary>
    [KernelFunction]
    [Description("Gets all payments made by a customer within a specified time period. Includes payment amounts and dates.")]
    public List<Payment> GetPaymentsForCustomer(
        [Description("The name of the customer")] string customerName,
        [Description("Number of months to look back (e.g., 6 for last 6 months)")] int months = 6)
    {
        var customer = _customerRepository.FetchByName(customerName);
        if (customer is null) return new List<Payment>();

        var cutoffDate = DateTime.Now.AddMonths(-months);
        
        // LINQ query: filter payments by customer and date range
        return _paymentRepository
            .FetchByCustomerId(customer.CustomerID)
            .Where(p => p.Date >= cutoffDate)
            .OrderBy(p => p.Date)
            .ToList();
    }

    /// <summary>
    /// KernelFunction that retrieves invoices using CustomerID.
    /// </summary>
    [KernelFunction]
    [Description("Gets all invoices for a customer using their CustomerID or MemberID within a specified time period.")]
    public List<Invoice> GetInvoicesForCustomerById(
        [Description("The CustomerID or MemberID")] string customerId,
        [Description("Number of months to look back (e.g., 6 for last 6 months)")] int months = 6)
    {
        var cutoffDate = DateTime.Now.AddMonths(-months);
        
        return _invoiceRepository
            .FetchByCustomerId(customerId)
            .Where(i => i.DueDate >= cutoffDate)
            .OrderBy(i => i.DueDate)
            .ToList();
    }

    /// <summary>
    /// KernelFunction that retrieves payment history using CustomerID.
    /// </summary>
    [KernelFunction]
    [Description("Gets all payments made by a customer using their CustomerID or MemberID within a specified time period.")]
    public List<Payment> GetPaymentsForCustomerById(
        [Description("The CustomerID or MemberID")] string customerId,
        [Description("Number of months to look back (e.g., 6 for last 6 months)")] int months = 6)
    {
        var cutoffDate = DateTime.Now.AddMonths(-months);
        
        return _paymentRepository
            .FetchByCustomerId(customerId)
            .Where(p => p.Date >= cutoffDate)
            .OrderBy(p => p.Date)
            .ToList();
    }

    /// <summary>
    /// KernelFunction that calculates total outstanding balance for a customer.
    /// Aggregates all unpaid invoices using LINQ Sum.
    /// </summary>
    [KernelFunction]
    [Description("Calculates the current outstanding balance for a customer (sum of all unpaid invoices).")]
    public decimal CalculateOutstandingBalance(
        [Description("The name of the customer")] string customerName)
    {
        var customer = _customerRepository.FetchByName(customerName);
        if (customer is null) return 0m;

        // LINQ aggregation: sum amounts of all unpaid invoices
        return _invoiceRepository
            .FetchUnpaidByCustomerId(customer.CustomerID)
            .Sum(i => i.Amount);
    }

    /// <summary>
    /// KernelFunction that calculates total outstanding balance using CustomerID.
    /// </summary>
    [KernelFunction]
    [Description("Calculates the current outstanding balance for a customer using their CustomerID or MemberID (sum of all unpaid invoices).")]
    public decimal CalculateOutstandingBalanceById(
        [Description("The CustomerID or MemberID")] string customerId)
    {
        // LINQ aggregation: sum amounts of all unpaid invoices
        return _invoiceRepository
            .FetchUnpaidByCustomerId(customerId)
            .Sum(i => i.Amount);
    }

    /// <summary>
    /// KernelFunction that returns all customer names.
    /// Enables the LLM to discover available customers for comparative analysis.
    /// </summary>
    [KernelFunction]
    [Description("Gets a list of all customer names in the system.")]
    public List<string> GetAllCustomerNames() =>
        _customerRepository.FetchAll().Select(c => c.Name).ToList();

    /// <summary>
    /// KernelFunction that calculates outstanding balances for ALL customers.
    /// Enables multi-customer comparisons and risk analysis.
    /// The LLM can use this to identify customers with highest balances or compare risk levels.
    /// </summary>
    [KernelFunction]
    [Description("Gets outstanding balances for all customers. Returns a dictionary with customer names as keys and their outstanding balances as values.")]
    public Dictionary<string, decimal> GetAllOutstandingBalances()
    {
        var result = new Dictionary<string, decimal>();
        var customers = _customerRepository.FetchAll();
        
        // Calculate outstanding balance for each customer
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
