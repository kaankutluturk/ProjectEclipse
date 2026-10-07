using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Eclipse.Modding;

// Controlled engine boundaries: numeric anti-cheat storage, profile mode, math,
// XML scalar conversion and non-item reward implementations. Native reward
// parsing/composition/choice and item constructor code are loaded by the runner.
namespace UnityEngine { public static class Mathf { public static float Pow(float x,float y)=>(float)Math.Pow(x,y); } public static class Debug { public static void LogWarning(object message){} } }
public static class Scalars {
 public static int ParseInt(this XmlNode n,int fallback=0)=>n==null?fallback:int.Parse(n.Value,CultureInfo.InvariantCulture);
 public static uint ParseUint(this XmlNode n)=>n==null?0:uint.Parse(n.Value,CultureInfo.InvariantCulture);
 public static long ParseLong(this XmlNode n,long fallback=0)=>n==null?fallback:long.Parse(n.Value,CultureInfo.InvariantCulture);
 public static float ParseFloat(this XmlNode n,float fallback=0)=>n==null?fallback:float.Parse(n.Value,CultureInfo.InvariantCulture);
 public static bool ParseBool(this XmlNode n)=>n!=null&&n.Value=="1";
 public static string GetStringOrDefault(this XmlNode n,string fallback)=>n?.Value??fallback;
 public static void RandomizeCryptoKey<T>(this T n){}
}
public class ListSF {
 public static bool Eclipse; public static int CurrentLevel=4;public static Inventory Inventory=new Inventory();public static ItemCatalog Catalog=new ItemCatalog();
 public static ListSF GetRoster()=>new ListSF();public bool IsEclipseMode()=>Eclipse;
 public int GetLevel()=>CurrentLevel;public int Level=>CurrentLevel;public Inventory GetInventory()=>Inventory;public static ItemCatalog GetItems()=>Catalog;
}
public class UserItem {}
public class Inventory { public HashSet<string> Owned=new HashSet<string>(); public UserItem FindItem(string name)=>Owned.Contains(name)?new UserItem():null; }
public class ItemCatalog { public Dictionary<string,ItemInfo> Items=new Dictionary<string,ItemInfo>();public ItemInfo GetItemByName(string name)=>Items.TryGetValue(name,out var item)?item:null; }
public class UpgradeData { public int Number; }
public class ItemInfo {
 public string Name,Type="Weapon";public int ItemLevel=4;public int Upgrade;
 public List<UpgradeData> Upgrades=new List<UpgradeData>(); public ItemInfo LevelVariant;
 public ItemInfo GetUpdateItemByLevel(int level,bool flag)=>LevelVariant;
 public ItemInfo GetUpgradeItemAtOrAboveUpgradeLevel(int level)=>Upgrades.Where(u=>u.Number>=level).Select(CreateUpgradedItem).FirstOrDefault();
 public List<UpgradeData> GetUpgrades(bool flag,int level)=>Upgrades;
 public ItemInfo CreateUpgradedItem(UpgradeData data)=>new ItemInfo{Name=Name,Type=Type,ItemLevel=ItemLevel,Upgrade=data.Number};
}
public class Result {
 public long Money,Bonus;public uint exp;
 public List<Rewardable> Other=new List<Rewardable>();
 public void AddReward(RewardMoney item)=>Other.Add(item);
 public void AddReward(RewardCurrency item)=>Other.Add(item);
 public void AddReward(RewardResistance item)=>Other.Add(item);
 public void AddReward(Rewardable item){if(item is RewardItem value)AddReward(value);else if(item is RewardLottery lottery)AddReward(lottery);else Other.Add(item);}

 public struct ItemGrant { public ItemInfo Item;public RewardItem RewardSource;public bool IsDrop; }
 public List<ItemGrant> Items=new List<ItemGrant>();
 public RewardLottery Lottery;
 /* LOTTERY SELECTION */
 /* ITEM SELECTION */
}
public static class GameUtils { public static long GetDenominatedValue(long value,int n)=>value; }
public static class NekkiMath { public static float Position; public static float randomFloat(float a,float b)=>a+(b-a)*Position; }
public class PerkStruct { public PerkStruct(XmlNode n){} }
public class RewardItem:Rewardable {
 public string Name; public uint UpgradeNumber; protected string levelExpression;
 internal string UpgradeLevelExpression {get;private set;}
 internal string EclipseRewardId {get;private set;} internal int EclipseGrantIndex {get;private set;}=-1;
 internal bool HasEclipseGrantConfiguration=>!string.IsNullOrEmpty(EclipseRewardId); private XmlElement _sourceNode;
 public int EvaluateUpgradeLevel()=>int.Parse(UpgradeLevelExpression);
 public List<PerkStruct> enchantments=new List<PerkStruct>();
 public int EvaluateLevel()=>0;
 /* ITEM CONSTRUCTOR */
}
namespace Eclipse.Modding {
 public static partial class ModRuntime {
  public static bool TryConfigureRewardGrant(global::RewardItem source,int playerLevel,out global::RewardItem configured,out string error){configured=source;error=string.Empty;return true;}
 }
}
public class RewardMoney:Rewardable { public RewardMoney(XmlNode n){} }
public class RewardCurrency:Rewardable { public RewardCurrency(XmlNode n){} }
public class RewardResistance:Rewardable { public RewardResistance(XmlNode n){} }

public class Adapter {
 readonly ModContentCatalog _content; public Adapter(ModContentCatalog content){_content=content;}
 public XmlElement Build(XmlDocument doc,RewardDefinition r)=>BuildRewardNode(doc,r);
 /* BUILDERS */
}
public static class Program {
 /* LOTTERY BUILDER */
 static int checks;static void Check(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
 public static void Main(string[] args){
  var catalog=new ModContentCatalog();var itemDoc=new XmlDocument();itemDoc.LoadXml("<Item Type='Weapon' Name='TEST_REWARD' WeaponDamage='1'/>");
  CoreContentImporter.ImportWeapons(catalog,new[]{itemDoc.DocumentElement},new Dictionary<string,XmlDocument>());
  var mod=ModDiscovery.DiscoverLoose(args[0]).Mods.Single(m=>m.Id.Value=="example.core-fight");
  RewardDefinition reward;
  using(var tx=catalog.BeginRegistration(mod)){
   var id=DefinitionId.Parse("core:items/weapon/TEST_REWARD");
   reward=tx.RegisterReward("native",new[]{new RewardItemGrant(id,2)},new[]{new RewardChoiceDefinition(new[]{new RewardChoiceItem(new RewardItemGrant(id,3),1)})});tx.Commit();
  }
  var adapter=new Adapter(catalog);
  const string source="<Fight><Rewards><Reward Money='5'/><Reward Money='77' Bonus='2' Exp='4' PrizeBase='1'><Item Name='shared'/><Level Min='3' Max='9' Money='10'><Item Name='shared-level'/></Level><NormalModeReward Exp='6'><Item Name='normal'/></NormalModeReward><EclipseModeReward Bonus='7'><Item Name='eclipse'/><Level Min='3' Max='9' Exp='8'><Item Name='old'/></Level></EclipseModeReward></Reward></Rewards></Fight>";
  var doc=new XmlDocument();doc.LoadXml(source);string original=doc.OuterXml;
  ModRewardDropProjection.Apply(doc.DocumentElement,1,ModRuleMode.Eclipse,3,9,reward,r=>adapter.Build(doc,r));
  Check(doc.SelectSingleNode("Fight/Rewards/Reward[1]").Attributes["Money"].Value=="5","Other result slot changed");
  foreach(bool eclipse in new[]{false,true})foreach(int level in new[]{2,3,9,10}){
   ListSF.Eclipse=eclipse;
   var before=new XmlDocument();before.LoadXml(original);
   var baseline=new RewardStruct(before.SelectSingleNode("Fight/Rewards/Reward[2]"),0,0).GetPrizeForLevel(level);
   var native=new RewardStruct(doc.SelectSingleNode("Fight/Rewards/Reward[2]"),0,0);
   var actual=native.GetPrizeForLevel(level);bool applies=eclipse&&level>=3&&level<=9;
   Check(actual.money==baseline.money&&actual.bonus==baseline.bonus&&actual.exp==baseline.exp&&actual.prizeBase==baseline.prizeBase,"Native composition changed economy");
   Check(actual.items.Any(i=>i.Name=="TEST_REWARD")==applies,"Eclipse/level gating incorrect");
   Check(actual.items.Any(i=>i.Name=="shared"),"Shared item lost");
   Check(actual.items.Any(i=>i.Name=="old")==false,"Replaced level item remained");
   if(applies){
    var item=actual.items.Single(i=>i.Name=="TEST_REWARD");
    Check(item.IsDrop&&item.UpgradeNumber==2,"Builder/native item identity, Drop or upgrade lost");
    NekkiMath.Position=0.5f;var choice=(RewardItem)actual.choices.Single().ChooseRandomReward();
    Check(choice.Name=="TEST_REWARD"&&choice.UpgradeNumber==3&&choice.IsDrop,"Native weighted choice lost item data");
   }
   var again=native.GetPrizeForLevel(level);
   Check(again.items.Count==actual.items.Count&&again.choices.Count==actual.choices.Count,"Repeated reward evaluation accumulated grants");
  }
  var lotteryDoc=new XmlDocument();lotteryDoc.LoadXml("<Reward><Lottery/><EclipseModeReward><Item Name='extra'/></EclipseModeReward></Reward>");
  ListSF.Eclipse=true;
  var withLottery=new RewardStruct(lotteryDoc.DocumentElement,0,0).GetPrizeForLevel(4);
  Check(withLottery.lottery!=null&&withLottery.items.Single().Name=="extra","Lottery plus non-lottery mode reward failed composition");
  lotteryDoc.LoadXml("<Reward><Lottery Type='Gold'><Slot/></Lottery><EclipseModeReward><Lottery Type='Gold'><Slot/></Lottery></EclipseModeReward></Reward>");
  var repeat=new RewardStruct(lotteryDoc.DocumentElement,0,0);
  var first=repeat.GetPrizeForLevel(4);var second=repeat.GetPrizeForLevel(4);
  Check(first.lottery.slots.Count==2&&second.lottery.slots.Count==2,"Repeated evaluation accumulated lottery slots");
  ListSF.Eclipse=false;var normal=repeat.GetPrizeForLevel(4);
  Check(normal.lottery.slots.Count==1,"Eclipse evaluation contaminated normal lottery");
  first.lottery.slots.Clear();
  Check(second.lottery.slots.Count==2&&repeat.GetPrizeForLevel(4).lottery.slots.Count==1,"Returned lottery collections alias source/other results");
  // Feed actual builder/parser output into the production result item selector.
  ListSF.CurrentLevel=4;ListSF.Eclipse=true;
  var selectedPrize=new RewardStruct(doc.SelectSingleNode("Fight/Rewards/Reward[2]"),0,0).GetPrizeForLevel(4);
  var grant=selectedPrize.items.Single(i=>i.Name=="TEST_REWARD");
  var sourceItem=new ItemInfo{Name="TEST_REWARD",Upgrades=new List<UpgradeData>{new UpgradeData{Number=0},new UpgradeData{Number=1},new UpgradeData{Number=2}}};
  ListSF.Catalog.Items.Add(sourceItem.Name,sourceItem);
  var result=new Result();result.AddReward(grant);
  Check(result.Items.Count==1&&result.Items[0].Item.Upgrade==2&&result.Items[0].IsDrop,"Native result lost projected upgrade/drop");
  Check(ReferenceEquals(result.Items[0].RewardSource,grant),"Native result detached item grant metadata");
  ListSF.Inventory.Owned.Add(sourceItem.Name);var owned=new Result();owned.AddReward(grant);
  Check(owned.Items.Count==0,"Owned equipment was regranted");ListSF.Inventory.Owned.Clear();
  var missing=new Result();var missingDoc=new XmlDocument();missingDoc.LoadXml("<Item Name='missing'/>");missing.AddReward(new RewardItem(missingDoc.DocumentElement));missing.AddReward((RewardItem)null);
  Check(missing.Items.Count==0,"Missing/null item selected");
  grant.UpgradeNumber=99;var clamped=new Result();clamped.AddReward(grant);
  Check(clamped.Items.Single().Item.Upgrade==2,"Native upgrade clamp failed");
  var encodedDoc=new XmlDocument();encodedDoc.LoadXml("<Item Name='TEST_REWARD' UpgradeLevel='400'/>");
  sourceItem.Upgrades.Add(new UpgradeData{Number=400});sourceItem.Upgrades.Add(new UpgradeData{Number=401});
  var encoded=new Result();encoded.AddReward(new RewardItem(encodedDoc.DocumentElement));
  Check(encoded.Items.Single().Item.Upgrade==400,"Encoded level was interpreted as ordinal");
  encodedDoc.LoadXml("<Item Name='TEST_REWARD' UpgradeLevel='500'/>");
  var unavailable=new Result();bool rejected=false;try{unavailable.AddReward(new RewardItem(encodedDoc.DocumentElement));}catch(InvalidOperationException){rejected=true;}
  Check(rejected&&unavailable.Items.Count==0,"Unavailable encoded level silently fell back");
  encodedDoc.LoadXml("<Item Name='TEST_REWARD' UpgradeLevel='-1'/>");rejected=false;
  try{new Result().AddReward(new RewardItem(encodedDoc.DocumentElement));}catch(InvalidOperationException){rejected=true;}
  Check(rejected,"Negative encoded level accepted");
  encodedDoc.LoadXml("<Item Name='TEST_REWARD' UpgradeLevel='400' UpgradeNumber='0'/>");rejected=false;
  try{new RewardItem(encodedDoc.DocumentElement);}catch(FormatException){rejected=true;}
  Check(rejected,"Ambiguous upgrade formats accepted");
  var consumeName="example.core-fight:items/consumable/token";ListSF.Catalog.Items.Add(consumeName,new ItemInfo{Name=consumeName,Type="Consumable"});ListSF.Inventory.Owned.Add(consumeName);
  missingDoc.LoadXml("<Item Name='"+consumeName+"' Drop='1'/>");var consumable=new Result();consumable.AddReward(new RewardItem(missingDoc.DocumentElement));
  Check(consumable.Items.Count==1,"Owned mod consumable blocked repeat grant");
  var lotterySourceDoc=new XmlDocument();lotterySourceDoc.LoadXml("<Lottery Type='Gold'><Slot/></Lottery>");
  var lotterySource=new RewardLottery(lotterySourceDoc.DocumentElement,0,0);
  var lotteryResult=new Result();var otherResult=new Result();
  lotteryResult.AddReward(lotterySource);otherResult.AddReward(lotterySource);
  lotteryResult.AddReward(new RewardLottery(lotterySourceDoc.DocumentElement,0,0));
  Check(lotteryResult.Lottery.slots.Count==2,"Lottery result merge lost slots");
  Check(lotterySource.slots.Count==1&&otherResult.Lottery.slots.Count==1,"Lottery result merge mutated source or another result");
  lotteryResult.AddReward((RewardLottery)null);
  Check(lotteryResult.Lottery.slots.Count==2,"Null lottery changed result");
  lotteryResult.Lottery.slots.Clear();
  Check(lotterySource.slots.Count==1,"Lottery result collection aliases caller");
  ListSF.Inventory.Owned.Clear();
  var slotDoc=new XmlDocument();slotDoc.LoadXml("<Slot Money='23' Bonus='7' Exp='11'><Item Name='TEST_REWARD' UpgradeNumber='1'><Enchantments><Perk Name='test_enchantment'/></Enchantments></Item><Money/><Currency/><Resistance/><Lottery Type='Gold'><Slot Weight='2'/></Lottery></Slot>");
  var slot=new LotteryPrizeEntry(slotDoc.DocumentElement,0,0);
  var resolvedPrize=BuildLotteryPrize(slot,4);
  Check(resolvedPrize.Money==23&&resolvedPrize.Bonus==7&&resolvedPrize.exp==11,"Selected slot scalar rewards lost");
  Check(resolvedPrize.Other.Count==3,"Selected non-item rewards not forwarded");
  Check(resolvedPrize.Items.Count==1&&resolvedPrize.Items[0].RewardSource.enchantments.Count==1,"Selected enchantment payload lost");
  Check(resolvedPrize.Lottery!=null&&resolvedPrize.Lottery.slots.Count==1,"Nested lottery silently lost");
  Check(ListSF.Inventory.Owned.Count==0,"Builder mutated inventory");
  slot.MinLevel=5;bool outOfRange=false;try{BuildLotteryPrize(slot,4);}catch(InvalidOperationException){outOfRange=true;}Check(outOfRange,"Out of range prize built");
  Console.WriteLine("PASS: "+checks+" native reward builder/parser/composition/result-selection checks; controlled host services, no inventory settlement.");
 }
}
