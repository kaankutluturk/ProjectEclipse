$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$source=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Assembly-CSharp/Fight.cs')
$stage=[regex]::Match($source,'(?s)    internal sealed class FormRenderBindings.*?(?=    private readonly Dictionary<Model, PendingModelTransition>)').Value
if(!$stage){throw 'Form binding stage extraction failed.'}
$actors=Get-Content -Raw -LiteralPath (Join-Path $root 'Assets/Scripts/Eclipse/Modding/FightActors.cs')
$stage += [regex]::Match($actors,'(?ms)^    internal Action BindEclipseActorOwnerForm.*?^    \}').Value
$fixture=Join-Path $root ('Temp/FormBindings-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$code=@'
using System;
using System.Collections.Generic;
using System.Linq;
class Model{public Action TransferFormCombatState(Model next){return()=>{};}public Model Owner;public List<Model> _Enemies=new List<Model>(),Weapons=new List<Model>();public bool RejectEnemy,RejectEnemyRestore;public int Exchanges;public Model GetRootModel()=>Owner??this;public List<Model> GetWeaponModels()=>Weapons;public Action ReplaceEnemyForm(Model old,Model next){if(RejectEnemy)throw new InvalidOperationException("enemy exchange");int index=_Enemies.IndexOf(old);_Enemies[index]=next;Exchanges++;return()=>{if(RejectEnemyRestore)throw new InvalidOperationException("enemy restore");_Enemies[index]=old;};}}
class Binding{public Model Current,Original;public bool Reject,RejectRestore;public int Calls,EventRestores;public Action CapturePendingEvents(){var captured=Current;return()=>{if(Current!=captured)throw new Exception("events restored before registration");EventRestores++;};}public bool ReplaceModel(Model old,Model next,bool player=false){Calls++;if(Reject||(RejectRestore&&next==Original)||Current!=old)return false;Current=next;return true;}}
class Rules{public Model Current;public bool Reject;public Action PrepareModelRebind(Model old,Model next){if(Reject||Current!=old)throw new InvalidOperationException("rules");return()=>Current=next;}}
class Perks{public Model Current,AttributeTarget,Registration;public Action ReplaceFormRegistration(Model old,Model next){Registration=next;return()=>Registration=old;}public Action TransferFormEffects(Model old,Model next){AttributeTarget=next;return()=>AttributeTarget=old;}public Action RebindQueuedFormActions(Model old,Model next){Current=next;return()=>Current=old;}}
class PendingActor{public Model Root;}
class OwnedActor{public Model Root,TargetRequest,Model;public PendingActor Birth;public object BehaviorInstance=new object();public int Born=27;public bool Removing;}
class Fight{
 List<PendingActor> _eclipseActorSpawns=new List<PendingActor>();
 Dictionary<Model,OwnedActor> _eclipseActors=new Dictionary<Model,OwnedActor>();
 bool RejectPresentation;

 Model _playerModel=new Model(),_enemyModel=new Model();Binding _Camera=new Binding(),_SelectAnimation=new Binding();Rules _rulesInspector=new Rules();
 List<Model> ActiveModels=new List<Model>();
 Perks perksStage=new Perks();
 Action BindFormPresentation(Model expected,Model replacement,bool player,bool actor=false){if(RejectPresentation)throw new InvalidOperationException("presentation");return()=>{};} Action BindFormParticipant(Model expected,Model replacement){_playerModel=replacement;return()=>_playerModel=expected;}
 bool IsEclipseActorModel(Model model)=>model!=null&&_eclipseActors.ContainsKey(model);
 bool IsEclipseFormParticipant(Model model)=>model==_playerModel||model==_enemyModel||IsEclipseActorModel(model);
 Action BindEclipseActorFormParticipant(Model old,Model next){var actor=_eclipseActors[old];_eclipseActors.Remove(old);_eclipseActors.Add(next,actor);actor.Model=next;return()=>{_eclipseActors.Remove(next);_eclipseActors.Add(old,actor);actor.Model=old;};}
 public Fight(){_Camera.Current=_Camera.Original=_SelectAnimation.Current=_SelectAnimation.Original=_rulesInspector.Current=_playerModel;ActiveModels.AddRange(new[]{_playerModel,_enemyModel});_enemyModel._Enemies.Add(_playerModel);}
 STAGE
 static void Check(bool x,string why){if(!x)throw new Exception(why);}
 public static void Main(){
  var f=new Fight();var next=new Model();
  var stage=new FormRenderBindings(f,f._playerModel,next);
  Check(f._Camera.Current==next&&f._SelectAnimation.Current==next&&f._rulesInspector.Current==next,"all registrations staged");
  Check(f._playerModel==next,"participant identity follows staged registrations");
  Check(f.perksStage.Current==next,"queued perks staged");
  Check(f.perksStage.AttributeTarget==next&&f.perksStage.Registration==next,"attribute effects and registration staged");
  stage.Dispose();stage.Dispose();Check(f._Camera.Current==f._playerModel&&f._SelectAnimation.Current==f._playerModel&&f._rulesInspector.Current==f._playerModel,"rollback and idempotent dispose");
  Check(f._SelectAnimation.EventRestores==1,"events restored once after reversing selector registration");
  Check(f.perksStage.Current==f._playerModel,"queued perks restored");
  Check(f.perksStage.AttributeTarget==f._playerModel&&f.perksStage.Registration==f._playerModel,"attribute effects and registration restored");
  f._rulesInspector.Reject=true;bool failed=false;try{new FormRenderBindings(f,f._playerModel,next);}catch(InvalidOperationException){failed=true;}
  Check(failed&&f._Camera.Current==f._playerModel,"rule validation precedes mutation");f._rulesInspector.Reject=false;
  f._SelectAnimation.Reject=true;failed=false;try{new FormRenderBindings(f,f._playerModel,next);}catch(InvalidOperationException){failed=true;}
  Check(failed&&f._Camera.Current==f._playerModel,"animation rejection restores camera");f._SelectAnimation.Reject=false;
  stage=new FormRenderBindings(f,f._playerModel,next);stage.Commit();stage.Dispose();Check(f._Camera.Current==next&&f._rulesInspector.Current==next,"commit retains bindings");
  Check(f._SelectAnimation.EventRestores==1,"commit and rejected exchange do not restore stale events");
  f=new Fight();stage=new FormRenderBindings(f,f._playerModel,next);f._SelectAnimation.RejectRestore=true;failed=false;
  try{stage.Dispose();}catch(AggregateException){failed=true;}
  Check(failed&&f._Camera.Current==f._playerModel&&f._rulesInspector.Current==f._playerModel,"restoration attempts remaining systems after failure");
  Check(f._SelectAnimation.EventRestores==0,"failed selector restoration does not install old events on replacement");
  f=new Fight();f._Camera.RejectRestore=true;f._SelectAnimation.Reject=true;failed=false;
  try{new FormRenderBindings(f,f._playerModel,next);}catch(AggregateException e){failed=e.InnerExceptions.Count==2;}
  Check(failed,"original plus rollback failures reported");
  f=new Fight();var weapon=new Model{Owner=f._enemyModel};weapon._Enemies.Add(f._playerModel);f._enemyModel.Weapons.Add(weapon);f.ActiveModels.Add(weapon);
  var retiredWeapon=new Model{Owner=f._playerModel};retiredWeapon._Enemies.Add(f._playerModel);f._playerModel.Weapons.Add(retiredWeapon);f.ActiveModels.Add(retiredWeapon);
  stage=new FormRenderBindings(f,f._playerModel,next);
  Check(f._enemyModel._Enemies[0]==next&&weapon._Enemies[0]==next&&weapon.Exchanges==1,"surviving fighter and weapon targeting exchanged once");
  Check(retiredWeapon.Exchanges==0,"retired weapon targeting is not mutated");
  stage.Dispose();Check(f._enemyModel._Enemies[0]==f._playerModel&&weapon._Enemies[0]==f._playerModel,"targeting restored with registrations");
  weapon.RejectEnemy=true;failed=false;try{new FormRenderBindings(f,f._playerModel,next);}catch(InvalidOperationException){failed=true;}
  Check(failed&&f._enemyModel._Enemies[0]==f._playerModel&&f._Camera.Current==f._playerModel,"later observer rejection restores earlier observers and camera");weapon.RejectEnemy=false;
  f._SelectAnimation.Reject=true;failed=false;try{new FormRenderBindings(f,f._playerModel,next);}catch(InvalidOperationException){failed=true;}
  Check(failed&&weapon._Enemies[0]==f._playerModel&&f._enemyModel._Enemies[0]==f._playerModel,"selector rejection restores all enemy targets");f._SelectAnimation.Reject=false;
  stage=new FormRenderBindings(f,f._playerModel,next);stage.Commit();stage.Dispose();Check(weapon._Enemies[0]==next,"commit retains enemy targeting");
  f=new Fight();stage=new FormRenderBindings(f,f._playerModel,next);f._enemyModel.RejectEnemyRestore=true;failed=false;try{stage.Dispose();}catch(AggregateException){failed=true;}
  Check(failed&&f._Camera.Current==f._playerModel&&f._SelectAnimation.Current==f._playerModel,"target rollback failure still attempts other registrations");
  // Use the production owner helper and coordinator, including a late rejection.
  f=new Fight();var old=f._playerModel;var other=f._enemyModel;
  var birth=new PendingActor{Root=old};var queued=new PendingActor{Root=old};var foreign=new PendingActor{Root=other};
  var live=new OwnedActor{Model=new Model(),Root=old,TargetRequest=other,Birth=birth};
  var hostile=new OwnedActor{Model=new Model(),Root=other,TargetRequest=old};
  var removing=new OwnedActor{Model=new Model(),Root=old,Removing=true};
  f._eclipseActors.Add(live.Model,live);f._eclipseActors.Add(hostile.Model,hostile);f._eclipseActors.Add(removing.Model,removing);
  f._eclipseActorSpawns.AddRange(new[]{queued,birth,foreign});var behavior=live.BehaviorInstance;
  stage=new FormRenderBindings(f,old,next);
  Check(live.Root==next&&removing.Root==next&&removing.Removing&&hostile.Root==other,"owner binding preserves removal intent and unrelated ownership");
  Check(hostile.TargetRequest==next&&live.TargetRequest==other,"explicit main targets follow replacement independently of owner");
  Check(birth.Root==next&&queued.Root==next&&foreign.Root==other,"queued and initializing births follow only replaced owner");
  Check(live.Born==27&&live.BehaviorInstance==behavior&&f._eclipseActors[live.Model]==live,"actor body, private state and lifetime are preserved");
  stage.Dispose();Check(live.Root==old&&hostile.TargetRequest==old&&birth.Root==old&&queued.Root==old,"owner/target/birth binding rollback");
  f.RejectPresentation=true;failed=false;try{new FormRenderBindings(f,old,next);}catch(InvalidOperationException){failed=true;}
  Check(failed&&f._playerModel==old&&live.Root==old&&hostile.TargetRequest==old&&birth.Root==old&&queued.Root==old,"late presentation rejection restores participant and all actor references");
  f.RejectPresentation=false;stage=new FormRenderBindings(f,old,next);stage.Commit();stage.Dispose();
  Check(live.Root==next&&hostile.TargetRequest==next&&queued.Root==next,"committed owner binding survives disposal");
  var opponentNext=new Model();var restore=f.BindEclipseActorOwnerForm(other,opponentNext);
  Check(hostile.Root==opponentNext&&live.TargetRequest==opponentNext&&foreign.Root==opponentNext,"opponent ownership/explicit target/pending birth transfer");
  restore();Check(hostile.Root==other&&live.TargetRequest==other&&foreign.Root==other,"opponent reference rollback");
  f=new Fight();var main=f._playerModel;var actorBody=new Model();var actorRecord=new OwnedActor{Model=actorBody,Root=main};
  f._eclipseActors.Add(actorBody,actorRecord);f.ActiveModels.Add(actorBody);
  f._Camera.Current=f._Camera.Original=f._SelectAnimation.Current=f._SelectAnimation.Original=actorBody;
  f._rulesInspector.Reject=true;f._enemyModel._Enemies.Add(actorBody);
  stage=new FormRenderBindings(f,actorBody,next);
  Check(f._playerModel==main&&f._rulesInspector.Current==main&&actorRecord.Model==next,"actor form skips canonical identity/rule inspector");
  Check(f._enemyModel._Enemies.Contains(next),"actor form rebinds native observers");
  stage.Dispose();Check(actorRecord.Model==actorBody&&f._enemyModel._Enemies.Contains(actorBody),"actor coordinator restores identity and observers");
  f.RejectPresentation=true;failed=false;try{new FormRenderBindings(f,actorBody,next);}catch(InvalidOperationException){failed=true;}
  Check(failed&&actorRecord.Model==actorBody&&f._Camera.Current==actorBody&&f._rulesInspector.Current==main,"late actor form rejection restores all actor registrations");
  Console.WriteLine("PASS: production form registration orchestration; preparation, staged exchange, commit, rollback, partial rejection and rollback-failure reporting. Actor ownership/explicit targets/queued births preserved through commit and late rollback; camera/selector/rule services controlled.");
 }
}
'@
$code=$code.Replace('STAGE',$stage)
[IO.File]::WriteAllText((Join-Path $fixture 'Program.cs'),$code)
[IO.File]::WriteAllText((Join-Path $fixture 'Test.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>')
dotnet run --project (Join-Path $fixture 'Test.csproj') --verbosity quiet
if($LASTEXITCODE -ne 0){throw 'Form registration checks failed.'}
