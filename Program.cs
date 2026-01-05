using SK.Repositories;
using SK.Services;

// Initialize repositories
var customerRepository = new CustomerRepository("Data/customers.csv");
var invoiceRepository = new InvoiceRepository("Data/invoices.csv");
var paymentRepository = new PaymentRepository("Data/payments.csv");

// Load data from CSV files
await customerRepository.LoadDataAsync();
await invoiceRepository.LoadDataAsync();
await paymentRepository.LoadDataAsync();

// Create chat service with automatic function calling
var chatService = new ChatService(customerRepository, invoiceRepository, paymentRepository);

Console.WriteLine("Semantic Kernel Demo");
Console.WriteLine("===================\n");
Console.WriteLine("Ask questions about customer financial data.");
Console.WriteLine("Type 'exit' to quit.\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(input)) continue;
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
    
    Console.WriteLine("\nAssistant: ");
    var response = await chatService.ProcessUserMessageAsync(input);
    Console.WriteLine(response);
    Console.WriteLine();
}
