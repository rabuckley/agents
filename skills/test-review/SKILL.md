---
name: test-review
description: >
  Review tests against one yardstick — a test earns its place when it fails if
  the behaviour it claims changes and survives refactors that keep the
  interface it tests intact. Flags
  implementation coupling, interaction verification, over- and under-assertion,
  non-determinism, and redundancy, with a deletion recommendation for every
  redundant test. Report only — no code changes. Use when the user wants tests
  reviewed or audited, asks whether tests are valuable, brittle, or redundant,
  or wants a test suite cleaned up. Do NOT use for writing tests (tdd),
  reviewing production code (feature-code-review), or module structure
  (braid-review).
argument-hint: "[path | git-ref | all]"
disallowed-tools: Edit Write NotebookEdit
---

# Test review

The yardstick, applied to every test in scope:

**A test earns its place when it fails if the behaviour it claims changes, and
survives refactors that keep the interface it tests intact.**

Both halves bind. Over-specification (interaction verification, asserted
incidentals) fails the refactor half; under-assertion (not-null, doesn't-throw)
fails the behaviour half; non-determinism decouples the result from the code
and fails both.

Observable behaviour is what the unit promises its callers. That includes calls
that are themselves the output — an email sent, an event published, a command
to an external system — and guarantees about _how_ it behaves: query or fetch
counts, exactly-once effects, constant-time comparison, a secret never logged.
Asserting such a promise is asserting behaviour when the promise is evident —
for example documented, stated as the test's rationale, pinned after an
incident, or a security property the domain makes obvious.

## Scope

- No argument or a git ref → this branch's changes since the ref (default: the
  main branch), committed or not: the tests changed, the tests that exercise
  changed production code, and that code's gaps under criterion 5.
- A path → the test files at or under it.
- `all` → the whole suite; when it exceeds what one context can hold, fan out
  sub-agents, each given this skill and a slice of the code under test together
  with every test that exercises it.

Confirm the scope is non-empty before reviewing; if it is empty, say so and
stop. For every test in scope, read the code under test — the contract
judgement, the subsumption argument, and gap-hunting all depend on it.

## Evidence bar

A suspicion becomes a finding only when it carries the specific artifact its
criterion names — the refactor that would falsely break the test, the wrong
result that would pass, the subsuming test, the concrete failure scenario, the
stable route. A suspicion whose artifact you cannot produce is dropped, silently.

## Criteria

**1. No unstable seams.** Flag tests coupled to a seam likely to churn when
the behaviour they pin is cheaply reachable through a stable interface. The
typical case is a private helper with one caller: however clean its signature,
its shape serves that caller and can be inlined or reshaped at will. Visibility
alone is not the test: an internal module other code is built against is an
interface too, and same-package or `mod tests` placement follows language
convention. Evidence: the stable route that cheaply exercises the same
behaviour, and the reshaping of the seam that would falsely fail the test.

**2. State over interaction.** Verifying a call that is not itself part of the
contract — a query for data — is flagged: it breaks on behaviour-preserving
refactors. Assert resulting state or output instead. Evidence: the
behaviour-preserving refactor (inline, reorder, batch, cache) that would
falsely fail the test, and the replacement assertion that catches what the
verification caught.

**3. Assert what the test claims.** Two directions, same criterion:

- Under-assertion — `!= null`, "doesn't throw", asserting only that something
  came back. Evidence: a wrong result, for the behaviour the test names, that
  the assertion would pass.
- Over-assertion — error-message prose, ordering the contract doesn't
  promise, exact match where exact form isn't the contract.
  Evidence: the contract-preserving change that would fail the test.

Exact-match assertions are correct precisely when exact form _is_ the contract
(wire formats, rendered output, serialisation, and text that is itself the
product — CLI output, validation messages shown to users, API error bodies).
An exception message read only by developers is not, though the offending
key or value it carries can be.

**4. Deletion with proof.** A redundant test gets a first-class deletion
recommendation. Evidence: the subsuming test, named, plus why the suite loses
nothing — no failure only this test catches, and no failure this test would
point to its cause more directly or catch substantially sooner. Subsumption
is argued from inputs and assertions — "looks trivial" does not qualify.

**5. High-value gaps only.** A missing test is a finding only with a concrete,
consequential failure scenario attached: the input or state, the wrong result,
the consequence. "No test for X" without that story is coverage-completeness,
which is outside this review.

**6. Legible in isolation.** The test body shows why each expected value is
expected — directly, through a well-named constant, or in a fixture meant to be
read beside it (a golden file); helpers hide only irrelevant noise — DAMP, not
DRY. Flag mystery guests (an assertion whose value's relevance is hidden
elsewhere) and copy-paste walls (duplicated setup burying the one value that
varies). Evidence: the assertion and its off-screen value, or the duplicated
block and its single point of variation.

**7. Determinism and isolation.** Flag tests whose outcome depends on the real
clock, randomness whose failing case cannot be reproduced, sleeps standing in
for synchronisation, test order, or shared mutable state across tests.
Evidence: the condition under which the outcome changes (midnight, a DST
change, the host timezone, a slow machine, a different order) and its
substitution — injected clock, fixed or reported seed, callback or polling,
fresh fixture, fake.

**8. Property, table, snapshot upgrades.** Prefer the idioms the codebase
already uses. Recommend a property test when a rule is a law (round-trip,
invariant, idempotence, oracle) whose edge cases the examples don't reach,
keeping the examples that pin a boundary. Recommend restating roughly five or
more examples of one rule as a table when they are enumerable cases through
one body — at that size, separate tests obscure the rule; a pair or trio of
well-named examples stays as it is. Recommend snapshot/approval tests where the problem
suits them — large structured output whose exact form is the contract — as a
deliberate tool, never the default. Evidence: the shared rule, stated as
the property or the table's columns.

**9. Failure diagnostics.** Judge the test by its failure: its name, in the
codebase's naming convention, and its output should identify the case and the
delta without a debugger. Flag only names that fail to distinguish the case
and assertions whose failure output hides the delta.
Evidence: the current name or output beside what a failure would actually tell
the reader.

## Report

Output is a report only — leave the working tree as you found it; fixes
happen on a separate, explicit request.

Rank findings by the cost the suite carries, worst first. Each finding:

```
N. file:line — [criterion] one-line claim
   Evidence: <the criterion's artifact>
   Remedy: <the change — deletion is first-class>
```

Close with the count and the single worst finding. "The suite holds — no
findings" is a valid verdict; deliver it without padding nits or praise.
