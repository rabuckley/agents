---
name: csharp-xml-docs
description: >
  The BCL and dotnet-api-docs house style for C# XML doc comments. Use when
  writing or editing a triple-slash `///` block on a public .NET API, when
  choosing the wording for a summary, param, returns, exception, or cref, or
  when checking whether existing XML docs follow the convention.
---

Document public API with triple-slash comments, following the BCL, `Microsoft.Extensions.*`, and ASP.NET Core. The [dotnet-api-docs wiki](https://github.com/dotnet/dotnet-api-docs/wiki) is the full house style guide, bar the two departures under params and returns; below are the parts most often got wrong.

Tag order is `summary`, `typeparam`, `param`, `returns`, `exception`, `remarks`, `example`, `seealso`, one tag per line with open and close on their own lines, unless the file already uses the single-line `/// <summary>...</summary>` form.

**Summary** - one sentence, present tense, third person, ending in a period, saying what the member does rather than restating its name. `String.Format` is "Replaces each format item in a specified string with the string representation of a specified object.", not "Formats a string." Keep parameter names and language keywords out of it. Async methods lead with `Asynchronously <verb>`. Properties are `Gets`, `Gets or sets`, or `Gets or initializes` for `init`-only, and a boolean property is `Gets or sets a value that indicates whether <condition>.` An exception type takes the one shape with no leading verb, `The exception that is thrown when <condition>.`, and a member that always throws is `Throws a <ExceptionType> exception in all cases.` with the reason in `remarks`.

**Params and returns** - noun phrase with a leading article, ending in a period, and let `cref` carry the type rather than naming it in prose: `<param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>`. The article is what makes a `<returns>` read: the renderer prints "Returns" ahead of the text, so `Url to the file with read permissions` lands as a fragment where `A URL with read permission for the file.` reads as a sentence.

Every parameter keeps its tag, but two shapes the wiki prescribes are not used here. A `cancellationToken` keeps the tag and leaves it empty, `<param name="cancellationToken"></param>`: the name says what a sentence would, and "The token to monitor for cancellation requests." lands on every async member without telling a caller anything. And a `<returns>` describes the value, never the task carrying it, so `<returns>A URL with read permission for the file.</returns>` rather than `A task object that, when awaited, produces a URL with read permission for the file.` A non-generic `Task` return takes no `<returns>` tag.

Booleans read by direction, and the split is load-bearing:

- parameter: `true to <effect>; otherwise, false.`
- return value or `<value>`: `true if <condition>; otherwise, false.`

Use the two-clause `true if X; false if Y` only when the false case needs stating. `; otherwise,` keeps its comma. The other fixed shapes:

- `out` param: `When this method returns, contains <description>. This parameter is treated as uninitialized.`
- `ref` param: `The <description>, passed by reference.`
- flag enum: `A bitwise combination of the enumeration values that <...>.`; a plain enum takes `One of the enumeration values that <...>.`, or just the clause when the prefix is the longer half of the line, as in `<param name="disposition">How the browser presents the file.</param>`
- `<value>` with a default: add a second sentence, `The default is <X>.`
- a member that always throws: `This method doesn't return a value.`

Fluent builders reuse one line across the whole surface, as `Microsoft.Extensions.*` does with `A reference to this instance after the operation has completed.` Pick one such line per builder and repeat it verbatim.

**Exceptions** - document everything a caller can hit, including what is thrown a few frames down, since an undocumented exception is one nobody catches. Write the condition as a bare clause reading as though "if" preceded it:

```csharp
/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
```

Separate multiple conditions for one exception type with a `-or-` line and a blank `///` line either side. For a member that always throws, the condition is `In all cases.` For a task-returning method that stores the exception, append `This exception is stored into the returned task.`

**Cross-references** - `<see cref=""/>` for every type and member, qualified when it crosses a namespace and using `{}` for generics (`<see cref="Contains{T}(ReadOnlySpan{T}, T)"/>`); `<see langword=""/>` for `null`, `true`, and `false`, so the rendering stays language-neutral; `<c>` for values and fragments with no symbol behind them, such as `<c>-1</c>`, `<c>"yyyy-MM-dd"</c>`, or `<c>index + count</c>`, while a named constant like `int.MaxValue` takes a `cref`; `<inheritdoc cref="..."/>` to carry docs across overloads differing only in argument shape.

**Remarks** - for what the signature cannot say: cost, aliasing, ownership, disposal, lifetime, ordering. What a caller needs in order to call correctly goes in `summary`, `param`, or `returns` instead. Do not document implementation details on interfaces unless it's a guarantee that all implementations must hold.

State the contract, never the case for the design. The reader is a caller deciding how to use the member, not a reviewer deciding whether the API is well shaped, so cut any clause about what the shape saves, what it avoids, or how the alternative would have gone: "so an implementation never has to reimplement it", "it is in the signature from the first release so that a later option is an additive change", "a blob written by another tool would otherwise come back with U+FEFF". The tell is a sentence whose subject is not the caller or the member: an implementer, a maintainer, a future release, the design itself. Reasoning a maintainer needs goes in a `//` comment on the code, which no renderer publishes. The fix is usually deletion, since the contract half of the sentence is already there and the "so ...", "rather than ...", or "otherwise ..." tail is the part to drop.

Type-level `<remarks>` is where that prose collects, because no single member's contract anchors it. Write one only for what holds across the whole type and cannot sit on any one member: thread safety, disposal, how the members compose, what a derived class owes. A type-level remark that restates in looser words what the members already document is worse than none, because the two drift.
