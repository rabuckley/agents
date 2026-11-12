_Apply this guidance proportionally and within the task's scope. Where a codebase has established conventions, follow them over these defaults._

## Coding guidelines

- Share code when its parts change for the same reason, rather than when they merely look alike.
- Test behaviour through stable interfaces, asserting what callers observe, so tests survive internal refactors. A well-defined internal module is an interface too, and a call to an external system is observable.

### Comments

Implementation comments you write say only what the code cannot: the non-obvious "why" (intent, trade-offs, invariants), or a summary of unavoidably dense code.
