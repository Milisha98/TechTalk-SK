using System.Text.Json;
using Microsoft.SemanticKernel;
using SK.Models;

namespace SK.Services;

public class OrchestrationService
{
    private readonly KernelService _kernelService;

    public OrchestrationService(KernelService kernelService)
    {
        _kernelService = kernelService;
    }

    public async Task<string> ProcessUserMessageAsync(string userMessage)
    {
        try
        {
            // Step 1: Call NL → FilterSpec semantic function
            var filterSpecResult = await _kernelService.Kernel.InvokeAsync(
                _kernelService.NLToFilterFunction,
                new KernelArguments { ["input"] = userMessage }
            );

            var filterSpecJson = filterSpecResult.ToString();
            
            // Step 2: Parse JSON into FilterSpec
            FilterSpec? filterSpec;
            try
            {
                filterSpec = JsonSerializer.Deserialize<FilterSpec>(
                    filterSpecJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (filterSpec is null)
                {
                    return "I couldn't understand your question. Please try rephrasing it.";
                }
            }
            catch (JsonException ex)
            {
                return $"I had trouble parsing your request. Error: {ex.Message}\nReceived: {filterSpecJson}";
            }

            // Step 3: Call ErpDataPlugin.ApplyFilterAsync
            var plugin = _kernelService.Kernel.Plugins["ErpData"];
            var applyFilterFunction = plugin["ApplyFilterAsync"];
            
            var filterResultObj = await _kernelService.Kernel.InvokeAsync(
                applyFilterFunction,
                new KernelArguments { ["spec"] = filterSpec }
            );

            var filterResult = filterResultObj.GetValue<FilterResult>();

            if (filterResult is null || string.IsNullOrEmpty(filterResult.CustomerID))
            {
                return $"I couldn't find a customer named '{filterSpec.CustomerName}'. Please check the name and try again.";
            }

            // Step 4: Serialize FilterResult to JSON
            var filterResultJson = JsonSerializer.Serialize(filterResult, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            // Step 5: Pass to Results → Insight semantic function
            var insightResult = await _kernelService.Kernel.InvokeAsync(
                _kernelService.ResultsToInsightFunction,
                new KernelArguments { ["input"] = filterResultJson }
            );

            // Step 6: Return final natural language answer
            return insightResult.ToString();
        }
        catch (Exception ex)
        {
            return $"An error occurred while processing your request: {ex.Message}";
        }
    }
}
