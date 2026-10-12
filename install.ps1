# Windows counterpart of install.sh: links this repo into each agent tool's
# user-level config location. Safe to re-run. Existing links are repointed;
# real files or dirs are never touched, so move them aside yourself and re-run.
#
# Directories become junctions, which need no privilege. File links are
# symbolic links, which need Developer Mode (Settings > System > For developers)
# or an elevated shell.
#
# Usage: ./install.ps1 [-DryRun]

param([switch]$DryRun)

$ErrorActionPreference = 'Stop'

$repo = $PSScriptRoot
$codexHome = if ($env:CODEX_HOME) { $env:CODEX_HOME } else { Join-Path $HOME '.codex' }
$skipped = $false

function Link([string]$relative, [string]$dst) {
    $src = Join-Path $repo $relative
    $isDir = Test-Path -LiteralPath $src -PathType Container
    $existing = Get-Item -LiteralPath $dst -Force -ErrorAction SilentlyContinue

    if ($existing -and $existing.LinkType) {
        $current = @($existing.Target)[0]
        if ($current -eq $src) {
            Write-Host "ok      $dst"
            return
        }
        Write-Host "relink  $dst -> $src (was $current)"
        if (-not $DryRun) {
            # Delete only the link itself; Remove-Item can recurse into a
            # directory link's target on Windows PowerShell 5.1.
            if ($existing.PSIsContainer) { [System.IO.Directory]::Delete($dst) }
            else { [System.IO.File]::Delete($dst) }
        }
    }
    elseif ($existing) {
        Write-Warning "SKIP    $dst is a real file or dir; move it aside and re-run"
        $script:skipped = $true
        return
    }
    else {
        Write-Host "link    $dst -> $src"
    }

    if ($DryRun) { return }

    New-Item -ItemType Directory -Force -Path (Split-Path $dst) | Out-Null
    $type = if ($isDir) { 'Junction' } else { 'SymbolicLink' }
    try {
        New-Item -ItemType $type -Path $dst -Target $src | Out-Null
    }
    catch {
        Write-Warning "FAIL    $dst : $($_.Exception.Message) (file links need Developer Mode or an elevated shell)"
        $script:skipped = $true
    }
}

Link 'AGENTS.md'    (Join-Path $HOME '.claude/CLAUDE.md')
Link 'skills'       (Join-Path $HOME '.claude/skills')
Link 'claude/rules' (Join-Path $HOME '.claude/rules')

Link 'skills'       (Join-Path $HOME '.agents/skills')

Link 'AGENTS.md'    (Join-Path $codexHome 'AGENTS.md')
# Must be a directory link: Codex ignores symlinked .rules files (openai/codex#32658).
Link 'codex/rules'  (Join-Path $codexHome 'rules')

Link 'AGENTS.md'    (Join-Path $HOME '.copilot/copilot-instructions.md')

# Runs a plugin CLI command unless dry-running; a failure is reported, not fatal.
function Run([string]$exe, [string[]]$arguments) {
    Write-Host "run     $exe $arguments"
    if ($DryRun) { return }
    & $exe @arguments
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "FAIL    $exe $arguments"
        $script:skipped = $true
    }
}

# Third-party skills are installed as plugins rather than vendored into
# skills/, so they update upstream and don't shadow the ones written here.
# Codex and Copilot error on re-adds, so each step checks the tool's list first.
function Plugin([string]$tool, [string]$repoSlug, [string]$marketplace, [string]$name) {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) {
        Write-Host "SKIP    $tool not on PATH; $name@$marketplace not installed"
        return
    }

    $id = "$name@$marketplace"
    switch ($tool) {
        'claude' {
            # Claude's add and install are both no-ops when already done.
            Run claude @('plugin', 'marketplace', 'add', $repoSlug)
            Run claude @('plugin', 'install', $id)
        }
        'codex' {
            if (-not (codex plugin marketplace list 2>$null | Select-String "^$marketplace ")) {
                Run codex @('plugin', 'marketplace', 'add', $repoSlug)
            }
            if (codex plugin list 2>$null | Select-String "^$([regex]::Escape($id)) +installed") {
                Write-Host "ok      codex $id"
            }
            else { Run codex @('plugin', 'add', $id) }
        }
        'copilot' {
            if (-not (copilot plugin marketplace list 2>$null | Select-String " $marketplace " -SimpleMatch)) {
                Run copilot @('plugin', 'marketplace', 'add', $repoSlug)
            }
            if (copilot plugin list 2>$null | Select-String " $id " -SimpleMatch) {
                Write-Host "ok      copilot $id"
            }
            else { Run copilot @('plugin', 'install', $id) }
        }
    }
}

foreach ($tool in 'claude', 'codex', 'copilot') {
    Plugin $tool 'mattpocock/skills' 'mattpocock' 'mattpocock-skills'
}

if ($skipped) { exit 1 }
