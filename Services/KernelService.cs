using dotenv.net;
using Microsoft.SemanticKernel;
using SK.Plugins;
using SK.Repositories;

namespace SK.Services;

public class KernelService
{
    private readonly Kernel _kernel;
    private readonly KernelFunction _nlToFilterFunction;
    private readonly KernelFunction _resultsToInsightFunction;

    public KernelService(
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

        // Create inline prompt functions
        _nlToFilterFunction = CreateNLToFilterFunction();
        _resultsToInsightFunction = CreateResultsToInsightFunction();
    }

    public Kernel Kernel => _kernel;
    public KernelFunction NLToFilterFunction => _nlToFilterFunction;
    public KernelFunction ResultsToInsightFunction => _resultsToInsightFunction;

    private KernelFunction CreateNLToFilterFunction()
    {
        var prompt = @"
You are a business intelligence assistant that converts natural language questions into structured filter specifications.

Given a user question about customer financial data, extract the following information and return it as JSON:

FilterSpec JSON Schema:
{
  ""CustomerName"": ""string - name of the customer"",
  ""Months"": ""integer - number of months to analyze"",
  ""IncludeOutstanding"": ""boolean - whether to include outstanding balance"",
  ""IncludeTrends"": ""boolean - whether to analyze trends"",
  ""IncludeAnomalies"": ""boolean - whether to identify anomalies"",
  ""Scope"": ""string - 'singleCustomer' or 'multiCustomer'""
}

Examples:

User: ""Does Acme Automotive have any outstanding balances? If so, summarise their last 6 months of payment behaviour and highlight anything unusual.""
Output:
{
  ""CustomerName"": ""Acme Automotive"",
  ""Months"": 6,
  ""IncludeOutstanding"": true,
  ""IncludeTrends"": true,
  ""IncludeAnomalies"": true,
  ""Scope"": ""singleCustomer""
}

User: ""Show me the current outstanding balance and recent payment behaviour for Blue Horizon Auto Electrics.""
Output:
{
  ""CustomerName"": ""Blue Horizon Auto Electrics"",
  ""Months"": 6,
  ""IncludeOutstanding"": true,
  ""IncludeTrends"": true,
  ""IncludeAnomalies"": false,
  ""Scope"": ""singleCustomer""
}

User Question: {{$input}}

Return ONLY the JSON, no additional text or explanation.
";

        return _kernel.CreateFunctionFromPrompt(prompt);
    }

    private KernelFunction CreateResultsToInsightFunction()
    {
        var prompt = @"
You are a financial analyst AI that analyzes customer payment data and provides business insights.

You will receive structured data about a customer's financial history in JSON format, including:
- Customer information
- Outstanding balance (already calculated)
- List of invoices with amounts, due dates, paid dates, and days late
- List of payments with amounts and dates

Your task is to analyze this raw data and provide a comprehensive business insight that covers:

1. **Outstanding Balance**: State the current outstanding balance
2. **Payment Timeliness**: Analyze the payment patterns - are they consistently late, on-time, or early? By how many days on average?
3. **Anomalies**: Identify any unusual patterns such as:
   - Unusually large or small payments
   - Payment gaps or irregular timing
   - Multiple invoices paid at once
   - Sudden changes in payment behavior
4. **Trends**: Identify trends in the data:
   - Is the outstanding balance increasing or decreasing?
   - Has payment behavior changed over time?
   - Any concerning patterns developing?

Be specific with numbers, dates, and amounts. Provide actionable insights.

Customer Financial Data:
{{$input}}

Provide your analysis:
";

        return _kernel.CreateFunctionFromPrompt(prompt);
    }
}
