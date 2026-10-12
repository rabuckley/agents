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

if ($skipped) { exit 1 }
