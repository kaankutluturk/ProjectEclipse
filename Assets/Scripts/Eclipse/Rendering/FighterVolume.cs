using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eclipse.Rendering
{
    // Presentation geometry only. No colliders or authoritative rig changes.
    public sealed class FighterVolume : MonoBehaviour
    {
        Mesh mesh;
        MeshRenderer draw;
        MaterialPropertyBlock tint;
        static Material material;
        Vector3[] vertices;
        Vector3[] normals;
        int[] triangles;
        const int Sides = 20, Rings = 12;

        // Used by the acceptance capture to inspect the same geometry under brighter light.
        public static float ReviewExposure = 1f;
        sealed class Weight
        {
            public readonly Dictionary<int,float> Values = new Dictionary<int,float>();
            public Vector3 Sample(Vector3[] source) { Vector3 result=Vector3.zero; foreach(var value in Values)result+=source[value.Key]*value.Value;return result; }
        }
        sealed class Panel
        {
            public string Name;
            public bool Rigid;
            public readonly List<Weight> Points = new List<Weight>();
            public readonly List<int> Faces = new List<int>();
            public readonly List<int[]> Boundary = new List<int[]>();
            public Vector3[] Pose;
            public Vector2[] UV;
            public int Offset, WallOffset;
        }
        readonly List<Panel> panels = new List<Panel>();
        int[] sourceFaces;
        bool sourceBody;
        int sourceCount;

        static Vector3 SurfacePoint(Vector3 point, float depth = .7f)
        { point.z *= depth; return point; }

        public static FighterVolume Create(Transform parent)
        {
            var obj = new GameObject("Experimental 3D volume");
            obj.layer = ExperimentalFighterCamera.Layer;
            obj.transform.SetParent(parent, false);
            var result = obj.AddComponent<FighterVolume>();
            result.mesh = new Mesh { name = "Eclipse dynamic fighter volume" };
            result.mesh.MarkDynamic();
            obj.AddComponent<MeshFilter>().sharedMesh = result.mesh;
            result.draw = obj.AddComponent<MeshRenderer>();
            if (material == null)
            {
                var shader = Resources.Load<Shader>("shaders/EclipseFighterVolume");
                if (shader == null) { Destroy(obj); return null; }
                material = new Material(shader);
            }
            result.draw.sharedMaterial = material;
            result.draw.shadowCastingMode = ShadowCastingMode.Off;
            result.draw.receiveShadows = false;
            result.draw.lightProbeUsage = LightProbeUsage.Off;
            result.draw.reflectionProbeUsage = ReflectionProbeUsage.Off;
            result.tint = new MaterialPropertyBlock();
            return result;
        }

        void Colorize(Color color)
        {
            // Preserve perk/arena colour, with a neutral near-black diffuse floor.
            Color ink = new Color(.035f,.033f,.03f,color.a);
            color = new Color(ink.r+color.r*.08f, ink.g+color.g*.08f, ink.b+color.b*.08f, color.a);
            tint.SetColor("_Color", color);
            Color rim = RimLight.SceneColor ?? new Color(.85f,.75f,.58f,1);
            float peak=Mathf.Max(.001f,Mathf.Max(rim.r,Mathf.Max(rim.g,rim.b)));
            tint.SetColor("_EdgeColor", new Color(rim.r/peak,rim.g/peak,rim.b/peak,1));
            tint.SetFloat("_Exposure",ReviewExposure);
            draw.SetPropertyBlock(tint);
        }

        public void UpdateTint(Color color) => Colorize(color);
        public void Geometry(List<Vector3> points, List<int> indices, Color color, List<Vector3> surfaceNormals = null, bool topologyChanged = true)
        {
            if(topologyChanged){mesh.Clear();mesh.indexFormat=points.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16;}
            mesh.SetVertices(points);if(topologyChanged)mesh.SetTriangles(indices,0);
            if(surfaceNormals!=null)mesh.SetNormals(surfaceNormals);else mesh.RecalculateNormals();
            mesh.RecalculateBounds();Colorize(color);
        }

        static long Edge(int a,int b) => ((long)Math.Min(a,b)<<32)|(uint)Math.Max(a,b);
        static string Group(string name)
        {
            int at=(name??string.Empty).IndexOf("Triangle",StringComparison.OrdinalIgnoreCase);
            return at<0 ? "surface" : name.Substring(0,at).TrimEnd('-','_');
        }
        void BuildPanels(Vector3[] source,int[] faces,string[] names,bool body)
        {
            panels.Clear();sourceFaces=faces;sourceCount=source.Length;sourceBody=body;
            var groups=new Dictionary<string,Panel>();var maps=new Dictionary<Panel,Dictionary<int,int>>();
            for(int i=0;i<faces.Length;i+=3)
            {
                string name=names!=null&&i/3<names.Length ? names[i/3] : "surface";
                if(body&&(name.StartsWith("BODY-",StringComparison.OrdinalIgnoreCase)||name.StartsWith("BODY_WOMAN-",StringComparison.OrdinalIgnoreCase)||name.StartsWith("HEAD-",StringComparison.OrdinalIgnoreCase)))continue;
                string group=Group(name);
                if(!groups.TryGetValue(group,out var panel))
                {
                    string upper=group.ToUpperInvariant();
                    panel=new Panel{Name=group,Rigid=upper.Contains("WEAPON")||upper.Contains("RANGED")||upper.Contains("BLADE")||upper.Contains("SWORD")||upper.Contains("KATANA")||upper.Contains("HELM")};
                    groups.Add(group,panel);maps.Add(panel,new Dictionary<int,int>());panels.Add(panel);
                }
                // A two-sided recovered drawing can contain opposite face winding.
                // Give each connected shell consistent winding before averaging normals.
                bool flip=Vector3.Cross(source[faces[i+1]]-source[faces[i]],source[faces[i+2]]-source[faces[i]]).z<0;
                for(int c=0;c<3;c++)
                {
                    int original=faces[i+(flip&&c>0?3-c:c)];
                    if(!maps[panel].TryGetValue(original,out int index))
                    {
                        index=panel.Points.Count;var weight=new Weight();weight.Values.Add(original,1);panel.Points.Add(weight);maps[panel].Add(original,index);
                    }
                    panel.Faces.Add(index);
                }
            }
            foreach(var panel in panels)
            {
                // Shared edge subdivision creates curved panels with shared face normals.
                // Never merge independently animated source vertices by their positions.
                for(int pass=0;pass<(panel.Rigid?0:2);pass++)
                {
                    var mids=new Dictionary<long,int>();var subdivided=new List<int>();
                    int Mid(int a,int b)
                    {
                        long key=Edge(a,b);if(mids.TryGetValue(key,out int found))return found;
                        var weight=new Weight();
                        foreach(int index in new[]{a,b})foreach(var item in panel.Points[index].Values)
                        {weight.Values.TryGetValue(item.Key,out float previous);weight.Values[item.Key]=previous+item.Value*.5f;}
                        int value=panel.Points.Count;panel.Points.Add(weight);mids.Add(key,value);return value;
                    }
                    for(int i=0;i<panel.Faces.Count;i+=3)
                    {
                        int a=panel.Faces[i],b=panel.Faces[i+1],c=panel.Faces[i+2],ab=Mid(a,b),bc=Mid(b,c),ca=Mid(c,a);
                        subdivided.AddRange(new[]{a,ab,ca,ab,b,bc,ca,bc,c,ab,bc,ca});
                    }
                    panel.Faces.Clear();panel.Faces.AddRange(subdivided);
                }
                var counts=new Dictionary<long,int>();var edges=new Dictionary<long,int[]>();
                for(int i=0;i<panel.Faces.Count;i+=3)for(int side=0;side<3;side++)
                {
                    int a=panel.Faces[i+side],b=panel.Faces[i+(side+1)%3];long key=Edge(a,b);
                    counts.TryGetValue(key,out int n);counts[key]=n+1;edges[key]=new[]{a,b};
                }
                foreach(var pair in counts)if(pair.Value==1)panel.Boundary.Add(edges[pair.Key]);
                panel.Pose=new Vector3[panel.Points.Count];panel.UV=new Vector2[panel.Points.Count];
                var bounds=new Bounds(panel.Points[0].Sample(source),Vector3.zero);
                foreach(var point in panel.Points)bounds.Encapsulate(point.Sample(source));
                for(int i=0;i<panel.Points.Count;i++)
                {
                    Vector3 point=panel.Points[i].Sample(source);
                    panel.UV[i]=new Vector2((point.x-bounds.min.x)/Mathf.Max(1,bounds.size.x),(point.y-bounds.min.y)/Mathf.Max(1,bounds.size.y));
                }
            }
            var topology=new List<int>();int vertexCount=0;
            foreach(var panel in panels)
            {
                panel.Offset=vertexCount;int count=panel.Points.Count;vertexCount+=count*2;panel.WallOffset=vertexCount;
                if(panel.Rigid)vertexCount+=panel.Boundary.Count*4;
                foreach(int index in panel.Faces)topology.Add(panel.Offset+index);
                for(int i=0;i<panel.Faces.Count;i+=3){topology.Add(panel.Offset+count+panel.Faces[i+2]);topology.Add(panel.Offset+count+panel.Faces[i+1]);topology.Add(panel.Offset+count+panel.Faces[i]);}
                for(int i=0;i<panel.Boundary.Count;i++)
                {
                    var edge=panel.Boundary[i];int a=panel.Offset+edge[0],b=panel.Offset+edge[1];
                    if(panel.Rigid){int wall=panel.WallOffset+i*4;topology.AddRange(new[]{wall,wall+1,wall+2,wall+1,wall+3,wall+2});}
                    else topology.AddRange(new[]{a,b,a+count,b,b+count,a+count});
                }
            }
            vertices=new Vector3[vertexCount];triangles=topology.ToArray();mesh.Clear();
        }
        static float DistanceToEdge(Vector3 point,Vector3 a,Vector3 b)
        {
            Vector2 p=new Vector2(point.x,point.y),x=new Vector2(a.x,a.y),y=new Vector2(b.x,b.y),delta=y-x;
            float t=delta.sqrMagnitude<.00001f?0:Mathf.Clamp01(Vector2.Dot(p-x,delta)/delta.sqrMagnitude);
            return Vector2.Distance(p,x+delta*t);
        }
        public void Surface(Vector3[] source,int[] faces,Color color,string[] names=null,bool body=false,float depthAnchor=0)
        {
            if(sourceFaces!=faces||sourceCount!=source.Length||sourceBody!=body)BuildPanels(source,faces,names,body);
            foreach(var panel in panels)
            {
                int count=panel.Points.Count;
                for(int i=0;i<count;i++)
                {
                    Vector3 point=panel.Points[i].Sample(source);
                    point.z=depthAnchor+(point.z-depthAnchor)*(panel.Rigid?.95f:.75f);
                    panel.Pose[i]=point;
                }
                for(int i=0;i<count;i++)
                {
                    Vector3 point=panel.Pose[i];float distance=100;
                    foreach(var edge in panel.Boundary)distance=Mathf.Min(distance,DistanceToEdge(point,panel.Pose[edge[0]],panel.Pose[edge[1]]));
                    // Thin rolled perimeter, fuller curved interior. Broad folds deform
                    // with the same native anchors, rather than triangular plate walls.
                    float fullness=1-Mathf.Exp(-distance/9);
                    float depth=panel.Rigid?2.5f:1.2f+5.5f*fullness;
                    Vector2 uv=panel.UV[i];
                    float fold=panel.Rigid?0:Mathf.Sin(uv.x*Mathf.PI*3+uv.y*.7f)*1.2f*fullness;
                    vertices[panel.Offset+i]=point+Vector3.forward*(depth+fold);
                    vertices[panel.Offset+count+i]=point-Vector3.forward*(depth-fold);
                }
                if(panel.Rigid)for(int i=0;i<panel.Boundary.Count;i++)
                {
                    var edge=panel.Boundary[i];int wall=panel.WallOffset+i*4;
                    vertices[wall]=vertices[panel.Offset+edge[0]];vertices[wall+1]=vertices[panel.Offset+edge[1]];
                    vertices[wall+2]=vertices[panel.Offset+count+edge[0]];vertices[wall+3]=vertices[panel.Offset+count+edge[1]];
                }
            }
            mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();Colorize(color);
        }

        public void Capsule(Vector3 start, Vector3 end, float diameter, Color color)
        {
            start = SurfacePoint(start); end = SurfacePoint(end);
            if (vertices == null || vertices.Length != Sides * Rings)
            {
                vertices = new Vector3[Sides * Rings]; normals = new Vector3[vertices.Length];
                var indices = new List<int>();
                for (int r = 0; r < Rings - 1; r++)
                    for (int s = 0; s < Sides; s++)
                    {
                        int a = r * Sides + s, b = r * Sides + (s + 1) % Sides;
                        indices.Add(a); indices.Add(a + Sides); indices.Add(b);
                        indices.Add(b); indices.Add(a + Sides); indices.Add(b + Sides);
                    }
                triangles = indices.ToArray(); mesh.Clear();
            }
            Vector3 axis = end - start;
            float length = axis.magnitude, radius = Mathf.Max(.01f, diameter * .5f);
            var rotation = length > .0001f ? Quaternion.FromToRotation(Vector3.up, axis / length) : Quaternion.identity;
            for (int r = 0; r < Rings; r++)
            {
                bool upper = r >= Rings / 2;
                float latitude = upper ? (r - Rings / 2) * Mathf.PI / (Rings - 2) : -Mathf.PI * .5f + r * Mathf.PI / (Rings - 2);
                float y = Mathf.Sin(latitude), ring = Mathf.Cos(latitude);
                for (int s = 0; s < Sides; s++)
                {
                    float angle = s * Mathf.PI * 2 / Sides;
                    Vector3 normal = new Vector3(ring * Mathf.Cos(angle), y, ring * Mathf.Sin(angle));
                    int i = r * Sides + s;
                    normals[i] = rotation * new Vector3(normal.x,normal.y/.55f,normal.z/.7f).normalized;
                    vertices[i] = start + rotation * (new Vector3(normal.x*radius,normal.y*radius*.55f,normal.z*radius*.7f) + (upper ? Vector3.up * length : Vector3.zero));
                }
            }
            mesh.vertices = vertices; mesh.normals = normals; mesh.triangles = triangles;
            mesh.RecalculateBounds(); Colorize(color);
        }

        void OnDestroy() { if (mesh != null) Destroy(mesh); }
    }
}
