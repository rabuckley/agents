# .NET 11 SDK and tooling

Source: [What's new in the SDK and tooling for .NET 11](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-11/sdk). The targets here are build files, scripts, CI pipelines and docs, not C# code.

## Retargeting

- `global.json` `sdk.version` → an 11.0.1xx SDK. Keep `rollForward` as the repo has it.
- `<TargetFramework>` → `net11.0`, ideally once in `Directory.Build.props`. Remove `<LangVersion>` pins of `14` or lower unless a project deliberately stays behind; C# 15 is the default for `net11.0`.
- `AnalysisLevel=latest` previously resolved to .NET 9 rules; it now resolves to .NET 11 rules. Expect new analyzer diagnostics after retargeting and treat them as findings, not noise.
- CA1873 (expensive logging arguments) is quieter: property access, `GetType()`, `GetHashCode()`, `GetTimestamp()` aren't flagged, and only Information level and below is checked. `#pragma warning disable CA1873` or `.editorconfig` suppressions added to silence false positives can often be removed. Re-run the build to confirm.
- `#:property Foo:Bar=value` in file-based apps is now an error: use a valid MSBuild property name.
- `NETSDK1235`: `PackAsTool=true` with a custom `NuspecFile` now warns. Remove `NuspecFile` from tool projects.

## Scripts and CI

| Signal | Replace with |
| --- | --- |
| `export ASPNETCORE_ENVIRONMENT=... && dotnet run`, or a launch profile added only to set env vars | `dotnet run -e KEY=VALUE` |
| Scripts piping `dotnet run` stdout through `grep -v "Using launch settings"` | Drop the filter; the notice is on stderr now. |
| Hand-edited `.slnf` JSON | `dotnet new slnf --parent-solution X.slnx --name Tests`; `dotnet sln Tests.slnf add/remove/list` |
| `dotnet reference add --project .` / `cd` + `--project` dance | Omit `--project`; it defaults to the current directory. |
| CI test steps with a job-level timeout or a custom fail-fast wrapper | `dotnet test --timeout 90s` (exit code 3), `--maximum-failed-tests N` (exit code 13), before `--` (MTP only) |
| Test result files overwriting each other across multi-target or multi-project runs | `--results-directory-layout per-module` |
| `dotnet build --no-dependencies` followed by `dotnet test --no-build` to skip P2P builds | `dotnet test --no-dependencies` (MTP) |
| Switching test runner by editing `global.json` in CI | `DOTNET_TEST_RUNNER=VSTest` or `Microsoft.Testing.Platform` |
| Traversal `dirs.proj` with a custom loop to run tests | `dotnet test dirs.proj` |
| Container publish with differing digests between identical builds | `-p:SOURCE_DATE_EPOCH="$(git log -1 --pretty=%ct)"` |
| Multi-arch container builds forced onto Docker because Podman couldn't do them | Podman now supports multi-arch SDK container builds. |
| `nuget.config` edited by hand for `minPublishAgeHours` | `dotnet nuget add/update source ... --min-publish-age-hours N` |
| `dotnet format` with an `is_global = true` `.editorconfig` in an unrelated subtree | `.globalconfig`; folder mode no longer scans unrelated subtrees. |
| Multi-targeted projects formatted per framework by hand | `dotnet format --framework <tfm>` |

The MSBuild server and NativeAOT CLI host are on by default. If CI shows stale-build or locked-file symptoms after the upgrade, `DOTNET_CLI_USE_MSBUILD_SERVER=false` (or `MSBUILDUSESERVER=0`) and `DOTNET_CLI_ENABLEAOT=false` are the opt-outs. Don't add them pre-emptively.

## Test templates

New xUnit projects can use v3 on Microsoft.Testing.Platform: `dotnet new xunit --xunit-version v3`. NUnit: `dotnet new nunit --test-runner Microsoft.Testing.Platform`. Moving existing suites from VSTest to MTP is its own migration; report it, don't fold it into this one.

## File-based apps

Applies only to repos with `#:` directive scripts (`dotnet run app.cs`).

- Copy-pasted helpers across scripts → `#:include helpers.cs`. Duplicate `#:sdk`, `#:property` and `#:package` directives with matching values are now allowed across included files.
- Scripts that shell out to `csc` or wrap a project only to reference a prebuilt DLL → `#:include ./libs/MyLibrary.dll`.
- `#:property` values with spaces can be quoted; `#:package`/`#:project`/`#:ref` accept `Name=Value` metadata such as `Aliases=CommandLine`.
- `dotnet reference add --file app.cs <project>` adds `#:project` directives.
- `dotnet format app.cs` formats a script.
