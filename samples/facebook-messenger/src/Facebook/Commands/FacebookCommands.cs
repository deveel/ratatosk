using System.ComponentModel.DataAnnotations;
using Cocona;

namespace Facebook;

public sealed class FacebookCommands(FacebookSampleSupport support)
{
    [Command("schema", Description = "Show all Facebook schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for Facebook credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();

    [Command("validate", Description = "Validate a sample Facebook message.")]
    public void Validate([Option('k', Description = "Sample kind: text or media.")] string kind = "text")
        => support.PrintValidation(kind);

    [Command("status", Description = "Show the connector runtime status using saved credentials when available.")]
    public async Task Status()
        => support.PrintStatus("facebook status", await support.GetStatusAsync());

    [Command("receive", Description = "Parse a sample Facebook webhook payload.")]
    public async Task Receive([Option('f', Description = "Optional JSON file path.")] string? file = null)
        => support.PrintReceiveResult("facebook receive", await support.ReceiveAsync(file));

    [Command("send", Description = "Build and send a live Facebook message interactively.")]
    public async Task Send()
        => await support.SendAsync();
}
