# NETBash Acceptance Checklist

## Installation

- [ ] System is Ubuntu 24.04 amd64
- [ ] Bash 5+ available
- [ ] Microsoft package repository configured
- [ ] ASP.NET Core Runtime 10.0 installed
- [ ] .NET SDK 10.0 installed (if `--sdk` flag used)
- [ ] Nginx installed with `nginx-extras`
- [ ] Nginx site config generated for the application
- [ ] systemd service unit created and enabled
- [ ] UFW firewall active: SSH + Nginx Full allowed
- [ ] Let's Encrypt SSL certificate obtained (if `--domain` and `--email` provided)
- [ ] Certbot auto-renewal timer configured
- [ ] Logrotate configured for application and nginx logs
- [ ] All modules track idempotent state (re-running is a no-op)

## Security

- [ ] systemd hardening: `NoNewPrivileges`, `ProtectSystem=strict`, `ProtectHome`, `PrivateTmp`, `PrivateDevices`, `ProtectKernelModules/Tunables/ControlGroups`, `CapabilityBoundingSet=`, `MemoryDenyWriteExecute`
- [ ] Application runs as `www-data:www-data`
- [ ] UFW blocks all incoming except SSH (22) and HTTP/HTTPS (80, 443)
- [ ] Nginx TLS 1.2 + 1.3 only, strong ciphers (`HIGH:!aNULL:!MD5`)
- [ ] Security HTTP headers: HSTS, X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy
- [ ] HTTP redirects to HTTPS (301)
- [ ] Certbot auto-renews certificates daily

## Deployment

- [ ] Atomic deploy via `releases/<app>/<version>` + `current/<app>` symlink
- [ ] Health check on `/health` with 12 retries at 5s intervals
- [ ] Auto-rollback on health check failure
- [ ] Manual rollback to previous version supported
- [ ] Manual rollback to specific version supported
- [ ] `--force` flag required to re-deploy an already-deployed app
- [ ] Backups created on deploy (releases, nginx config, systemd units)

## Maintenance

- [ ] `netbash status` shows installed modules and deployed releases
- [ ] `netbash --dry-run` shows what would be done without executing
- [ ] All scripts pass `bash -n` syntax check
- [ ] All scripts pass ShellCheck with zero warnings
- [ ] Logrotate rotates logs daily, keeps 14 rotations, compresses old logs
- [ ] Logging uses `[timestamp] [LEVEL] message` format with DEBUG/INFO/SUCCESS/WARN/ERROR levels
- [ ] Release process uses SemVer (`X.Y.Z`), CHANGELOG.md, and git tags

## Verification Commands

```bash
# Syntax
bash -n lib/*.sh scripts/*.sh netbash
shellcheck lib/*.sh scripts/*.sh netbash

# Test suite
sudo env NETBASH_DIR=/opt/netbash bash scripts/test.sh

# Idempotency
sudo netbash install --app test --force --dry-run

# Status
netbash status
```
