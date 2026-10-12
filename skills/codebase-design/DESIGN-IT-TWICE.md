# Design It Twice

When the user wants to explore alternative interfaces for a chosen deepening candidate, use this parallel sub-agent pattern. Based on "Design It Twice" (Ousterhout): your first idea is unlikely to be the best.

Uses the vocabulary in [SKILL.md](SKILL.md): **module**, **interface**, **seam**, **adapter**, **leverage**.

## Process

### 1. Frame the problem space

Before spawning sub-agents, write a user-facing explanation of the problem space for the chosen candidate:

- The constraints any new interface would need to satisfy
- The dependencies it would rely on, and which can run in tests (see [DEEPENING.md](DEEPENING.md))
- A rough illustrative code sketch to ground the constraints, not a proposal, just a way to make the constraints concrete

Show this to the user.

### 2. Spawn sub-agents

Spawn sub-agents in parallel, each producing a different interface for the deepened module. Separate agents keep each design from anchoring on the others.

Prompt each sub-agent with a separate technical brief (file paths, coupling details, which dependencies need a port per [DEEPENING.md](DEEPENING.md), what sits behind the seam). The brief is independent of the user-facing problem-space explanation in Step 1. Give each agent a different design constraint, chosen to pull along this candidate's real tensions. For example:

- Agent 1: "Minimise the interface: maximise leverage per entry point."
- Agent 2: "Optimise for the most common caller: make the default case trivial."
- Agent 3: "Re-cut the concept: draw the module around a different abstraction than the current code does."

Include the [SKILL.md](SKILL.md) vocabulary and the project's GLOSSARY.md, if one exists, in the brief so each sub-agent names things consistently with the architecture language and the project's domain language.

Each sub-agent outputs:

1. Interface (types, methods, params, plus invariants, ordering, error modes)
2. Usage example showing how callers use it
3. What the implementation hides behind the seam
4. Dependency strategy and adapters (see [DEEPENING.md](DEEPENING.md))
5. Trade-offs: where leverage is high, where it's thin

### 3. Present and compare

Present the designs, then compare them by the **concept** each design draws, **depth** (leverage at the interface), **locality** (where change concentrates), and **seam placement**.

After comparing, give your own recommendation: which design you think is strongest and why. If elements from different designs would combine well, propose a hybrid. Be opinionated: the user wants a strong read, not a menu.
