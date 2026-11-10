_Apply this guidance proportionally and within the task's scope. Where a codebase has established conventions, follow them over these defaults._

## Coding guidelines

- Share code when its parts change for the same reason, rather than when they merely look alike.
- Test behaviour through stable interfaces, asserting what callers observe, so tests survive internal refactors. A well-defined internal module is an interface too, and a call to an external system is observable.

### Comments

Implementation comments you write say what the code cannot: the non-obvious "why" (intent, trade-offs, invariants), or a summary of unavoidably dense code.

## Out-of-scope findings

When you find a genuine bug or hazard outside the task, fix it only when it's entangled with your change: the fix shares its abstraction or migration, or leaving it would force the next change to undo yours. Otherwise report it.

In the recap, name every change the task didn't ask for, however small.
