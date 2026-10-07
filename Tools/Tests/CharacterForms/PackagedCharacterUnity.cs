#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Eclipse.Modding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class PackagedCharacterUnity
{
    const string Active="Eclipse.PackagedCharacterUnity.Active";
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static double started,lastReport; static bool campaign,entered;
    static int checks,phase,phaseFrame; static string combatException;
    static string owner=>Argument("-characterPackageModId");
    static bool sawMove, captured; static float minWrist=float.PositiveInfinity,maxWrist=float.NegativeInfinity;
    static string Move=>owner+":moves/authored_move";
    static string Character=>owner+":warriors/authored_character";
    static bool OpponentOnly=>Environment.GetCommandLineArgs().Contains("-packagedOpponentOnly");
    static bool ImportedRig=>Environment.GetCommandLineArgs().Contains("-importedRigAcceptance");
    static bool ImportedMotion=>Environment.GetCommandLineArgs().Contains("-importedMotionAcceptance");
    static string MotionMove=>owner+":moves/"+Argument("-importedMotionClip");
    static int motionSamples;static bool motionPayloadChecked;
    static float importedEnemyLife,importedPlayerLife,importedStartX; static bool importedAttack,importedReaction,importedAi;
    static int importedReadyFrame=-1;
    static bool importedPunchCaptured,importedIdleCaptured;
    static string Argument(string key){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);if(i<0||i+1>=args.Length)throw new Exception("Missing "+key);return args[i+1];}
    static PackagedCharacterUnity(){if(SessionState.GetBool(Active,false)){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Update;Application.logMessageReceived+=Log;}}
    public static void Run()
    {
        var root=Path.GetDirectoryName(Application.dataPath);
        if(!File.Exists(Path.Combine(root,"packaged-character-fixture.marker")))throw new Exception("Requires isolated packaged character fixture.");
        Environment.SetEnvironmentVariable("ECLIPSE_MODS_ROOT",Argument("-characterPackageModsRoot"));
        var arguments=Environment.GetCommandLineArgs();int productIndex=Array.IndexOf(arguments,"-packaged-characterAcceptanceProductName");
        if(productIndex<0||productIndex+1>=arguments.Length||!System.Text.RegularExpressions.Regex.IsMatch(arguments[productIndex+1],"^PackagedCharacterUnity-[0-9a-f]{32}$"))throw new Exception("Pass the generated acceptance product through the runner.");
        PlayerSettings.companyName="EclipseAcceptance";PlayerSettings.productName=arguments[productIndex+1];
        SessionState.SetBool(Active,true);EditorSceneManager.OpenScene("Assets/src/GUI/Scenes/GameLoaderScene/GameLoader.unity");
        var view=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));view.Show();view.Focus();EditorApplication.EnterPlaymode();
    }
    static object Field(object value,string name)=>value.GetType().GetField(name,Hidden|BindingFlags.Public).GetValue(value);
    static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
    static void Update()
    {
        if(!EditorApplication.isPlaying)return;
        try{
            if(ModRuntime.Host!=null&&ModRuntime.Host.HasErrors)throw new Exception(ModRuntime.Host.FormatReport());
            if(ModRuntime.Scripts!=null&&ModRuntime.Scripts.HasErrors)throw new Exception("Generated package script registration failed");
            if(combatException!=null)throw new Exception(combatException);
            if(EditorApplication.timeSinceStartup-started>300)throw new Exception("Timed out phase "+phase);
            if(EditorApplication.timeSinceStartup-lastReport>20){
                lastReport=EditorApplication.timeSinceStartup;Debug.Log("[PackagedCharacterUnity] Waiting entered="+entered+" phase="+phase);
                if(ImportedMotion&&Fight.GetCurrentFight()?.GetPlayerModel()!=null){
                    var waiting=Fight.GetCurrentFight();
                    Debug.Log("[PackagedCharacterUnity] Motion readiness stage="+waiting.stageType+" current="+waiting.GetPlayerModel().GetCurrentAnimation()?.Name+" frame="+waiting.get_FightTimeInFrames()+" punch="+waiting.Controller.IsQuadrantEnabled(FightCID.Punch)+" overlay="+(UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null));
                }
            }
            if(!campaign&&Eclipse.UI.TitleScreen.IsOpen){
                var title=UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.TitleScreen>();
                if((bool)Field(title,"splashing")||(string)Field(title,"currentPage")!="Home")return;
                typeof(Eclipse.UI.TitleScreen).GetMethod("BeginCampaign",Hidden).Invoke(title,null);
                var directory=SF2Paths.GetUserDataDirectory();
                Check(Eclipse.Saves.CampaignSaveSession.PreviewDirectory==null&&directory.StartsWith(Application.persistentDataPath,StringComparison.OrdinalIgnoreCase)&&Application.persistentDataPath.Contains("PackagedCharacterUnity-"),"Profile not isolated");
                var profile=XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(),"usersDefault.xml",XmlUtils.XmlSourceMode.Normal,true,XmlCryptoUtils.GetIsEncryptionEnabled());
                ((System.Xml.XmlElement)profile.SelectSingleNode("/Root/Warriors/Warrior[@ID='1']")).SetAttribute("Tutorial","END");
                Directory.CreateDirectory(directory);XmlUtils.SaveDocumentWithHash(profile,Path.Combine(directory,Constants.UsersFileName).Replace('\\','/'));
                campaign=true;return;
            }
            if(!entered){
                if(ModRuntime.Scripts==null||Module.GetInstance()==null||Eclipse.UI.TitleScreen.IsOpen||UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null)return;
                var screen=Module.GetInstance().GetCurrentScreenType();if(screen!=ScreenType.ModuleDojo&&screen!=ScreenType.ModuleMap)return;
                Check(!ModRuntime.Host.HasErrors,ModRuntime.Host.FormatReport());Check(ModRuntime.Host.EnabledMods.Any(m=>m.Id.Value==owner),"Generated character package not enabled");Check(!ModRuntime.Scripts.HasErrors,"Startup mod errors");
                var encounter=ListSF.GetFightById(new FightIDS(ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse(owner+":fights/preview"))));
                Check(encounter!=null,"Core encounter missing");entered=GameUtils.StartFight(encounter,false,null,true,false);return;
            }
            var fight=Fight.GetCurrentFight();if(fight==null||fight.get_FightTimeInFrames()<10)return;
            if(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count!=0)
                throw new Exception("Packaged character callback failed: "+string.Join(";",ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Select(f=>f.Error)));
            if(fight.GetFightDefinition()?.FightId.ToString()!=ModRuntime.Scripts.Content.RuntimeFightId(DefinitionId.Parse(owner+":fights/preview")))return;
            var player=fight.GetPlayerModel();var enemy=fight.GetEnemyModel();if(player==null||enemy==null)return;
            player.Parameters.set_IsImmortalityEnabled(!ImportedRig);enemy.Parameters.set_IsImmortalityEnabled(!ImportedRig);

            int frame=fight.get_FightTimeInFrames();
            if(ImportedMotion){UpdateMotion(fight,player,enemy,frame);return;}
            if(ImportedRig){UpdateImported(fight,player,enemy,frame);return;}
            switch(phase){
            case 0:
                if(UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null||!fight.Controller.IsQuadrantEnabled(FightCID.Punch))return;
                var definition=ModRuntime.Scripts.Content.Fights.Single(f=>f.Id.Namespace.Value==owner);
                Check(definition.PlayerCharacter==(OpponentOnly?(DefinitionId?)null:DefinitionId.Parse(Character)),"Generated public fight player selection");
                Check(enemy.Parameters.EclipseCharacterId==Character,"Native generated opponent identity");
                Check(player.Parameters.UserControlled&&!player.Parameters.AiControlled,"Canonical player retains native controls");
                Check(OpponentOnly?player.Parameters.EclipseCharacterId!=Character:player.Parameters.EclipseCharacterId==Character,"Native player role matches package option");
                ValidateSkin(enemy);if(!OpponentOnly)ValidateSkin(player);
                enemy.Parameters.AiControlled=OpponentOnly;
                if(OpponentOnly){phase=4;phaseFrame=frame;break;}
                var clip=player.GetAvailableAnimations().Single(m=>m.Name==Move);
                Check(clip.AnimationEndFrame==59&&ModRuntime.Scripts.Content.Moves.Single(m=>m.Id.ToString()==Move).MidFrames==0,"Native clip sample bounds and timing");
                Position(player,450);Position(enemy,1000);
                Send(fight,0);phase=1;phaseFrame=frame;break;
            case 1:
            case 3:
                if(frame-phaseFrame>=3)Send(fight,1);
                if(player.GetCurrentAnimation()?.Name==Move){
                    if(!sawMove&&phase==3){var animation=Field(player,"_Animation");Check((int)Field(animation,"sign")==-1,"Native exported Punch faces opposite direction");}
                    if(!sawMove){var data=(Vector3[][])Field(player.GetCurrentAnimation(),"_AnimationContainer");Check(data.Length==60&&data.All(f=>f.Length==67),"Unmodified exported clip loads native point count");}
                    sawMove=true;var wrist=player.GetModelObject().FindNodeOrParent("NWrist_1").GetEnd();var pivot=player.GetModelObject().FindNodeOrParent("NPivot").GetEnd();
                    float value=wrist.GetY()-pivot.GetY();minWrist=Mathf.Min(minWrist,value);maxWrist=Mathf.Max(maxWrist,value);
                    if(!captured){captured=true;new GameObject("Packaged player capture").AddComponent<PackagedCharacterCapture>();}
                }
                if(frame-phaseFrame>240)throw new Exception("Generated Punch export did not play/finish: "+player.GetCurrentAnimation()?.Name);
                if(!sawMove||player.GetCurrentAnimation()?.Name==Move)return;
                Check(maxWrist-minWrist>5,"Authored IK wrist motion reaches native model");
                Check(!Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput,"Generated player has no orphan mod input capture");
                if(phase==1){
                    ValidateSkin(player);Position(player,1000);Position(enemy,450);phase=2;phaseFrame=frame;
                }else{enemy.Parameters.AiControlled=true;phase=4;phaseFrame=frame;sawMove=false;}
                break;
            case 2:
                if(frame-phaseFrame<30)return;
                sawMove=false;minWrist=float.PositiveInfinity;maxWrist=float.NegativeInfinity;Send(fight,0);phase=3;phaseFrame=frame;break;
            case 4:
                sawMove|=enemy.GetCurrentAnimation()?.Name==Move;
                if(frame-phaseFrame>480)throw new Exception("Generated native AI did not choose authored export: "+enemy.GetCurrentAnimation()?.Name);
                if(!sawMove||enemy.GetCurrentAnimation()?.Name==Move)return;
                Check(sawMove,"Generated Lua tactic reaches native opponent animation");ValidateSkin(enemy);
                if(!captured){captured=true;new GameObject("Packaged opponent capture").AddComponent<PackagedCharacterCapture>();}
                typeof(Fight).GetMethod("AbortFight",Hidden).Invoke(fight,new object[]{GameOverTypes.GAME_OVER_SURRENDER});phase=5;phaseFrame=frame;break;
            case 5:
                if(!PackagedCharacterCapture.Done)return;
                Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0,"Generated package callback errors");
                Check(!Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput,"End cleanup leaves no mod input capture");
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"packaged-character-result.txt"),"PASS: "+checks+" full-game generated character checks; actual exported models/skin, public Lua fight selection, "+(OpponentOnly?"canonical player with exported opponent":"60x67 player clip, native Punch in both facing directions and IK deformation")+", weighted skin helpers, generated AI selection and surrender cleanup. Controlled Gymnast scene/rig/events; attack contacts, physical devices, arbitrary rigs and exported platforms remain outside this acceptance.");
                Debug.Log("[PackagedCharacterUnity] PASS: "+checks+" full-game checks");Finish(0);break;
            }
        }catch(Exception error){Debug.LogError("[PackagedCharacterUnity] FAIL: "+error);Finish(1);}
    }
    static void Position(Model model,float x)=>model.GetType().GetMethod("TrainingMoveToX",Hidden|BindingFlags.Public).Invoke(model,new object[]{x});
    static void Control(Fight fight,int action,FightCID key)=>fight.Controller.GetType().GetMethod("SendGamepadControlEvent",Hidden|BindingFlags.Public).Invoke(fight.Controller,new object[]{action,key});
    static void UpdateMotion(Fight fight,Model player,Model enemy,int frame)
    {
        switch(phase){
        case 0:
            if(fight.stageType!=StageType.Stage.STAGE_FIGHT)return;
            enemy.Parameters.AiControlled=false;
            if(UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null||!fight.Controller.IsQuadrantEnabled(FightCID.Punch))return;
            Check(player.Parameters.EclipseCharacterId==Character&&enemy.Parameters.EclipseCharacterId==Character,"Imported motion uses public player/opponent identities");
            Check(player.Parameters.UserControlled&&!player.Parameters.AiControlled,"Imported motion retains native player input");
            var move=ModRuntime.Scripts.Content.Moves.Single(m=>m.Id.ToString()==MotionMove);
            Check(move.Conditions.SelectMany(c=>c.Keys).Single().Key=="Punch","This native fixture selects a Punch-bound source clip");
            Check(move.MidFrames==0&&!move.Intervals.Any(i=>i.Attack!=null),"Source clip keeps 60 Hz spacing and does not invent attacks");
            motionSamples=move.EndFrame+1;
            Check(player.GetAvailableAnimations().Any(m=>m.Name==MotionMove),"Native source-action eligibility");
            ValidateImportedSkin(player);Position(player,450);Position(enemy,1000);Send(fight,0);phase=1;phaseFrame=frame;break;
        case 1:
        case 3:
            if(frame-phaseFrame>=3)Send(fight,1);
            if(player.GetCurrentAnimation()?.Name==MotionMove){
                if(!sawMove)Check(player.FacingSign==(phase==1?1:-1),"Source action faces the opponent under native input");
                sawMove=true;
                if(!motionPayloadChecked){CheckMotionPayload(player.GetCurrentAnimation());motionPayloadChecked=true;}
                var wrist=player.GetModelObject().FindNodeOrParent("NWrist_1").GetStart().GetX();
                minWrist=Math.Min(minWrist,wrist);maxWrist=Math.Max(maxWrist,wrist);
                if(!captured&&frame-phaseFrame>motionSamples/3){
                    captured=true;PackagedCharacterCapture.Done=false;
                    new GameObject("Imported source action capture").AddComponent<PackagedCharacterCapture>().FileName="imported-source-motion.png";
                }
            }
            if(frame-phaseFrame<motionSamples+90)return;
            Check(sawMove&&maxWrist-minWrist>1,"Evaluated source action drives native visible wrist motion");ValidateImportedSkin(player);
            if(phase==1){Position(player,1000);Position(enemy,450);player.PlayAnimation("StanceIdle",-1);phase=2;phaseFrame=frame;}
            else{enemy.Parameters.AiControlled=true;sawMove=false;phase=4;phaseFrame=frame;}
            break;
        case 2:
            if(frame-phaseFrame<30)return;
            sawMove=false;minWrist=float.PositiveInfinity;maxWrist=float.NegativeInfinity;Send(fight,0);phase=3;phaseFrame=frame;break;
        case 4:
            sawMove|=enemy.GetCurrentAnimation()?.Name==MotionMove;
            if(frame-phaseFrame>480)throw new Exception("Imported source action was not selected by native Lua AI");
            if(!sawMove)return;
            Check(sawMove,"Source action reaches native opponent through public Lua AI");ValidateImportedSkin(enemy);
            phase=5;break;
        case 5:
            if(!PackagedCharacterCapture.Done)return;
            typeof(Fight).GetMethod("AbortFight",Hidden).Invoke(fight,new object[]{GameOverTypes.GAME_OVER_SURRENDER});phase=6;break;
        case 6:
            Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0&&!Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput,"Imported motion cleanup");
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"packaged-character-result.txt"),"PASS: "+checks+" imported source-motion assertions; unchanged exported payload, public Lua/native player input on both controlled facings, visible wrist movement, weighted skin bounds, eligible-action Lua AI and surrender cleanup. Most assertions compare binary points. No authored attacks, physical devices, arbitrary actions/body plans or exported platforms are accepted; screenshot requires inspection.");
            Debug.Log("[PackagedCharacterUnity] PASS imported motion: "+checks);Finish(0);break;
        }
    }
    static void CheckMotionPayload(InfoAnimation move)
    {
        var actual=(Vector3[][])Field(move,"_AnimationContainer");
        string path=Path.Combine(Argument("-characterPackageModsRoot"),owner,"assets/animations",Argument("-importedMotionClip")+".bytes");
        using(var reader=new BinaryReader(File.OpenRead(path))){
            Check(reader.ReadInt32()==motionSamples&&actual.Length==motionSamples,"Native source clip duration");
            for(int f=0;f<motionSamples;f++){
                reader.ReadByte();int nodes=reader.ReadInt32();Check(nodes==67&&actual[f].Length==nodes,"Native source clip point order/count");
                for(int n=0;n<nodes;n++){
                    var expected=new Vector3(reader.ReadSingle(),-reader.ReadSingle(),reader.ReadSingle());
                    Check(Vector3.Distance(expected,actual[f][n])<.0002,"Actual native reader preserves imported source motion");
                }
            }
            Check(reader.BaseStream.Position==reader.BaseStream.Length,"Complete source payload consumed");
        }
    }
    static void UpdateImported(Fight fight,Model player,Model enemy,int frame)
    {
        switch(phase){
        case 0:
            if(UnityEngine.Object.FindFirstObjectByType<Eclipse.UI.EclipseLoadingOverlay>()!=null||!fight.Controller.IsQuadrantEnabled(FightCID.Punch))return;
            if(player.GetCurrentAnimation()?.Name!="StanceIdle")return;
            if(importedReadyFrame<0){importedReadyFrame=frame;return;}
            if(frame-importedReadyFrame<6)return;
            if(!importedIdleCaptured){
                enemy.Parameters.AiControlled=false;
                if(enemy.GetCurrentAnimation()?.Name!="StanceIdle")return;
                importedIdleCaptured=true;PackagedCharacterCapture.Done=false;
                new GameObject("Imported idle capture").AddComponent<PackagedCharacterCapture>().FileName="imported-character-idle.png";
                return; // Render before the test teleports or dispatches movement.
            }
            if(!PackagedCharacterCapture.Done)return;
            Check(player.Parameters.EclipseCharacterId==Character&&enemy.Parameters.EclipseCharacterId==Character,"Imported public encounter identities");
            Check(player.Parameters.UserControlled&&!player.Parameters.AiControlled,"Imported player native control role");
            Check(ModRuntime.Scripts.Content.Moves.All(m=>m.Id.Namespace.Value!=owner),"Imported fighter needs no authored default clips");
            Check(player.GetAvailableAnimations().Count>20,"Imported fighter inherits complete native animation eligibility");
            ValidateImportedSkin(player);ValidateImportedSkin(enemy);
            enemy.Parameters.AiControlled=false;Position(player,450);Position(enemy,850);
            importedStartX=player.GetModelObject().FindNodeOrParent("NPivot").GetStart().GetX();
            Control(fight,0,FightCID.QuadrantForward);phase=7;phaseFrame=frame;break;
        case 7:
            if(frame-phaseFrame<30)return;
            Control(fight,1,FightCID.QuadrantForward);
            Check(player.GetModelObject().FindNodeOrParent("NPivot").GetStart().GetX()>importedStartX+2,"Imported native movement input moves the combat body");
            ValidateImportedSkin(player);Position(player,450);Position(enemy,550);phase=8;phaseFrame=frame;break;
        case 8:
            if(frame-phaseFrame<30||player.GetCurrentAnimation()?.Name!="StanceIdle")return;
            importedEnemyLife=enemy.Parameters.RemainingHealthInDamageUnits;
            Control(fight,0,FightCID.Punch);phase=1;phaseFrame=frame;break;
        case 1:
            if(frame-phaseFrame>=3)Control(fight,1,FightCID.Punch);
            importedAttack|=player.GetCurrentAnimation()?.Type==InfoAnimation.AnimationKind.AnimationAttack;
            importedReaction|=enemy.GetCurrentAnimation()?.Name!="StanceIdle";
            if(!importedPunchCaptured&&enemy.Parameters.RemainingHealthInDamageUnits<importedEnemyLife){
                importedPunchCaptured=true;PackagedCharacterCapture.Done=false;
                new GameObject("Imported contact capture").AddComponent<PackagedCharacterCapture>().FileName="imported-character-punch.png";
            }
            if(frame-phaseFrame>240)throw new Exception("Imported native Punch/contact failed: "+player.GetCurrentAnimation()?.Name+" enemy="+enemy.GetCurrentAnimation()?.Name+" sawAttack="+importedAttack+" health="+enemy.Parameters.RemainingHealthInDamageUnits+" before="+importedEnemyLife);
            if(!importedAttack||enemy.Parameters.RemainingHealthInDamageUnits>=importedEnemyLife||player.GetCurrentAnimation()?.Type==InfoAnimation.AnimationKind.AnimationAttack)return;
            Check(importedAttack&&importedReaction,"Imported core attack and native victim reaction");ValidateImportedSkin(player);
            Position(player,1000);Position(enemy,450);phase=2;phaseFrame=frame;break;
        case 2:
            if(frame-phaseFrame<90)return;
            // Teleporting with TrainingMoveToX is not native crossing. Establish
            // the mirrored stance explicitly, then dispatch Kick via controls.
            Check(player.PlayAnimation("StanceIdle",-1),"Imported mirrored core stance setup");
            importedAttack=false;Control(fight,0,FightCID.Kick);phase=3;phaseFrame=frame;break;
        case 3:
            if(frame-phaseFrame>=3)Control(fight,1,FightCID.Kick);
            importedAttack|=player.GetCurrentAnimation()?.Type==InfoAnimation.AnimationKind.AnimationAttack;
            if(frame-phaseFrame>240)throw new Exception("Imported mirrored Kick input failed");
            if(!importedAttack||frame-phaseFrame<30||player.GetCurrentAnimation()?.Type==InfoAnimation.AnimationKind.AnimationAttack)return;
            Check(importedAttack,"Imported native mirrored Kick");ValidateImportedSkin(player);
            Check(player.FacingSign==-1,"Imported native Kick preserves mirrored facing");
            Position(player,650);Position(enemy,550);enemy.Parameters.AiControlled=true;
            importedPlayerLife=player.Parameters.RemainingHealthInDamageUnits;phase=4;phaseFrame=frame;break;
        case 4:
            importedAi|=enemy.GetCurrentAnimation()?.Type==InfoAnimation.AnimationKind.AnimationAttack;
            if(frame-phaseFrame>1200)throw new Exception("Imported Standard AI made no native incoming contact");
            if(!importedAi||player.Parameters.RemainingHealthInDamageUnits>=importedPlayerLife)return;
            Check(importedAi,"Imported Standard AI attacks with native damage");ValidateImportedSkin(enemy);ValidateImportedSkin(player);
            captured=true;PackagedCharacterCapture.Done=false;new GameObject("Imported fighter capture").AddComponent<PackagedCharacterCapture>();phase=5;phaseFrame=frame;break;
        case 5:
            if(!PackagedCharacterCapture.Done)return;
            typeof(Fight).GetMethod("AbortFight",Hidden).Invoke(fight,new object[]{GameOverTypes.GAME_OVER_SURRENDER});phase=6;phaseFrame=frame;break;
        case 6:
            Check(ModRuntime.Scripts.CallbackDiagnostics.RecentFailures.Count==0&&!Eclipse.UI.Modding.ModUiGameBridge.BlocksGameplayInput,"Imported encounter cleanup");
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath),"packaged-character-result.txt"),"PASS: "+checks+" imported-rig native checks; public Lua player/opponent identities, inherited animation eligibility without authored clips, movement, native Punch/contact/reaction, controlled mirrored stance and Kick, Standard AI incoming contact, weighted skin coordinates and geometry bounds on both facings and surrender cleanup. Selected imported humanoid source "+owner+"; arbitrary body plans, source animation import, physical input and exported platforms remain unverified. Screenshot requires separate visual inspection.");
            Debug.Log("[PackagedCharacterUnity] PASS imported: "+checks);Finish(0);break;
        }
    }
    static void ValidateImportedSkin(Model model)
    {
        var document=new System.Xml.XmlDocument();document.Load(Path.Combine(Argument("-characterPackageModsRoot"),owner,"assets/models/skin1.xml"));
        var helpers=document.SelectNodes("/Scene/Nodes/*[@Type='SkinnedNode']");Check(helpers.Count>0,"Imported weighted skin mounted");
        var body=model.GetModelObject();
        var driver=body.GetAllNodes().Take(67).Select(n=>n.GetStart()).ToArray();
        float minX=driver.Min(p=>p.GetX()),maxX=driver.Max(p=>p.GetX()),minY=driver.Min(p=>p.GetY()),maxY=driver.Max(p=>p.GetY());
        float allowance=Math.Max(100,Math.Max(maxX-minX,maxY-minY));
        bool bounded=true;
        foreach(System.Xml.XmlNode helper in helpers){
            var actual=body.FindNodeOrParent(helper.Name)?.GetStart();Check(actual!=null,"Imported helper binding "+helper.Name);
            float x=0,y=0;
            for(int i=1;i<=int.Parse(helper.Attributes["BonesCount"].Value);i++){
                var a=body.FindNodeOrParent(helper.Attributes["BoneStart"+i].Value).GetStart();var b=body.FindNodeOrParent(helper.Attributes["BoneEnd"+i].Value).GetStart();
                Func<string,float> number=field=>float.Parse(helper.Attributes[field+i].Value,System.Globalization.CultureInfo.InvariantCulture);
                float weight=number("Weight"),along=number("Along"),across=number("Across")*model.FacingSign;
                x+=weight*(a.GetX()+along*(b.GetX()-a.GetX())+across*(b.GetY()-a.GetY()));
                y+=weight*(a.GetY()+along*(b.GetY()-a.GetY())-across*(b.GetX()-a.GetX()));
            }
            Check(Math.Abs(actual.GetX()-x)<.02&&Math.Abs(actual.GetY()-y)<.02&&actual.GetZ()==0,"Imported native weighted skin coordinates "+helper.Name);
            bounded&=actual.GetX()>=minX-allowance&&actual.GetX()<=maxX+allowance&&actual.GetY()>=minY-allowance&&actual.GetY()<=maxY+allowance;
        }
        Check(bounded,"Imported geometry remains within the humanoid combat body's vicinity; no exploding source calibration");
    }
    static void Send(Fight fight,int action)=>fight.Controller.GetType().GetMethod("SendGamepadControlEvent",Hidden|BindingFlags.Public).Invoke(fight.Controller,new object[]{action,FightCID.Punch});
    static void ValidateSkin(Model model){
        var body=model.GetModelObject();var node=body.FindNodeOrParent("EclipseFixtureNode-3");Check(node!=null,"Exported Gymnast skin helper mounted");
        // ModelMacroNode computes current Start XY from child Starts through
        // Vector3f.AddScaledXY, which deliberately excludes Z. End retains the
        // previous helper pose. This proves planar native skin binding only.
        var actual=node.GetStart();var expected=new Vector3();
        // Coefficients come from the actual upstream HEAD_GEAR export, not a
        // reconstructed proxy. Read the installed XML used by this model.
        var document=new System.Xml.XmlDocument();document.Load(Path.Combine(Argument("-characterPackageModsRoot"),owner,"assets/models/skin1.xml"));
        var helper=document.SelectSingleNode("/Scene/Nodes/EclipseFixtureNode-3");
        for(int i=1;i<=int.Parse(helper.Attributes["NodesCount"].Value);i++){
            var point=body.FindNodeOrParent(helper.Attributes["ChildNode"+i].Value).GetStart();
            var weight=float.Parse(helper.Attributes["LCC"+i].Value,System.Globalization.CultureInfo.InvariantCulture);expected+=new Vector3(point.GetX(),point.GetY(),0)*weight;
        }
        var actualVector=new Vector3(actual.GetX(),actual.GetY(),actual.GetZ());
        Check(Vector3.Distance(actualVector,expected)<.01,"Native exported skin follows weighted head landmarks in XY: "+model.Parameters.EclipseCharacterId+" actual="+actualVector+" expected="+expected);
    }
    static void Log(string message,string stack,LogType type){if(entered&&type==LogType.Exception&&(stack.Contains("Fight.")||stack.Contains("Model.")||stack.Contains("Modding")))combatException=message+"\n"+stack;}
    static void Finish(int code){SessionState.SetBool(Active,false);EditorApplication.update-=Update;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
public sealed class PackagedCharacterCapture : MonoBehaviour
{
    internal static bool Done;
    internal string FileName="packaged-character-native.png";
    IEnumerator Start(){yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath),FileName),texture.EncodeToPNG());Done=true;UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(gameObject);}
}
#endif
