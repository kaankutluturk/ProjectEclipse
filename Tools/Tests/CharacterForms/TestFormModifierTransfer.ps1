$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$stageSource=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/PerksStage.cs')
$modelSource=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Model.cs')
$stage=[regex]::Match($stageSource,'(?ms)^    internal System.Action TransferFormEffects.*?^    \}').Value
$stage += [regex]::Match($stageSource,'(?ms)^    internal void RequireFormReferencesTransferred.*?^    \}').Value
$kinds=[regex]::Match($stageSource,'(?ms)^    private static bool IsFormBodyModifier.*?^    \}').Value
$copy=[regex]::Match($modelSource,'(?ms)^    internal System.Action CopyFormModifiersFrom.*?^    \}').Value
if(!$stage -or !$kinds -or !$copy){throw 'Modifier transfer extraction failed.'}
$fixture=Join-Path $root ('Temp/FormModifiers-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/ActionType.cs') -Destination $fixture
$code=@'
using System;
using System.Collections.Generic;
class Vector3f{public float X;public Vector3f(float x){X=x;}public Vector3f(Vector3f other){X=other.X;}}
class Model{
COPY
public Vector3f impulseFactor=new Vector3f(1);public float hitEffectScale=1,additionalDamage;public int? _perkColor=1;public int _perkSlowFactor=1,_perkSlowFrame;public bool _perkCollisionDisabled,FailColor;
public class Presentation{public void SetSlow(int factor,int frame){}}public Presentation _presentation=new Presentation();
void ApplyPerkColor(int? value){_perkColor=value;if(FailColor){FailColor=false;throw new Exception("renderer");}}
}
class ActionDefinition{public ActionType Type;public ActionType get_Type()=>Type;}
class PerkActionSetAttributes:ActionDefinition{}
class ModHealthChange:ActionDefinition{}
class InfoPerk{public List<PerksStage.ActionPerk> Actions=new List<PerksStage.ActionPerk>(),Pending=new List<PerksStage.ActionPerk>();public List<PerksStage.ActionPerk> GetPendingActions()=>Pending;public List<PerksStage.ActionPerk> GetActiveActions()=>Actions;public Action TransferAttributeEffect(PerksStage.ActionPerk action,Model old,Model next){throw new Exception("missing attribute history");}public Action TransferHealthEffect(PerksStage.ActionPerk action,Model old,Model next){throw new Exception("unused health fixture");}}
class Registration{public List<InfoPerk> Perks=new List<InfoPerk>();public List<InfoPerk> GetInfoPerks()=>Perks;}
class PerksStage{
STAGE
KINDS
public class ActionPerk{public Model TargetModel,SourceModel;public ActionDefinition Action;public int DurationFrames=240,ElapsedFrames=17;}
List<Registration> modelRegistrations=new List<Registration>();
List<ActionPerk> expiredActions=new List<ActionPerk>();static Dictionary<string,List<ActionPerk>> actionsByNamespace=new Dictionary<string,List<ActionPerk>>();
static void Check(bool value,string message){if(!value)throw new Exception(message);}
static void Main(){
 var old=new Model{impulseFactor=new Vector3f(3),hitEffectScale=2,additionalDamage=7,_perkColor=42,_perkSlowFactor=4,_perkSlowFrame=3,_perkCollisionDisabled=true};
 var next=new Model();var other=new Model();var stage=new PerksStage();var registration=new Registration();var perk=new InfoPerk();registration.Perks.Add(perk);stage.modelRegistrations.Add(registration);
 var types=new[]{ActionType.ACTION_CHANGE_IMPULSE,ActionType.ACTION_CHANGE_HIT_EFFECT_SCALE,ActionType.ACTION_CHANGE_ADD_DAMAGE_VALUE,ActionType.ACTION_CHANGE_MODEL_COLOR,ActionType.ACTION_SLOW_MODEL,ActionType.ACTION_TURN_OFF_COLLISION,ActionType.ACTION_SHOW_ICONS,ActionType.ACTION_PERK_AREA};
 foreach(var type in types)perk.Actions.Add(new ActionPerk{TargetModel=old,SourceModel=old,Action=new ActionDefinition{Type=type}});
 var attributed=new ActionPerk{TargetModel=other,SourceModel=old,Action=new ActionDefinition()};perk.Actions.Add(attributed);
 var historyOnly=new ActionPerk{TargetModel=old,SourceModel=old,Action=new ActionDefinition{Type=ActionType.ACTION_STEAL_MAGIC}};
 stage.expiredActions.Add(historyOnly);actionsByNamespace["history"]=new List<ActionPerk>{historyOnly};
 stage.expiredActions.Add(perk.Actions[0]);
 var undo=stage.TransferFormEffects(old,next);
 Check(historyOnly.TargetModel==next&&historyOnly.SourceModel==next&&historyOnly.DurationFrames==240,"history-only attribution retargeted without replaying expired effect");
 stage.RequireFormReferencesTransferred(new HashSet<Model>{old});
 var leftover=new ActionPerk{TargetModel=old,Action=new ActionDefinition()};
 foreach(var list in new[]{stage.expiredActions,perk.Pending,perk.Actions}){
  list.Add(leftover);bool rejected=false;try{stage.RequireFormReferencesTransferred(new HashSet<Model>{old});}catch(InvalidOperationException){rejected=true;}Check(rejected,"retirement rejects recent, queued and active references");list.Remove(leftover);
 }
 actionsByNamespace["fixture"]=new List<ActionPerk>{leftover};bool namespaceRejected=false;try{stage.RequireFormReferencesTransferred(new HashSet<Model>{old});}catch(InvalidOperationException){namespaceRejected=true;}Check(namespaceRejected,"retirement rejects namespace-only references");actionsByNamespace.Clear();
 Check(next.impulseFactor.X==3&&next.impulseFactor!=old.impulseFactor&&next.hitEffectScale==2&&next.additionalDamage==7,"applied impulse/damage copied without sharing vector");
 Check(next._perkColor==42&&next._perkSlowFactor==4&&next._perkSlowFrame==3&&next._perkCollisionDisabled,"color, slow phase and collision preserved");
 foreach(var action in perk.Actions)Check(action.SourceModel==next&&action.TargetModel==(action==attributed?other:next)&&action.DurationFrames==240&&action.ElapsedFrames==17,"target/source and unchanged effect lifetime");
 undo();Check(next.impulseFactor.X==1&&next.hitEffectScale==1&&next.additionalDamage==0&&next._perkColor==1&&next._perkSlowFactor==1&&next._perkSlowFrame==0&&!next._perkCollisionDisabled,"replacement modifiers restored");
 foreach(var action in perk.Actions)Check(action.SourceModel==old&&action.TargetModel==(action==attributed?other:old),"effect identity restored");
 Check(historyOnly.TargetModel==old&&historyOnly.SourceModel==old,"history and live alias restored without double transfer");
 perk.Actions.Add(new ActionPerk{TargetModel=old,SourceModel=other,Action=new PerkActionSetAttributes()});
 bool failed=false;try{stage.TransferFormEffects(old,next);}catch(Exception){failed=true;}
 Check(failed&&next._perkSlowFactor==1&&perk.Actions[0].TargetModel==old&&attributed.SourceModel==old,"later effect failure restores modifiers and earlier references");
 next.FailColor=true;failed=false;try{next.CopyFormModifiersFrom(old);}catch(Exception){failed=true;}
 Check(failed&&next._perkColor==1&&next.impulseFactor.X==1&&old._perkSlowFrame==3,"renderer failure restores replacement and leaves source untouched");
 Console.WriteLine("PASS: production modifier copy/dispatch for eight action types, source attribution, existing timing, independent vectors and rollback. Renderer, action storage and attribute rejection controlled; no native expiry playtest.");
}
}
'@
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code.Replace('COPY',$copy).Replace('STAGE',$stage).Replace('KINDS',$kinds))
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Form modifier checks failed.'}
