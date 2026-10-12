---
name: commit
description: Commits the user's changes. Use when the user asks to commit or save their changes.
---

# Commit

## Working tree

Status:

!`git status --short`

Recent messages, for the repo's conventions:

!`git log -10 --no-merges --format='--- %s%n%b'`

## What goes in

Commit the changes that belong to the work at hand.

Each commit should be one logical change.

When a change corrects something an unpushed commit on this branch introduced, commit it as a fixup of that commit, so the history never contradicts itself: `git commit --fixup=<sha>`, or `--fixup=amend:<sha>` with a corrected message when the change makes that commit's message wrong. Tell the user it's folded in by `git rebase --autosquash <base>`, or offer `/restructure-commits` when the fixups need regrouping.

Git rejects `-F` with any `--fixup`, and `-m` with `--fixup=amend:`. Without an editor, write the corrected message by running `git commit --fixup=amend:<sha> --no-edit`, then `git commit --amend -F <file>` where the file's first line stays `amend! <original subject>` and the rest is the full replacement message. A failed fixup commit leaves its files staged, so check the commit landed before staging the next one.

## Message

State only reasons you know. Keep the title under 72 characters, ideally nearer 50, and wrap the body at 72.

Where the log above shows no settled convention, use a conventional commit title.
