---
name: design-first
description: Settle a change's design with the user before writing code. Use when asked to "design and implement" something, when a ticket leaves the approach open, or before adding a mechanism (guard, retry, budget, index, limit) against a race, capacity or performance problem.
---

# Design first

Write a short **design note**, get the user's yes, then implement. A design reviewed into shape after implementing costs a review round per correction, and some corrections delete the implementation.

## 1. Check the premise

Show that the problem exists before designing for it:

- **A race or ordering problem**: give the caller chain, with file:line for each hop, that produces the interleaving, and the configuration that allows it (concurrency settings, delivery guarantees, locks).
- **A capacity, size or performance problem**: measure it. Query production or representative data for the real distribution: the maximum, the p99, the growth rate. If you can't measure it, say what would.
- **A reported bug**: reproduce it, or trace the path that produces it. Then list every code path that reaches the same cause, with file:line and counts, because the design decides which level owns the fix.

When the premise fails, stop and report. The design is then "don't build it".

## 2. Choose one approach

Sketch two or three genuinely different approaches, not variations of one. Judge each one against the questions in [design-review](../design-review/SKILL.md):

- What domain concept does each new module stand for?
- Which module owns each rule?
- Where do the boundaries fall?

Pick one approach. For each rejected approach, give the one-line reason that rules it out.

## 3. Write the note

Keep it to one screen:

```
## <change>
Premise: <the evidence from step 1>
Concepts: <each domain concept involved, in the domain's words, and the module that owns it>
Approach: <the chosen design: the modules touched, the interface changes, where each rule lives>
Relies on: <each behaviour of existing code the approach needs to be safe (retry, idempotency, transaction scope, ordering, rollback)> — <file:line that guarantees it>
Rejected: <approach> — <reason>, one line each
Test seam: <where the behaviour is observed>
Open: <questions only the user can answer>
```

Hold `Relies on` to the evidence bar of step 1: read the code that provides each behaviour. A name such as `RetryPolicy` or `Session` is not a citation. A behaviour you can't cite goes under `Open`, and the note isn't ready for approval until each entry has a citation or an answer.

Save the note as `design/<branch>.md` so that later reviewers can use it as the spec. Put it in the harness's session scratchpad if it has one, such as the scratchpad directory in Claude Code's system prompt. That directory is outside the repo and doesn't need write approval. Otherwise, put it under `.scratch/` in the repo. Codex, for example, has no scratchpad. Give the user the note's full path.

## 4. Get the yes

Present the note and wait. Implement only after the user approves it, folding in their changes. Afterwards, `/review-loop` reviews the work against the note. A scratchpad doesn't outlast its session, so for a review in a later session, pass the note's path or copy the note into `.scratch/`.
