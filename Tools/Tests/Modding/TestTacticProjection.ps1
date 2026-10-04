param([string]$CompileProject='', [string]$AssemblyDirectory='', [string]$UnityManagedDirectory='')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
if(!$CompileProject){$CompileProject=Join-Path $root 'Assembly-CSharp.csproj'}
dotnet msbuild $CompileProject /nologo /v:quiet /clp:ErrorsOnly
if($LASTEXITCODE -ne 0){throw 'Tactic projection requires current compiled production code.'}
. (Join-Path $PSScriptRoot '../Shared/LoadUnityManagedAssemblies.ps1')
$assembly=Import-SF2ManagedRuntime $root $AssemblyDirectory $UnityManagedDirectory
$hidden=[Reflection.BindingFlags]'NonPublic,Instance'
$adapterType=$assembly.GetType('Eclipse.Modding.LegacyContentAdapter',$true)
$adapter=[Runtime.CompilerServices.RuntimeHelpers]::GetUninitializedObject($adapterType)
$project=$adapterType.GetMethod('BuildTacticNode',$hidden)
$constructor=[Eclipse.Modding.TacticDefinition].GetConstructors($hidden)[0]
$checks=0
function Check([bool]$value,[string]$message){$script:checks++;if(!$value){throw $message}}
function Definition([string]$name,[Eclipse.Modding.ModTacticKind]$kind,[string]$template){
    $arguments=[object[]]::new(17)
    $arguments[0]=[Eclipse.Modding.DefinitionId]::Parse('fixture.tactics:tactics/'+$name)
    $arguments[1]=$kind;$arguments[2]=$template;$arguments[3]=[int]0;$arguments[4]=[single]0
    return $constructor.Invoke($arguments)
}
foreach($kind in @([Eclipse.Modding.ModTacticKind]::Random,[Eclipse.Modding.ModTacticKind]::Tabular)){
    $document=[xml]'<TacticsSettings><Tactics/></TacticsSettings>'
    $definition=Definition $kind.ToString().ToLowerInvariant() $kind ''
    $node=$project.Invoke($adapter,[object[]]@($document,$definition))
    $null=$document.SelectSingleNode('/TacticsSettings/Tactics').AppendChild($node)
    Check ($null -ne $node.SelectSingleNode('QuickAttacks') -and $null -ne $node.SelectSingleNode('Evades')) 'Standalone tactic omitted mandatory parser containers'
    Check ($node.SelectSingleNode('QuickAttacks').ChildNodes.Count -eq 0 -and $node.SelectSingleNode('Evades').ChildNodes.Count -eq 0) 'Standalone tactic invented native scoring entries'
    [TacticsCompiler]::CompileTacticsSettings($document)
    $native=[Tactic]::new($node)
    Check ($native.get_Name() -eq $definition.RuntimeName) 'Actual native parser changed tactic identity'
    Check ($native.get_QuickAttacks().Count -eq 0 -and $native.get_Evades().Count -eq 0) 'Actual native parser did not accept empty standalone lists'
}
$document=[xml](Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/vanillaXml/tacticSettings.xml'))
$definition=Definition 'inherited' ([Eclipse.Modding.ModTacticKind]::Tabular) 'Standard'
$node=$project.Invoke($adapter,[object[]]@($document,$definition))
Check ($null -eq $node.SelectSingleNode('QuickAttacks') -and $null -eq $node.SelectSingleNode('Evades')) 'Templated omissions lost inheritance semantics'
$null=$document.SelectSingleNode('/TacticsSettings/Tactics').AppendChild($node)
[TacticsCompiler]::CompileTacticsSettings($document)
$baseline=[Tactic]::new($document.SelectSingleNode('/TacticsSettings/Tactics/Tactic[@Name="Standard"]'))
$native=[Tactic]::new($node)
Check ($native.get_QuickAttacks().Count -eq $baseline.get_QuickAttacks().Count -and $native.get_Evades().Count -eq $baseline.get_Evades().Count) 'Native compiler failed to retain core template scoring lists'
Check ($native.get_QuickAttacks().Count -gt 0) 'Core template fixture did not exercise inherited attack entries'
Write-Output "PASS: $checks actual production tactic projection/native compiler/parser checks; standalone random/tabular lists and untouched core inheritance. No fight simulation claimed."
