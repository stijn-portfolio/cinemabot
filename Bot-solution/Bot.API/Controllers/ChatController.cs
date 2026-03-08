using Bot.API.Models;
using Bot.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly KernelService _kernelService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(KernelService kernelService, ILogger<ChatController> logger)
    {
        _kernelService = kernelService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.SessionId))
            {
                return BadRequest(new { error = "SessionId is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message is required" });
            }

            _logger.LogInformation("Chat request from session {SessionId}: {Message}",
                request.SessionId, request.Message);

            // Get response from Kernel (with automatic function calling)
            var response = await _kernelService.GetResponseAsync(
                request.SessionId,
                request.Message);

            _logger.LogInformation("Chat response: {Response}", response);

            // For now, return simple response
            // In Phase 4 (Teams), we'll add adaptive card detection and building
            return Ok(new ChatResponseDto
            {
                Response = response,
                Card = null // TODO: Detect JSON and build adaptive cards
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return StatusCode(500, new { error = "An error occurred processing your request" });
        }
    }

    [HttpDelete("{sessionId}")]
    public IActionResult ClearSession(string sessionId)
    {
        _kernelService.ClearSession(sessionId);
        return Ok(new { message = "Session cleared" });
    }

    [HttpGet("sessions/count")]
    public IActionResult GetSessionsCount()
    {
        var count = _kernelService.GetActiveSessionsCount();
        return Ok(new { activeSessionsCount = count });
    }
}
