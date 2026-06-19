# Changelog

## [1.0.0] - 2026-06-18

### Added
- Initial release of NETBash
- CLI entry point with install/deploy/rollback/status commands
- .NET 10 Runtime installation (SDK optional via --sdk)
- Nginx reverse proxy with HTTP/2, Brotli, Gzip, WebSockets
- systemd service with full hardening (ProtectSystem, NoNewPrivileges, PrivateTmp)
- UFW firewall configuration (SSH, HTTP, HTTPS)
- Let's Encrypt SSL with automatic daily renewal
- Versioned atomic deployment with symlink-based rollback
- Health check endpoint validation with automatic rollback on failure
- Log rotation for application and nginx logs
- Backup of releases, nginx configs, and systemd services
- Multi-application support
- Idempotent operations with state tracking
- Dry-run mode (--dry-run)
- Force mode (--force) for reinstalls/redeploys
- Comprehensive test suite (syntax, idempotency, logging, CLI, deploy, rollback)
