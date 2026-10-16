# Writing skills

- Write the smallest instruction that corrects an observed failure. Start with representative tasks and a baseline; prune rules the intended model already follows. Explain a non-obvious constraint's reason so the agent can generalise it.
- Treat discovery and execution as separate behaviours. Front-load user intent and distinguishing triggers in `description`; scope hidden in the body cannot help selection. Test natural prompts without naming the skill, including near misses that belong to a neighbouring skill. Explicit invocation tests execution only.
- Keep the common path in `SKILL.md`. Move branch-specific detail behind a direct relative link that says **when to read it** and what it contains. Keep required references one hop away; an unlabelled bibliography is not a workflow.
- Prescribe exact commands and ordering where mistakes are costly; give decision criteria where several approaches are valid. Make completion observable: name the artifact, evidence or validation that ends a step. Permit an empty result when there is nothing to report.
- Use contrasting examples to teach a difficult boundary, rather than accumulating synonyms or one-off exceptions. When an eval fails, fix the underlying decision rule and check that nearby valid cases still pass.
- Bundle repeated deterministic work into scripts when traces show agents rebuilding it. State dependencies, inputs, outputs and failure handling; say whether to execute a helper or read it. Verify the resulting artifact, not merely that the script ran.
- Separate portable instructions from host mechanics. Before adding invocation, tool or subagent frontmatter, check the target host's docs; a field accepted by one host may be ignored elsewhere. Skills here are shared across tools.
- Compare revisions in fresh contexts with the same model, inputs and standing guidance. Ensure the baseline cannot discover the target skill through metadata or adjacent files. Use neutral fixture names; record the skill revision, model, outcomes and costs. Label proposed cases separately from executed results.

Consult these when the corresponding question arises:

- Format or paths: [Agent Skills specification](https://agentskills.io/specification).
- Trigger accuracy: [description evaluation](https://agentskills.io/skill-creation/optimizing-descriptions).
- Instruction structure: [Anthropic authoring guidance](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices).
- Eval design and interpretation: [output evaluation](https://agentskills.io/skill-creation/evaluating-skills); local [baseline contamination examples](test-review/evals/README.md).
- Host differences and source provenance: [research notes](../docs/research/skill-writing.md).
