---
name: architecture
description: Make or revisit an architectural choice by testing its premises, responsibility ownership and strongest alternatives against evidence.
disable-model-invocation: true
argument-hint: "[question | ADR | plan | path]"
---

# Architectural choices

Recommend a design from the problem and the evidence. An existing design is a candidate, and its rationale is a hypothesis to test.

## Frame the decision

Use the supplied question or artifacts to state the choice, the outcome it must support, and the constraints that could rule out an option. If the choice is missing, ask for it before exploring the codebase.

Within that choice, reconsider everything in the ADR and plan, including accepted decisions, settled decisions and rejected alternatives. Invoking this skill authorizes reconsidering those decisions. Their status records history rather than proving correctness. Preserve constraints the user explicitly places on this task. Distinguish a binding requirement from a document's assumption about how to satisfy it.

Keep the work to analysis and a recommendation unless the user also requests implementation or document edits.

## Test the premises first

Identify the assumptions on which the choice turns, especially claims about consumers, reuse, independence and the problem's existence. For each decisive assumption, seek evidence that could disprove it before inspecting the proposed design's mechanics.

Read the relevant implementation and its actual callers, configuration, schemas and prompts as applicable. Cite file locations or supplied evidence. Separate observed behavior, stated requirements and guesses about future needs. A document repeating another document's rationale is one claim, not independent evidence. When an option depends on an unknown, run the smallest investigation that could decide it; if that is out of reach, make the recommendation conditional on it.

Repo convention docs and precedent elsewhere in the codebase are premises too. Each records a past choice made for its own context. Test them like any rationale, and explain what each option enforces and costs. When the choice turns on how a library, framework or platform behaves, read its documentation and, where informative, how comparable projects draw the same boundary. Fetch those sources yourself. A hedge such as "check the docs for your version" leaves the step unfinished.

If the evidence contradicts a rationale, say which premise fails and how that changes the choice. Preserve the contradiction in the recommendation; reinterpret the rationale only if new evidence supports that interpretation. Apply the same scrutiny to your preferred alternative.

## Place responsibilities by purpose

For each contested responsibility, establish the following:

- Which user outcome or component's job it serves.
- Who uses it and which parts of the contract each consumer needs.
- Whose requirements determine its policy, vocabulary, inputs and outputs.
- Which independent invariant, volatile decision, transaction or failure behavior warrants separating it.

Place the responsibility with the component whose purpose it serves and whose needs change it. Current location, generic names and the kind of data it operates on do not establish ownership.

A generic-looking capability with one consumer belongs to that consumer by default. A hypothetical second consumer does not justify a shared service. A separate boundary can still hide a volatile decision or enforce an independent invariant. Name that reason and the cost it removes. Even several consumers may need different policies rather than a shared abstraction.

For example, an image description used only by Language, with its content shaped by Language's lexicon rules, is evidence for Language ownership. Calling it "a fact about the image" does not answer whose needs define it. An image decoder hiding a vendor format can warrant a separate module with one consumer, because the format is its own reason to change.

## Compare credible alternatives

Include the current or proposed design, the simplest viable alternative, and any materially different contender supported by the problem. Give the strongest case for each against the actual requirements. Keeping the current design or building nothing can be the right choice.

Compare options on the constraints that decide this case: ownership and change locality, knowledge crossing boundaries, operational or consistency costs, and migration cost where relevant. Distinguish the best design from the cost of reaching it. Assess migration cost by the decisions and invariants it disturbs, such as dependency cycles, contract changes and behaviour risk. Mechanical edits such as import rewrites are cheap however many files they touch. Avoid scores that hide which trade-off determines the recommendation.

For each serious contender, state its strongest objection and what evidence would make it the preferred choice. Prefer the smallest design that meets demonstrated needs and is cheap to change. Recommend a larger boundary when its concrete benefit justifies its cost.

## Deliver a decision

Lead with the recommendation and the decisive evidence. Write for a reader who has not seen the files you read, and define any name you coined during the analysis. Keep the explanation proportional to the choice, covering:

- The problem and binding constraints.
- Which existing premises survive or fail, with evidence.
- Where responsibilities belong and whose needs change them.
- The strongest alternatives and why each loses under current evidence.
- The main trade-off, remaining uncertainty and what would reverse the decision.
