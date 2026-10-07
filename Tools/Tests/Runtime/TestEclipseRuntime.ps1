# Build Assembly-CSharp.csproj first. Exercises the compiled replay progression
# with real stage definitions and in-memory roster XML. The extracted Eclipse
# action uses fake scene/quest/save services; no Unity UI or player saves.
$ErrorActionPreference = 'Stop'
$projectPath = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
. (Join-Path $PSScriptRoot '../Shared/LoadUnityManagedAssemblies.ps1')
$assembly = Import-SF2ManagedRuntime $projectPath
$checks = 0
function Assert-True($condition, [string]$label) {
    if (!$condition) { throw $label }
    $script:checks++
}
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
function Set-BattleField($battle, [string]$field, $value) {
    [Battle].GetField($field, $flags).SetValue($battle, $value.PSObject.BaseObject)
}
function New-BattleFixture([System.Xml.XmlElement]$definition, [int]$wins = 0, [int]$cycle = 0) {
    $type = switch ($definition.GetAttribute('Type')) {
        'REPLAYABLE' { [BattleType]::FightReplayable }
        'BOSSES_REPLAYABLE' { [BattleType]::FightBossesReplayable }
        'FINAL_BATTLE_REPLAYABLE' { [BattleType]::FightFinalReplayable }
        default { [BattleType]::FightBosses }
    }
    $runtimeType = if ($type -in @([BattleType]::FightReplayable, [BattleType]::FightBossesReplayable, [BattleType]::FightFinalReplayable)) { [BattleReplayable] } else { [Battle] }
    $battle = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($runtimeType)
    Set-BattleField $battle '_name' $definition.GetAttribute('Name')
    Set-BattleField $battle '_type' $type
    $data = New-Object DeflatedString
    $data.Set($definition)
    Set-BattleField $battle '_sourceDefinition' $data
    [xml]$save = '<Battle Name="ZONE_2|Test|" ReplayCount="0" Locked="0" Hidden="0" />'
    $roster = New-Object RosterBattle -ArgumentList $save.DocumentElement
    $roster.SetReplayCount($cycle)
    $battle.SetRosterBattle($roster)
    $fights = New-Object 'System.Collections.Generic.List[FightList]'
    foreach ($node in $definition.SelectNodes('Fight')) {
        $fight = [Runtime.Serialization.FormatterServices]::GetUninitializedObject([FightList])
        $fight.Name = $node.GetAttribute('Name')
        $fight.Index = $fights.Count
        $fight.Battle = $battle
        $fight.ReplayCount = [int]$node.GetAttribute('Replays')
        $fight.set_Type($type)
        [xml]$fightSave = '<Fight CompletedCount="0" EclipseCompletedCount="0" LossCount="3" EclipseLossCount="2" StoryCount="4" RandomGroupSeed="123" RandomRuleSeed="456" />'
        $rosterFight = New-Object RosterFight -ArgumentList $fightSave.DocumentElement
        $rosterFight.SetWinCount($wins * $fight.ReplayCount)
        $rosterFight.SetEclipseWinCount($wins * $fight.ReplayCount)
        $fight.SetRosterFight($rosterFight)
        if ($battle -is [BattleReplayable]) { $battle.RefreshFightStatus($fight) }
        else { $fight.Status = [ConditionStatus]::StatusComplete }
        $fights.Add($fight)
    }
    Set-BattleField $battle '_fights' $fights
    Set-BattleField $battle '_fightCount' ([ushort]$fights.Count)
    Set-BattleField $battle '_fightsParsed' $true
    return $battle
}

[xml]$stages = Get-Content -Raw (Join-Path $projectPath 'Assets/vanillaXml/stages.xml')
[xml]$equipRuleXml = '<EquipItem Type="Ranged" MinLevel="99" />'
[xml]$requireRuleXml = '<RequireItem Type="Ranged" MinLevel="11" />'
function New-ItemRuleSourceFixture([System.Xml.XmlElement]$definition) {
    $rule = [Runtime.Serialization.FormatterServices]::GetUninitializedObject([ItemRule])
    $data = New-Object DeflatedString
    $data.Set($definition)
    [Rule].GetField('xmlSource', $flags).SetValue($rule, $data)
    return $rule
}
$equipRule = New-ItemRuleSourceFixture $equipRuleXml.DocumentElement
$requireRule = New-ItemRuleSourceFixture $requireRuleXml.DocumentElement
Assert-True (!$equipRule.IsEntryRequirement()) 'EquipItem was mistaken for an entry requirement'
Assert-True ($requireRule.IsEntryRequirement()) 'RequireItem was not recognized as an entry requirement'
$duelFight = New-Object FightList
$duelFight.set_Type([BattleType]::FightPeriodic)
Assert-True ($duelFight.MeetsPlayerItemRequirements($null)) 'Duel was blocked by the challenge-only item gate'
[xml]$legacyBossSave = @'
<Warrior>
  <Battles>
    <Battle Name="ZONE_1|BOSS_HARDMODE|" Locked="0" Hidden="0" ReplayCount="0" />
    <Battle Name="ZONE_2|BOSS_HARDMODE|" Locked="0" Hidden="0" ReplayCount="0" />
    <Battle Name="ZONE_2|BOSS_HERMIT|" Locked="0" Hidden="0" ReplayCount="0" />
    <Battle Name="ZONE_3|BOSS_HARDMODE|" Locked="0" Hidden="0" ReplayCount="0" />
  </Battles>
  <Fights>
    <Fight IDS="ZONE_1|BOSS_LYNX|6" CompletedCount="1" />
    <Fight IDS="ZONE_2|BOSS_HERMIT|6" CompletedCount="1" />
    <Fight IDS="ZONE_3|BOSS_BUTCHER|6" CompletedCount="0" />
  </Fights>
</Warrior>
'@
$repairs = [Eclipse.Content.QuestCompatibility]::RestoreUnsupportedHardmodeBosses($legacyBossSave.DocumentElement)
Assert-True ($repairs -eq 4) 'Legacy hardmode repair changed an unexpected number of nodes'
Assert-True ($legacyBossSave.SelectNodes('//Battle[contains(@Name,"BOSS_HARDMODE")]').Count -eq 0) 'Unsupported hardmode entry survived repair'
Assert-True ($legacyBossSave.SelectNodes('//Battle[@Name="ZONE_1|BOSS_LYNX|"]').Count -eq 1) 'Completed Lynx entry was not restored'
Assert-True ($legacyBossSave.SelectNodes('//Battle[@Name="ZONE_2|BOSS_HERMIT|"]').Count -eq 1) 'Existing Hermit entry was duplicated'
Assert-True ($legacyBossSave.SelectNodes('//Battle[@Name="ZONE_3|BOSS_BUTCHER|"]').Count -eq 0) 'Incomplete boss was exposed by repair'
Assert-True ([Eclipse.Content.QuestCompatibility]::RestoreUnsupportedHardmodeBosses($legacyBossSave.DocumentElement) -eq 0) 'Legacy hardmode repair was not idempotent'
[xml]$eligibleEclipseSave = @'
<Warrior EclipseMode="Off">
  <MapButtons />
  <Quests><Variables><Variable Name="ForgeEnabled" Value="2" /></Variables></Quests>
  <Fights><Fight IDS="ZONE_2|Tournament|4" CompletedCount="1" /></Fights>
</Warrior>
'@
Assert-True ([Eclipse.Content.QuestCompatibility]::EnsureEligibleEclipseButton($eligibleEclipseSave.DocumentElement)) 'Eligible save did not recover the Eclipse button'
$eclipseButton = $eligibleEclipseSave.SelectSingleNode('//MapButtons/Button')
Assert-True ($eclipseButton.GetAttribute('Name') -eq 'EclipseModeOn') 'Recovered Eclipse button has the wrong mode'
Assert-True ($eclipseButton.GetAttribute('Image') -eq 'sun_icon') 'Recovered Eclipse button does not use the atlas sprite name'
Assert-True (![Eclipse.Content.QuestCompatibility]::EnsureEligibleEclipseButton($eligibleEclipseSave.DocumentElement)) 'Eclipse button recovery was not idempotent'
$definitions = $stages.SelectNodes('//Battle[contains(@Name,"ECLIPSEMODE") and (@Type="REPLAYABLE" or @Type="BOSSES_REPLAYABLE" or @Type="FINAL_BATTLE_REPLAYABLE")]')
foreach ($definition in $definitions) {
    $battle = New-BattleFixture $definition
    Assert-True (!$battle.TryStartNextReplay()) ($battle.get_Name() + ': untouched battle advanced')
    for ($cycle = 1; $cycle -le 3; $cycle++) {
        $fights = $battle.GetFights()
        foreach ($fight in $fights) {
            for ($win = 0; $win -lt $fight.ReplayCount; $win++) {
                $fight.GetRosterFight().RecordWin()
                $fight.GetRosterFight().IncrementEclipseWinCount()
            }
            $battle.RefreshFightStatus($fight)
            if ($fight -ne $fights[$fights.Count - 1]) {
                Assert-True (!$battle.TryStartNextReplay()) ($battle.get_Name() + ': partial segment reset')
            }
        }
        $before = @($fights | ForEach-Object { $_.GetRosterFight().GetNode().OuterXml }) -join "`n"
        Assert-True ($battle.TryStartNextReplay()) ($battle.get_Name() + ': completed segment did not reopen')
        Assert-True ($battle.GetCompletedCycles() -eq $cycle) 'Wrong replay cycle'
        Assert-True ($battle.GetFirstOpenFight() -eq $fights[0]) 'Replay did not restart at first opponent'
        Assert-True ($battle.GetStatus() -eq [ConditionStatus]::StatusOpen) 'Battle remains completed'
        $after = @($fights | ForEach-Object { $_.GetRosterFight().GetNode().OuterXml }) -join "`n"
        Assert-True ($before -ceq $after) 'Replay changed lifetime wins/losses, seeds or story counters'
        Assert-True (!$battle.TryStartNextReplay()) 'Replay update was not idempotent'
    }
}

$hermit = $stages.SelectSingleNode('//Zone[@Name="ZONE_2"]/Battle[@Name="BOSS_HERMIT_ECLIPSEMODE"]')
$battle = New-BattleFixture $hermit 4 0
Assert-True ($battle.TryStartNextReplay()) 'Stale completed save did not recover'
Assert-True ($battle.GetCompletedCycles() -eq 4) 'Stale cycle did not catch up to saved wins'
$rosterNode = [RosterBattle].GetField('_node', $flags).GetValue($battle.GetRosterBattle())
$reloaded = New-Object RosterBattle -ArgumentList $rosterNode.CloneNode($true)
Assert-True ($reloaded.GetReplayCount() -eq 4) 'ReplayCount was not serialized'
$battle.SetRosterBattle($reloaded)
Assert-True (!$battle.TryStartNextReplay()) 'Reload advanced the cycle again'
$battle.GetFights()[0].GetRosterFight().RecordWin()
$battle.RefreshFightStatus($battle.GetFights()[0])
Assert-True (!$battle.TryStartNextReplay()) 'Partially replayed bodyguards reset'
Assert-True ($battle.GetFirstOpenFight().Index -eq 1) 'Partial replay lost its next opponent'
$battle = New-BattleFixture $hermit 1
$battle.GetRosterBattle().SetLocked($true)
Assert-True (!$battle.TryStartNextReplay()) 'Explicitly locked battle was reopened'
$battle = New-BattleFixture $hermit 1
Set-BattleField $battle '_rosterBattle' $null
Assert-True (!$battle.TryStartNextReplay()) 'Missing roster was reopened'
$battle = New-BattleFixture $hermit 1
[FightList].GetField('rosterFight', $flags).SetValue($battle.GetFights()[0], $null)
Assert-True (!$battle.TryStartNextReplay()) 'Missing fight progress was treated as complete'
$battle = New-BattleFixture $hermit 1
$battle.GetFights()[0].ReplayCount = 0
Assert-True (!$battle.TryStartNextReplay()) 'Unlimited fight was treated as a finite segment'

# Run the production Eclipse action with actual Battle/RosterFight objects.
# Only its external services are replaced, allowing map-refresh/save assertions.
$actionSource = Get-Content -Raw (Join-Path $projectPath 'Assets/Scripts/Assembly-CSharp/QuestAction.cs')
$action = [regex]::Match($actionSource, '(?ms)^public class QuestActionUpdateEclipseBattles : QuestAction\r?\n\{.*?^\}')
Assert-True $action.Success 'Cannot find Eclipse update action'
$shim = @'
using System;
using System.Collections.Generic;
using System.Xml;
namespace EclipseRuntimeTest {
    public class QuestParameters { }
    public class QuestAction {
        public virtual void Execute(QuestParameters parameters) { }
        protected void FinishAction() { }
    }
    public class Roster {
        public bool EclipseMode = true;
		public int Adds;
        public bool IsEclipseMode() { return EclipseMode; }
		public void AddBattle(Battle battle, bool unique, bool show, bool locked, bool hidden, int replayCount) {
			XmlDocument save = new XmlDocument();
			save.LoadXml("<Battle Name='test' Locked='0' Hidden='0' ReplayCount='0' />");
			RosterBattle roster = new RosterBattle(save.DocumentElement);
			roster.SetHidden(hidden);
			roster.SetReplayCount(replayCount);
			battle.SetRosterBattle(roster);
			Adds++;
		}
    }
    public class ListSF {
        public static ListSF Instance = new ListSF();
        public static Roster Roster = new Roster();
        public List<Battle> Battles = new List<Battle>();
        public int Saves;
        public static ListSF GetInstance() { return Instance; }
        public static Roster GetRoster() { return Roster; }
        public List<Battle> GetBattles() { return Battles; }
        public void RequestSave() { Saves++; }
    }
    public class Scene<T> where T : new() {
        public static T Current = new T();
        public static T get_Current() { return Current; }
    }
    public class ZoneScrollItem {
        public Battle Selected;
        public Battle get_LastBattle() { return Selected; }
    }
    public class MapScene {
        public ZoneScrollItem Zone = new ZoneScrollItem();
        public int Reselections;
        public ZoneScrollItem GetCurrentZone() { return Zone; }
        public void UpdateBattleButtonHidden(Battle battle) { }
        public void UpdateCurrentZone() { }
        public void SelectBattle(Battle battle, float duration) { Zone.Selected = battle; Reselections++; }
    }
/* ACTION */
}
'@
$refs = @($assembly.Location)
if ($PSVersionTable.PSEdition -eq 'Core') {
    foreach ($name in @('mscorlib', 'System.Collections', 'System.Xml', 'System.Xml.ReaderWriter', 'System.Xml.XmlDocument')) {
        $refs += Join-Path $PSHOME ('ref/' + $name + '.dll')
    }
} else { $refs += 'System.Xml' }
Add-Type -TypeDefinition $shim.Replace('/* ACTION */', $action.Value) -ReferencedAssemblies $refs
$normal = New-BattleFixture $stages.SelectSingleNode('//Zone[@Name="ZONE_2"]/Battle[@Name="BOSS_HERMIT"]') 1
$eclipse = New-BattleFixture $hermit 1
$eclipse.SetRosterBattle($null)
$zone = New-Object Zone -ArgumentList @('ZONE_2','test')
foreach ($entry in @($normal, $eclipse)) {
    $entry.SetZone($zone)
    $zone.Battles.Add($entry)
    [EclipseRuntimeTest.ListSF]::Instance.Battles.Add($entry)
}
$normal.GetRosterBattle().SetHidden($true)
$map = [EclipseRuntimeTest.Scene[EclipseRuntimeTest.MapScene]]::Current
$map.Zone.Selected = $eclipse
$update = New-Object EclipseRuntimeTest.QuestActionUpdateEclipseBattles
$update.Execute($null)
Assert-True ([EclipseRuntimeTest.ListSF]::Roster.Adds -eq 1) 'Missing Eclipse roster entry was not introduced'
Assert-True ($eclipse.GetCompletedCycles() -eq 1) 'Eclipse action did not advance completed Hermit'
Assert-True ([EclipseRuntimeTest.ListSF]::Instance.Saves -eq 1) 'Progress-only change was not saved'
Assert-True ($map.Reselections -eq 1) 'Progress-only change did not rebuild selected preview'
Assert-True ($normal.GetStatus() -eq [ConditionStatus]::StatusComplete) 'Normal story completion changed'
$update.Execute($null)
Assert-True ([EclipseRuntimeTest.ListSF]::Instance.Saves -eq 1) 'Repeated update rewrote progress'
$eclipse.GetFights()[0].GetRosterFight().RecordWin()
$eclipse.RefreshFightStatus($eclipse.GetFights()[0])
foreach ($mode in @($false, $true, $false, $true)) {
    [EclipseRuntimeTest.ListSF]::Roster.EclipseMode = $mode
    $update.Execute($null)
    Assert-True ($eclipse.GetCompletedCycles() -eq 1) 'Mode toggle reset a partial replay'
    Assert-True ($eclipse.GetFirstOpenFight().Index -eq 1) 'Mode toggle lost bodyguard progress'
    Assert-True ($eclipse.IsHidden() -eq !$mode) 'Eclipse visibility did not follow mode'
}
# Revealing a future story entry must not unlock its Eclipse counterpart.
$savedCounterpart=$eclipse.GetRosterBattle()
$adds=[EclipseRuntimeTest.ListSF]::Roster.Adds
$eclipse.SetRosterBattle($null)
$normal.GetRosterBattle().SetLocked($true)
foreach($mode in @($false,$true,$false,$true)) {
    [EclipseRuntimeTest.ListSF]::Roster.EclipseMode=$mode
    $normal.GetRosterBattle().SetHidden($true)
    $map.Zone.Selected=$normal
    $update.Execute($null)
    Assert-True ([EclipseRuntimeTest.ListSF]::Roster.Adds -eq $adds) 'Locked source introduced an unlocked counterpart'
    Assert-True ($null -eq $eclipse.GetRosterBattle()) 'Locked source acquired a saved counterpart'
    Assert-True (!$normal.IsHidden() -and $normal.GetRosterBattle().IsLocked()) 'Locked source disappeared or unlocked on mode switch'
}
# Old saves may already contain an exposed counterpart. Hide it without resetting
# its own lock, completed rounds or replay history, and repair selected preview.
$eclipse.SetRosterBattle($savedCounterpart)
$savedCounterpart.SetLocked($false)
$savedCounterpart.SetHidden($false)
$map.Zone.Selected=$eclipse
$roundBefore=$eclipse.GetFirstOpenFight().Index
$replayBefore=$eclipse.GetCompletedCycles()
$update.Execute($null)
Assert-True ($eclipse.IsHidden() -and !$normal.IsHidden()) 'Locked source left old Eclipse counterpart visible'
Assert-True ([object]::ReferenceEquals($map.Zone.Selected,$normal)) 'Hidden Eclipse selection did not return to locked normal entry'
Assert-True (!$savedCounterpart.IsLocked() -and $eclipse.GetCompletedCycles() -eq $replayBefore -and $eclipse.GetFirstOpenFight().Index -eq $roundBefore) 'Lock gate reset counterpart progress'
$savesBefore=[EclipseRuntimeTest.ListSF]::Instance.Saves
$update.Execute($null)
Assert-True ([EclipseRuntimeTest.ListSF]::Instance.Saves -eq $savesBefore) 'Repeated locked update rewrote profile'
$normal.GetRosterBattle().SetLocked($false)
$savedCounterpart.SetLocked($true)
$update.Execute($null)
Assert-True (!$eclipse.IsHidden() -and $normal.IsHidden() -and $savedCounterpart.IsLocked()) 'Unlocked source overwrote independently locked counterpart'
Assert-True ($eclipse.GetCompletedCycles() -eq $replayBefore -and $eclipse.GetFirstOpenFight().Index -eq $roundBefore) 'Unlock reset counterpart history'

# A saved intermission source owns a shared counterpart even if the obsolete base
# source remains locked. Exercise both enumeration orders.
$intermissionNode=$stages.SelectSingleNode('//Zone[@Name="ZONE_2"]/Battle[@Name="BOSS_HERMIT"]').CloneNode($true)
$intermissionNode.SetAttribute('Name','BOSS_HERMIT_INTERMISSION')
$intermission=New-BattleFixture $intermissionNode 1
$intermission.SetZone($zone)
$zone.Battles.Add($intermission)
$normal.GetRosterBattle().SetLocked($true)
foreach($reverse in @($false,$true)) {
    [EclipseRuntimeTest.ListSF]::Instance.Battles.Clear()
    $ordered=if($reverse){@($intermission,$normal,$eclipse)}else{@($normal,$eclipse,$intermission)}
    foreach($entry in $ordered){[EclipseRuntimeTest.ListSF]::Instance.Battles.Add($entry)}
    foreach($locked in @($false,$true)) {
        $intermission.GetRosterBattle().SetLocked($locked)
        foreach($mode in @($false,$true)) {
            [EclipseRuntimeTest.ListSF]::Roster.EclipseMode=$mode
            $update.Execute($null)
            Assert-True ($eclipse.IsHidden() -eq ($locked -or !$mode)) 'Obsolete base lock overrode intermission visibility'
            Assert-True ($intermission.IsHidden() -eq (!$locked -and $mode)) 'Intermission source visibility differs'
        }
    }
}
# An unlocked but unfinished battle is fought as itself in Eclipse mode (its Eclipse="1"
# rules make it harder). Its replay counterpart must not replace it, even when an old
# save already exposed the counterpart and hid the normal entry.
$openZone = New-Object Zone -ArgumentList @('ZONE_2','open')
$openNormal = New-BattleFixture $stages.SelectSingleNode('//Zone[@Name="ZONE_2"]/Battle[@Name="BOSS_HERMIT"]') 0
$openEclipse = New-BattleFixture $hermit 0
# Non-replayable fixtures start complete; leave the last fight of this one unwon.
$openNormal.GetFights()[$openNormal.GetFights().Count - 1].Status = [ConditionStatus]::StatusOpen
foreach ($entry in @($openNormal, $openEclipse)) { $entry.SetZone($openZone); $openZone.Battles.Add($entry) }
[EclipseRuntimeTest.ListSF]::Instance.Battles.Clear()
foreach ($entry in @($openNormal, $openEclipse)) { [EclipseRuntimeTest.ListSF]::Instance.Battles.Add($entry) }
$openNormal.GetRosterBattle().SetHidden($true)
$openEclipse.GetRosterBattle().SetHidden($false)
$map.Zone.Selected = $openEclipse
[EclipseRuntimeTest.ListSF]::Roster.EclipseMode = $true
$adds = [EclipseRuntimeTest.ListSF]::Roster.Adds
Assert-True ($openNormal.GetStatus() -ne [ConditionStatus]::StatusComplete) 'Fixture battle is unexpectedly complete'
$update.Execute($null)
Assert-True (!$openNormal.IsHidden() -and $openEclipse.IsHidden()) 'Unfinished battle was replaced by its Eclipse replay'
Assert-True ([object]::ReferenceEquals($map.Zone.Selected, $openNormal)) 'Selection did not return to the unfinished battle'
Assert-True ([EclipseRuntimeTest.ListSF]::Roster.Adds -eq $adds) 'Unfinished battle introduced a replay counterpart'
$openEclipse.SetRosterBattle($null)
$update.Execute($null)
Assert-True ([EclipseRuntimeTest.ListSF]::Roster.Adds -eq $adds -and $null -eq $openEclipse.GetRosterBattle()) 'Unfinished battle acquired a saved replay counterpart'
Write-Output "PASS: $checks Eclipse assertions across $($definitions.Count) replay segments (three cycles each, saved progress, action integration, locked pairs, intermission ownership, unfinished battles and map refresh)."
