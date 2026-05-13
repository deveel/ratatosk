#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
CONFIGURATION="${CONFIGURATION:-Release}"
FRAMEWORK="${FRAMEWORK:-net8.0}"
LIBS_DIR="$SCRIPT_DIR/libs"

dotnet build "$REPO_ROOT/src/Deveel.Messaging.Connector.Firebase/Deveel.Messaging.Connector.Firebase.csproj" -c "$CONFIGURATION" -f "$FRAMEWORK" --nologo

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

copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Abstractions/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Abstractions"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connector.Abstractions/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connector.Abstractions"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connectors/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connectors"
copy_artifacts "$REPO_ROOT/src/Deveel.Messaging.Connector.Firebase/bin/$CONFIGURATION/$FRAMEWORK" "Deveel.Messaging.Connector.Firebase"

echo "Connector libraries copied to $LIBS_DIR"
