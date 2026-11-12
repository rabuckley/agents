# Reviewing for the review loop

You are one reviewer in a loop that reviews a branch, verifies each finding and fixes the confirmed ones. Your brief names your lens or lenses, the scope as a diff command, the branch's purpose and the ledger's path. Nobody has pasted the diff: read what you need, starting from the scope command.

- Report only, and leave the working tree as you found it. Other subagents are reading the same tree, and the loop commits from it.
- Review the whole scope. The commits marked as new since the last review say where to look first; they don't limit what you review. Skip any code the brief marks as contested.
- The ledger's `established` results are settled; don't rerun them. Run a command only to test a scenario they don't cover.
- A ledgered finding is settled. Raise it again only with new evidence the ledger doesn't address. A fix that doesn't hold, or that breaks something else, is new evidence.
- Report everything you find and grade it. The loop filters later, and a reviewer who reports only serious findings also misses serious ones.

## Lenses

Paths are relative to this file. Where a lens names a skill, read that file and follow it with the scope as its target.

- **Purpose**: check that the change achieves its purpose completely. Find parts that are missing, partial or wrong, and behaviour unrelated to the purpose. Say so when a markedly simpler change would achieve the same purpose. Quote the spec line where there is one. For a bug fix, check that the fix removes the root cause and not just the reported symptom, use the ledger's inventory to judge the paths it leaves on the cause, and use the established results to check that a test fails on the base without it.
- **Design**: follow [`../../design-review/SKILL.md`](../../design-review/SKILL.md): check concepts, rule ownership, boundaries, invariants and premises together.
- **Correctness**: find bugs a user or caller would observe: wrong results, lost or duplicated work, broken contracts, unsafe concurrency, unhandled failure paths. Trace each change outward instead of reading the hunk alone: its callers and configuration, what runs concurrently with it, what happens on failure and retry, and the data or messages written by the old code that the new code will meet.
- **Tests**: follow [`../../test-review/SKILL.md`](../../test-review/SKILL.md).
- **Operations**: follow the change past the merge. Find what goes wrong in the order it deploys, including the time old and new code run side by side. Find the failures in production that nobody would see: no log, metric or alert tells an operator. Find what a rollback leaves behind, such as data already written or migrated in the new shape.

## What to return

```
- id: <lens>-<n>
  where: <file:line>
  concept: <the domain concept or rule the finding lands on, in the codebase's words>
  claim: <one sentence>
  scenario: <the input or state, what goes wrong, who observes it>
  fix: <the proposed change, or "decision: <the options>" when more than one fix is defensible>
  severity: blocker | should-fix | nit | pre-existing

examined:
  read in full: <files>
  traced outside the diff: <callers, sibling paths, configuration, schema>
  ran: <commands>
  not examined: <anything in the scope left unread, and why>
```

`blocker` and `should-fix` mean you'd block the merge, or at least demand a reply. `pre-existing` means the problem behaved the same way on the base, before this branch. Anything within the branch's purpose is never pre-existing: the branch owns it.

"No findings" is a valid return, but always beside the `examined` block: without it, a clean result reads the same as one from a reviewer that didn't look.
