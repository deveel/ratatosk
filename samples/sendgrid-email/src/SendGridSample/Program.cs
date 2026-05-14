using Cocona;
using Deveel.Messaging;
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
    .AddSendGridEmail("sendgrid", c => c.WithSettings("SendGrid"));

builder.Services.AddSingleton<SendGridSample.SendGridSampleSupport>();

var app = builder.Build();
app.AddSubCommand("sendgrid", command => command.AddCommands<SendGridSample.SendGridCommands>());
app.Run();
