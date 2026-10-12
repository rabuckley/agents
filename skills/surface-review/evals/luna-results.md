# Luna comparison, 2026-09-08

The entrypoint shrank from 3,228 to 1,365 whitespace-delimited words (58%).
This measures instruction length, not token usage or review latency.
Platform packs remain separate; only the .NET proposal-samples sentence changed.

## Method

All reviewers used `gpt-5.6-luna`, with the same inherited reasoning setting
and tool access. They received the skill path and raw case files, without
expected verdicts, evaluation history, or the author's diagnoses. Cases 05
and 08 ran in separate fresh agents; cases 01–04, 06, and 07 ran as a group
in a fresh agent per revision. Grouping limits independence between cases.

There were 35 case reviews across the original, two intermediate revisions,
and the final text. The second intermediate revision repeated case 08 twice
and did not repeat case 05. After the final grouped case 07 produced a
cast-based finding, case 07 ran in three fresh agents: once with the original
and twice with the final prompt. These are development smoke tests on familiar
.NET fixtures, not held-out or cross-platform validation. No aggregate
performance or cost claim follows from them.

Git blob identifiers for the entrypoints:

- Original: `b76dece6dec8aec08a97d7cdf8608240a8656b0d`.
- First revision: `77db66fbd5e798af1f04f40f13d1f4453a3dc8ca`.
- Second revision: `f12164b8d0befe17b76186bf38c99683e41ad143`.
- Final: `d93da550056e362b3c5cc3a2245a618d19dbe283`.

## Observations

The original and first revision both found the staged importer, redundant
service layer, retry configuration burden, and exposed mutable index state.
Both kept case 05 below Shape and left case 07 clean. The original invented
a queue-limit requirement in case 06; both intermediate revisions stayed
silent there. AddRange coverage and duplicate findings varied between runs.

Case 08 exposed a fixture problem as well as reviewer errors. The fixture
does not establish whether terminal failures require different recovery.
It also leaves validation in a stubbed helper. Neither a forced clean verdict
nor accepting every contract question is a defensible grading rule. The
current rubric therefore treats it as diagnostic-only, leaving the fixture
unchanged. One second-revision run was silent; the other assumed missing
validation and mislabelled a static-class guideline as a house policy.

The final prompt makes the policy boundary literal: only bullets under
Rulings receive the policy exception. Its uncertainty check is general and
does not name the watcher or prescribe its verdict. The final watcher review
still asserted missing validation without evidence. It also identified the
visible Directory.Exists call and eager exception timing. That observation
is grounded in code; whether its timing warrants a finding needs a caller
scenario, and one try block can cover both creation and enumeration.

The final grouped run retained the main diagnoses for cases 01–04 and caught
AddRange's unnecessarily concrete input. Case 05 stayed at Contract/Idiom
with local remedies. Remaining errors included a cast-based finding on
case 01, an unsupported testability-policy attribution in case 02, and a
cast-based finding on case 07 that directly contradicted the .NET pack.
Case 06 raised eager-success cancellation timing instead of importing queue
limits: the token is visibly absent from that path, but whether pre-cancelled
calls must fail needs a clearer contract. These are not all equivalent errors.

The fresh case 07 checks were all clean: one original and two final runs.
The cast-based finding was not reproduced in isolation. This is evidence of
run variance, not proof that grouping caused it or that the prompts are equal.
No further rules were added to force the case silent.

## Validation and limits

YAML parsing, unchanged invocation fields, local Markdown links, and
`git diff --check` passed. The bundled quick validator rejects the existing
`argument-hint` and `disable-model-invocation` fields on both original and
revised snapshots. They were preserved rather than changing invocation
behaviour as part of prompt optimisation.

The rubric now separates current expectations from historical scores and
marks the seam-policy change explicitly. It does not claim an improved pass
rate by changing the grading contract. Scope resolution and other platforms
have not been behaviourally tested in this comparison.
