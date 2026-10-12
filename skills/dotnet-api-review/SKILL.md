---
name: dotnet-api-review
description: Review the public surface of a .NET library, type, or API proposal, judging the abstraction first and Framework Design Guidelines idiom second.
disable-model-invocation: true
argument-hint: "[path | type name | git-ref | proposal file]"
disallowed-tools: Edit Write NotebookEdit
---

# .NET API design review

An API is an abstraction: everything a caller must know to use it correctly, which is the signatures plus the ordering rules, exceptions, nullability, threading, ownership, and defaults behind them. The purpose of abstraction is not to be vague, but to create a new semantic level in which one can be absolutely precicse. Its job is to let the caller get a lot done while knowing little, and to hide one design decision that is hard or likely to change (Parnas). The Framework Design Guidelines (FDG) make an abstraction idiomatic; they cannot make it right. Idiomatic is _easy_ in Hickey's sense, familiar and near at hand; the review judges _simple_, unbraided, which is objective and is what the caller lives with. **Making the wrong abstraction idiomatic is not the goal.** So the review runs in three tiers, and a finding in an earlier tier dominates anything later: do not polish the names on a type that should not exist.

1. **Shape**: is this the right abstraction, at the right depth?
2. **Contract**: is what the caller must know beyond the signatures small, explicit, and hard to get wrong?
3. **Idiom**: does it read like the rest of .NET (FDG and modern conventions)?

The review is standing criteria, not steps: apply every test to every suspicion for as long as the review runs. Output is a report; change no files.

## Target

`$ARGUMENTS`

With no argument, run `git fetch origin` and review the public surface changed against the default branch (`origin/HEAD`, else `origin/main`) plus `git diff HEAD`. Start from `git diff --stat <base>...HEAD`, then read the diff per file. A path or type name reviews that surface as it stands; a git ref diffs against it; an API proposal file (the dotnet/runtime template or any sketch) reviews the proposal, and a proposal without usage samples is itself a finding, since scenario code is the instrument. Scope is the public and protected members of public types, plus the documented contract. Always read the whole type, not just the hunk, and read it as a caller would: the member list, the XML docs, and the tests or samples that use it.

Then find the scenarios the API exists to serve: tests, samples, docs, the motivating issue. Confirm scope is non-empty; if it is, say so and stop.

## Ground in the domain

You cannot judge an abstraction for a domain you do not understand: the shape tests ask what is likely to change, what the common case is, and which errors can be designed away, and those are facts about the domain before they are facts about the code. Before writing scenarios, establish three things, and when the domain is outside what you know well (a protocol, a standard, a regulated or mathematical field, a hardware interface), search the web and the docs for them rather than guessing:

- **The constraints.** The invariants, units, orderings, and failure modes the domain imposes: the RFC or spec, the standard's vocabulary, the physical or legal limits. These decide which exceptions are real and which names are right.
- **Prior art.** How the BCL, well-regarded .NET libraries, and the best libraries in other ecosystems shape the same concept. Look beyond .NET on purpose: libraries in languages with expressive type systems (Rust, Swift, Kotlin, F#, Haskell, TypeScript) show which invariants of this domain can be made structural (a sum type for the result, ownership for the buffer, a non-nullable field, a phantom type for a state), and the C# surface should then be asked what it needs to encode the same thing (records, a closed hierarchy, `required`, `readonly struct`). Read all of it for what it reveals: which operations callers reach for, which knobs they actually turn, where the complaints and workarounds cluster. That is evidence about needs, not a template; the best API for this job may not have been designed yet, and prior art is often where the complexity the proposal is meant to design away came from. Judge the proposal on its scenario code; matching or diverging from precedent is neither a finding nor a defence.
- **What varies.** Which decisions in this domain change across users and over time (algorithms, formats, backends, policies) and which are fixed. The varying ones are what the type should hide (Parnas).

Name what you consulted in the report so a reader can check it. A review that skipped this step on an unfamiliar domain should say so.

## The instrument: scenario code

Scenario code is the primary measurement of this review, and what the FDG itself calls scenario-driven design. For each main scenario (two or three; invent them from the type's purpose if none are documented), write the code a caller would write against the API as proposed. Where the type's own scenarios, docs, or motivating issue state a scale, write the scenario at that scale; with no stated scale, do not invent one. Then count what the caller had to know to write it: types touched, members called, ordering constraints, required configuration, exceptions to anticipate, disposal and lifetime. That count is the depth measurement.

Then write the same scenario against the simplest API you can imagine for that job. The gap between the two is either the finding or the evidence that there is none. "Developers should be able to implement main scenarios without reading the documentation" is the bar (FDG); a scenario that needs the docs to get right is a shape finding, and the fix is rarely better docs.

## Shape tests

A suspicion becomes a finding only by surviving every applicable test; one that fails is withdrawn silently. Each test names the artifact a finding must carry so a reader can dispute it.

1. **Depth.** Compare what a caller must know against what the API spares them from knowing. Roughly equal is shallow: the remedy is to collapse or remove, not to document. Artifact: the scenario code and the two counts.
2. **Information hiding.** Name the secret the type keeps: the decision that would be hard or likely to change. If you can only name a step in a process (parse, then validate, then emit), the type is a flowchart stage, not an abstraction. Then name any decision the interface reveals: a storage format, a concrete collection type, a threading model, an ordering the caller must respect, a dependency's type in a signature. Promising more than callers need (Parnas's over-specification) is a finding when changing the decision would force callers to change. Artifact: the secret, or the leaked decision and the member exposing it.
3. **Common case first.** The common scenario must be simple, and rarely used features must not tax it: no setup ceremony before the first useful call (create, set a few properties, call), sensible defaults instead of required configuration, special cases via optional parameters or additional members rather than a wider baseline. Artifact: the member or parameter the common case pays for but never uses.
4. **Errors defined out of existence.** Before accepting an exception, ask whether the design could make the error impossible or the operation total: remove-when-absent is a no-op, buffers grow rather than overflow, a type that cannot be constructed half-built, an enum instead of a string mode. Count the states the surface can represent against the states the domain allows (a nullable plus a flag, two bools spanning four states where three are legal); each extra state is an exception the caller must learn or a redesign. What remains is reported once, by the contract rules below, not by an error code, a magic value, or a swallowed failure. Artifact: the error and the design that removes it, or the two state counts.
5. **Somewhat general.** The interface should be general enough that one method serves several scenarios and simpler than the special-purpose version it replaces, while the implementation serves today's scenarios only. Flag both directions: a special-case mixture baked into a general mechanism, and speculative capability nothing exercises. Name the primitive a general member exposes, the domain operation the scenarios compose from; several conveniences that are each sugar over one unexposed operation collapse into it. Artifact: the scenarios one general member covers, the member nothing calls, or the primitive and the members that collapse into it.
6. **Each layer its own abstraction.** A type whose members forward to another type's members with the same signatures adds interface without adding function. Artifact: the forwarded member pair and what breaks if the layer is deleted (usually nothing: callers call the target).
7. **Hypothetical seam.** An interface or abstract class with one implementation and no API that consumes it is a seam nothing crosses. FDG: ship abstractions only once several implementations and a consumer have tested them. The exception is an abstraction that _is_ the product (a plug-in contract); then it needs a reference implementation and conformance tests. Artifact: the implementation count and consumer count.
8. **Design it twice.** For each new type or abstraction, sketch one materially different shape (a static method instead of a type, a single options object instead of overloads, a struct instead of a class, a method instead of an event) and compare on the scenario code. The alternative becomes the finding only when it is clearly better there; a tie, or "more idiomatic", is not a finding. Artifact: the alternative sketch and the scenario it improves.
9. **Unbraided.** Name what the member complects. The usual braids in an API: value with time (a result that depends on which operations were called before, in an order the caller must learn, or on ambient state and clocks; configuration set before first use, create-set-call, is not this braid and is the FDG's preferred shape); identity with value (a mutable class standing for something that is a value, so equality and copying surprise); two behaviours in one member (a flag parameter); information with behaviour (a type that is plain data wearing methods, so callers cannot treat it as data). Artifact: the two strands and what separating them costs.
10. **Hard to name, hard to describe.** A member whose one-sentence doc comment is hard to write simply and completely, or whose name needs a vague noun (Manager, Helper, Info, Data, Process), is a shape problem. Artifact: the attempted sentence.

When a finding is disputed or needs sharper language than these tests give, read [`references/foundations.md`](references/foundations.md): Parnas, Ousterhout's lecture and book, and Hickey, digested with quotes.

## Contract tests

What the caller must know beyond the signatures. Each is a finding when the caller can get it wrong silently.

- **Errors.** Which exceptions, for which inputs, and can a caller avoid them without try/catch (a Tester-Doer pair like `CanSeek`/`Seek`, or a `TryX` method with `[NotNullWhen(true)]` on the out parameter)? Exceptions are for contract violations and failures, never normal control flow. Reuse `ArgumentNullException`, `ArgumentOutOfRangeException`, `ArgumentException`, `InvalidOperationException`, `NotSupportedException`, `ObjectDisposedException`; throw neither `Exception` nor `SystemException`; a custom exception needs a caller who would catch it specifically.
- **Null and absence.** Nullable annotations are part of the contract. Model absence once: nullable return where absence is normal, Try pattern where the caller tests first, throw where absence is a bug. Collections and arrays are never null: return the empty one.
- **Ownership and mutability.** Who owns a returned collection, buffer, or stream, and may the caller mutate it? Exposed `List<T>` or arrays from properties, mutable structs, and mutable static state are findings.
- **Lifetime and threading.** Which instances are thread-safe, which must be disposed, and whether `Dispose` is idempotent and safe to call twice.
- **Async.** Expose the shape the work is: I/O-bound is async only, compute-bound is sync only; a sync wrapper over async or a `Task.Run` wrapper over sync is a finding. Async members take a `CancellationToken` (last, defaulted) when they can actually cancel, return `Task` unless measurement justifies `ValueTask`, and are never `async void`.
- **Compatibility.** If the surface is already shipped, every remedy states whether it is binary-, source-, or behaviourally breaking. Public surface is forever: you can add an overload, unseal, or add a virtual member later; you can never remove, rename, seal, change a parameter name, or add an abstract or interface member without breaking someone. Design the first version so the safe additions are the likely ones.

## Idiom (FDG)

Apply to every member in scope once the shape is settled. The rules that most often decide shape are inline; the full digest is in [`references/fdg.md`](references/fdg.md), and the post-2008 conventions (async, `Span<T>`, nullable, records, sealing, DI and options, breaking-change rules) in [`references/modern-dotnet.md`](references/modern-dotnet.md). Read the section that matches the surface under review rather than the whole file.

**Types.** Most types are classes. A struct only for a small (under 16 bytes), immutable, single value that is rarely boxed; today that is usually a `readonly record struct`. An abstract class when the abstraction will grow (adding a member to a shipped interface breaks every implementer; adding a non-abstract member to a base class breaks nobody); an interface when value types must implement it or a type already has a base class. Every abstraction ships with at least one implementation and one API that consumes it. No marker interfaces (use an attribute). An enum over string modes, static constants, and any pair of bools; zero means `None`; singular name for simple enums, plural for flags; never for open sets. Plain data is a record. Public nested types and static "bucket" classes are findings.

**Members.** A property is data: cheap, order-independent, no throwing getter, settable in any order with sane defaults. Work, conversion, anything slow, anything that returns a fresh array or object each call, is a method. No public instance fields. Constructors capture parameters (same names as the properties they set) and do minimal work; no virtual calls from a constructor; a static factory when the operation is not plainly "construct". Overloads keep parameter names and order consistent, the longest does the work and is the only virtual one. Parameters take the least-derived type that does the job (`IEnumerable<T>` over `List<T>`), validate at the public surface with `ArgumentException` family plus `paramName`, and avoid `out` and `ref`. Collections: never expose `List<T>`, `Dictionary<K,V>`, or arrays-as-properties; return `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, or `IEnumerable<T>` for forward-only; no settable collection properties. Events use `EventHandler<TEventArgs>` with a protected virtual `OnX(e)` raiser; a callback that is not an event is `Func`/`Action`. Extension methods belong on interfaces or where a dependency rule forbids an instance method, never as a place to put what should be an instance member of a type you own. Operators only on types that should feel primitive, symmetric, with named equivalents.

**Extensibility.** Choose the least powerful mechanism that serves the scenario (unsealed class, then event or callback, then protected virtual, then abstract). A protected member is public for compatibility purposes. Public non-virtual calling protected virtual (`Foo()` / `FooCore()`) keeps the contract in one place. Abstract classes have protected constructors.

**Naming.** PascalCase for everything public, camelCase parameters, no abbreviations or underscores, `I` prefix for interfaces, `T` prefix for type parameters, suffixes for `Exception`, `Attribute`, `EventArgs`, `Collection`, `Async`. Types and properties are nouns, methods verbs, booleans affirmative (`CanSeek`), events present and past tense (`Closing`, `Closed`). Mirror the names the BCL uses for the same concept (`Count` not `Length` for a collection, `TryParse`, `GetEnumerator`). Check naming last.

## Rulings

Where the authorities pull apart, these verdicts hold; do not re-litigate them in a report.

- **Shape over idiom.** An idiomatic pattern (an `IFooService` plus `FooService` plus `AddFoo()` registration, a Template Method pair, an options class) is not evidence the abstraction is right; familiarity is not depth. Run the shape tests on it first and count what the caller must know; idiom is the last tier, not the first.
- **Interfaces are not for testability.** FDG's "prefer classes" and the hypothetical-seam test agree: an interface with one implementation, introduced so a test can mock it, is a finding. Test through the public surface of a concrete type, or inject the real dependency's abstraction where one already exists.
- **Seal unless inheritance is designed.** FDG 2008 said do not seal without a strong reason. The breaking-change rules settle it the other way: unsealing later is free, sealing later breaks derived types, so the sealed version is the reversible choice, and current dotnet/runtime reviews approve new option, result, and service types sealed. If a type is meant to be inherited, its protected and virtual surface is API and is reviewed as such.
- **Overloads versus options.** Overloads exist so the common case is the short call. Use them for a small number of convenience shapes with consistent parameter names and order, with the longest overload doing the work; once optional inputs pass three or four, or will grow, use an options type. Never use `bool` parameters to select behaviour: an enum reads at the call site and can grow.
- **Out parameters.** FDG says avoid `out`/`ref`; the Try pattern is the sanctioned exception, and the only one you should accept.
- **Depth over length.** A long public type with one job and a small surface is fine; a thin type with many members is not. Member count and parameter count are the signals, not lines.
- **Generality versus YAGNI.** Flag speculative capability (members nothing calls). Do not flag a general interface that is simpler than the special-purpose one it replaces.
- **Prior art is evidence, not precedent.** "The BCL does it with events" or "FDG prefers events over callbacks" does not make a simpler shape (a value-returning method, an `IAsyncEnumerable<T>`, a record) wrong; FDG's idiom rules date from 2008 and the consistency they buy is worth less than a scenario the caller cannot get wrong. Flag divergence only when the scenario code is worse for it, and say what the caller loses.

## Report

Rank findings by lifetime cost, worst first: shape findings before contract, contract before idiom. Each finding:

```
N. Type.Member — [Shape|Contract|Idiom] one-line claim
   Evidence: <the test's artifact: scenario code, counts, the leaked decision,
             the alternative sketch, the FDG rule by name>
   Remedy: <the change; deletion or collapse is first-class>
   Breaking: <none | source | binary | behavioural> (only if already shipped)
```

Lead with the outcome: the count and the single worst finding, then the list. **"The surface holds: no findings" is a correct and common verdict**; deliver it without padding nits or praise. The bar for a finding is that you would block the API from shipping over it, or at least require a reply before approving: public surface is forever, so the bar is lower than for implementation code, but "I would have named it differently" does not clear it, and neither does a hazard that needs a cast, reflection, or a deliberate misuse to reach (a read-only interface over a private array is the normal BCL shape, not a finding). What the caller can get wrong by writing the obvious code is the test.

## Gotchas

Corrections to the mistakes this reviewer makes by default:

- You will start with naming because it is the most legible rule set. Names are the last tier. Write the scenario code first; most shape findings fall out of it.
- You will propose adding an interface, an abstraction, a wrapper, or a doc comment, because additions feel safe. The shape remedy is usually a removal or a collapse.
- You will read the implementation and review it. Implementation matters here only where it leaks into the contract: the exceptions it throws, the state it shares, the cost a caller cannot predict. Where the body is a stub, reason from the documented contract, not from what the stub omits.
- You will import scenarios from prior art that the type never claimed to serve (the BCL's richer result type, its extra knobs, its queue limit) and call their absence a finding, often dressed as "silently fails under load". "The BCL makes it mandatory" is precedent, not evidence. A missing capability is a finding only when one of the type's own scenarios is wrong or impossible without it; a smaller surface that serves its scenarios is the goal, not a gap. Load the caller creates by how they call (how many waiters they spawn, how large a buffer they pass) is the caller's to bound, as it is for `SemaphoreSlim` or `Channel<T>`. A missing limit or capacity knob is a finding only when the type grows an internal resource the caller can neither observe nor bound.
- When nothing clears the bar, you will fill the report with idiom nits at uniform severity. Return the clean verdict instead.
