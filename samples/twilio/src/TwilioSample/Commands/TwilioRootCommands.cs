using System.ComponentModel.DataAnnotations;
using Cocona;

namespace TwilioSample;

public sealed class TwilioRootCommands(TwilioSampleSupport support)
{
    [Command("schema", Description = "Show all Twilio schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for Twilio credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();
}
