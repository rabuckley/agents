---
name: better-comments
description: Trim and tighten the comments in this branch's work.
disable-model-invocation: true
argument-hint: "[base-ref | paths]"
---

Review every comment block that contains a line this branch added or changed (diff against the merge-base with the default branch, or the given ref or paths), plus existing comments the change made false. A block is done only when every sentence in it, including sentences the branch didn't touch, has been cut, or kept with evidence for each of its claims and a named mistake it prevents. Edit comments only, not code.

Start from cutting: a sentence stays only if a competent reader, without it, would plausibly get something wrong: break an invariant, "fix" a deliberate workaround, undo a trade-off, or misread dense code. For each sentence you keep, name that mistake ("without it, an editor would release the lock before the callback runs"). Naming a category (why, invariant, summary) is not enough, and neither is a mistake the code's names, types, assertions or tests already prevent. This cuts sentences that restate the code or its names, whys any reader infers from the code ("retry because the request can fail"), summaries of code that reads fine on its own, invariants a type or assertion already enforces, history and change narration ("now", "updated to", "previously", "no longer"), which belongs in the commit message, and anything addressed to the reviewer. A relationship claim ("mirrors X", "for display only", "X is the source of truth") says what the code is connected to, not what goes wrong; it stays only when rewritten to state what must stay true and what breaks otherwise ("lists fields in X's order: the importer reads them by position"). When unsure, cut. A comment survives as only its passing sentences, and often as none.

A sentence that passes is still unchecked. Before keeping it, verify each claim it makes against the code: every stated reason, and anything it says a caller, query, schema, migration, script or job does. Find the evidence (the foreign key the index serves, the query that reads deleted rows, the migrator code that re-creates the rows) and note its location. That a reason sounds plausible is not evidence. Cut a claim the repository should show but doesn't, or one it contradicts or shows to be incomplete; rewrite it to what the evidence shows only if the rewrite passes the test above. A claim about something outside the repository that you can't check stays only if it passes, and the recap lists it as unverified.

Keep the documentation a public API is required to have, at its minimum: a required summary that can only restate the member's name stays one short line, and every other doc sentence must pass the test. Doc comments on non-public members get the same test as any other comment. Leave tool directives (lint suppressions, pragmas, build tags) alone.

Then check that each surviving sentence sits where its reader looks, and is written for that reader. A doc comment is read by callers, so it states the contract, not the implementation. A comment in a body is read by whoever edits that code, so it explains that code, not a caller's purpose ("for the X form", "what the page submits") or a callee's internals. Move a sentence that is true but misplaced to the code it describes, rephrased for the reader there; cut it if no place fits.

Rewrite survivors to be short, specific and in the present tense, matching the file's style.

Don't commit. Recap what you changed, each kept sentence with the mistake it prevents and where its evidence is, any unverified claims, plus any open TODOs.
