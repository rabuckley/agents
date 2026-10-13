---
name: commit
description: Commits the user's changes. Use when the user asks to commit or save their changes.
---

# Commit

## Working tree

Status:

!`git status --short`

Recent subjects, for the repo's conventions:

!`git log -10 --no-merges --format=%s`

The latest two in full:

!`git log -2 --no-merges --format=%n%B`

## What goes in

Commit the changes that belong to the work at hand.

Each commit should be one logical change.

`git commit` takes the whole index, including whatever `git rm` or `git mv` staged when they ran and whatever a failed commit left staged. Check `git diff --cached --name-status` before each commit.

When a change corrects something an unpushed commit on this branch introduced, commit it as a fixup of that commit, so the history never contradicts itself: `git commit --fixup=<sha>`, or `--fixup=amend:<sha>` with a corrected message when the change makes that commit's message wrong. To fold fixups in, run `GIT_SEQUENCE_EDITOR=: GIT_EDITOR=true git rebase -i --autosquash <base>` yourself once the user wants history tidied, after creating a backup branch. Offer `/restructure-commits` when the fixups need regrouping.

Git rejects `-F` with any `--fixup`, and `-m` with `--fixup=amend:`. Without an editor, write the corrected message by running `git commit --fixup=amend:<sha> --no-edit`, then `git commit --amend -F <file>` where the file's first line stays `amend! <original subject>` and the rest is the full replacement message.

## Message

State only reasons you know. Keep the title under 72 characters, ideally nearer 50, and wrap the body at 72.

Where the log above shows no settled convention, use a conventional commit title.
