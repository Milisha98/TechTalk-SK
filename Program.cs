using SK.Repositories;
using SK.Services;

// Application entry point
// 1. Initialize repositories and load CSV data
// 2. Create ChatService with Semantic Kernel configuration
// 3. Start interactive console chat loop
var chatService = await InitializeApplicationAsync();
await RunChatLoopAsync(chatService);

/// <summary>
/// Initializes the application by loading data and creating the ChatService.
/// </summary>
static async Task<ChatService> InitializeApplicationAsync()
{
    // Create repositories with paths to CSV files
    var customerRepository = new CustomerRepository("Data/customers.csv");
    var invoiceRepository = new InvoiceRepository("Data/invoices.csv");
    var paymentRepository = new PaymentRepository("Data/payments.csv");

    // Load all CSV data into memory
    await customerRepository.LoadDataAsync();
    await invoiceRepository.LoadDataAsync();
    await paymentRepository.LoadDataAsync();

    Console.WriteLine("ERP data loaded successfully.\n");

    // Create ChatService with Semantic Kernel automatic function calling
    return new ChatService(customerRepository, invoiceRepository, paymentRepository);
}

/// <summary>
/// Runs the interactive console chat loop.
/// Users can ask natural language questions about customer financial data.
/// The LLM will automatically call plugin functions and generate insights.
/// </summary>
static async Task RunChatLoopAsync(ChatService chatService)
{
    Console.WriteLine("Semantic Kernel Demo");
    Console.WriteLine("===================\n");
    Console.WriteLine("Ask questions about customer financial data.");
    Console.WriteLine("Type 'exit' to quit.\n");
    
    // Display example questions to guide users
    Console.WriteLine("Example questions:");
    Console.WriteLine("1. Does Acme Automotive have any outstanding balances? If so, summarise their last 6 months of payment behaviour and highlight anything unusual.");
    Console.WriteLine("2. Show me the current outstanding balance and recent payment behaviour for Blue Horizon Auto Electrics.");
    Console.WriteLine("3. Which customers have the highest outstanding balances?\n");

    // Interactive chat loop
    while (true)
    {
        Console.Write("You: ");
        var input = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(input)) continue;
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
        
        // Send user message to ChatService
        // SK will automatically:
        // 1. Analyze the question
        // 2. Call appropriate plugin functions
        // 3. Analyze the returned data
        // 4. Generate natural language insights
        Console.WriteLine("\nAssistant: ");
        var response = await chatService.ProcessUserMessageAsync(input);
        Console.WriteLine(response);
        Console.WriteLine();
    }
}
