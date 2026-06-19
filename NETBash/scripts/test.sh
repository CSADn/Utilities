#!/usr/bin/env bash
# NETBash - Test suite: syntax, idempotency, logging, CLI, deploy, rollback, force
set -Eeuo pipefail

NETBASH_TEST_PASS=0
NETBASH_TEST_FAIL=0

# Run a single test case and record pass/fail
_test() {
    local name="$1"
    shift
    __log INFO "TEST: ${name}"
    if "$@" 2>/dev/null; then
        __log SUCCESS "PASS: ${name}"
        ((NETBASH_TEST_PASS++)) || true
    else
        __log ERROR "FAIL: ${name}"
        ((NETBASH_TEST_FAIL++)) || true
    fi
}

# Verify all scripts pass bash -n syntax check
test_syntax() {
    __log INFO "--- Syntax Checks ---"
    for file in "${NETBASH_DIR}/lib"/*.sh "${NETBASH_DIR}/scripts"/*.sh "${NETBASH_DIR}/netbash"; do
        _test "bash -n: $(basename "$file")" bash -n "$file"
    done
}

# Verify idempotency state functions work correctly
test_idempotencia() {
    __log INFO "--- Idempotency Checks ---"
    _test "guardar_estado / verificar_estado roundtrip" bash -c "
        NETBASH_ROOT=\$(mktemp -d)
        export NETBASH_ROOT
        source '${NETBASH_DIR}/lib/common.sh'
        guardar_estado 'test_module'
        verificar_estado 'test_module' || exit 1
        limpiar_estado 'test_module'
        verificar_estado 'test_module' && exit 1 || exit 0
        rm -rf \"\$NETBASH_ROOT\"
    "
}

# Verify log output routing (INFO -> stdout, ERROR -> stderr)
test_logging() {
    __log INFO "--- Logging Checks ---"
    _test "__log INFO outputs to stdout" bash -c "
        source '${NETBASH_DIR}/lib/common.sh'
        result=\$(__log INFO 'hello' 2>&1)
        [[ \"\$result\" == *'[INFO]'* ]] || exit 1
    "
    _test "__log ERROR outputs to stderr" bash -c "
        source '${NETBASH_DIR}/lib/common.sh'
        result=\$(__log ERROR 'test' 2>&1 1>/dev/null)
        [[ -n \"\$result\" ]] || exit 1
    "
}

# Verify CLI argument parsing
test_cli_parser() {
    __log INFO "--- CLI Parser Checks ---"
    _test "netbash --help shows usage" bash -c "
        '${NETBASH_DIR}/netbash' --help 2>&1 | grep -q 'NETBash'
    "
    _test "netbash without args fails" bash -c "
        '${NETBASH_DIR}/netbash' 2>&1 && exit 1 || exit 0
    "
    _test "netbash --force flag parsed" bash -c "
        '${NETBASH_DIR}/netbash' --force deploy --app test --dry-run 2>&1 && exit 1 || exit 0
    "
}

# Verify deploy error handling (missing source, missing DLL)
test_deploy() {
    __log INFO "--- Deploy Checks ---"
    _test "deploy_realizar fails on missing source" bash -c "
        source '${NETBASH_DIR}/lib/common.sh'
        source '${NETBASH_DIR}/lib/deploy.sh'
        source '${NETBASH_DIR}/lib/healthcheck.sh'
        source '${NETBASH_DIR}/lib/systemd.sh'
        ! ( deploy_realizar 'test-app' '/nonexistent/path' 'test.dll' 5000 ) 2>&1
    "
    _test "deploy_realizar fails on missing DLL" bash -c "
        source '${NETBASH_DIR}/lib/common.sh'
        source '${NETBASH_DIR}/lib/deploy.sh'
        source '${NETBASH_DIR}/lib/healthcheck.sh'
        source '${NETBASH_DIR}/lib/systemd.sh'
        tmpdir=\$(mktemp -d)
        ! ( deploy_realizar 'test-app' \"\$tmpdir\" 'missing.dll' 5000 ) 2>&1
        ec=\$?
        rm -rf \"\$tmpdir\"
        [[ \$ec -eq 0 ]]
    "
}

# Verify rollback error handling (no releases, nonexistent version)
test_rollback() {
    __log INFO "--- Rollback Checks ---"
    _test "rollback_al_anterior fails with no releases" bash -c "
        NETBASH_ROOT=\$(mktemp -d)
        export NETBASH_ROOT
        source '${NETBASH_DIR}/lib/common.sh'
        source '${NETBASH_DIR}/lib/rollback.sh'
        source '${NETBASH_DIR}/lib/systemd.sh'
        ! ( rollback_al_anterior 'test-app' ) 2>&1
        ec=\$?
        rm -rf \"\$NETBASH_ROOT\"
        [[ \$ec -eq 0 ]]
    "
    _test "rollback_a_version fails with no version" bash -c "
        NETBASH_ROOT=\$(mktemp -d)
        export NETBASH_ROOT
        source '${NETBASH_DIR}/lib/common.sh'
        source '${NETBASH_DIR}/lib/rollback.sh'
        source '${NETBASH_DIR}/lib/systemd.sh'
        ! ( rollback_a_version 'test-app' 'v999.0.0' ) 2>&1
        ec=\$?
        rm -rf \"\$NETBASH_ROOT\"
        [[ \$ec -eq 0 ]]
    "
}

# Verify --force guard works correctly
test_force() {
    __log INFO "--- --force Checks ---"
    _test "require_force fails without --force" bash -c "
        source '${NETBASH_DIR}/lib/common.sh'
        NETBASH_FORCE=false
        export NETBASH_FORCE
        require_force 2>&1 && exit 1 || exit 0
    "
    _test "require_force passes with --force" bash -c "
        source '${NETBASH_DIR}/lib/common.sh'
        NETBASH_FORCE=true
        export NETBASH_FORCE
        require_force
    "
}

# Run the full test suite and report results
test_framework() {
    __log INFO ""
    __log INFO "=============================="
    __log INFO "  NETBash Test Suite"
    __log INFO "=============================="

    test_syntax
    test_idempotencia
    test_logging
    test_cli_parser
    test_deploy
    test_rollback
    test_force

    __log INFO "=============================="
    __log INFO "Results: ${NETBASH_TEST_PASS} passed, ${NETBASH_TEST_FAIL} failed"
    __log INFO "=============================="

    if [[ $NETBASH_TEST_FAIL -gt 0 ]]; then
        return 1
    fi
}

if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    NETBASH_DIR="${NETBASH_DIR:-$(realpath "$(dirname "$0")/..")}"
    # shellcheck disable=SC1091
    source "${NETBASH_DIR}/lib/common.sh"
    test_framework
fi
