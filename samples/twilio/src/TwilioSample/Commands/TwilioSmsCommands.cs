using System.ComponentModel.DataAnnotations;
using Cocona;

namespace TwilioSample;

public sealed class TwilioSmsCommands(TwilioSampleSupport support)
{
    [Command("validate", Description = "Validate a sample SMS message.")]
    public void Validate()
        => support.PrintSmsValidation();

    [Command("status", Description = "Show SMS connector runtime status using saved credentials when available.")]
    public async Task Status()
        => support.PrintStatus("twilio sms status", await support.GetSmsStatusAsync());

    [Command("receive", Description = "Parse a sample Twilio SMS webhook payload.")]
    public async Task Receive(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: form or json.")] string mode = "form")
        => support.PrintReceiveResult($"twilio sms receive {mode}", await support.ReceiveSmsAsync(file, mode));

    [Command("receive-status", Description = "Parse a sample Twilio SMS status callback.")]
    public async Task ReceiveStatus(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: form or json.")] string mode = "form")
        => support.PrintStatusUpdateResult($"twilio sms receive-status {mode}", await support.ReceiveSmsStatusAsync(file, mode));

    [Command("send", Description = "Send a live Twilio SMS message using saved or environment-based settings.")]
    public async Task Send()
        => await support.SendSmsAsync();
}
