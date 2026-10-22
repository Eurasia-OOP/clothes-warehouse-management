#!/bin/sh
# Runs before nginx starts. Writes the API base URL the browser uses, so one image works in any environment.
set -eu

if [ -n "${API_BASE_URL:-}" ]; then
    webroot=/usr/share/nginx/html

    # Drop the precompressed copies of the baked file, or gzip_static would serve the old value.
    rm -f "$webroot/appsettings.json.gz" "$webroot/appsettings.json.br"

    printf '{\n  "Api": {\n    "BaseUrl": "%s"\n  }\n}\n' "$API_BASE_URL" > "$webroot/appsettings.json"
fi
