# Documentation decision cases

Run each prompt in a fresh session with the C# rule and `csharp-xml-docs` skill.
Judge the resulting comments and report against the expected decisions, not exact
phrasing. These are manual cases, not recorded model results. For the pipeline
case, provide the named project fixture as well as the prompt.

## Test helper

Prompt: Review documentation coverage for this helper in a test assembly. It is
public so several test projects can use it.

```csharp
public static class TestClock
{
    public static DateTimeOffset Noon => new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
}
```

Expected: No XML comments added solely for public accessibility or reuse across
test projects.

## DTO property

Prompt: Document this generated-client response. The established contract says
`Delay` is a number of seconds; null means automatic scheduling, and zero means
immediate dispatch.

```csharp
public sealed record DispatchDetails
{
    public int? Delay { get; init; }
}
```

Expected: Document the type and property, preserving seconds, null semantics, and
the distinct meaning of zero. Do not replace these with signature repetition.

## Incidental ordering

Prompt: Document this supported library method. The contract permits any order;
sorting is an implementation choice for the current storage backend.

```csharp
public IReadOnlyList<string> GetNames() => names.OrderBy(x => x).ToArray();
```

Expected: Do not promise sorted results. If an existing comment promises sorting,
report the discrepancy with the intended contract and correct the comment.

## Implementer obligation and rationale

Prompt: Document this library interface. Every implementation must leave the
supplied stream open when disposed. This saves callers from needing a wrapper to
reuse the stream. Derived implementations have the same obligation.

```csharp
public interface IRecordWriter : IDisposable
{
    void Write(Stream destination, string record);
}
```

Expected: Preserve the implementer obligation and stream ownership contract.
Omit the wrapper rationale from published documentation; preserve it beside an
implementation only if it explains a non-obvious decision there.

## Inherited documentation pipeline

Fixture: A small library with a documented interface, an implementation using
`<inheritdoc/>`, and a documentation build that distributes only compiler XML
without expanding inheritance.

Prompt: Check whether consumers receive the implementation's inherited contract
in this project's distributed documentation. Make the documentation usable.

Expected: Inspect the distributed output, identify the missing inherited text,
and supply explicit documentation or configure expansion using the project's
existing tooling. IDE tooltips alone are insufficient evidence. Verify the
resulting output. If no build can run, report the verification limit.

## Lazy result

Prompt: Document this supported library method. The contract is to read a file
anew on each enumeration, with file errors occurring during enumeration. The
iterator owns the reader and closes it when enumeration ends or its enumerator
is disposed.

```csharp
public IEnumerable<string> ReadLines(string path)
{
    using var reader = File.OpenText(path);
    while (reader.ReadLine() is { } line)
        yield return line;
}
```

Expected: Explain deferred work, enumeration-time file failures, repeated reads,
and reader lifetime. Do not describe file errors as call-time failures or imply
that an early-stopping consumer can leave its enumerator undisposed safely.
