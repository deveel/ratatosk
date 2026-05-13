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
builder.Services.AddSingleton<TwilioSample.TwilioSampleSupport>();

var app = builder.Build();
app.AddSubCommand("twilio", command =>
{
    command.AddCommands<TwilioSample.TwilioRootCommands>();
    command.AddSubCommand("sms", sms => sms.AddCommands<TwilioSample.TwilioSmsCommands>());
    command.AddSubCommand("whatsapp", whatsapp => whatsapp.AddCommands<TwilioSample.TwilioWhatsAppCommands>());
});
app.Run();
