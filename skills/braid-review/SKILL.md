---
name: braid-review
description: Structural design review through the complecting lens — names what the code braids together, judges lifetime cost rather than how the code reads today, and leans toward deletion. Reviews the working diff by default, or a git ref / path passed as argument. Pick this over code-review (correctness bugs) or simplify (applies mechanical cleanups) when the question is whether the design itself is entangled.
disable-model-invocation: true
argument-hint: "[git-ref | path]"
---

# Braid review

Judge the **artifact** — what this code will cost to understand and change over
its life — never the **construct** — how it reads and types today. Your taste
is trained on the most common code, which makes ceremony feel right and
unfamiliar simplicity feel wrong; comfort is not evidence. "Simple" here is
Hickey's objective sense: one braid, inspectable by looking — not "familiar",
not "few lines", not "passes the tests".

These are standing criteria for the whole review, not steps: apply them to
every suspicion, in any order, for as long as the review runs.

## Target

`$ARGUMENTS`

When no target is given, run `git fetch origin` and review the working diff
against the default branch (`origin/HEAD`, else `origin/main`) plus `git diff
HEAD`. A git ref or range diffs against that; a path reviews those files as
they stand. Start from `git diff --stat <base>...HEAD`, then read the diff per
file. Always read the enclosing function or module of each hunk — braids live
in structure the hunk only touches.

## The tests

A suspicion becomes a finding only by surviving every applicable test. One
that fails is withdrawn silently — near-misses are not reported, hedged, or
listed as "minor observations".

1. **Name the braid.** State the two things this code braids together, and
   what you would have to accept to get one without the other. Void if you
   cannot name both strands, or cannot say what separating them costs.
   "This braids retry policy with transport: you cannot test the policy
   without a socket" is a finding; "this could be more modular" is not.
2. **Incidental, not inherent.** Complexity the problem genuinely demands is
   not a finding — incidental means *we chose the construct that caused it*.
   Void if the messiness maps one-to-one onto real demands of the problem.
3. **Artifact, not construct.** Is the objection about lifetime cost, or about
   how the code reads today? Void if construct-only. This test vetoes: style,
   naming taste, line counts, and "I'd have written it differently" all die
   here, whatever other test they passed.
4. **Depth.** Compare what a caller must know to use this against what it
   spares them from knowing. Roughly equal means shallow: the fix is to
   collapse it, not to document it. For the vocabulary — deep/shallow,
   seam, leverage — use the `codebase-design` skill rather than improvising.
5. **Carrying cost.** Every structure levies a recurring toll on readers and
   changes. Name this one's toll and the threshold that would justify it.
   Void once the threshold is genuinely cleared — a seam is paid for by the
   second adapter that crosses it, a parameter by the function that actually
   reads it. A layer that only relays is never paid for by what happens at
   its far end.
6. **What breaks if this is deleted?** Ask it of every abstraction the code
   adds. If the answer is "nothing — callers would call the target directly",
   the finding is the deletion. A structural review that proposes only
   additions has reviewed nothing; removal is the default fix, wrappers the
   exception.

## Rulings

Where the authorities disagree, these verdicts hold — do not hedge or
re-litigate them in a report.

- **Length is not a finding.** Never flag a function for being long; flag
  shallowness. A one-line pass-through is a defect; a 60-line function with
  one job is not (length is a preference, not a hard limit — never a finding
  on its own). Intention-diverging-from-implementation justifies a
  split only inside an already-deep module, never as a mandate to fragment
  one.
- **Plain data plus pure functions is a coherent design, not anemia.** The
  anemic-domain-model smell fires only where the code has already paid for a
  domain model — ORM, rich-object mapping, OO ceremony — and gets none of the
  benefit. Data-as-data with functions over it pays no such cost; do not
  prescribe moving behaviour onto the data types.
- **Tests never answer a structural finding.** Every bug found in the field
  passed the type checker and all the tests. "It's covered by tests" resolves
  nothing about a braid; don't accept it, don't offer it.
- **A diff that mixes behaviour change with restructuring is not a
  refactoring.** Flag it and ask for the split — it is the most mechanically
  checkable structural defect a diff can have.
- **Comments follow the contract rule.** Interface documentation that leaks
  implementation detail is a structural finding (the interface stops being
  smaller than the implementation). Do not push for more implementation-body
  comments; that is not this review's axis.
- **YAGNI and generality are different axes.** Flag speculative *capability*
  (features nothing exercises). Do not flag a general *interface* that is
  simpler than the special-purpose one it replaces.

## Report

Rank findings by lifetime cost, worst first. Each finding states, in order:
where; the braid (X complected with Y, or the test it failed); the recurring
cost in concrete terms; the fix — a deletion where the removal test says so.
A reader must be able to *dispute* every finding: if there is nothing they
could check to prove it wrong, it isn't one.

**"No structural findings" is a correct and common verdict.** The severity
bar: would you block a merge over it, or at least demand a reply before
approving? Below that bar, say nothing — a review that always finds something
is noise and will be turned off. Do not pad a clean verdict with nits,
observations, or praise-then-caveats.

## Gotchas

Corrections to the mistakes this reviewer makes by default:

- You will want to split the 60-line parser into `parseHeader` /
  `parseBody` helpers and call it "separating concerns". That fragments a
  deep function into conjoined shallow ones and moves the reader's problem
  from scrolling to chasing. Depth is more important than length.
- You will want to add — a wrapper, an interface, a test, a comment — because
  additions feel safe. The structural fix is usually a removal.
- When a value is only relayed through signatures to a distant reader, you
  will judge the fix "more abstraction" and let it pass. It's the opposite:
  resolving the value where it is read *deletes* a parameter from every
  function that never read it. (A parameter a function actually reads is not
  a relay — leave it alone.)
- An interface with one implementation, a factory with one product, a mapper
  whose body restates its type signature: the finding is deletion, and
  "might need it later" does not survive the carrying-cost test.
- "Consider separating concerns", "this could be more modular", "high
  coupling here" — book-report prose. No named braid, no finding.
- When nothing clears the severity bar, the temptation is to fill the report
  with construct nits at uniform severity. Return the clean verdict instead.

When a finding turns on interface or module shape and needs sharper language
than "shallow", read `references/red-flags.md` — Ousterhout's red flags,
verbatim.
