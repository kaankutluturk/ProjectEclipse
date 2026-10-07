using System;
using System.Collections.Generic;
using Eclipse.Modding;
using ObscuredFloat=System.Single;

// Controlled collaborators around the complete, extracted GameUtils.EndFight.
public enum GameOverTypes { GAME_OVER_WIN, GAME_OVER_LOSS, GAME_OVER_SURRENDER, GAME_OVER_RAID_TIMEOUT, GAME_OVER_RAID_ROUND_TIMEOUT }
public enum BattleType { FightTournament,FightPeriodic,FightReplayable,FightBossesReplayable,FightFinalReplayable,FightAscension,FightRaid,FightBosses,FightFinalTitan }
public enum ConditionStatus { StatusComplete }
public class ComboStatistic { }
public class ModelParameters {public bool IsPlayer;public float GetCurrentLife()=>0;}
public class RaidModelParameters:ModelParameters { }
public class FightIDS {public FightIDS(){}public FightIDS(FightIDS other){}public string GetBattle()=>"boss";}
public class Roster {public FightIDS GetFightIds()=>new FightIDS();public int GetLevel()=>1;}
public class RewardLottery { }
public class RewardPrize {public RewardLottery lottery;}
public class RewardStruct {public RewardPrize GetPrizeForLevel(int level){Program.SourceEvaluations++;return new RewardPrize{lottery=Program.DefinedLottery?new RewardLottery():null};}}
public class Clock {public void RandomizeSeeds(){} }
public class Battle {public void UpdateRosterFight(FightList f,bool won){Program.Trace.Add("progress");}public void UpdateByTime(int n){} }
public class BattleReplayable:Battle {public ConditionStatus GetStatus()=>ConditionStatus.StatusComplete;}
public class BattleAscension:Battle {public void SetAscensionLevel(int i){}public void AdvanceAscensionAfterFight(FightList f){}public FightList GetRosterBattle()=>null;public int GetFightCount()=>0;public void RefreshAllFightStatuses(){} }
public class FightList {
 public ModStoryEncounter EclipseStoryEncounter;public FightIDS FightId=new FightIDS();public int RewardIndex;
 public Battle Battle=new Battle();public BattleType Type=BattleType.FightTournament;
 public BattleType get_Type()=>Type;public bool HasMultipleOpponentsAndRounds()=>false;public RewardStruct GetRewardAt(int n)=>new RewardStruct();
 public Clock GetRosterFight()=>new Clock();public List<RewardStruct> GetRewards()=>new List<RewardStruct>{new RewardStruct()};public int GetAscensionLevel()=>0;
}
public class FightResult {
 public FightList FightDefinition;
 public RewardPrize Prize=new RewardPrize();
 public FightList FightDefinition;public BattleType FightType;public FightIDS FightId;public ModelParameters PlayerParameters,OpponentParameters;public GameOverTypes GameOverType;
 public bool IsWinner()=>GameOverType==GameOverTypes.GAME_OVER_WIN;
 public void CalculateRewards(RewardStruct r,ComboStatistic a,ComboStatistic b,FightList f){Program.Trace.Add("calculate");Prize.lottery=Program.Lottery?new RewardLottery():null;}
}
public class ArgsDict:Dictionary<string,object> { }
public static class StatisticsEvent {public enum EventType {Fight_End}}
public static class StatisticsCollector {public static void LogEvent(StatisticsEvent.EventType e,ArgsDict a){} }
public class QuestParameters {public FightIDS fightIds;public string fightResult,raidId;public int levelUp;public float fightAvgFps;}
public static class QuestEvent {public enum QuestEventType {QUEST_EVENT_RESET_ASCENSION,QUEST_EVENT_FIGHT_END,QUEST_EVENT_RAID_FIGHT_END}}
public class ListSF {
 static readonly ListSF Current=new ListSF();static readonly Roster Profile=new Roster();
 public QuestParameters LotteryQuestParameters;public static Roster GetRoster()=>Profile;public static ListSF GetInstance()=>Current;
 public static FightList GetFightById(FightIDS id)=>Program.Fight;public static int GetCurrentTime()=>0;public static void RegisterReplayableBattle(Battle b){}public static Battle GetBattleById(FightIDS id)=>Program.Fight.Battle;
 public bool ApplyFightRewards(FightResult r){Program.Trace.Add("grant");Program.Grant?.Invoke();return false;}
 public QuestParameters GetQuestParameters()=>new QuestParameters();public bool RaiseQuestEvent(QuestEvent.QuestEventType e){Program.Trace.Add("quest");return false;}
 public void RunQuestActions(){}public void RequestSave(){Program.Trace.Add("save");}
}
public static class MenuController {public static void RefreshMoney(){}public static void RefreshMenu(){} }
public class Fight {public static Fight GetCurrentFight()=>new Fight();public void ShowEndFightScreen(FightResult r){Program.Trace.Add("presentation");Program.Present?.Invoke();}}
public static class GameLog {public static void Error(string s){throw new Exception(s);} }
namespace Eclipse.Modding {
 public static class ModRuntime {
 public static ModStoryEvents StoryEvents=new ModStoryEvents();
  public static void PrepareBattleLottery(RewardLottery lottery,QuestParameters context,bool raid,string encounter){Program.Trace.Add("lottery-prepared");}
  public static void ShowPendingBattleLottery(){}
  public static ModStoryEvent CaptureBattleResult(Roster r,FightList f,GameOverTypes outcome,ModelParameters a,ModelParameters b){Program.Trace.Add("capture");return new ModStoryEvent(ModStoryEventKind.BattleResult,null,battle:new ModBattleResultSnapshot(null,outcome==GameOverTypes.GAME_OVER_WIN?"win":"loss",false));}
 }
 public static class ModModeRuntime {
  public static bool Allowed=true;public static bool CanResolve(FightList f)=>Allowed;public static bool IsRaid(FightList f)=>false;
  public static void Complete(FightList f,bool won){Program.Trace.Add("mode");}public static void SetRaidResult(FightResult r){}public static void NotifyResult(FightList f){}public static void ShowRaidResult(){}
 }
}
public static class GameUtils {
 static bool ShowRatePrompt;static bool CanShowRatePrompt(string id)=>false;
 static void SchedulePeriodicFightNotification(FightList f,bool won){}static void HandleSurrender(FightResult r){}static void TrackEvent(string s){Program.Trace.Add("finish");}
 /* END FIGHT */
}
public static class Program {
 public static List<string> Trace=new List<string>();public static Action Grant,Present;public static bool Lottery,DefinedLottery;public static int SourceEvaluations;public static FightList Fight;
 static int checks,delivered;
 static void Check(bool value,string message){checks++;if(!value)throw new Exception(message+": "+string.Join(",",Trace));}
 static void Reset(){Trace.Clear();Grant=null;Present=null;Lottery=DefinedLottery=false;SourceEvaluations=0;ListSF.GetInstance().LotteryQuestParameters=null;delivered=0;ModModeRuntime.Allowed=true;ModRuntime.StoryEvents.Clear();ModRuntime.StoryEvents.BindProfile();Fight=new FightList{EclipseStoryEncounter=ModRuntime.StoryEvents.BeginEncounter()};ModRuntime.StoryEvents.CreateScope(ModId.Parse("example.test")).Subscribe(ModStoryEventKind.BattleResult,e=>{delivered++;Trace.Add("event");});}
 static void Run(GameOverTypes outcome=GameOverTypes.GAME_OVER_WIN)=>GameUtils.EndFight(null,Fight,new ModelParameters{IsPlayer=true},new ModelParameters(),outcome);
 public static void Main(){
  Reset();Run();Check(delivered==1&&Trace[0]=="capture"&&Trace[Trace.Count-1]=="event","delivery order");Check(Trace.IndexOf("event")>Trace.IndexOf("presentation")&&Trace.IndexOf("event")>Trace.IndexOf("grant"),"premature delivery");
  Run();Check(delivered==1,"duplicate event");
  Reset();Grant=()=>{Grant=null;Run();};Run();Check(delivered==1&&Trace.FindAll(x=>x=="capture").Count==1,"reentrant completion");
  Reset();Grant=()=>{throw new Exception("grant failure");};try{Run();}catch(Exception e){if(e.Message!="grant failure")throw;}Check(delivered==0,"failed grant event");Grant=null;Run();Check(delivered==0,"failed attempt retried");
  Reset();Grant=()=>{ModRuntime.StoryEvents.UnbindProfile();ModRuntime.StoryEvents.BindProfile();};Run();Check(delivered==0,"cross-profile event");
  Reset();Present=()=>{throw new Exception("presentation failure");};try{Run();}catch(Exception e){if(e.Message!="presentation failure")throw;}Check(delivered==0,"failed presentation event");
  Reset();Run(GameOverTypes.GAME_OVER_SURRENDER);Check(delivered==1&&!Trace.Contains("grant"),"surrender reward semantics");
  Reset();Lottery=true;Run();Check(delivered==1&&!Trace.Contains("quest")&&ListSF.GetInstance().LotteryQuestParameters!=null,"deferred lottery observation");
  Check(SourceEvaluations==0,"Lottery detection re-evaluated reward definitions");
  Reset();DefinedLottery=true;Run();Check(Trace.Contains("quest")&&ListSF.GetInstance().LotteryQuestParameters==null,"Lottery from another reward scope suppressed quest progression");
  Reset();Lottery=true;Run(GameOverTypes.GAME_OVER_LOSS);Check(Trace.Contains("quest")&&ListSF.GetInstance().LotteryQuestParameters==null,"Loss deferred a lottery claim");
  Reset();Lottery=true;Run(GameOverTypes.GAME_OVER_SURRENDER);Check(Trace.Contains("quest")&&ListSF.GetInstance().LotteryQuestParameters==null,"Surrender deferred a lottery claim");
  Reset();ModModeRuntime.Allowed=false;Run();Check(delivered==0&&Trace.Count==0,"mode gate bypass");
  Reset();Fight.EclipseStoryEncounter=null;Run();Check(delivered==0&&Trace.Contains("grant"),"untracked result changed native grant");
  Console.WriteLine("PASS: "+checks+" complete native EndFight flow checks with controlled collaborators.");
 }
}
