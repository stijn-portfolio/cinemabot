using Bot.Core.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Concurrent;

namespace Bot.API.Services;

public class KernelService
{
    private readonly ConcurrentDictionary<string, ChatHistory> _sessions = new();
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly OpenAIPromptExecutionSettings _settings;

    public KernelService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key not configured");
        var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        var cinemaApiUrl = configuration["CinemaAPI:BaseUrl"] ?? "http://localhost:5001";

        // Build kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: model,
            apiKey: apiKey);

        _kernel = builder.Build();

        // Register CinemaPlugin
        _kernel.Plugins.AddFromObject(new CinemaPlugin(cinemaApiUrl), "CinemaPlugin");

        // Get chat service
        _chat = _kernel.GetRequiredService<IChatCompletionService>();

        // Configure function calling
        _settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
    }

    public async Task<string> GetResponseAsync(string sessionId, string userMessage)
    {
        // Get or create chat history for this session
        var history = _sessions.GetOrAdd(sessionId, _ => CreateNewChatHistory());

        // Add user message
        history.AddUserMessage(userMessage);

        // Get AI response (with automatic function calling)
        var response = await _chat.GetChatMessageContentAsync(
            history,
            executionSettings: _settings,
            kernel: _kernel);

        // Add assistant response to history
        if (!string.IsNullOrEmpty(response.Content))
        {
            history.AddAssistantMessage(response.Content);
        }

        return response.Content ?? "I apologize, but I couldn't process your request.";
    }

    private ChatHistory CreateNewChatHistory()
    {
        var history = new ChatHistory();
        history.AddSystemMessage(@"
You are a cinema ticket booking assistant.

You can help users:
- Browse all movies or filter by genre
- Check screening times for specific movies
- Book tickets for screenings
- Update existing bookings (change number of seats)
- Cancel bookings
- View their bookings by email

When showing movies or screenings, present the information in a friendly, conversational way.

When a user wants to book tickets, make sure to collect ALL required information:
1. Screening ID (from the list of screenings)
2. Customer name
3. Email address
4. Phone number
5. Number of seats

Be helpful, friendly, and guide users through the booking process step by step.
");

        return history;
    }

    public void ClearSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }

    public int GetActiveSessionsCount()
    {
        return _sessions.Count;
    }
}
