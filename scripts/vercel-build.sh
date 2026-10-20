#!/usr/bin/env bash
# Vercel's build image has no .NET SDK by default; install it on the fly
# (Vercel's build containers are ephemeral, so this runs on every build)
# then publish the Blazor WASM client as the static site Vercel serves.
set -euo pipefail

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"

if ! command -v dotnet >/dev/null 2>&1; then
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 8.0 --install-dir "$DOTNET_ROOT"
fi

dotnet publish TodoApp.Client/TodoApp.Client.csproj -c Release -o publish-output
