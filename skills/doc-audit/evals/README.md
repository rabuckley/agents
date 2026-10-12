# doc-audit evals

## outbox (dsegroup/code 46c3c7c9ed14)

A whole-tree case built from a real commit rather than a hand-written snippet. The target
is `docs(core): clarify outbox guarantees` in dsegroup/code: 30 files, 531 insertions,
1478 deletions, over the five `DSE.Core.Outbox.*` projects, the outbox SQL and three guides.
The eval scopes to the 26 C# source files (`/doc-audit src/core/src`) and treats the target
commit's after-state as the expected result.

The source is proprietary, so nothing from it lives here. `outbox/build-fixture.sh` builds
the before tree as a throwaway git repo plus the expected tree from a local checkout;
`outbox/score.py` counts comment lines per file and checks named markers in the before,
expected and result trees.

What the target did, for calibration:

- Cut comment lines from 1358 to 623 (45% kept). Private and internal members lose their
  `///` blocks outright (`BatchOutcome`, `PendingFailure`, `ParkReason`, `LeasedClaim`,
  `FormatError`, `CountersReading`); a one-line `//` survives only where it carries a fact.
- Fixed five false claims: that the options validator "proves" a batch fits its lease
  (it checks configured timeouts, not elapsed lease time, and the dispatcher neither renews
  leases nor checks a deadline); the `dse.outbox.lease.lost`, `dse.outbox.parked` and
  `dse.outbox.purged` metric descriptions; "skew-free by construction" on the age gauge.
- Rewrote the public XML docs to the `csharp-xml-docs` house style: `Asynchronously <verb>`
  summaries, empty `<param name="cancellationToken">`, `The default is X.`, noun-phrase
  params, `<exception>` before `<remarks>`, no em dashes.
- Dropped design rationale wholesale, including "why" passages the doc-audit keep list
  protects: "deliberately not configurable", the MassTransit QueryDelay reference, the
  `readpast` under RCSI note, the lock-escalation sizing of `PurgeBatchSize`.

### Results, sonnet, 2026-09-08 (skill at 95040d2f, "rework doc audit")

Runs via `claude -p --model sonnet` in the fixture repo, skill invoked as `/doc-audit <scope>`.
Raw transcripts and diffs stay in the session scratchpad; they quote proprietary source.

| Scope | Turns | Comment lines kept (target) | Target's 5 false claims fixed | House style applied | Edits |
| --- | --- | --- | --- | --- | --- |
| `src/core/src` (26 files) | 55, 354 s, $1.34 | 100% (45%) | 0 | none | 1: completed a truncated csproj `<Description>` |
| `OutboxClaim.cs` | 29 | 98% (53%) | 0 of 2 present | emptied 2 `cancellationToken` params, declined the rest | 2 |
| `OutboxDispatcherMetrics.cs` | 23 | 101% (26%) | 0 of 5 present | none | 2 (added the lease-lost counter to the class summary; reworded one `error.type` claim) |
| `OutboxDispatcherService.cs` | 32 | 101% (33%) | 0 of 1 present | none | 4 (per-loop retry delay; "at most 15 minutes"; MassTransit lead-in; "applock" in a storage-free project) |

Whole-tree verdict, quoted: "overwhelmingly keep. This is unusually well-written documentation,
nearly every comment carries a real, non-obvious why". No verdict table was produced for the
tree; each single-file run did produce one.

### Why sonnet does not reach the target

1. **The skill's keep list protects what the target deleted.** Most of the 735 lines the target
   cut are design rationale: what a choice saves, what the alternative would cost, tuning
   advice, prior art. The skill names "a why" first among the keeps and gives one sentence
   against over-keeping. Sonnet did what the text says. `csharp-xml-docs` has the rule the
   target applied ("State the contract, never the case for the design", reasoning goes in a
   `//` or nowhere) and `doc-audit` does not point at it or restate it.
2. **No test for over-strong guarantees.** The five claims the target corrected are true
   sentences with a false modal: the validator "proves" a bound cooperative cancellation
   cannot enforce; `lease.lost` "means a publisher overran"; the age gauge is "skew-free by
   construction". The skill's fix verdict is referent-shaped (grep, `git log -S`), and sonnet
   confirmed the referents resolve and called the lease bound "startup-validated" and true.
   The pre-rework `references/csharp.md` flagged guarantee words ("proves", "never", "exactly",
   "cannot") for checking; that check went with the rework.
3. **House style is not in scope and sibling consistency wins.** On `OutboxClaim.cs` sonnet
   considered `csharp-xml-docs`, found the other 25 files consistent in the old style, and
   declined to "trade one inconsistency for another". The skill's "apply the same conventions
   you would to a document" gives no ruling on style versus sibling consistency and no
   pointer to the C# skill. Whole-tree, where consistency was not an excuse, style was never
   considered.
4. **Private and internal `///` blocks are judged sentence by sentence.** The target removes
   them wholesale (`BatchOutcome`, `PendingFailure`, `ParkReason`, `LeasedClaim`, helpers)
   and keeps a one-line `//` only where a fact survives. The adjacent-code test passes most of
   those summaries as keeps because a record's three fields do not spell out what a "publish
   pass decided".
5. **The process flattens at tree scale.** "Inventory everything in scope before judging any
   of it" over 1358 comment lines filled ~150k tokens of context with reads; the judging step
   then collapsed to one paragraph and no table. Per file, the same skill fires with 2 to 4
   defensible fixes. The whole-tree run also left the fixture to grep the real checkout for
   referents, which an eval should prevent.

What sonnet found that the target did not: the class remarks on `OutboxDispatcherService`
claimed every loop backs off after a failure when only the dispatch loop does (the target
also dropped that sentence); "at most 15 minutes" stated a default as a ceiling; the
MassTransit reference names a library the repo does not depend on; the metrics summary
omitted the lease-lost counter. The judgement is sound on a file; the target's bar is a
different set of rules, not better execution of these.

Provenance note: no Claude Code transcript on this machine contains the target's edits, so the
commit was produced by another tool or by hand. The eval compares doc-audit with a bar set
outside it.
