using Cocona;
using Deveel.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder();
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);

builder.Services.AddLogging(logging =>
{
    logging
        .SetMinimumLevel(LogLevel.Information)
        .AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
});

builder.Services.AddMessaging()
    .AddClient()
    .AddFirebasePush("firebase", c => c.WithSettings("Firebase"));

builder.Services.AddSingleton<Firebase.FirebaseSampleSupport>();

var app = builder.Build();
app.AddSubCommand("firebase", command => command.AddCommands<Firebase.FirebaseCommands>());
app.Run();
