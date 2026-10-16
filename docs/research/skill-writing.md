# Skill-writing research

Primary sources checked 2026-10-16. This note summarizes documented guidance; the final section is an inference for this repository.

## Documented guidance

- **Treat descriptions as routing interfaces.** Put capability, actual trigger phrases, and boundaries in frontmatter, where the host can see them before activation. Front-load key triggers: Codex can shorten descriptions when many skills are installed. Test realistic prompts for both correct activation and unwanted activation. [OpenAI: Build skills](https://learn.chatgpt.com/docs/build-skills#how-chatgpt-and-codex-use-skills)
- **Default to one focused, instruction-only workflow.** Add scripts when determinism or external tooling justifies them. Make inputs and outputs explicit. Optional `agents/openai.yaml` can disable implicit invocation for skills requiring explicit selection; this is host-specific metadata, not part of the portable core. [OpenAI: Best practices and optional metadata](https://learn.chatgpt.com/docs/build-skills#best-practices)
- **Spend instructions on demonstrated gaps.** First run representative tasks without the skill, capture failures, create three scenarios, then add the minimum guidance that improves results. Evaluate with a fresh task-running agent and every model actually targeted. Judge observed behavior, rather than how convincing the prose looks. [Anthropic: Evaluation and iteration](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices#evaluation-and-iteration)
- **Match prescription to fragility.** Give judgment-heavy tasks heuristics; use exact commands and ordered steps only when variation can break the operation. For critical work, specify a validation-and-repair loop with a concrete completion condition. [Anthropic: Core principles](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices#core-principles), [feedback loops](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices#workflows-and-feedback-loops)
- **Make loading selective.** Keep SKILL.md as the operational core and navigation; link supporting files directly from it with conditions for reading each. Avoid chains of references; long reference files need a contents list because agents may preview only the opening. [Anthropic: Progressive disclosure](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices#progressive-disclosure-patterns)
- **Distinguish portable requirements from authoring targets.** The standard requires `name`/`description` YAML frontmatter and defines their constraints. Under 500 lines / 5,000 tokens is a recommendation, not a reason to pad a skill. Use relative paths from the skill root. `skills-ref validate ./my-skill` checks format and naming, not behavioral quality. [Agent Skills specification](https://agentskills.io/specification)

## Recommendations for this repository (inference)

Keep skill-authoring guidance in `skills/AGENTS.md`: the root file is installed as global instructions by [install.sh](../../install.sh) and [install.ps1](../../install.ps1).

Compare execution quality against a baseline in fresh contexts, grade observable outcomes, and generalise fixes rather than patching individual examples. [Agent Skills: Evaluating output quality](https://agentskills.io/skill-creation/evaluating-skills)

Check baseline isolation explicitly. Local [test-review eval notes](../../skills/test-review/evals/README.md) record skill discovery through metadata and adjacent files, plus diagnostic filenames leaking expected answers. These are repository observations, not universal host guarantees. Keep proposed cases distinct from executed evidence, as the local [documentation decision cases](../../skills/csharp-xml-docs/evals/README.md) do.

For invocation fields, check the intended host: [Claude Code's controls](https://code.claude.com/docs/en/skills#control-who-invokes-a-skill) differ from Codex's optional metadata. Treat successful format validation as separate from evidence that the host honours a field.
