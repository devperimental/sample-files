using ExternalApi.Filters;
using Microsoft.AspNetCore.Mvc;
using TestServiceLayer.Shared.Behaviours;

namespace ExternalApi.Controllers;

/// <summary>
/// NotifyController
/// </summary>
[ServiceFilter(typeof(HandleExceptionAttribute))]
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class NotifyController : ControllerBase
{
    private readonly ITestCallback _testCallback;

    /// <summary>
    /// NotifyController
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="testService"></param>
    public NotifyController(ITestCallback testCallback)
    {
        _testCallback = testCallback;
    }

    [HttpPost("partner/{partnerName}")]
    public async Task<IActionResult> GetByKey(string partnerName)
    {
        if (string.IsNullOrEmpty(partnerName))
        {
            return BadRequest();
        }

        using (var streamReader = new StreamReader(HttpContext.Request.Body))
        {
            var json = await streamReader.ReadToEndAsync().ConfigureAwait(true);
            var idempotencyKey = Request.Headers["IdempotencyKey"];
            var requestId = Request.Headers["RequestId"];

            await _testCallback.HandleCallback(json, partnerName, idempotencyKey, requestId).ConfigureAwait(true);
        }

        return Ok();
    }
}
