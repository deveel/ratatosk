using System.ComponentModel.DataAnnotations;
using Cocona;

namespace TwilioSample;

public sealed class TwilioWhatsAppCommands(TwilioSampleSupport support)
{
    [Command("validate", Description = "Validate a sample WhatsApp message.")]
    public void Validate([Option('k', Description = "Sample kind: text or template.")] string kind = "text")
        => support.PrintWhatsAppValidation(kind);

    [Command("status", Description = "Show WhatsApp connector runtime status using saved credentials when available.")]
    public async Task Status()
        => support.PrintStatus("twilio whatsapp status", await support.GetWhatsAppStatusAsync());

    [Command("receive", Description = "Parse a sample Twilio WhatsApp webhook payload.")]
    public async Task Receive(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: form or json.")] string mode = "form")
        => support.PrintReceiveResult($"twilio whatsapp receive {mode}", await support.ReceiveWhatsAppAsync(file, mode));

    [Command("receive-status", Description = "Parse a sample Twilio WhatsApp status callback.")]
    public async Task ReceiveStatus(
        [Option('f', Description = "Optional payload file path.")] string? file = null,
        [Option('m', Description = "Payload mode: form or json.")] string mode = "form")
        => support.PrintStatusUpdateResult($"twilio whatsapp receive-status {mode}", await support.ReceiveWhatsAppStatusAsync(file, mode));

    [Command("send", Description = "Build and send a live Twilio WhatsApp message interactively.")]
    public async Task Send()
        => await support.SendWhatsAppAsync();
}
