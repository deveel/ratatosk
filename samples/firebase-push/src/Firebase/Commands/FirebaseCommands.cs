using System.ComponentModel.DataAnnotations;
using Cocona;

namespace Firebase;

public sealed class FirebaseCommands(FirebaseSampleSupport support)
{
    [Command("schema", Description = "Show all Firebase schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for Firebase credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();

    [Command("validate", Description = "Validate a sample Firebase message.")]
    public void Validate([Option('k', Description = "Sample kind: device or topic.")] string kind = "device")
        => support.PrintValidation(kind);

    [Command("send", Description = "Build and send a live Firebase notification interactively.")]
    public async Task Send()
        => await support.SendAsync();

    [Command("batch", Description = "Send a live Firebase batch to a device token.")]
    public async Task Batch()
        => await support.SendBatchAsync();

    [Command("status", Description = "Show Firebase connector status using saved credentials when available.")]
    public async Task Status()
        => await support.PrintStatusAsync();
}
