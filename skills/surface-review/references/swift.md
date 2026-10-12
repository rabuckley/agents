# Swift pack

Sources: the Swift API Design Guidelines (swift.org/documentation/api-design-guidelines), The Swift Programming Language (Error Handling, Access Control), Apple's "Choosing Between Structures and Classes", SE-0117 (open/public), SE-0235 (Result), SE-0296 (async/await), SE-0302 (Sendable), SE-0413 (typed throws), the library-evolution docs (swift.org/blog/library-evolution, swiftlang/swift LibraryEvolution.rst), and the Swift 6 concurrency migration guide's library-evolution chapter. Consult them when a finding needs the verbatim rule.

## Surface

Everything `public` or `open` in the module, plus `@usableFromInline`/`@inlinable` declarations (ABI-public) and the doc comments. `open` is a distinct, larger promise than `public`: it invites subclassing and overriding, and "marking a class as open explicitly indicates that you've considered the impact of code from other modules using that class as a superclass". A public protocol's requirements are surface for every conformer. In a single-module app target nothing is `public`; the surface is then what the default `internal` exposes across the module (a type's non-`private` members, its `private(set)` boundaries), and `package` declarations are surface to sibling targets. The distribution also picks which compatibility regime below applies.

## Contract expressions

- **Errors.** `throws` for recoverable failures the caller can distinguish and respond to; an error enum with associated values models the related conditions. Optionals where only absence matters: "when an operation fails, it's often useful to understand what caused the failure" is the line between them. Untyped `throws` stays the default for public surface; typed `throws(E)` fits errors handled entirely in-package, generic passthrough, and dependency-free code (SE-0413), and the named error type is then a commitment that breaks callers to change in either direction. `Result` is for storing or passing a failure across time, not a general signature shape; since async/await, `async throws` covers what completion-handler `Result` did. One channel per operation.
- **Absence.** `Optional` is the single blessed shape; `guard let`/`if let` is the caller's test. A throwing and an optional variant of the same operation is two channels; keep one unless both cases are real scenarios.
- **Ownership and mutability.** Structs by default; value semantics keep local changes local until deliberately communicated. A class opts into shared identity: reserve it for things that are identities (file handles, connections), and "treat identity with care". Exposed mutable class state is the aliasing hazard the struct default removes. An `@escaping` closure parameter declares that the surface keeps what it was passed; non-escaping declares that it does not.
- **Lifetime and concurrency.** Cleanup the caller must drive is a `close()`-style method, a scoped `withX { }` block, or a `~Copyable` type consumed by its teardown — shapes that make use-after-cleanup unrepresentable; `deinit` timing is not a contract. Conform a public type to `Sendable` when "all of their public API is safe to use across concurrency domains"; public non-frozen types get no implicit conformance, so opting in is a deliberate contract. Decide actor isolation early: adding `@MainActor` to shipped API is source- and ABI-breaking. Cancellation is cooperative through the task, not a parameter.
- **Sync and async.** Async variants keep the same base name, no `Async` suffix (SE-0296 rejects the C# convention); overload resolution picks by calling context.
- **Compatibility.** Name the regime first; the rules differ. An app target or source-distributed package answers only for source compatibility: adding a defaulted parameter is routine there, while adding an enum case breaks every exhaustive client switch. A binary framework built with library evolution answers for ABI too, and the traps invert: a parameter cannot be added even with a default, while a non-frozen enum gains cases freely (clients carry `@unknown default`). Direction matters: `public` can become `open` and an immutable property mutable, never the reverses. In either regime, shipped argument labels, parameter order, `throws`, `final`, and `@escaping` are frozen. Check a disputed change against the library-evolution rules rather than memory.

## Idiom

Clarity at the point of use is the ranking rule, and clarity beats brevity. The rules that most often decide a member:

- **Omit needless words**, including words that repeat type information (`allViews.remove(cancelButton)`, not `removeElement`); include every word needed to avoid ambiguity at the use site (`remove(at:)` vs `remove(_:)`).
- **Argument labels make the call a grammatical phrase**: `x.insert(y, at: z)`, `move(from:to:)`. Omit the first label for value-preserving conversions; label an argument that would read wrongly bare (`view.dismiss(animated: false)`); label all defaulted arguments.
- **Name by side effect**: no side effects reads as a noun phrase (`x.distance(to: y)`, `x.sorted()`), side effects as an imperative verb (`x.sort()`, `x.append(y)`); mutating/nonmutating pairs use `sort`/`sorted`, `formUnion`/`union`. Booleans read as assertions (`isEmpty`, `intersects`). Protocols are nouns for what something is, `-able`/`-ible`/`-ing` for capabilities.
- **Prefer defaulted parameters to method families**; prefer methods and properties to free functions; factory methods begin with `make`; document the complexity of a computed property that is not O(1), since property access is assumed cheap.
- If a doc comment for a declaration is hard to write in a simple sentence fragment, "you may have designed the wrong API" — the guidelines' own version of the hard-to-describe test.

## Rulings

- **Structs by default.** A class needs identity, Objective-C interop, or a designed inheritance hierarchy; "prefer protocol inheritance" when building a hierarchy from scratch. Proposing a class where a struct serves is a finding; so is a mutable struct standing for a value with identity.
- **Public, not open.** `open` is the designed-for-subclassing contract (its override surface is API and reviewed as such); default new classes to `public`, which is the reversible choice.
- **Protocol-oriented is not a defence.** The generic seam ruling holds: a protocol with one conforming type, introduced so a test can substitute it, is a finding. Swift culture reaches for protocols early; the seam test, not the culture, decides.
- **Same name, async and sync.** Do not import the `Async`-suffix habit from other platforms.

## Reflexes

- You will propose a protocol for every dependency because that is the ecosystem's testing folklore. Run the hypothetical-seam test first.
- You will suffix async methods with `Async` and wrap failures in `Result`. Swift's channels are bare names and `throws`.
- You will design classes with unlabeled boolean parameters and C#/Java-shaped overload families where labels and defaulted parameters are the Swift shape.
