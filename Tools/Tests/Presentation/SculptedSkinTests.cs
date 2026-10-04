using System;
using System.Collections.Generic;
using System.Linq;
using Eclipse.Rendering;
using UnityEngine;

static class SculptedSkinTests
{
    static int checks;
    static void Check(bool condition,string message){checks++;if(!condition)throw new Exception(message);}
    static void Closed(List<Vector3> vertices,List<int> faces,List<Vector3> normals)
    {
        Check(vertices.Count>100 && faces.Count>300,"Skin surface missing");
        Check(vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)),"Nonfinite skin positions");
        Check(normals.Count==vertices.Count && normals.All(n=>float.IsFinite(n.x)&&Math.Abs(n.sqrMagnitude-1)<.001f),"Invalid smooth lighting normals");
        var edges=new Dictionary<long,int>();
        bool collapsed=false;
        for(int i=0;i<faces.Count;i+=3)
        {
            collapsed |= Vector3.Cross(vertices[faces[i+1]]-vertices[faces[i]],vertices[faces[i+2]]-vertices[faces[i]]).sqrMagnitude==0;
            for(int j=0;j<3;j++)
            {int a=faces[i+j],b=faces[i+(j+1)%3];long key=((long)Math.Min(a,b)<<32)|(uint)Math.Max(a,b);edges.TryGetValue(key,out int n);edges[key]=n+1;}
        }
        Check(!collapsed,"Collapsed triangle");
        Check(edges.Values.All(n=>n==2),"Open seams or internal patch walls");
    }
    static void Main()
    {
        var skin=new SculptedFighterSkin();var vertices=new List<Vector3>();var faces=new List<int>();
        foreach(float direction in new[]{-1f,1f})
        foreach(var anchor in new[]{Vector3.zero,new Vector3(1100,1200,37)})
        {
            skin.Reset();skin.Clear();
            skin.Section(anchor+Vector3.up*80,anchor+Vector3.up*(80+60*direction),9,6,7,5,4);
            skin.Build(anchor,vertices,faces);Closed(vertices,faces,skin.Normals);
            float low=Math.Min(80,80+60*direction),high=Math.Max(80,80+60*direction);
            Check(vertices.Min(v=>v.y)<low-2 && vertices.Max(v=>v.y)>high+2,"Negative-direction section bounds clipped end caps");
            var original=vertices.ToArray();var topology=faces.ToArray();
            skin.Clear();var moved=anchor+new Vector3(30,-20,11);
            skin.Section(moved+Vector3.right*80,moved+Vector3.right*(80+60*direction),9,6,7,5,4);
            skin.Build(moved,vertices,faces);
            Check(!skin.TopologyChanged && faces.SequenceEqual(topology),"Animation rebuilt connected skin topology");
            Closed(vertices,faces,skin.Normals);
            for(int i=0;i<vertices.Count;i++)
                if(Vector3.Distance(vertices[i],new Vector3(original[i].y,-original[i].x,original[i].z))>.01f)
                    throw new Exception("Rigid rig rotation or anchor translation distorted skin");
            checks++;
        }
        skin.Reset();skin.Clear();
        skin.Section(new Vector3(0,0,0),new Vector3(0,60,0),15,12,10,9);
        skin.Section(new Vector3(0,45,0),new Vector3(40,55,0),10,6,8,5);
        skin.Section(new Vector3(40,55,0),new Vector3(70,30,0),6,4,5,3);
        skin.Ellipsoid(new Vector3(0,76,0),new Vector3(.3f,1,0),10,16,9);
        skin.Build(Vector3.zero,vertices,faces);Closed(vertices,faces,skin.Normals);
        Check(skin.TopologyChanged,"Rig reset failed to rebuild new anatomy");
        ReferencePose();
        Console.WriteLine("PASS: "+checks+" sculpted skin assertions: closed finite geometry, bidirectional cap bounds, large-coordinate anchors, smooth normals, rigid animation/topology reuse and separated length-preserving reference poses.");
    }
    static void ReferencePose()
    {
        var live = new Dictionary<string, Vector3> {
            {"NPivot",new Vector3(1000,2000,77)}, {"NStomach",new Vector3(1000,1970,77)},
            {"NChest",new Vector3(1000,1920,77)}, {"NNeck",new Vector3(1000,1890,77)},
            {"NHead",new Vector3(1000,1865,77)} };
        for(int side=1;side<=2;side++)
        {
            string s="_"+side;float sign=side==1?1:-1;
            live["NShoulder"+s]=new Vector3(1000+20*sign,1920,77);
            live["NElbow"+s]=new Vector3(1000+55*sign,1950,80);
            // A hand can be directly against the chest in a guarding pose.
            live["NWrist"+s]=new Vector3(1000+4*sign,1920,77);
            live["NFingertips"+s]=new Vector3(1000,1900,77);
            live["NHip"+s]=new Vector3(1000+16*sign,2000,77);
            live["NKnee"+s]=new Vector3(1000+20*sign,2075,77);
            live["NAnkle"+s]=new Vector3(1000+25*sign,2140,77);
            live["NHeel"+s]=new Vector3(1000+25*sign,2145,77);
            live["NToeTip"+s]=new Vector3(1000+50*sign,2145,77);
        }
        var before=live.ToDictionary(p=>p.Key,p=>p.Value);
        var reference=FighterReferencePose.Create(live);
        Check(live.All(p=>p.Value==before[p.Key]),"Reference construction mutated native pose input");
        Check(reference["NPivot"]==live["NPivot"],"Reference root anchor moved");
        Check(reference.Count==live.Count&&reference.Values.All(p=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z)),"Reference bindings incomplete or nonfinite");
        foreach(var pair in new[]{("NPivot","NStomach"),("NStomach","NChest"),("NChest","NNeck"),("NNeck","NHead")})
            Check(Math.Abs(Vector3.Distance(reference[pair.Item1],reference[pair.Item2])-Vector3.Distance(live[pair.Item1],live[pair.Item2]))<.001f,"Torso reference changed native section length");
        for(int side=1;side<=2;side++)
        {
            string s="_"+side;
            foreach(var pair in new[]{("NShoulder","NElbow"),("NElbow","NWrist"),("NWrist","NFingertips"),("NHip","NKnee"),("NKnee","NAnkle"),("NAnkle","NHeel"),("NHeel","NToeTip")})
                Check(Math.Abs(Vector3.Distance(reference[pair.Item1+s],reference[pair.Item2+s])-Vector3.Distance(live[pair.Item1+s],live[pair.Item2+s]))<.001f,"Limb reference changed native section length");
            Check(Math.Abs(reference["NWrist"+s].x-reference["NChest"].x)>50,"Guard hand remained against torso in reference pose");
        }
        Check(Vector3.Distance(reference["NKnee_1"],reference["NKnee_2"])>40,"Reference thighs remain in contact");
        Check(Vector3.Distance(reference["NToeTip_1"],reference["NToeTip_2"])>60,"Reference feet overlap");
    }
}
