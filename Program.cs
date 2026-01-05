using SK.Repositories;
using SK.Services;

var chatService = await InitializeApplicationAsync();
 await RunChatLoopAsync(chatService);

static async Task<ChatService> InitializeApplicationAsync()
{
    var customerRepository = new CustomerRepository("Data/customers.csv");
    var invoiceRepository = new InvoiceRepository("Data/invoices.csv");
    var paymentRepository = new PaymentRepository("Data/payments.csv");

    await customerRepository.LoadDataAsync();
    await invoiceRepository.LoadDataAsync();
    await paymentRepository.LoadDataAsync();

    Console.WriteLine("ERP data loaded successfully.\n");

    return new ChatService(customerRepository, invoiceRepository, paymentRepository);
}

static async Task RunChatLoopAsync(ChatService chatService)
{
    Console.WriteLine("Semantic Kernel Demo");
    Console.WriteLine("===================\n");
    Console.WriteLine("Ask questions about customer financial data.");
    Console.WriteLine("Type 'exit' to quit.\n");
    
    Console.WriteLine("Example questions:");
    Console.WriteLine("1. Does Acme Automotive have any outstanding balances? If so, summarise their last 6 months of payment behaviour and highlight anything unusual.");
    Console.WriteLine("2. Show me the current outstanding balance and recent payment behaviour for Blue Horizon Auto Electrics.");
    Console.WriteLine("3. Which customers have the highest outstanding balances?\n");

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
}
