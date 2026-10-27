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

Before deleting doc comments or required tags, check repository guidance and the effective linter, analyzer, compiler and documentation-build configuration, including visibility rules and test exemptions. Required comments are candidates for rewording and tightening; preserve the required coverage and tags, not the existing wording. When the usefulness tests would otherwise delete a required comment, rewrite it as concise, accurate documentation that satisfies the requirement. Preserve the enforcing rules and suppressions.

Deletion is the default for optional prose. A sentence stays only when you can name the mistake it prevents or the caller need it serves; being accurate does not earn it a place.

Before deciding any fate, find the tests for each in-scope implementation comment: search for tests that exercise the behaviour it describes and read their assertions. A test counts only when it asserts that behaviour; one that merely executes the code does not. This step is done when every implementation block has a named test or "no test".

Decide each in-scope block's fate before editing any of it: delete it, keep it, or keep parts. Apply the usefulness tests below first, subject to documentation requirements, and verify claims in the prose you retain or rewrite. When a block has a line that needs fixing, start by asking whether the block should exist at all.

For implementation comments, identify the concrete mistake a competent maintainer could plausibly make without the sentence: remove a deliberate workaround, undo a trade-off, break an invariant, or misread unavoidably dense code. Names, types and assertions may enforce what must hold without explaining why it matters; retain rationale only when it prevents a maintainer from removing or weakening that enforcement. A test raises the bar further: when it fails on the mistake, the failure already prevents it. Keep a comment on tested behaviour only when the test's name and failure would leave a maintainer unable to tell whether to fix the code or change the test. Cut restatements, obvious reasons, summaries of clear code, change narration and remarks addressed to the reviewer. Keep a relationship claim such as "mirrors X" only when it explains a dependency the reader needs to preserve and what breaks if it changes.

An argument that the design is sound is not rationale. "Approving and authoring are separate grants" defends the design. "A separate role, so the editor role never grants publishing" names what a maintainer would break by merging the roles. Keep only that clause.

Classify documentation by its readers, not its visibility. Treat it as API documentation only when callers outside the feature read it. When every caller of a type or member sits in the same feature, its readers are that feature's maintainers: apply the implementation test, even when it is exported or public.

For API documentation, retain a sentence only when it helps callers use the API correctly: understand valid inputs, interpret results, account for side effects or handle failures. Keep a contract fact already visible in types or tests only when a caller would otherwise have to read the implementation or tests to learn it. Apply this caller-use test independently: rejecting an implementation comment does not make it API documentation. Internal rationale, change history and a particular caller's purpose do not belong in the contract merely because they have nowhere else to go. Cut redundant or irrelevant prose beyond the required minimum.

Doc comments in tests, including summaries on public test classes and fixtures, are implementation comments. Their readers are maintainers. Test names, arrange code and assertions already state the behaviour and the setup, so a summary that lists them repeats the code. Beyond the required minimum, keep only what a maintainer can't see in the code: shared state that tests must respect, a constraint on ordering or isolation, or why the setup differs from the obvious one.

Issue and PR numbers record history. Keep one only when it points to an open constraint or workaround that the reader needs to follow up.

## Verify claims

Check stated reasons and claims about callers, queries, schemas, migrations, scripts or jobs against the relevant implementation or authoritative source. Plausibility is not evidence. Correct a wrong claim only if the corrected sentence still passes the usefulness test; otherwise delete it. Don't add caveats or detail to make a claim complete. A correction that needs more words than the original usually means the sentence should go.

Separate uncertainty about usefulness from uncertainty about truth. Delete clear restatements. Investigate uncertain factual claims; missing evidence alone does not prove a claim false. When verification is unavailable, preserve the explanation only when it may record an important external constraint or workaround; don't strengthen it, and flag the claim as unverified in the recap. Record evidence locations for disputed or consequential claims; sentence-level scrutiny is an internal discipline, not a requirement to report every retained sentence.

## Place and rewrite survivors

Put information where its reader looks. API documentation states the caller's contract; implementation comments explain the code being maintained. Reassess each sentence for the destination's reader before moving it; relocation cannot rescue a sentence that fails the destination's standard. Keep comment moves within the same file when that improves their context. If useful material belongs in broader documentation or another file, leave it in place and report the suggested destination.

Before rewriting survivors, read and apply the writing rules in [unslop](../unslop/SKILL.md). Write in the present tense. Match the file's comment format and apply documentation conventions consistently across a type's members. The documentation's audience and published output take precedence over language-specific wording templates. Don't match the file's density: wordy neighbouring comments are not a standard, and copying them carries the verbosity into new code. A kept block should not end longer than it started.

## Verify and report

Review the final diff for lost contracts, rationale or caveats, unintended code changes and unnecessary scope. Run checks proportional to the edit: prose-only changes usually need diff inspection; when doc-comment coverage or required tags change, run the enforcing lint, analyzer, compiler or documentation check.

Don't commit. Recap meaningful edits, unresolved consequential claims, and validation performed or blocked. For each in-scope block you kept, whole or in part, name in one line the mistake it prevents, the caller need it serves or the tooling requirement it satisfies. For a kept implementation comment, also name the test you found or "no test". Include evidence locations where they help assess a disputed or consequential claim.
