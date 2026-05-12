#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
CONFIGURATION="${CONFIGURATION:-Release}"
FRAMEWORK="${FRAMEWORK:-net8.0}"
LIBS_DIR="$SCRIPT_DIR/libs"

dotnet build "$REPO_ROOT/src/Deveel.Messaging.Connectors/Deveel.Messaging.Connectors.csproj" -c "$CONFIGURATION" -f "$FRAMEWORK" --nologo
dotnet build "$REPO_ROOT/src/Deveel.Messaging.Connector.Facebook/Deveel.Messaging.Connector.Facebook.csproj" -c "$CONFIGURATION" -f "$FRAMEWORK" --nologo
dotnet build "$REPO_ROOT/src/Deveel.Messaging.Connector.Firebase/Deveel.Messaging.Connector.Firebase.csproj" -c "$CONFIGURATION" -f "$FRAMEWORK" --nologo
dotnet build "$REPO_ROOT/src/Deveel.Messaging.Connector.Sendgrid/Deveel.Messaging.Connector.Sendgrid.csproj" -c "$CONFIGURATION" -f "$FRAMEWORK" --nologo
dotnet build "$REPO_ROOT/src/Deveel.Messaging.Connector.Telegram/Deveel.Messaging.Connector.Telegram.csproj" -c "$CONFIGURATION" -f "$FRAMEWORK" --nologo
dotnet build "$REPO_ROOT/src/Deveel.Messaging.Connector.Twilio/Deveel.Messaging.Connector.Twilio.csproj" -c "$CONFIGURATION" -f "$FRAMEWORK" --nologo

mkdir -p "$LIBS_DIR"

copy_artifacts() {
  local source_dir="$1"
  local assembly_name="$2"
  for extension in dll pdb xml; do
    if [[ -f "$source_dir/$assembly_name.$extension" ]]; then
      cp "$source_dir/$assembly_name.$extension" "$LIBS_DIR/"
    fi
  done
}

copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connectors/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Abstractions"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connectors/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connector.Abstractions"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connectors/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connectors"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connector.Facebook/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connector.Facebook"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connector.Firebase/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connector.Firebase"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connector.Sendgrid/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connector.Sendgrid"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connector.Telegram/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connector.Telegram"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connector.Twilio/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connector.Twilio"

echo "Multi-connector libraries copied to $LIBS_DIR"
