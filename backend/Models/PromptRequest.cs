/// <summary>
/// The PromptRequest class represents a user's request to forward on their prompt to the connected LLM services
/// </summary>
public class PromptRequest
{
    /// <summary>
    /// The user-provided text prompt to be forwarded to LLM services
    /// </summary>
    public string Prompt { get; set; } = string.Empty;
}