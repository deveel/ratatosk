# Facebook Messenger Sample

This sample demonstrates how to use the Deveel Messaging Facebook Messenger connector to send and receive messages via the Facebook Messenger Platform.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- A [Facebook](https://www.facebook.com/) account
- A **Facebook Page** (required to use the Messenger API)
- A **Facebook App** with Messenger product enabled

## Facebook App Setup

1. Go to the [Facebook Developers Portal](https://developers.facebook.com/).
2. Click **My Apps** → **Create App** and select **Business** as the app type.
3. Navigate to **Dashboard** → **Add Product** and select **Messenger**.
4. Under **Messenger** → **Settings** → **Access Tokens**, click **Generate Token** next to your Page.
5. Copy the **Page Access Token**.
6. Note your **Page ID** (found in the Page's **About** section).

For receiving messages via webhooks:

1. Under **Messenger** → **Settings** → **Webhooks**, configure a callback URL.
2. Generate a **Verify Token** (any string you choose).
3. Subscribe to the `messages` and `messaging_postbacks` webhook events.

> **Note**: Your Facebook App must be in **Development** mode for testing with whitelisted test users. For production, the app requires review by Facebook.

## Configuration

Run the configuration wizard:

```bash
dotnet run -- facebook configure
```

You will be prompted for:

| Field | Description |
|-------|-------------|
| PageAccessToken | The Facebook Page access token |
| PageId | The numeric ID of your Facebook Page |
| RecipientPsid | The PSID of the test user to receive messages |
| WebhookUrl | (Optional) Public HTTPS URL for webhook callbacks |
| VerifyToken | (Optional) Webhook verification token |
| MediaUrl | (Optional) Default URL for media attachments |

Alternatively, set environment variables:

```bash
export FACEBOOK_PAGE_ACCESS_TOKEN="EAAx...your-token"
export FACEBOOK_PAGE_ID="123456789"
export FACEBOOK_RECIPIENT_PSID="987654321"
export FACEBOOK_WEBHOOK_URL="https://example.com/webhook"
```

## Getting a Test Recipient PSID

1. Open your Facebook Page and click **Send Message**.
2. Send a message to your Page from your personal account.
3. The PSID appears in the webhook callback, or you can view it at your webhook endpoint.
4. Alternatively, use the [Page Conversations API](https://developers.facebook.com/docs/messenger-platform/reference/conversations).

## Building

Before building, run the library build script from the sample root:

```bash
./build-libs.sh
```

Then build the sample:

```bash
dotnet build
```

## Running

```bash
dotnet run -- facebook <command>
```

### Commands

| Command | Description |
|---------|-------------|
| `schema [name]` | Show all Facebook schemas or a single schema |
| `configure` | Prompt for credentials and save to local config |
| `validate -k <kind>` | Validate a sample message (`text` or `media`) |
| `status` | Show the connector runtime status |
| `receive [-f file]` | Parse a sample Facebook webhook payload |
| `send` | Build and send a live Facebook message interactively |

### Send Example

```bash
dotnet run -- facebook send
```

You will be prompted to select the message type (Text or Media), enter recipient details, and compose the message content. The text body supports multi-line input — type `!done` on a new line to finish.
