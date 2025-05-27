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
    public string ModelName => "gpt-4.1";

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
            throw new InvalidOperationException("Failed to deserialise OpenAI response.");
        }

        return new ModelResponse
        {
            ModelName = ModelName,
            ResponseText = responsePayload.Output[0].Content[0].Text,
            TokensUsed = responsePayload.Usage.TotalTokens,
            LatencyMs = sw.Elapsed.TotalMilliseconds
        };
    }

    private class OpenAiResponse
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long CreatedAt { get; set; }
        public string? Status { get; set; }
        public object? Error { get; set; }
        public object? IncompleteDetails { get; set; }
        public object? Instructions { get; set; }
        public object? MaxOutputTokens { get; set; }
        public string? Model { get; set; }
        public required Output[] Output { get; set; }
        public bool ParallelToolCalls { get; set; }
        public object? PreviousResponseId { get; set; }
        public Reasoning? Reasoning { get; set; }
        public bool Store { get; set; }
        public double Temperature { get; set; }
        public TextFormat? Text { get; set; }
        public string? ToolChoice { get; set; }
        public object[]? Tools { get; set; }
        public double TopP { get; set; }
        public string? Truncation { get; set; }
        public required Usage Usage { get; set; }
        public object? User { get; set; }
        public object? Metadata { get; set; }
    }

    private class Output
    {
        public string? Type { get; set; }
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }
        public required Content[] Content { get; set; }
    }

    private class Content
    {
        public string? Type { get; set; }
        public required string Text { get; set; }
        public object[]? Annotations { get; set; }
    }

    private class Reasoning
    {
        public object? Effort { get; set; }
        public object? Summary { get; set; }
    }

    private class TextFormat
    {
        public Format? Format { get; set; }
    }

    private class Format
    {
        public string? Type { get; set; }
    }

    private class Usage
    {
        public int InputTokens { get; set; }
        public InputTokensDetails? InputTokensDetails { get; set; }
        public int OutputTokens { get; set; }
        public OutputTokensDetails? OutputTokensDetails { get; set; }
        public int TotalTokens { get; set; }
    }

    private class InputTokensDetails
    {
        public int CachedTokens { get; set; }
    }

    private class OutputTokensDetails
    {
        public int ReasoningTokens { get; set; }
    }
}