---
name: better-comments
description: Trim and tighten the comments in this branch's work.
disable-model-invocation: true
argument-hint: "[base-ref] [paths...]"
---

Leave readers with the information they need to use or change the code correctly. Prefer deleting repetition to polishing it; brevity alone is not the objective. Edit comments only, not code.

## Scope

Use an explicit base ref when supplied. Otherwise, resolve the repository's default branch from its remote HEAD or repository metadata, using the locally available tracking ref. If it cannot be resolved, ask for a base ref rather than guessing `main` or `master`. If the ref or its merge-base with `HEAD` is unavailable, report that and ask for a usable ref.

Compare the working tree with that merge-base: include committed branch changes, staged changes and unstaged changes, plus untracked source files. Paths restrict this scope; they do not replace the baseline. Report the resolved base and any path restriction.

Review every complete comment block containing an added or changed line, including untouched sentences within that block, plus existing comments the scoped code changes made false. Leave tool directives (lint suppressions, pragmas, build tags), generated comments and licence headers alone. Read beyond the scope as needed for evidence; avoid turning the cleanup into a wider documentation audit.

## Decide what earns its place

Decide each in-scope block's fate before editing any of it: delete it, keep it, or keep parts. Then check every sentence you keep against the tests below: identify its reader and purpose, check what the surrounding code already communicates, and verify its factual claims. Rewording or rewrapping a sentence implies it survived, so never touch one you haven't tested. When a block has a line that needs fixing, start by asking whether the block should exist at all.

For implementation comments, identify the concrete mistake a competent maintainer could plausibly make without the sentence: remove a deliberate workaround, undo a trade-off, break an invariant, or misread unavoidably dense code. Names, types, assertions and tests may enforce what must hold without explaining why it matters; retain rationale that prevents a maintainer from removing or weakening that enforcement. Cut restatements, obvious reasons, summaries of clear code, change narration and remarks addressed to the reviewer. Relationship claims such as "mirrors X" earn their place when they explain a dependency the reader needs to preserve and what breaks if it changes.

For API documentation, retain a sentence when it helps callers use the API correctly: understand valid inputs, interpret results, account for side effects or handle failures. Useful contract facts can belong here even when visible in types or tests; callers should not need to inspect the implementation. Apply this caller-use test independently: rejecting an implementation comment does not make it API documentation. Internal rationale, change history and a particular caller's purpose do not belong in the contract merely because they have nowhere else to go. Follow the repository's documentation conventions, including required summaries, and cut redundant or irrelevant prose. Judge documentation on non-public interfaces by its actual readers and use, rather than visibility alone.

Doc comments in tests, including summaries on public test classes and fixtures, are implementation comments. Their readers are maintainers. Test names, arrange code and assertions already state the behaviour and the setup, so a summary that lists them repeats the code. Keep only what a maintainer can't see in the code: shared state that tests must respect, a constraint on ordering or isolation, or why the setup differs from the obvious one.

Issue and PR numbers record history. Keep one only when it points to an open constraint or workaround that the reader needs to follow up.

## Verify claims

Check stated reasons and claims about callers, queries, schemas, migrations, scripts or jobs against the relevant implementation or authoritative source. Plausibility is not evidence. Correct contradicted or incomplete claims to what the evidence supports, retaining only information useful to the reader.

Separate uncertainty about usefulness from uncertainty about truth. Delete clear restatements. Investigate uncertain factual claims; missing evidence alone does not prove a claim false. When verification is unavailable and a comment may record an important external constraint or workaround, preserve the potentially useful explanation, avoid strengthening it, and flag the claim as unverified in the recap. Record evidence locations for disputed or consequential claims; sentence-level scrutiny is an internal discipline, not a requirement to report every retained sentence.

## Place and rewrite survivors

Put information where its reader looks. API documentation states the caller's contract; implementation comments explain the code being maintained. Reassess each sentence for the destination's reader before moving it; relocation cannot rescue a sentence that fails the destination's standard. Keep comment moves within the same file when that improves their context. If useful material belongs in broader documentation or another file, leave it in place and report the suggested destination.

Before rewriting survivors, read and apply the writing rules in [unslop](../unslop/SKILL.md). Write in the present tense and match the file's comment format.

## Verify and report

Review the final diff for lost contracts, rationale or caveats, unintended code changes and unnecessary scope. Run checks proportional to the edit: prose-only changes usually need diff inspection; changes to structured documentation may need the relevant parser or documentation build.

Don't commit. Recap meaningful edits, unresolved consequential claims, and validation performed or blocked. List each in-scope block you kept without change, with one line saying why it earns its place. Include evidence locations where they help assess a disputed or consequential claim.
