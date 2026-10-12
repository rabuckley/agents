# braid-review evals

Nine language-agnostic cases. Five must fire with a specific diagnosis; four must
stay silent. The negatives matter more than the positives — anything can find
problems.

How to run: give a fresh session (no skill loaded for baseline; skill body pasted
or invoked for treatment) the prompt *"Review the design of the code in
`cases/<file>`"* and compare against the expected column. A case passes only if
the verdict matches — a fire with the wrong diagnosis, or a "silent" case that
draws findings, fails.

## Cases

| Case | Must | Expected |
| --- | --- | --- |
| `01-hypothetical-seam.ts` | fire | Interface + factory + singleton with one implementation and one consumer. Expected: propose deletion (collapse to a direct function), not a wrapper or more tests. |
| `02-flag-threading.py` | fire | Boolean policy flag threaded through four layers to reach one leaf branch. Expected: name the braid — every intermediate signature complected with a leaf rounding policy — and resolve the policy where it is decided. |
| `03-time-dependent-result.cs` | fire | `FinalPrice` reads mutable static state and `DateTime.Now`: same order, different price depending on when you call. Expected: value complected with time; pass promotions and the clock in. |
| `04-temporal-module.go` | fire | "Stages" split by *when* (parse → resolve → validate), communicating through one mutable context; ordering lives in comments. Expected: temporal decomposition — the module boundaries protect nothing; make each stage a typed input→output function. |
| `05-mapper-chain.ts` | fire | Three structurally identical types + two field-copy mappers. Expected: carrying cost not cleared; delete the layer, reintroduce on real divergence. |
| `06-already-simple.py` | silent | Two honest functions, one constant. No findings. |
| `07-long-deep-function.ts` | silent | ~60-line parser, one job, no braids (R1). Must NOT propose splitting by length. |
| `08-plain-data-functions.py` | silent | Frozen dataclasses + pure functions (R2). Must NOT propose moving behaviour onto the classes / a rich domain model. |
| `09-inherent-complexity.go` | silent | Retry with backoff, jitter, idempotency keys — every part maps to a demand of the problem. Must NOT flag inherent complexity. |

## Baseline (bare Claude, no skill) — 2026-07-17

Prompt: "review the design of the code in <file>; if the design is sound, say
so." Note this framing is already a strong elicitation — the skill has to beat
*this*, not a surface-level diff review.

| Case | sonnet | haiku |
| --- | --- | --- |
| 01 | PASS — fired, proposed collapse to a function | — |
| 02 | PASS — named the threading, resolved policy at boundary | — |
| 03 | PASS — mutable static + ambient time | — |
| 04 | PASS — ordering-by-comments, god-context | — |
| 05 | PASS — proposed deletion | — |
| 06 | PASS — silent | — |
| 07 | FAIL — "mixed abstraction levels", proposed extracting header/trailer/date helpers | FAIL — proposed parseHeader()/parseTrailerRecord() extraction |
| 08 | PASS — silent, endorsed the design | NOISY — no domain-model push, but elevated nits (repeated subtotal call, missing validation) to findings |
| 09 | PASS — silent, named the complexity inherent | — |

## With skill — 2026-07-17

Skill body supplied as standing instructions, target = case path.

| Case | sonnet | haiku |
| --- | --- | --- |
| 01 | PASS — braid named, deletion fix | PASS — deletion fix |
| 02 | PASS — relay braid named, deletion of the three parameters | FAIL first run (over-silenced: read the two leaf rounding modes as paying for the relay plumbing) → carrying-cost test reworded ("a layer that only relays is never paid for by what happens at its far end") → re-run below |
| 03 | PASS — three falsifiable findings incl. ambient clock and duplicated promotion mechanism | PASS — braid named, pure-function fix |
| 04 | PASS — ordering complected with type-system silence; typed-signature fix | PARTIAL — fired on a real secondary defect (redundant CustomerIDs map, deletion fix) but missed the primary temporal braid |
| 05 | PASS — ceremony braid named, deletion fix | PASS — deletion fix |
| 06 | PASS — "No structural findings." | PASS — clean verdict |
| 07 | R1 held (no fragmentation proposed) — but flagged a genuine bug accidentally embedded in the case (blank-line filter corrupted reported line numbers). Case fixed; re-run below | PASS — explicitly refused to split; cited depth over length |
| 08 | PASS — clean verdict | PASS — clean verdict, R2 reasoning |
| 09 | PASS — clean verdict | PASS — inherent-complexity reasoning |

Re-runs after iteration:

- 02 haiku, after the carrying-cost reword alone: still over-silenced (named
  the braid, then judged every fix "more abstraction"). After adding the
  relay-deletion gotcha ("resolving the value where it is read *deletes* a
  parameter..."): PASS — fired, boundary fix.
- 07 sonnet, after removing the accidental blank-line bug from the case:
  PASS — "No structural findings."
- 09 haiku regression check after the new gotcha: PASS — still silent, still
  inherent-complexity reasoning (the gotcha's "a parameter a function actually
  reads is not a relay" guard held).

Final state: 9/9 sonnet, 8/9 haiku plus one partial. Both baseline failures
(07 fragmentation on both models; 08 haiku noise) are fixed by the skill.
Remaining known weakness: haiku on cross-file braids can fire on a secondary
defect and miss the primary one (04) — real finding, wrong altitude;
acceptable for the weakest model.

### What the baseline cut from the skill

Bare Claude, given the "review the design" framing, already finds hypothetical
seams, flag threading, time-complected state, temporal decomposition, and
ritual mappers — and already proposes deletions for them. So the skill carries
**no catalogue of patterns to hunt**. What bare Claude reliably gets wrong, and
the skill therefore carries: fragmenting long-but-deep functions (07, both
models), elevating construct-level nits when there is nothing structural to
say (08 haiku), and — outside this corpus, in ordinary diff review — not
adopting the structural framing at all unless asked. The skill is the framing,
the falsifiable tests, the arbitration rulings, and the permission to return
nothing.
