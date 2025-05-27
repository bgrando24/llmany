public interface ILLMConnector
{
    /// <summary>
    /// The service providing the response (e.g. OpenAI, Anthropic)
    /// </summary>
    public string ServiceName { get; }
    /// <summary>
    /// The specific model used (e.g. gpt-3.5-turbo, claude-sonnet-4)
    /// </summary>
    public string ModelName { get; }

    /// <summary>
    /// Sends a user's prompt to the connected LLM services asynchronously
    /// </summary>
    /// <param name="request">The user's prompt request containing the text to be processed</param>
    /// <returns>A Task that represents the asynchronous operation, containing the model's response</returns>
    Task<ModelResponse> GetResponseAsync(PromptRequest request, CancellationToken ct = default);
}