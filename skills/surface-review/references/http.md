# JSON-over-HTTP pack

The boundary surface between a backend and its clients: reviewed on its own when the target is an API layer, and as the third split of a full-stack diff, where it is often the highest-value surface because the caller is in the same diff. The frontend's fetching code, generated client, and mirrored types are the scenario code; read them before judging the endpoints, and measure in wire units: requests issued per screen, the ordering among them, fields fetched against fields rendered, and which error branches the client actually handles.

## Surface

The routes, methods, and status codes; the request and response shapes as serialized (not as declared server-side: the serializer's treatment of null, default, and missing is part of the contract); the error envelope; and the client-side mirror of all of it, whether hand-written TypeScript types, a generated client, or a shared schema. Both sides of the mirror are one surface: a finding on the shape lands on both. RPC-ish POST endpoints are this surface under the same tests; streaming shapes (WebSocket, SSE messages) are too, with reconnection and replay semantics as their lifetime expression.

## Contract expressions

- **Errors.** One envelope, machine-distinguishable: a stable code or type field the client can switch on, never message prose; validation failures enumerated per field so the client renders them without parsing. Status codes carry the class of failure, the envelope carries the instance. A client observed switching on message strings is the caller-gets-it-wrong evidence.
- **Absence.** JSON has two absences, missing and null, and default binding blurs them on every mainstream serializer; the contract says which one means what, and the client types mirror it exactly (`?` for may-be-missing, `| null` for emitted-as-null). This ambiguity is why this pack rules PUT over PATCH: a full replacement gives missing one fixed meaning — rejected for a required field, or clear-to-default — so nothing rides on telling it apart from null.
- **Ownership and mutability.** The server owns resource state; every client copy is a cache. Shapes should make caching sane: stable ids to key on, and one canonical shape per resource — a client cache keyed by resource id can only reuse entries when every endpoint returns the resource in the same shape. Where concurrent edits are a real scenario, the surface carries versions or ETags; where they are not, it does not.
- **Lifetime and concurrency.** Requests retry and networks duplicate: mutations state their idempotency, and unsafe retries the client will obviously make (a double-submitted POST) are designed away with PUT or an idempotency key (a convention to state, not a standard to cite). Wire lifetimes are contract too: what a deleted resource returns afterwards, when cursors expire, how long a job resource stays retrievable.
- **Sync and async.** Work longer than a request holds gets a job resource to poll or subscribe to, not a held connection; the shape tells the client which it is, and how to cancel (delete the job, or a cancel action).
- **Compatibility.** The wire is versioned by deployment reality, not by repo boundaries: even a same-repo PR has a window where the old frontend calls the new backend or the reverse, so removals, renames, and type changes break live clients that additive fields do not. State the deployment coupling in the report and hold breaking wire changes to it.

## Idiom

Stripe- and GitHub-grade REST is the bar, and consistency across the API's own endpoints outranks any single convention: similar resources read the same way. The conventions that most often decide a finding: plural-noun resource routes with standard methods for CRUD; a verb-named action endpoint (`POST /orders/{id}/cancel`) for a transition that is not a whole-resource replace; PUT full replacement over PATCH (see Absence); cursor pagination with a stable ordering; one error envelope across the API; opaque string ids. Beyond these, judge idiom by the platform's own precedents and the caller's code, not a longer rule list.

## Rulings

- **The endpoint serves the screen's scenario, not the schema.** A response mirroring the entity or table is the leaked-storage form of the information-hiding test: the columns are the decision being revealed. Screen-shaping and the canonical shape reconcile by level: the screen decides which resources and fields an endpoint composes; each resource embedded keeps its one canonical shape, subset by omission, never renamed or renested — and the mutation path always speaks the canonical shape.
- **Identity comes from the credential, never the payload.** A `userId` or `tenantId` in a request body that the server should derive from auth is a caller-forgeable field; deleting the field is the wire form of defining the error out of existence. The 404-versus-403 policy for out-of-scope resources is part of the error contract, stated once.
- **Mirrored types are derived or verified, never twins.** A hand-maintained TS mirror of a C# DTO is duplication whose drift no compiler sees; generation or schema verification is the remedy, and its absence on a surface that keeps changing is a contract finding.
- **Chattiness is a depth finding.** A client that must call three endpoints in order to render one screen is the flowchart decomposition on the wire; the ordering the client must learn is the artifact. Its dual is the same finding: one call returning the entity graph the screen ignores.

## Reflexes

- You will review each side of the boundary alone and miss that the frontend code in the diff is the backend surface's measurement. Read the calls first.
- You will accept a DTO because it serializes cleanly, when it mirrors storage the caller never asked to know.
- You will call error handling done because status codes exist, while the caller in the same diff switches on message strings.
- You will treat a same-repo wire change as private and skip the compatibility statement. The deployment window makes it public.
