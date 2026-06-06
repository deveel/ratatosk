using Cocona;
using Ratatosk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder();
builder.Configuration.SetBasePath(SampleConfigurationStore.GetProjectRoot());
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();

    if (Environment.GetEnvironmentVariable("DEVEEL_VERBOSE") == "true")
    {
        logging
            .SetMinimumLevel(LogLevel.Information)
            .AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
    }
});

builder.Services.AddMessaging()
    .AddClient()
    .AddTelegramBot("telegram", c => c
        .WithSettings("Telegram")
        .WithRetryPolicy(options =>
        {
            options.WithMaxAttempts(3)
                   .WithExponentialBackoff()
                   .WithBaseDelay(TimeSpan.FromSeconds(1))
                   .WithJitter()
                   .RetryOnErrorCodes("RATE_LIMITED", "RETRY_AFTER");
        }));

builder.Services.AddSingleton<Telegram.TelegramSampleSupport>();

var app = builder.Build();
app.AddSubCommand("telegram", command => command.AddCommands<Telegram.TelegramCommands>());
app.Run();
