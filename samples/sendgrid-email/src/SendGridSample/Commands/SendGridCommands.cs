using System.ComponentModel.DataAnnotations;
using Cocona;

namespace SendGridSample;

public sealed class SendGridCommands(SendGridSampleSupport support)
{
    [Command("schema", Description = "Show all SendGrid schemas or a single schema.")]
    public void Schema([Argument(Description = "Optional schema name.")] string? name = null)
        => support.PrintSchemas(name);

    [Command("configure", Description = "Prompt for SendGrid credentials and save them to the local app configuration file.")]
    public void Configure()
        => support.Configure();

    [Command("validate", Description = "Validate a sample SendGrid message.")]
    public void Validate([Option('k', Description = "Sample kind: html or template.")] string kind = "html")
        => support.PrintValidation(kind);

    [Command("status", Description = "Show the connector runtime status using saved credentials when available.")]
    public async Task Status()
        => support.PrintStatus("sendgrid status", await support.GetStatusAsync());

    [Command("receive", Description = "Parse a sample inbound SendGrid webhook.")]
    public async Task Receive(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: json or form.")] string mode = "json")
        => support.PrintReceiveResult($"sendgrid receive {mode}", await support.ReceiveAsync(file, mode));

    [Command("receive-status", Description = "Parse a sample SendGrid delivery event callback.")]
    public async Task ReceiveStatus(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: json or form.")] string mode = "json")
        => support.PrintStatusUpdateResult($"sendgrid receive-status {mode}", await support.ReceiveStatusAsync(file, mode));

    [Command("send", Description = "Build and send a live SendGrid message interactively.")]
    public async Task Send()
        => await support.SendAsync();
}
