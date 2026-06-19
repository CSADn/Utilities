#!/usr/bin/env bash
# NETBash - UFW firewall: install, configure SSH + HTTP/HTTPS, enable
set -Eeuo pipefail

# Install UFW and allow OpenSSH and Nginx Full (80 + 443)
firewall_instalar() {
    if verificar_estado "firewall"; then
        __log INFO "UFW already configured."
        return 0
    fi

    if ! command -v ufw &>/dev/null; then
        __log INFO "Installing UFW..."
        require_dry_run apt-get install -y -qq ufw
    fi

    __log INFO "Configuring UFW firewall..."
    require_dry_run ufw --force reset
    require_dry_run ufw default deny incoming
    require_dry_run ufw default allow outgoing
    require_dry_run ufw allow OpenSSH
    require_dry_run ufw allow 'Nginx Full'
    require_dry_run ufw --force enable

    guardar_estado "firewall"
    __log SUCCESS "UFW firewall configured: SSH, HTTP, HTTPS allowed."
}

# Display current UFW status
firewall_verificar() {
    if command -v ufw &>/dev/null; then
        ufw status verbose | head -20
    else
        __log WARN "UFW not installed."
    fi
}
