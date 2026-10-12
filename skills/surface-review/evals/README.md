# surface-review evals

Use [dotnet-expected.md](dotnet-expected.md) as the current .NET rubric.
The historical rounds record fixture changes and disputed judgments; they
are not a current scorecard.

The [Luna comparison](luna-results.md) records the shortening experiment and
its remaining evaluation limits.

## Comparing revisions

Snapshot the skill and references before editing. Compare that baseline and
the candidate on unchanged fixtures with the same model, reasoning setting,
tool access, and request. Use Luna when requested. Give reviewers only the
skill and raw case, without this rubric, history, or expected diagnoses.
Use fresh sessions per case when possible and disclose any grouped runs.

Record required diagnoses missed, unsupported findings, remedy quality,
classification, and available cost/latency measurements separately. Repeat
ambiguous or changed outcomes; a single pass is a smoke test, not proof of
parity. Inspect whether the applicable pack was read. Keep held-out cases
separate from examples embedded in the skill, and add cross-platform and
scope-resolution cases before claiming general performance.

## Pack coverage (before trusting a new pack)

Each non-.NET pack needs at least one fire case and one silent case before
its verdicts are trusted; the dotnet rounds showed the silent cases are
where a review skill earns its keep. Sketches, mirroring the .NET set:

- **React fire**: a component that mirrors a prop in state and syncs it
  with an Effect, plus an `onFetched`-style child fetch. Expected: state
  ownership finding (lift or `key`), not a hook-extraction suggestion.
- **React silent**: an uncontrolled input with `defaultValue`-named
  initial prop, a controlled pair as a distinct shape, status union for
  async state. Must not propose context, a custom hook, or a reducer.
- **Swift fire**: a class-based manager with a one-conformer protocol
  injected for tests, `Async`-suffixed methods returning `Result`.
  Expected: seam finding plus channel findings, struct-by-default remedy.
- **Swift silent**: a small struct API with labeled arguments, defaulted
  parameters, `throws` for its one recoverable failure. Must not propose
  a protocol or a Result-based variant.
- **Kotlin fire**: a shipped library exposing a data class and a function
  that grew a defaulted parameter in a minor release. Expected: both
  compatibility findings by name.
- **Kotlin silent**: a sealed hierarchy with `xOrNull`/throwing pair and
  read-only collections. Must not propose a builder or an interface.
- **Full-stack fire**: a .NET endpoint returning the EF entity shape plus
  the React fetch code that renders a third of it and switches on error
  message strings. Expected: boundary findings (leaked storage shape,
  error envelope) evidenced by the frontend code as scenario code, with
  both packs plus http.md read; not two disconnected single-side reviews.
- **Full-stack silent**: a screen-shaped response with a coded error
  envelope, generated client types, and an additive-only change. Must not
  import REST idiom nits above the empty shape/contract tiers.

## Results

### Round 1, sonnet subagents, 2026-08-23 (post-adversarial-review skill text)

| Case | Verdict | Note |
| --- | --- | --- |
| 01 | PASS | Shape fire, flowchart/value-with-time diagnosis, collapse remedy; pack read and cited. |
| 02 | PASS | Both seams deleted; DI-trio reflex cited by name; no mocking defence accepted. |
| 03 | PASS | Options-type remedy with init defaults; bool→enum for backoff only; Polly grounded via web. |
| 04 | PASS (tier note) | All three expected findings and remedies; top finding filed Shape where expected says Contract. |
| 05 | MARGINAL | Right three members, no interface/builder/split, but all inflated to Shape/Contract and an options reshape proposed; round 3 passed with "no Shape findings, no reshape". |
| 06 | PASS | Fired only on the waiter-priority starvation braid (round 3's own finding); QueueLimit import explicitly rejected. |
| 07 | PASS | Silent; array-downcast and imported-introspection suspicions rejected, citing the pack reflex. |
| 08 | FAIL | Fired on the silent case: single `IOException` conflating overflow vs dead watch — the suspicion round 3 raised and withdrew as a prior-art import. All four forbidden reshapes refused. |

6 pass, 1 marginal, 1 fail against round 3's clean sheet. Both blemishes
are the historical failure modes (tier inflation on 05, prior-art-shaped
fire on 08); re-run both before editing skill text to separate single-run
variance from restructure cost.

### Re-runs, sonnet, 2026-08-23

| Case | Verdict | Note |
| --- | --- | --- |
| 05 | PASS | Three expected members at Contract/Idiom tier, no reshape; create-set-call shape affirmed against `JsonSerializerOptions` prior art. Round-1 tier inflation was single-run variance. |
| 08 | fired again, identical finding | Reproducible, but argued both times from the case's own doc comment (two named terminal causes, one catchable `IOException`; the obvious retry loop cannot branch), with prior art as evidence only and all four forbidden reshapes refused. dotnet-api-review round 3 raised this same suspicion and withdrew it; that now reads as over-suppression, not correct silence. Recommendation: re-grade 08 like 06 ("must not import prior-art shapes", not "must be silent"), or harden the case with a distinct overflow exception type, after which silence is the right verdict. |
