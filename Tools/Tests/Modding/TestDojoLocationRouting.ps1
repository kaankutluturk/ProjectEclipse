$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$location=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Location.cs')
$method=[regex]::Match($location,'(?ms)^\tpublic static string ResolveEntryLocation\(.*?^\t\}')
if(!$method.Success){throw 'Production location routing method not found.'}
$fixture=@'
using System;
public static class GameUtils { public static string DefaultLocation; }
namespace Eclipse.Modding { public static class ModRuntime { public static string ResolveDojoLocation(string fallback) { return fallback; } } }
public static class DojoRoutingFixture {
 /* METHOD */
 public static void Run(){
  int checks=0;
  Action<bool,string> check=(ok,message)=>{checks++;if(!ok)throw new Exception(message);};
  foreach(var selected in new[]{"new_year_china_dojo","example.dojo:locations/garden","dojo"}){
   GameUtils.DefaultLocation=selected;
   check(ResolveEntryLocation(BattleType.FightNone,"dojo")==selected,"Dojo retained stale definition location");
  }
  foreach(var empty in new[]{null,"","  "}){
   GameUtils.DefaultLocation=empty;
   check(ResolveEntryLocation(BattleType.FightNone,"fallback")=="fallback","Unset selection erased fallback");
  }
  GameUtils.DefaultLocation="example.dojo:locations/garden";
  foreach(BattleType type in Enum.GetValues(typeof(BattleType))){
   if(type==BattleType.FightNone)continue;
   check(ResolveEntryLocation(type,"battlefield")=="battlefield","Dojo selection leaked into "+type);
  }
  check(GameUtils.DefaultLocation=="example.dojo:locations/garden","Encounter routing mutated selection");
  Console.WriteLine("PASS: "+checks+" production dojo-entry routing checks; no rendered scene or persistence claim.");
 }
}
'@
$temp=Join-Path $root ('Temp/DojoRouting-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
$file=Join-Path $temp 'Fixture.cs'
$fixture.Replace('/* METHOD */',$method.Value) | Set-Content -Encoding UTF8 -LiteralPath $file
Add-Type -Path @($file,(Join-Path $root 'Assets/Scripts/Assembly-CSharp/BattleType.cs'))
[DojoRoutingFixture]::Run()
