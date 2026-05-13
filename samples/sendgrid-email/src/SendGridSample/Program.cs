using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder();
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
builder.Services.AddSingleton<SendGridSample.SendGridSampleSupport>();

var app = builder.Build();
app.AddSubCommand("sendgrid", command => command.AddCommands<SendGridSample.SendGridCommands>());
app.Run();
