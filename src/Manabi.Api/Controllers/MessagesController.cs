using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manabi.Api.Services;
using Manabi.Shared.Models.Responses;

namespace Manabi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController(ChatService chatService) : ControllerBase
{
    private string CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    // GET /api/messages/{otherUserId}?skip=0&take=50
    [HttpGet("{otherUserId}")]
    public async Task<ActionResult<List<MessageResponse>>> GetConversation(
        string otherUserId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var messages = await chatService.GetConversationAsync(CurrentUserId, otherUserId, skip, take);
        return Ok(messages);
    }

    // POST /api/messages/{otherUserId}/read
    [HttpPost("{senderId}/read")]
    public async Task<IActionResult> MarkRead(string senderId)
    {
        await chatService.MarkAsReadAsync(CurrentUserId, senderId);
        return NoContent();
    }
}
