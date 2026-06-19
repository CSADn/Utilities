#!/usr/bin/env bash
# NETBash - Nginx installation, site config generation, and reload
set -Eeuo pipefail

# Install Nginx with extras (Brotli, geoip, etc.)
nginx_instalar() {
    if verificar_estado "nginx"; then
        __log INFO "Nginx already installed."
        return 0
    fi

    __log INFO "Installing Nginx..."
    require_dry_run apt-get install -y -qq nginx nginx-extras

    require_dry_run systemctl enable nginx
    require_dry_run systemctl start nginx

    guardar_estado "nginx"
    __log SUCCESS "Nginx installed and running."
}

# Generate Nginx site config from template and enable it
nginx_configurar_app() {
    local app="$1"
    local domain="$2"
    local port="$3"
    local conf_file="/etc/nginx/sites-available/${app}"
    local conf_link="/etc/nginx/sites-enabled/${app}"

    if [[ -f "$conf_file" ]] && [[ -L "$conf_link" ]]; then
        __log INFO "Nginx config for '${app}' already present."
        return 0
    fi

    if [[ ! -f "${NETBASH_DIR}/templates/netbash-nginx.conf" ]]; then
        die "Template netbash-nginx.conf not found."
    fi

    __log INFO "Generating Nginx config for '${app}'..."

    sed \
        -e "s/__APP__/${app}/g" \
        -e "s/__DOMAIN__/${domain}/g" \
        -e "s/__PORT__/${port}/g" \
        "${NETBASH_DIR}/templates/netbash-nginx.conf" > "$conf_file"

    if [[ ! -L "$conf_link" ]]; then
        ln -sf "$conf_file" "$conf_link"
    fi

    nginx_recargar
    __log SUCCESS "Nginx config for '${app}' deployed."
}

# Verify Nginx installation
nginx_verificar() {
    if command -v nginx &>/dev/null; then
        local version
        version="$(nginx -v 2>&1 || true)"
        __log SUCCESS "Nginx: ${version}"
    else
        __log WARN "Nginx not found."
    fi
}

# Test config and reload Nginx
nginx_recargar() {
    __log INFO "Testing Nginx configuration..."
    nginx -t || die "Nginx configuration test failed."
    __log INFO "Reloading Nginx..."
    systemctl reload nginx || die "Failed to reload Nginx."
}
