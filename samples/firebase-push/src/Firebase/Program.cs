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
    .AddFirebasePush("firebase", c => c
        .WithSettings("Firebase")
        .WithRetryPolicy(options =>
        {
            options.WithMaxAttempts(3)
                   .WithExponentialBackoff()
                   .WithBaseDelay(TimeSpan.FromSeconds(1))
                   .WithJitter()
                   .RetryOnErrorCodes("UNAVAILABLE", "INTERNAL", "DEADLINE_EXCEEDED");
        }));

builder.Services.AddSingleton<Firebase.FirebaseSampleSupport>();

var app = builder.Build();
app.AddSubCommand("firebase", command => command.AddCommands<Firebase.FirebaseCommands>());
app.Run();
