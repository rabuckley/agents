---
name: test-review
description: >
  Reviews tests and reports which earn their place: which are brittle,
  vacuous, flaky, redundant or copy the code under test, which over-use mocks
  or assertions, and which high-value tests are missing. Report only, no code
  changes. Use when the user wants tests reviewed or audited, asks whether
  tests are valuable, brittle or redundant, or wants a test suite cleaned up.
  Do NOT use for writing tests (tdd), reviewing production code, or module
  structure (braid-review).
argument-hint: "[path | git-ref | all]"
disallowed-tools: Edit Write NotebookEdit
---

# Test review

A test earns its place when the confidence it buys is worth more than it costs.
Judge each test against five principles. They trade off: a test may give one up
only for something worth more, such as fidelity or a failure that points
straight at its cause.

1. **It fails when the behaviour it claims breaks.** Find a mutant, a plausible
   wrong implementation, that the test lets through. Usual causes: not-null or
   doesn't-throw assertions, inputs equal to a default or to each other, a
   skipped test, a fake that diverges from the real collaborator on the path
   under test.
2. **It survives changes that keep that behaviour.** Assert what callers
   observe. That includes calls that are the output (an email sent, a command
   to an external system) and evident promises about how the code behaves
   (documented, pinned after an incident, or a security property). The rest is
   structure: a helper shaped by its one caller, a query the code happens to
   make, an order or message nobody promised. A module other code builds
   against is an interface whatever its visibility. Exact form is the contract
   only when the text or bytes are the product, not a message only developers
   read.
3. **Its expected values come from outside the code under test**: a rule across
   many cases, an outcome a caller observes, or a previous release. A value
   copied from a declaration in the code under test passes whether that code is
   right or wrong. A snapshot update counts only if someone reviews the diff.
4. **It gives the same result every run**, whatever the clock, machine speed,
   test order or state left by other tests. Randomness is fine when a failing
   case can be reproduced.
5. **Its reason to exist and its reason for failing are clear.** The body shows
   why each expected value is expected; a failure names the case and the
   difference. Restate roughly five or more examples of one rule as a table,
   and add a property test when a law's edge cases go unreached. Prefer the
   codebase's existing idioms.

The same trade-off sizes the suite. Delete a test another test subsumes, unless
it catches a failure sooner or points to its cause more directly. Report a
missing test only when it is high-value.

## Process

1. **Scope.** No argument or a git ref: tests changed since the ref (default:
   the main branch) plus tests that exercise changed code. A path: the tests
   under it. `all`: the whole suite, fanned out to sub-agents by code under
   test if one context cannot hold it. If the scope is empty, say so and stop.
2. **Read the code under test** for every test in scope.
3. **Judge each test** against the principles.
4. **Check each finding's evidence** before reporting. Drop, silently, any
   finding that lacks its evidence:
   - Principle 1: the mutant that passes.
   - Principle 2: the behaviour-preserving change that breaks the test.
   - Principle 3: the declaration the expected value copies.
   - Principle 4: the condition that changes the result (midnight, timezone,
     slow machine, test order).
   - Principle 5: the hidden value, the unhelpful failure output, or the shared rule.
   - Deletion: the subsuming test and why the suite loses nothing.
   - Missing test: the input, the wrong result and its consequence.
5. **Report**, leaving the working tree as you found it.

## Report

Lead with the count and the worst finding, then rank by the cost each finding
imposes on the suite:

```
N. file:line — [principle] one-line claim
   Evidence: <evidence>
   Remedy: <change; deletion is first-class>
```

For example:

```
1. orders.test.ts:42 — [1] total is never asserted
   Evidence: returning an empty Order passes; only the spy calls are checked.
   Remedy: assert order.total equals 2 × 9.99.
```

"The suite holds, no findings" is a valid verdict. Don't pad it.
