---
name: review-loop
description: Review a branch with parallel fresh-context reviewers, verify each finding against the code, fix the confirmed ones and re-review only the fixes, in one session.
disable-model-invocation: true
argument-hint: "[base-ref]"
---

# Review loop

Run review → verify → fix → commit in this session, at most twice, and hand the user only the findings that need their decision. Reviewers and verifiers run as **fresh-context subagents**, so each one reads the code cold. You orchestrate and fix; you don't review.

The loop exists to **converge**. Each round reviews only the code the last round didn't accept, and every finding has to survive a verifier before anyone acts on it. Run the rounds back to back without pausing for the user. The stop rules in step 5 are the only places the loop ends.

## 1. Pin the scope

Run `git fetch origin`. The base is the argument if one was given; otherwise use the merge-base with `origin/<default branch>`. Confirm the diff is non-empty and the tree is clean. Stash or ask about uncommitted work, because the loop commits.

Check for a ledger from an earlier run (see step 5). If there is one, resume from it: the first round reviews `<ledgered reviewed tip>..HEAD`, and the ledger carries over. If history has been rewritten since and the tip is no longer an ancestor of `HEAD`, review the whole branch but keep the ledger. Each run gets two rounds of its own.

Then record the following:

- **Round scope**: `<base>...HEAD` in a fresh run's first round, and `<last reviewed sha>..HEAD` after that.
- **Reviewed tip**: the `HEAD` sha the round reviews.
- **Purpose**: what the branch is for, in a sentence or two. Take it from the spec if there is one: an issue referenced in the commits, a design note under `.scratch/` or `docs/`, or a path the user gave. Otherwise infer it from the commit messages and the branch name. For a bug fix, the purpose is the reported failure *and its root cause*, so other paths to the same cause are in scope. For developer-facing work, it's the stated goal, such as faster tests or a simpler build. If the purpose can't be inferred, ask the user for one sentence before reviewing.
- **Checks**: the repo's own build, test, lint and spelling commands, from its CI workflow, `package.json` or build files.

Done when the round scope resolves, its `--stat` is in hand, and the checks are listed.

## 2. Review in parallel

This skill asks you to delegate. Spawn one subagent per lens in a single turn: in Claude Code with the Agent tool, in Codex with `spawn_agent` and `fork_turns="none"`. Without that setting a Codex subagent inherits this conversation and reviews with the author's reasoning in mind. Keep the fan-out to the lenses below. Give each subagent a self-contained brief:

- The round scope as a diff **command**, plus its `--stat` and commit list. Don't paste the diff: the subagent reads what it needs.
- The lens brief below. Where a brief names a skill, give the absolute path of its `SKILL.md`, resolved from this file's directory, and tell the subagent to read that file and follow it in report-only mode. These skills are user-invoked, so a subagent can't load them by name.
- The **ledger** from earlier rounds (see step 5), so that it doesn't re-raise settled findings.
- The return format under *Finding format*.

| Lens | Brief | When |
|---|---|---|
| Correctness | Find bugs a user or caller would observe: wrong results, lost or duplicated work, broken contracts, unsafe concurrency, unhandled failure paths. Read the callers and the configuration around each hunk, not just the hunk itself. | Always |
| Design | Follow [`../design-review/SKILL.md`](../design-review/SKILL.md): check concepts, rule ownership, boundaries, invariants and premises together. | Always |
| Tests | Follow [`../test-review/SKILL.md`](../test-review/SKILL.md) on the round scope. | When the scope changes tests or testable behaviour |
| Purpose | Check that the change achieves its purpose completely. Find parts that are missing, partial or wrong, and behaviour unrelated to the purpose. Quote the spec line where there is one. For a bug fix, check that the fix removes the root cause rather than the reported symptom, and that a test fails on the base without it. | A run's first round |

### Finding format

Ask every reviewer for this format. Reviewers report everything they find and grade it; the filtering happens in step 3. A reviewer told to report only serious findings also misses serious ones. "No findings" is a valid return.

```
- id: <lens>-<n>
  where: <file:line>
  claim: <one sentence>
  scenario: <the input or state, what goes wrong, who observes it>
  fix: <the proposed change, or "decision: <the options>" when more than one fix is defensible>
  severity: blocker | should-fix | nit | pre-existing
```

`blocker` and `should-fix` mean you'd block the merge, or at least demand a reply. `pre-existing` means the problem behaved the same way on the base, before this branch. Anything within the branch's purpose is never pre-existing: the branch owns it.

## 3. Verify

Filter first:

- Drop nits, and anything the repo's linters or analyzers already catch.
- Check each `pre-existing` finding before shelving it. Confirm it's outside the purpose, and look once at the same behaviour on the base. Move it to the follow-ups only if both hold. Otherwise regrade it and verify it like any other finding.
- Deduplicate: two findings with the same root cause become one. Keep every lens's scenario, because each one is evidence.

Then give every remaining finding to a fresh-context verifier subagent, spawned the same way as the reviewers. Batch findings by file or module, with at most four verifiers, and give each one the findings verbatim plus the diff command. The verifier's job is to **disprove**:

- Reproduce the scenario from the code. Trace the actual callers, configuration, schema and infrastructure (the topic settings, the table definitions, the DI registrations). Run a test or a query where that's cheap. When the scenario depends on a fact the verifier can't establish, the verdict is `unverified`, not `confirmed`.
- Judge the diagnosis and the proposed fix separately, because a right diagnosis often comes with a wrong prescription.
- Mark `decision` when more than one fix is defensible and the choice changes a contract, a schema, scope or a trade-off the user owns.

The verdict is one of `confirmed`, `rejected`, `unverified` or `decision`. Each one comes with its evidence: the file:line, command output or caller chain that settles it.

Done when every finding has a verdict with evidence.

## 4. Fix and commit

Fix the `confirmed` findings, following the *Address the feedback* section of [`../address-review/SKILL.md`](../address-review/SKILL.md). In short, group by root cause before editing, look for the fix that removes the cause, and escalate any invariant that a reshape could make true by construction rather than guarding it. When a fix turns out to need a decision, move it to the `decision` pile instead of choosing.

Run the checks that cover the change. Commit each root-cause group as a **plain commit** per [`../commit/SKILL.md`](../commit/SKILL.md), without `fixup!` or `amend!`. History is restructured once, before the PR.

## 5. Update the ledger and decide whether to loop

The ledger records the round number and reviewed tip, plus one line per finding: its id, the claim, the verdict, the reason, and the commit for a fixed finding. Keep it in `$(git rev-parse --git-dir)/review-loop/<branch>.md`, where it survives compaction and is never committed. Give it to every later reviewer, with the instruction: *a ledgered finding is settled. Raise it again only with new evidence the ledger doesn't address.*

Stop at the first of these:

- **Converged**: the round confirmed nothing.
- **Round 2 done**: there's no round 3. Fix round 2's `confirmed` findings only when the fix is local. Otherwise move them to `decision`. No reviewer sees these fixes, so give each one to a verifier with its finding. The verifier checks that the fix resolves the finding and changes nothing else. Revert any fix that fails and move its finding to `decision`.
- **Not converging**: a round-2 finding refutes a round-1 diagnosis, so that fix treated a symptom. Report the pair as one `decision` item and don't patch it again. A finding that faults only how a sound diagnosis was carried out is an ordinary round-2 finding.

Otherwise, set the round scope to `<reviewed tip>..HEAD` and return to step 2. Round 2 reviews the fix commits only, so drop the Purpose lens.

## 6. Hand off

Before reporting, run the full set of checks from step 1. CI failures after the PR opens cost a whole extra round.

Report:

1. **Needs your decision**: each `decision` and `unverified` item, with the options, the evidence and your recommendation. This is the main output, so put it first.

   When the root-cause fix addresses the branch's purpose, recommend doing it in this branch. Recommend a follow-up only for work outside the purpose. Never recommend a guard as the interim fix for an invariant a reshape would make true by construction. Per address-review, guards written now are the reason the reshape never happens.
2. **Fixed**: one line per root-cause group, with its commit.
3. **Rejected**: one line each, with the evidence. This keeps you from re-litigating them.
4. **Follow-ups**: pre-existing problems, left unfixed.
5. **Checks**: what ran and what passed. Name anything that couldn't run.
6. **Loop**: the rounds run and why the loop stopped.

Then offer to run `/restructure-commits` once, before the PR.
