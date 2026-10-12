# .NET evaluation rubric

This section is the current grading contract. Historical results below record
older prompts, fixtures, and judgments; they do not override this rubric.
Cases remain unchanged so prompt comparisons use the same input.

Grade diagnosis, remedy, unsupported claims, and classification separately.
A case passes only when required diagnoses are present, remedies preserve the
scenario, and no unsupported finding is added. Accept equivalent remedies;
do not require matching prose or an exact finding count. A policy finding
must name the ruling. Shape, Contract, and Idiom classify the problem rather
than determine its severity.

| Case | Required diagnosis or acceptable verdict | Reject |
| --- | --- | --- |
| `01-flowchart-type.cs` | Shape: mandatory Load/Parse/Validate/Commit ordering and partial states; collapse to an operation returning a complete result. | Naming as the primary problem; a separate finding cured by the same collapse; hazards requiring downcasts. |
| `02-di-trio.cs` | Shape: remove the forwarding service layer. Assess the repository interface separately against the decision it hides or consumer variation; the test-substitution policy applies only when that motivation is established. | DI or mocking as sufficient justification; adding behaviour solely to justify retaining the layer. |
| `03-ctor-overexposure.cs` | Shape: positional configuration and overload burden; options/defaults and readable backoff choices. | Only renaming booleans; an alternative that drops required retry capabilities. |
| `04-overspecified-return.cs` | Representation and ownership findings for Entries and GetTags; unnecessarily concrete AddRange input. Shape or Contract for exposed storage, Contract or Idiom for input restriction. | Requiring immutable copies when a read-only view serves the scenario; omitting AddRange. |
| `05-idiom-only.cs` | Idiom for public MaxLength field and boolean switches; Contract or Idiom for replaceable mutable ReservedWords. Local properties, collection ownership, and readable options/enums suffice. | Shape classification without separate evidence; interface, builder, type split, or mandatory wholesale configuration redesign. |
| `06-silent-rate-limiter.cs` | Clean verdict, or a contract finding demonstrated by a concrete inconsistency in the declared contract. Documented waiter priority is not itself a promise that TryAcquire eventually succeeds. | QueueLimit, lease metadata, blocking overloads, interfaces, or other imported requirements; guessing what stubbed helpers omit; treating caller-created waiters as unbounded internal production. |
| `07-silent-deep-type.cs` | Clean verdict. | Splitting the type, member-count complaints, cast-based array mutation, extra introspection, or speculative TryBind requirements. |
| `08-silent-beats-prior-art.cs` | Diagnostic only: the fixture does not establish recovery requirements or fully specify cancellation and validation. Record supported findings, unsupported claims, and unresolved contract questions separately; exclude it from pass totals until the intended contract is settled. | Events, disposable watcher objects, enum-plus-nullable records, extra error types solely from precedent, or assumptions about validation inside stubbed WatchCore. |

For cases 06 and 08, an unexpected finding needs human adjudication against
the stated contract. Do not silently change the expected verdict or patch the
fixture to turn a failed run into a pass.

The current seam expectation deliberately replaces the historical requirement
to delete both interfaces solely from implementation counts. The revised
skill treats those counts as evidence and reserves the house policy for
abstractions introduced solely for test substitution. Report this as a
changed review policy, not a measured performance improvement.

## Historical results

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
