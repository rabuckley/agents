---
name: design-review
description: Review whether a change's module boundaries follow its reasons to change. Checks that each module stands for a real concept, that each rule lives with its owner, and that boundaries fall where the reason to change does. Reviews the branch diff by default, or a git ref or path passed as argument.
disable-model-invocation: true
argument-hint: "[git-ref | path]"
---

# Design review

A design is right when what changes together lives together. Each module should own one **reason to change**, and the rest of the system should depend on the module's concept, never on that reason (Parnas's secret).

In business code, the domain is the best evidence of a reason to change: concepts, their rules, and the places where the language shifts. Three other kinds of evidence count too:

- a volatile technical decision, such as a vendor API, a wire format or a storage engine;
- a consistency or failure boundary: what must commit atomically, or fail independently;
- the git history of what has actually changed together.

When these disagree with the domain, follow the reason to change.

The findings worth raising are where concept, ownership and boundary meet. A concept nobody has named leaves its rule without an obvious home. A rule with no home gets copied across a boundary. Report the cause rather than each symptom.

## Target

`$ARGUMENTS`

With no target, run `git fetch origin` and review the branch against the default branch (`origin/HEAD`, else `origin/main`), plus `git diff HEAD`. A git ref diffs against that ref. A path reviews those files as they stand.

Start from `git diff --stat <base>...HEAD`, then read the diff file by file. Ownership can't be judged from a hunk alone, so read each touched module in full, along with its callers.

## Learn the domain first

Build the domain's vocabulary before judging anything. Draw it from, in this order:

1. `GLOSSARY.md` and the ADRs;
2. the spec or design note, or the branch's purpose when there is neither;
3. the domain types and their doc comments;
4. the words the issue and the commit messages use.

Done when you can name every concept the change touches in the domain's words, and say which module owns each one. Where the domain is still unsettled, prefer coarse modules and tolerate duplication. A boundary drawn from a guess locks the guess in.

## The questions

Ask these of every module, type and rule the change adds or reshapes. Use the [codebase-design](../codebase-design/SKILL.md) vocabulary (module, interface, seam, depth, secret) and its deletion test.

1. **Concept.** Name the real thing this code stands for, in one sentence, in the domain's words. It can be a thing or a process: a workflow that spans concepts is a concept too, and its ordering and failure rules need an owner. It's a finding when:
   - a technical name sits on code that makes domain decisions;
   - one name covers two concepts, so the sentence needs "and";
   - one concept goes by two names;
   - a primitive or a set of flags stands in for a concept that has a rule, such as a pair of booleans encoding a lifecycle.
2. **Ownership.** For every rule, invariant and decision in the change, name the concept that owns it and check that it lives there. It's a finding when:
   - callers re-derive the rule;
   - the rule is copied across layers;
   - a domain rule lives in an adapter, a handler or infrastructure;
   - a caller makes a decision the callee should make.

   Two pieces of code are copies of one rule only if they change for the same reason. Show the shared business rule or the history of them changing together. Code that only looks alike stays separate. Parsing untrusted input into a domain type is the edge's own rule, not a copy of the domain's.
3. **Boundary.** Check that each boundary falls where the reason to change does. It's a finding when:
   - an external model leaks into the core instead of being translated at the edge;
   - a volatile technical decision is spread across domain modules instead of hidden in one;
   - two contexts share a type that means different things to each;
   - one concept is split, so every change to it edits both sides;
   - a seam has a single adapter and hides no secret.

   A boundary that cuts through a transaction, or turns one set-based query into per-row calls, costs more than it buys.
4. **Invariant.** Ask whether a different representation would make the rule true by construction: one source of truth, a type that can't hold the illegal state, or a derived value instead of an authored one. If it would, the guards, tests and checks that protect the rule are symptoms. The finding is the reshape, together with what it retires.
5. **Premise.** For each mechanism added to prevent a condition (a guard, retry, budget, filter, lock or index), show the caller chain or the data that produces the condition. If the condition can't happen, the finding is to delete the mechanism.

## Rulings

- **Prescribe where knowledge goes, not patterns.** Repositories, aggregates, domain events, services and layers are means, never findings. A domain that is CRUD stays CRUD.
- **The codebase's consistent vocabulary beats textbook terms.** Flag inconsistency, not unfamiliarity.
- **The default fix moves or deletes.** A new module or type earns its place only by owning a rule that currently has no home, or has several.

## Report

Rank findings by lifetime cost, worst first. Merge findings that share a root cause into one. For example, payload checks in three places and a classifier that re-derives an error code may both trace to one missing owner for "payment acknowledgement". That's one finding, about the missing owner.

```
N. where — [concept | ownership | boundary | invariant | premise] one-line claim
   Concept: the thing at stake, in the domain's words
   Now:     where its knowledge lives, every copy with file:line
   Belongs: the owning module, and the reason to change it owns
   Retires: what the fix deletes: copies, guards, tests, parameters
```

A reader must be able to dispute every finding by checking something.

**"The design holds" is a correct and common verdict.** The bar for a finding is that you'd block the merge over it, or at least demand a reply. Return the clean verdict without padding.

## Gotchas

These correct the mistakes this reviewer makes by default:

- **You'll reach for a new type to hold a homeless rule.** First check whether an existing concept already owns it.
- **You'll name findings after patterns**, like "this should be an aggregate". Name the concept and the rule instead. The pattern is the implementer's choice.
- **You'll accept a technical name because it accurately describes what the code does**, like `PaymentMessageClassifier`. Ask what domain decision it makes, and whose decision that is.
- **You'll see only the nouns.** A process spread across the entities it touches has no owner, even when every entity looks tidy.
- **You'll treat a fix that adds a guard as the fix.** Ask whether the guard compensates for a rule that has no single owner.
