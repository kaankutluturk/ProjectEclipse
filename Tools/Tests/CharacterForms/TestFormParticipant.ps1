$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Fight.cs')
$stage=[regex]::Match($source,'(?s)    internal Action BindFormParticipant.*?(?=\tprivate class MagicBuffer)').Value
if(!$stage){throw 'Participant binding extraction failed.'}
$fixture=Join-Path $root ('Temp/FormParticipant-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code=@'
using System;
using System.Collections.Generic;
using System.Xml;
using DefinitionId = System.String;
class ModelParameters { public bool IsPlayer; public object FightTactic=new object(); }
class Model { public ModelParameters Parameters=new ModelParameters(); }
class Fight {
STAGE
Model _playerModel=new Model(),_enemyModel=new Model();
ModelParameters playerParameters,enemyParameters;
List<Model> ActiveModels=new List<Model>();
List<ModelParameters> enemyParametersList=new List<ModelParameters>();
int currentEnemyIndex;
object enemyTactic;
Dictionary<Model,object> _eclipseShields=new Dictionary<Model,object>();
Dictionary<(Model,DefinitionId),XmlNode> _eclipseOpponentInstances=new Dictionary<(Model,DefinitionId),XmlNode>();
Dictionary<(Model,DefinitionId),XmlNode> _eclipseInnateInstances=new Dictionary<(Model,DefinitionId),XmlNode>();
Fight() {
 _playerModel.Parameters.IsPlayer=true;
 playerParameters=_playerModel.Parameters; enemyParameters=_enemyModel.Parameters;
 enemyParametersList.Add(enemyParameters); ActiveModels.Add(_playerModel); ActiveModels.Add(_enemyModel);
 enemyTactic=enemyParameters.FightTactic;
}
static void Check(bool value,string message){if(!value)throw new Exception(message);}
static void Reject(Action action){bool failed=false;try{action();}catch(InvalidOperationException){failed=true;}Check(failed,"expected rejection");}
static void Main(){
 foreach(bool player in new[]{true,false}) {
  var f=new Fight();var old=player?f._playerModel:f._enemyModel;var next=new Model();next.Parameters.IsPlayer=player;
  var doc=new XmlDocument();doc.LoadXml("<state counter='7'/>");var state=doc.DocumentElement;
  var shield=new object();f._eclipseShields.Add(old,shield);
  f._eclipseOpponentInstances.Add((old,"sample:perk"),state);f._eclipseInnateInstances.Add((old,"sample:innate"),state);
  var other=player?f._enemyModel:f._playerModel;
  f._eclipseInnateInstances.Add((other,"sample:innate"),state);
  var undo=f.BindFormParticipant(old,next);
  Check((player?f._playerModel:f._enemyModel)==next,"active participant");
  Check((player?f.playerParameters:f.enemyParameters)==next.Parameters,"round parameters");
  Check(f.ActiveModels[player?0:1]==next,"simulation list slot");
  Check(f._eclipseShields[next]==shield&&!f._eclipseShields.ContainsKey(old),"shield identity");
  Check(f._eclipseOpponentInstances[(next,"sample:perk")]==state,"behavior state identity");
  Check(f._eclipseInnateInstances[(next,"sample:innate")]==state&&f._eclipseInnateInstances.ContainsKey((other,"sample:innate")),"innate and unrelated state");
  if(!player)Check(f.enemyParametersList[0]==next.Parameters&&f.enemyTactic==next.Parameters.FightTactic,"opponent sequence and tactic");
  undo();undo();
  Check((player?f._playerModel:f._enemyModel)==old&&f.ActiveModels[player?0:1]==old,"idempotent rollback");
  Check(f._eclipseShields[old]==shield&&f._eclipseOpponentInstances[(old,"sample:perk")]==state,"state rollback");
 }
 var fight=new Fight();var replacement=new Model();
 fight._eclipseInnateInstances.Add((replacement,"conflict"),new XmlDocument());
 Reject(()=>fight.BindFormParticipant(fight._enemyModel,replacement));
 Check(fight.ActiveModels[1]==fight._enemyModel,"collision rejected before mutation");
 fight._eclipseInnateInstances.Clear();replacement.Parameters.IsPlayer=true;
 Reject(()=>fight.BindFormParticipant(fight._enemyModel,replacement));
 replacement.Parameters.IsPlayer=false;fight.enemyParametersList[0]=new ModelParameters();
 Reject(()=>fight.BindFormParticipant(fight._enemyModel,replacement));
 Console.WriteLine("PASS: production participant binding for both sides, round/sequence/tactic identity, shield and Lua state continuity, rollback and preflight rejection. Controlled model data; no game playtest.");
}
}
'@
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code.Replace('STAGE',$stage))
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Form participant checks failed.'}
