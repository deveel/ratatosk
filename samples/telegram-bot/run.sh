#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VERBOSE=false

while [ $# -gt 0 ]; do
  case "$1" in
    --verbose|-v)
      VERBOSE=true
      shift
      ;;
    --)
      shift
      break
      ;;
    *)
      break
      ;;
  esac
done

if [ "$VERBOSE" = true ]; then
  export DEVEEL_VERBOSE=true
fi

dotnet run --project "$SCRIPT_DIR/src/Telegram/Telegram.csproj" -- "$@"


