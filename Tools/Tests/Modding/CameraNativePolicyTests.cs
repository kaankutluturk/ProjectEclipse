using System;
using System.Collections.Generic;
using Eclipse.Modding;

namespace Eclipse.Modding
{
    static class ModRuntime { public static ModScriptSession Scripts; }
    sealed class ModScriptSession { public bool Active=true; }
}
class Model { public bool Alive=true; }
class Render {}
class Camera { public Render Render=new Render(); public Render GetRender()=>Render; }
class Body { public Transform transform=new Transform(); }
class Transform { public Position localPosition; }
struct Position { public float x,y,z; }
class LocationSelector
{
    public Body Body=new Body(); public Body GetLayerObject()=>Body;
    public void SetPositionY(float value){var p=Body.transform.localPosition;p.y=(float)Math.Round(value,2,MidpointRounding.AwayFromZero);Body.transform.localPosition=p;}
}
partial class Fight
{
    static Fight Current; readonly Camera _Camera=new Camera();
    class Round { public int round=1; } readonly Round round=new Round();
    Model Player=new Model(),Enemy=new Model(); bool Raid,Paused,MainEligible=true;
    class OwnedActor { public ModId Owner; public object Birth; public bool Active=true; public Model Model; }
    readonly Dictionary<Model,OwnedActor> _eclipseActors=new Dictionary<Model,OwnedActor>();
    public static Fight GetCurrentFight()=>Current;
    Model GetPlayerModel()=>Player; Model GetEnemyModel()=>Enemy;
    bool get_IsRaidFight()=>Raid; bool IsPaused()=>Paused;
    bool ProjectileOwnerActive(ModScriptSession session,ModId owner)=>session!=null&&session.Active;
    bool CanMoveEclipseFighter(Model model)=>MainEligible&&model!=null&&model.Alive;
    bool ActorValid(OwnedActor actor,bool mutation,out string error){error=null;return actor.Active&&actor.Model.Alive;}
    static int checks;
    static void Check(bool yes,string message){checks++;if(!yes)throw new Exception(message);}
    static void Main()
    {
        var owner=ModId.Parse("fixture.camera");var other=ModId.Parse("fixture.other");var settings=new ModCameraSettings(400,40,1.4);
        ModRuntime.Scripts=new ModScriptSession();var f=Current=new Fight();
        Check(!f.TryAcquireEclipseCamera(null,owner,settings,out var control,out var error),"Null root");
        Check(!f.TryAcquireEclipseCamera(new Model(),owner,settings,out control,out error),"Unmanaged root");
        foreach(var flag in new[]{"paused","raid","inactive","dead","session"})
        {
            f.Paused=flag=="paused";f.Raid=flag=="raid";f.MainEligible=flag!="inactive";f.Player.Alive=flag!="dead";
            ModRuntime.Scripts=flag=="session"?null:new ModScriptSession();
            Check(!f.TryAcquireEclipseCamera(f.Player,owner,settings,out control,out error),"Creation rejects "+flag);
        }
        f.Paused=false;f.Raid=false;f.MainEligible=true;f.Player.Alive=true;ModRuntime.Scripts=new ModScriptSession();
        Check(f.TryAcquireEclipseCamera(f.Player,owner,settings,out var main,out error),"Live player acquisition");
        Check(ReferenceEquals(f.GetEclipseCameraSettings(f._Camera.Render),settings),"Only bound fight render sees ownership");
        Check(f.GetEclipseCameraSettings(new Render())==null,"Foreign render remains native");
        Check(!f.TryAcquireEclipseCamera(f.Enemy,other,settings,out control,out error)&&error.Contains(owner.Value),"Native fight-wide cross-source conflict");
        f.Paused=true;Check(main.IsActive&&main.TrySet(new ModCameraSettings(500),out error),"Pause keeps retained control");f.Paused=false;
        f.Player=new Model();Check(main.IsActive,"Canonical main form handover keeps lifetime");
        f.Player.Alive=false;Check(!main.IsActive&&f.GetEclipseCameraSettings(f._Camera.Render)==null,"Current main death restores native view");f.Player.Alive=true;
        Check(f.TryAcquireEclipseCamera(f.Enemy,owner,settings,out var enemy,out error),"Enemy host acquisition");
        f.round.round++;Check(!enemy.IsActive,"Round identity expires control");
        Check(f.TryAcquireEclipseCamera(f.Player,owner,settings,out main,out error),"Next round acquisition");
        ModRuntime.Scripts=new ModScriptSession();Check(!main.IsActive,"Script-session replacement expires control");
        Check(f.TryAcquireEclipseCamera(f.Player,owner,settings,out main,out error),"New session acquisition");
        ModRuntime.Scripts.Active=false;Check(!main.IsActive,"Owner session disabled");ModRuntime.Scripts.Active=true;
        var actor=new OwnedActor{Owner=owner,Model=new Model(),Birth=new object()};f._eclipseActors.Add(actor.Model,actor);
        Check(!f.TryAcquireEclipseCamera(actor.Model,owner,settings,out control,out error),"Initializing actor rejected");actor.Birth=null;
        Check(!f.TryAcquireEclipseCamera(actor.Model,other,settings,out control,out error),"Foreign actor cannot acquire");
        Check(f.TryAcquireEclipseCamera(actor.Model,owner,settings,out var actorCamera,out error),"Initialized owned actor acquisition");
        f._eclipseActors.Remove(actor.Model);actor.Model=new Model();f._eclipseActors.Add(actor.Model,actor);
        Check(actorCamera.IsActive,"Same actor record survives form handover");
        actor.Active=false;Check(!actorCamera.IsActive,"Actor retirement expires camera");
        Check(f.TryAcquireEclipseCamera(f.Player,owner,settings,out main,out error),"Fight cleanup acquisition");
        f._eclipseCamera.Clear();Check(!main.IsActive,"Explicit boundary cleanup releases camera");
        Check(f.TryAcquireEclipseCamera(f.Player,owner,settings,out main,out error),"Reacquisition after clear");
        Current=new Fight();Check(!main.IsActive&&f.GetEclipseCameraSettings(f._Camera.Render)==null,"Fight identity change expires camera");
        Projection();Console.WriteLine("Camera native policy PASS: "+checks+" checks; production acquisition/lifetime/projection with controlled native eligibility, session, actor and rounded layer services.");
    }
    static void Projection()
    {
        var projection=new ModCameraProjection();var game=new LocationSelector();var backdrop=new LocationSelector();
        game.SetPositionY(12.34f);backdrop.SetPositionY(-40.12f);
        for(int i=0;i<3000;i++)
        {
            projection.Begin();
            Check(Math.Abs(game.Body.transform.localPosition.y-12.34f)<.001&&Math.Abs(backdrop.Body.transform.localPosition.y+40.12f)<.001,"Presentation redraw restores rounded baseline");
            projection.ApplyVertical(game,17.3333,1.2345f,1);
            projection.ApplyVertical(backdrop,17.3333,1.2345f,.4f);
            Check(Math.Abs(game.Body.transform.localPosition.y+9.06f)<.001&&Math.Abs(backdrop.Body.transform.localPosition.y+48.68f)<.001,"Native positive-down pan and parallax");
        }
        projection.Begin();Check(Math.Abs(game.Body.transform.localPosition.y-12.34f)<.001,"Release removes previous pan");
        projection.ApplyVertical(game,0,2,1);projection.Begin();Check(Math.Abs(game.Body.transform.localPosition.y-12.34f)<.001,"Zero pan leaves native baseline");
        projection.ApplyVertical(game,-30,2,1);Check(Math.Abs(game.Body.transform.localPosition.y-72.34f)<.001,"Negative pan at changed zoom");
        projection.Begin();game.SetPositionY(50);projection.ApplyVertical(game,25,1,1);projection.Begin();Check(game.Body.transform.localPosition.y==50,"Native baseline changes preserved");
        projection.ApplyVertical(game,25,1,1);game.Body=null;projection.Begin();Check(true,"Destroyed layer safely dropped");
    }
}
