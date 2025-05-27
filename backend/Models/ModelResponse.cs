/// <summary>
/// The ModelResponse class provides a single interface for receiving and utilising responses from connected LLM services
/// </summary>
public class ModelResponse
{
    /// <summary>
    /// The service providing the response (e.g. OpenAI, Anthropic)
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    /// <summary>
    /// The specific model used (e.g. gpt-3.5-turbo, claude-sonnet-4)
    /// </summary>
    public string ModelName { get; set; } = string.Empty;
    public string ResponseText { get; set; } = null!;
    public int TokensUsed { get; set; }
    /// <summary>
    /// The latency of the response in milliseconds. 
    /// Measured from when the request is sent to when the response is received in full.
    /// </summary>
    public double LatencyMs { get; set; }
}