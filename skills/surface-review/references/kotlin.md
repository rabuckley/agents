# Kotlin pack

Sources: the Kotlin library authors' guidelines (kotlinlang.org/docs/api-guidelines-introduction.html and its sub-pages), the coding conventions (kotlinlang.org/docs/coding-conventions.html), the explicit-API-mode KEEP, and Kotlin/binary-compatibility-validator. Consult them when a finding needs the verbatim rule. The primary sources are thin on suspend functions and Flow in public API; say so rather than inventing rules.

## Surface

Every public declaration, which in Kotlin happens by default: "public-by-default visibility" makes accidental API the platform's signature hazard, and explicit API mode (explicit visibility and explicit return types for public declarations) is the fix. `@PublishedApi internal` and the bodies of public inline functions are public surface too. A data class's surface includes its constructor, `copy()`, and `componentN()` functions, which is why it is dangerous to ship (see rulings).

## Contract expressions

- **Errors.** Four sanctioned mechanisms, used consistently across the library: a nullable return for missing data, with the `x`/`xOrNull` naming pair carrying the contract; exceptions for genuine errors, never control flow, with the design allowing condition checks before the attempt; and `kotlin.Result` or a custom result type where a value-or-failure return fits, the `...Catching` suffix naming the wrapping half of a throwing pair. Validate with `require()` (IllegalArgumentException) and `check()` (IllegalStateException), messages naming the offending input; adopt one documented exception policy, wrapping low-level exceptions in a library-specific one with the original as `cause`.
- **Absence.** Nullable types are the contract, made deliberate by explicit API mode; `single`/`singleOrNull` is the naming template. Collections return empty, not null.
- **Ownership and mutability.** Accept and return the read-only collection interfaces (`List`, `Set`, `Map`, `Collection`), always; avoid arrays in the API, and copy defensively if unavoidable; avoid exposing mutable state at all.
- **Lifetime and concurrency.** Cleanup the caller must drive implements `AutoCloseable`, so callers get `use {}`. On suspend and Flow design the primary sources are quiet; hold to the generic tests. For multiplatform, `expect`/`actual` pairs must behave identically on every platform: same valid inputs, same results, same errors, with any unavoidable difference documented.
- **Sync and async.** No primary-source rule; apply the generic shape-of-the-work test.
- **Compatibility.** Three kinds: binary, source, behavioural, and they break independently. The traps: adding a parameter with a default value is source-compatible but binary-breaking; narrowing a return type is source-compatible but binary-breaking, widening breaks both, so never change return types; inferred types drift when the implementation changes, which is why explicit API mode demands written types; adding a property to a data class changes the constructor and `copy()` signatures. Deprecate across minor releases through `@Deprecated` levels WARNING then ERROR then HIDDEN, with `ReplaceWith`; remove only in a major release. A shipped library runs the binary-compatibility validator (`apiDump`/`apiCheck`) so the `.api` file diff is the review artifact.

## Idiom

- **Members are the core concept; everything else is an extension.** "Only regular properties, overrides, and overloaded operators should be declared as members by default" — `CharSequence` has three members and an ecosystem of extensions. Restrict extension visibility to keep API pollution down.
- **Composability over configuration parameters.** Chainable functions (Flow's `buffer`, `conflate`) or a single modifier-style parameter beat a parameter list that grows a flag per feature. Build a small set of core abstractions and layer conveniences on top (Flow's operators on `transform`).
- **A property** does not throw, is cheap (or cached), and returns the same result while state is unchanged; otherwise it is a function.
- **No boolean arguments**: `doWork(true)` reads as nothing at the call site; separate named functions (`map`/`mapNotNull`) or an enum. Named arguments where parameters share a primitive type.
- **Sealed hierarchies** close an extension point so `when` is exhaustive (`JsonElement`); offer extension "in clearly specified ways" and prevent the rest. **Value classes** give IDs and units their own type without cost; arithmetic types are for arithmetic.
- **DSLs**: a builder function taking a lambda-with-receiver last; required values as parameters, optional state in the lambda; adding a builder property later is non-breaking.
- **Naming**: classes are nouns in UpperCamelCase, functions verbs in lowerCamelCase; no `Util`, `Manager`, `Wrapper`; one word per concept across the library (`element` or `item`, not both); parameters ordered general to specific.

## Rulings

- **Defaults versus overloads splits by distribution.** The coding conventions prefer defaulted parameters; the compatibility rules make them binary-breaking to add or change. Verdict: defaulted parameters for source-distributed and application-level surface; explicit overloads (or `@JvmOverloads` with its Kotlin-caller caveat, or the experimental `@IntroducedAt`) for a binary-stable library.
- **No data classes in shipped public API.** The convenience is the breakage: constructor, `copy()`, and `componentN()` all move when a property is added. A normal class with explicit `equals`/`hashCode`/`toString` is the stable shape; provide a meaningful `toString` either way.
- **Factory naming.** Prefer a distinct, descriptive name that says why the factory is special; the type-named, capitalized factory is for returning an abstract type, DSL-style construction, or a factory with genuinely no special semantics.
- **One error mechanism per situation.** Pick from the four sanctioned mechanisms by case (absence → nullable, genuine error → exception) and use the same choice for the same situation everywhere; mixing channels for one operation is a finding under the generic errors test.

## Reflexes

- You will ship data classes because they are one keyword. In public API the keyword is the liability.
- You will let things default to public. Ask what explicit API mode would have forced; accidental surface is a finding.
- You will add a defaulted parameter to a shipped library function and call it non-breaking. It is binary-breaking.
- You will write Java-shaped builders and setters where defaulted parameters, named arguments, or a DSL lambda are the Kotlin shape.
