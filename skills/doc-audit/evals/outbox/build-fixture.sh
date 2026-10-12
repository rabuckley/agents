#!/usr/bin/env bash
# Builds the outbox eval fixture from a local dsegroup/code checkout.
#
# The case is the outbox documentation rewrite committed as 46c3c7c9ed14 in dsegroup/code.
# Its source is proprietary, so nothing from it is stored here; this script materialises the
# "before" tree (parent commit) as a throwaway git repo and the "after" tree beside it.
#
# Usage: build-fixture.sh <dsegroup/code checkout> <output dir>
# Then:  cd <output dir>/fixture && claude -p --model claude-haiku-5-5 --dangerously-skip-permissions \
#            --output-format stream-json --verbose "/doc-audit src/core/src" > ../run.jsonl
#        python3 score.py <output dir>/fixture <output dir>/expected <output dir>/fixture src
set -euo pipefail
repo=${1:?dsegroup/code checkout}; out=${2:?output dir}
target=46c3c7c9ed14c23e4ddbd3b39b686a1ae90c262b
base=$(git -C "$repo" rev-parse "$target^")
mkdir -p "$out/fixture" "$out/expected"
git -C "$repo" archive "$base" \
  src/core/src/DSE.Core.Outbox.Abstractions src/core/src/DSE.Core.Outbox.Dispatcher \
  src/core/src/DSE.Core.Outbox.EntityFrameworkCore src/core/src/DSE.Core.Outbox.EntityFrameworkCore.SqlServer \
  src/core/src/DSE.Core.Outbox.Dapr src/core/data/outbox \
  src/core/test/DSE.Core.Outbox.Abstractions.Tests src/core/test/DSE.Core.Outbox.Dispatcher.Tests \
  src/core/test/DSE.Core.Outbox.EntityFrameworkCore.Tests src/core/test/DSE.Core.Outbox.EntityFrameworkCore.SqlServer.Tests \
  docs/outbox.md docs/outbox-operations.md docs/transactional-outbox.md \
  | tar -x -C "$out/fixture"
for f in $(git -C "$repo" show --name-only --format= "$target"); do
  mkdir -p "$out/expected/$(dirname "$f")"
  git -C "$repo" show "$target:$f" > "$out/expected/$f"
done
git -C "$out/fixture" init -q
git -C "$out/fixture" add -A
git -C "$out/fixture" -c user.name=fixture -c user.email=fixture@example.invalid commit -qm before
echo "fixture: $out/fixture (before, committed); expected: $out/expected"
