using System;
using System.IO;
using System.Linq;
using System.Xml;
using Eclipse.Modding;

static class Program
{
    static int checks;
    static string fixture;
    static void Check(bool condition,string message){checks++;if(!condition)throw new Exception(message);}
    static (ModContentCatalog Content,string Hash) Load(string suffix,bool valid=true,string caps="content.register")
    {
        string parent=Path.Combine(fixture,Guid.NewGuid().ToString("N")),dir=Path.Combine(parent,"fixture.actors");
        Directory.CreateDirectory(Path.Combine(dir,"scripts"));
        File.WriteAllText(Path.Combine(dir,"mod.toml"),"schema=1\nid=\"fixture.actors\"\nname=\"Actors\"\nversion=\"1.0.0\"\nauthors=[\"Fixture\"]\nentrypoint=\"scripts/main.lua\"\ncapabilities=[\""+caps+"\"]\n[[dependencies]]\nid=\"core\"\nversion=\">=1.0.0 <2.0.0\"\n");
        const string start="local sf2=require('sf2');local warrior=sf2.warriors.register{id='unit',template=sf2.warriors.get_template('core:warrior-templates/default'),level=1};";
        File.WriteAllText(Path.Combine(dir,"scripts/main.lua"),start+suffix);
        var mod=ModDiscovery.DiscoverLoose(parent).Mods.Single();var content=new ModContentCatalog();
        var templates=new XmlDocument();templates.LoadXml("<Templates><Warrior Name='Default'/></Templates>");
        CoreContentImporter.ImportWarriorTemplates(content,templates.DocumentElement);
        bool accepted=false;
        using(var tx=content.BeginRegistration(mod))
        using(var ctx=new MoonSharpScriptRuntime().CreateContext(mod,new ModApiFacade(mod,new AssetResolver(new IAssetProvider[]{new LooseModProvider(mod)}),tx,new ModStateRuntime(),null)))
        {
            try{ctx.ExecuteEntrypoint();tx.Commit();accepted=true;}
            catch(Exception){if(valid)throw;}
        }
        Check(accepted==valid,"Unexpected registration result: "+suffix);
        if(!valid)Check(content.Actors.Count==0&&content.Warriors.Count==0,"Failed actor registration partially committed.");
        return (content,ModSaveData.ComputeContentSetFingerprint(new[]{mod},content));
    }
    static void Main(string[] args)
    {
        fixture=args[0];
        const string definition="sf2.actors.register{id='ally',character=warrior";
        var baseline=Load(definition+"}");
        var actor=baseline.Content.Actors.Single();
        Check(actor.Id.ToString()=="fixture.actors:actors/ally"&&!actor.OpposingTeam&&actor.AiControlled&&actor.LifetimeFrames==1800&&actor.MaxHealth==1,"Actor defaults/identity invalid.");
        Check(baseline.Hash==Load(definition+"}").Hash,"Actor fingerprint depends on loose path or registration object identity.");
        var empty=Load("");Check(empty.Hash!=baseline.Hash,"Actor definitions omitted from fingerprint.");
        foreach(string field in new[]{"team='opponent'","ai=false","lifetime_frames=60","max_health=2"})
            Check(baseline.Hash!=Load(definition+","+field+"}").Hash,"Actor fingerprint omitted "+field);
        foreach(string fields in new[]{"lifetime_frames=1,max_health=0.01","lifetime_frames=36000,max_health=100"})
            Check(Load(definition+","+fields+"}").Content.Actors.Count==1,"Actor boundary rejected.");
        foreach(string field in new[]{"team='ally'","ai=1","lifetime_frames=0","lifetime_frames=36001","lifetime_frames=1.5","max_health=0","max_health=100.01","max_health=0/0","max_health=1/0","max_health='1'","unknown=true"})
            Load(definition+","+field+"}",false);
        foreach(string call in new[]{"sf2.actors.register{}","sf2.actors.register{id='ally',character={}}","sf2.actors.register{id='ally',character=warrior},{}","sf2.actors.register{id='ally'}"})
            Load(call,false);
        Load(definition+"};"+definition+"}",false);
        Console.WriteLine("PASS: "+checks+" production Lua actor definition/default/bound/strict-field/handle/duplicate/rollback/fingerprint checks. Native lifetime/combat is verified separately in Unity.");
    }
}
