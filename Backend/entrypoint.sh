#!/bin/sh
# Listens on Render's $PORT when present, 8080 otherwise (local compose).
set -eu
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"
exec dotnet Backend.dll
