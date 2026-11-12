# When the branch is a bug fix

A bug-fix branch claims a cause is gone, so the loop establishes two more facts before anyone reviews.

**Purpose.** The outcome is that the reported failure *and its root cause* are gone, so name the cause and not the fix. A spec that leaves sibling paths out of scope decides what the branch fixes, not what the review covers: other paths to the same cause stay in scope.

**Inventory.** In step 2, list every code path in the codebase that reaches the cause, with file:line and a count, and how many of those paths the branch changes. Record it on the ledger's `inventory` line.

**Tests against the base.** In step 2, run the branch's new or changed tests against the base's code, in a scratch worktree, to see whether they fail without the fix. Remove the worktree afterwards, and record the result with the round's `established` results.

In the run's second round, redo only whichever of the two a fix commit has made stale.
