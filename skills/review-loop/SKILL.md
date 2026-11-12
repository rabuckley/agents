---
name: review-loop
description: Review a branch with fresh-context reviewers, purpose and design first, prove each finding with a failing test where the code permits, fix the confirmed ones and review the fixed branch again, in one session.
disable-model-invocation: true
argument-hint: "[base-ref]"
---

# Review loop

Run review → verify → fix → commit in this session, at most twice, and hand the user only the findings that need their decision. Reviewers and verifiers run as **fresh-context subagents**, so each one reads the code cold. You orchestrate and fix; you don't review.

Four ideas shape every step. Where a step leaves a choice open, make it from these:

- **A review judges what will merge.** That is the whole branch at its tip, not the changes that got it there. A fix makes a new tree, and a verdict on the old one doesn't carry over. So every round reviews `<base>...HEAD` cold, as the PR's reader will.
- **The loop converges on claims, not on scope.** A settled claim stays settled in the ledger, and every finding has to survive a verifier before anyone acts on it. The loop has converged when a cold read of the tip confirms nothing.
- **Something that ran settles a claim; an argument only narrows it.** Fresh context removes the author's reasoning, but the model still misses what it missed before. Reviewer, verifier and fixer are usually one model, so a verifier overruling a reviewer is one mind disagreeing with itself. Wherever the code permits, a verdict rests on something that ran.
- **Not converging is itself a finding, about the design.** When fixes keep drawing findings, the cause is usually a concept with no single owner. The loop fixes local defects and escalates missing owners. Patching on would end the findings too, but with a pile of guards around a rule nobody owns.

Once step 1 is done, run the rounds back to back without pausing for the user. Step 7 holds the only stop rules. The run is long and the user sees none of the subagents' output, so say in a line what each step found as it ends.

A run gets two rounds. "Round 2" below means the run's second round, whatever its number in the ledger. No reviewer reads what round 2 commits, which is why steps 5 and 6 treat its fixes more strictly.

## The ledger

The ledger is the loop's only state. It lives in `$(git rev-parse --git-common-dir)/review-loop/<branch>.md`, where no commit picks it up, and where it survives compaction, a later run and a change of worktree. Write to it as each step produces something, so that a compaction mid-round doesn't cost the verdicts. Subagents read it by path.

```
base: <sha>
purpose: <one or two sentences> | <source: spec <path>, user, or inferred; then "confirmed by user" if they did>
fan-out: <the reviewers and verifiers planned, and the risk that sized them>
checks: <the commands>
inventory: <bug fixes only>

## Round <n>, gate reviewed <sha>, detail reviewed <sha>
established: <each command run at the tip, and its result>
examined: <per lens: files read in full, what it traced outside the diff, what it left unread>
contested: <code no detail lens read, and the decision it waits on>
- <ids> | <concept> | <claim> | <verdict> | <evidence> | <outcome>
```

- **ids**: `r<round>-<lens>-<n>`. Rounds number continuously across runs, so an id names one finding for the life of the branch. A merged finding keeps the id of every lens that raised it.
- **verdict**: `confirmed`, `rejected`, `unverified` or `decision` from a verifier; `dropped` or `follow-up` from your filter in step 4.
- **evidence**: `observed: <the test and its output>` or `traced: <file:line, caller chain>`. For `dropped`, the reason. For `follow-up`, the place on the base that shows the same behaviour.
- **outcome**: `fixed in <commit>`, or a decision's options.

## Subagents

Every subagent starts from a fresh context: in Claude Code, the Agent tool with an agent type that doesn't fork this conversation; in Codex, `spawn_agent` with `fork_turns="none"`. A subagent that inherits this conversation reviews with the author's reasoning in mind. Spawn a step's subagents in a single turn, and keep to the fan-out step 1 recorded.

Reviewers and verifiers read their own instructions, from [`references/reviewer.md`](references/reviewer.md) and [`references/verifier.md`](references/verifier.md). Give each one that file's absolute path, resolved from this file's directory, and the round's facts:

- The round scope as a diff **command**, with its `--stat` and commit list. Mark the commits new since the last review. Don't paste the diff.
- The reviewed tip.
- The purpose, and the path of the spec if there is one.
- The ledger's path.

A reviewer also gets its lens or lenses, and any contested code to skip. A verifier also gets its findings.

## 1. Pin the scope

Run `git fetch origin`. The base is the argument if the user gave one; otherwise use the merge-base with `origin/<default branch>`. Confirm the diff is non-empty and the tree is clean. Stash or ask about uncommitted work, because the loop commits.

If the branch has a ledger, it carries over: its claims are settled for this run too.

Then record in the ledger:

- **Round scope**: `<base>...HEAD`, in every round.
- **Reviewed tip**: the `HEAD` sha the round reviews.
- **New since the last review**: the commits in `<last reviewed tip>..HEAD`, when the ledger has a tip that is still an ancestor of `HEAD`.
- **Purpose**: the outcome someone needs from the branch, in a sentence or two, stated apart from how the branch achieves it. A purpose has to be able to disagree with the diff, or reviewing against it shows nothing. Take it from the spec if there is one: an issue referenced in the commits, a design note in the session scratchpad, `.scratch/` or `docs/`, or a path the user gave. Otherwise infer it from the commit messages and the branch name, and record it as inferred: that purpose came from the diff, so it can't disagree with it. If you can't infer it, ask the user for one sentence before reviewing.
- **Checks**: the repo's own build, test, lint and spelling commands, as its CI runs them.
- **Shape**: whether the branch can be reviewed as one thing, judged from the `--stat` and the commit list. When it carries more than one separable purpose, or is too large for a reviewer to read each touched module in full, the first finding is "split it". Propose the split and run no reviewer until the user has chosen.
- **Fan-out**: each reviewer must be able to read its part in full, and the number of independent reads should match what being wrong would cost and how hard it would be to undo. Line count measures neither: a five-line fix to a log message needs one reviewer per pass, carrying that pass's lenses together, and one verifier, while a five-line change to a migration needs every lens. A large feature needs its detail lenses split by module.

When the branch is a bug fix, read [`references/bug-fix.md`](references/bug-fix.md) now. It changes how you state the purpose and adds two facts to step 2.

Then show the user the purpose with its source, the shape and the fan-out, in a few lines. When the purpose is inferred or the shape says split, wait for their answer: every later step judges against the purpose, and this is the cheapest moment to correct it. Otherwise carry on.

Done when the round scope resolves, its `--stat` is in hand, the ledger holds the purpose, the fan-out and the checks, and the user has answered if you asked.

## 2. Establish the shared facts

Settle once what every subagent would otherwise work out for itself. Run the checks at the reviewed tip, unless step 6 has just run all of them there, and record the commands and their results on the round's `established` line. A check that fails is a finding: it goes to step 4's filter with the reviewers' findings.

## 3. Review: gate, then detail

The lenses run in two passes, ordered by what retires what. A Purpose or Design finding can delete the code that a detail reviewer would read line by line, so those two lenses go first.

| Pass | Lens | When |
|---|---|---|
| Gate | Purpose | Always |
| Gate | Design | Always |
| Detail | Correctness | Always |
| Detail | Tests | When the scope changes tests or testable behaviour |
| Detail | Operations | When the scope changes what a running system does or holds |

**Gate pass.** Take the gate findings through steps 4 to 6 before any detail lens runs. Gate fixes move the tip: record the new tip and redo whatever in `established` the fixes made stale.

Code that a gate `decision` could retire or reshape is *contested*: a detail finding on it is void under one of the user's options. Record it on the round's `contested` line and tell the detail reviewers to skip it. When too little survives for a detail pass to be worth running, end the round (*Gated*, step 7).

**Detail pass.** Run the detail lenses at the tip the gate left, and take their findings through steps 4 to 6.

## 4. Verify

Enter every finding in the ledger as its reviewer returned it, and each reviewer's `examined` block, before you filter anything. Once the detail pass has returned, compare the changed files in the `--stat` with the files either pass read in full. A changed file that no lens read is a gap, unless it's contested: spawn Correctness again on the gap alone.

Then filter. The filter changes a line's verdict and never removes the line. You are the only agent here that wants the loop to converge, and a finding you drop or regrade passes no verifier, so the ledger has to show each one with its reason.

- Give two names for one concept a single name. Step 5 groups findings by concept, so the names have to agree across lenses.
- Mark as `dropped` anything the repo's linters or analyzers already catch, and the nits that are minor at every scale. A reviewer grades severity where the finding sits, so a nit can be minor there and still be evidence for step 5. One that says a rule is copied, for example, points at a missing owner.
- Check each `pre-existing` finding before shelving it. Confirm it's outside the purpose, and look once at the same behaviour on the base. Mark it `follow-up` only if both hold. Otherwise regrade it, note the old grade and the reason on its line, and verify it like any other finding.
- Merge findings that share a root cause. Keep every lens's id and scenario, because each one is evidence.

Give every remaining finding to a verifier, word for word, batched by file or module.

Done when every finding has a verdict with evidence, and the ledger holds them.

## 5. Triage: local defect or missing owner

Group the `confirmed` findings by root cause. Fix a group unless one of the rules below moves it to `decision`. This step is the only place that decides whether a confirmed finding gets fixed.

**A missing owner.** Any of these signals says the group is a symptom:

- **Recurrence**: the ledger already holds a `confirmed` or `decision` entry from an earlier pass, round or run, on the same concept or on a parallel code path. A second hit after a fix means the local fixes are treating a missing owner. Name the earlier ids in the item.
- **Convergence**: three or more lenses have confirmed findings on the concept this round. A merged finding counts every lens that raised it.
- **Scatter**: the rule lives in several places with no single module enforcing it. Either a confirmed finding says so, or the fix would put it in more than one code path.

Put the owner question below to a subagent now, before fixing anything. The decision item offers the reshape that would make the rule true by construction as one option, and the local fix as the other, with the answer attached. Leave unfixed any other group whose code the answer would retire.

**A refuted diagnosis.** A finding refutes the diagnosis behind an earlier round's fix, so that fix treated a symptom. Report the pair as one `decision` item and don't patch it again. A finding that faults only how the fix carried out a sound diagnosis is an ordinary finding.

**A loop that didn't converge.** Any confirmed finding in round 2 means the cold read of the fixed branch still found something. Before fixing anything, put the owner question to a subagent over the whole ledger, both rounds together. Every group on a concept its answer names becomes a `decision`, with the answer attached. Fix what remains only when the fix is local: contained in the code the finding names, and changing no interface, schema or contract.

### The owner question

Give one subagent the branch diff command, the ledger's path, any owner answers already given, and this question:

> Name the concept these findings share. If they land on several, take each concept that two or more findings land on. For each concept, list every place in the codebase that decides it today, inside the diff or not, with file:line and counts. Which single module should own it, and what would that retire: code, tests, prose and ledger findings? "None" is a valid answer.

## 6. Fix and commit

Fix the groups that triage left, following the *Address the feedback* section of [`../address-review/SKILL.md`](../address-review/SKILL.md). When a fix turns out to need a decision, move it to the `decision` pile and don't choose.

When a finding came with a reproducing test, apply the test first and run it in this tree. It must fail before the fix and pass after it, which shows the fix works. Commit it with the fix, as the regression test. A test that doesn't fail here didn't reproduce the finding: mark the finding `unverified`, with both outputs.

Run the checks that cover the change. Commit each root-cause group as a **plain commit** per [`../commit/SKILL.md`](../commit/SKILL.md), without `fixup!` or `amend!`: you restructure the history once, before the PR. Record each commit against its findings in the ledger.

In round 2, give each fix that has no reproducing test to a verifier with its finding. Revert any fix that fails and move its finding to `decision`. This check stands in for the review those fixes won't get, and it is weaker than one.

## 7. Loop or stop

Stop at the first of these:

- **Gated**: too little code survived the gate for a detail pass. The detail lenses haven't run, and won't until the user decides.
- **Converged**: neither pass confirmed anything. A cold read of the tip that will merge came back clean.
- **Nothing changed**: the round committed no fixes, because every confirmed group became a decision. The tip is the one just reviewed.
- **Round 2 done**: the loop didn't converge. Any fix committed in this round sits on the tip unreviewed.

Otherwise, return to step 2 and review `<base>...HEAD` again at the new tip.

## 8. Hand off

Run the full set of checks from step 1 before reporting. CI failures after the PR opens cost a whole extra round.

Then read [`references/hand-off.md`](references/hand-off.md) and report as it says.
