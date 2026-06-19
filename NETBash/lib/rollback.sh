#!/usr/bin/env bash
# NETBash - Manual rollback: revert to previous or specific version
set -Eeuo pipefail

# Roll back to a specific named version under releases/<app>/
rollback_a_version() {
    local app="$1"
    local target_version="$2"

    local release_dir="${NETBASH_ROOT}/releases/${app}/${target_version}"
    if [[ ! -d "$release_dir" ]]; then
        die "Version '${target_version}' not found in releases/${app}/."
    fi

    local current_link="${NETBASH_ROOT}/current/${app}"

    __log INFO "Rolling back '${app}' to version ${target_version}..."
    require_dry_run systemd_detener_servicio "$app"
    require_dry_run ln -sfn "$release_dir" "$current_link"
    require_dry_run systemd_iniciar_servicio "$app"
    __log SUCCESS "Rolled back '${app}' to version ${target_version}."
}

# Roll back to the second-most-recent version
rollback_al_anterior() {
    local app="$1"
    local releases_dir="${NETBASH_ROOT}/releases/${app}"
    local current_link="${NETBASH_ROOT}/current/${app}"

    if [[ ! -d "$releases_dir" ]]; then
        die "No releases found for '${app}'."
    fi

    local current_target
    current_target="$(readlink "$current_link" 2>/dev/null || true)"

    local versions=()
    for rev in "$releases_dir"/*/; do
        versions+=("$(basename "$rev")")
    done

    if [[ ${#versions[@]} -lt 2 ]]; then
        die "Not enough releases to rollback. Need at least 2, found ${#versions[@]}."
    fi

    mapfile -t versions < <(printf '%s\n' "${versions[@]}" | sort -r)
    local previous="${versions[1]}"
    rollback_a_version "$app" "$previous"
}

# List all releases for an app, marking the current one with *
rollback_listar_versiones() {
    local app="$1"
    local releases_dir="${NETBASH_ROOT}/releases/${app}"

    if [[ ! -d "$releases_dir" ]]; then
        __log INFO "No releases for '${app}'."
        return 0
    fi

    local current_link="${NETBASH_ROOT}/current/${app}"
    local current_target
    current_target="$(readlink "$current_link" 2>/dev/null || true)"

    __log INFO "Releases for '${app}':"
    for rev in "$releases_dir"/*/; do
        local rev_name
        rev_name="$(basename "$rev")"
        local marker=" "
        if [[ "$rev" == "$current_target" ]]; then
            marker="*"
        fi
        __log INFO "  ${marker} ${rev_name}"
    done
}
