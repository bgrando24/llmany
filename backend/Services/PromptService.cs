using Microsoft.Extensions.Caching.Memory;


/// <summary>
/// The IPromptService interface defines the contract for a service that handles user prompts and aggregates responses from multiple LLM services
/// </summary>
public class PromptService : IPromptService
{
    private readonly IEnumerable<ILLMConnector> _connectors;
    private readonly IMemoryCache _cache;

    public PromptService(IEnumerable<ILLMConnector> connectors, IMemoryCache cache)
    {
        _connectors = connectors;
        _cache = cache;
    }

    public async Task<AggregatedResponse> FanOutAsync(PromptRequest prompt, CancellationToken ct = default)
    {
        var tasks = _connectors
            .Select(c => SafeCall(c, prompt, ct))
            .ToArray();

        var results = await Task.WhenAll(tasks);
        var aggResponses = new AggregatedResponse
        {
            Prompt = prompt.Prompt,
            Responses = results.Where(r => r != null)!.Select(r => r!).ToList()
        };

        // store latest responses in memory cache
        _cache.Set("LatestResponses", aggResponses, TimeSpan.FromMinutes(10));
        _cache.Set("LatestPrompt", prompt.Prompt, TimeSpan.FromMinutes(10));

        return aggResponses;
    }

    /// <summary>
    /// Retrieve latest aggregated responses from memory cache
    /// </summary>
    /// <returns>
    /// An AggregatedResponse containing the latest responses, or null if not found
    /// </returns>
    public AggregatedResponse? GetLatestResponses()
    {
        // try to get the latest responses from memory cache
        if (_cache.TryGetValue("LatestResponses", out AggregatedResponse? cachedResponse))
        {
            return cachedResponse;
        }
        return null;
    }

    public async Task<ModelResponse> CompareLatestAsync(CancellationToken ct)
    {
        var latestResponses = GetLatestResponses();
        if (latestResponses == null || latestResponses.Responses.Count == 0)
        {
            throw new InvalidOperationException("No cached responses available for comparison");
        }

        // build a comparison prompt from the latest responses
        var instructionPromptText = "Compare the following responses from the given LLM services. Provide a concise summary particularly highlighting any consistent, and/or conflicting, information between the responses. Note, the responses use Markdown syntax. Please also use Markdown syntax.\n";
        string comparisonPromptText = "";
        foreach (var response in latestResponses.Responses)
        {
            comparisonPromptText += $"\nService - {response.ServiceName}:\n{response.ResponseText}\n";
        }

        // use chatGPT for comparison
        var comparisonConnector = _connectors.FirstOrDefault(c => c.ServiceName == "OpenAI");

        if (comparisonConnector == null)
        {
            throw new InvalidOperationException("No comparison connector found for OpenAI");
        }

        var comparisonPrompt = new PromptRequest
        {
            Prompt = comparisonPromptText,
            Instructions = instructionPromptText
        };

        var result = await comparisonConnector.GetResponseAsync(comparisonPrompt, ct) ?? throw new InvalidOperationException("Failed to get comparison response from OpenAI");

        return new ModelResponse
        {
            ServiceName = comparisonConnector.ServiceName,
            ModelName = comparisonConnector.ModelName,
            ResponseText = result.ResponseText,
            TokensUsed = result.TokensUsed,
            LatencyMs = result.LatencyMs
        };
    }

    private async Task<ModelResponse?> SafeCall(ILLMConnector c, PromptRequest prompt, CancellationToken ct)
    {
        try
        {
            return await c.GetResponseAsync(prompt, ct);
        }
        catch (Exception ex)
        {
            // log failure, return a placeholder or null
            Console.Error.WriteLine($"Error from {c.ModelName}: {ex.Message}");

            return null;
        }
    }
}