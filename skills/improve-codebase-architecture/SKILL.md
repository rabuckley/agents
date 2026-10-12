---
name: improve-codebase-architecture
description: Scan a codebase for architectural friction, present refactor candidates as a visual HTML report, then grill through whichever one you pick.
disable-model-invocation: true
---

# Improve Codebase Architecture

Surface architectural friction and propose refactors that remove it.

This command is _informed_ by the project's domain model and built on a shared design vocabulary:

- Call the Skill tool with "codebase-design" for the architecture vocabulary and principles.
- `GLOSSARY.md` holds the domain language; ADRs record past decisions (see **ADR conflicts** below for when to reopen one).

## Process

### 1. Explore

**Scope before you scan.** Restructuring a module pays off by making future changes to it easier, so put extra weight on the parts of the codebase that change often. Decide *where* to look before you look:

- If the user named a direction (a module, a subsystem, a pain point), take it, and skip the inference below.
- Otherwise, use the commit history to find the codebase's hot spots, the files and areas that change most often, and let those paths pull your attention first. If the changes are scattered with no clear hot spot, widen the net.

Read the project's domain glossary (`GLOSSARY.md`) and any ADRs in the area you're touching first.

Then walk the codebase and note where its structure makes changes or bugs costlier than they need to be.

### 2. Present candidates as an HTML report

Keep only candidates whose benefit outweighs their cost. If none survive, say so and skip the report.

Write a self-contained HTML file to the OS temp directory so nothing lands in the repo. Resolve the temp dir from `$TMPDIR`, falling back to `/tmp` (or `%TEMP%` on Windows), and write to `<tmpdir>/architecture-review-<timestamp>.html` so each run gets a fresh file. Open it for the user (`xdg-open <path>` on Linux, `open <path>` on macOS, `start <path>` on Windows) and tell them the absolute path.

The report uses **Tailwind via CDN** for layout and styling, and **Mermaid via CDN** for diagrams where a graph/flow/sequence reliably communicates the structure. For each candidate, render a card with:

- **Files**: which files/modules are involved
- **Problem**: why the current architecture is causing friction, with the evidence you saw
- **Solution**: plain English description of what would change
- **Benefits**: what gets easier
- **Cost**: migration effort, blast radius, tests that would be rewritten
- **Before / After diagram**: side-by-side, where a picture clarifies the change
- **Recommendation strength**: one of `Strong`, `Worth exploring`, `Speculative`, rendered as a badge, weighing benefit against cost

If one candidate clearly leads, end the report with a **Top recommendation** section: which you'd tackle first and why.

**ADR conflicts**: if a candidate contradicts an existing ADR, only surface it when the friction is real enough to warrant revisiting the ADR. Mark it clearly in the card (e.g. a warning callout: _"contradicts ADR-0007, but worth reopening because…"_). Don't list every theoretical refactor an ADR forbids.

See [HTML-REPORT.md](HTML-REPORT.md) for the HTML scaffold and card layout.

Leave interface design for the grilling loop: sketch only enough of the target shape to make the cost credible. After the file is written, ask the user: "Which of these would you like to explore?"

### 3. Grilling loop

Once the user picks a candidate that leaves design choices open, call the Skill tool with "grilling" to walk the decision tree with them.

As decisions crystallize:

- **User rejects the candidate with a load-bearing reason?** Offer an ADR, framed as: _"Want me to record this as an ADR so future architecture reviews don't re-suggest it?"_ Only offer when the reason would actually be needed by a future explorer to avoid re-suggesting the same thing; skip ephemeral reasons ("not worth it right now") and self-evident ones. Nothing was built, so this overrides domain-modeling's hard-to-reverse test.
- **Want to explore alternative interfaces?** Call the Skill tool with "codebase-design" and use its design-it-twice parallel sub-agent pattern.
