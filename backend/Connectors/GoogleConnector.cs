using System.Diagnostics;
using System.Net.Http.Headers;


/// <summary>
/// A connector for Google Gemini LLM services
/// </summary>
public class GoogleConnector : ILLMConnector
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public string ServiceName => "Google Gemini";
    // public string ModelName => "gemini-2.5-pro-preview-05-06";
    public string ModelName => "gemini-2.0-flash";

    public GoogleConnector(IConfiguration config, IHttpClientFactory httpFactory)
    {
        _apiKey = config["Google:ApiKey"]!;
        _http = httpFactory.CreateClient();
        _http.BaseAddress = new Uri($"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={_apiKey}");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ModelResponse> GetResponseAsync(PromptRequest requestPrompt, CancellationToken ct = default)
    {
        //  {          
        //     "contents": [
        //       {           
        //         "parts": [
        //           {                                              
        //             "text": "Explain how AI works in a few words"
        //           }
        //         ]
        //       }
        //     ]
        //   }

        // build request object
        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = requestPrompt.Prompt
                        }
                    }
                }
            },
        };

        // use stopwatch for latency measure, and send request
        var sw = Stopwatch.StartNew();
        var response = await _http.PostAsJsonAsync("", request, ct);
        sw.Stop();

        // check for successful response, build ModelResponse object to return from 'this' method
        response.EnsureSuccessStatusCode();
        var responsePayload = await response.Content.ReadFromJsonAsync<GoogleGeminiResponse>(cancellationToken: ct);

        if (responsePayload == null)
        {
            throw new InvalidOperationException("Failed to deserialise Google Gemini response.");
        }

        return new ModelResponse
        {
            ServiceName = ServiceName,
            ModelName = ModelName,
            ResponseText = responsePayload.Candidates[0].Content.Parts[0].Text,
            LatencyMs = sw.ElapsedMilliseconds
        };
    }

    private class GoogleGeminiResponse
    {
        //{
        //   "candidates": [
        //     {
        //       "content": {
        //         "parts": [
        //           {
        //             "text": "AI works by learning patterns from data to make predictions or decisions.\n"
        //           }
        //         ],
        //         "role": "model"
        //       },
        //       "finishReason": "STOP",
        //       "avgLogprobs": -0.027852905648095266
        //     }
        //   ],
        //   "usageMetadata": {
        //     "promptTokenCount": 8,
        //     "candidatesTokenCount": 14,
        //     "totalTokenCount": 22,
        //     "promptTokensDetails": [
        //       {
        //         "modality": "TEXT",
        //         "tokenCount": 8
        //       }
        //     ],
        //     "candidatesTokensDetails": [
        //       {
        //         "modality": "TEXT",
        //         "tokenCount": 14
        //       }
        //     ]
        //   },
        //   "modelVersion": "gemini-2.0-flash",
        //   "responseId": "6Yg2aJSPAoi27dcPkdXY8QI"
        // }

        public required Candidate[] Candidates { get; set; }
        public string? ModelVersion { get; set; }
        public string? ResponseId { get; set; }

        public class Candidate
        {
            public required Content Content { get; set; }
            public string? FinishReason { get; set; }
            public double AvgLogprobs { get; set; }
        }

        public class Content
        {
            public required Parts[] Parts { get; set; }
            public string? Role { get; set; }
        }

        public class Parts
        {
            public required string Text { get; set; }
        }

        public class UseageMetadata
        {
            public int PromptTokenCount { get; set; }
            public int CandidatesTokenCount { get; set; }
            public int TotalTokenCount { get; set; }
            public PromptTokensDetails[]? PromptTokensDetails { get; set; }
            public CandidatesTokensDetails[]? CandidatesTokensDetails { get; set; }
        }
        public class PromptTokensDetails
        {
            public string? Modality { get; set; }
            public int TokenCount { get; set; }
        }

        public class CandidatesTokensDetails
        {
            public string? Modality { get; set; }
            public int TokenCount { get; set; }
        }

    }
}