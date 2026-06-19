#!/usr/bin/env bash
# NETBash - Deploy orchestrator: stop service, copy release, swap symlink, start, verify
set -Eeuo pipefail

# Run the full deploy sequence: validate args, check force, call deploy_realizar
cmd_deploy() {
    require_root

    if [[ -z "$NETBASH_APP" ]]; then
        die "--app is required for deploy."
    fi

    if [[ -L "${NETBASH_ROOT}/current/${NETBASH_APP}" ]]; then
        if ! require_force; then
            __log WARN "A release is already deployed. Use --force to redeploy."
            exit 0
        fi
    fi

    local source_dir="${NETBASH_SOURCE_DIR:-${NETBASH_ROOT}/current/${NETBASH_APP}}"
    local dll_relative="${NETBASH_DLL:-${NETBASH_APP}.dll}"

    source "${NETBASH_DIR}/lib/systemd.sh"
    source "${NETBASH_DIR}/lib/deploy.sh"
    source "${NETBASH_DIR}/lib/healthcheck.sh"

    __log INFO "=== Deploy '${NETBASH_APP}' ==="
    __log INFO "Source: ${source_dir}"
    __log INFO "DLL:    ${dll_relative}"

    deploy_realizar "$NETBASH_APP" "$source_dir" "$dll_relative" "$NETBASH_PORT"

    __log SUCCESS "=== Deploy complete ==="
}
