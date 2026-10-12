---
name: refine
description: Assess a ticket or spec before committing to build it — domain-language check, seam sketch, dependency-classified risk, and an estimate with named drivers.
disable-model-invocation: true
---

Pre-build assessment of a ticket or spec: surface what is still ambiguous, where the risk lives, and how big the work really is. The output is an assessment — not code, and not an implementation plan.

Uses the vocabulary of the **codebase-design** skill — module, interface, seam, adapter.

## Process

### 1. Pin the work item

Whatever the user pointed at — a tracker issue, a spec file, or pasted text. If they pointed at nothing, ask. Done when you can restate the requested change in one paragraph and the user's pointer resolves to actual content.

### 2. Check the domain language

Read the domain types (and their doc comments) in the affected area of the codebase — they are the project's glossary. Done when every noun and verb in the ticket either matches the code's vocabulary or carries a flag: *conflicts with an existing name*, or *too fuzzy to implement from*.

### 3. Sketch the seams

Identify which existing seams the work touches and whether it needs any new ones. Prefer existing seams, at the highest point possible. Classify every dependency behind those seams as *runs in tests* or *needs a port*, per the codebase-design skill's [DEEPENING.md](../codebase-design/DEEPENING.md). Done when each new seam is justified in one sentence and every dependency the change touches is classified.

### 4. Estimate

Size the work and name the drivers:

- New seams and dependencies that need a port drive uncertainty up — say which ones.
- Flags from step 2 mean elaboration is unfinished — the estimate must say so rather than absorb the ambiguity.
- If the seam sketch decomposes into independently shippable parts, say where the ticket should split.

Done when the estimate states a size, an uncertainty level, and the specific drivers of both.

### 5. Report

Deliver in your response: risk-ranked open questions, the estimate with its drivers, and any split recommendation. This is a read-only task — leave SESSION.md and the tracker untouched unless asked.
