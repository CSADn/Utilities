#!/usr/bin/env bash
# NETBash - Health checking: poll /health endpoint with retries and timeout
set -Eeuo pipefail

readonly HEALTHCHECK_RETRIES=12
readonly HEALTHCHECK_INTERVAL=5
readonly HEALTHCHECK_TIMEOUT=3

# Poll the application's /health endpoint until it responds or retries are exhausted
healthcheck_verificar() {
    local app="$1"
    local port="$2"
    local retries="${3:-${HEALTHCHECK_RETRIES}}"
    local interval="${4:-${HEALTHCHECK_INTERVAL}}"
    local health_url="http://127.0.0.1:${port}/health"

    __log INFO "Waiting for /health on ${health_url}..."
    __log INFO "Will retry up to ${retries} times every ${interval}s."

    local attempt=1
    while [[ $attempt -le "$retries" ]]; do
        if curl -sf --max-time "$HEALTHCHECK_TIMEOUT" "$health_url" &>/dev/null; then
            __log SUCCESS "Health check passed for '${app}' (attempt ${attempt})."
            return 0
        fi
        __log DEBUG "Health check attempt ${attempt}/${retries} failed. Waiting ${interval}s..."
        sleep "$interval"
        ((attempt++))
    done

    __log ERROR "Health check failed for '${app}' after ${retries} attempts."
    return 1
}

# Poll /health via the Nginx reverse proxy (external-facing)
healthcheck_verificar_nginx() {
    local domain="$1"
    local retries="${2:-6}"
    local interval="${3:-10}"
    local health_url="https://${domain}/health"

    __log INFO "Checking /health via Nginx at ${health_url}..."

    local attempt=1
    while [[ $attempt -le "$retries" ]]; do
        if curl -sfk --max-time "$HEALTHCHECK_TIMEOUT" "$health_url" &>/dev/null; then
            __log SUCCESS "Nginx health check passed for '${domain}'."
            return 0
        fi
        __log DEBUG "Nginx health check attempt ${attempt}/${retries} failed."
        sleep "$interval"
        ((attempt++))
    done

    __log WARN "Nginx health check for '${domain}' failed after ${retries} attempts."
    return 1
}
