#!/usr/bin/env bash
# NETBash - Release script: SemVer validation, CHANGELOG check, git tag creation
set -Eeuo pipefail

NETBASH_RELEASE_DIR="$(realpath "$(dirname "$0")/..")"

source "${NETBASH_RELEASE_DIR}/lib/common.sh"

# Validate version format (SemVer), check CHANGELOG exists, create git tag
release_crear() {
    local version="${1:-}"
    local message="${2:-}"

    if [[ -z "$version" ]]; then
        die "Usage: $0 <version> [message]"
    fi

    if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
        die "Version must follow SemVer (e.g. 1.0.0, 1.1.0, 2.0.0). Got: $version"
    fi

    local changelog="${NETBASH_RELEASE_DIR}/CHANGELOG.md"
    if [[ ! -f "$changelog" ]]; then
        die "CHANGELOG.md not found at ${changelog}"
    fi

    if ! command -v git &>/dev/null; then
        die "git is required for release."
    fi

    local repo_root
    repo_root="$(git -C "$NETBASH_RELEASE_DIR" rev-parse --show-toplevel 2>/dev/null || true)"
    if [[ -z "$repo_root" ]]; then
        die "Not a git repository."
    fi

    if ! git -C "$repo_root" diff --quiet 2>/dev/null; then
        die "Working directory has uncommitted changes. Commit or stash them first."
    fi

    __log INFO "Creating release v${version}..."

    local tag_msg="${message:-Release v${version}}"

    if git -C "$repo_root" rev-parse "v${version}" &>/dev/null; then
        die "Tag v${version} already exists."
    fi

    git -C "$repo_root" tag -a "v${version}" -m "$tag_msg"
    __log SUCCESS "Tag v${version} created."

    __log INFO "To push the tag: git push origin v${version}"
    __log SUCCESS "Release v${version} ready."
}

if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    release_crear "$@"
fi
