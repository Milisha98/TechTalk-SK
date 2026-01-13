# Demo Prompts for Semantic Kernel Demo

## Core Demo Scenario (Primary - Must Work)
Does Acme Automotive have any outstanding balances? If so, summarise their last 6 months of payment behaviour and highlight anything unusual.

## Single Customer Analysis
Show me the current outstanding balance and recent payment behaviour for Blue Horizon Auto Electrics.

## Multi-Customer Comparison
Which members have the highest outstanding balances?

## Risk Identification
Does any member stand out as particularly risky based on late payments and growing balances?

## Error Handling Test
What about XYZ Motors?

---

## Additional Prompts (Optional - For Extended Demo)

### Customer-Specific Queries
- Tell me about Crimson Panel & Paint's payment history
- How is Jupiter Diesel Services performing financially?
- What's the payment pattern for Delta Mobile Mechanics?

### Comparative Analysis
- Compare the payment behaviour of Acme Automotive and Blue Horizon Auto Electrics
- Which member has the best payment record?
- Are there any mustomers with improving payment trends?

### Business Intelligence
- Which members should I be concerned about?
- Who are my most reliable customers?
- Are there any red flags I should be aware of?

### Time-Based Analysis
- What happened with customer payments in the last 3 months?
- Show me payment trends over the last 6 months
- Has anyone's payment behaviour changed recently?

---

## Notes for Demo Recording

1. Start with the core demo scenario (Acme Automotive) - this shows all 4 required categories
2. Follow with Blue Horizon to show contrast (good customer vs problematic customer)
3. Use multi-customer comparison to show cross-customer analysis capability
4. Use risk identification to show business intelligence
5. Optional: Show error handling with invalid customer

The LLM will automatically:
- Call the appropriate plugin functions
- Retrieve the necessary data
- Analyze patterns and anomalies
- Generate natural language insights

No manual orchestration needed - this is Semantic Kernel's automatic function calling in action!
