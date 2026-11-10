---
name: better-comments
description: Trim and tighten the comments in this branch's work.
disable-model-invocation: true
argument-hint: "[base-ref] [paths...]"
---

Leave readers the information they need to use or change the code correctly, and nothing else. Edit comments only, not code. Don't commit.

## Scope

Compare the working tree with its merge-base against the base ref, so the scope covers committed, staged, unstaged and untracked changes. Use the base ref when one is given. Otherwise use the repository's default branch, read from the remote rather than assumed to be `main`. If no base resolves, ask for one. Paths narrow the scope; they do not replace the base.

Review every comment that has an added or changed line, including its untouched sentences, and every existing comment that the changed code made false. Leave tool directives (lint suppressions, pragmas, build tags), generated comments and licence headers alone.

## Decide what stays

Delete by default. A sentence stays only when it passes the test for its readers; being accurate is not enough. When a comment has a line that needs fixing, first ask whether the comment should exist.

Readers choose the test, not visibility. Documentation is for callers only when callers outside the feature read it. When every caller sits in the same feature, the readers are that feature's maintainers, even if the member is public. Comments in tests, doc comments included, are for maintainers.

**Maintainers.** Name the mistake a competent maintainer would plausibly make without the sentence: remove a workaround, undo a trade-off, break an invariant, or misread dense code. Before you judge, search for a test that asserts the behaviour the comment describes; a test that only runs the code does not count. A test that fails on the mistake already prevents it, so keep the comment only when the test's name and failure would not tell a maintainer whether to fix the code or change the test.

An argument that the design is sound does not name a mistake. "Approving and authoring are separate grants" defends the design. "A separate role, so the editor role never grants publishing" says what breaks if someone merges the roles. Keep only the second.

In tests, the names, setup and assertions already state the behaviour. Keep what they cannot show: shared state, an ordering or isolation constraint, or why the setup differs from the obvious one.

Keep an issue or PR number only when it points to an open constraint the reader must follow up.

**Callers.** Keep a sentence only when it helps a caller use the API correctly: valid inputs, the meaning of results, side effects, failures. A fact that types or tests already show stays when the caller would otherwise have to read the implementation or the tests to learn it. Internal rationale, change history and one caller's purpose are not part of the contract.

**Required comments.** Before you delete a doc comment or tag, check whether repository guidance or the linter, analyzer, compiler or documentation build requires it, including visibility rules and test exemptions. Rewrite a required comment that fails its test as short, accurate documentation instead of deleting it. Leave the rules and their suppressions unchanged.

## Check claims

Check each claim you keep or rewrite against the code or another authoritative source. Correct a wrong claim only if the corrected sentence still passes its test; otherwise delete it. A correction that needs more words than the original usually means the sentence should go.

Failing to verify a claim does not make it false. Keep an unverified claim only when it may record an external constraint or workaround, leave its wording alone, and flag it in the recap.

## Rewrite what stays

Read [unslop](../unslop/SKILL.md) before rewriting, and [csharp-xml-docs](../csharp-xml-docs/SKILL.md) before rewriting a `///` block.

Match the file's comment format but not its length. Wordy neighbours are not a standard. Where documentation is published beyond the language, such as into an OpenAPI schema, word it for that reader rather than by the language's templates.

In doc comments, refer to symbols with the format's reference syntax, such as `<see cref>` or `{@link}`, because the tooling validates those links and renames keep them correct. Check that each reference resolves to the symbol you mean. Keep code formatting for literal values.

Move a comment within its file when that puts it where its reader looks. Leave material that belongs in another file where it is, and report where it should go.

## Verify and report

Read the final diff for lost contracts or rationale and for code changes. If doc-comment coverage, required tags or symbol references changed, run the check that enforces them.

In the recap, give the resolved base and any path restriction, the meaningful edits, the claims left unverified, and the checks you ran or could not run. For each comment you kept, name in one line the mistake it prevents, the caller need it serves or the requirement it satisfies. For a maintainer comment, also name the test you found or "no test".
