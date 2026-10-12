---
name: surface-review
description: Review the public surface of a library, module, component, or API proposal in any language, judging the abstraction first, the contract second, and platform idiom last.
disable-model-invocation: true
argument-hint: "[path | type or component name | git-ref | proposal file]"
---

# Public surface review

Judge how much callers can do against what they must know. Review **Shape** (the abstraction), **Contract** (correct usage), then **Idiom** (platform conventions). Prefer removing caller obligations to adding wrappers or documentation. Produce a report; change no files.

## Target and evidence

`$ARGUMENTS`

A path or name selects the current surface; a git ref selects changes against its merge-base with HEAD; a proposal selects its declared contract. With no argument, use the default branch (`origin/HEAD`), falling back to `main`, then `master`. Include staged, unstaged, and relevant untracked changes. If no base exists, review those local changes and state the limitation. Report the selected scope and base. If no public surface is in scope, say so and stop.

Read complete declarations, documentation, and real callers, including tests and samples. Inspect implementation only to establish exposed behaviour, ownership, errors, or cost. For stubbed bodies, use the documented contract. Missing samples or unavailable callers are evidence limitations, not defects by themselves.

Read the applicable pack before assessing scenarios:

| Surface | Pack |
| --- | --- |
| .NET / C# | [dotnet.md](references/dotnet.md) |
| Swift | [swift.md](references/swift.md) |
| Kotlin | [kotlin.md](references/kotlin.md) |
| React + TypeScript components and hooks | [react.md](references/react.md) |
| HTTP routes, payloads, and client mirrors | [http.md](references/http.md) |

Each pack defines visibility, contract expressions, idiom, and local rulings. For mixed platforms, review each surface and their boundary; use actual client code to assess the wire contract and merge duplicate findings. Without a pack, use these tests and the platform's primary guidelines, noting the missing pack.

Establish domain constraints, supported scenarios, and decisions likely to vary. Research unfamiliar domain rules and version-sensitive platform claims when they could change a verdict. Use relevant prior art, including other languages, to discover alternatives; precedent supplies neither requirements nor a defence. Stop researching when the questions affecting findings are resolved; disclose unresolved ones.

## Scenarios and finding threshold

Select scenarios covering distinct caller obligations, starting with observed callers. Label inferred scenarios and ground them in the surface's stated purpose. Use only stated scale and requirements. Write representative caller code; for each suspected shape problem, compare a materially different, simpler API on the same scenario. Preserve required semantics, failure behaviour, ownership, and scale in the alternative.

Name the obligations removed: types, calls, configuration, ordering, error handling, or cleanup. Counts can clarify a comparison but are not an aggregate depth score. Documentation is legitimate for domain semantics; avoidable hidden ordering or ambiguous states are design problems.

Report a finding only when the evidence identifies a caller consequence worth blocking or requiring a reply over, and a remedy addresses it. A tie between designs is not a finding. **Only bullets under a heading named Rulings are house policies exempt from this threshold.** Label policy findings and cite the ruling. All other tests and pack guidance are diagnostic signals, even where phrased categorically.

Challenge each claim against the declared contract, platform defaults, caller control, and unavailable implementation. If it needs an unstated requirement, report the uncertainty and what would resolve it. Missing documentation alone does not disprove normal platform behaviour; withdraw claims that depend on guessing what hidden code omits.

Missing capability needs a supported scenario that fails without it. Caller-created load alone does not justify a capacity knob; demonstrate an internal resource the caller cannot observe or bound. Hazards requiring casts, reflection, or deliberate misuse are outside this review.

## Shape tests

Apply relevant tests across the scoped surface. Each candidate needs the named evidence, not merely the test's vocabulary.

1. **Depth.** Compare caller obligations with complexity hidden. Show scenario code and what a simpler shape removes. Length or member count alone proves nothing.
2. **Information hiding.** Name the difficult or varying decision hidden, or the member leaking it and the caller changes a replacement would force. A process stage alone is not a secret.
3. **Common case first.** Identify setup, parameters, or types required for features the common scenario does not use. Prefer defaults and optional capability to a wider baseline.
4. **Errors defined out of existence.** Show a representable invalid state or avoidable failure and a type or total operation that removes it without changing required semantics. Count legal versus representable states where useful.
5. **Somewhat general.** Show scenarios one simpler primitive can serve, or unsupported capability and its maintenance cost. Absence of repository callers does not prove a published member unused.
6. **Each layer its own abstraction.** Show a forwarding member pair and what deletion would lose. Keep layers that hide policy or a changing dependency; matching signatures alone do not prove redundancy.
7. **Hypothetical seam.** Identify implementations, consumers, and what actually varies. Apply the seam ruling below; account for external consumers before calling a seam unused.
8. **Design it twice.** For each new abstraction, compare a materially different shape on its main scenario: function versus type, value versus stateful object, or data versus callbacks. Report only a demonstrated improvement.
9. **Unbraided.** Identify coupled concerns and the cost of separating them: results with call history, identity with mutable value, flags selecting behaviours, data tied to behaviour, or shared mutable ownership. Ordinary configure-then-call usage alone is not temporal coupling.
10. **Hard to name.** Attempt a precise one-sentence description. A vague name is a clue; establish the mixed responsibilities before classifying it as Shape.

For disputed design reasoning, consult the relevant section of [foundations.md](references/foundations.md).

## Contract and idiom

Check each applicable contract dimension using its expression in the target's pack:

- **Errors and absence.** Distinguishable failures where callers need different recovery; one channel per failure; normal absence represented explicitly. Prefer empty collections when no elements is the only meaning.
- **Ownership.** Who owns inputs and outputs, whether references are retained, and which mutations callers can observe or cause.
- **Lifetime and concurrency.** Cleanup, repeat cleanup, use after cleanup, concurrent or reentrant calls, and thread/context requirements.
- **Sync and async.** Match the work's execution model; identify blocking wrappers, cancellation obligations, and stale-result behaviour.
- **Compatibility.** Establish whether consumers ship together or independently. For shipped surfaces, state source, binary, wire, or behavioural breaks caused by both the change and its remedy.

Then assess platform idiom across the surviving surface. Explain the knowledge callers lose through inconsistency, or cite an explicit house ruling. Classify local naming or member conventions as Idiom unless separate evidence establishes a shape or contract problem. Suppress nits on members a larger remedy would remove.

## Rulings

- **Seams are not for testability.** A single-implementation abstraction introduced solely for test substitution is a policy finding. Test the concrete public surface or use an existing dependency abstraction. A plug-in contract is a product surface and needs a reference implementation and conformance tests.

## Calibration

- `Load(); Parse(); Commit();` with order-dependent failures supports Shape when one operation preserves the job and removes ordering. `new Options { Limit = 10 }` followed by a call does not establish that problem.
- A caller branching on message text to choose recovery supports Contract. Another library's richer errors alone do not justify adding error types.
- A public field can warrant an Idiom finding under the pack's conventions; it does not establish that the containing type needs redesign.

## Report

Lead with scope, finding count, and the worst consequence. Rank by demonstrated lifetime cost and compatibility impact; use Shape, Contract, Idiom as the tie-break order, not as severity. Merge findings with the same cause and remedy.

For each finding provide:

```
N. Unit.Member [Shape|Contract|Idiom; Policy if applicable]: claim
   Evidence: caller scenario or other test artifact, with file/line or proposal section
   Remedy: change and the obligation or failure it removes
   Breaking: affected consumers and compatibility kind (when shipped)
```

Finish with packs and sources consulted and material evidence limitations. When nothing clears the threshold, report "The surface holds: no findings" without nits or praise. Complete the review when each scoped surface has been assessed and each candidate is supported or rejected; distinguish incomplete coverage from a clean verdict.
