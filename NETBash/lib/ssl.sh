#!/usr/bin/env bash
# NETBash - Let's Encrypt SSL: certbot install, certificate obtain, auto-renewal
set -Eeuo pipefail

# Install Certbot with the Nginx plugin
ssl_instalar_certbot() {
    if verificar_estado "certbot"; then
        __log INFO "Certbot already installed."
        return 0
    fi

    __log INFO "Installing Certbot..."
    require_dry_run apt-get install -y -qq certbot python3-certbot-nginx

    guardar_estado "certbot"
    __log SUCCESS "Certbot installed."
}

# Obtain an SSL certificate for the given domain using the Nginx plugin
ssl_obtener_certificado() {
    local domain="$1"
    local email="$2"

    if [[ -d "/etc/letsencrypt/live/${domain}" ]]; then
        __log INFO "SSL certificate for '${domain}' already exists."
        return 0
    fi

    if [[ -z "$email" ]]; then
        die "--email is required to obtain SSL certificate."
    fi

    __log INFO "Obtaining SSL certificate for '${domain}'..."
    require_dry_run certbot --nginx \
        --non-interactive \
        --agree-tos \
        --email "$email" \
        --domains "$domain" \
        --redirect

    __log SUCCESS "SSL certificate obtained for '${domain}'."
}

# Configure a daily systemd timer for automatic certificate renewal
ssl_configurar_renovacion() {
    if verificar_estado "certbot_renew"; then
        __log INFO "Certbot auto-renewal already configured."
        return 0
    fi

    __log INFO "Configuring certbot auto-renewal..."
    if command -v systemctl &>/dev/null; then
        local timer_file="/etc/systemd/system/certbot-renew.timer"
        if [[ ! -f "$timer_file" ]]; then
            cat > /tmp/certbot-renew.service << 'EOF'
[Unit]
Description=Certbot Renewal

[Service]
Type=oneshot
ExecStart=/usr/bin/certbot renew --quiet --post-hook "systemctl reload nginx"
EOF
            cat > /tmp/certbot-renew.timer << 'EOF'
[Unit]
Description=Daily Certbot Renewal

[Timer]
OnCalendar=daily
Persistent=true

[Install]
WantedBy=timers.target
EOF
            require_dry_run cp /tmp/certbot-renew.service /etc/systemd/system/
            require_dry_run cp /tmp/certbot-renew.timer /etc/systemd/system/
            require_dry_run systemctl daemon-reload
            require_dry_run systemctl enable certbot-renew.timer
            require_dry_run systemctl start certbot-renew.timer
            rm -f /tmp/certbot-renew.service /tmp/certbot-renew.timer
        fi
    fi

    guardar_estado "certbot_renew"
    __log SUCCESS "Certbot auto-renewal configured (daily)."
}
