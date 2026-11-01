#!/bin/bash
# Baut auf dem Mac die eigenständigen Windows-Programmordner für x64 und ARM64 (ohne installiertes .NET).
set -euo pipefail
cd "$(dirname "$0")/.."
DOTNET=${DOTNET:-$HOME/.dotnet/dotnet}
export DOTNET_CLI_TELEMETRY_OPTOUT=1
python3 tools/make_icons.py >/dev/null
for rid in win-x64 win-arm64; do
  rm -rf "artifacts/publish/$rid"
  "$DOTNET" publish src/Pathrova.App -c Release -r "$rid" --self-contained true -o "artifacts/publish/$rid" \
    -p:DebugType=none -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
  rm -f "artifacts/publish/$rid"/*.pdb
done
echo "Fertig: artifacts/publish/win-x64 und artifacts/publish/win-arm64"
