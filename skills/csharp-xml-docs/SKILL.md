---
name: csharp-xml-docs
description: >
  The BCL and dotnet-api-docs house style for C# XML doc comments. Use when
  writing or editing a triple-slash `///` block, choosing the wording for a
  summary, param, returns, exception, or cref, or checking existing XML docs.
---

The project's documentation rules decide which APIs need triple-slash comments; this skill explains how to write them. In this repository, scope is defined in [the C# rule](../../claude/rules/csharp.md#xml-documentation).

Describe the intended contract and verify that the implementation supports it. Report discrepancies; incidental implementation behavior is not a guarantee. Include units, bounds, defaults, null and empty semantics, ordering, ownership, and failures where they matter to correct use. Contract accuracy takes precedence over wording templates.

Choose wording for the audience and published output. For wire contracts and API DTOs whose XML documentation feeds OpenAPI or generated clients, write language-neutral descriptions of fields and their observable semantics. Prefer "The concurrency token to send with edits." over "Gets or initializes the concurrency token." Use XML markup and C# symbol references where the publishing pipeline renders them usefully. This audience rule takes precedence over the wording and cross-reference templates below.

For APIs consumed primarily as .NET libraries, follow the BCL, `Microsoft.Extensions.*`, and ASP.NET Core style. The [dotnet-api-docs wiki](https://github.com/dotnet/dotnet-api-docs/wiki) provides further guidance; the cancellation-token and task-return conventions below are deliberate house departures. Take layout and wording templates from neighbouring docs, not their length: extra remarks or restated implementation nearby are not a convention to copy.

Tag order is `summary`, `typeparam`, `param`, `returns` (or `value` for a property), `exception`, `remarks`, `example`, `seealso`. Put opening and closing tags on their own lines, except for the empty cancellation-token tag below. Preserve an existing single-line `/// <summary>...</summary>` style when the complete comment line fits within the project's line-length guidelines.

## Summaries

Use one concise sentence ending in a period. Avoid unnecessary signature repetition; use `<paramref>` when referring to a parameter makes the contract clearer.

- Methods: present tense, third person, describing the operation rather than restating its name. Lead with `Asynchronously <verb>` when the operation is asynchronous; a name or return type alone does not establish this.
- Types: describe the concept or role, usually `Represents <concept>.` or `Provides <capability>.` Exception types use `The exception that is thrown when <condition>.`
- Constructors: `Initializes a new instance of the <see cref="TypeName"/> class.` Adapt the kind for a struct and add distinguishing initialization behavior when needed.
- Properties: match the accessors with `Gets`, `Gets or sets`, or `Gets or initializes`. Boolean properties use the matching prefix followed by `a value that indicates whether <condition>.`

## Parameters and returns

Prefer a noun phrase with a leading article and a period; let `cref` carry a type reference: `<param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>`. Keep meaningful semantics even when they need a different sentence shape.

Every parameter keeps its tag. A `cancellationToken` leaves its tag empty, `<param name="cancellationToken"></param>`, unless cancellation has member-specific semantics worth documenting. Explain effects such as partial work or rollback when they matter to correct use; the token's name cannot convey them.

For `Task<T>` and `ValueTask<T>`, `<returns>` describes the resulting value, such as `A URL with read permission for the file.`, rather than the task carrying it. Non-generic `Task` and `ValueTask` take no `<returns>` tag; describe meaningful completion guarantees in the summary or remarks.

Booleans read by direction:

- Parameter: `true to <effect>; otherwise, false.`
- Return value or `<value>`: `true if <condition>; otherwise, false.`

Use `<see langword="true"/>` and `<see langword="false"/>` in XML. Use the two-clause `true if X; false if Y` only when the false case needs stating.

Other shapes depend on the actual behavior:

- `out`: `When this method returns, contains <description>.` For `Try…` methods, describe the value on both success and failure.
- `ref`: describe the incoming value and how the method may change it; repeating `passed by reference` adds no contract.
- Flag enum: `A bitwise combination of the enumeration values that <...>.` A plain enum takes `One of the enumeration values that <...>.`, or just the clause when clearer, such as `How the browser presents the file.`
- `<value>` with a default: add `The default is <X>.`
- Fluent return: use `A reference to this instance after the operation has completed.` only when the returned object is guaranteed to be the same instance.

## Exceptions

Document exceptions thrown directly by the member and known propagated exceptions relevant to callers, with explicit conditions. Inspect relevant calls to identify likely failures; do not turn every incidental dependency failure into a supported guarantee. This follows the bounded priorities in the [upstream exceptions guidance](https://github.com/dotnet/dotnet-api-docs/wiki/Exceptions).

Write each condition as a bare clause reading as though "if" preceded it:

```xml
<exception cref="ArgumentNullException">
<paramref name="json"/> is <see langword="null"/>.
</exception>
```

Separate multiple conditions for one exception type with `-or-` in its own `<para>`. Use `<para>` for paragraph separation; blank `///` lines do not establish paragraphs.

For members that always throw, use `Throws a <see cref="ExceptionType"/> exception in all cases.` as the summary and `In all cases.` as the exception condition. Use `This method doesn't return a value.` in `<returns>` only when the declaration has a return value and the return conventions above call for the tag.

Verify whether each exception is thrown synchronously or delivered through the returned awaitable; an `Async` suffix does not establish this. For a `Task` that stores the exception, append `This exception is stored into the returned task.` For `ValueTask`, describe delivery when awaited without assuming a backing `Task`.

For lazy results such as `IEnumerable<T>` and `IAsyncEnumerable<T>`, document when work and failures occur, whether repeated enumeration repeats work, and resource ownership where these affect correct use. Distinguish call-time, await-time, and enumeration-time failures.

## Cross-references and inheritance

Use `<see cref=""/>` for type and member references, with `{}` for generics (`<see cref="Contains{T}(ReadOnlySpan{T}, T)"/>`). Qualify references as needed to resolve the intended symbol. Use `<paramref>` and `<typeparamref>` for parameters, `<see langword=""/>` for language keywords, and `<c>` for values or fragments with no symbol behind them, such as `<c>-1</c>` or `<c>index + count</c>`. Named constants such as `int.MaxValue` take a `cref`. See the [C# XML documentation reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags).

Use `<inheritdoc/>` for overrides and interface implementations with the same contract; use an explicit `cref` when the source needs identifying. Inherit across overloads only when parameter meanings, results, and failure behavior match, and verify parameter mapping in the generator. Add or replace documentation for contract differences. Verify that the documentation generator expands inheritance in the distributed output: IDE inheritance alone does not populate the compiler's XML documentation. See the [inheritance guidance](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags#inheritdoc).

## Remarks

Use remarks for supplemental contract details, including obligations for interface implementers and derived classes. Keep essential calling information in `summary`, `param`, or `returns`. Type-level remarks describe shared guarantees without repeating member contracts.

Document “Disposing the writer leaves the stream open.” Remove “so callers don’t need a wrapper to reuse the stream.” Preserve necessary maintainer reasoning beside the implementation.

## Examples and final check

Both `Try…` outcomes, null semantics, and a precise parameter reference:

```csharp
/// <summary>
/// Attempts to read a port number from <paramref name="text"/>.
/// </summary>
/// <param name="text">
/// The decimal text to parse; <see langword="null"/> or empty text is invalid.
/// </param>
/// <param name="port">
/// When this method returns, contains the port number from 1 through 65535 on success,
/// or zero on failure.
/// </param>
/// <returns>
/// <see langword="true"/> if the text is a valid port number; otherwise, <see langword="false"/>.
/// </returns>
public bool TryReadPort(string? text, out int port);
```

Before finishing, check that the XML is well formed, parameter and type-parameter names match the declaration, references resolve, and every promise reflects the intended contract and is supported by the implementation. Use the project's documentation build to check compiler diagnostics and generated output, including inherited documentation, where available.
