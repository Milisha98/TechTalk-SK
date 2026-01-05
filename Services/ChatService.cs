using dotenv.net;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SK.Plugins;
using SK.Repositories;

namespace SK.Services;

public class ChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ChatHistory _chatHistory;

    public ChatService(
        ICustomerRepository customerRepository,
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        // Load environment variables
        DotEnv.Load();
        
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
            ?? throw new InvalidOperationException("OPENAI_API_KEY not found in environment variables");
        var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") 
            ?? throw new InvalidOperationException("OPENAI_MODEL not found in environment variables");

        // Build kernel with OpenAI chat completion
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: model,
            apiKey: apiKey
        );

        // Register ERP Data Plugin
        var erpPlugin = new ErpDataPlugin(customerRepository, invoiceRepository, paymentRepository);
        builder.Plugins.AddFromObject(erpPlugin, "ErpData");

        _kernel = builder.Build();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();

        // Initialize chat history with system prompt
        _chatHistory = new ChatHistory("""
            You are a financial analyst AI assistant that helps users analyze customer payment data.
            
            You have access to ERP data through the following functions:
            - GetCustomerByName: Look up customer information
            - GetInvoicesForCustomer: Retrieve invoices for a customer
            - GetPaymentsForCustomer: Retrieve payments made by a customer
            - CalculateOutstandingBalance: Calculate current outstanding balance
            
            When analyzing customer financial data, provide insights that cover:
            1. Outstanding Balance: State the current outstanding balance
            2. Payment Timeliness: Analyze payment patterns (late, on-time, early)
            3. Anomalies: Identify unusual patterns (large/small payments, gaps, sudden changes)
            4. Trends: Identify trends (balance changes, behavior changes, concerning patterns)
            
            Be specific with numbers, dates, and amounts. Provide actionable business insights.
            """);
    }

    public async Task<string> ProcessUserMessageAsync(string userMessage)
    {
        try
        {
            // Add user message to chat history
            _chatHistory.AddUserMessage(userMessage);

            // Configure automatic function calling
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            // Get chat completion - SK will automatically call functions as needed
            var response = await _chatService.GetChatMessageContentAsync(
                _chatHistory,
                executionSettings,
                _kernel
            );

            // Add assistant response to chat history
            _chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

            return response.Content ?? "I couldn't generate a response.";
        }
        catch (Exception ex)
        {
            return $"An error occurred while processing your request: {ex.Message}";
        }
    }

    public void ClearHistory() => _chatHistory.Clear();
}
