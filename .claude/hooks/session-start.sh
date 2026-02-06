#!/bin/bash
set -euo pipefail

# Only run in remote (Claude Code on the web) environments
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

# Install .NET SDK 10.0 if not already installed
if ! dotnet --version 2>/dev/null | grep -q '^10\.'; then
  sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0
fi

# Restore NuGet packages
cd "$CLAUDE_PROJECT_DIR"
dotnet restore
