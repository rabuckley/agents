# Deepening

How to deepen a cluster whose parts share one concept, given its dependencies. Assumes the vocabulary in [SKILL.md](SKILL.md): **module**, **interface**, **seam**, **adapter**.

## Dependencies

For each dependency, ask whether tests can reach the behaviour that matters through the real thing, fast and deterministically, failure modes included. The answer determines how the deepened module is tested across its seam.

- **Reachable**: pure computation, a temp directory, Postgres in a container, a stand-in like PGLite. Test the deepened module with the dependency running. It needs no port for testing's sake.
- **Not reachable**: a remote service, a third-party API, a failure the real thing can't produce on demand. If it goes behind a **port**, shape the port by what the module needs, not by the dependency's API; production uses a real adapter, tests a fake.

## Testing strategy: replace, don't layer

Once tests at the deepened module's interface cover the behaviour of old unit tests on shallow modules that are now internal to it, delete those old tests, except those reaching edge cases that are cheaper to test through an internal seam.
