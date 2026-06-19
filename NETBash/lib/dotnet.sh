#!/usr/bin/env bash
# NETBash - .NET Runtime/SDK installation: Microsoft repo, Runtime 10, optional SDK
set -Eeuo pipefail

readonly DOTNET_RUNTIME_PACKAGE="aspnetcore-runtime-10.0"
readonly DOTNET_SDK_PACKAGE="dotnet-sdk-10.0"
readonly DOTNET_CHANNEL="10.0"

# Add the Microsoft package repository for Ubuntu 24.04
dotnet_instalar_repo() {
    if verificar_estado "dotnet_repo"; then
        __log INFO "Microsoft repository already configured."
        return 0
    fi

    __log INFO "Configuring Microsoft package repository..."
    require_dry_run wget -qO- https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb \
        -O /tmp/packages-microsoft-prod.deb
    require_dry_run dpkg -i /tmp/packages-microsoft-prod.deb
    require_dry_run rm -f /tmp/packages-microsoft-prod.deb
    require_dry_run apt-get update -qq

    guardar_estado "dotnet_repo"
    __log SUCCESS "Microsoft repository configured."
}

# Install the ASP.NET Core Runtime 10.0 (framework-dependent deployment)
dotnet_instalar_runtime() {
    if verificar_estado "dotnet_runtime"; then
        __log INFO ".NET Runtime ${DOTNET_CHANNEL} already installed."
        return 0
    fi

    dotnet_instalar_repo

    __log INFO "Installing .NET Runtime ${DOTNET_CHANNEL}..."
    require_dry_run apt-get install -y -qq "${DOTNET_RUNTIME_PACKAGE}"

    guardar_estado "dotnet_runtime"
    __log SUCCESS ".NET Runtime ${DOTNET_CHANNEL} installed."
}

# Install the .NET SDK 10.0 (optional, controlled by --sdk flag)
dotnet_instalar_sdk() {
    if verificar_estado "dotnet_sdk"; then
        __log INFO ".NET SDK ${DOTNET_CHANNEL} already installed."
        return 0
    fi

    dotnet_instalar_repo

    __log INFO "Installing .NET SDK ${DOTNET_CHANNEL}..."
    require_dry_run apt-get install -y -qq "${DOTNET_SDK_PACKAGE}"

    guardar_estado "dotnet_sdk"
    __log SUCCESS ".NET SDK ${DOTNET_CHANNEL} installed."
}

# Verify the installed .NET version
dotnet_verificar() {
    __log INFO "Verifying .NET installation..."
    if command -v dotnet &>/dev/null; then
        local version
        version="$(dotnet --version 2>/dev/null || true)"
        __log SUCCESS "dotnet version: ${version:-unknown}"
    else
        __log WARN "dotnet command not found in PATH."
    fi
}
