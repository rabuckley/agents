# C# 15

C# 15 is the default language version for `net11.0`. Every feature here except memory safety ships without `<LangVersion>preview</LangVersion>`. Sources: [What's new in C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15), [Union types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union).

Features are listed most-valuable first. Each entry gives the **signals** that find candidates, the **rewrite**, and the **fit** test that decides whether the rewrite earns its place.

## Closed hierarchies

`closed` on a class lets only its own assembly derive from it, so a `switch` that handles every direct descendant is exhaustive and needs no default arm.

**Signals**

- An `abstract` base (often an `abstract record`) whose derived types all live in the same assembly and are consumed by `switch` or `is` chains.
- Discard arms that exist only to satisfy the compiler: `_ => throw new UnreachableException()`, `_ => throw new ArgumentOutOfRangeException(...)`, `_ => throw new InvalidOperationException("Unknown ...")`, `default: throw`.
- A private or `internal` constructor on a public abstract base, added to stop outside derivation.
- Hand-rolled `Match(Func<A,T>, Func<B,T>)` methods on a base type.

**Rewrite**

```csharp
// Before
public abstract record GateState;
public sealed record Closed : GateState;
public sealed record Open(float Percent) : GateState;

string Describe(GateState s) => s switch
{
    Closed => "closed",
    Open(var p) => $"{p}% open",
    _ => throw new UnreachableException(),
};

// After
public closed record GateState;
public record Closed : GateState;
public record Open(float Percent) : GateState;

string Describe(GateState s) => s switch
{
    Closed => "closed",
    Open(var p) => $"{p}% open",
};
```

Delete the discard arm. That deletion is the point: adding a new case now warns at every switch that misses it.

**Fit**

- `closed` implies `abstract`; it can't be combined with `abstract`, `sealed` or `static`. Remove the explicit `abstract`.
- Derivation isn't transitive. A non-closed intermediate type can be derived from in other assemblies, so mark intermediate levels `closed` too if switches match on them.
- Leave it off when another assembly legitimately derives from the base (plugins, test doubles in a test project). Adding `closed` to a public type that consumers derive from is a breaking change.

## Union types

`union Pet(Cat, Dog, Bird);` declares a value that is exactly one of the listed **case types**, with an implicit conversion from each case and exhaustive `switch`.

**Signals**

- `OneOf<...>`, `OneOf.OneOf`, `Dunet`, `LanguageExt` `Either<L,R>`, or a hand-written `Result<T>`/`Either<A,B>` with a tag field and `Match`.
- A marker interface or empty abstract base that exists only to group unrelated types for pattern matching.
- A method returning `object` that callers immediately type-switch on.

**Rewrite**

```csharp
public union Pet(Cat, Dog, Bird);

Pet pet = new Dog("Rex");          // implicit conversion
string name = pet switch
{
    Dog d => d.Name,
    Cat c => c.Name,
    Bird b => b.Name,
};
```

**Choosing union or closed hierarchy**

- Prefer a **closed hierarchy** when you own all the cases and they share members or a base contract.
- Prefer a **union** when the cases are types you don't own or that shouldn't share a base: `union Result<T>(T, Error)`, `union IntOrString(int, string)`, a set of existing message types.

**Fit**

- The generated union is always a `struct` that stores its contents as `object?`, so it boxes value-type cases. On a hot path with value-type cases, either keep the existing type or write a custom union with `[Union]`, `IUnion` and the non-boxing `HasValue`/`TryGetValue` members (see the reference page).
- `default(Pet)` contains `null` and nullable analysis won't warn about it. Add a `null` arm wherever a default value can reach the switch (fields, array elements), and prefer `null` to `_` so new cases still warn.
- A type pattern tests the contents, not the union: `pet is Pet` is error CS8121. Use `pet is not null`.
- Unions don't add equality, deconstruction or `with`. Code that relied on `OneOf`'s equality or `Match`/`Switch` helpers must be rewritten to `switch` expressions; migrate a library type one call site cluster at a time.
- Requires the .NET 11 runtime (`UnionAttribute`, `IUnion`). A library that multi-targets older frameworks can't expose a union on those targets.

## Collection expression arguments

A leading `with(...)` element passes arguments to the collection's constructor or factory.

**Signals**

- `new List<T>(capacity)` / `new Dictionary<K,V>(capacity)` followed by `Add` calls or `AddRange`.
- `new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ... }`, `new Dictionary<string, T>(StringComparer.Ordinal)` with an initializer.
- Collection expressions that were abandoned or avoided because a comparer or capacity was needed.

**Rewrite**

```csharp
List<string> names = [with(capacity: values.Length * 2), .. values];
HashSet<string> set = [with(StringComparer.OrdinalIgnoreCase), "Hello", "HELLO"];
```

**Fit**: apply where the target type is explicit on the left. Leave `var x = new HashSet<string>(comparer);` with no elements alone; the rewrite gains nothing there.

## Labeled `break` and `continue`

`break label;` and `continue label;` target an enclosing loop or `switch` by name.

**Signals**

- A `bool found`/`done`/`stop` flag set in an inner loop and checked at each outer level.
- `goto` that jumps past nested loops.
- Inner loops extracted into a local function only so `return` can exit both.
- Run the [IDE0410](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0410) analyzer: it flags exactly the flag and `goto` patterns. `dotnet format style --diagnostics IDE0410 --severity info --verify-no-changes` lists them without editing.

**Rewrite**

```csharp
outer: for (int row = 0; row < grid.Height; row++)
{
    for (int column = 0; column < grid.Width; column++)
    {
        if (grid[row, column].IsBlocked) continue outer;
        if (grid[row, column].IsGoal) break outer;
    }
}
```

The label goes directly on the loop or `switch` it names.

## Extension indexers

An `extension(T receiver)` block can declare `this[...]`. The block needs a named receiver.

**Signals**: static extension methods named `At`, `GetAt`, `Item`, `ElementAt` wrappers, or `Get(this X x, Key k)` that callers use as an indexer; existing C# 14 `extension` blocks with such methods.

```csharp
public static class SequenceIndexer
{
    extension(IEnumerable<int> sequence)
    {
        public int this[int index] => sequence.ElementAt(index);
    }
}
```

**Fit**: only where indexer syntax reads naturally and the cost is what an indexer implies. An indexer over `IEnumerable` that is O(n) hides cost; keep that one a method.

## Non-virtual static interface members on older runtimes

Non-virtual `static` interface members now compile when the target runtime lacks default interface implementation support.

**Signals**: libraries that multi-target `netstandard2.0` or `net4x` and keep a companion static class (`MeasurementHelpers`, `FooExtensions`) only because the interface couldn't hold a static helper.

**Fit**: move the helper onto the interface only when it belongs to the interface's contract. `static abstract`/`static virtual`, instance default members and `protected` members still need runtime support.

## Memory safety (preview, report only)

Needs `<LangVersion>preview</LangVersion>`, `AllowUnsafeBlocks`, and the `updated-memory-safety-rules` feature for the full rules. Taking an address, `fixed`, `stackalloc` to pointer and `sizeof` no longer need `unsafe`; dereferencing still does. `unsafe(expr)` scopes an unsafe context to one expression (field initializers, constructor initializers, `catch` filters).

Don't enable preview features while modernising. Report projects with `unsafe` code as candidates and let the owner opt in.

## Breaking changes

Check the [C# compiler breaking changes for .NET 11](https://learn.microsoft.com/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%2011) when the build produces new errors after retargeting.
