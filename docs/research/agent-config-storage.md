# Storing personal agent configuration (instructions, skills, rules)

Researched 2026-10-12 against official docs and source. Versions that matter: Claude Code v2.1.28x, Codex CLI `main` @ [`c308e12`](https://github.com/openai/codex/tree/c308e127e0e8cd040f670ca0e0496ae1c7519d3f) (2026-10-12).

**Legend.** Plain statements are documented facts, each followed by its source. **[Rec]** marks my recommendation. **[Inference]** marks a conclusion I drew from source code or from combining docs. **[Ambiguous]** marks places where the docs are silent or contradict each other.

> Note: `developers.openai.com/codex/*` URLs now 308-redirect to `learn.chatgpt.com/docs/*`. Both are OpenAI-owned. Citations use the destination URL where I fetched it.

---

## TL;DR

**[Rec]** Keep one canonical, tool-neutral tree in this repo. Link it into each tool's *user-level* location with a small idempotent `install.sh` that runs `ln -sfn`. Don't point any tool's whole config dir (`CODEX_HOME`, `CLAUDE_CONFIG_DIR`, `COPILOT_HOME`) at the repo, because each of those dirs also holds credentials, transcripts and SQLite state.

```
agents/                          # this repo
├── AGENTS.md                    # global instructions, canonical (tool-neutral)
├── skills/<name>/SKILL.md       # Agent Skills standard; one folder per skill
├── claude/
│   ├── rules/*.md               # Claude-only path-scoped rules (~/.claude/rules)
│   └── settings.json            # optional: see "settings" caveat below
├── codex/
│   ├── rules/*.rules            # Codex exec-policy (command approval), NOT instructions
│   └── config.toml.example      # template only; real config.toml stays machine-local
├── copilot/                     # only if Copilot-specific additions are needed
├── install.sh                   # creates the links below; safe to re-run
├── .gitignore
└── docs/research/
```

| Link (created by `install.sh`) | → target in repo | Why this form |
|---|---|---|
| `~/.claude/CLAUDE.md` | `AGENTS.md` | Claude Code has no user-level `AGENTS.md`, only `~/.claude/CLAUDE.md` ([memory](https://code.claude.com/docs/en/memory)). |
| `~/.claude/skills` | `skills/` | Claude reads `~/.claude/skills`, not `~/.agents/skills` ([skills](https://code.claude.com/docs/en/skills)). |
| `~/.claude/rules` | `claude/rules/` | User-level rules ([memory](https://code.claude.com/docs/en/memory)). |
| `~/.agents/skills` | `skills/` | Read by Codex, Copilot CLI, VS Code and Cursor (see table). |
| `~/.codex/AGENTS.md` | `AGENTS.md` | Codex global instructions ([agents-md](https://learn.chatgpt.com/docs/agent-configuration/agents-md)). |
| `~/.codex/rules` | `codex/rules/` | Link the **directory**. Codex silently ignores symlinked `.rules` *files* ([#32658](https://github.com/openai/codex/issues/32658)). |
| `~/.copilot/copilot-instructions.md` | `AGENTS.md` | Copilot CLI user instructions ([CLI instructions](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions)). |

**[Rec]** Use plain `ln -sfn` rather than stow or chezmoi. The targets are spread across four dotdirs that each also hold runtime state, and the Codex rules quirk needs a directory-level link. A script states each of these links explicitly.

**Observed on this machine (2026-10-12).** These are findings, not recommendations:
- `CODEX_HOME=/Users/ryanbuckley/dev/personal/configs//agentic`. Codex's entire state dir, including `auth.json`, `history.jsonl`, `sessions/` and roughly 100 MB of `*.sqlite`, lives inside the `personal/configs` git working tree. Only an allow-list `.gitignore` (`agentic/*`, `!agentic/AGENTS.md`, …) keeps it out of commits. `~/.claude/{CLAUDE.md,skills,rules}` are symlinks into that same dir.
- In this repo, `agentic/` already contains a skeleton of Codex runtime dirs (`sessions/`, `sqlite/`, `plugins/cache/…/node_modules`, `skills/.system`, about 21 MB), created at 19:24–19:25 today. The repo has **no `.gitignore`**, so `git add .` would commit it.
- `…/agentic/rules/` holds both `csharp.md` (a Claude rule) and `default.rules` (Codex exec-policy, which the Codex TUI rewrites itself). Each tool ignores the other's extension, so nothing breaks. The cost is that one dir couples two unrelated mechanisms, and a tool writes into it.
- `…/agentic/skills/` contains `synced/` (written by the Claude Code claude.ai sync), `.system/` (Codex bundled skills) and `.trash/`. A writable symlinked skills dir receives tool-written state.

---

## Per-tool reference

| Tool | Global instructions | Project instructions | Global skills | Rules | Config-dir env var | Symlinks | Imports |
|---|---|---|---|---|---|---|---|
| **Claude Code** | `~/.claude/CLAUDE.md` ([memory](https://code.claude.com/docs/en/memory)). No user-level `AGENTS.md`; `~/.claude/CLAUDE.md` "doesn't count" against and loads alongside project `AGENTS.md` ([memory#agents-md](https://code.claude.com/docs/en/memory)) | `./CLAUDE.md`, `./.claude/CLAUDE.md`, `./CLAUDE.local.md` (gitignored). `./AGENTS.md` is read only when no CLAUDE.md is present, needs v2.1.277+, and is configurable via `cc-plugin-agents-md@builtin` → `instructionFiles` ([memory](https://code.claude.com/docs/en/memory)) | `~/.claude/skills/<n>/SKILL.md`. Precedence: enterprise > personal > project ([skills](https://code.claude.com/docs/en/skills)) | `~/.claude/rules/*.md`, `.claude/rules/*.md` with optional `paths:` frontmatter ([memory](https://code.claude.com/docs/en/memory)) | `CLAUDE_CONFIG_DIR` moves settings, history, plugins and `.credentials.json`, and keys the macOS Keychain entry to the dir ([settings](https://code.claude.com/docs/en/settings), [auth](https://code.claude.com/docs/en/authentication)) | Skill dirs: yes, deduped by target ([skills](https://code.claude.com/docs/en/skills)). `.claude/rules`: yes, but targets outside cwd need external-import approval ([memory](https://code.claude.com/docs/en/memory)). Cowork skips a symlinked `~/.claude/CLAUDE.md` and `~/.claude/rules` ([memory](https://code.claude.com/docs/en/memory)) | `@path`, relative/absolute/`~`, max 4 hops. User-scope imports load without a dialog ([memory](https://code.claude.com/docs/en/memory)) |
| **Codex CLI** | `$CODEX_HOME/AGENTS.override.md`, else `AGENTS.md`, first non-empty wins ([agents-md](https://learn.chatgpt.com/docs/agent-configuration/agents-md)) | `AGENTS.override.md` › `AGENTS.md` › `project_doc_fallback_filenames`, one per dir, root→cwd, 32 KiB cap (`project_doc_max_bytes`) ([agents-md](https://learn.chatgpt.com/docs/agent-configuration/agents-md)) | `~/.agents/skills` (USER), `.agents/skills` (REPO), `/etc/codex/skills` (ADMIN) ([skills](https://learn.chatgpt.com/docs/build-skills)). `$CODEX_HOME/skills` is still read but marked "Deprecated … kept for backward compatibility" in [source](https://github.com/openai/codex/blob/c308e127e0e8cd040f670ca0e0496ae1c7519d3f/codex-rs/ext/skills/src/host_roots.rs#L95-L108) | `rules/*.rules` per config layer = **command-approval policy**, not prompt content. TUI writes `~/.codex/rules/default.rules`. Project rules need trust ([rules](https://learn.chatgpt.com/docs/agent-configuration/rules)) | `CODEX_HOME` (default `~/.codex`) ([agents-md](https://learn.chatgpt.com/docs/agent-configuration/agents-md), [advanced](https://learn.chatgpt.com/docs/config-file/config-advanced)) | Skill folders: yes ([skills](https://learn.chatgpt.com/docs/build-skills)). Global AGENTS.md: **[Inference]** yes, since it uses `fs::metadata`, which follows links ([source](https://github.com/openai/codex/blob/c308e127e0e8cd040f670ca0e0496ae1c7519d3f/codex-rs/codex-home/src/instructions/mod.rs#L41-L46)). `.rules` files: **no**, `is_file()` filter ([source](https://github.com/openai/codex/blob/c308e127e0e8cd040f670ca0e0496ae1c7519d3f/codex-rs/core/src/exec_policy.rs#L1153-L1158), [#32658](https://github.com/openai/codex/issues/32658), open) | **[Ambiguous]** None documented ([agents-md](https://learn.chatgpt.com/docs/agent-configuration/agents-md) is silent) |
| **Copilot CLI** | `~/.copilot/copilot-instructions.md`, `~/.copilot/instructions/**/*.instructions.md` ([CLI instructions](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions)) | `.github/copilot-instructions.md`, `.github/instructions/**/*.instructions.md` (`applyTo`), `AGENTS.md`, `CLAUDE.md`, `.claude/CLAUDE.md`, `GEMINI.md`. No precedence order; avoid conflicts (same) | `~/.copilot/skills`, `~/.agents/skills`; project skills win ([CLI skills](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-skills)) | n/a | `COPILOT_HOME`; also `COPILOT_CUSTOM_INSTRUCTIONS_DIRS` ([CLI instructions](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions), [config dir](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-config-dir-reference)) | A symlinked `settings.json` is written through to its target ([config dir](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-config-dir-reference)) | `@relative` in copilot-instructions/AGENTS/CLAUDE.md. **Absolute and `~/` paths not loaded**; must stay inside repo or the instructions dir ([CLI instructions](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions)) |
| **Copilot in VS Code** | Local agent: VS Code profile (Settings Sync-able). Copilot agent host: `~/.copilot/…`. Claude agent host: `~/.claude/rules`. `~/.claude/CLAUDE.md` when `chat.useClaudeMdFile` is on ([VS Code instructions](https://code.visualstudio.com/docs/copilot/customization/custom-instructions)) | `AGENTS.md` (`chat.useAgentsMdFile`; nested is experimental, off by default), `CLAUDE.md`, `.github/…` (same) | `~/.copilot/skills`, `~/.claude/skills`, `~/.agents/skills` ([VS Code skills](https://code.visualstudio.com/docs/copilot/customization/agent-skills)) | n/a | `chat.instructionsFilesLocations` / `chat.agentSkillsLocations` are deprecated (same pages) | not documented | — |
| **Copilot on github.com** | Personal instructions are UI-only and apply to Chat only ([personal](https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-personal-instructions)) | Repo files; `AGENTS.md` nearest-wins; or a root `CLAUDE.md`/`GEMINI.md` ([repo](https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions)) | — | — | — | — | — |
| **Cursor** | User Rules are set **in the UI** (Customize → Rules). No on-disk path is documented ([rules](https://cursor.com/docs/context/rules)) | `.cursor/rules/**/*.mdc` (`.md` there is ignored); `AGENTS.md` at root and nested; precedence Team → Project → User (same) | `~/.agents/skills`, `~/.cursor/skills`, plus compat `~/.claude/skills`, `~/.codex/skills` ([skills](https://cursor.com/docs/context/skills)) | `.cursor/rules` (above) | not documented | not documented | `@file` is a pointer and is **not inlined** (same) |

**[Ambiguous] Copilot docs conflict on VS Code personal instructions.** The GitHub support matrix marks VS Code Chat "Personal ✗" ([support matrix](https://docs.github.com/en/copilot/reference/custom-instructions-support)). The VS Code docs, by contrast, document profile-level and `~/.copilot/…` user instructions ([VS Code instructions](https://code.visualstudio.com/docs/copilot/customization/custom-instructions)). GitHub's own pages also disagree on `~/.claude/skills`: the CLI skills page doesn't list it ([CLI skills](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-skills)), but VS Code does.

### Open standards

- **AGENTS.md**: plain Markdown with no required fields. "The closest AGENTS.md to the edited file wins." It is stewarded by the Agentic AI Foundation under the Linux Foundation. The spec says nothing about a user-level/global file, so global locations are tool-specific ([agents.md](https://agents.md/)).
- **Agent Skills**: a folder with `SKILL.md`. `name` is required (≤64 chars, `[a-z0-9-]`, **must match the dir name**) and so is `description` (≤1024 chars). Optional fields are `license`, `compatibility`, `metadata` (string map, e.g. `version`) and `allowed-tools` (experimental). Keep `SKILL.md` under 500 lines. Validate with `skills-ref validate` ([spec](https://agentskills.io/specification)). Claude Code extends the frontmatter with fields such as `disable-model-invocation`, `context` and `paths`. Outside Claude Code (claude.ai upload, Skills API), only the six standard fields are accepted and others hard-error ([skills](https://code.claude.com/docs/en/skills)).
  - **[Rec]** Use only standard fields in shared skills. If a skill needs Claude-only fields, expect other tools to ignore them. Claude Code itself "ignores a field it doesn't recognize" ([skills](https://code.claude.com/docs/en/skills)).
  - Reserved names in Claude Code: `synced` and anything starting with `anthropic-skills` ([skills](https://code.claude.com/docs/en/skills)).

---

## Approaches compared

| Approach | How | Pros | Cons |
|---|---|---|---|
| **A. Repo + per-file/dir symlinks** (manual script) **[Rec]** | `ln -sfn` from tool dirs into repo | Explicit, so tool state never lands in the repo unless a link target is writable by the tool. Edits are live: Claude watches `~/.claude/skills` and reloads in-session ([skills](https://code.claude.com/docs/en/skills)), and Codex auto-detects skill changes ([skills](https://learn.chatgpt.com/docs/build-skills)) | Tools that write into a linked dir write into the repo: Claude `skills/synced`, `.trash` ([claude-directory](https://code.claude.com/docs/en/claude-directory)); Codex `rules/default.rules` ([rules](https://learn.chatgpt.com/docs/agent-configuration/rules)). Cowork ignores symlinked user memory ([memory](https://code.claude.com/docs/en/memory)) |
| **B. GNU Stow** | Package dirs mirror `$HOME`; stow creates links | Standard; `--adopt` imports existing files ([stow manual](https://www.gnu.org/software/stow/manual/stow.html)) | Tree folding vs `--no-folding` decides dir vs file links ([stow manual](https://www.gnu.org/software/stow/manual/stow.html)). File-level links break Codex `.rules` ([#32658](https://github.com/openai/codex/issues/32658)). Also needs one package path per tool dotdir |
| **C. chezmoi** | Source state rendered to `$HOME` | Templates (`.tmpl`) suit machine-specific values. `private_` sets permissions ([chezmoi](https://www.chezmoi.io/reference/target-types/)) | **Copies** by default ([chezmoi](https://www.chezmoi.io/reference/target-types/)), so in-place edits by you or by tools drift until `chezmoi re-add`. Heavier than needed for about 5 links |
| **D. Point the tool's home at the repo** (`CODEX_HOME`, `CLAUDE_CONFIG_DIR`, `COPILOT_HOME`) | env var | One variable; no links | The whole state dir lands in the working tree: Codex `auth.json`/history ([advanced](https://learn.chatgpt.com/docs/config-file/config-advanced)); Claude transcripts/credentials ([claude-directory](https://code.claude.com/docs/en/claude-directory)); Copilot `config.json` auth, `mcp-oauth-config/` ([config dir](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-config-dir-reference)). Safety then depends entirely on an allow-list `.gitignore`. Moving `CLAUDE_CONFIG_DIR` also creates a separate login ([auth](https://code.claude.com/docs/en/authentication)). This is the **current setup** for Codex |
| **E. Claude Code plugin / marketplace from git** | `.claude-plugin/marketplace.json`; `claude plugin marketplace add <owner/repo or ./dir>` ([create marketplace](https://code.claude.com/docs/en/plugin-marketplaces)) | Versioned, installable on any machine; can bundle skills, agents, hooks and MCP. A **local-dir** marketplace loads relative-path plugins **in place**, with no version bump needed ([host](https://code.claude.com/docs/en/plugins/host-marketplace)). Supports `claude plugin validate` and `claude plugin eval` | **Cannot ship CLAUDE.md or rules**: "Claude Code doesn't load a `CLAUDE.md` at the plugin root" ([components](https://code.claude.com/docs/en/plugins/components)). Skills become namespaced `/plugin:skill` ([skills](https://code.claude.com/docs/en/skills)). Git installs are cached copies; auto-update is off by default for third-party marketplaces, and a pinned `version` blocks updates until bumped ([host](https://code.claude.com/docs/en/plugins/host-marketplace)). Claude-only: Codex has its own `.codex-plugin/plugin.json` format ([Codex plugins](https://developers.openai.com/codex/plugins/build)) |
| **F. Single source + thin shims** | One `AGENTS.md`; `CLAUDE.md` = symlink, or a file containing `@AGENTS.md` + Claude-only additions | Claude docs bless both forms and dedupe: "A `CLAUDE.md` symlinked to `AGENTS.md`… Claude reads the content once" ([memory](https://code.claude.com/docs/en/memory)) | The `@` shim only helps Claude; Copilot CLI forbids `~`/absolute imports ([CLI instructions](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions)); Codex documents no imports |

**[Rec]** Use **A + F** for personal use, and add **E** only for a subset you want to share or install on machines you don't control. At user scope, a symlinked `~/.claude/CLAUDE.md → AGENTS.md` is the simplest shim. Switch to an `@` import shim only when Claude-specific text is needed. User-scope imports load without the approval dialog ([memory](https://code.claude.com/docs/en/memory)).

**[Rec]** Skill-dir duplication: `~/.agents/skills` and `~/.claude/skills` both point at the same `skills/`, and VS Code and Cursor read both. Claude dedupes by target ([skills](https://code.claude.com/docs/en/skills)). Codex dedupes roots by path ([source](https://github.com/openai/codex/blob/c308e127e0e8cd040f670ca0e0496ae1c7519d3f/codex-rs/ext/skills/src/host_roots.rs#L69)). **[Ambiguous]** For Cursor and VS Code, whether a skill reached via two symlinked roots shows twice is undocumented, so verify on install.

---

## What to exclude

Never commit:
- **Claude Code**: `.credentials.json` (Linux/Windows, or the macOS fallback); `~/.claude.json` (sign-in session, MCP config, per-project trust); `projects/` (transcripts plus auto memory); `history.jsonl`; `file-history/`; `paste-cache/`; `shell-snapshots/`; `session-env/`; `sessions/`; `plans/`; `debug/`; `backups/`; `stats-cache.json`; `remote-settings.json`; `policy-limits.json*`; `plugins/` (cache, `installed_plugins.json`); `jobs/`; `daemon/`; `tasks/`; `usage-data/`; `skills/synced/`; `skills/.trash/`; `*.heapsnapshot`. "Transcripts and history are not encrypted at rest" ([claude-directory](https://code.claude.com/docs/en/claude-directory), [settings](https://code.claude.com/docs/en/settings)). Also never commit `.claude/settings.local.json`; Claude adds `**/.claude/settings.local.json` to your global git excludes when it writes one ([settings](https://code.claude.com/docs/en/settings)).
- **Codex**: `auth.json` ("treat … like a password", [auth](https://developers.openai.com/codex/auth)); `history.jsonl`; `sessions/`; `log/`; `*.sqlite*`; `cache/`; `.tmp/`; `tmp/`; `plugins/` cache; `skills/.system/` (bundled); `rules/default.rules` (TUI-written approvals) ([advanced](https://learn.chatgpt.com/docs/config-file/config-advanced), [rules](https://learn.chatgpt.com/docs/agent-configuration/rules)). Also `models_cache.json`, `installation_id`, `version.json`, `.codex-global-state.json*` and the various `*-locks`/`ipc`/`app-server-*` dirs (observed locally, undocumented).
- **Copilot CLI**: `config.json` (contains auth), `mcp-oauth-config/`, `mcp-secrets/`, `session-state/`, `session-store.db`, `logs/`, `command-history-state/`, `permissions-config.json`, `installed-plugins/`, `plugin-data/`, `ide/` ([config dir](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-config-dir-reference)).
- **Machine-specific**: Codex `config.toml` (`[projects."<abs path>"]` trust entries, MCP commands); Claude `settings.json` hooks with absolute paths, and `extraKnownMarketplaces` local paths. **[Rec]** Commit a `.example` and keep the live file local. Alternatively, symlink `~/.claude/settings.json` only if you accept that `/config` and `/model` write into the repo. Claude writes user settings there ([settings](https://code.claude.com/docs/en/settings)).

**[Rec]** `.gitignore` for *this* repo (layout A):

```gitignore
# Tool-written state that lands here through symlinked dirs
skills/synced/
skills/.trash/
skills/.system/
codex/rules/default.rules
# Machine-local
*.local.*
local/
.DS_Store
```

**[Rec]** If you version `~/.claude` or `~/.codex` *directly* (approach D), use an allow-list rather than a deny-list, so new state files stay out by default. Your existing `personal/configs/.gitignore` already does this for Codex:

```gitignore
# ~/.claude as repo root
/*
!/CLAUDE.md
!/rules/
!/skills/
/skills/synced/
/skills/.trash/
!/agents/
!/commands/
!/output-styles/
!/.gitignore

# ~/.codex (CODEX_HOME) as repo root
/*
!/AGENTS.md
!/rules/
/rules/default.rules
!/.gitignore
```

---

## Private vs shareable; versioning and testing

- **Separation.** **[Rec]** Keep a single repo with a gitignored `local/` directory, plus `CLAUDE.local.md`-style files for per-machine notes. For per-project private notes in Claude, `CLAUDE.local.md` exists for this purpose, but it only lives in one worktree. Claude docs suggest `@~/.claude/<file>.md` imports to share across worktrees ([memory](https://code.claude.com/docs/en/memory)). Caveat: a `CLAUDE.local.md` makes Claude skip the project `AGENTS.md` unless you set `instructionFiles: claude-md-and-agents-md` ([memory](https://code.claude.com/docs/en/memory)). Keep anything secret out of instruction files entirely. Transcripts capture tool output in plaintext ([claude-directory](https://code.claude.com/docs/en/claude-directory)).
- **Shareable subset.** **[Rec]** Expose shareable skills as a Claude marketplace from this repo: `.claude-plugin/marketplace.json` with a relative-path plugin whose `skills/` symlinks into `skills/`. Symlinks *within the marketplace* are dereferenced into the cache on install, and links pointing outside it are skipped ([host](https://code.claude.com/docs/en/plugins/host-marketplace)). Others can then use `claude plugin marketplace add rabuckley/agents`.
- **Versioning.** `metadata.version` is the standard's slot for a skill version ([spec](https://agentskills.io/specification)). For Claude plugins, either bump `version` on each release or omit it so users track commits. Don't set it in both `plugin.json` and the marketplace entry ([host](https://code.claude.com/docs/en/plugins/host-marketplace)).
- **Testing.**
  - Static: `skills-ref validate ./skills/<n>` ([spec](https://agentskills.io/specification)) and `claude plugin validate .` ([create marketplace](https://code.claude.com/docs/en/plugin-marketplaces)).
  - Behavioural: `claude plugin eval` (v2.1.269+; cases in the plugin's `evals/`, which can include a grader asserting the skill fired) ([evals](https://code.claude.com/docs/en/plugin-evals)). For a single skill there is the skill-creator plugin with its own `evals/evals.json` format, and the two formats aren't interchangeable (same).
  - Codex rules: `codex execpolicy check --rules … -- <cmd>` ([rules](https://learn.chatgpt.com/docs/agent-configuration/rules)).
  - Instruction quality: `/doctor prompt-audit` audits CLAUDE.md, AGENTS.md, rules and skills ([memory](https://code.claude.com/docs/en/memory)).
  - **[Rec]** Run validate plus evals in CI on this repo.

---

## Open questions

1. **Cursor user rules on disk.** The docs describe only the Customize UI ([rules](https://cursor.com/docs/context/rules)), so plain files can't sync personal Cursor rules. Is there an undocumented `~/.cursor/rules`? Unverified.
2. **Cursor/VS Code: does a global `AGENTS.md` exist?** Neither documents one. Copilot CLI uses `~/.copilot/copilot-instructions.md` instead.
3. **Claude relative `@` imports from a symlinked `~/.claude/CLAUDE.md`.** Do they resolve against the link or the target? The docs say "relative to the file containing the import" ([memory](https://code.claude.com/docs/en/memory)). Test before relying on it.
4. **Codex `AGENTS.md` imports.** Not documented ([agents-md](https://learn.chatgpt.com/docs/agent-configuration/agents-md)). Assume none.
5. **Codex user skills.** Docs list only `~/.agents/skills` ([skills](https://learn.chatgpt.com/docs/build-skills)). Source keeps `$CODEX_HOME/skills` as deprecated ([source](https://github.com/openai/codex/blob/c308e127e0e8cd040f670ca0e0496ae1c7519d3f/codex-rs/ext/skills/src/host_roots.rs#L95-L108)), and your current skills sit there. Migrate to `~/.agents/skills` before removal.
6. **`COPILOT_SKILLS_DIRS`.** It surfaced in search results attributed to the CLI command reference, but I did not verify it on a fetched page. The CLI skills page instead documents `/skills add` ([CLI skills](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-skills)).
7. **Codex `#32658`** (symlinked `.rules` files ignored) is open as of 2026-09-15 ([issue](https://github.com/openai/codex/issues/32658)). Re-check before switching to per-file links.

---

## Sources

- Claude Code: [memory / CLAUDE.md / rules / AGENTS.md](https://code.claude.com/docs/en/memory) · [skills](https://code.claude.com/docs/en/skills) · [settings](https://code.claude.com/docs/en/settings) · [.claude directory](https://code.claude.com/docs/en/claude-directory) · [authentication](https://code.claude.com/docs/en/authentication) · [env vars](https://code.claude.com/docs/en/env-vars) · [plugins overview](https://code.claude.com/docs/en/plugins) · [plugin components](https://code.claude.com/docs/en/plugins/components) · [create marketplace](https://code.claude.com/docs/en/plugin-marketplaces) · [host marketplace](https://code.claude.com/docs/en/plugins/host-marketplace) · [plugin evals](https://code.claude.com/docs/en/plugin-evals)
- OpenAI Codex: [AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md) · [skills](https://learn.chatgpt.com/docs/build-skills) · [rules](https://learn.chatgpt.com/docs/agent-configuration/rules) · [config basics](https://learn.chatgpt.com/docs/config-file/config-basic) · [advanced config](https://learn.chatgpt.com/docs/config-file/config-advanced) · [auth](https://developers.openai.com/codex/auth) · [plugins](https://developers.openai.com/codex/plugins/build) · source: [host_roots.rs](https://github.com/openai/codex/blob/c308e127e0e8cd040f670ca0e0496ae1c7519d3f/codex-rs/ext/skills/src/host_roots.rs), [instructions/mod.rs](https://github.com/openai/codex/blob/c308e127e0e8cd040f670ca0e0496ae1c7519d3f/codex-rs/codex-home/src/instructions/mod.rs), [exec_policy.rs](https://github.com/openai/codex/blob/c308e127e0e8cd040f670ca0e0496ae1c7519d3f/codex-rs/core/src/exec_policy.rs) · [issue #32658](https://github.com/openai/codex/issues/32658)
- GitHub Copilot: [repo instructions](https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions) · [personal instructions](https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-personal-instructions) · [support matrix](https://docs.github.com/en/copilot/reference/custom-instructions-support) · [CLI instructions](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions) · [CLI skills](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-skills) · [CLI config dir](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-config-dir-reference) · VS Code: [instructions](https://code.visualstudio.com/docs/copilot/customization/custom-instructions), [skills](https://code.visualstudio.com/docs/copilot/customization/agent-skills)
- Cursor: [rules](https://cursor.com/docs/context/rules) · [skills](https://cursor.com/docs/context/skills) (`docs.cursor.com` 308-redirects to `cursor.com/docs`)
- Standards: [agents.md](https://agents.md/) · [Agent Skills spec](https://agentskills.io/specification)
- Dotfile tools: [GNU Stow manual](https://www.gnu.org/software/stow/manual/stow.html) · [chezmoi target types](https://www.chezmoi.io/reference/target-types/)
