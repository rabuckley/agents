# .NET pack

## Surface

Public and protected members of public types, plus the documented contract (XML docs). Nullable annotations are part of the contract. For proposals, use background, declarations, usage samples, alternatives, and risks to establish caller scenarios. Missing samples limit the evidence; they do not alone establish a defect.

## Contract expressions

- **Errors.** Exceptions, for contract violations and failures only. Avoidable without try/catch via a Tester-Doer pair (`CanSeek`/`Seek`) or a `TryX` method returning `bool` with `[NotNullWhen(true)]` (`[MaybeNullWhen(false)]` for generic `T`) on the out parameter; pair every `TryX` with a throwing `X`. Reuse `ArgumentNullException`, `ArgumentOutOfRangeException`, `ArgumentException`, `InvalidOperationException`, `NotSupportedException`, `ObjectDisposedException`; throw neither `Exception` nor `SystemException`; a custom exception needs a caller who would catch it specifically.
- **Absence.** Nullable return where absence is normal, the Try pattern where the caller tests first, throw where absence is a caller bug. Collections and arrays are never null: return the empty one.
- **Ownership and mutability.** Never expose `List<T>`, `Dictionary<K,V>`, or arrays as properties, and collection properties are get-only (`init` at most); return `IReadOnlyList<T>` or `IReadOnlyCollection<T>`, or `IEnumerable<T>` when forward-only iteration is the only supported scenario; take the least-derived type that does the job (`IEnumerable<T>` over `List<T>`). Mutable structs and mutable static state are findings. Span/Memory ownership rules live in the modern-dotnet digest below.
- **Lifetime and threading.** `IDisposable` where cleanup is real; `Dispose` idempotent and safe to call twice. The BCL baseline callers assume: public static members are thread-safe, instance members are not unless documented; state any divergence.
- **Async.** `Async` suffix on awaitable-returning members; `CancellationToken` last, defaulted when cancellation is optional, offered only where cancellation is real; return `Task` unless measurement justifies `ValueTask`; never `async void` outside event handlers. I/O-bound is async only, compute-bound sync only; a sync-over-async or `Task.Run` wrapper is a finding, except on an abstract base (`Stream`) whose implementers may be genuinely either.
- **Compatibility.** State binary-, source-, or behavioural. Safe later: add an overload (unless it captures existing calls), unseal, add a non-abstract member. Never safe: remove, rename, seal, change a parameter name (named arguments), add an abstract member, or add an interface member without a default implementation. Full rules in the modern-dotnet digest.

## Idiom

The rules that most often decide shape:

- **Types.** Most types are classes. A struct only for a small, immutable, single value (usually `readonly record struct`). An abstract class when the abstraction will grow; an interface when value types must implement it or a base class is taken. Every abstraction ships with an implementation and a consumer. An enum over string modes and any pair of bools, never for open sets; zero is `None` or the most common default (on a flags enum, always `None`). Plain data is a record. Encode invariants with the type system's tools: `required` and `init` against half-built instances, a closed hierarchy via internal constructors for a fixed set of cases, `readonly record struct` for values. Public nested types and static bucket classes are findings.
- **Members.** A property is data: cheap, order-independent, no throwing getter; work and conversion are methods. No public instance fields. Constructors capture parameters (named as the properties they set) and do minimal work; a static factory when the operation is not plainly "construct". Overloads keep parameter names and order consistent; the longest does the work. Validate at the public surface with the `ArgumentException` family plus `paramName`. Events use `EventHandler<TEventArgs>`, with a protected virtual `OnX(e)` raiser on unsealed classes; a callback that is not an event is `Func`/`Action`. Extension methods never house what should be an instance member of a type you own.
- **Extensibility.** The least powerful mechanism that serves the scenario: unsealed class, then event or callback, then protected virtual, then abstract. A protected member is public for compatibility purposes. Abstract classes have protected constructors, or internal ones when the hierarchy is deliberately closed.
- **Naming.** PascalCase public, camelCase parameters, no abbreviations or underscores, `I` interfaces, `T` type parameters, suffixes for `Exception`/`Attribute`/`EventArgs`/`Async`. Types and properties nouns, methods verbs, booleans affirmative (`CanSeek`). Mirror BCL names for the same concept (`Count` not `Length` for a collection, `TryParse`, `GetEnumerator`).

Full digests: [`dotnet-fdg.md`](dotnet-fdg.md) (the 2008 Framework Design Guidelines) and [`dotnet-modern.md`](dotnet-modern.md) (post-2008: async, spans, nullability, records, sealing, DI and options, breaking-change rules, how the review board reads proposals). Read the section that matches the surface, not the whole file.

## Rulings

- **Seal unless inheritance is designed.** Unsealing later is free, sealing later breaks derived types, so sealed is the reversible choice; current dotnet/runtime reviews approve new option, result, and service types sealed. A type meant to be inherited has its protected and virtual surface reviewed as API.
- **Overloads versus options.** Overloads for a small number of convenience shapes with consistent names and order, the longest doing the work; once optional inputs pass three or four, or will grow, an options type. Never `bool` parameters to select behaviour: an enum reads at the call site and can grow.
- **Out parameters.** FDG says avoid `out`/`ref`; the Try pattern is the sanctioned exception, and the only one to accept.
- **Interfaces are not for testability.** FDG's "prefer classes" agrees with the generic seam ruling; the `IFooService` + `FooService` + `AddFoo()` DI trio is the pattern to run the shape tests on, not a defence.

## Reflexes

- You will propose the DI trio (interface, implementation, registration extension) because it is the ecosystem's most familiar pattern. It is a hypothetical seam until a second implementation or an out-of-assembly consumer exists.
- You will import BCL scenarios and knobs (`RateLimitLease`, `QueueLimit`, buffer-size options, `FileSystemWatcher`'s event shape) as requirements. They are precedent, not evidence; `SemaphoreSlim` and `Channel<T>` leave caller-created load to the caller.
- You will prefer events because the FDG does. FDG's idiom dates from 2008; an `IAsyncEnumerable<T>`, a value-returning method, or a record is not wrong for diverging when the scenario code is simpler.
- You will flag `IReadOnlyList<T>` or a read-only interface over a private array because a cast can reach the backing store. That is the normal BCL shape, not a finding.
