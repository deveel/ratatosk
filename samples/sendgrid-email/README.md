# SendGrid Email Sample

This sample demonstrates how to use the Ratatosk SendGrid connector to send emails via the SendGrid API.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- A [SendGrid](https://sendgrid.com/) account
- A SendGrid API Key

## SendGrid Setup

1. Create a [SendGrid account](https://signup.sendgrid.com/).
2. Navigate to **Settings** → **API Keys** in the SendGrid dashboard.
3. Click **Create API Key** and choose **Full Access** or **Restricted Access** with at least **Mail Send** permission.
4. Copy the generated API key.
5. Verify **Sender Authentication** — you must verify the sender email address (single sender or domain) before sending.

## Configuration

Run the configuration wizard:

```bash
dotnet run -- sendgrid configure
```

You will be prompted for:

| Field | Description |
|-------|-------------|
| ApiKey | Your SendGrid API key |
| FromEmail | The verified sender email address |
| ToEmail | Default recipient email for testing |
| SandboxMode | Enable sandbox mode (default: `true` for testing) |
| WebhookUrl | (Optional) URL for inbound email webhooks |
| FromName | (Optional) Default sender display name |
| ReplyTo | (Optional) Default reply-to address |
| TemplateId | (Optional) Default SendGrid template ID |

Alternatively, set environment variables:

```bash
export SENDGRID_API_KEY="SG.your-api-key"
export SENDGRID_FROM_EMAIL="sender@example.com"
export SENDGRID_TO_EMAIL="recipient@example.com"
export SENDGRID_SANDBOX_MODE="true"
```

## Building & Running

```bash
./run.sh -- sendgrid <command>
```

| Flag | Description |
|------|-------------|
| `-v`, `--verbose` | Enable console logging output (hidden by default) |

Examples:

```bash
./run.sh -- sendgrid send       # quiet
./run.sh -v -- sendgrid status  # show logs
```

To build without running:

```bash
dotnet build
```

### Commands

| Command | Description |
|---------|-------------|
| `schema [name]` | Show all SendGrid schemas or a single schema |
| `configure` | Prompt for credentials and save to local config |
| `validate -k <kind>` | Validate a sample message (`html` or `template`) |
| `status` | Show the connector runtime status |
| `receive [-f file] [-m mode]` | Parse a sample inbound webhook payload |
| `receive-status [-f file] [-m mode]` | Parse a sample delivery event callback |
| `send` | Build and send a live email interactively |

### Send Example

```bash
./run.sh -- sendgrid send
```

You will be prompted to select the message type (HTML or Template), enter sender/recipient, subject, and body. The HTML body supports multi-line input — type `!done` on a new line to finish.
