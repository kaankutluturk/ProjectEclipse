using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Eclipse.Modding;

public partial class Fight
{
    private sealed class PendingActor
    {
        public Model Root; public ModId Owner; public ModScriptSession Session;
        public ActorDefinition Definition; public int Round; public double X,Y,Z;
        public Action<string,string> Complete;
    }
    private sealed class OwnedActor : IModActor
    {
        public Fight Fight; public Model Model,Root,TargetRequest; public bool PlayerTeam,OwnerPlayer,Removing;
        public ModId Owner; public ModScriptSession Session; public ActorDefinition Definition;
        public int Round,Born,Requests; public long Sequence; public double X,Y,Z; public PendingActor Birth;
        public double HealthChange; public int HealthRequests; public PendingFighterPlayback Playback;
        public bool Spawned; public System.Xml.XmlNode BehaviorInstance;
        public string Id => "a" + Sequence.ToString(CultureInfo.InvariantCulture);
        public bool TrySnapshot(out ModActorSnapshot snapshot,out string error)
        {
            snapshot=null; if (!Fight.ActorValid(this,false,out error) || Birth!=null) { error=error??"Actor is still initializing."; return false; }
            var body=EclipseFighterOperations.Capture(Model);
            if(body==null){error="Actor geometry is unavailable.";return false;}
            snapshot=new ModActorSnapshot(Id,Definition.Id.ToString(),PlayerTeam?"player":"opponent",
                Fight.ActorTargetId(Model.GetCombatTarget()),body,Math.Max(0,Fight.fightTimeInFrame-Born),Definition.LifetimeFrames);
            return true;
        }
        public bool TryMoveBy(double x,double y,double z,out string error)
        {
            if(!Fight.ActorValid(this,true,out error))return false;
            if(!ModProjectileLimits.ValidDisplacement(x,y,z)||!ModProjectileLimits.ValidDisplacement(X+x,Y+y,Z+z))
            {error="Individual and combined actor displacement must be finite in -100..100 per axis this step.";return false;}
            if(x==0&&y==0&&z==0)return true;
            if(Requests>=ModProjectileLimits.MaximumRequestsPerStep){error="Actor movement request limit reached.";return false;}
            X+=x;Y+=y;Z+=z;Requests++;return true;
        }
        public bool TrySetTarget(string mainTarget,IModActor actorTarget,out string error)
        {
            if(!Fight.ActorValid(this,true,out error))return false;
            Model target=null;
            if(mainTarget=="player")target=Fight.GetPlayerModel();
            else if(mainTarget=="opponent")target=Fight.GetEnemyModel();
            else if(mainTarget=="nearest"&&actorTarget==null){TargetRequest=null;return true;}
            else if(actorTarget is OwnedActor other&&other.Fight==Fight&&other.Owner==Owner&&Fight.ActorValid(other,false,out _))target=other.Model;
            if(target==null||target==Model||Fight.ActorTeam(target)==PlayerTeam)
            {error="Target must be a living hostile main fighter or this mod's live actor in this fight.";return false;}
            if(EclipseFighterOperations.Capture(target)?.Health<=0){error="Target is defeated.";return false;}
            TargetRequest=target;return true;
        }
        public bool TryChangeHealth(double amount,out string error)
        {
            if(!Fight.ActorValid(this,true,out error))return false;
            if(double.IsNaN(amount)||double.IsInfinity(amount)||Math.Abs(amount)>Definition.MaxHealth||Math.Abs(HealthChange+amount)>Definition.MaxHealth)
            {error="Individual and combined actor health changes must be finite within plus/minus max_health this step.";return false;}
            if(amount==0)return true;
            if(HealthRequests>=32){error="Actor health request limit reached.";return false;}
            HealthChange+=amount;HealthRequests++;return true;
        }
        public bool TryPlayMove(DefinitionId move,Action<bool,string> complete,out string error)
        {
            if(!Fight.ActorValid(this,true,out error)||Fight._applyingEclipseActors)return false;
            if(!Session.Content.TryGetMove(move,out var definition)||!HasEclipseMove(Model,definition.RuntimeName))
            {error="Move is unavailable on this actor's current rig/equipment.";return false;}
            if(Playback!=null){error="This actor already has a move request pending this step.";return false;}
            Playback=new PendingFighterPlayback{Name=definition.RuntimeName,Complete=complete,Session=Session,Round=Round};return true;
        }
        public bool TryRemove(out string error)
        {if(!Fight.ActorValid(this,true,out error))return false;Removing=true;X=Y=Z=0;return true;}
    }
    private sealed class ActorEventRecord
    {public ModId Owner;public bool OwnerPlayer;public ModActorEvent Event;}
    private readonly List<PendingActor> _eclipseActorSpawns=new List<PendingActor>();
    private readonly Dictionary<Model,OwnedActor> _eclipseActors=new Dictionary<Model,OwnedActor>();
    private readonly List<ActorEventRecord> _eclipseActorEvents=new List<ActorEventRecord>();
    private long _eclipseActorSequence,_eclipseActorEventSequence;
    private bool _applyingEclipseActors,_eclipseActorTeamsActive,_eclipseActorDispatching;

    private bool ActorOwnerValid(Model root,ModScriptSession session,ModId owner) =>
        ReferenceEquals(session,ModRuntime.Scripts)&&ProjectileOwnerActive(session,owner)&&CanMoveEclipseFighter(root)&&!get_IsRaidFight();
    private bool TryQueueEclipseActor(Model root,ModId owner,DefinitionId definition,double x,double y,double z,Action<string,string> complete,out string error)
    {
        error=null;var session=ModRuntime.Scripts;
        if(_applyingEclipseActors||IsPaused()||!ActorOwnerValid(root,session,owner))
        {error="Actors require a living main fighter in an active offline non-raid simulation callback.";return false;}
        if(definition.Namespace!=owner||!session.Content.TryGetActor(definition,out var prefab))
        {error="Actor definition must be registered by the calling mod.";return false;}
        if(!ModFighterMotionLimits.IsValid(x,y,z)){error="Actor spawn offsets must be finite in -1000..1000 per axis.";return false;}
        if(_eclipseActors.Count+_eclipseActorSpawns.Count>=ModActorLimits.MaximumPerFight||
            _eclipseActors.Values.Count(a=>a.Owner==owner)+_eclipseActorSpawns.Count(a=>a.Owner==owner)>=ModActorLimits.MaximumPerMod)
        {error="Actor ownership capacity is full.";return false;}
        if(NativeActorModels().Any(m=>!(m is WeaponModel)&&m!=GetPlayerModel()&&m!=GetEnemyModel()&&!_eclipseActors.ContainsKey(m)))
        {error="This encounter already contains unmanaged extra fighters.";return false;}
        _eclipseActorSpawns.Add(new PendingActor{Root=root,Owner=owner,Session=session,Definition=prefab,Round=round.round,X=x,Y=y,Z=z,Complete=complete});
        return true;
    }
    private static void FinishActorSpawn(PendingActor request,string id,string error)
    {var complete=request.Complete;request.Complete=null;try{complete?.Invoke(id,error);}catch(Exception e){UnityEngine.Debug.LogWarning("[ModActors] Receipt update failed: "+e.Message);}}
    private void ApplyEclipseActorSpawns()
    {
        if(IsPaused()||_eclipseActorSpawns.Count==0)return;
        _applyingEclipseActors=true;
        try
        {
            while(_eclipseActorSpawns.Count!=0)
            {
                var request=_eclipseActorSpawns[0];_eclipseActorSpawns.RemoveAt(0);
                OwnedActor actor=null;Action unregister=null,restoreTargets=null;PreparedFormModel prepared=null;
                try
                {
                    if(request.Round!=round.round||!ActorOwnerValid(request.Root,request.Session,request.Owner))throw new OperationCanceledException("Owner, fighter, session or round changed before actor creation.");
                    bool ownerPlayer=request.Root==GetPlayerModel(),playerTeam=ownerPlayer!=request.Definition.OpposingTeam;
                    var origin=EclipseFighterOperations.Capture(request.Root)??throw new InvalidOperationException("Owner geometry is unavailable.");
                    request.X+=origin.X;request.Y+=origin.Y;request.Z+=origin.Z;
                    var parameters=ModRuntime.BuildFormParameters(request.Definition.Character,playerTeam);
                    GameUtils.InitializeActorParameters(parameters,playerTeam,request.Definition.AiControlled,request.Definition.MaxHealth);
                    prepared=new PreparedFormModel(parameters);
                    {
                        actor=new OwnedActor{Fight=this,Model=prepared.Model,Root=request.Root,Owner=request.Owner,Session=request.Session,Definition=request.Definition,
                            OwnerPlayer=ownerPlayer,PlayerTeam=playerTeam,Round=request.Round,Born=fightTimeInFrame,Sequence=++_eclipseActorSequence,Birth=request};
                        actor.Model.set_Name(request.Definition.Id.ToString());
                        _eclipseActors.Add(actor.Model,actor);_eclipseActorTeamsActive=true;
                        unregister=RegisterActorNative(actor.Model);
                        restoreTargets=RefreshEclipseActorTeams();
                        PrepareActorNative(actor.Model);
                        prepared.Take();
                        unregister=null;restoreTargets=null;
                    }
                }
                catch(Exception failure)
                {
                    restoreTargets?.Invoke();unregister?.Invoke();
                    if(actor!=null)_eclipseActors.Remove(actor.Model);
                    FinishActorSpawn(request,null,"Native actor creation failed: "+failure.Message);
                }
                finally{prepared?.Dispose();}
            }
        }
        finally{_applyingEclipseActors=false;}
    }
    private void InitializeEclipseActorBirths()
    {
        foreach(var actor in _eclipseActors.Values.Where(a=>a.Birth!=null).ToArray())
        {
            var request=actor.Birth;
            try
            {
                if(!ActorValid(actor,false,out var error))throw new InvalidOperationException(error);
                // PrepareFormAnimation queues the native entry. Model.Render starts
                // its first frame on the following step; placement must precede
                // that step's collision/AI pass, not the scheduling pass.
                if(actor.Model.GetCurrentAnimation()==null)
                {
                    if(fightTimeInFrame-actor.Born>2)throw new InvalidOperationException("Native entry animation did not start.");
                    continue;
                }
                actor.Model.ShiftModelPosition(new Vector3f(0,0,0),true);
                for(int pass=0;pass<4;pass++)
                {
                    var body=EclipseFighterOperations.Capture(actor.Model)??throw new InvalidOperationException("Actor geometry is unavailable.");
                    double x=request.X-body.X,y=request.Y-body.Y,z=request.Z-body.Z;
                    if(Math.Abs(x)<=.05&&Math.Abs(y)<=.05&&Math.Abs(z)<=.05)break;
                    actor.Model.ShiftModelPosition(new Vector3f((float)x,(float)y,(float)z),true);
                }
                var final=EclipseFighterOperations.Capture(actor.Model);
                if(Math.Abs(final.X-request.X)>.05||Math.Abs(final.Y-request.Y)>.05||Math.Abs(final.Z-request.Z)>.05)
                    throw new InvalidOperationException("Native actor birth position did not settle: requested "+request.X+","+request.Y+","+request.Z+"; observed "+final.X+","+final.Y+","+final.Z+".");
                actor.Model.GetRenderObject()?.SetActive(true);
                actor.Birth=null;actor.Born=fightTimeInFrame;
                ActorEvent(actor,"spawned");FinishActorSpawn(request,actor.Id,null);
                actor.Spawned=true;DispatchEclipseActor(actor,ModEffectEvent.ActorSpawn);
            }
            catch(Exception failure){actor.Birth=null;FinishActorSpawn(request,null,"Native actor initialization failed: "+failure.Message);RetireEclipseActor(actor,"spawn_failed");}
        }
    }
    private bool ActorValid(OwnedActor actor,bool mutation,out string error)
    {
        error=null;
        if(!_eclipseActors.TryGetValue(actor.Model,out var live)||live!=actor||actor.Removing||
            !NativeActorModels().Contains(actor.Model)||actor.Round!=round.round||!ActorOwnerValid(actor.Root,actor.Session,actor.Owner)||
            actor.Birth==null&&(fightTimeInFrame-actor.Born>=actor.Definition.LifetimeFrames||EclipseFighterOperations.Capture(actor.Model)?.Health<=0)||mutation&&IsPaused())
        {error="Actor expired, is removing, or is outside its live owner/body/round/session.";return false;}
        return true;
    }
    private bool TryGetEclipseActors(Model root,ModId owner,out IReadOnlyList<IModActor> actors,out string error)
    {
        actors=null;error=null;
        if(!ActorOwnerValid(root,ModRuntime.Scripts,owner)){error="Actor observations require a living owner in an active offline non-raid round.";return false;}
        actors=_eclipseActors.Values.Where(a=>a.Root==root&&a.Owner==owner&&a.Birth==null&&ActorValid(a,false,out _)).OrderBy(a=>a.Sequence).Cast<IModActor>().ToArray();return true;
    }
    private bool TryGetEclipseActorEvents(Model root,ModId owner,out IReadOnlyList<ModActorEvent> events,out string error)
    {
        events=null;error=null;
        if(GetCurrentFight()!=this||(root!=GetPlayerModel()&&root!=GetEnemyModel())||!ProjectileOwnerActive(ModRuntime.Scripts,owner))
        {error="Actor events require a current main fighter and active calling mod.";return false;}
        events=_eclipseActorEvents.Where(e=>e.Owner==owner&&e.OwnerPlayer==(root==GetPlayerModel())).Select(e=>e.Event).ToArray();return true;
    }
    private void ActorEvent(OwnedActor actor,string kind)
    {
        _eclipseActorEvents.Add(new ActorEventRecord{Owner=actor.Owner,OwnerPlayer=actor.OwnerPlayer,Event=new ModActorEvent(++_eclipseActorEventSequence,actor.Id,kind,fightTimeInFrame)});
        var owned=_eclipseActorEvents.Where(e=>e.Owner==actor.Owner&&e.OwnerPlayer==actor.OwnerPlayer).ToArray();
        if(owned.Length>ModActorLimits.MaximumEvents)_eclipseActorEvents.Remove(owned[0]);
    }
    private void UpdateEclipseActors()
    {
        foreach(var actor in _eclipseActors.Values.ToArray())
            if(!ActorValid(actor,false,out _))RetireEclipseActor(actor,
                actor.Removing||!NativeActorModels().Contains(actor.Model)?"removed":actor.Round!=round.round?"round_ended":
                !ActorOwnerValid(actor.Root,actor.Session,actor.Owner)?"owner_changed":
                EclipseFighterOperations.Capture(actor.Model)?.Health<=0?"died":"expired");
    }
    private void ApplyEclipseActors()
    {
        if(IsPaused())return;
        UpdateEclipseActors();
        _applyingEclipseActors=true;
        try
        {
            foreach(var actor in _eclipseActors.Values.OrderBy(a=>a.Sequence).ToArray())
            {
                double x=actor.X,y=actor.Y,z=actor.Z,health=actor.HealthChange;
                var playback=actor.Playback;actor.Playback=null;
                actor.X=actor.Y=actor.Z=actor.HealthChange=0;actor.Requests=actor.HealthRequests=0;
                try
                {
                    if(x!=0||y!=0||z!=0)actor.Model.ShiftModelPosition(new Vector3f((float)x,(float)y,(float)z),true);
                    if(health!=0)UpdateLife(actor.Model,(float)health);
                    if(playback!=null)
                    {
                        bool started=ActorValid(actor,false,out _)&&actor.Model.PlayAnimation(playback.Name);
                        FinishEclipseFighterPlayback(playback,started,started?null:"Actor ended or native playback rejected the move.");
                    }
                }
                catch(Exception failure)
                {
                    if(playback!=null)FinishEclipseFighterPlayback(playback,false,"Actor command failed: "+failure.Message);
                    UnityEngine.Debug.LogWarning("[ModActors] Native command failed: "+failure.Message);
                }
            }
            UpdateEclipseActors();RefreshEclipseActorTeams();
        }
        finally{_applyingEclipseActors=false;}
    }
    internal ModActorIdentity CaptureEclipseActorIdentity(Model model)
    {
        if(model==null||!_eclipseActors.TryGetValue(model,out var actor)||actor.Birth!=null||actor.Removing)return null;
        return new ModActorIdentity(actor.Id,actor.Definition.Id.ToString(),actor.Owner.ToString(),actor.PlayerTeam?"player":"opponent");
    }
    private void DispatchEclipseActor(Model model,ModEffectEvent kind,ModDamageEvent damage=null,ModIncomingHit incoming=null,
        ModAnimationLifecycleEvent animation=null)
    {
        if(model!=null&&_eclipseActors.TryGetValue(model,out var actor))DispatchEclipseActor(actor,kind,damage,incoming,animation);
    }
    private void DispatchEclipseActor(OwnedActor actor,ModEffectEvent kind,ModDamageEvent damage=null,ModIncomingHit incoming=null,
        ModAnimationLifecycleEvent animation=null,string endReason=null)
    {
        // Applied lethal contacts still reach this host. Commands cannot resurrect
        // it: ActorValid rejects defeated bodies before the next cleanup pass.
        if(_eclipseActorDispatching||actor.Birth!=null||!actor.Spawned||!actor.Definition.Behavior.HasValue||
            !_eclipseActors.TryGetValue(actor.Model,out var current)||current!=actor||
            actor.Removing&&kind!=ModEffectEvent.ActorEnd||!ReferenceEquals(actor.Session,ModRuntime.Scripts)||
            !ProjectileOwnerActive(actor.Session,actor.Owner))return;
        var behaviorId=actor.Definition.Behavior.Value;
        if(!actor.Session.HasBehaviorHandler(behaviorId,kind))return;
        _eclipseActorDispatching=true;
        try
        {
            if(actor.BehaviorInstance==null)
            {
                var document=new System.Xml.XmlDocument();document.LoadXml("<ActorBehaviorInstance/>");
                actor.BehaviorInstance=document.DocumentElement;
            }
            var context=new Dictionary<string,string>
            {
                {"source","actor"},{"side",actor.PlayerTeam?"player":"opponent"},{"actor_id",actor.Id},
                {"actor_definition",actor.Definition.Id.ToString()},{"actor_owner",actor.Owner.ToString()},
                {"fight_id",_eclipseFightId},{"round",actor.Round.ToString(CultureInfo.InvariantCulture)}
            };
            if(endReason!=null)context["actor_end_reason"]=endReason;
            var fighter=new ModInstanceFighter(new EclipseFighterOperations(this,actor.Model,damage,incoming,animation:animation),actor.BehaviorInstance);
            if(!actor.Session.TryInvokeBehavior(behaviorId,kind,actor.Definition.InitialParameters,context,fighter,out var error))
                UnityEngine.Debug.LogWarning("[ModCombat] "+kind+" failed for actor "+actor.Id+": "+error);
        }
        catch(Exception failure){UnityEngine.Debug.LogWarning("[ModCombat] Actor dispatch failed: "+failure.Message);}
        finally{_eclipseActorDispatching=false;DrainEclipseAnimationEvents();}
    }
    private void DispatchEclipseActorTicks()
    {
        foreach(var actor in _eclipseActors.Values.OrderBy(a=>a.Sequence).ToArray())
        {
            if(!round.processing||_eclipseFightEndDispatched||_eclipseEndedRound==round.round)break;
            DispatchEclipseActor(actor,ModEffectEvent.Tick);
        }
    }
    private void NotifyEclipseResolvedContact(Model model,ModEffectEvent kind,ModDamageEvent damage)
    {
        if(model==GetPlayerModel())DispatchEclipseCombatEvent(kind,damage);
        else if(model==GetEnemyModel())DispatchEclipseOpponent(kind,damage);
        else DispatchEclipseActor(model,kind,damage);
    }
    private void NotifyEclipseAppliedContact(Model victim,Model attacker,ModDamageEvent observation)
    {
        void Defender()
        {
            if(observation.Damage>0)NotifyEclipseResolvedContact(victim,ModEffectEvent.DamageReceived,observation);
            if(observation.Blocked)NotifyEclipseResolvedContact(victim,ModEffectEvent.Block,observation);
        }
        void Attacker()
        {
            if(observation.Damage>0)NotifyEclipseResolvedContact(attacker,ModEffectEvent.DamageDealt,observation);
            if(observation.Critical)NotifyEclipseResolvedContact(attacker,ModEffectEvent.Critical,observation);
        }
        // Keep the established player-before-opponent ordering for the canonical
        // duel while routing actor contacts independently of that pair.
        if(attacker==GetPlayerModel()&&victim!=attacker){Attacker();Defender();}
        else{Defender();Attacker();}
    }
    private List<(OwnedActor Actor,ModAnimationLifecycleEvent Event)> CaptureEclipseActorAnimations(Model source,ModAnimationLifecycleEvent observed)
    {
        if(_eclipseActors.Count==0)return null;
        var result=new List<(OwnedActor,ModAnimationLifecycleEvent)>();
        foreach(var actor in _eclipseActors.Values.OrderBy(a=>a.Sequence))
        {
            if(!actor.Spawned||actor.Removing||!actor.Definition.Behavior.HasValue)continue;
            var target=source==actor.Model?"self":source==actor.Model.GetCombatTarget()?"opponent":"other";
            result.Add((actor,new ModAnimationLifecycleEvent(observed.Type,observed.AnimationName,target,observed.Frame)));
        }
        return result;
    }
    private void DispatchEclipseActorAnimations(List<(OwnedActor Actor,ModAnimationLifecycleEvent Event)> observations)
    {
        if(observations==null)return;
        foreach(var observed in observations)
        {
            if(!round.processing||_eclipseFightEndDispatched)break;
            DispatchEclipseActor(observed.Actor,observed.Event.Type,animation:observed.Event);
        }
    }
    private bool ActorTeam(Model model) => _eclipseActors.TryGetValue(model,out var actor)?actor.PlayerTeam:model==GetPlayerModel();
    private string ActorTargetId(Model target) => target==null?null:target==GetPlayerModel()?"player":target==GetEnemyModel()?"opponent":_eclipseActors.TryGetValue(target,out var actor)?actor.Id:null;
    // Return a rollback for staging only. Ordinary simulation commits immediately.
    private Action RefreshEclipseActorTeams()
    {
        if(!_eclipseActorTeamsActive)return ()=>{};
        var roots=new[]{GetPlayerModel(),GetEnemyModel()}.Where(m=>m!=null).Concat(_eclipseActors.Values.Where(a=>!a.Removing).OrderBy(a=>a.Sequence).Select(a=>a.Model)).ToArray();
        var restore=new List<Action>();
        try
        {
            foreach(var root in roots)
            {
                var hostiles=roots.Where(m=>m!=root&&ActorTeam(m)!=ActorTeam(root)&&EclipseFighterOperations.Capture(m)?.Health>0).ToArray();
                var current=root.GetCombatTarget();Model selected=null;
                _eclipseActors.TryGetValue(root,out var actor);
                if(hostiles.Contains(current)&&!root.CanChangeCombatTarget)selected=current;
                else if(actor?.TargetRequest!=null&&hostiles.Contains(actor.TargetRequest))selected=actor.TargetRequest;
                else
                {
                    var origin=EclipseFighterOperations.Capture(root);
                    selected=hostiles.OrderBy(m=>Math.Abs((EclipseFighterOperations.Capture(m)?.X??0)-(origin?.X??0)))
                        .ThenBy(m=>m==current?0:1).FirstOrDefault();
                }
                if(!root.CombatEnemiesMatch(hostiles,selected))restore.Add(root.ReplaceCombatEnemies(hostiles,selected));
            }
        }
        catch{for(int i=restore.Count-1;i>=0;i--)restore[i]();throw;}
        return ()=>{for(int i=restore.Count-1;i>=0;i--)restore[i]();};
    }
    private void RetireEclipseActor(OwnedActor actor,string reason,bool requestRemoval=true)
    {
        if(!_eclipseActors.TryGetValue(actor.Model,out var current)||current!=actor)return;
        actor.Removing=true;
        DispatchEclipseActor(actor,ModEffectEvent.ActorEnd,endReason:reason);
        CancelEclipseActorProjectiles(actor.Model);
        _eclipseActors.Remove(actor.Model);
        _eclipseShields.Remove(actor.Model);
        foreach(var key in _eclipseStatusIcons.Keys.Where(key=>key.Item1==actor.Model).ToArray())TryClearEclipseStatusIcon(actor.Model,key.Item2,out _);
        if(actor.Playback!=null)FinishEclipseFighterPlayback(actor.Playback,false,"Actor retired before move playback.");
        actor.Playback=null;
        if(actor.Birth!=null)FinishActorSpawn(actor.Birth,null,"Actor retired before initialization.");
        actor.Birth=null;
        foreach(var child in actor.Model.GetWeaponModels().ToArray())RequestModelRemoval(child);
        if(requestRemoval)RequestModelRemoval(actor.Model);ActorEvent(actor,reason);
    }
    private void ForgetEclipseActor(Model model)
    {
        if(_eclipseActors.TryGetValue(model,out var actor))RetireEclipseActor(actor,"removed",false);
    }
    private void CancelEclipseActors(string reason)
    {
        var pending=_eclipseActorSpawns.ToArray();_eclipseActorSpawns.Clear();
        foreach(var request in pending)FinishActorSpawn(request,null,"Actor spawn cancelled by round/fight teardown.");
        foreach(var actor in _eclipseActors.Values.ToArray())RetireEclipseActor(actor,reason);
    }
}
