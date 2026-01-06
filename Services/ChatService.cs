using dotenv.net;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SK.Plugins;
using SK.Repositories;

namespace SK.Services;

/// <summary>
/// Service that handles chat interactions using Semantic Kernel's automatic function calling.
/// This is where the "magic" happens - SK automatically orchestrates function calls based on user questions.
/// </summary>
public class ChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ChatHistory _chatHistory;

    /// <summary>
    /// Initializes the ChatService with Semantic Kernel configuration.
    /// Sets up automatic function calling and the system prompt.
    /// </summary>
    public ChatService(
        ICustomerRepository customerRepository,
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        // Load OpenAI credentials from .env file
        DotEnv.Load();
        
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
            ?? throw new InvalidOperationException("OPENAI_API_KEY not found in environment variables");
        var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") 
            ?? throw new InvalidOperationException("OPENAI_MODEL not found in environment variables");

        // Configure Semantic Kernel with OpenAI chat completion
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: model,
            apiKey: apiKey
        );

        // Register the ERP Data Plugin - makes all KernelFunctions available to the LLM
        var erpPlugin = new ErpDataPlugin(customerRepository, invoiceRepository, paymentRepository);
        builder.Plugins.AddFromObject(erpPlugin, "ErpData");

        _kernel = builder.Build();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();

        // System prompt guides the LLM on its role and analysis structure
        // This prompt tells the LLM:
        // 1. What role to play (financial analyst)
        // 2. What functions are available
        // 3. What structure to follow in responses (balance, timeliness, anomalies, trends)
        // 4. To be specific with data (numbers, dates, amounts)
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

    /// <summary>
    /// Processes a user message using Semantic Kernel's automatic function calling.
    /// 
    /// How it works:
    /// 1. User message is added to chat history
    /// 2. FunctionChoiceBehavior.Auto() tells SK to automatically call functions
    /// 3. LLM analyzes the question and decides which plugin functions to call
    /// 4. SK invokes the functions with appropriate parameters
    /// 5. LLM receives the returned data and analyzes it
    /// 6. LLM generates a natural language response with insights
    /// 
    /// No manual orchestration needed - this is SK's core capability!
    /// </summary>
    public async Task<string> ProcessUserMessageAsync(string userMessage)
    {
        try
        {
            // Add user message to conversation history
            _chatHistory.AddUserMessage(userMessage);

            // FunctionChoiceBehavior.Auto() is the key to automatic function calling
            // The LLM will autonomously decide which functions to call
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            // SK orchestrates everything automatically:
            // - LLM analyzes user question
            // - LLM calls plugin functions as needed (can be multiple calls)
            // - LLM receives structured data from functions
            // - LLM generates natural language insights
            var response = await _chatService.GetChatMessageContentAsync(
                _chatHistory,
                executionSettings,
                _kernel
            );

            // Add response to history for conversation continuity
            _chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

            return response.Content ?? "I couldn't generate a response.";
        }
        catch (Exception ex)
        {
            return $"An error occurred while processing your request: {ex.Message}";
        }
    }

    /// <summary>
    /// Clears the conversation history. Useful for starting a fresh conversation.
    /// </summary>
    public void ClearHistory() => _chatHistory.Clear();
}
