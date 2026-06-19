#!/usr/bin/env bash
# NETBash - Logrotate configuration: app logs, nginx logs
set -Eeuo pipefail

# Create a logrotate config for the app's logs and nginx logs
logrotate_configurar() {
    local app="$1"
    local conf_file="/etc/logrotate.d/netbash-${app}"

    if [[ -f "$conf_file" ]]; then
        __log INFO "Logrotate for '${app}' already configured."
        return 0
    fi

    __log INFO "Configuring logrotate for '${app}'..."

    cat > "$conf_file" << LOGROTATE
/opt/netbash/releases/${app}/*/logs/*.log {
    daily
    rotate 14
    compress
    delaycompress
    missingok
    notifempty
    copytruncate
    dateext
}

/var/log/netbash-${app}/*.log {
    daily
    rotate 14
    compress
    delaycompress
    missingok
    notifempty
    copytruncate
    dateext
}

/var/log/nginx/*.log {
    daily
    rotate 14
    compress
    delaycompress
    missingok
    notifempty
    sharedscripts
    postrotate
        systemctl reload nginx > /dev/null 2>&1 || true
    endscript
}
LOGROTATE

    __log SUCCESS "Logrotate configured for '${app}'."
}
