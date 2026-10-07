using System;
using System.IO;
using System.Xml;
public static class GameUtils { public static PerkItems PerkItemList = new PerkItems(); }
public static class XmlFixtureExtensions {
    public static string GetStringOrDefault(this XmlAttribute value,string fallback="") => value?.Value ?? fallback;
    public static int ParseInt(this XmlAttribute value,int fallback=0) => int.TryParse(value?.Value,out int n)?n:fallback;
    public static XmlNode AppendImportedClone(this XmlNode parent,XmlNode child) {
        var doc=parent as XmlDocument ?? parent.OwnerDocument;
        return parent.AppendChild(doc.ImportNode(child,true));
    }
    public static XmlNode AppendElement(this XmlNode parent,string name) {
        var doc=parent as XmlDocument ?? parent.OwnerDocument; return parent.AppendChild(doc.CreateElement(name));
    }
}
// Only the unrelated perk parser/presentation surface is stubbed. Clone below is
// extracted verbatim from production, and PerkItems is compiled in full.
public sealed class PerkInfoItem {
    public string Name,DescriptionKey,MoveName;
    public bool IsClone,IsHidden;
    public int Level,UpgradeLevel;
    public XmlNode Source;
    public void Parse(XmlNode node) {Source=node.CloneNode(true);Name=node.Attributes["Name"].Value;DescriptionKey=node.Attributes["Description"]?.Value;}
    public XmlNode GetDefaultNode()=>Source;
    public void ReleaseDefinitionXml() {}
    // CLONE_BODY
}
public static class Program {
    static int checks;
    static void Check(bool condition,string message) {checks++;if(!condition)throw new Exception(message);}
    static XmlNode Node(string xml){var d=new XmlDocument();d.LoadXml(xml);return d.DocumentElement;}
    public static void Main(string[] args) {
        var perks=GameUtils.PerkItemList;
        perks.Parse(Node("<Perks><Perk Name='vanilla' Description='unchanged'/></Perks>"));
        perks.ParseProgression(Node("<Perks><Perk Name='vanilla'/></Perks>"));
        var original=perks.FindProgressionPerk("vanilla");
        var archive=new XmlDocument();archive.Load(Path.Combine(args[0],"Assets/DExml/CharacterProgress.xml"));
        foreach(string name in new[]{"PERK_MASTER_OF_STYLE","PERK_RELENTLESS"}) {
            var source=archive.SelectSingleNode("CharacterProgress/Perks/Perk[@Name='"+name+"']");
            if(source==null)source=archive.SelectSingleNode("*/Perks/Perk[@Name='"+name+"']");
            Check(source!=null,"Archived upgrade record missing");
            string owned="example.upgrade:perks/"+name.ToLowerInvariant();
            var basePerk=perks.AddExternalBasePerk(Node("<Perk Name='"+owned+"' Description='base'><Set Unchanged='17'/></Perk>"));
            string before=basePerk.Source.OuterXml;
            perks.AddExternalPerkUpgrades(owned,source);
            Check(perks.GetProgressionVariants(owned).Count==6,"Expected base plus five upgrades");
            Check(perks.FindProgressionPerk(owned,0).DescriptionKey=="base","Base description changed");
            foreach(XmlNode upgrade in source.SelectNodes("UpgradeLevel")) {
                int level=int.Parse(upgrade.Attributes["Value"].Value);var variant=perks.FindProgressionPerk(owned,level);
                Check(variant!=null && variant.DescriptionKey==upgrade.Attributes["Description"].Value,"Upgrade description lost");
                foreach(XmlAttribute value in upgrade["Set"].Attributes)
                    Check(variant.Source["Set"].Attributes[value.Name].Value==value.Value,"Native upgrade parameter lost: "+value.Name);
                Check(variant.Source["Set"].Attributes["Unchanged"].Value=="17","Upgrade discarded base parameters");
            }
            Check(basePerk.Source.OuterXml==before,"Clone mutated base definition");
            bool duplicate=false;try{perks.AddExternalPerkUpgrades(owned,source);}catch(InvalidOperationException){duplicate=true;}
            Check(duplicate && perks.GetProgressionVariants(owned).Count==6,"Duplicate install changed progression");
            Check(perks.RemoveExternalBasePerk(owned) && perks.GetProgressionVariants(owned).Count==0,"External variants survived removal");

            string rankOne=owned+"_rank1";
            perks.AddExternalBasePerk(Node("<Perk Name='"+rankOne+"' Description='base'><Set Unchanged='17'/></Perk>"));
            perks.AddExternalPerkUpgrades(rankOne,source,1);
            var rankOneVariants=perks.GetProgressionVariants(rankOne);
            Check(rankOneVariants.Count==5 && rankOneVariants[0].UpgradeLevel==1 && perks.FindProgressionPerk(rankOne,0)==null,
                "Initial upgrade 1 did not publish exactly native ranks 1..5");
            for(int rank=1;rank<=5;rank++) Check(perks.FindProgressionPerk(rankOne,rank)!=null,"Missing native rank "+rank);
            Check(perks.RemoveExternalBasePerk(rankOne) && perks.GetProgressionVariants(rankOne).Count==0,"Rank-one variants survived removal");

            string missing=owned+"_missing";
            perks.AddExternalBasePerk(Node("<Perk Name='"+missing+"' Description='base'><Set Unchanged='17'/></Perk>"));
            bool missingRejected=false;try{perks.AddExternalPerkUpgrades(missing,source,6);}catch(InvalidOperationException){missingRejected=true;}
            Check(missingRejected && perks.GetProgressionVariants(missing).Count==0,"Missing initial rank partially published progression");
            Check(perks.RemoveExternalBasePerk(missing),"Missing-rank base cleanup failed");
            Check(ReferenceEquals(original,perks.FindProgressionPerk("vanilla")),"Removal changed vanilla progression");
        }
        Console.WriteLine("PASS: "+checks+" native-source perk upgrade checks; production PerkItems and Clone, archive payloads, descriptions, isolation and teardown.");
    }
}
