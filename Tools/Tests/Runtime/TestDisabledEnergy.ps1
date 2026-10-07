# Managed regression checks only; no live profile or Unity scene is loaded.
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../Shared/LoadUnityManagedAssemblies.ps1')
$projectPath = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$null = Import-SF2ManagedRuntime $projectPath
$checks = 0
function Assert-Energy($condition, [string]$message) {
    if (!$condition) { throw $message }
    $script:checks++
}
foreach ($name in @('usersDefault.xml', 'usersDefaultWarrior.xml')) {
    [xml]$xml = Get-Content -Raw (Join-Path $projectPath ('Assets/vanillaXml/' + $name))
    $item = $xml.SelectSingleNode('//Warrior/Items/Item[@Name="Unlimited_Energy"]')
    Assert-Energy ($null -ne $item -and [int]$item.Count -gt 0) ($name + ' lacks unlimited energy')
}
# Bypass unrelated profile construction. Zero backing fields model a depleted
# old save without its entitlement; no profile XML is available for timer writes.
$roster = [Runtime.Serialization.FormatterServices]::GetUninitializedObject([Roster])
$roster.PowerMax = 5
$roster.PowerRegenSeconds = 0
$roster.HasUnlimitedEnergy = $false
Assert-Energy $roster.HasUnlimitedEnergy 'Legacy writes can disable unlimited energy'
foreach ($value in @([int]::MinValue, -100, -1, 0, 1, 100, [int]::MaxValue)) {
    Assert-Energy ($roster.ChangePower($value)) 'Energy cost rejected'
    $roster.SetPower($value)
    Assert-Energy ($roster.GetMaxPower() -eq 5) 'Energy changed from full'
}
foreach ($time in @([long]::MinValue, -1L, 0L, 1720000000L, [long]::MaxValue)) {
    $roster.UpdatePowerRegeneration($time)
    Assert-Energy ($roster.GetEnergyRefillTimer() -eq -1) 'Energy regeneration timer is active'
    Assert-Energy ($roster.GetMaxPower() -eq 5) 'Clock changed energy'
}
$notifications = [Runtime.Serialization.FormatterServices]::GetUninitializedObject([LocalNotificationManager])
$notifications.ScheduleEnergy(0L)
$notifications.ScheduleEnergyFull(0L)
$checks += 2 # No Unity scheduler, localization, or live state is available.
Write-Output "PASS: $checks disabled-energy assertions (fresh templates, depleted roster, extreme costs/timestamps, no refill notifications)."
