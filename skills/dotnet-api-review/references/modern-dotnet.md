# Modern .NET conventions (after the 2008 FDG)

Condensed from Microsoft Learn, dotnet/runtime docs, dotnet/apireviews notes
(2021-2026), and .NET team blog posts (Toub, Fowler, Landwerth, Torgersen).
Where this file and `fdg.md` disagree, this file wins. Sources per section at
the end of the section. Read the section that matches the surface.

## Async (Task-based Asynchronous Pattern)

- Suffix awaitable-returning members with `Async`; never suffix a synchronous
  member. A method that starts work without returning an awaitable is
  `Begin`/`Start`. Combinators (`WhenAll`) are exempt. Return `Task` where
  the sync shape returns `void`, `Task<T>` where it returns `T`; put what
  would have been `out` parameters into the result type.
- Expose the shape the work is: I/O-bound work is async only; compute-bound
  work is sync only. A sync wrapper over async risks deadlock and pool
  starvation; an async wrapper (`Task.Run`) over sync adds no scalability,
  bloats the surface, and the caller can offload themselves (Toub; Learn
  "Implementing TAP"). Abstract bases like `Stream` are the exception because
  derived types may be genuinely asynchronous. Review example: `ZipArchive`
  got a static `CreateAsync` because its constructor does I/O, while
  `ZipArchiveEntry.Delete` got no `DeleteAsync` because it does none.
- `CancellationToken` last (CA1068), named `cancellationToken`, defaulted
  when cancellation is optional; offer it only on operations that actually
  cancel; flow it to every call you make. `IProgress<T>` named `progress`,
  null allowed, only where progress is real.
- Validate arguments synchronously and throw directly for usage errors; every
  other failure goes onto the returned task. Returned tasks are hot.
- Default to `Task`/`Task<T>`. `ValueTask` only when measurement shows
  allocation pressure, synchronous completion is common, and callers will
  just await it once (never await twice, never block on it; CA2012). On
  virtual or interface members ask whether that holds for every implementer.
- `async void` only for event handlers. No `Task.Result`/`.Wait()` on
  incomplete tasks in library code; `ConfigureAwait(false)` throughout
  general-purpose libraries (CA2007), not in app code.
- Adding an async member beside an existing sync one is additive; changing a
  member from sync to async (or back) is a behavioural breaking change.

Sources: learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap;
.../implementing-the-task-based-asynchronous-pattern;
devblogs.microsoft.com/dotnet/understanding-the-whys-whats-and-whens-of-valuetask/;
devblogs.microsoft.com/pfxteam/should-i-expose-synchronous-wrappers-for-asynchronous-methods/;
devblogs.microsoft.com/pfxteam/should-i-expose-asynchronous-wrappers-for-synchronous-methods/;
devblogs.microsoft.com/dotnet/configureawait-faq/;
github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md;
github.com/dotnet/apireviews/blob/main/2025/03-11-quick-reviews/README.md.

## Buffers (`Span<T>` and `Memory<T>`)

- Synchronous read-only input: `ReadOnlySpan<T>` (the universal receiver:
  arrays, `string` for `char`, `stackalloc`, native memory). Synchronous
  writable: `Span<T>`. Asynchronous read-only: `ReadOnlyMemory<T>`.
  Asynchronous writable: `Memory<T>`. No pointers for buffer methods.
- Do not offer both `ReadOnlySpan<T>` and `Span<T>` overloads, nor span
  overloads beside array or pointer overloads for the same operation; the
  implicit conversions make the pair redundant. When a `ReadOnlySpan<char>`
  overload competes with an `object` overload, add
  `[OverloadResolutionPriority]`.
- Ownership: one owner per buffer; a consumer may touch it only during its
  lease. A `void` method's lease on a `Memory<T>` parameter ends at return; a
  `Task`-returning method's lease ends when the task completes. A constructor
  or settable property taking `Memory<T>` makes the whole instance a consumer.
  An `IMemoryOwner<T>` parameter means you now own it: dispose or transfer,
  never both; a type whose constructor takes one implements `IDisposable`.
- Return `Memory<T>` (or a span) rather than the backing array when the caller
  needs offset and length. Name binary spans so they cannot be confused with
  text (`thumbprintBytes`, `utf8Label`).

Sources: learn.microsoft.com/dotnet/standard/memory-and-spans/memory-t-usage-guidelines;
github.com/dotnet/runtime/blob/main/docs/coding-guidelines/api-guidelines/System.Memory.md;
apireviews 2024-10-15, 2024-10-22, 2025-02-04, 2025-02-18.

## Nullability and the Try pattern

- Annotations are the contract. A parameter is non-nullable when the member
  throws `ArgumentNullException` (or would dereference) on null; nullable
  when null is accepted, substituted, or optional. Returns and `out`
  parameters are nullable if null can come back under any circumstance.
  When in doubt, nullable. Keep the runtime null checks; annotations change
  nothing at run time and many callers have analysis off.
- `TryX` returns `bool`, with `[NotNullWhen(true)] out T? result` (or
  `[MaybeNullWhen(false)] out T result` when `T` is generic). Pair every
  `TryX` with a throwing `X`. Define the try condition strictly: failures
  outside it still throw. Do not mix the Try pattern with an
  `OperationStatus`-style result; pick one. Name to match the sibling
  (`Dictionary.Remove` stays `Remove`, not `TryRemove`).
- Other attributes that carry contract: `[NotNullWhen(false)]` for
  `IsNullOrEmpty` shapes, `[NotNullIfNotNull]`, `[MemberNotNull]`,
  `[DoesNotReturn]`. On virtual members a non-nullable return binds every
  override.

Sources: learn.microsoft.com/dotnet/csharp/nullable-references;
learn.microsoft.com/dotnet/csharp/language-reference/attributes/nullable-analysis;
github.com/dotnet/runtime/blob/main/docs/coding-guidelines/api-guidelines/nullability.md;
devblogs.microsoft.com/dotnet/embracing-nullable-reference-types/;
learn.microsoft.com/dotnet/standard/design-guidelines/exceptions-and-performance;
apireviews 2024-11-19, 2025-02-04, 2025-11-04.

## Types: records, init, required, sealing

- A record when the type's job is to hold data, equal values should be equal,
  and immutability is wanted; `readonly record struct` for small values
  (roughly 64 bytes or less by Learn's type-choice page; the FDG's 16-byte
  struct rule is the stricter, older number); `record class` for larger data
  or inheritance; a class for behaviour, polymorphism, or mutable state.
  Mutable records break hash-based storage; collection members compare by
  reference. Not for EF entities.
- `init` lets callers use object initializers and forbids later mutation but
  does not force initialization; `required` does. `required` members must be
  as visible as the type; a type with them cannot satisfy `new()`.
- Sealing. FDG 2008: do not seal without a good reason. The breaking-change
  rules make the asymmetry the practical guide: removing `sealed` later is
  allowed, adding it to a type with accessible constructors is breaking. New
  option, result, and service types in 2024-2026 dotnet/runtime reviews are
  approved as sealed classes, and every Learn DI/options sample is
  `public sealed class`. The runtime's written sealed-by-default rule covers
  internal and private types only (coding-style.md, CA1852); there is no
  written rule for public types, so treat "seal unless inheritance is a
  designed scenario" as the reviewer's default, not as a citation. A type
  designed for inheritance has its protected and virtual surface reviewed as
  API.

Sources: learn.microsoft.com/dotnet/csharp/fundamentals/types/records;
learn.microsoft.com/dotnet/csharp/fundamentals/types/;
learn.microsoft.com/dotnet/csharp/language-reference/keywords/required;
devblogs.microsoft.com/dotnet/c-9-0-on-the-record/;
learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1852;
github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md;
apireviews 2026-06-09, 2026-05-12, 2026-02-03.

## Collections and properties (analyzers that refine the FDG)

- CA1819: a property must not return an array; return a read-only collection
  or make it a method. CA2227: collection properties are get-only (or
  `init`); offer `Clear`/`AddRange` if replacement is common; DTOs are the
  accepted exception. Prefer `IReadOnlyList<T>`/`IReadOnlyCollection<T>`
  (or `ImmutableArray<T>` for fixed results) over `ReadOnlyCollection<T>` in
  new surface; the runtime reviews return `ImmutableArray<T>` and take
  `IEnumerable<T>`/`IEnumerable<KeyValuePair<,>>`.
- Prefer existing delegate and collection types over new ones: `Func`/
  `Action`, `IEnumerable<T>`, `KeyValuePair<,>` instead of a custom delegate
  or a tuple-typed collection.

Sources: learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1819;
.../ca2227; apireviews 2024-03-19, 2024-07-16, 2025-01-07, 2025-09-30.

## Dependency injection and options (library authors)

- Registration is `Add{Service}(this IServiceCollection services, ...)` in a
  static `{Type}Extensions` class, returning `IServiceCollection`; name it so
  it cannot collide with Microsoft packages and do not use the
  `Microsoft.Extensions.DependencyInjection` namespace for third-party
  packages. Offer the overloads that fit: parameterless with defaults,
  `IConfiguration` section, config section path (`BindConfiguration` +
  `ValidateDataAnnotations().ValidateOnStart()`), `Action<TOptions>`.
- An options class is non-abstract, has a public parameterless constructor
  and public read-write properties (fields are not bound); validate with
  `ValidateOnStart` so failure is at startup. Consumers take `IOptions<T>`,
  `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>`; the wrapper frees the
  library from dictating `T`'s lifetime.
- No stateful statics or global state; no service locator (`GetService` where
  injection would do, or a runtime-resolving factory); no `IDisposable`
  transients; a receiver never disposes an injected dependency; singletons
  are thread-safe; many constructor dependencies signal too many
  responsibilities; prefer one constructor taking every resolvable
  dependency.

Sources: learn.microsoft.com/dotnet/core/extensions/options-library-authors;
.../options; .../dependency-injection-guidelines; .../dependency-injection.

## Compatibility: what breaks

From dotnet/runtime `breaking-change-rules.md` and Learn "Change rules for
compatibility". Bucket 1 (rename or remove a type or member, change a return
type, seal an unsealed type) is generally not accepted in the runtime;
behavioural changes are judged case by case.

Allowed without breaking: add an overload (unless it captures existing
calls with different behaviour), add a non-abstract member to a class, add a
constructor only if the parameterless one is kept, unseal, make `abstract`
`virtual`, add an interface implementation, add an interface member with a
default implementation (with care), add `params`, widen accepted input on a
non-virtual member, throw a more derived exception, change `struct` to
`readonly struct`, return a more derived runtime type, move a type with
`TypeForwardedTo`.

Breaking, including the non-obvious:

- Rename a parameter, even by case (named arguments); add, remove, or
  reorder parameters; change `ref`/`out`/`in`; remove `params`.
- Add a member to a shipped interface (without a default implementation);
  add an abstract member to an unsealed class with accessible constructors;
  add or remove `virtual`, `static`, `sealed` on a member.
- Seal a type with accessible constructors; decrease visibility; change a
  namespace, base class, or `struct`/`class`; change an enum's underlying
  type or member values; change a constant.
- Change a default parameter value (behavioural on recompile), or a
  property's default; narrow accepted input; widen or change returned values
  (`ToString` output counts); change event order; change sync to async or
  back; add a field to a struct that had none.
- Add an overload that captures existing call sites with different
  behaviour (`M(uint)` then `M(int)`).

Sources: github.com/dotnet/runtime/blob/main/docs/coding-guidelines/breaking-change-rules.md;
.../breaking-changes.md; learn.microsoft.com/dotnet/core/compatibility/library-change-rules.

## The API proposal and how the review board reads it

A proposal (dotnet/runtime template) carries: background and motivation;
the API as a ref-assembly-style declaration diff (no bodies); usage samples;
alternative designs; risks (breaking changes, performance). When reviewing a
proposal rather than code, check it has each of these before judging it,
and treat missing usage samples as a finding in their own right: the board
calls a proposal actionable only with a concrete sketch and code samples.

Recurring board feedback to apply (each backed by a 2024-2026 note):

- Consistency with the BCL and sibling types beats local optimality: mirror
  existing names (`TryGetTarget`/`SetTarget` after `WeakReference<T>`),
  existing overload patterns, existing parameter order (`isFinalBlock` last
  "consistent with Brotli"), existing non-virtual-ness.
- Parameter names are changed freely before approval because they cannot be
  after (`thumbprint` to `thumbprintBytes`, `addBeginOfSentence` to
  `addBeginningOfSentence`).
- Bool parameters get pushback (`ownsEncoder` rejected); a flags enum was
  turned into an options struct; but `Stop(bool stopDependentServices)` was
  approved, so it is judgment.
- Redundant overloads are cut (`JsonNode.WriteTo(Stream)` rejected as
  redundant with `JsonSerializer.Serialize`); when a better name replaces an
  old member, the old one is hidden with `[EditorBrowsable(Never)]` rather
  than obsoleted.
- No API when the problem is better solved elsewhere ("could we handle this
  automatically?"; JIT work preferred to a defaulted parameter; a niche
  `AddOrReplace` rejected with a workaround).
- Unnecessary abstraction is removed ("Remove unnecessary abstract
  modifiers"; a scope type made internal and replaced with `IDisposable`).
- Options bags are designed for but deferred until needed; an existing
  options type is extended rather than a second one added.

Sources: github.com/dotnet/runtime/blob/main/docs/project/api-review-process.md;
github.com/dotnet/runtime/blob/main/.github/ISSUE_TEMPLATE/02_api_proposal.yml;
github.com/dotnet/runtime/blob/main/docs/coding-guidelines/adding-api-guidelines.md;
github.com/dotnet/apireviews (2024-2026 quick-review READMEs; specific dates
cited inline above).
