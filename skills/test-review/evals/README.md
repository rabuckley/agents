# test-review evals

Thirteen cases: nine synthetic, four extracted verbatim from real dsegroup code
(`apps-webs` frontend, `code/src/identity` backend). Seven must fire with a
specific diagnosis; five must hold — stay silent, or fire only an evidenced
high-value gap while holding on the criterion the case is a hard negative for;
one (04) is a judgement case that passes on the quality of the argument.
The negatives matter more than the positives — anything can find problems.

How to run: on `claude-haiku-5-5`, give a fresh session the prompt *"Review the tests in
`cases/<file>`; the file contains both the code under test and its tests. If
the tests are sound, say so."* For the baseline arm the skill must be genuinely
absent — see the contamination note below. For the treatment arm, have the
session read `SKILL.md` and follow it as standing instructions with the case as
target. A case passes only if the verdict matches: a fire with the wrong
diagnosis, or a hold case that draws form findings, fails.

Two protocol caveats, inherited and learned:

- **Contamination.** The first baseline run silently auto-loaded the skill —
  a model-invoked skill's description reaches sub-agents, and the description
  alone carries the yardstick. Setting `disable-model-invocation` was not
  enough either: cases live inside the skill directory and agents read the
  neighbouring `SKILL.md` uninvited. A clean baseline needs the skill file
  renamed away *and* an explicit "do not read any file other than the target"
  instruction. Note the baseline still isn't bare Claude: user-level CLAUDE.md
  ("prefer fewer, high-value tests", "test through public APIs") loads into
  every agent, so the skill has to beat Claude-with-house-guidelines.
- **Filename leak.** Case names telegraph their diagnosis (one baseline
  openly cited "the filename's framing"). Same weakness as the braid-review
  corpus; tolerated, but discount verdicts that lean on the name.

## Cases

| Case | Must | Expected |
| --- | --- | --- |
| `01-verify-calls.ts` | fire | Spy-based `toHaveBeenNthCalledWith` order/arg pinning; return value never asserted. Reservations and the confirmation are outgoing commands, so asserting them is asserting output. Expected: criterion 3 both directions — reservation order pinned though the contract doesn't promise it, and under-assertion on the discarded `Order`. Flagging the `send` assertion under criterion 2 is a wrong diagnosis. |
| `02-private-internals.py` | fire | Three tests import private `_wrap_text`, one at a width the public API can never produce. Expected: criterion 1 — `_wrap_text` is a seam shaped by its one caller; route through `format_receipt`. |
| `03-overassert-prose.cs` | fire | Criterion 3 both directions: exact error-message prose; ordered assertion against a documented "no particular order" contract; `Assert.NotNull`-only test (also a criterion-4 deletion). |
| `04-redundant-trio.go` | judge | Criterion 4: `SingleItem`/`TwoItems` (all quantity 1) catch no failure `MultipleItemsWithQuantities` misses, but their joint failures localize the cause (base path, summation, multiplication). Either verdict passes when argued: deletion naming the subsuming test and why localization is worthless for a one-line loop, or keeping a test for the specific cause its failure isolates. Fails: deletion without a subsumption argument, or keeping on "looks like a boundary". |
| `05-clock-and-sleep.py` | fire | Criterion 7 (sleep-as-sync, module-shared store, uninjectable clock) and the deep one: the sweep assertion passes even if sweep is a no-op, since `is_active` re-checks expiry itself. |
| `06-example-cluster.ts` | fire | Criterion 8: eight examples of one tiered-discount rule → single table; all rows kept (each boundary is load-bearing). |
| `07-sociable-fake.ts` | hold | Sociable in-memory fake, state-asserted, fresh per test. Must NOT flag the fake, demand mocks, collapse five distinct-rule tests into a table, or call bare `toThrow()` under-asserted — the test's claim is rejection with the wishlist unchanged, and asserting the message would be over-assertion. |
| `08-regression-pin.go` | hold | Implementation-aware fetch-count pin with linked issue/commit; refetched price equals the old one, so the count is the only possible observable. The linked incident makes the count an evident promise, so it is observable behaviour — must hold. |
| `09-golden-wire.cs` | hold | Exact-match assertions on a documented byte-exact partner wire format. Must apply criterion 3's exact-form carve-out; speculative escaping gaps fail the evidence bar. |
| `10-real-i18n-format.ts` | fire | Real frontend code. Criterion 7 (unpinned timezone can falsely fail short-date tests), criterion 3 (`toContain('9.99')` passes `£9.990`), criterion 8 (five-example `formatMoney` cluster → table). |
| `11-real-auth-snapshot-store.ts` | hold | Hard negative for criterion 2: `vi.fn()` listeners look like interaction verification, but notification *is* the pub/sub store's observable output — must not be flagged. An evidenced criterion-5 gap (user changes while status constant → stale identity) is a legitimate bonus, not a failure. |
| `12-real-skipped-auth-handler.cs` | fire | Real backend code. Three tests permanently skipped ("TODO: rework authorization") can never fail — coverage that doesn't exist over an absent authorization gate; `CreateHandler` ignores its `claims` parameter (mystery fixture); `...Exception2` naming; `Assert.NotNull` noise. |
| `13-real-scope-matching.cs` | hold | Table-driven access-control scope tests through the public API. Must endorse the tables and self-match rows; evidenced criterion-5 gaps on untested `Matches` branches (cross-scope false-match) are legitimate; an unevidenced completeness sweep fails. |

## Baseline (skill absent, sonnet) — 2026-07-19

| Case | Verdict |
| --- | --- |
| 01 | FAIL — declared **sound**; praised the call-order pinning as "matching the implementation exactly"; missed the unasserted return value |
| 02 | PASS — fired on private-helper testing (CLAUDE.md's "test through public APIs" doing the work) |
| 03 | PASS — fired: culture-brittle prose, disclaimed ordering, vacuous NotNull, incidental fields |
| 04 | PASS — fired; even spotted that quantity-1 tests can't detect a dropped multiplication |
| 05 | PARTIAL — fired on sleep/internals/clock but missed that the sweep assertion passes with sweep as a no-op |
| 06 | FAIL — declared sound; no table suggestion |
| 07 | PASS — silent (its one "nicety": assert the error message — the exact over-assertion the skill forbids) |
| 08 | PASS — silent; ran the test, honoured the pin |
| 09 | PASS — silent; independently justified golden exact-match |
| 10 | FAIL — declared sound; *praised* the fragment assertions as "defensively written"; timezone noted only as a caveat |
| 11 | PASS — held; one sharp note on what the reentrancy test can't distinguish |
| 12 | PARTIAL — "mostly sound"; caught the ignored `claims` parameter but called the permanent skips "honest", no deletion posture |
| 13 | PASS — held on form; gaps listed lean completeness-shaped (MaxLength boundary et al.) |

## With skill (sonnet) — 2026-07-19

| Case | Verdict |
| --- | --- |
| 01 | PASS — criterion 2 with the falsely-breaking refactors named (reorder/batch), state-recording-fake remedy, return-value under-assertion ranked worst |
| 02 | PASS — internals + unreachable `width=16` + carve-out explicitly rejected; also caught that deleting the item loop passes the total-only assertion |
| 03 | PASS — ordering vs documented contract worst-first; expose-amounts-as-properties remedy; argued deletion of `Open_ReturnsAccount` via subsumption |
| 04 | PASS — both redundant tests deleted with per-mutant subsumption arguments; table collapse as secondary |
| 05 | PASS — found the no-op-sweep false coverage the baseline missed, plus criterion 8 with substitutions |
| 06 | PASS — table collapse with all rows argued load-bearing; property form explicitly rejected (no independent oracle) |
| 07 | PASS (run 3) — silent; see iteration log |
| 08 | PASS (run 3) — silent; fetch count identified as fake state, carve-out honoured, "could not produce the refuting refactor" |
| 09 | PASS — silent; escaping gap explicitly rejected as failing the evidence bar |
| 10 | PASS — timezone false-failure ranked worst, digit-boundary under-assertion with concrete break, five-example table collapse |
| 11 | PASS — hard negative held (listeners not flagged); one high-value evidenced gap (stale user on same-status change) |
| 12 | PASS — skips-as-phantom-coverage worst-first with access-control framing; missed naming the `claims` mystery fixture explicitly (remedy subsumes it) |
| 13 | PASS — form held (tables endorsed, no tautology flag); three evidenced gaps on the wildcard-matching branches |

## Baseline (skill absent, claude-haiku-5-5) — 2026-10-12

Skill text: the uncommitted 2026-10-12 revision (see iteration log). Single
run per case; criterion numbers below are the current ones.

| Case | Verdict |
| --- | --- |
| 01 | PARTIAL — declared sound; endorsed the order pinning; did list the unasserted return value as a gap |
| 02 | PASS — fired: weak receipt assertion, private-helper coupling, plus a real blank-line bug for an over-long first word |
| 03 | PASS — fired: disclaimed ordering, culture-brittle prose, NotNull-only |
| 04 | PARTIAL — declared sound; called the quantity-1 tests "largely redundant" but kept them as a judgement call |
| 05 | PASS — caught the no-op sweep (the Sonnet baseline missed it), sleep, shared store |
| 06 | FAIL — declared sound; no table suggestion |
| 07 | PASS — sound; nits: assert the error message, seed before the rejected add |
| 08 | PASS — silent; count read as the external-call observable |
| 09 | NOISY — sound, then a gap sweep: escaping, culture, rounding, negatives |
| 10 | PARTIAL — timezone flagged as a real flake; fragment assertions praised; no table |
| 11 | PASS — held; stale-user gap and the live-`Set` reentrancy point |
| 12 | PASS — deletion posture on the skips, ignored `claims` caught; `...Exception2` not named |
| 13 | PASS — held on form; gaps mix evidenced `Matches` branches with completeness items |

## With skill (claude-haiku-5-5) — 2026-10-12

| Case | Verdict |
| --- | --- |
| 01 | PASS — criterion 3 both ways, calls explicitly held as outgoing commands; plus a failure-path gap |
| 02 | PARTIAL — criterion 1 demoted to "judgement call, lower confidence" and ranked last; cited "pure function with a clear contract" |
| 03 | PARTIAL — ordering and NotNull deletion right; called exact exception-message match "defensible" as user-visible text, flagging only culture |
| 04 | PARTIAL — deleted `TwoItems` with a subsumption argument; kept `SingleItem` as a one-item boundary |
| 05 | PASS — no-op sweep worst-first, injected-clock and join remedies |
| 06 | PASS (weak) — table collapse with columns, but framed as optional |
| 07 | NOISY — two evidenced under-assertions on the rejection test (empty start makes "unchanged" vacuous; bare `toThrow()`) |
| 08 | PASS — silent; incident pin read as an evident promise |
| 09 | NOISY — held exact-form; fired the speculative escaping gap and a culture-pin gap |
| 10 | PARTIAL — timezone and digit-boundary under-assertions right; table collapse explicitly rejected |
| 11 | PASS — listeners held; stale-user gap and live-`Set` reentrancy |
| 12 | PASS — skip deletions, inert `claims`, `...Exception2` naming |
| 13 | PASS — tables endorsed; three evidenced `Matches` mutants; one self-admittedly conditional gap the evidence bar should have dropped |

Tally: baseline 8 pass / 3 partial / 2 fail; skill 7 / 4 / 2. On Haiku the
skill wins 01 and 06 but loses 02 and 03, both on wording added in the
2026-10-12 revision: "visibility is not the test" let Haiku excuse a helper
shaped by its one caller, and "messages users or API clients read" let it
treat an internal exception message as contract.

Follow-up the same day: criterion 1 now names a private helper with one caller
as the typical case, and the exact-form exception lists text that is itself
the product and excludes developer-only exception messages. Re-run on the two
regressed cases, skill arm only: **02 PASS** on both Haiku (all three helper
tests flagged with rewrites through `format_receipt`) and Sonnet (seam,
falsely-breaking refactors, deletion); **03 PASS** on both, with the exception
message flagged as developer prose (Haiku ranked it worst, citing the culture
dependence as well).

## With skill (claude-haiku-5-5) — 2026-10-12, after the over-steer review

A review of the revised skill found wording that steers Claude away from calls
it would make correctly by default; see the iteration log. Re-run on the touched
cases, skill arm only, single run each:

| Case | Verdict |
| --- | --- |
| 02 | PASS (weak) — criterion 1 on the helper tests with `format_receipt` routes; no "pure function" excuse; did not call out the unreachable `width=16` |
| 03 | PASS — ordering, developer-only message prose, `NotNull`; plus an evidenced gap: `Withdraw`'s stored balance is never observed |
| 04 | PASS — both quantity-1 tests deleted; argued that no cardinality branch exists, so no failure localizes better |
| 06 | FAIL with the five-example threshold deleted ("small enough that it does not obscure the rule"); PASS (weak) with it restored — table with columns, framed optional; plus an evidenced per-unit-rounding under-assertion |
| 07 | PASS — silent; bare `toThrow()` held for lack of a wrong result that passes |
| 10 | PASS — timezone with the UTC+13 scenario, `9.99` digit-count under-assertion, table (optional); one noisy exact-match demand on the short date |

## With skill (claude-haiku-5-5) — 2026-10-12, after the default-behaviour review

Re-run on the cases the prescriptive-wording review touched; see the iteration
log. Skill arm only.

| Case | Verdict |
| --- | --- |
| 01 | PASS — criterion 3 both ways, spied calls held as outgoing commands; plus an evidenced reservation-failure gap |
| 03 | PASS — ordering, developer-only message prose (ranked worst with its culture dependence), `NotNull` strengthened rather than deleted on a localization argument; plus an evidenced `>=` boundary gap |
| 06 | FAIL on the first wording ("their number obscures the rule" read as a judgement, held); PASS 2/2 once the count became the trigger — table with columns, all boundary rows kept, rounding case separate |
| 07 | PASS — silent; bare `toThrow()` held for lack of a wrong result that passes |
| 10 | PARTIAL on the first wording (table rejected: "each pins a different rule"); PASS on the fixed wording — timezone, digit under-assertion, five-example table |

## Iteration log

- **Case 08 v1** had an authoring bug the baseline caught by running it: only two
  fetches ever occur, so `responses[2]=250` was dead and the test could never
  pass. **v2** (`{100, 250}`) drew a *correct* treatment finding: with a
  price-change observable, the fetch-count pin is subsumed by the price
  assertions — the carve-out protects implementation-awareness, not redundancy.
  **v3** makes the refetched price equal the old one (`countingFetcher`), so the
  count is the only possible detector and uniquely load-bearing. Also trimmed
  the error path from the interface (was drawing a fair criterion-6 gap) and
  reworded the comment to claim only what the sequential test exercises.
- **Case 07 v1** contained two genuine gaps both arms found (append path never
  exercised with distinct SKUs; `customerId` never varied). Patched with two
  tests. The re-run then over-fired criterion 9 on the resulting trio →
  added the materiality threshold to criterion 9 ("roughly five or more
  examples; a pair or trio of well-named examples stays") → silent.
- **2026-10-12 review.** Rules keyed on surface features were replaced by the
  principle beneath them: observable behaviour now covers promised calls and
  how-guarantees (retiring the history-only carve-out), criterion 1 asks about
  seam stability rather than visibility, and the "push it up" criterion was
  deleted (Claude's default weighing of test level was already sound), so
  later criteria shifted down by one. Result tables above use the old
  numbering. Case 01's expected diagnosis moved from criterion 2 to criterion
  3: its spied calls are outgoing commands.
  Re-run (sonnet) on the touched cases: **01 PASS** (criterion 3 both ways,
  calls explicitly not flagged as outgoing commands); **02 PASS** (unstable
  seam, refactors named; didn't call out the unreachable `width=16`); **08
  PASS** (silent, incident pin read as an evident promise); **11 PASS**
  (listeners held as output; evidenced stale-user gap; also an evidenced
  under-assertion: the reentrancy test passes with the live `Set`, matching
  the baseline's note, plus one borderline notification-order nit).
- **2026-10-12 over-steer review.** Criterion 3 now asks for the behaviour the
  test claims, not the whole contract; criterion 4 keeps a test whose failure
  points to its cause more directly (case 04 became a judgement case);
  criterion 1 regained the extract-as-module remedy for helpers whose edge
  cases are expensive to reach; criterion 7 requires the condition under which
  the outcome changes, not just a clock read; criterion 6 accepts a named
  constant; criterion 8 prefers the codebase's existing idioms. Deleting
  criterion 8's "roughly five" threshold failed case 06 on Haiku, so the
  threshold stays.
- **2026-10-12 default-behaviour review.** Deleted "test doubles are tolerated
  anywhere" (it licensed doubles that diverge from the real collaborator; both
  baselines held case 07's fake without it) and the characterization-test
  timing rule (golden masters are often permanent, and Claude cannot tell
  whether "the refactor" has landed). Criterion 9 can flag assertions whose
  failure output hides the delta, not only names; criterion 4 keeps a test that
  catches the failure substantially sooner; criterion 8 restates rather than
  collapses, and prefers rather than requires existing idioms; evident
  how-guarantees include security properties obvious from the domain; `all`
  fans out by code under test rather than test directory. Not yet re-run:
  cases 06, 07, 08, 10 and 11 touch the changed criteria.
- **2026-10-12 narrowing review.** Criterion 2 no longer claims interaction
  verification "catches no bug": against a stub that ignores its arguments it
  is the only detector of a wrong ID or filter, so the evidence now includes
  the replacement assertion that keeps that detection. Criterion 7 flags
  randomness only when its failing case cannot be reproduced, so property
  tests stop reading as flaky. Default scope now includes tests exercising
  changed production code, and that code's criterion-5 gaps, so a branch that
  changes code but no tests is no longer empty. The evident-promise list is
  marked as examples; developer exception messages may still pin the offending
  key or value; the report asks for the working tree left as found instead of
  "change no files", so running a mutant or the suite is allowed.
  Deleted: criterion 1's extract-as-module remedy (the criterion already
  exempts behaviour expensive to reach through the caller), the report's
  example ranking, and the git recipe for the default scope. Declined:
  counting a test's readability as a reason to keep a subsumed one (house
  guideline favours fewer tests) and an "unverified, high-consequence" escape
  from the evidence bar (cases 09 and 13 show the noise it would admit). Not
  yet re-run: 01, 02, 03, 06, 07, 08, 10, 11.
- **2026-10-12 prescriptive-wording review.** The yardstick no longer claims
  "iff" or "any internal refactor": a test fails when the behaviour it claims
  changes and survives refactors that keep the interface it tests intact,
  which criterion 1's internal-module interfaces and criterion 4's
  localization keep already required. Criterion 8's size threshold now gates
  only tables; a property test is recommended when the examples don't reach a
  law's edge cases, keeping the boundary examples. Deleted: criterion 2's
  prescribed fake (its evidence already demands a replacement that catches a
  wrong argument) and "a delegation to a collaborator" (commands are already
  output); "incidental fields" from the over-assertion cues (it invites
  arguing down whole-value equality, and both baselines flag genuinely
  incidental fields unprompted); the snapshot rule's "diff will be read"
  condition (uncheckable); criterion 7's "network in unit tests" (a surface
  rule the condition-of-change evidence covers). The table threshold must be
  phrased as the trigger — "roughly five or more examples of one rule", with
  "obscures the rule" as its reason — not as a judgement the count merely
  illustrates: the judgement phrasing failed 06 and 10 on Haiku. Declined:
  separate tautological-assertion and "every import returns `undefined`"
  cues (criterion 3's under-assertion evidence covers both) and an
  over-assertion counterweight for whole-value equality (not yet observed).
  Not re-run: 02, 04, 05, 08, 09, 11, 12, 13.

## What the baseline cut from the skill

Claude-with-house-guidelines already catches internals-testing, prose/ordering
over-assertion, redundancy with decent subsumption instincts, and sleep-based
flakiness — the skill carries no catalogue of those. What it reliably gets
wrong, and the skill therefore carries: **endorsing interaction verification as
sound** (01: baseline praised what treatment converted to state-recording
fakes; 10: praised fragment assertions), **never proposing the table/property
upgrade** (06, 10), **no deletion posture for phantom coverage** (12's
permanent skips graded "honest"), **stopping at surface smells** (05: flagged
the sleep but not that the assertion proves nothing), and — on hold cases —
padding with completeness demands the evidence bar exists to kill. The skill is
the yardstick, the per-criterion evidence artifacts, the definition
of observable behaviour (promised calls and how-guarantees), the exact-form
contract, and the permission to return nothing.
