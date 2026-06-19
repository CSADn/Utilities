#!/usr/bin/env bash
# NETBash - Common library: logging, validation, idempotency state, traps
set -Eeuo pipefail

if ! declare -p NETBASH_ROOT &>/dev/null 2>&1; then
    readonly NETBASH_ROOT="/opt/netbash"
fi
NETBASH_LIB="$(realpath "$(dirname "${BASH_SOURCE[0]}")")" && readonly NETBASH_LIB && export NETBASH_LIB
NETBASH_VERSION="${NETBASH_VERSION:-1.0.0}" && readonly NETBASH_VERSION && export NETBASH_VERSION

# Log a message with timestamp and level: DEBUG, INFO, SUCCESS, WARN, ERROR
__log() {
    local level="$1"
    shift
    local timestamp
    timestamp="$(date -u '+%Y-%m-%dT%H:%M:%SZ')"
    case "$level" in
        DEBUG) printf "[%s] [DEBUG] %s\n" "$timestamp" "$*" >&2 ;;
        INFO)  printf "[%s] [INFO]  %s\n" "$timestamp" "$*" ;;
        SUCCESS) printf "[%s] [SUCCESS] %s\n" "$timestamp" "$*" ;;
        WARN)  printf "[%s] [WARN]  %s\n" "$timestamp" "$*" >&2 ;;
        ERROR) printf "[%s] [ERROR] %s\n" "$timestamp" "$*" >&2 ;;
        *)     printf "[%s] [INFO]  %s\n" "$timestamp" "$*" ;;
    esac
}

# Log error and exit with code 1
die() {
    __log ERROR "$@"
    exit 1
}

# Require the script to be run as root
require_root() {
    if [[ "$EUID" -ne 0 ]]; then
        die "This command must be run as root."
    fi
}

# Require the OS to be Ubuntu 24.04
require_ubuntu() {
    if [[ ! -f /etc/os-release ]]; then
        die "Cannot detect OS. Expected Ubuntu 24.04."
    fi
    . /etc/os-release
    if [[ "$ID" != "ubuntu" ]]; then
        die "Unsupported OS: $ID. Expected Ubuntu."
    fi
    if [[ "$VERSION_ID" != "24.04" ]]; then
        die "Unsupported Ubuntu version: $VERSION_ID. Expected 24.04."
    fi
}

# Require Bash 5 or greater
require_bash5() {
    if (( BASH_VERSINFO[0] < 5 )); then
        die "Bash 5+ is required. Current version: $BASH_VERSION"
    fi
}

# Require --force flag for destructive re-operations
require_force() {
    if [[ "${NETBASH_FORCE:-false}" != "true" ]]; then
        __log WARN "This operation requires --force to proceed."
        return 1
    fi
    return 0
}

# If --dry-run is set, log the command instead of running it
require_dry_run() {
    if [[ "${NETBASH_DRY_RUN:-false}" == "true" ]]; then
        __log INFO "[DRY-RUN] would execute: $*"
        return 0
    fi
    "$@"
}

# Mark a module as completed in the state directory
guardar_estado() {
    local module="$1"
    local state_dir="${NETBASH_ROOT}/.state"
    mkdir -p "$state_dir"
    touch "${state_dir}/${module}"
}

# Check whether a module has been completed (idempotency guard)
verificar_estado() {
    local module="$1"
    local state_file="${NETBASH_ROOT}/.state/${module}"
    [[ -f "$state_file" ]]
}

# Remove a module's state marker
limpiar_estado() {
    local module="$1"
    local state_file="${NETBASH_ROOT}/.state/${module}"
    rm -f "$state_file"
}

# EXIT trap: log unexpected failures
cleanup_handler() {
    local exit_code=$?
    if [[ $exit_code -ne 0 ]]; then
        __log ERROR "Script failed with exit code $exit_code"
    fi
    exit "$exit_code"
}

# ERR trap: log error details with line number and command
error_handler() {
    local line_no="$1"
    local cmd="$2"
    local err_code="$3"
    __log ERROR "Error at line ${line_no}: '${cmd}' exited with code ${err_code}"
}

trap 'error_handler "${LINENO}" "$BASH_COMMAND" "$?"' ERR
trap cleanup_handler EXIT

readonly __required_utils=(curl wget grep sed awk)
# Verify that all required command-line utilities are available
verificar_requisitos() {
    local missing=()
    for util in "${__required_utils[@]}"; do
        if ! command -v "$util" &>/dev/null; then
            missing+=("$util")
        fi
    done
    if [[ ${#missing[@]} -gt 0 ]]; then
        die "Missing required utilities: ${missing[*]}"
    fi
}
