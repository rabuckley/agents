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

### XML documentation

Use triple-slash comments for domain models, API DTOs consumed by generated clients, and the supported public surface of libraries. Cover the types and relevant members, including properties and enum values, with domain meaning and consumer contracts. Internal domain models are included; persistence-only records are not included merely because they store domain data. Public accessibility alone does not make application or test code a supported library surface.

For other code, including test classes, fixtures and helpers shared between test projects, write comments where they explain non-obvious intent, constraints or trade-offs that a maintainer needs to preserve.

Read the `csharp-xml-docs` skill before writing or editing a `///` block. It defines the house style; contract accuracy takes precedence over wording templates.

### Testing

- One test project per production assembly.
- xUnit, Arrange/Act/Assert. When a test passes a delegate to an assertion method, define it in the Act section as a local function (`void Act() => sut.Withdraw(-1);`), not a lambda at the assertion call site or in a variable.
- Prefer plain xUnit `Assert` over fluent assertion libraries.
- Reach for property-based tests where the behaviour under test has an invariant worth stating.
