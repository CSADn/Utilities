#!/usr/bin/env bash
# NETBash - Install orchestrator: dotnet, nginx, ssl, firewall, logrotate
set -Eeuo pipefail

# Run the full install sequence: prerequisites, runtime, nginx, ssl, firewall, logrotate
cmd_install() {
    require_root
    require_ubuntu

    if [[ -z "$NETBASH_APP" ]]; then
        die "--app is required for install."
    fi

    if verificar_estado "dotnet_runtime" && ! require_force; then
        __log WARN "Installation state detected. Use --force to reinstall."
        exit 0
    fi

    __log INFO "=== NETBash Install: ${NETBASH_APP} ==="
    __log INFO "Domain: ${NETBASH_DOMAIN:-none}"
    __log INFO "Port:   ${NETBASH_PORT}"
    __log INFO "SDK:    ${NETBASH_SDK}"

    source "${NETBASH_DIR}/lib/dotnet.sh"
    source "${NETBASH_DIR}/lib/nginx.sh"
    source "${NETBASH_DIR}/lib/systemd.sh"
    source "${NETBASH_DIR}/lib/firewall.sh"
    source "${NETBASH_DIR}/lib/ssl.sh"
    source "${NETBASH_DIR}/lib/logrotate.sh"

    mkdir -p "${NETBASH_ROOT}/releases/${NETBASH_APP}"
    mkdir -p "${NETBASH_ROOT}/current"
    mkdir -p "${NETBASH_ROOT}/backups/${NETBASH_APP}"
    mkdir -p "${NETBASH_ROOT}/env"

    dotnet_instalar_runtime
    if [[ "$NETBASH_SDK" == "true" ]]; then
        dotnet_instalar_sdk
    fi
    dotnet_verificar

    nginx_instalar
    if [[ -n "$NETBASH_DOMAIN" ]]; then
        nginx_configurar_app "$NETBASH_APP" "$NETBASH_DOMAIN" "$NETBASH_PORT"

        ssl_instalar_certbot
        if [[ -n "$NETBASH_EMAIL" ]]; then
            ssl_obtener_certificado "$NETBASH_DOMAIN" "$NETBASH_EMAIL"
        fi
        ssl_configurar_renovacion
    fi

    firewall_instalar
    logrotate_configurar "$NETBASH_APP"

    __log SUCCESS "=== Install complete for '${NETBASH_APP}' ==="
}
