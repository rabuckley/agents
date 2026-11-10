---
name: csharp-xml-docs
description: >
  House style for C# XML doc comments: how much to write, and where the style
  departs from BCL conventions. Use when writing, editing or reviewing a
  triple-slash `///` block.
---

The project's rules decide which members get a `///` comment. This skill covers what goes in one.

## How much to write

Write the least that lets a caller use the member correctly. Callers read the comment in a tooltip beside the signature, so repeating the name and types gives them nothing.

A one-sentence `<summary>` is a complete comment. Add another tag only for a fact the signature does not show: a unit, a range, a default, what `null` or empty means, who owns a resource, an exception the caller should handle. Where it fits, state the fact in the summary with `<paramref>` instead. The compiler warns when only some parameters have tags (CS1573), so tag all of them or none.

Keep what a caller needs in the summary, because tooltips show the summary and not `<remarks>`. Use `<remarks>` for detail a caller can do without.

Don't document an exception the signature implies, such as `ArgumentNullException` for a non-nullable parameter.

State the contract: what the member is meant to guarantee, and what implementers must uphold. Leave out how it works and why it was designed that way. A reason a maintainer needs goes in a `//` comment beside the code. Behaviour the code happens to have, such as a sort order, is not a guarantee. If the code breaks the intended contract, report it rather than documenting the bug.

Copy layout from neighbouring comments, not their length.

## Style

Word comments as the BCL does, with these exceptions:

- **Layout.** Put opening and closing tags on their own lines. Keep an existing single-line `/// <summary>...</summary>` when it fits the line length.
- **Cancellation tokens.** In a full set of `<param>` tags, leave this one empty, `<param name="cancellationToken"></param>`, unless cancelling does something a caller must know, such as leaving partial work.
- **Tasks.** For `Task<T>` and `ValueTask<T>`, `<returns>` describes the value, not the task: `A URL with read permission for the file.` `Task` and `ValueTask` take no `<returns>`.
- **Wire contracts.** Where the comment feeds OpenAPI or a generated client, describe the field for a reader who doesn't know C#: `The concurrency token to send with edits.`, not `Gets or initializes the concurrency token.`

Use `<inheritdoc/>` on an override or implementation whose contract matches its base.

## Examples

A summary that earns its place by naming a side effect, with no other tags:

```csharp
/// <summary>
/// Asynchronously deletes the file, including versions still under retention.
/// </summary>
public Task DeleteAsync(FileId id, CancellationToken cancellationToken);
```

A wire-contract field:

```csharp
/// <summary>
/// The delay before dispatch, in seconds. <see langword="null"/> schedules automatically
/// and zero dispatches immediately.
/// </summary>
public int? Delay { get; init; }
```
