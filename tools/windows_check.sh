#!/bin/bash
# Überträgt die Windows-Builds an den Test-PC (SSH) und startet dort Selbsttest, MSIX-Bau und WACK.
# WACK braucht Administratorrechte; der SSH-Nutzer muss Administrator sein.
set -euo pipefail
cd "$(dirname "$0")/.."
WIN_USER=${PATHROVA_WINDOWS_USER:-"michel nguyen"}
WIN_HOST=${PATHROVA_WINDOWS_HOST:-192.168.0.182}
SSH=(ssh -o "User=\"$WIN_USER\"" "$WIN_HOST")
SCP=(scp -o "User=\"$WIN_USER\"")
TMP=$(mktemp -d); trap 'rm -rf "$TMP"' EXIT
tar -czf "$TMP/pathrova.tgz" artifacts/publish/win-x64 artifacts/publish/win-arm64 packaging tools/windows
"${SSH[@]}" 'if (Test-Path "$env:USERPROFILE\pathrova-check") { Remove-Item "$env:USERPROFILE\pathrova-check" -Recurse -Force }; New-Item "$env:USERPROFILE\pathrova-check" -ItemType Directory | Out-Null'
"${SCP[@]}" "$TMP/pathrova.tgz" "$WIN_HOST:pathrova-check/"
"${SSH[@]}" 'cd "$env:USERPROFILE\pathrova-check"; tar -xzf pathrova.tgz; powershell -NoProfile -ExecutionPolicy Bypass -File tools\windows\check.ps1' || status=$?
mkdir -p artifacts/windows-check artifacts/msix
"${SCP[@]}" -r "$WIN_HOST:pathrova-check/artifacts/windows-check/*" artifacts/windows-check/ || true
"${SCP[@]}" "$WIN_HOST:pathrova-check/artifacts/msix/*" artifacts/msix/ || true
exit ${status:-0}
