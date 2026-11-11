# ASP.NET Core 11

Source: [What's new in ASP.NET Core in .NET 11](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-11). For the step-by-step upgrade (NavMenu JS module, antiforgery, `ValidatableType`), use the [10.0 to 11.0 migration guide](https://learn.microsoft.com/aspnet/core/migration/100-to-110).

Read **Behaviour changes** first, then only the areas the app uses: Blazor sections for `.razor` files, Minimal APIs for `Map*` endpoints, MVC notes for controllers, SignalR for `Hub` subclasses, OpenAPI for `AddOpenApi`.

## Behaviour changes to check first

These change runtime behaviour without a code change. For each one the codebase is exposed to, confirm the new behaviour is wanted or apply the opt-out.

| Change | Exposed if | Mitigation |
| --- | --- | --- |
| `IAuthorizationRequirementData` attributes are now **enforced** on MVC controllers/actions, SignalR hubs/methods, `AuthorizeView` and `AuthorizeRouteView`. | Custom attributes implementing `IAuthorizationRequirementData` used outside Minimal APIs. | Security-relevant: attributes that were silently ignored now apply. Confirm each is intended; delete duplicate filters or policies that mirrored them. |
| Endpoint filters run even when parameter binding fails. | Minimal API endpoints with `AddEndpointFilter`. | Filters must tolerate unbound arguments; they can check `StatusCode == 400` and rewrite the body. |
| OpenAPI documents target 3.2 by default; `Microsoft.OpenApi` 3.x has breaking changes. | `AddOpenApi` with custom `IOpenApiDocumentTransformer`/`IOpenApiSchemaTransformer`/`IOpenApiOperationTransformer`, or clients and generators that need 3.0/3.1. | Fix transformers per the Microsoft.OpenApi upgrade guide; pin `options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1` if downstream tooling needs it. |
| OpenAPI schema output changes: array schema IDs become `stringArray`, `TodoArray`; non-body enum parameters keep C# names; multiple `Produces` per status code are all kept; `[Obsolete]` emits `deprecated: true`. | Snapshot tests of the document, generated clients. | Regenerate and review the diff. |
| Zstandard is enabled by default in response compression and request decompression; `Vary: Accept-Encoding` always emitted. | `AddResponseCompression`/`AddRequestDecompression`. | Usually wanted. Drop third-party zstd providers. |
| OpenTelemetry semantic-convention attributes (`http.request.method`, `url.path`, ...) added to the request activity by default. | Tracing configured. | Opt out with AppContext switch `Microsoft.AspNetCore.Hosting.SuppressActivityOpenTelemetryData`. |
| Validation localization turns on automatically when both `AddLocalization` and `AddValidation` are registered; `ErrorMessage` is tried as a resource key first. | Both registered. | Check that no `ErrorMessage` string collides with a resource key. |
| Identity reads time from the registered `TimeProvider`. | A `TimeProvider` singleton registered for other reasons. | Usually wanted; check a fake provider isn't registered outside tests. |
| SignalR .NET client: authentication auto-refresh on by default. | `HubConnectionBuilder` in .NET clients. | Usually wanted. |
| Blazor: `Virtualize.OverscanCount` default 3 → 15. | `<Virtualize>` without `OverscanCount`. | Set it explicitly if the old value mattered. |
| Blazor: QuickGrid paging and sorting live in the query string; headers and paginator render as `<a>`. | `<QuickGrid>`. | CSS: add `a.col-title`, `nav a`, `nav a[aria-disabled="true"]` beside the `button` selectors. Several grids on one page each need a distinct `QueryParameterNameOptions` and their own `PaginationState`. Renaming a column `Title` breaks bookmarked sort URLs. Opt out of links (not URL state) with `AppContext.SetSwitch("Microsoft.AspNetCore.Components.QuickGrid.EnableUrlBasedQuickGridNavigationAndSorting", false)`. |
| Blazor: client prerendering uses the server's culture. | `.Client` project with custom culture plumbing. | Delete the plumbing, or opt out with `UseCultureFromServer = false`. |
| Blazor: static SSR forms with `DataAnnotationsValidator` validate in the browser. | SSR `EditForm`s. | Remove jQuery unobtrusive validation added for them. |
| Blazor: antiforgery is injected automatically and validation is deferred to middleware. | `app.UseAntiforgery()` in a Blazor Web App. | Remove the call unless the migration guide's exceptions apply. |
| Blazor: session cookie issued before streaming on pages using `[SupplyParameterFromSession]` or session TempData. | Cookie-consent-sensitive apps. | Check consent flow. |

**Obsolete or deprecated**: `HttpsConnectionAdapterOptions.TlsClientHelloBytesCallback` → `listenOptions.UseTlsClientHelloListener(...)`, called before `UseHttps()`. `Microsoft.AspNetCore.Grpc.Swagger` / `AddGrpcSwagger` is deprecated with no named replacement; report it.

**New analyzer warnings**: BL0010 flags `JSRuntime.InvokeAsync<object>(...)` whose result is ignored; replace with `InvokeVoidAsync`. `ASP0029` suppressions (`#pragma warning disable ASP0029`, `<NoWarn>ASP0029`) are now redundant because `[ValidatableType]`/`[SkipValidation]` are stable; remove them.

## Minimal APIs and MVC

| Signal | Replace with | Fit |
| --- | --- | --- |
| `OpenTelemetry.Instrumentation.AspNetCore` package, `.AddAspNetCoreInstrumentation()` | `.AddSource("Microsoft.AspNetCore")` on `WithTracing` | Check the instrumentation package's options (filters, enrichment) aren't in use before removing it. |
| Async work inside endpoint handlers to validate input (uniqueness checks); `.Result`/`.GetAwaiter().GetResult()` inside `ValidationAttribute.IsValid`; FluentValidation `MustAsync` used only with Minimal APIs | `AsyncValidationAttribute.IsValidAsync(value, context, ct)` or `IAsyncValidatableObject.ValidateAsync(...)` with `AddValidation()` | Throw from the sync `IsValid`/`Validate` so an async-only validator is never silently skipped. Validators may run concurrently; they must be thread-safe. Don't migrate off FluentValidation wholesale unasked. |
| `ErrorMessageResourceType`/`ErrorMessageResourceName` on every attribute, custom adapters localizing `Microsoft.Extensions.Validation` messages | `AddLocalization()` + `AddValidation()`; `ValidationOptions.LocalizerProvider`; `IValidationMessageFormatter` for attributes with extra arguments | Key convention: `{Type}_{Member}_{Attribute}_Error`, then `{Type}_{Attribute}_Error`, then `{Attribute}_Error`. |
| MVC controllers for health or `robots.txt` that run the whole pipeline; `.ShortCircuit()` on Minimal APIs (still fine) | `[ShortCircuit]` / `[ShortCircuit(404)]` on controllers, actions or lambdas | Skips authentication, CORS and all later middleware; only for anonymous endpoints. |
| `OneOf<...>` or a `[JsonPolymorphic]` hierarchy used in a JSON body only to mean "one of N"; endpoints returning `object` | C# 15 `union` types; OpenAPI emits `anyOf`; `[JsonUnion]` classifies ambiguous shapes | Body serialization only, not route/query/header/form binding. SignalR needs the JSON protocol. A wire-format change: **propose**. |
| `IAsyncEnumerable<SseItem<T>>` returned directly from a handler (serialized as JSON, not SSE) | `TypedResults.ServerSentEvents(stream)` | This is a bug fix: apply. |
| POST used for search only because QUERY wasn't documented | `MapMethods("/search", ["QUERY"], ...)` with OpenAPI 3.2 | **Propose**: changes the HTTP contract. |
| Custom OpenAPI transformers that describe file responses as binary, mark operations deprecated, fix enum parameter casing or merge multiple content types | Delete them; `.Produces<FileContentResult>(contentType: ...)`, `[ProducesResponseType<FileStreamResult>(...)]`, `[Obsolete]` now do it | Diff the generated document before and after. |
| `ASPNETCORE_ENVIRONMENT` set before `dotnet build` for build-time OpenAPI generation | `<OpenApiGenerationEnvironment>Development</OpenApiGenerationEnvironment>` | |
| `AddOutputCache(o => o.AddPolicy(...))` workarounds for dynamic or per-tenant policies | Implement `IOutputCachePolicyProvider` | Only where policies are genuinely dynamic. |
| Third-party zstd `ICompressionProvider` | Built-in; tune with `Configure<ZstandardCompressionProviderOptions>` | |
| Reaching into `SslStream.TransportContext.GetChannelBinding` | `ITlsConnectionFeature.TryGetChannelBindingBytes(ChannelBindingKind.Endpoint, out var cbt)` | |
| TLS handshake failures logged with custom middleware | `ITlsHandshakeFeature.Exception` | |

## SignalR

- Clients that stop and restart `HubConnection` when a bearer token expires → server `MapHub<T>(..., o => { o.EnableAuthenticationRefresh = true; o.OnAuthenticationRefresh = ... })`, `Hub.OnAuthenticationRefreshedAsync()`; .NET client `WithAuthenticationRefresh(...)`; TypeScript client `withAuthenticationRefresh({...})`. `OnAuthenticationRefresh` should reject a refresh that changes the user (compare `sub`).
- Long-running non-streaming hub methods without cancellation, or converted to streaming only to get cancellation → pass a `CancellationToken` to `InvokeAsync`; the hub method's `CancellationToken` parameter fires.
- Duplicated authorization policies on hubs mirroring endpoint attributes → `IAuthorizationRequirementData` attributes now work on hubs directly.

## Blazor

### All render modes

| Signal | Replace with |
| --- | --- |
| Label or column-header text duplicating a model's `[Display(Name=...)]`; reflection helpers reading `DisplayAttribute` | `<DisplayName For="@(() => Model.Prop)" />` |
| `Navigation.Uri + "#..."`, string-built fragment links | `Navigation.GetUriWithFragment("id")` (handles a non-root `<base href>`) |
| URIs built from `ToBaseRelativePath` to reach a sibling path | `NavigateTo("x", new NavigationOptions { RelativeToCurrentUri = true })`; `<NavLink RelativeToCurrentUri="true">` |
| `@inject IWebHostEnvironment` / `IWebAssemblyHostEnvironment` used only for `@if (Env.IsDevelopment())` markup | `<EnvironmentView Include="Development">` / `Exclude="Production"` |
| Hard-coded `<base href="/" />` in `App.razor` | `<BasePath />` |
| `autostart="false"` plus a hand-written `Blazor.start({ circuit: { reconnectionOptions }, webAssembly: { environment, applicationCulture }, ssr: { disableDomPreservation }, logLevel })` | `MapRazorComponents<App>().WithBrowserOptions(o => ...)` or `<ConfigureBrowser Options=... />`. API was renamed during previews; check it exists in the installed SDK. |
| Old top-level `Blazor.start({ loadBootResource })` with `blazor.server.js`/`blazor.webassembly.js` | Optional: nest under `webAssembly: {}` / `circuit: {}`. Old shape still works; low value. |
| JS interop that scrolls virtualized lists (`scrollIntoView`, `scrollTop`) | `InitialItemIndex`, `ScrollToItemAsync(index)`; `AnchorMode="End"` for chat/log lists. Throws before first interactive render. |
| CSP `style-src 'unsafe-inline'` kept only for `<Virtualize>` | Remove; spacers use data attributes now. |
| Inline `onclick` in `NavMenu.razor` | Collocated `NavMenu.razor.js` per the migration guide; removes the CSP hash. |
| `@onclick` on every `TemplateColumn` cell to make a row clickable | `<QuickGrid OnRowClick="...">` |
| Interactive render mode chosen only so QuickGrid paging/sorting works | Static SSR now works via URL links. **Propose**: render-mode change. |
| JS interop or `MarkupString` to render MathML | Plain `<math>` markup. |

### Static SSR

- Flash messages or Post-Redirect-Get data passed through query strings or cookies → cascading `ITempData` or `[SupplyParameterFromTempData]`.
- Reading `HttpContext.Session` from a cascaded `HttpContext` → `[SupplyParameterFromSession]` (needs `AddSession`/`UseSession`; values aren't cleared on read).
- Hand-rolled `IMemoryCache` of rendered fragments or whole-page output caching for an expensive subtree → `<CacheView VaryByQuery="..." ExpiresAfter="...">`. Child lifecycles don't run on a hit.

### Interactive Server

- Custom code reaching `HttpConnectionDispatcherOptions` for `_blazor` → `AddInteractiveServerRenderMode(o => o.ConfigureConnection = d => ...)`.
- JS `visibilitychange` listeners calling `Blazor.pauseCircuit()` → `Microsoft.AspNetCore.Components.Server.AutoPause` with `options.AddAutoPause(...)`.
- Custom drain or shutdown signalling to clients → `Circuit.RequestCircuitPauseAsync(ct)`.
- Forced reconnects or reloads when claims change → circuits now pick up refreshed authentication; `AuthorizeView` re-renders.

### WebAssembly

- `Microsoft.AspNetCore.Components.WebAssembly.DevServer` package and `"inspectUri"` in `launchSettings.json` (standalone WASM) → `Microsoft.AspNetCore.Components.Gateway` and `<StaticWebAssetSpaFallbackEnabled>true</StaticWebAssetSpaFallbackEnabled>`; delete `inspectUri` from every profile.
- Timers or loops started from `Program.cs` or `OnInitialized` for app-wide background work → `builder.Services.AddHostedService<T>()`, now run in WASM.
- Per-environment rebuilds or JS-injected config → environment variables now appear in `builder.Configuration`; set them from the server with `options.InteractiveWebAssembly.EnvironmentVariables`.

### Forms

- Async checks in `OnValidSubmit`, or `async void` `OnFieldChanged` handlers with manual `CancellationTokenSource` → `AsyncValidationAttribute`/`IAsyncValidatableObject` (run by `DataAnnotationsValidator`) or `EditContext.RegisterAsyncFieldValidator(field, ct => ...)`; show progress with `EditContext.IsValidationPending(...)`.
- A custom `IComponentActivator` used only to control `[Inject]` property population → `IComponentPropertyActivator`; `[Inject(Key = "...")]` for keyed services.

## Experimental: report only

Blazor AI components (`Microsoft.AspNetCore.Components.AI`), Device Bound Session Credentials (`Microsoft.AspNetCore.Authentication.DeviceBoundSessions`), QuickGrid `AnchorMode`/`ItemComparer` (`ASP0030`).

## Tooling

- `Microsoft.McpServer.ProjectTemplates` install steps in docs → `dotnet new mcpserver` ships in the SDK.
- WSL dev-cert export scripts → `dotnet dev-certs https --trust` now trusts in Windows too.
