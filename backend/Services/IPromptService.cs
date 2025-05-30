public interface IPromptService
{
    Task<AggregatedResponse> FanOutAsync(PromptRequest prompt, CancellationToken ct = default);
    Task<ModelResponse> CompareLatestAsync(CancellationToken ct = default);
}