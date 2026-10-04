using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Eclipse.Modding;

static class AuthoredGeometryTests
{
    static int checks;
    const string Body = "test.character:models/body.xml", Skin = "test.character:models/skin.xml";
    const string Rig = "<Scene><Nodes><A Type='Node' Mass='1'/><B Type='Node' Mass='1'/></Nodes><Edges><AB Type='Edge' End1='A' End2='B' Iterations='2'/></Edges><Figures><C Type='Capsule' Edge='ABCI1' Radius1='2' Radius2='3'/></Figures></Scene>";
    static XmlDocument Xml(string text) { var document = new XmlDocument(); document.LoadXml(text); return document; }
    static void Check(bool value, string why) { checks++; if (!value) throw new Exception(why); }
    static void Validate(string text) => ModCharacterGeometry.Validate(new[] { Body }, new[] { Xml(text) }, new HashSet<string> { Body });
    static void Reject(string text, string field)
    {
        try { Validate(text); throw new Exception("Accepted invalid geometry: " + field); }
        catch (InvalidDataException e) { Check(e.Message.Contains(Body) && e.Message.Contains(field), "Diagnostic lacks model/field: " + e.Message); }
    }
    public static void Main(string[] args)
    {
        Validate(Rig); Check(true, "Valid expanded capsule binding");
        const string skinned = "<Scene><Nodes><A Type='Node'/><B Type='Node'/><S Type='SkinnedNode' BonesCount='1' BoneStart1='A' BoneEnd1='B' Weight1='1' Along1='.5' Across1='.2'/></Nodes><Figures/></Scene>";
        Validate(skinned); Check(true, "Valid rotational skin attachment");
        foreach (var pair in new[] {
            ("BonesCount='1'", "BonesCount='17'", "@BonesCount"),
            ("BoneEnd1='B'", "BoneEnd1='missing'", "@BoneEnd1"),
            ("BoneEnd1='B'", "BoneEnd1='A'", "endpoints"),
            ("Weight1='1'", "Weight1='-1'", "@Weight1"),
            ("Weight1='1'", "Weight1='.2'", "sum to 1"),
            ("Along1='.5'", "Along1='101'", "@Along1"),
            ("Across1='.2'", "Across1='NaN'", "@Across1") }) Reject(skinned.Replace(pair.Item1, pair.Item2), pair.Item3);
        foreach (var pair in new[] {
            ("End2='B'", "End2='missing'", "@End2"),
            ("Mass='1'", "Mass='-1'", "@Mass"),
            ("Mass='1'", "Mass='NaN'", "@Mass"),
            ("Mass='1'", "Mass='100001'", "@Mass"),
            ("Iterations='2'", "Iterations='0'", "@Iterations"),
            ("Iterations='2'", "Iterations='33'", "@Iterations"),
            ("Iterations='2'", "Iterations='1.5'", "@Iterations"),
            ("Edge='ABCI1'", "Edge='ABCI2'", "@Edge"),
            ("Radius1='2'", "Radius1='-1'", "@Radius1"),
            ("Type='Capsule'", "Type='Unknown'", "@Type"),
            ("Type='Node'", "Type='Unknown'", "@Type"),
            ("Type='Edge'", "Type='Unknown'", "@Type") }) Reject(Rig.Replace(pair.Item1, pair.Item2), pair.Item3);
        Reject(Rig.Replace("<B Type='Node' Mass='1'/>", "<A Type='Node' Mass='1'/>"), "duplicate node");
        Reject(Rig.Replace("End2='B'", "End2='B' Length='-1'"), "@Length");
        Reject(Rig.Replace("</Edges>", "<ABCI1 Type='Edge' End1='A' End2='B'/></Edges>"), "duplicate expanded edge");
        Reject(Rig.Replace("</Figures>", "<C Type='Triangle' Node1='A' Node2='B' Node3='A'/></Figures>"), "duplicate figure");
        Reject("<Scene><Nodes><A Type='Node'/></Nodes><Figures><T Type='Triangle' Node1='A' Node2='A' Node3='missing'/></Figures></Scene>", "@Node3");
        Reject("<Scene><Nodes><M Type='MacroNode' NodesCount='1' ChildNode1='A' LCC1='1'/><A Type='Node'/></Nodes><Figures/></Scene>", "@ChildNode1");
        Reject("<Scene><Nodes><A Type='Node'/><M Type='MacroNode' NodesCount='1' ChildNode1='A'/></Nodes><Figures/></Scene>", "@LCC1");
        Reject("<Scene><Nodes><A Type='Node'/><M Type='CenterOfMass' NodesCount='1' ChildNode1='A'/></Nodes><Figures/></Scene>", "positive total mass");
        Reject("<Scene><Nodes><A Type='MacroNode'/></Nodes><Figures/></Scene>", "@NodesCount");
        Reject("<Scene/>", "/Scene");
        Reject("<Other><Figures/></Other>", "/Scene");
        Reject("<Scene><Figures/></Scene>", "at least one node");
        var large = new System.Text.StringBuilder("<Scene><Nodes>");
        for (int i = 0; i < 4097; i++) large.Append("<N" + i + " Type='Node'/>");
        large.Append("</Nodes><Figures/></Scene>"); Reject(large.ToString(), "4096 nodes");
        large = new System.Text.StringBuilder("<Scene><Nodes><A Type='Node'/></Nodes><Edges>");
        for (int i = 0; i < 257; i++) large.Append("<E" + i + " Type='Edge' End1='A' End2='A' Iterations='32'/>");
        large.Append("</Edges><Figures/></Scene>"); Reject(large.ToString(), "8192 expanded edges");
        Validate(Rig.Replace("</Nodes>", "<M Type='CenterOfMass' NodesCount='2' ChildNode1='A' ChildNode2='B'/></Nodes>"));
        var legacy = Xml("<Scene><Nodes><A Type='Node' Mass='1'/><A Type='Node' Mass='0'/><Ghost Type='Unknown'/></Nodes><Edges><Unused Type='Unknown'/></Edges><Figures/></Scene>");
        var overlay = Xml("<Scene><Nodes><M Type='MacroNode' NodesCount='1' ChildNode1='A' LCC1='1'/></Nodes><Figures><T Type='Triangle' Node1='A' Node2='M' Node3='M'/></Figures></Scene>");
        ModCharacterGeometry.Validate(new[] { "core:models/legacy.xml", Skin }, new[] { legacy, overlay }, new HashSet<string> { Skin });
        Check(true, "Legacy first-wins bindings compose with authored overlays");
        overlay.LoadXml(overlay.OuterXml.Replace("ChildNode1=\"A\"", "ChildNode1=\"Ghost\""));
        try { ModCharacterGeometry.Validate(new[] { "legacy", Skin }, new[] { legacy, overlay }, new HashSet<string> { Skin }); throw new Exception("Phantom node accepted"); }
        catch (InvalidDataException e) { Check(e.Message.Contains("@ChildNode1"), "Native-ignored nodes cannot satisfy bindings"); }
        ModelLoader.FHGHPCACAKJ.Documents[Body] = Xml(Rig.Replace("End2='B'", "End2='missing'"));
        var model = new ModelObject(); model.Model.Parameters.EclipseBodyModel = Body.Substring(0, Body.Length - 4);
        try { ModelLoader.Load(model, new List<string> { Body }); throw new Exception("Loader accepted invalid model"); }
        catch (InvalidDataException) { Check(ModelLoader.Parsed == 0, "Preflight precedes recovered parsing"); }
        ModelLoader.FHGHPCACAKJ.Documents[Body] = Xml(Rig); ModelLoader.Load(model, new List<string> { Body });
        Check(ModelLoader.Parsed == 1, "Validated model reaches native parser");
        var archive = new ModelObject(); archive.Model.Parameters.EclipseBodyModel = "core:models/legacy";
        ModelLoader.FHGHPCACAKJ.Documents["core:models/legacy.xml"] = Xml("<Scene><Figures/></Scene>");
        ModelLoader.Load(archive, new List<string> { "core:models/legacy.xml" });
        Check(ModelLoader.Parsed == 2, "Core handles keep archival parsing");
        ModelLoader.FHGHPCACAKJ.Documents["core:models/legacy.xml"] = legacy;
        ModelLoader.FHGHPCACAKJ.Documents[Skin] = Xml("<Scene><Nodes><M Type='MacroNode' NodesCount='1' ChildNode1='A' LCC1='1'/></Nodes><Figures/></Scene>");
        archive.Model.Parameters.EclipseSkinModels = new[] { Skin };
        ModelLoader.Load(archive, new List<string> { "core:models/legacy.xml", Skin });
        Check(ModelLoader.Parsed == 4, "Authored skin validates against archival body before native parsing");
        ModelLoader.FHGHPCACAKJ.Documents[Skin] = Xml("<Scene><Nodes><M Type='MacroNode' NodesCount='1' ChildNode1='missing' LCC1='1'/></Nodes><Figures/></Scene>");
        try { ModelLoader.Load(archive, new List<string> { "core:models/legacy.xml", Skin }); throw new Exception("Invalid overlay accepted"); }
        catch (InvalidDataException) { Check(ModelLoader.Parsed == 4, "Invalid later skin rejects before earlier archival body is parsed"); }
        Console.WriteLine("PASS: authored geometry " + checks + " checks; production validator and loader integration with controlled native parser.");
    }
}
