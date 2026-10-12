# dotnet-api-review evals

Eight C# cases. Five must fire with a specific diagnosis; three must stay
silent. The silent cases matter most: anything can find problems, and the
skill's job includes returning a clean verdict and refusing the idiomatic
"add an interface, split it up, match the BCL" reflex.

How to run: give a fresh session (no skill loaded for baseline; skill body
pasted or invoked for treatment) the prompt *"Review the public API of the
code in `cases/<file>`"* and compare against the expected column. A case
passes only if the verdict matches: a fire with the wrong diagnosis, a fire
whose top finding is an idiom nit when a shape finding exists, or a silent
case that draws findings, fails.

## Cases

| Case | Must | Expected |
| --- | --- | --- |
| `01-flowchart-type.cs` | fire (Shape) | `CsvImporter` is a flowchart: `Load`, `Parse`, `Validate`, `Commit` must be called in order, enforced by `InvalidOperationException`, with results read back from properties. Expected: temporal decomposition / value braided with time; collapse to one operation that takes path and schema and returns a result value (rows, errors), with no ordering for the caller to learn. Not expected as the top finding: naming or the cast to `List<ImportError>`. |
| `02-di-trio.cs` | fire (Shape) | `IInvoiceService`/`InvoiceService` forwards every member to `IInvoiceRepository` with renamed signatures; both interfaces have one implementation and no consumer other than DI. Expected: pass-through layer, delete it; hypothetical seam on the interfaces (the repository interface may stay only if a second implementation or an out-of-assembly consumer is shown). Must not accept "it's the standard DI pattern" or "needed for mocking" as a defence. |
| `03-ctor-overexposure.cs` | fire (Shape) | Five constructor overloads ending in an eight-parameter constructor with three `bool`s. Expected: the common case pays for the rare features; replace with an options type (or init-only properties) with defaults and a `RetryPolicy.Default`, and an enum or strategy for backoff instead of `exponential`/`jitter` flags. An idiom-only finding ("use an enum for the bools") without the shape diagnosis fails. |
| `04-overspecified-return.cs` | fire (Contract) | `Entries` exposes the internal `SortedDictionary<string, List<int>>` (storage, mutability, and sort order all promised); `GetTags()` returns the internal `List<string>`; `AddRange` takes `Dictionary<string,int>`. Expected: leaked representation and over-specification; return `IReadOnlyDictionary`/`IReadOnlyList` or a method, take `IEnumerable<KeyValuePair<…>>` or similar. |
| `05-idiom-only.cs` | fire (Idiom only) | Shape is fine: one sealed type, one operation. Findings: public field `MaxLength`; settable `List<string>` property; `bool, bool` parameter pair. Expected: idiom findings only, ranked below any shape finding. Must NOT propose an interface, a builder, or splitting the type. |
| `06-silent-rate-limiter.cs` | silent | Sealed `TokenBucket` with an options record, `TryAcquire`/`AcquireAsync(CancellationToken)`, idempotent `Dispose`, guarded arguments. No findings. Must NOT propose `ITokenBucket`, a sync `Acquire` that blocks, more overloads, or a factory. |
| `07-silent-deep-type.cs` | silent | `RouteTemplate` has many members (Parse/TryParse/TryMatch/Bind/equality) but one job and a small contract. No findings. Must NOT propose splitting into matcher and binder types, flag member count, or flag `IReadOnlyList<string>` backed by an array (defeating it needs a cast). |
| `08-silent-beats-prior-art.cs` | silent | `DirectoryWatcher.WatchAsync` returns `IAsyncEnumerable<FileChange>` with a closed record hierarchy, against BCL prior art (`FileSystemWatcher`: events, `EnableRaisingEvents`, `Error` event, buffer size) and FDG's "prefer events over callbacks". The new shape is simpler on every scenario. No findings. Must NOT propose events, an `IDisposable` watcher object, a `FileChangeKind` enum plus nullable `OldPath`, or "consistency with FileSystemWatcher". |

## Results

### Round 1, sonnet, 2026-08-22 (skill text before the round-1 fixes)

Baselines in this round were contaminated: the no-skill agents auto-invoked
the skill through its description (their reports carry the skill's tiers and
scenario-code section). Useful as a trigger test, useless as a baseline;
re-run with skills disabled in round 2.

| Case | With skill | Note |
| --- | --- | --- |
| 01 | PASS | Shape finding, collapse-to-one-operation remedy, idiom nits suppressed. |
| 02 | PASS | Pass-through layer deleted; kept `IInvoiceRepository` with an explicit caveat. |
| 03 | PASS | Options-type remedy backed by Polly prior art; bools to enum as a secondary finding. |
| 04 | PASS | Leaked `SortedDictionary` top finding; also caught `Dictionary<string,int>` batch input being less capable than `Add`. |
| 05 | PASS (tier inflated) | Settable `List<string>` labelled Shape rather than Contract, but no reshape proposed. The auto-invoked "baseline" did reshape into a static method plus options type. |
| 06 | FAIL | Fired: imported the BCL `RateLimitLease.RetryAfter` scenario and faulted `TryAcquire` for returning `bool`; `tokens > Capacity` unvalidated; thread-safety undocumented. |
| 07 | FAIL | Fired on `IReadOnlyList<string>` backed by an array ("caller can downcast and mutate"). |

What round 1 changed in the skill: prior art reworded as evidence not
precedent, plus a ruling to that effect; design-it-twice fires only when the
alternative is clearly better (a tie was a finding before); the severity bar
excludes hazards that need a cast or reflection; a gotcha against importing
scenarios from prior art; a gotcha about stubbed bodies. Case 06 was
tightened (documented thread-safety and `Dispose` faulting waiters, `tokens`
validated against `Capacity`) so its remaining findings were real ones, not
over-firing.

### Round 2, sonnet, 2026-08-22 (revised skill; true baselines with skills disabled)

| Case | Baseline (no skill) | With skill | Note |
| --- | --- | --- | --- |
| 05 | (round 1 only, contaminated) | PASS, tier inflated | No reshape; still called settable configuration a "value with time" braid. Fix: test 9 now exempts create-set-call configuration. Also found an accidental case bug (reserved-word comparison ignored normalization); case fixed. |
| 06 | FAIL, fair | FAIL, fair | Both fired on unvalidated `TokensPerRefill`/`RefillInterval` (a real hole in the case, since patched); the skill run no longer imported the BCL lease shape. Also asked for a fairness contract; documented in the case. |
| 07 | silent | FAIL, arguable | Skill run explicitly declined the array-downcast hazard (fix held) but asked for constraint/optional semantics to be documented and a `TryBind`. Member docs added to the case; `Bind` documents why it throws. |
| 08 | FAIL, fair | PASS | Baseline flagged "buffered, never dropped" as unbounded memory (true of the case text, since bounded with `MaxPendingChanges`) and the open hierarchy (closed with a `private protected` constructor). Skill run was silent with the right reasoning: shallower than `FileSystemWatcher` on every scenario, FDG "prefer events" not invoked. |

### Round 3, sonnet, 2026-08-22 (final skill and case texts)

| Case | With skill | Note |
| --- | --- | --- |
| 05 | PASS | "No Shape-tier findings" stated outright; the three expected Contract/Idiom items; no reshape. |
| 06 | fire, not over-firing | Explicitly rejected the `QueueLimit` and `RateLimitLease` imports, citing the gotcha. Fired on a real consequence of the fairness sentence added to the case in round 2 (waiter priority can starve `TryAcquire` indefinitely). Across four rounds every 06 finding was different and defensible; the over-firing modes (prior art as requirement, stub-body speculation) are gone. Known weakness: a rate limiter invites contract questions faster than a case can close them; treat 06 as "must not import prior art", not "must be silent". Intermediate round-3 run (before the gotcha was sharpened) fired on "no `QueueLimit`", which is what prompted the sharpening. |
| 07 | PASS (after case fix) | Round-3 run fired on `TryParse` throwing for null, against `int.TryParse` convention; a fair point, case fixed. Re-run: silent, with the array-downcast and imported-introspection suspicions both rejected in the report. |
| 08 | PASS | Silent on the hardened case; withdrew the one prior-art-imported suspicion (distinct overflow exception) by name. |

Final state on sonnet: 01-05, 07, 08 as expected; 06 fires on genuine
contract questions only. Baselines without the skill (round 2): 06 fire
(fair), 07 silent, 08 fire (fair); the skill's value on the silent cases is
therefore mainly 08 (declining to fault a design for beating its prior art)
and, on the fire cases, ranking shape above idiom and proposing collapse
rather than additions.

### What the rounds cut or added in the skill

- Prior art reworded twice: evidence about needs, not a template; then
  extended to expressive-type-system languages as a source of which
  invariants can be made structural.
- Design-it-twice: a tie is not a finding (it was, in round 1's text).
- Severity bar: hazards needing a cast, reflection, or deliberate misuse
  are out; so is load the caller creates by how they call.
- Test 9: configuration set before first use (create-set-call) is not a
  value/time braid.
- Gotchas: stubbed bodies are reasoned about from the contract; prior-art
  scenarios and knobs are not requirements, and "the BCL makes it
  mandatory" is precedent, not evidence.
