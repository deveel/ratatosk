# Firebase Cloud Messaging Sample

This sample demonstrates how to use the Ratatosk Firebase Push connector to send push notifications via Firebase Cloud Messaging (FCM).

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- A [Google Firebase](https://firebase.google.com/) project with Cloud Messaging enabled
- A Firebase service account key (JSON)

## Firebase Setup

1. Go to the [Firebase Console](https://console.firebase.google.com/).
2. Create a new project or select an existing one.
3. Navigate to **Project Settings** → **Service accounts**.
4. Click **Generate new private key** and download the JSON file.
5. Note your **Project ID** from Project Settings → General.

> **Note**: Firebase Cloud Messaging may require you to enable the Cloud Messaging API and configure platform-specific settings (APNs for iOS, etc.). For testing, you can use the service account key directly.

## Configuration

Run the configuration wizard:

```bash
dotnet run -- firebase configure
```

You will be prompted for:

| Field | Description |
|-------|-------------|
| ProjectId | Your Firebase project ID |
| ServiceAccountKey | The full JSON content of the service account key |
| DeviceToken | (Optional) A default FCM device registration token |
| Topic | (Optional) A default FCM topic name |
| DryRun | Enable dry-run mode (default: `true` for testing) |

Alternatively, set environment variables:

```bash
export FIREBASE_PROJECT_ID="my-project-id"
export FIREBASE_SERVICE_ACCOUNT_KEY='{"type":"service_account",...}'
export FIREBASE_DRY_RUN="true"
```

## Building & Running

```bash
./run.sh -- firebase <command>
```

| Flag | Description |
|------|-------------|
| `-v`, `--verbose` | Enable console logging output (hidden by default) |

Examples:

```bash
./run.sh -- firebase send       # quiet
./run.sh -v -- firebase status  # show logs
```

To build without running:

```bash
dotnet build
```

### Commands

| Command | Description |
|---------|-------------|
| `schema [name]` | Show all Firebase schemas or a single schema |
| `configure` | Prompt for credentials and save to local config |
| `validate -k <kind>` | Validate a sample message (`device` or `topic`) |
| `send` | Build and send a live push notification |
| `batch` | Send a batch of push notifications |
| `status` | Show the connector runtime status |

### Send Example

```bash
./run.sh -- firebase send
```

You will be prompted to select the target type (Device or Topic) and compose the notification message. The message body supports multi-line input — type `!done` on a new line to finish.
