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

exit "$skipped"
