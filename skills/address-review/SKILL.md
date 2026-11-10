---
name: address-review
description: Addresses existing code-review feedback on a PR or a pasted review. Use when asked to implement or respond to review comments.
disable-model-invocation: true
---

## Find the feedback

For GitHub, collect unresolved review threads, review bodies, and PR comments that carry feedback. Use GraphQL `reviewThreads` and `isResolved` to distinguish resolved threads; `gh pr view --comments` alone is insufficient. Paginate threads and their comments, as well as reviews and PR comments. Check outdated comments against the current code rather than assuming they are addressed.

Report in the conversation. Reply to or resolve GitHub threads only when the user asks.

## Address the feedback

Treat each review comment as a claim, including its priority and scope advice: verify it against the current code and intended behaviour before acting on it.

Before editing, group the verified findings by root cause, across the reviewer's priorities. A must-fix symptom and a deferred design finding often share one cause, and fixing the symptom first commits you to the local patch.

Verify a finding's diagnosis and its proposed fix separately; a correct diagnosis often comes with a wrong prescription. Before adopting a reviewer's suggested interface or name, check that it describes one concept and doesn't pull a caller's rule into the callee.

For each valid finding, look for a better fix than the literal one requested before editing. Trace the underlying cause: the code may model the domain inaccurately, put knowledge in the wrong place, or force distinct concepts through one abstraction. Prefer the fix that addresses that cause unless it costs more than it returns. When it needs independent work beyond the PR, land a sound in-scope fix and describe the broader option and its benefit in the report.

Before satisfying any comment with a guard — a test, a validation rule, prose that enforces an invariant, or code that compensates for a rule another layer should own (a client-side cascade, a cleanup helper, a fallback, a second copy of a mapping) — classify the invariant: is there a concrete representation, one you can name in this codebase, that would make it true by construction (one source of truth instead of copies that must agree, a type that cannot express the illegal state, a derived value instead of an authored one)? When there is, the guard is off the table for that finding — in scope or not, and unlike other structural fixes it is not landed "for now": guards written now are the reason the reshape never happens. Escalate instead, without attempting the reshape inline: present the reshape and what it retires (the tests, rules, and prose it deletes), and let the user choose it or the literal patch. Carry on with the findings that don't depend on that choice.

When a decision during the review takes on a cost (a schema change, a deployment gate), reconsider every finding deferred for that cost alone.

When a fix needs a regression test, choose the test level that exercises the failure; framework integration bugs may need tests of the generated output rather than only isolated helpers.

For documentation fixes, correct misleading claims and trim repetition while preserving _useful_ rationale, caveats, references, and public contracts.

## Report

Account for each actionable comment or question in the report: what was fixed or answered, what was already addressed, and what was declined, deferred, or blocked and why. Name any change no comment asked for.
