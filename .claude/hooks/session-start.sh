#!/bin/bash
set -euo pipefail

# Only run in remote (Claude Code on the web) environments
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

# Fix /tmp permissions — the remote container may ship /tmp as 0755 owned by
# a non-root user, which prevents apt's _apt user (and other processes) from
# creating temporary files.  Standard /tmp is mode 1777 (world-writable +
# sticky bit).
if [ "$(stat -c '%a' /tmp)" != "1777" ]; then
  chmod 1777 /tmp
fi

DOTNET_ROOT="/root/.dotnet"
export DOTNET_ROOT
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

# Install .NET SDK 10.0 if not already installed, using Microsoft's official
# install script.  This avoids depending on apt repositories being configured
# or GPG-signed correctly.
if ! dotnet --version 2>/dev/null | grep -q '^10\.'; then
  wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh \
    || curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  /tmp/dotnet-install.sh --channel 10.0 --install-dir "$DOTNET_ROOT"
  rm -f /tmp/dotnet-install.sh
fi

# Restore NuGet packages (non-fatal — a transient network issue should not
# prevent the session from starting; the agent can retry restore later).
cd "$CLAUDE_PROJECT_DIR"
dotnet restore || echo "WARNING: dotnet restore failed; packages may need to be restored manually."
