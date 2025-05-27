/// <summary>
/// The AggregatedResponse class is used to help encapsulate the responses from multiple LLM services into a single interface
/// </summary>
public class AggregatedResponse
{
    public string Prompt { get; set; } = string.Empty;
    public List<ModelResponse> Responses { get; set; } = [];
}