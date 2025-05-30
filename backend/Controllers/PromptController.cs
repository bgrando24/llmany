using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class PromptController : ControllerBase
{
    private readonly IPromptService _promptService;

    public PromptController(IPromptService promptService)
        => _promptService = promptService;

    [HttpPost]
    public async Task<ActionResult<AggregatedResponse>> Post([FromBody] PromptRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Prompt))
            return BadRequest("Prompt must not be empty.");

        var agg = await _promptService.FanOutAsync(req, ct);
        return Ok(agg);
    }

    [HttpGet("compare-latest")]
    public async Task<ActionResult<AggregatedResponse?>> GetLatest(CancellationToken ct)
    {
        var latest = await _promptService.CompareLatestAsync(ct);
        if (latest == null)
            return NotFound("No cached responses available.");

        return Ok(latest);
    }
}