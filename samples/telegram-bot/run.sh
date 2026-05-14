#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_LIBS=false
VERBOSE=false

while [ $# -gt 0 ]; do
  case "$1" in
    --build-libs|-b)
      BUILD_LIBS=true
      shift
      ;;
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

LIBS_DIR="$SCRIPT_DIR/libs"

if [ "$BUILD_LIBS" = true ]; then
  echo "Building library dependencies (--build-libs)..."
  "$SCRIPT_DIR/build-libs.sh"
elif [ ! -d "$LIBS_DIR" ]; then
  echo "Library dependencies directory not found. Building..."
  "$SCRIPT_DIR/build-libs.sh"
else
  dll_count=$(ls "$LIBS_DIR"/*.dll 2>/dev/null | wc -l)
  if [ "$dll_count" -eq 0 ]; then
    echo "No library dependencies found. Building..."
    "$SCRIPT_DIR/build-libs.sh"
  fi
fi

if [ "$VERBOSE" = true ]; then
  export DEVEEL_VERBOSE=true
fi

dotnet run --project "$SCRIPT_DIR/src/Telegram/Telegram.csproj" -- "$@"
