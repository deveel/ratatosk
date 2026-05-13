using System.ComponentModel.DataAnnotations;
using Cocona;

namespace Telegram;

public sealed class TelegramCommands(TelegramSampleSupport support)
{
    [Command("schema", Description = "Show all Telegram schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for Telegram credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();

    [Command("validate", Description = "Validate a sample Telegram message.")]
    public void Validate([Option('k', Description = "Sample kind: text or location.")] string kind = "text")
        => support.PrintValidation(kind);

    [Command("status", Description = "Show Telegram connector status when saved credentials are available.")]
    public async Task Status()
        => await support.PrintStatusAsync();

    [Command("send", Description = "Build and send a live Telegram message interactively.")]
    public async Task Send()
        => await support.SendAsync();
}
