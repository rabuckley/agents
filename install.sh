#!/usr/bin/env bash
# Links this repo into each agent tool's user-level config location.
# Safe to re-run. Existing symlinks are repointed; real files or dirs are
# never touched, so move them aside yourself and re-run.
#
# Usage: ./install.sh [--dry-run]

set -euo pipefail

repo="$(cd "$(dirname "$0")" && pwd)"
codex_home="${CODEX_HOME:-$HOME/.codex}"
dry_run=0
[[ "${1:-}" == "--dry-run" ]] && dry_run=1
skipped=0

link() {
  local src="$repo/$1" dst="$2"

  if [[ -L "$dst" ]]; then
    if [[ "$(readlink "$dst")" == "$src" ]]; then
      echo "ok      $dst"
      return
    fi
    echo "relink  $dst -> $src (was $(readlink "$dst"))"
  elif [[ -e "$dst" ]]; then
    echo "SKIP    $dst is a real file or dir; move it aside and re-run" >&2
    skipped=1
    return
  else
    echo "link    $dst -> $src"
  fi

  if (( ! dry_run )); then
    mkdir -p "$(dirname "$dst")"
    ln -sfn "$src" "$dst"
  fi
}

link AGENTS.md      "$HOME/.claude/CLAUDE.md"
link skills         "$HOME/.claude/skills"
link claude/rules   "$HOME/.claude/rules"

link skills         "$HOME/.agents/skills"

link AGENTS.md      "$codex_home/AGENTS.md"
# Must be a directory link: Codex ignores symlinked .rules files (openai/codex#32658).
link codex/rules    "$codex_home/rules"

link AGENTS.md      "$HOME/.copilot/copilot-instructions.md"

# Runs a plugin CLI command unless dry-running; a failure is reported, not fatal.
run() {
  echo "run     $*"
  (( dry_run )) && return
  "$@" || { echo "FAIL    $*" >&2; skipped=1; }
}

# Third-party skills are installed as plugins rather than vendored into
# skills/, so they update upstream and don't shadow the ones written here.
# Codex and Copilot error on re-adds, so each step checks the tool's list first.
plugin() {
  local tool="$1" repo_slug="$2" marketplace="$3" name="$4"

  if ! command -v "$tool" >/dev/null; then
    echo "SKIP    $tool not on PATH; $name@$marketplace not installed"
    return
  fi

  case "$tool" in
    claude)
      # Claude's add and install are both no-ops when already done.
      run claude plugin marketplace add "$repo_slug"
      run claude plugin install "$name@$marketplace"
      ;;
    codex)
      codex plugin marketplace list 2>/dev/null | grep -q "^$marketplace " \
        || run codex plugin marketplace add "$repo_slug"
      if codex plugin list 2>/dev/null | grep -Eq "^$name@$marketplace +installed"; then
        echo "ok      codex $name@$marketplace"
      else
        run codex plugin add "$name@$marketplace"
      fi
      ;;
    copilot)
      copilot plugin marketplace list 2>/dev/null | grep -q " $marketplace " \
        || run copilot plugin marketplace add "$repo_slug"
      if copilot plugin list 2>/dev/null | grep -q " $name@$marketplace "; then
        echo "ok      copilot $name@$marketplace"
      else
        run copilot plugin install "$name@$marketplace"
      fi
      ;;
  esac
}

for tool in claude codex copilot; do
  plugin "$tool" mattpocock/skills mattpocock mattpocock-skills
done

exit "$skipped"
