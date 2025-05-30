using System.Diagnostics;
using System.Net.Http.Headers;


/// <summary>
/// A connector for Anthropic LLM services
/// </summary>
public class AnthropicConnector : ILLMConnector
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public string ServiceName => "Anthropic";
    // public string ModelName => "claude-opus-4-20250514";
    public string ModelName => "claude-3-5-haiku-latest";

    public AnthropicConnector(IConfiguration config, IHttpClientFactory httpFactory)
    {
        _apiKey = config["Anthropic:ApiKey"]!;
        _http = httpFactory.CreateClient();
        _http.BaseAddress = new Uri("https://api.anthropic.com/v1/");
        _http.DefaultRequestHeaders.Add("x-api-key", config["Anthropic:ApiKey"]!);
        _http.DefaultRequestHeaders.Add("Anthropic-Version", "2023-06-01");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ModelResponse> GetResponseAsync(PromptRequest requestPrompt, CancellationToken ct = default)
    {
        // build request object
        var request = new
        {
            model = ModelName,
            max_tokens = 1024,
            temperature = 1,
            system = "Provide your response using Markdown syntax",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = requestPrompt.Prompt
                }
            },
        };

        // use stopwatch for latency measure, and send request
        var sw = Stopwatch.StartNew();
        var response = await _http.PostAsJsonAsync("messages", request, ct);
        sw.Stop();

        // check for successful response, build ModelResponse object to return from 'this' method
        response.EnsureSuccessStatusCode();
        var responsePayload = await response.Content.ReadFromJsonAsync<AnthropicResponse>(cancellationToken: ct);

        if (responsePayload == null)
        {
            throw new InvalidOperationException("Failed to deserialise Anthropic response.");
        }

        return new ModelResponse
        {
            ServiceName = ServiceName,
            ModelName = ModelName,
            ResponseText = responsePayload.Content[0].Text,
            LatencyMs = sw.ElapsedMilliseconds
        };
    }

    private class AnthropicResponse
    {
        // {
        //   "content": [
        //     {
        //       "text": "Hi! My name is Claude.",
        //       "type": "text"
        //     }
        //   ],
        //   "id": "msg_013Zva2CMHLNnXjNJJKqJ2EF",
        //   "model": "claude-3-7-sonnet-20250219",
        //   "role": "assistant",
        //   "stop_reason": "end_turn",
        //   "stop_sequence": null,
        //   "type": "message",
        //   "usage": {
        //     "input_tokens": 2095,
        //     "output_tokens": 503
        //   }
        // }
        public string? Id { get; set; }
        public string? Model { get; set; }
        public string? Role { get; set; }
        public string? StopReason { get; set; }
        public object? StopSequence { get; set; }
        public required Content[] Content { get; set; }
        public Usage Usage { get; set; } = new Usage();
    }

    private class Content
    {
        public required string Text { get; set; }
        public string? Type { get; set; }
    }

    private class Usage
    {
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }
}