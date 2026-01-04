# Semantic Kernel Demo - Implementation Plan

## Progress Overview

### ✅ Completed
1. **Project Setup**
   - NuGet packages installed:
     - Microsoft.SemanticKernel 1.68.0
     - Gradio.Net 0.5.0
     - DotEnv.Net 4.0.0
   - .gitignore configured (including .env files)

2. **Data Layer (Repository Pattern)**
   - Created CSV data files in `Data/` folder:
     - `customers.csv` - 10 automotive industry businesses
     - `invoices.csv` - 50 invoices with varied payment patterns
     - `payments.csv` - 45 payments including Acme's anomalous large payments
   - Created Models:
     - `Customer` (with string CustomerID)
     - `Invoice` (with nullable PaidDate)
     - `Payment`
   - Created Repository interfaces and implementations:
     - `ICustomerRepository` / `CustomerRepository`
     - `IInvoiceRepository` / `InvoiceRepository`
     - `IPaymentRepository` / `PaymentRepository`
   - All repositories support LINQ querying via `IQueryable<T>`

---

## Remaining Work

### Phase 1: Data Transfer Objects (DTOs)
**Purpose:** Define structured objects for communication between SK components

1. **Create `Models/FilterSpec.cs`**
   - Properties:
     - `customerName` (string)
     - `months` (int)
     - `includeOutstanding` (bool)
     - `includeTrends` (bool)
     - `includeAnomalies` (bool)
     - `scope` (string: "singleCustomer" or "multiCustomer")

2. **Create `Models/FilterResult.cs`**
   - Properties:
     - Customer information
     - Outstanding balance (decimal)
     - List of invoices
     - List of payments
     - Payment timeliness metrics
     - Anomaly descriptions (list of strings)
     - Trend indicators (string or structured)

---

### Phase 2: ERP Data Plugin
**Purpose:** SK plugin to process financial data using LINQ

1. **Create `Plugins/ErpDataPlugin.cs`**
   - Constructor: inject repositories
   - Method: `LoadDataAsync()` - initialize all repositories
   - Method: `ApplyFilterAsync(FilterSpec spec)` - main processing logic

2. **Implement LINQ-based calculations:**
   - Outstanding balance calculation
   - Payment timeliness analysis (days late/early)
   - Payment frequency analysis
   - Anomaly detection:
     - Unusually large payments (e.g., 2+ standard deviations)
     - Long payment gaps
   - Trend calculation:
     - Rolling outstanding balance over time
     - Detect upward/downward trends

3. **Return structured `FilterResult`**

---

### Phase 3: Environment Configuration
**Purpose:** Securely manage OpenAI API credentials

1. **Create `.env` file** (in project root, already gitignored)
   - `OPENAI_API_KEY=<key>`
   - `OPENAI_MODEL=gpt-4o` (or gpt-4.5)

2. **Create configuration loader**
   - Use DotEnv.Net to load environment variables
   - Validate required variables are present

---

### Phase 4: Semantic Kernel Setup
**Purpose:** Configure SK with semantic functions

1. **Create `Prompts/` folder structure**
   - `Prompts/NLToFilter/` - natural language to FilterSpec
     - `skprompt.txt` - prompt template
     - `config.json` - function configuration
   - `Prompts/ResultsToInsight/` - FilterResult to natural language
     - `skprompt.txt` - prompt template
     - `config.json` - function configuration

2. **Write prompt templates:**
   - **NLToFilter prompt:**
     - Input: user question + schema description
     - Output: JSON FilterSpec
     - Include examples for few-shot learning
   
   - **ResultsToInsight prompt:**
     - Input: structured FilterResult (JSON)
     - Output: natural language business insights
     - Must cover: outstanding balance, timeliness, anomalies, trends

3. **Create Kernel builder**
   - Configure OpenAI chat completion
   - Load semantic functions from prompts
   - Register ErpDataPlugin

---

### Phase 5: Orchestration
**Purpose:** Chain SK components together

1. **Create `Services/OrchestrationService.cs`**
   - Method: `ProcessUserMessageAsync(string userMessage)`
   - Steps:
     1. Call NL→FilterSpec semantic function
     2. Parse JSON response into FilterSpec
     3. Call `ErpDataPlugin.ApplyFilterAsync(spec)`
     4. Serialize FilterResult to JSON
     5. Call Results→Insight semantic function
     6. Return final natural language answer

2. **Add error handling:**
   - Invalid JSON parsing
   - Repository errors
   - OpenAI API errors
   - Return user-friendly error messages

---

### Phase 6: Gradio.NET Chat UI
**Purpose:** Web-based chat interface for demo

1. **Update `Program.cs`**
   - Load environment variables
   - Initialize repositories and load data
   - Create Kernel with SK configuration
   - Create OrchestrationService instance

2. **Build Gradio interface:**
   - Create `gr.Blocks()` layout
   - Add `gr.Chatbot()` component
   - Add `gr.Textbox()` for user input
   - Wire Submit event to `ProcessUserMessageAsync`
   - Display responses in chat history

3. **Optional enhancements:**
   - Show intermediate FilterSpec (collapsible)
   - Show raw metrics (collapsible)
   - Add example questions as buttons

4. **Launch the app:**
   - `App.Launch()` with appropriate configuration

---

### Phase 7: Testing & Refinement
**Purpose:** Ensure demo stability for recording

1. **Test core demo scenario:**
   - "Does Acme Automotive have any outstanding balances? If so, summarise their last 6 months of payment behaviour and highlight anything unusual."
   - Verify all four categories present:
     - Outstanding balance
     - Payment timeliness
     - Anomalies
     - Trends

2. **Test additional questions:**
   - Blue Horizon Logistics query
   - Multi-customer comparison
   - Risk identification

3. **Refine prompts if needed:**
   - Improve JSON parsing reliability
   - Enhance insight quality
   - Ensure consistent output format

4. **Error handling:**
   - Test with invalid customer names
   - Test with ambiguous queries
   - Ensure graceful degradation

---

### Phase 8: Documentation
**Purpose:** Enable others to run the demo

1. **Create `README.md`**
   - Overview of the demo
   - Prerequisites (.NET 8, OpenAI API key)
   - Setup instructions:
     - Clone repository
     - Create .env file with API key
     - Run `dotnet restore`
     - Run `dotnet run`
   - Example questions to try
   - Architecture overview

2. **Add code comments:**
   - Document complex LINQ queries
   - Explain anomaly detection logic
   - Describe plugin methods

---

## Implementation Notes

- **Stick to console application** - keep it simple for instructional purposes
- **Use repository pattern** - already implemented for CSV data access
- **Expression-bodied members** - for single-line functions
- **Use `is not null`** - instead of `!= null` per latest C# guidelines
- **Run `dotnet build`** - after code changes to verify compilation

---

## Success Criteria

1. Demo runs entirely locally (only external dependency: OpenAI)
2. All test questions produce accurate, insightful responses
3. Code is clean, readable, and well-documented
4. Demo is stable enough for video recording
5. Clearly demonstrates SK's multi-step reasoning capabilities
