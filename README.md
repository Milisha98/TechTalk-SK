# Semantic Kernel Demo - Financial Analysis

A demonstration of Microsoft Semantic Kernel's automatic function calling capabilities using a financial analysis scenario with ERP-style data.

## Overview

This demo showcases how Semantic Kernel can:
- **Understand natural language** business questions
- **Automatically decide** which functions to call to retrieve data
- **Analyze structured data** from multiple sources (invoices, payments, customers)
- **Identify patterns, trends, and anomalies** in financial data
- **Generate actionable business insights** in natural language

Unlike traditional chatbots, this demo demonstrates **multi-step reasoning** where the LLM autonomously orchestrates multiple function calls to answer complex business questions.

## What Makes This Different?

**GitHub Copilot** and **O365 Copilot** are great for code completion and document editing, but they don't execute multi-step reasoning pipelines that interact with structured data through plugins.

**Semantic Kernel** enables:
- Automatic function calling (LLM decides what to call)
- Multi-step orchestration (multiple function calls per query)
- Structured data analysis (CSV, databases, APIs)
- Business intelligence generation

## Prerequisites

- **.NET 9.0 SDK** or later
- **OpenAI API Key** (GPT-4o recommended)
- A text editor or IDE (Visual Studio, VS Code, Rider, etc.)

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd SK
```

### 2. Create .env File

Create a `.env` file in the project root with your OpenAI credentials:

```
OPENAI_API_KEY=your-api-key-here
OPENAI_MODEL=gpt-4o
```

**Important:** The `.env` file is gitignored to protect your API key.

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Run the Demo

```bash
dotnet run
```

The application will:
1. Load customer, invoice, and payment data from CSV files
2. Initialize the Semantic Kernel with your OpenAI configuration
3. Start an interactive console chat interface

## Example Questions

### Core Demo Question
```
Does Acme Automotive have any outstanding balances? If so, summarise their last 6 months of payment behaviour and highlight anything unusual.
```

This demonstrates all four analysis categories:
- Outstanding balance
- Payment timeliness  
- Anomalies
- Trends

### Other Queries

**Single Customer Analysis:**
```
Show me the current outstanding balance and recent payment behaviour for Blue Horizon Auto Electrics.
```

**Multi-Customer Comparison:**
```
Which customers have the highest outstanding balances?
```

**Risk Identification:**
```
Does any customer stand out as particularly risky based on late payments and growing balances?
```

See `Instructions/Prompts.md` for more example questions.

## How It Works

### Architecture

```
User Question (Natural Language)
    ↓
ChatService (Semantic Kernel)
    ↓
LLM decides which functions to call
    ↓
ErpDataPlugin (KernelFunctions)
    ├─ GetCustomerByName()
    ├─ GetInvoicesForCustomer()
    ├─ GetPaymentsForCustomer()
    ├─ CalculateOutstandingBalance()
    ├─ GetAllCustomerNames()
    └─ GetAllOutstandingBalances()
    ↓
Repositories (Data Access)
    ├─ CustomerRepository
    ├─ InvoiceRepository
    └─ PaymentRepository
    ↓
CSV Files (Data/customers.csv, invoices.csv, payments.csv)
    ↓
LLM analyzes returned data
    ↓
Natural Language Response
```

### Key Components

**1. ErpDataPlugin**
- Exposes granular functions that the LLM can call
- Each function has clear descriptions for the LLM
- Functions return structured data (Customer, Invoice, Payment objects)

**2. ChatService**
- Configures Semantic Kernel with automatic function calling
- Uses `FunctionChoiceBehavior.Auto()` - LLM decides what to call
- Maintains conversation history via `ChatHistory`
- System prompt guides the LLM to act as a financial analyst

**3. Repository Pattern**
- Clean separation between data access and business logic
- CSV files loaded into memory as strongly-typed objects
- LINQ queries for filtering and aggregation

**4. Automatic Function Calling**
- **No manual orchestration** - SK handles everything
- LLM analyzes the user's question
- LLM decides which functions to call and with what parameters
- LLM can call multiple functions per query
- LLM analyzes the returned data to generate insights

## Data Structure

### customers.csv
```
CustomerID,Name,ABN,Region
102847,Acme Automotive,12345678901,VIC
...
```

### invoices.csv
```
InvoiceID,CustomerID,Amount,DueDate,PaidDate
INV001,102847,12500.00,2024-07-15,2024-08-20
...
```

### payments.csv
```
PaymentID,CustomerID,Amount,Date
PAY001,102847,12500.00,2024-08-20
...
```

## Project Structure

```
SK/
├── Data/                    # CSV data files
│   ├── customers.csv
│   ├── invoices.csv
│   └── payments.csv
├── Models/                  # Data models
│   ├── Customer.cs
│   ├── Invoice.cs
│   └── Payment.cs
├── Repositories/            # Data access layer (Repository Pattern)
│   ├── ICustomerRepository.cs
│   ├── CustomerRepository.cs
│   ├── IInvoiceRepository.cs
│   ├── InvoiceRepository.cs
│   ├── IPaymentRepository.cs
│   └── PaymentRepository.cs
├── Plugins/                 # Semantic Kernel plugins
│   └── ErpDataPlugin.cs
├── Services/                # Application services
│   └── ChatService.cs
├── Instructions/            # Documentation
│   ├── Overview.md
│   ├── Plan.md
│   ├── TestResults.md
│   └── Prompts.md
├── Program.cs               # Application entry point
└── .env                     # OpenAI configuration (gitignored)
```

## Testing

Comprehensive testing was performed in Phase 9. See `Instructions/TestResults.md` for:
- Test scenarios and results
- Identified issues and resolutions
- Success criteria validation

All tests passed with excellent results.

## Development Notes

- Uses **repository pattern** for clean data access
- Follows **.NET conventions**: `is not null` instead of `!= null`
- Uses **expression-bodied members** for single-line functions
- **Console interface** - simple, universally understood baseline

## Success Criteria ✅

- ✅ Demo runs entirely locally (only OpenAI dependency)
- ✅ All test questions produce accurate, insightful responses
- ✅ Code is clean, readable, and well-documented
- ✅ Demo is stable for video recording
- ✅ Clearly demonstrates SK's multi-step reasoning

## Resources

- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Automatic Function Calling](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/)
- [OpenAI Function Calling](https://platform.openai.com/docs/guides/function-calling)

## License

This is a demonstration project for educational purposes.
