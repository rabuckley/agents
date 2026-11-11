---
name: dotnet-11
description: >
  Audits a .NET codebase for places to adopt .NET 11 features (C# 15, the
  .NET 11 libraries, ASP.NET Core 11, EF Core 11, the .NET 11 SDK) and
  applies the ones that fit. Use when retargeting or upgrading to net11.0,
  when asked to modernise .NET or C# code, or when asked whether a codebase
  uses what .NET 11 offers, such as union types, closed hierarchies or
  collection expression arguments. Not for upgrades to other .NET versions.
---

# Adopt .NET 11

Find every place where a .NET 11 feature makes existing code simpler, safer or faster, apply the ones that clearly fit, and report the rest. A feature earns its place by **fit**: it removes code, removes a hand-rolled workaround or a dependency, turns a runtime failure into a compile-time one, or measurably speeds up a path that matters. A rewrite that only swaps one spelling for a newer one is churn; leave it out.

The references were written against the .NET 11 RC 2 documentation. If an API named there doesn't exist in the installed SDK, follow the source link at the top of that reference before using it.

## 1. Establish the baseline

Read `global.json`, `Directory.Build.props`, `Directory.Packages.props` and every project file. Record, per project: target framework(s), SDK (`Microsoft.NET.Sdk`, `.Web`, `.Razor`, `.BlazorWebAssembly`), any `LangVersion`, the EF Core provider if any, and the test runner.

If the projects don't target `net11.0` yet, retarget first using the **Retargeting** section of [`references/sdk-11.md`](references/sdk-11.md), then build and test. Feature adoption starts from a green build on `net11.0`; fix what the retarget breaks before going further.

In a multi-targeted library, older targets default to older C# versions; they get C# 15 syntax only if the project sets `LangVersion` explicitly. Even then, .NET 11 APIs and union types (which need runtime types) are available only on `net11.0`: guard them with `#if NET11_0_OR_GREATER` or leave them out. Build every target to confirm any other C# 15 feature works on the older ones. Note which projects are multi-targeted before the sweep.

**Done when** every project has a row and the solution builds on `net11.0`.

## 2. Load the references that apply

| Reference | Load when |
| --- | --- |
| [`references/csharp-15.md`](references/csharp-15.md) | Always. |
| [`references/libraries-11.md`](references/libraries-11.md) | Always. |
| [`references/sdk-11.md`](references/sdk-11.md) | Always: build files, CI and scripts. Its file-based apps section only when the repo has `#:` directive scripts. |
| [`references/aspnetcore-11.md`](references/aspnetcore-11.md) | A project uses `Microsoft.NET.Sdk.Web`, `.Razor` or `.BlazorWebAssembly`, or references `Microsoft.AspNetCore.*`. |
| [`references/efcore-11.md`](references/efcore-11.md) | A project references `Microsoft.EntityFrameworkCore*`. Read only the provider sections that match. |

Read each loaded reference in full before sweeping; each entry's **fit** rules decide what you'll do with its hits.

## 3. Sweep

For every entry in every loaded reference, search the codebase for its signals: `*.cs`, `*.razor`, project and props files, CI and scripts, as the entry says. Skip generated code (`obj/`, `bin/`, `*.g.cs`, `*.Designer.cs`). Rows in a **Behaviour changes** table are entries too: record where the codebase is exposed, even though the remedy may be no change. Judge each hit against the entry's fit rules and record it:

```
<file>:<line>  <feature>  <apply | propose | report>  <one-line reason>
```

Run the sweep in parallel subagents when the codebase is large, one per reference, each given the reference path and told to return this list.

**Done when** every entry of every loaded reference has either candidate hits or an explicit "no hits". A sweep that skipped entries isn't finished.

## 4. Sort each candidate

- **Apply**: behaviour-preserving and local. The new code does what the old did with less code or a stronger compile-time check: collection expression arguments, labeled jumps replacing flags, `closed` on an internal abstract base and its now-dead discard arms, a new BCL API replacing a hand-rolled helper with identical semantics, `MaxByAsync` replacing `OrderByDescending().First...`.
- **Propose**: a change that alters a public API, a wire format, a database schema, generated SQL, a dependency or runtime behaviour. Unions replacing `OneOf` in public signatures, owned types to complex types, `closed` on a public type, a new default you'd opt out of. Describe the trade-off and wait for the user.
- **Report**: preview or experimental features, and anything whose fit needs measurement you can't take. Leave the code alone.

When the user asked for an audit only, treat every candidate as report and skip step 5.

## 5. Apply

Work one feature at a time across the codebase, so each change reviews as one idea. After each feature: build with warnings as the repo configures them, and run the tests. For anything touching EF Core, also run `dotnet ef migrations add Check<Feature>` and confirm the generated migration is empty when the change was meant to be model-neutral; delete it afterwards. Follow the repo's `.editorconfig` and existing style over the reference snippets.

If a change breaks the build or a test and the fix isn't obvious, revert it and move the candidate to **propose** with what failed.

## 6. Report

Group by area (C#, libraries, ASP.NET Core, EF Core, SDK/CI), then by apply, propose, report:

- **Applied**: feature, files changed, build and test result.
- **Proposed**: feature, locations, the trade-off, what the user must decide.
- **Reported**: feature, locations, why it's held back (preview, needs measurement, owner's call).
- **Behaviour changes found**: every breaking or default-behaviour change from the references that the codebase is exposed to, with the location and the mitigation applied or needed.

If a loaded reference produced no candidates, say so in one line, so the reader knows it was checked.
