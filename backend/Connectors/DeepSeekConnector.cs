using System.Diagnostics;
using System.Net.Http.Headers;


/// <summary>
/// A connector for DeepSeek services
/// </summary>
public class DeepSeekConnector : ILLMConnector
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public string ServiceName => "DeepSeek";
    public string ModelName => "deepseek-chat";

    public DeepSeekConnector(IConfiguration config, IHttpClientFactory httpFactory)
    {
        _apiKey = config["DeepSeek:ApiKey"]!;
        _http = httpFactory.CreateClient();
        _http.BaseAddress = new Uri("https://api.deepseek.com/");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ModelResponse> GetResponseAsync(PromptRequest requestPrompt, CancellationToken ct = default)
    {
        //     {
        //     "model": "deepseek-chat",
        //     "messages": [
        //       {"role": "system", "content": "You are a helpful assistant."},
        //       {"role": "user", "content": "Hello!"}
        //     ],
        //     "stream": false
        //   }

        // build request object
        var request = new
        {
            model = ModelName,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = requestPrompt.Prompt
                }
            },
            stream = false // set to true if you want streaming responses
        };

        // use stopwatch for latency measure, and send request
        var sw = Stopwatch.StartNew();
        var response = await _http.PostAsJsonAsync("chat/completions", request, ct);
        sw.Stop();

        // check for successful response, build ModelResponse object to return from 'this' method
        response.EnsureSuccessStatusCode();
        var responsePayload = await response.Content.ReadFromJsonAsync<DeepSeekResponse>(cancellationToken: ct);

        if (responsePayload == null)
        {
            throw new InvalidOperationException("Failed to deserialise DeepSeek response.");
        }

        return new ModelResponse
        {
            ServiceName = ServiceName,
            ModelName = ModelName,
            ResponseText = responsePayload.Choices[0].Message.Content,
            LatencyMs = sw.ElapsedMilliseconds
        };
    }

    private class DeepSeekResponse
    {
        //   {
        //   "id": "879efd34-cc29-4345-80f0-2dcb14bcc0b7",
        //   "object": "chat.completion",
        //   "created": 1748403778,
        //   "model": "deepseek-chat",
        //   "choices": [
        //     {
        //       "index": 0,
        //       "message": {
        //         "role": "assistant",
        //         "content": "Hello! 😊 How can I assist you today?"
        //       },
        //       "logprobs": null,
        //       "finish_reason": "stop"
        //     }
        //   ],
        //   "usage": {
        //     "prompt_tokens": 11,
        //     "completion_tokens": 11,
        //     "total_tokens": 22,
        //     "prompt_tokens_details": {
        //       "cached_tokens": 0
        //     },
        //     "prompt_cache_hit_tokens": 0,
        //     "prompt_cache_miss_tokens": 11
        //   },
        //   "system_fingerprint": "fp_8802369eaa_prod0425fp8"
        // }
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Created { get; set; }
        public string? Model { get; set; }
        public required Choice[] Choices { get; set; }
        public string? SystemFingerprint { get; set; }

        public class Choice
        {
            public int Index { get; set; }
            public required Message Message { get; set; }
            public object? Logprobs { get; set; }
            public string? FinishReason { get; set; }
        }

        public class Message
        {
            public string? Role { get; set; }
            public required string Content { get; set; }
        }

        public class Usage
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
            public PromptTokensDetails? PromptTokensDetails { get; set; }
            public int PromptCacheHitTokens { get; set; }
            public int PromptCacheMissTokens { get; set; }
        }

        public class PromptTokensDetails
        {
            public int CachedTokens { get; set; }
        }
    }
}