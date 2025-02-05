namespace Hue.AI.CLI.Configuration
{
    public class AzureOpenAIConfig
    {
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
        public string Deployment { get; set; }
        public string? Model { get; set; }
    }
}
