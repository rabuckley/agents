---
name: codebase-design
description: Shared vocabulary for finding the right module abstractions and making them deep. Use when the user wants to design or improve a module's interface, find deepening opportunities, decide where a seam goes, make code more testable or AI-navigable, or when another skill needs the deep-module vocabulary.
---

# Codebase Design

Find the right abstraction for a module first. Depth, locality, and testability are how you check it, not targets in themselves.

## Glossary

Use these terms for design reasoning. Where the codebase or domain already names a thing, use its name.

**Module**: anything with an interface and an implementation. Deliberately scale-agnostic: a function, class, package, or tier-spanning slice.

**Interface**: everything a caller must know to use the module correctly: the type signature, but also invariants, ordering constraints, error modes, required configuration, and performance characteristics.

**Implementation**: what's inside a module, its body of code. Distinct from **Adapter**: a thing can be a small adapter with a large implementation (a Postgres repo) or a large adapter with a small implementation (an in-memory fake). Reach for "adapter" when the seam is the topic; "implementation" otherwise.

**Depth**: leverage at the interface. The amount of behaviour a caller (or test) can exercise per unit of interface they have to learn. A module is **deep** when a large amount of behaviour sits behind a small interface, **shallow** when the interface is nearly as complex as the implementation. Not Ousterhout's ratio of implementation lines to interface lines, which rewards padding the implementation.

**Seam** _(Michael Feathers)_: a place where you can alter behaviour without editing in that place; the *location* at which a module's interface lives. Where to put the seam is its own design decision, distinct from what goes behind it.

**Adapter**: a concrete thing that satisfies an interface at a seam. Describes *role* (what slot it fills), not substance (what's inside).

**Leverage**: what callers get from depth. More capability per unit of interface they learn. One implementation pays back across N call sites and M tests.

**Locality**: what maintainers get from depth. Change, bugs, knowledge, and verification concentrate in one place rather than spreading across callers. Fix once, fixed everywhere.

## Finding the abstraction

Settle what the module *is* before sizing its interface:

- **Concept.** State in one sentence what the module stands for. A sentence that needs "and" may describe two modules.
- **Secret** _(Parnas)_: the design decision the module hides, if it has one, chosen because it is likely to change or hard to get right. Callers depend on the concept, never on the secret.
- **Caller decisions.** Expose what callers genuinely need to decide; decide everything else for them.

Interface size is the tie-breaker between abstractions that pass these, not the goal.

## Principles

- **Depth is a property of the interface, not the implementation.** A deep module can be internally composed of small parts; they just aren't part of the interface. A module can have **internal seams** (private to its implementation, used by its own tests) as well as the **external seam** at its interface.
- **The deletion test.** Imagine deleting the module. If complexity vanishes, it was a pass-through. If complexity reappears in its callers, it was earning its keep. An invariant it enforces or a secret it keeps counts as complexity that reappears.
- **The interface is the test surface.** Callers and tests cross the same seam. Wanting to test past it signals the module may be the wrong shape; the exception is an internal piece whose edge cases are cheaper to reach directly, tested through an internal seam.
- **Proposed interfaces pass the same checks.** Before recommending a new method or type, state its concept in one sentence and name which caller decisions it absorbs. If the name has to describe two rules, the fix is moving knowledge across a layer rather than consolidating it.
- **One adapter means a hypothetical seam. Two adapters means a real one.** This governs substitution points such as injected ports, not module boundaries: hiding a secret or pointing a dependency inward needs no second adapter. A test adapter counts only when tests can't reach the behaviour through the real dependency (see [DEEPENING.md](DEEPENING.md)).

## Going deeper

- **Deepening a cluster given its dependencies**, see [DEEPENING.md](DEEPENING.md): testing across dependencies, and replace-don't-layer.
- **Exploring alternative interfaces**, see [DESIGN-IT-TWICE.md](DESIGN-IT-TWICE.md): spin up parallel sub-agents to design the interface several genuinely different ways, then compare on depth, locality, and seam placement.
