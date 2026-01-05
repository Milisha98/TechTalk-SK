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
     - `payments.csv` - 39 payments with realistic ERP alignment
   - Created Models:
     - `Customer` (with string CustomerID)
     - `Invoice` (with nullable PaidDate)
     - `Payment`
   - Created Repository interfaces and implementations:
     - `ICustomerRepository` / `CustomerRepository`
     - `IInvoiceRepository` / `InvoiceRepository`
     - `IPaymentRepository` / `PaymentRepository`
   - All repositories support LINQ querying via `IQueryable<T>`

3. **DTOs (Data Transfer Objects)**
   - Created basic models: `Customer`, `Invoice`, `Payment`
   - These are returned directly by plugin functions
   - LLM receives structured data to analyze

4. **Environment Configuration**
   - Created `.env` file with OpenAI credentials (gitignored)
   - OPENAI_API_KEY configured
   - OPENAI_MODEL set to gpt-4o
   - DotEnv.Net will load variables at runtime

5. **Redesigned ErpDataPlugin for Automatic Function Calling**
   - Deleted FilterSpec.cs and FilterResult.cs (overcomplicated DTOs)
   - Redesigned plugin with granular KernelFunctions:
     - `GetCustomerByName(string)` - lookup customer
     - `GetInvoicesForCustomer(string, int)` - fetch invoices
     - `GetPaymentsForCustomer(string, int)` - fetch payments
     - `CalculateOutstandingBalance(string)` - compute balance
   - Each function has clear description for LLM
   - Simple parameters, focused responsibility
   - LLM will autonomously call these based on user questions

---

## Remaining Work

### Phase 6: Redesign Kernel Setup for Auto Function Calling
**Purpose:** Expose granular functions that LLM can call autonomously

1. **Remove FilterSpec/FilterResult complexity**
   - Delete `FilterSpec.cs` and `FilterResult.cs`
   - Plugin returns basic models directly (`Customer`, `Invoice`, `Payment`)

2. **Create granular KernelFunctions:**
   - `GetCustomerByName(string customerName)` - lookup customer
   - `GetInvoicesForCustomer(string customerName, int months)` - fetch invoices
   - `GetPaymentsForCustomer(string customerName, int months)` - fetch payments
   - `CalculateOutstandingBalance(string customerName)` - compute balance
   - Each function has clear description for LLM
   - Simple parameters, focused responsibility

3. **Let LLM decide what to call**
   - No predetermined flow
   - LLM analyzes user question and calls needed functions

---

### Phase 6: Redesign Kernel Setup for Auto Function Calling
**Purpose:** Use SK's automatic function calling

1. **Remove manual orchestration code:**
   - Delete `KernelService` with inline prompts
   - Delete `OrchestrationService`

2. **Create simple chat-based service:**
   - Configure kernel with OpenAI chat completion
   - Register ErpDataPlugin
   - Set system prompt ("You are a financial analyst...")
   - Use `ChatHistory` to maintain conversation
   - Call chat completion with `FunctionChoiceBehavior.Auto()`

3. **SK handles everything automatically:**
   - LLM decides which functions to call
   - SK invokes functions
   - LLM analyzes results
   - Returns natural language answer

---

### Phase 7: Gradio.NET Chat UI
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

### Phase 8: Testing & Refinement
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

### Phase 9: Documentation
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
   - Document LINQ queries for data fetching
   - Explain how LLM analyzes raw data to find patterns
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
