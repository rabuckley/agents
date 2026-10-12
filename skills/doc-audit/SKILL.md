---
name: doc-audit
description: Edits comments and prose so they are accurate and worth the reader's time, and reports documentation problems it cannot fix in place.
argument-hint: "[git-ref | branch | range | path]"
disable-model-invocation: true
allowed-tools: Bash(git diff:*), Bash(git log:*), Bash(git rev-parse:*), Bash(git status:*), Read, Grep, Glob, Edit
---

# Documentation audit

Review the comments and prose in scope. Fix what is wrong or padded, leave what is useful, and report what you cannot fix.

## Scope

The argument selects what to audit:

- **No argument**: staged and unstaged changes against `HEAD`.
- **Ref or branch**: `git diff <ref>...HEAD`.
- **Range** (`a..b` or `a...b`): that range with git's usual semantics.
- **Path**: every file under it.

For diffs, audit the changed passages and the documentation block that encloses each one, not the whole file. Read outside the scope whenever you need it to judge a passage.

## Deciding what to do with each passage

Pick one of five actions.

**Keep** a passage when the intended reader needs it and cannot easily recover it where they are. What that means depends on the kind of text:

- API docs: the contract beyond the signature (preconditions, error behaviour, ownership, ordering).
- Inline comments: what the body cannot say, such as why a workaround exists or which invariant a block relies on.
- Standalone docs: whatever the stated audience needs for the stated task.

Rationale stays when it prevents a plausible mistake or explains a constraint.

**Delete** a passage when it restates the code, repeats a nearby passage, or is filler.

**Condense** a passage when it is useful but padded. Keep the examples, context and qualifications a reader needs to act correctly. Match the repository's conventions and do not restyle text you are not otherwise changing.

**Fix** a passage when it is inaccurate. Before rewriting, check every consequential claim against the implementation, tests and any referenced contract. Where code and docs disagree, work out which is wrong. Do not document a bug as intended behaviour. If you cannot find evidence either way, leave the passage and note what evidence is missing.

Describe constraints in the owning component's terms, not the current consumer's. Write "callers must not hold the lock across this call", not "the scheduler holds the lock, so this must not block".

**Report** instead of editing when the problem is missing documentation, a defect in the code, or a contract conflict you could not resolve.

## Never change

Legal text, safety-critical text, generated files, and anything a machine consumes: directives, doctests and documentation blocks a tool requires. These rules override everything above.

## Steps

1. Determine the scope and read enough surrounding code to understand each passage.
2. Decide keep, delete, condense, fix or report for each passage.
3. Make the edits.
4. Reread every edit as a reader who never saw the original.
5. Check the diff for edits outside the scope, lost meaning and formatting errors.
6. Run the repository's documentation checks if they exist and you are permitted to.

## Report

Finish with:

1. A table of edits, one row per passage, with the file, the action taken and the reason.
2. Unresolved findings: missing docs, code defects, contract conflicts, and passages left because evidence was missing.
3. Which checks ran and their result, and which you could not run.
