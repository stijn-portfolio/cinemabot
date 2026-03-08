using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Bot.Core.Plugins;

// Load configuration
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var apiKey = config["OpenAI:ApiKey"];
var model = config["OpenAI:Model"] ?? "gpt-4o-mini";

// Create Kernel
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
    modelId: model,
    apiKey: apiKey);

var kernel = builder.Build();

// Register CinemaPlugin
kernel.Plugins.AddFromType<CinemaPlugin>("CinemaPlugin");

// Get chat service
var chat = kernel.GetRequiredService<IChatCompletionService>();

// Setup execution settings
var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Create chat history
var history = new ChatHistory();
history.AddSystemMessage(@"
You are a cinema ticket booking assistant.
You can help users:
- Browse movies by genre
- Check screening times
- Book tickets
- Update or cancel bookings
- View their bookings

When booking tickets, make sure to ask for all required information:
- Screening ID
- Customer name
- Email
- Phone number
- Number of seats
");

Console.WriteLine("🎬 Cinema Bot Test Console");
Console.WriteLine("==========================");
Console.WriteLine("Type your questions or commands.");
Console.WriteLine("Examples:");
Console.WriteLine("  - Show me all movies");
Console.WriteLine("  - Show action movies");
Console.WriteLine("  - When does movie 1 play?");
Console.WriteLine("  - Book 2 tickets for screening 1");
Console.WriteLine();
Console.WriteLine("Type 'exit' to quit.");
Console.WriteLine();

// Chat loop
while (true)
{
    Console.Write("User > ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
    {
        break;
    }

    history.AddUserMessage(userInput);

    try
    {
        var response = await chat.GetChatMessageContentAsync(
            history,
            executionSettings: settings,
            kernel: kernel);

        Console.WriteLine($"Assistant > {response.Content}");
        Console.WriteLine();

        history.AddAssistantMessage(response.Content);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error > {ex.Message}");
        Console.WriteLine();
    }
}

Console.WriteLine("Goodbye! 👋");
