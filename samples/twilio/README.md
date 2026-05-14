# Twilio SMS & WhatsApp Sample

This sample demonstrates how to use the Deveel Messaging Twilio connectors to send SMS and WhatsApp messages via the Twilio API.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- A [Twilio](https://www.twilio.com/) account
- A Twilio phone number (for SMS) or WhatsApp-enabled number (for WhatsApp)

## Twilio Setup

1. Create a [Twilio account](https://www.twilio.com/try-twilio).
2. In the [Twilio Console](https://console.twilio.com/), note your **Account SID** and **Auth Token**.
3. **For SMS**: Purchase a phone number under **Phone Numbers** → **Manage** → **Buy a Number**.
4. **For WhatsApp**: 
   - Go to **Messaging** → **Try it out** → **Send a WhatsApp message**.
   - Follow the onboarding to connect your Twilio number to the WhatsApp Sandbox.
   - The sender number format is `whatsapp:+14155238886` (sandbox) or your approved WhatsApp number.

## Configuration

Run the configuration wizard:

```bash
dotnet run -- twilio configure
```

You will be prompted for:

| Field | Description |
|-------|-------------|
| AccountSid | Your Twilio Account SID (starts with `AC`) |
| AuthToken | Your Twilio Auth Token |
| SmsFrom | (Optional) Your Twilio SMS phone number |
| SmsTo | (Optional) Default SMS recipient number |
| MessagingServiceSid | (Optional) Twilio Messaging Service SID |
| SmsWebhookUrl | (Optional) URL for SMS webhook callbacks |
| SmsStatusCallback | (Optional) URL for SMS delivery status |
| WhatsAppFrom | (Optional) WhatsApp sender number |
| WhatsAppTo | (Optional) Default WhatsApp recipient |
| WhatsAppTemplateId | (Optional) WhatsApp template ID |
| WhatsAppWebhookUrl | (Optional) URL for WhatsApp webhook callbacks |
| WhatsAppStatusCallback | (Optional) URL for WhatsApp delivery status |

Alternatively, set environment variables:

```bash
export TWILIO_ACCOUNT_SID="AC1234567890123456789012345678901234"
export TWILIO_AUTH_TOKEN="your-auth-token"
export TWILIO_SMS_FROM="+15551234567"
export TWILIO_SMS_TO="+15557654321"
export TWILIO_WHATSAPP_FROM="whatsapp:+14155238886"
export TWILIO_WHATSAPP_TO="whatsapp:+15557654321"
```

## Building & Running

The `run.sh` script builds the required libraries (only if missing) and runs the sample in one step:

```bash
./run.sh -- twilio <command>
```

| Flag | Description |
|------|-------------|
| `-b`, `--build-libs` | Force rebuild library dependencies even if already present |
| `-v`, `--verbose` | Enable console logging output (hidden by default) |

Examples:

```bash
./run.sh -- twilio sms send            # quiet, build deps only if needed
./run.sh -v -- twilio status           # show logs, build deps only if needed
./run.sh -b -- twilio configure        # force rebuild deps, quiet run
./run.sh -b -v -- twilio whatsapp send # force rebuild deps + show logs
```

To build without running:

```bash
./build-libs.sh
dotnet build
```

### Root Commands

| Command | Description |
|---------|-------------|
| `schema [name]` | Show all Twilio schemas or a single schema |
| `configure` | Prompt for credentials and save to local config |

### SMS Commands

```bash
./run.sh -- twilio sms <command>
```

| Command | Description |
|---------|-------------|
| `validate` | Validate a sample SMS message |
| `status` | Show SMS connector runtime status |
| `receive [-f file] [-m mode]` | Parse a sample Twilio SMS webhook payload |
| `receive-status [-f file] [-m mode]` | Parse a sample SMS status callback |
| `send` | Send a live SMS message interactively |

### WhatsApp Commands

```bash
./run.sh -- twilio whatsapp <command>
```

| Command | Description |
|---------|-------------|
| `validate -k <kind>` | Validate a sample WhatsApp message (`text` or `template`) |
| `status` | Show WhatsApp connector runtime status |
| `receive [-f file] [-m mode]` | Parse a sample WhatsApp webhook payload |
| `receive-status [-f file] [-m mode]` | Parse a sample WhatsApp status callback |
| `send` | Build and send a live WhatsApp message interactively |

### Send Example

```bash
./run.sh -- twilio sms send
```

You will be prompted to select the sender mode (From number or Messaging service), enter recipient details, and compose the message body. The message body supports multi-line input — type `!done` on a new line to finish.
