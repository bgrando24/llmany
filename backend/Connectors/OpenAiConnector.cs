using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


/// <summary>
/// A connector for OpenAI services, implementing the ILLMConnector interface
/// </summary>
public class OpenAiConnector : ILLMConnector
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public string ServiceName => "OpenAI";
    // public string ModelName => "gpt-4.1";
    public string ModelName => "o4-mini";

    public OpenAiConnector(IConfiguration config, IHttpClientFactory httpFactory)
    {
        _apiKey = config["OpenAI:ApiKey"]!;
        _http = httpFactory.CreateClient();
        _http.BaseAddress = new Uri("https://api.openai.com/v1/");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ModelResponse> GetResponseAsync(PromptRequest requestPrompt, CancellationToken ct = default)
    {
        // build request object
        var request = new
        {
            instructions = "Provide your response in Markdown syntax",
            model = ModelName,
            input = requestPrompt.Prompt
        };

        // Log the outgoing request JSON
        // var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
        // Console.WriteLine("OpenAI request payload:\n" + json);

        // Log headers
        // Console.WriteLine("OpenAI request headers:");
        // foreach (var header in _http.DefaultRequestHeaders)
        // {
        //     Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        // }

        // use stopwatch for latency measure, and send request
        var sw = Stopwatch.StartNew();
        var response = await _http.PostAsJsonAsync("responses", request, ct);
        sw.Stop();

        // check for successful response, build ModelResponse object to return from 'this' method
        response.EnsureSuccessStatusCode();
        var responsePayload = await response.Content.ReadFromJsonAsync<OpenAiResponse>(cancellationToken: ct);

        if (responsePayload == null)
        {
            throw new InvalidOperationException("Failed to deserialize OpenAI response.");
        }

        if (responsePayload.Output == null || responsePayload.Output.Count == 0)
        {
            throw new InvalidOperationException("OpenAI response does not contain any output.");
        }

        var firstMessageOutput = responsePayload.Output.FirstOrDefault(o => o.Type == "message" && o.Content != null);

        if (firstMessageOutput == null || firstMessageOutput.Content == null || firstMessageOutput.Content.Count == 0)
        {
            throw new InvalidOperationException("OpenAI response does not contain valid message content.");
        }

        return new ModelResponse
        {
            ServiceName = ServiceName,
            ModelName = ModelName,
            ResponseText = firstMessageOutput.Content[0].Text,
            TokensUsed = responsePayload.Usage.TotalTokens,
            LatencyMs = sw.Elapsed.TotalMilliseconds
        };
    }

    private class OpenAiResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Object { get; set; } = string.Empty;
        public long CreatedAt { get; set; }
        public string? Status { get; set; }
        public bool Background { get; set; }
        public object? Error { get; set; }
        public object? IncompleteDetails { get; set; }
        public string? Instructions { get; set; }
        public object? MaxOutputTokens { get; set; }
        public string Model { get; set; } = string.Empty;
        public List<Output> Output { get; set; } = new();
        public bool ParallelToolCalls { get; set; }
        public object? PreviousResponseId { get; set; }
        public Reasoning Reasoning { get; set; } = new();
        public string ServiceTier { get; set; } = string.Empty;
        public bool Store { get; set; }
        public double Temperature { get; set; }
        public TextFormat Text { get; set; } = new();
        public string? ToolChoice { get; set; }
        public List<object> Tools { get; set; } = new();
        public double TopP { get; set; }
        public string? Truncation { get; set; }
        public Usage Usage { get; set; } = new();
        public object? User { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    private class Output
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? Role { get; set; }
        public List<Content>? Content { get; set; } // Content is optional
        public List<object>? Summary { get; set; } // Summary is optional
    }

    private class Content
    {
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<object> Annotations { get; set; } = new();
    }

    private class Reasoning
    {
        public string Effort { get; set; } = string.Empty;
        public object? Summary { get; set; }
    }

    private class TextFormat
    {
        public Format Format { get; set; } = new();
    }

    private class Format
    {
        public string Type { get; set; } = string.Empty;
    }

    private class Usage
    {
        public int InputTokens { get; set; }
        public InputTokensDetails InputTokensDetails { get; set; } = new();
        public int OutputTokens { get; set; }
        public OutputTokensDetails OutputTokensDetails { get; set; } = new();
        public int TotalTokens { get; set; }
    }

    private class InputTokensDetails
    {
        public int CachedTokens { get; set; }
    }

    private class OutputTokensDetails
    {
        public int ReasoningTokens { get; set; }
        public int AudioTokens { get; set; }
        public int AcceptedPredictionTokens { get; set; }
        public int RejectedPredictionTokens { get; set; }
    }
}