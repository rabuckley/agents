---
name: commit
description: Commits the user's changes. Use when the user asks to commit or save their changes.
---

# Commit

## Working tree

Status:

!`git status --short`

Recent subjects, for the repo's conventions:

!`git log -10 --no-merges --format=%s --ignore-missing HEAD`

The latest two in full:

!`git log -2 --no-merges --format=%n%B --ignore-missing HEAD`

## What goes in

Commit the changes that belong to the work at hand.

Each commit should be one logical change.

`git commit` takes the whole index, including whatever `git rm` or `git mv` staged when they ran and whatever a failed commit left staged. Check `git diff --cached --name-status` before each commit.

When a change corrects or completes an unpushed commit on this branch, commit it as an fixup/amend of that commit, so the history never contradicts itself: `git commit --fixup=<sha>`, or `--fixup=amend:<sha>` with a corrected message when the change makes that commit's message wrong. Route each hunk to the commit whose change made it necessary. `git blame` finds that commit only when the hunk edits lines the commit wrote; for a stale doc, find the commit that changed the behaviour it describes. When a fix changes an API, the call sites a later commit added go in that later commit's fixup; otherwise the commits between won't build. To stage part of a file, write those hunks to a patch and `git apply --cached` it.

Once the user wants history tidied, create a backup branch, then fold the fixups in:

```sh
GIT_SEQUENCE_EDITOR=: GIT_EDITOR=true git rebase -i --autosquash --autostash \
  --exec '<check>' --reschedule-failed-exec "$(git merge-base HEAD <target>)"
```

`<target>` is the branch this one merges into; `<check>` is the repo's build, plus tests where affordable. `--autostash` sets uncommitted work aside, so it neither blocks the rebase nor leaks into the checks. `--exec` checks each resulting commit in place, so no separate worktree is needed. When a check fails, the rebase stops on that commit: fix it, `git commit --amend --no-edit`, and `git rebase --continue` reruns the check. Report any commit left unchecked. Offer `/restructure-commits` when the fixups need regrouping.

Git rejects `-F` with any `--fixup`, and `-m` with `--fixup=amend:`. Without an editor, run `git commit --fixup=amend:<sha> --no-edit`, then `git commit --amend -F <file>`. The file must contain the `amend!` marker, a blank line, then the complete replacement message starting with its own subject:

```text
amend! feat(x): old subject

feat(x): new or same subject

Body…
```

Autosquash removes the marker paragraph; the replacement subject must appear below it even when unchanged.

## Message

State only reasons you know. Keep the title under 72 characters, ideally nearer 50, and wrap the body at 72.

Apply [unslop](../unslop/SKILL.md) to the commit message, subject included.

For an internal API, the subject names the API and what it now takes or returns.

Where the log above shows no settled convention, use a conventional commit title. Take the title format from the log, not the body length: long bodies in recent commits are not a reason to write one.
