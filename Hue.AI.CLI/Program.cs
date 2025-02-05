using System;
using System.Reflection;
using Hue.AI.CLI;
using Hue.AI.CLI.Configuration;
using Hue.AI.CLI.Plugins;
using HueApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

// Create the host builder
var builder = Host.CreateDefaultBuilder(args);

// Load the configuration file and user secrets
//
// These need to be set either directly in the configuration.json file or in the user secrets. Details are in
// the configuration.json file.
var configurationFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "configuration.json");
builder.ConfigureAppConfiguration((builder) => builder
    .AddJsonFile(configurationFilePath, true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>());

// Configure the services for the host
builder.ConfigureServices((context, services) =>
{
    // Setup configuration options
    var configurationRoot = context.Configuration;

    services.AddSingleton<HueInformation>();

    services.AddOptions<AzureOpenAIConfig>()
                     .Bind(configurationRoot.GetSection(nameof(AzureOpenAIConfig)));

    services.AddOptions<HueConfig>()
                    .Bind(configurationRoot.GetSection(nameof(HueConfig)));

    services.AddSingleton<LocalHueApi>(sp =>
    {
        var hueConfig = sp.GetRequiredService<IOptions<HueConfig>>().Value;

        var hueApi = new LocalHueApi(hueConfig.BridgeIp, hueConfig.ApiKey);
        return hueApi;
    });


    // Chat completion service that kernels will use
    services.AddSingleton<IChatCompletionService>(sp =>
    {
        var azureOpenAIOptions = sp.GetRequiredService<IOptions<AzureOpenAIConfig>>().Value;

        return new AzureOpenAIChatCompletionService(azureOpenAIOptions.Deployment, azureOpenAIOptions.Endpoint, azureOpenAIOptions.ApiKey);
    });

    services.AddSingleton<Kernel>((sp) =>
    {
        // Create a collection of plugins that the kernel will use
        KernelPluginCollection pluginCollection = [];
        pluginCollection.AddFromObject(sp.GetRequiredService<HueInformation>());

        //Kernel kernel = kernelBuilder.Build();

        // When created by the dependency injection container, Semantic Kernel logging is included by default
        return new Kernel(sp, pluginCollection);
    });

    // Add the primary hosted service to start the loop.
    services.AddHostedService<ConsoleGPTService>();
});

// Build and run the host. This keeps the app running using the HostedService.
var host = builder.Build();
await host.RunAsync();