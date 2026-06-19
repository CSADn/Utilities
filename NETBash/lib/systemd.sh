#!/usr/bin/env bash
# NETBash - systemd service unit creation, start, stop, restart, status
set -Eeuo pipefail

# Create a systemd service unit from template and enable it
systemd_instalar_servicio() {
    local app="$1"
    local dll_path="$2"
    local service_file="/etc/systemd/system/netbash-${app}.service"

    if [[ -f "$service_file" ]]; then
        __log INFO "systemd service for '${app}' already exists."
        return 0
    fi

    if [[ ! -f "${NETBASH_DIR}/templates/netbash.service" ]]; then
        die "Template netbash.service not found."
    fi

    local current_dir
    current_dir="${NETBASH_ROOT}/current/${app}"

    __log INFO "Creating systemd service for '${app}'..."

    sed \
        -e "s/__APP__/${app}/g" \
        -e "s|__CURRENT_DIR__|${current_dir}|g" \
        -e "s|__DLL_PATH__|${dll_path}|g" \
        "${NETBASH_DIR}/templates/netbash.service" > "$service_file"

    require_dry_run systemctl daemon-reload
    require_dry_run systemctl enable "netbash-${app}"

    __log SUCCESS "systemd service for '${app}' created."
}

# Start the service and wait briefly for it to be active
systemd_iniciar_servicio() {
    local app="$1"
    __log INFO "Starting service 'netbash-${app}'..."
    require_dry_run systemctl start "netbash-${app}" || {
        __log ERROR "Failed to start netbash-${app}."
        return 1
    }
    __log SUCCESS "Service 'netbash-${app}' started."
}

# Stop the service if it is currently running
systemd_detener_servicio() {
    local app="$1"
    if systemctl is-active --quiet "netbash-${app}" 2>/dev/null; then
        __log INFO "Stopping service 'netbash-${app}'..."
        require_dry_run systemctl stop "netbash-${app}"
        __log SUCCESS "Service 'netbash-${app}' stopped."
    fi
}

# Restart the service
systemd_recargar_servicio() {
    local app="$1"
    __log INFO "Restarting service 'netbash-${app}'..."
    require_dry_run systemctl restart "netbash-${app}"
    __log SUCCESS "Service 'netbash-${app}' restarted."
}

# Check whether the service is active and report
systemd_estado_servicio() {
    local app="$1"
    if systemctl is-active --quiet "netbash-${app}" 2>/dev/null; then
        __log SUCCESS "Service 'netbash-${app}' is active."
        return 0
    else
        __log WARN "Service 'netbash-${app}' is not active."
        return 1
    fi
}
