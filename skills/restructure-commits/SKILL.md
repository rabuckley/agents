---
name: restructure-commits
description: Rework an unmerged branch's commits into clean, reviewable history.
disable-model-invocation: true
---

# Restructure commits

Rework an unmerged branch's commits so each is one logical change — however many or few that makes. A commit's tests, docs and config travel with it. Keep commits and messages that still fit; draft the rest per [`../commit/SKILL.md`](../commit/SKILL.md) so they read correctly at their point in the series. Restructuring changes how the change is *told*, never what it *is* — the tip tree comes out **byte-identical**, apart from changes the user explicitly approves.

## Process

1. **Scope.** Resolve the base (user-named, else the merge-base with the branch this one will merge into) and confirm the branch is unmerged. Stash or ask about uncommitted changes; the backup branch doesn't protect them.
2. **Design** the target series. Where it involves choices the request didn't settle (split points, drops, drive-bys), show the user the old → new mapping with proposed subjects and wait for approval.
3. **Back up — mandatory.** Before touching history, create a backup branch at HEAD; never `-f` over an existing one.
4. **Rebuild** (mechanics below).
5. **Verify**:
   - Invariant: `git diff <backup> HEAD` shows exactly the approved content changes — usually nothing. Anything else means the rebuild lost or invented content — find out which.
   - Check `git range-diff <base>..<backup> <base>..HEAD` against the designed mapping.
   - Bisectability: where the history will be bisected, verify each commit builds and passes as far as is affordable, and name what went unverified.
   - Leave the backup in place; give the user the command to drop it.

## Mechanics

This harness has no interactive editor, so `git add -p` and editor-driven `git rebase -i` are out. To script a rebase, write the todo to a file, then `GIT_SEQUENCE_EDITOR='cp <todo-file>' GIT_EDITOR=true git rebase -i <base>`.

Traps and tricks:

- Commits you create, rather than cherry-pick, are authored by the current user; credit the original author where the work was someone else's.
- Splitting one file across commits: pin the endpoint. `git checkout <backup> -- <file>` restores the final content, so only the intermediate state is yours to hand-edit and the invariant survives by construction. The same move resolves conflicts whenever no later commit still touches the file.
- Squashes: keep the source commits' attribution, and any issue refs that still apply. Under `GIT_EDITOR=true` a `squash` keeps the concatenated messages as-is; follow it with `exec git commit --amend -F <msg-file>` to set the drafted one.
- Lost the thread: `git reset --hard <backup>` and restart. That backup is why it exists.
