# Runs the actual compiled managed runtime, without launching/controlling Unity
# or reading/writing a save. Build Assembly-CSharp.csproj before running.
param([string]$AssemblyDirectory = '', [string]$UnityManagedDirectory = '')
$ErrorActionPreference = 'Stop'
$projectPath = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
. (Join-Path $PSScriptRoot '../Shared/LoadUnityManagedAssemblies.ps1')
$null = Import-SF2ManagedRuntime $projectPath $AssemblyDirectory $UnityManagedDirectory
$checks = 0
function Assert-Near($actual, $expected, $label) {
    if ([Math]::Abs($actual - $expected) -gt 0.0002) { throw "${label}: expected $expected, got $actual" }
    $script:checks++
}
foreach ($count in @(0, 1, 15, 20, 23, 28, 36, 57)) {
    $model = New-Object ModelParameters
    $model.MaxLife = 1
    $model.ShieldTotal = $count
    $model.HasShieldTotalOverride = $true
    $model.SetCurrentLife(1)
    $bars = [Math]::Max(1, $count)
    Assert-Near $model.RemainingHealthBars $bars 'initial count'
    Assert-Near $model.CurrentHealthBarFraction 1 'initial bar'
    $clone = $model.Clone()
    Assert-Near $clone.ShieldTotal $count 'clone shield count'
    if (!$clone.HasShieldTotalOverride) { throw 'Clone lost explicit shield override' }
    $model.ChangeLife(-0.25)
    Assert-Near $model.CurrentHealthBarFraction 0.75 'quarter bar damage'
    Assert-Near $model.GetLifeRatio() (1 - 0.25 / $bars) 'pool damage scaling'
    $model.ChangeLife(0.25)
    Assert-Near $model.CurrentHealthBarFraction 1 'healing'
    if ($bars -gt 1) {
        $model.ChangeLife(-1.5)
        Assert-Near $model.RemainingHealthBars ($bars - 1) 'cross-bar count'
        Assert-Near $model.CurrentHealthBarFraction 0.5 'cross-bar carry'
        if ($model.GetLifeDepleted()) { throw 'Boss died with shields remaining' }
    }
    $model.RestoreFullLife()
    Assert-Near $model.RemainingHealthBars $bars 'round reset'
    for ($i = 1; $i -le $bars; $i++) {
        $model.ChangeLife(-1)
        Assert-Near $model.RemainingHealthBars ($bars - $i) 'whole bar loss'
    }
    if (!$model.GetLifeDepleted()) { throw "Boss survived all $bars bars" }
    Assert-Near $model.CurrentHealthBarFraction 0 'empty bar'
    $model.RestoreFullLife()
    $null = $model.UpdateLife(-$bars)
    if (!$model.GetLifeDepleted()) { throw 'Alternate damage entry point bypasses shields' }
}
# Exercise the real strike limiter BEFORE applying pool damage. Earlier tests
# only applied deltas directly and therefore missed the late-fight damage cap.
foreach ($count in @(0, 1, 15, 40, 57)) {
    foreach ($rawDamage in @(0.25, 2.5)) {
        $model = New-Object ModelParameters
        $model.MaxLife = 1
        $model.ShieldTotal = $count
        $model.SetCurrentLife(1)
        $bars = [Math]::Max(1, $count)
        $expectedHits = [int][Math]::Ceiling($bars / $rawDamage)
        for ($hit = 0; $hit -lt $expectedHits; $hit++) {
            $before = $model.RemainingHealthInDamageUnits
            $overkill = $false
            $applied = $model.ResolveStrikeDamage($rawDamage, [ref]$overkill)
            if ($before -ge $rawDamage) {
                Assert-Near $applied $rawDamage 'identical hits retain full damage throughout pool'
                if ($overkill) { throw 'Nonlethal hit falsely marked overkill' }
            } else {
                Assert-Near $applied ($before + 0.01) 'true final overkill retains legacy margin'
                if (!$overkill) { throw 'Final overkill not marked' }
            }
            $model.ChangeLife(-$applied)
            Assert-Near $model.RemainingHealthInDamageUnits ([Math]::Max([double]0, [double]$before - [double]$applied)) 'strike pool delta'
        }
        if (!$model.GetLifeDepleted()) { throw "Damage tapered: $count bars survived $expectedHits equal hits" }
    }
}
$model = New-Object ModelParameters
$model.MaxLife = 1
$model.ShieldTotal = 40
foreach ($remainingBars in @(40.0, 20.0, 5.0, 1.0, 0.5)) {
    $model.SetCurrentLife($remainingBars / 40)
    $overkill = $false
    Assert-Near $model.ResolveStrikeDamage(0.25, [ref]$overkill) 0.25 'same quarter-bar hit at all health levels'
    if ($overkill) { throw 'Last-bar ordinary hit incorrectly clamped' }
}
# Reproduce the old unit mismatch at one bar left: 0.25 was reduced to 0.035.
$model.SetCurrentLife(1.0 / 40)
Assert-Near ($model.GetLifeRatio() + 0.01) 0.035 'old normalized-life cap reproduction'
$model.SetCurrentLife(0.1 / 40)
$overkill = $false
Assert-Near $model.ResolveStrikeDamage(0.25, [ref]$overkill) 0.11 'last partial bar is killable in one hit'
$priorCulture = [Threading.Thread]::CurrentThread.CurrentCulture
try {
    [Threading.Thread]::CurrentThread.CurrentCulture = [Globalization.CultureInfo]::GetCultureInfo('pl-PL')
    $frame = New-Object 'CocosAnimationData+SpriteFrameCocos'
    foreach ($value in @('{1024,512}', '{{1024,512}}', ' { 1024.0, 512.0 } ')) {
        $frame.SetSourceSize($value)
        Assert-Near $frame.GetSourceSize().x 1024 'source width'
        Assert-Near $frame.GetSourceSize().y 512 'source height'
    }
    $frame.SetOffset('{-1.5,2.25}')
    Assert-Near $frame.GetOffset().x -1.5 'offset x'
    Assert-Near $frame.GetOffset().y 2.25 'offset y'
    foreach ($bad in @('{NaN,1}', '{Infinity,1}', '{1}', 'not a size')) {
        $rejected = $false
        try { $frame.SetSourceSize($bad) } catch { $rejected = $true }
        if (!$rejected) { throw "Malformed vector accepted: $bad" }
        $checks++
    }
} finally { [Threading.Thread]::CurrentThread.CurrentCulture = $priorCulture }
Write-Output "PASS: $checks runtime assertions (uniform strike damage, overkill, health pools, carry, healing, death, reset, cloning, Cocos parsing)."
