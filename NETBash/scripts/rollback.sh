#!/usr/bin/env bash
# NETBash - Rollback orchestrator: revert to previous or specific version
set -Eeuo pipefail

# Run rollback: use NETBASH_VERSION for specific version, otherwise rollback to previous
cmd_rollback() {
    require_root

    if [[ -z "$NETBASH_APP" ]]; then
        die "--app is required for rollback."
    fi

    source "${NETBASH_DIR}/lib/rollback.sh"
    source "${NETBASH_DIR}/lib/systemd.sh"

    local target_version="${NETBASH_VERSION:-}"

    __log INFO "=== Rollback '${NETBASH_APP}' ==="

    if [[ -n "$target_version" ]]; then
        rollback_a_version "$NETBASH_APP" "$target_version"
    else
        rollback_al_anterior "$NETBASH_APP"
    fi

    __log SUCCESS "=== Rollback complete ==="
}
