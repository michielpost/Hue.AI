using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Hue.AI.CLI
{
    /// <summary>
    /// This is the main application service.
    /// This takes console input, then sends it to the configured AI service, and then prints the response.
    /// All conversation history is maintained in the chat history.
    /// </summary>
    internal class ConsoleGPTService : IHostedService
    {
        private readonly Kernel kernel;
        private readonly IHostApplicationLifetime _lifeTime;

        public ConsoleGPTService(Kernel kernel, IHostApplicationLifetime lifeTime)
        {
            this.kernel = kernel;
            _lifeTime = lifeTime;
        }

        /// <summary>
        /// Start the service.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => ExecuteAsync(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop a running service.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// The main execution loop. This awaits input and responds to it using semantic kernel functions.
        /// </summary>
        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            string systemMsg = "You are an AI assistant that helps giving information about lights in a home and can set colors on a light and activate light scenes.";
            //Create new chat
            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            var chat = new ChatHistory(
                    systemMessage: systemMsg
                );

            Console.WriteLine("I am your Hue AI assistant. I can give information about your lights, set colors or turn them off.");
            Console.WriteLine("How can I help you?");
            Console.WriteLine();

            while (true)
            {
                var input = Console.ReadLine();
                if(input == "exit")
                {
                    break;
                }

                //Reset
                //if (chat.Count > 5)
                //{
                //    chat = new ChatHistory(
                //    systemMessage: systemMsg
                //);
                //    Console.WriteLine("FORCED CONTEXT RESET");
                //}

                chat.AddUserMessage(input);

                AzureOpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
                var executionSettings = new AzureOpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    //MaxTokens = 100,
                    //Temperature = 0.2
                };

                var answer = await chatService.GetChatMessageContentAsync(chat, executionSettings, kernel);
                chat.AddAssistantMessage(answer.Content!);
                Console.WriteLine("AI: " + answer);
                Console.WriteLine();

            }
            Console.WriteLine("done");

            Console.WriteLine();
        }
    }
}
