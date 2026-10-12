---
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/*.sln"
---

## C# guidelines

Write code that reads like the code around it. The rules below are the house
preferences a project's own files won't tell you; where a project consistently
deviates from one, match the project.

- Use `is` over `==` for null and constant checks, and `is { }` where it reads better than a null test.
- Declare constructors in the type body rather than as primary constructors.
- Prefer records with explicit `{ get; init; }` properties, `required` where it applies, unless the type is essentially a named tuple.

Create projects with `dotnet new`, add packages with `dotnet add package`, and register projects with `dotnet sln add`. Don't hand-edit project or solution files to do those jobs.

Classes internal to a project should be `internal`; `public` members inside an internal class are fine. Testing is not a reason to widen accessibility. Where a test genuinely needs internals, use `InternalsVisibleTo`, and only from a production assembly to its own test project. Between two production assemblies it's an architectural smell.

Avoid the `DTO` suffix. Use `Request`, `Summary` for list items, and `Details` for get-by-id. CQRS commands and handlers are suffixed `Command` and `CommandHandler`.

### Public XML documentation

Document every public API with triple-slash comments, following the BCL, `Microsoft.Extensions.*`, and ASP.NET Core. The house style has fixed wording for summaries, boolean params and returns, `out` and `ref` params, task returns, enum values, exception conditions, and crefs, and it is easy to get subtly wrong: read the `csharp-xml-docs` skill before writing or editing a `///` block.

### Testing

- One test project per production assembly.
- xUnit, Arrange/Act/Assert. When a test passes a delegate to an assertion method, define it in the Act section as a local function (`void Act() => sut.Withdraw(-1);`), not a lambda at the assertion call site or in a variable.
- Prefer plain xUnit `Assert` over fluent assertion libraries.
- Reach for property-based tests where the behaviour under test has an invariant worth stating.
