# EF Core 11

EF Core 11 requires the .NET 11 runtime; it doesn't run on earlier .NET or .NET Framework. Source: [What's new in EF Core 11](https://learn.microsoft.com/ef/core/what-is-new/ef-core-11.0/whatsnew). Read the provider section that matches the project's `Use*` call (`UseSqlServer`, `UseSqlite`, `UseCosmos`) and skip the others.

Many EF 11 gains are free: better SQL for to-one includes (pruned split-query joins, fewer `ORDER BY` keys), `GroupBy` translation, no-op `CAST` stripping, and SQL Server vector columns no longer loaded by default. They need no code change but can change generated SQL, so note them for the verification step.

## Behaviour changes to check first

- **SQL Server compatibility level now defaults to 160 (SQL Server 2022).** `LEAST`/`GREATEST` and other 2022 translations are emitted by default. If production runs an older SQL Server or Azure SQL at a lower level, set `UseCompatibilityLevel(...)` explicitly. Search: `UseSqlServer(` without `UseCompatibilityLevel`.
- **SQL Server vector properties (`SqlVector<T>`) are no longer in entity `SELECT`s.** Code that reads `entity.Embedding` after a plain query now sees no value. Search: `SqlVector<`; project the column explicitly (`Select(b => new { b.Id, b.Embedding })`) where it's read back.
- **Cosmos: `Newtonsoft.Json` replaced by `System.Text.Json`.** The `__jObject` shadow property is gone and unmapped JSON properties aren't preserved on round-trip. Search: `"__jObject"`, `JObject`, `EF.Property<JObject>`.
- **Cosmos: projecting an `undefined` path now always throws `InvalidOperationException`.** Guard with `EF.Functions.IsDefined(...)` or `EF.Functions.CoalesceUndefined(...)`.
- **Cosmos: `SaveChanges` uses transactional batches by default** (`AutoTransactionBehavior.Auto`). Set `Never` to restore pre-11 behaviour.

Full list: [EF Core 11 breaking changes](https://learn.microsoft.com/ef/core/what-is-new/ef-core-11.0/breaking-changes).

## Modelling

### Complex types replace owned types

Complex types are now stable enough to replace owned entity mappings, and work with TPT/TPC inheritance, JSON columns, Cosmos, keys and indexes.

**Signals**: `OwnsOne(`, `OwnsMany(`, `[Owned]`; owned types on TPT/TPC hierarchies (`UseTptMappingStrategy`, `UseTpcMappingStrategy`); `OwnsOne(...).ToJson()`.

```csharp
modelBuilder.Entity<Animal>()
    .UseTptMappingStrategy()
    .ComplexProperty(a => a.Details, b => b.ToJson());
```

**Fit**: complex types have value semantics and no identity. Switch only when the owned type has no key of its own and nothing compares it by reference. The change alters the model snapshot and can alter column names; generate a migration and read it before applying. Treat as a separate, reviewed change, never a drive-by.

### Simpler complex-type configuration

```csharp
// Before
modelBuilder.Entity<MyEntity>().ComplexProperty(e => e.Details).Property(d => d.Description).HasMaxLength(500);
// After
modelBuilder.Entity<MyEntity>().Property(e => e.Details.Description).HasMaxLength(500);
```

### Keys and indexes on complex-type properties

`HasKey(c => c.CustomerId.Value)`, `HasIndex(c => c.Address.PostalCode)`, `HasIndex("Items[].Sku")` for complex collections in JSON. **Signals**: indexes created with raw SQL in migrations (`migrationBuilder.Sql("CREATE INDEX` / `CREATE JSON INDEX`) on columns that belong to a complex type. On SQL Server, an index on a JSON-mapped path with `HasColumnType("json")` emits `CREATE JSON INDEX`.

### Temporal period columns as CLR properties

**Signals**: `EF.Property<DateTime>(e, "PeriodStart")` / `"PeriodEnd"`, `IsTemporal(`.

```csharp
b.IsTemporal(t =>
{
    t.HasPeriodStart(e => e.PeriodStart);
    t.HasPeriodEnd(e => e.PeriodEnd);
});
```

Then query `e.PeriodStart` directly.

### Foreign keys without database constraints

**Signals**: migrations that call `DropForeignKey` right after creating a table, or relationships left unmodelled only to avoid the constraint.

```csharp
.HasForeignKey(e => e.BlogId).ExcludeForeignKeyFromMigrations();
```

The index on the FK column is still created.

## Queries

| Signal | Replace with | Notes |
| --- | --- | --- |
| `OrderByDescending(x => k).FirstOrDefaultAsync()` / `OrderBy(...).First...` used to pick one element by key | `MaxByAsync(x => k)` / `MinByAsync(x => k)` | Same SQL shape (`TOP(1) ... ORDER BY`), clearer intent. |
| Two queries merged in memory, or `GroupJoin`+`SelectMany`+`DefaultIfEmpty` unioned with the reverse join, to emulate a full outer join; `FromSql` with `FULL JOIN` | `FullJoin(inner, outerKey, innerKey, resultSelector)` | Translates to `FULL JOIN` on relational providers. Also new in LINQ to Objects. |
| `FromSql`/`FromSqlRaw` containing `JSON_PATH_EXISTS` | `EF.Functions.JsonPathExists(col, "$.path")` | SQL Server 2022+. |
| `FromSql` containing `JSON_CONTAINS` | `EF.Functions.JsonContains(col, value, "$.path")` | SQL Server 2025. |
| `Contains` on a primitive collection column | No change; set `UseCompatibilityLevel(170)` when on SQL Server 2025 | Translates to `JSON_CONTAINS` instead of `OPENJSON`. |
| `FromSql` with `FREETEXTTABLE`/`CONTAINSTABLE` | `FreeTextTable(...)` / `ContainsTable(...)` returning `FullTextSearchResult<T>` with `Rank` | |
| `migrationBuilder.Sql("CREATE FULLTEXT ...")` | `modelBuilder.HasFullTextCatalog(...)`; `HasFullTextIndex(...).UseKeyIndex(...).UseCatalog(...)` | Generate a migration and check it's a no-op against the existing database. |
| `FromSql` with `VECTOR_SEARCH`; raw `CREATE VECTOR INDEX` | `VectorSearch(...).OrderBy(r => r.Distance).Take(k).WithApproximate()`; `HasVectorIndex(...)` | **Experimental** in SQL Server and EF; report, don't adopt unasked. |
| `FromSql` with `DATETRUNC`, `SWITCHOFFSET`, `TODATETIMEOFFSET`; client evaluation of `DateTimeOffset.UtcDateTime`/`.DateTime`/`.Offset.TotalMinutes` | `EF.Functions.DateTrunc(...)`, `dto.ToOffset(...)`, `new DateTimeOffset(dt, offset)`, the properties directly | SQL Server. |
| SQLite: `string.Join` over an ordered navigation evaluated on the client (`AsEnumerable()` before it) | `string.Join(", ", b.Posts.OrderByDescending(p => p.Id).Select(p => p.Title))` in the query | Needs SQLite 3.44+. |
| `ToAsyncEnumerable()` on an EF query | `AsAsyncEnumerable()` | EF 11 ships an analyzer and code fix for this. |

Prefer replacing `FromSql` only when the LINQ form expresses the whole query; a raw query that does more than the one function stays.

## Cosmos DB

- Owned types → complex types (see above).
- Manual session-token plumbing across instances → `SessionTokenManagementMode(SessionTokenManagementMode.SemiAutomatic)`, `Database.GetSessionToken()`, `Database.UseSessionToken(token)`.
- Bulk inserts done through the raw `CosmosClient` with `AllowBulkExecution` → `options.BulkExecutionEnabled()` on `UseCosmos`.
- Indexing policy set out-of-band (portal, ARM/Bicep) → `HasAutomaticIndexing().Except(...)`, `HasIndex(...)` in the model. Only move it if the team wants the model to own the policy.

## Migrations and tooling

- **`.config/dotnet-ef.json`**: if scripts, READMEs or CI repeat `--project`/`--startup-project`/`--context`, add the config file and shorten the commands. Paths resolve relative to the parent of `.config`.
- **`--context "*"`** for `migrations list`, `migrations script`, `database update`, `database drop`: replaces per-context loops in scripts.
- **`dotnet ef database update <Name> --add`**: creates, compiles and applies in one step. Useful in Aspire/container dev loops; keep CI on explicit `migrations add` + review.
- **`migrations remove --offline` / `--connection`**.
- The model snapshot now records the latest migration ID, so migrations added on divergent branches conflict in source control. Remove any custom CI check that detected divergent migration trees only if it did nothing more than that.
- `dotnet ef` now writes logs to stderr and only the result to stdout. Scripts that strip log lines from `migrations script` output can drop that filtering.
