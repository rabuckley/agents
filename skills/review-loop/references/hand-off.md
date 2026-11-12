# Handing off

The user saw none of the subagents' work, so write each item to stand on its own: the claim in plain words beside any ledger id, and the evidence a reader could check. Open with the purpose the branch was reviewed against, and say when it was inferred, even if the user confirmed it. Then report:

1. **Needs your decision**: each `decision` and `unverified` item, and the owner answers, with the options, the evidence and your recommendation. This is the main output.

   When the root-cause fix addresses the branch's purpose, recommend doing it in this branch. Recommend a follow-up only for work outside the purpose. Never recommend a guard as the interim fix for an invariant a reshape would make true by construction: guards written now are the reason the reshape never happens.

   A reshape the user chooses goes through [`/design-first`](../../design-first/SKILL.md) with them before anyone implements it, with the owner answer and the inventory as its premise. The design's first question is which module owns the concept. Brief an implementer only after the user approves the note.
2. **Fixed**: one line per root-cause group, with its commit.
3. **Rejected**: one line each, with the evidence, so nobody argues them again. Put the ones rejected on a trace alone first: those are the model overruling itself.
4. **Follow-ups**: pre-existing problems, left unfixed.
5. **Dropped**: a count, with the reasons grouped. The lines are in the ledger.
6. **Checks**: what ran and what passed. Name anything that couldn't run.
7. **Loop**: the rounds run, why the loop stopped, and whether the tip was reviewed clean. If it wasn't, name the commits no reviewer has read and the contested code no detail lens read. If the loop didn't converge, say what that points at.
8. **Upstream**: review is the latest and most expensive place to catch a defect. For each confirmed finding and decision, ask for the earliest point at which its class could have been caught or made impossible, such as a lint or analyzer rule, a type, a line in the repo's `AGENTS.md`, or a `/design-first` that was skipped. Name only the findings with a concrete answer, and draft the rule or the line. Propose these; don't apply them. An empty list is a valid result.

Then offer to run `/restructure-commits` once, before the PR.
