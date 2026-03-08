namespace Bot.API.Models;

public class ChatRequestDto
{
    public required string SessionId { get; set; }
    public required string Message { get; set; }
}
