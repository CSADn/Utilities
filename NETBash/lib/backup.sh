#!/usr/bin/env bash
# NETBash - Backup: tar.gz archive of releases, nginx configs, systemd units
set -Eeuo pipefail

# Create a timestamped backup tarball for the given app
backup_realizar() {
    local app="$1"
    local backup_dir="${NETBASH_ROOT}/backups/${app}"
    local timestamp
    timestamp="$(date -u '+%Y%m%d%H%M%S')"
    local backup_file="${backup_dir}/backup-${timestamp}.tar.gz"

    __log INFO "Creating backup for '${app}'..."

    require_dry_run mkdir -p "$backup_dir"

    local sources=()
    if [[ -d "${NETBASH_ROOT}/releases/${app}" ]]; then
        sources+=("${NETBASH_ROOT}/releases/${app}")
    fi
    if [[ -L "${NETBASH_ROOT}/current/${app}" ]]; then
        sources+=("$(readlink -f "${NETBASH_ROOT}/current/${app}")")
    fi
    if [[ -f "/etc/nginx/sites-available/${app}" ]]; then
        sources+=("/etc/nginx/sites-available/${app}")
    fi
    if [[ -f "/etc/systemd/system/netbash-${app}.service" ]]; then
        sources+=("/etc/systemd/system/netbash-${app}.service")
    fi

    if [[ ${#sources[@]} -eq 0 ]]; then
        __log WARN "Nothing to backup for '${app}'."
        return 0
    fi

    require_dry_run tar -czf "$backup_file" "${sources[@]}"

    __log SUCCESS "Backup created: ${backup_file}"
}

# List all backups for an app with file sizes
backup_listar() {
    local app="$1"
    local backup_dir="${NETBASH_ROOT}/backups/${app}"

    if [[ ! -d "$backup_dir" ]]; then
        __log INFO "No backups for '${app}'."
        return 0
    fi

    __log INFO "Backups for '${app}':"
    for bk in "$backup_dir"/backup-*.tar.gz; do
        [[ -f "$bk" ]] && __log INFO "  - $(basename "$bk") ($(du -h "$bk" | cut -f1))"
    done
}
