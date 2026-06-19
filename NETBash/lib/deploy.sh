#!/usr/bin/env bash
# NETBash - Atomic deployment: copy release, symlink swap, health check, auto-rollback
set -Eeuo pipefail

# Deploy a release atomically: copy source to timestamped directory, symlink swap, verify
deploy_realizar() {
    local app="$1"
    local source_dir="$2"
    local dll_relative="$3"
    local port="$4"

    if [[ ! -d "$source_dir" ]]; then
        die "Source directory not found: ${source_dir}"
    fi

    local version
    version="$(date -u '+%Y%m%d%H%M%S')"
    local release_dir="${NETBASH_ROOT}/releases/${app}/${version}"
    local current_link="${NETBASH_ROOT}/current/${app}"
    local dll_path="${release_dir}/${dll_relative}"

    __log INFO "Deploying '${app}' version ${version}..."

    require_dry_run mkdir -p "$release_dir"
    require_dry_run cp -a "${source_dir}/." "$release_dir"

    if [[ ! -f "$dll_path" ]]; then
        die "DLL not found at expected path: ${dll_path}"
    fi

    require_dry_run mkdir -p "${NETBASH_ROOT}/current"
    require_dry_run ln -sfn "$release_dir" "${current_link}.new"
    require_dry_run systemd_detener_servicio "$app"
    require_dry_run mv -T "${current_link}.new" "$current_link"
    require_dry_run systemd_iniciar_servicio "$app"

    if healthcheck_verificar "$app" "$port"; then
        __log SUCCESS "Deploy '${app}' version ${version} completed successfully."
    else
        __log ERROR "Health check failed after deploy. Initiating rollback..."
        deploy_rollback "$app"
        return 1
    fi
}

# Rollback to the previous release by relinking the current symlink
deploy_rollback() {
    local app="$1"
    local current_link="${NETBASH_ROOT}/current/${app}"
    local releases_dir="${NETBASH_ROOT}/releases/${app}"

    if [[ ! -d "$releases_dir" ]]; then
        __log WARN "No releases found for '${app}'. Cannot rollback."
        return 1
    fi

    local current_target
    current_target="$(readlink "$current_link" 2>/dev/null || true)"
    local previous=""
    for rev in "$releases_dir"/*/; do
        local rev_name
        rev_name="$(basename "$rev")"
        if [[ "$rev_name" != "$(basename "$current_target")" ]]; then
            previous="$rev"
        fi
    done

    if [[ -z "$previous" ]]; then
        __log ERROR "No previous release found for rollback."
        return 1
    fi

    __log INFO "Rolling back '${app}' to $(basename "$previous")..."
    require_dry_run systemd_detener_servicio "$app"
    require_dry_run ln -sfn "$previous" "$current_link"
    require_dry_run systemd_iniciar_servicio "$app"
    __log SUCCESS "Rollback to $(basename "$previous") completed."
}
