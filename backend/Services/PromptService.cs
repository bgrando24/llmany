/// <summary>
/// The IPromptService interface defines the contract for a service that handles user prompts and aggregates responses from multiple LLM services
/// </summary>
public class PromptService : IPromptService
{
    private readonly IEnumerable<ILLMConnector> _connectors;

    public PromptService(IEnumerable<ILLMConnector> connectors) => _connectors = connectors;

    public async Task<AggregatedResponse> FanOutAsync(PromptRequest prompt, CancellationToken ct = default)
    {
        var tasks = _connectors
            .Select(c => SafeCall(c, prompt, ct))
            .ToArray();

        var results = await Task.WhenAll(tasks);
        return new AggregatedResponse
        {
            Prompt = prompt.Prompt,
            Responses = results.Where(r => r != null)!.Select(r => r!).ToList()
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