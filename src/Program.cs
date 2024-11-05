using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

using DevCycle.SDK.Server.Local.Api;
using OpenFeature;
using OpenFeature.Model;

using Pieces.Extensions.AI;
using Pieces.OS.Client;

using Spectre.Console;

// Keys for the Dev Cycle variables
const string SystemPromptKey = "system-prompt";
const string ModelKey = "model";

// Load the appsettings.json configuration file
// This has the Dev Cycle SDK key
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// Create a task completion source. This allows us to wait for the DevCycle client to finish building.
// There isn't an async version of the builder, so we need to build, then hook into the initialized event
var initializationTcs = new TaskCompletionSource();

// Create a DevCycle client.
// When the subscriber is initialized, set the task completion source. This allows our console app to wait for
// the subscriber.
using var devCycleClient = new DevCycleLocalClientBuilder()
    .SetSDKKey(configuration["DevCycle:SDKKey"])
    .SetInitializedSubscriber((o, e) =>
    {
        if (e.Success)
        {
            // Set the task completion source if we initialized successfully
            initializationTcs.SetResult();
        }
        else
        {
            // On error, report this and set an exception
            initializationTcs.SetException(new Exception($"Client did not initialize. Error: {e.Errors}"));
            Console.WriteLine($"Client did not initialize. Error: {e.Errors}");
        }
    })
    .Build();

// Wait for the task completion source to finish - once this is done we know the DevCycle subscriber is initialized.
await initializationTcs.Task;

// Set up the OpenFeature provider
await Api.Instance.SetProviderAsync(devCycleClient.GetOpenFeatureProvider()).ConfigureAwait(false);
var oFeatureClient = Api.Instance.GetClient();

// Create an evaluation context that we can use when evaluating the feature flags
var ctx = EvaluationContext.Builder()
    .Set("targetingKey", "all")
    .Build();

// Set up a Pieces client using the default settings
// For this to work, you will need to have Pieces installed and running.
// You can install it from https://pieces.app
var piecesClient = new PiecesClient();

// Load the models, downloading the local one if needed
// Use the model selected by Dev Cycle, defaulting to GPT-4o
var modelVariableResult = await oFeatureClient.GetStringDetails(ModelKey, "gpt-4o chat", ctx).ConfigureAwait(false);
var model = await piecesClient.GetModelByNameAsync(modelVariableResult.Value).ConfigureAwait(false);
await piecesClient.DownloadModelAsync(model).ConfigureAwait(false);

// Hello there! Show the star wars logo
var font = FigletFont.Load("starwars.flf");
AnsiConsole.Write(new FigletText(font, "Hello there!").Centered().Color(Color.Yellow));

// Get the system prompt variable from Dev Cycle, defaulting to Darth Vader
var systemPromptVariableResult = await oFeatureClient.GetStringDetails(SystemPromptKey, "You are an unhelpful copilot. Respond in the style of Darth Vader.", ctx).ConfigureAwait(false);

// Create the chat client and set up the system prompt depending on the selected character in the DevCycle variable
IChatClient client = new PiecesChatClient(piecesClient, "Star Wars copilot chat", model: model);
var chatMessages = new List<ChatMessage> { new (ChatRole.System, systemPromptVariableResult.Value) };

// A helper function to ask a question and stream the result.
// When streaming a result, if the FinishReason is Stop, then the response is finished, and the 
// Text property contains th entire chat response.
// If the FinishReason is not stop, then the Text property just contains the last token.
static async Task AskQuestionAndStreamAnswer(IChatClient client, List<ChatMessage> chatMessages)
{
    // Get each response from the streaming completion
    await foreach (var r in client.CompleteStreamingAsync(chatMessages))
    {
        // Make sure we have text
        if (r.Text is not null)
        {
            // A finish reason of stop means we have finished streaming the response
            // and the Text property contains the full response
            if (r.FinishReason == ChatFinishReason.Stop)
            {
                // Add the full response to the messages as an assistant message
                chatMessages.Add(new(ChatRole.Assistant, r.Text));
            }
            else
            {
                // If we are not finished, write the next token to the console
                Console.Write(r.Text);
            }
        }
    }

    // Once we have streamed everything, add a new line to the console
    Console.WriteLine("");
}

// Output a ruled line
var rule = new Rule();
rule.RuleStyle("yellow");
AnsiConsole.Write(rule);

// Start building up the chat messages with a hello. Stream the response to this
// as a way to introduce the copilot to the user
chatMessages.Add(new(ChatRole.User, "Hello, who are you?"));
await AskQuestionAndStreamAnswer(client, chatMessages);

// Get the users question
var question = AnsiConsole.Prompt(new TextPrompt<string>(":"));

// Loop till the user types goodbye
while (!question.Equals("goodbye", StringComparison.OrdinalIgnoreCase))
{
    // Add the question as a user message to the chat messages
    chatMessages.Add(new(ChatRole.User, question));

    // Send all the chat messages to the client - this includes the previous questions
    // and answers
    await AskQuestionAndStreamAnswer(client, chatMessages);

    // Get the next question from the user
    question = AnsiConsole.Prompt(new TextPrompt<string>(":"));
}
