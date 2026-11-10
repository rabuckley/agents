---
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/*.sln"
---

## C# guidelines

Match the surrounding code for layout and formatting. For names and API shape in
new code, follow the Framework Design Guidelines and the patterns of the BCL,
ASP.NET Core and Microsoft.Extensions, even where the project does otherwise;
don't rename existing members to comply. The rules below are house preferences;
where a project consistently deviates from one, match the project. Comments are
the exception: they follow the rules below however many or few their neighbours
have.

- Use `is` over `==` for null and constant checks, and `is { }` where it reads better than a null test.
- Declare constructors in the type body rather than as primary constructors.
- Prefer records with explicit `{ get; init; }` properties, `required` where it applies, unless the type is essentially a named tuple.

Create projects with `dotnet new`, add packages with `dotnet add package`, and register projects with `dotnet sln add`. Don't hand-edit project or solution files to do those jobs.

Classes internal to a project should be `internal`; `public` members inside an internal class are fine. Testing is not a reason to widen accessibility. Where a test genuinely needs internals, use `InternalsVisibleTo`, and only from a production assembly to its own test project. Between two production assemblies it's an architectural smell.

Avoid the `DTO` suffix. Use `Request`, `Summary` for list items, and `Details` for get-by-id. CQRS commands and handlers are suffixed `Command` and `CommandHandler`.

### XML documentation

Most types and members need no `///` comment. Add one only when the name and types still let a caller misuse or misread the member: a unit, a valid range, what `null` means, a side effect. Improve the name first if that would do. Public visibility alone is no reason to add one. Where an analyzer or documentation build requires a comment, write a one-sentence summary. Put intent, constraints and trade-offs a maintainer must preserve in a `//` comment beside the code.

Read the `csharp-xml-docs` skill before writing or editing a `///` block.

### Testing

- One test project per production assembly.
- xUnit, Arrange/Act/Assert. When a test passes a delegate to an assertion method, define it in the Act section as a local function (`void Act() => sut.Withdraw(-1);`), not a lambda at the assertion call site or in a variable.
- Prefer plain xUnit `Assert` over fluent assertion libraries.
- Reach for property-based tests where the behaviour under test has an invariant worth stating.
