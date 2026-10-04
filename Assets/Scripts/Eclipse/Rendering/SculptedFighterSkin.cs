using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eclipse.Rendering
{
    // Mesh construction must not weld a guarding hand to the chest, or touching
    // legs together. Keep native lengths but separate limbs before skin binding.
    internal static class FighterReferencePose
    {
        internal static Dictionary<string, Vector3> Create(IReadOnlyDictionary<string, Vector3> live)
        {
            var pose = new Dictionary<string, Vector3>();
            float Length(string a, string b) => Mathf.Max(.01f, Vector3.Distance(live[a], live[b]));
            Vector3 root = live["NPivot"];
            pose["NPivot"] = root;
            pose["NStomach"] = root + Vector3.down * Length("NPivot", "NStomach");
            pose["NChest"] = pose["NStomach"] + Vector3.down * Length("NStomach", "NChest");
            pose["NNeck"] = pose["NChest"] + Vector3.down * Length("NChest", "NNeck");
            pose["NHead"] = pose["NNeck"] + Vector3.down * Length("NNeck", "NHead");
            // Keep head landmarks on their own frame, rather than treating the
            // skull as an oval at a single rig point.
            if(live.ContainsKey("NTop"))pose["NTop"]=pose["NHead"]+Vector3.down*Length("NHead","NTop");
            if(live.ContainsKey("NHeadF"))pose["NHeadF"]=pose["NHead"]+Vector3.forward*Length("NHead","NHeadF");
            float shoulders = Mathf.Clamp(Length("NShoulder_1", "NShoulder_2") * .5f, 14, 28);
            float hips = Mathf.Clamp(Length("NHip_1", "NHip_2") * .5f, 12, 22);
            for (int side = 1; side <= 2; side++)
            {
                string s = "_" + side;
                float sign = side == 1 ? 1 : -1;
                var arm = new Vector3(sign * .7f, .7f, 0).normalized;
                var forearm = new Vector3(sign * .18f, .98f, 0).normalized;
                var leg = new Vector3(sign * .12f, 1, 0).normalized;
                pose["NShoulder" + s] = pose["NChest"] + Vector3.right * (sign * shoulders);
                pose["NElbow" + s] = pose["NShoulder" + s] + arm * Length("NShoulder" + s, "NElbow" + s);
                pose["NWrist" + s] = pose["NElbow" + s] + forearm * Length("NElbow" + s, "NWrist" + s);
                pose["NFingertips" + s] = pose["NWrist" + s] + forearm * Length("NWrist" + s, "NFingertips" + s);
                if(live.ContainsKey("NKnuckles"+s))
                    pose["NKnuckles"+s]=pose["NWrist"+s]+forearm*Length("NWrist"+s,"NKnuckles"+s);
                pose["NHip" + s] = root + Vector3.right * (sign * hips);
                pose["NKnee" + s] = pose["NHip" + s] + leg * Length("NHip" + s, "NKnee" + s);
                pose["NAnkle" + s] = pose["NKnee" + s] + leg * Length("NKnee" + s, "NAnkle" + s);
                pose["NHeel" + s] = pose["NAnkle" + s] + Vector3.up * Length("NAnkle" + s, "NHeel" + s);
                pose["NToeTip" + s] = pose["NHeel" + s] + Vector3.right * (sign * Length("NHeel" + s, "NToeTip" + s));
            }
            return pose;
        }
    }

    // A single blended surface follows the native pose. The field is entirely
    // presentation data: it never writes bones, physics, collisions or animation.
    public sealed class SculptedFighterSkin
    {
        struct BodySection
        {
            public Vector3 Start, End, Axis, Across, Deep;
            public float Length, WidthA, WidthB, DepthA, DepthB, Cap;
            public Bounds Bounds;
            public float Distance(Vector3 point)
            {
                float along=Vector3.Dot(point-Start,Axis);
                float t=Length>.001f?Mathf.Clamp01(along/Length):0;
                Vector3 delta=point-Vector3.Lerp(Start,End,t);
                float width=Mathf.Lerp(WidthA,WidthB,t),depth=Mathf.Lerp(DepthA,DepthB,t);
                float x=Vector3.Dot(delta,Across)/width,z=Vector3.Dot(delta,Deep)/depth,y=Vector3.Dot(delta,Axis)/Cap;
                float radius=Mathf.Sqrt(x*x+y*y+z*z);
                float gradient=Mathf.Sqrt(x*x/(width*width)+y*y/(Cap*Cap)+z*z/(depth*depth));
                return gradient>.00001f?radius*(radius-1)/gradient:-Mathf.Min(width,depth);
            }
        }
        struct Influence
        {
            public int Section;
            public float Weight, Along, Fraction, Across, Deep;
            public Vector3 Normal;
        }
        Influence[] binding;
        int boundSections;
        internal bool HasBinding => binding != null;
        public double LastDeformMilliseconds { get; private set; }
        public bool TopologyChanged { get; private set; }
        public void Reset() { binding=null; }
        readonly List<BodySection> sections=new List<BodySection>(40);
        readonly Dictionary<long,int> crossings=new Dictionary<long,int>(16000);
        readonly int[] cube=new int[8],inside=new int[4],outside=new int[4];
        static readonly int[,] Tetrahedra={{0,1,3,7},{0,3,2,7},{0,2,6,7},{0,6,4,7},{0,4,5,7},{0,5,1,7}};
        float[] field;
        Vector3 origin, anchor;
        public readonly List<Vector3> Normals = new List<Vector3>(16000);
        int nx,ny,nz;
        float cell;
        int[] skinFaces;
        Vector3[] normalSums;
        List<int>[] skinNeighbors;
        const float Blend=5f;
        public double LastBuildMilliseconds { get; private set; }
        public void Clear()=>sections.Clear();
        public void Section(Vector3 start,Vector3 end,float widthA,float widthB,float depthA,float depthB,float cap=6,Vector3? depthAxis=null)
        {
            Vector3 axis=end-start;float length=axis.magnitude;
            axis=length>.001f?axis/length:Vector3.up;
            Vector3 across=Vector3.Cross(axis,depthAxis??Vector3.forward).normalized;
            if(across.sqrMagnitude<.01f)across=Vector3.Cross(axis,Vector3.up).normalized;
            if(across.sqrMagnitude<.01f)across=Vector3.right;
            Vector3 deep=Vector3.Cross(across,axis).normalized;
            float width=Mathf.Max(widthA,widthB),depth=Mathf.Max(depthA,depthB);
            Vector3 radius=new Vector3(
                Mathf.Abs(across.x)*width+Mathf.Abs(deep.x)*depth+Mathf.Abs(axis.x)*cap,
                Mathf.Abs(across.y)*width+Mathf.Abs(deep.y)*depth+Mathf.Abs(axis.y)*cap,
                Mathf.Abs(across.z)*width+Mathf.Abs(deep.z)*depth+Mathf.Abs(axis.z)*cap);
            var bounds=new Bounds((start+end)*.5f,Vector3.zero);bounds.Encapsulate(start-radius);bounds.Encapsulate(start+radius);
            bounds.Encapsulate(end-radius);bounds.Encapsulate(end+radius);
            bounds.Expand(Blend*2);
            sections.Add(new BodySection{Start=start,End=end,Axis=axis,Across=across,Deep=deep,Length=length,
                WidthA=widthA,WidthB=widthB,DepthA=depthA,DepthB=depthB,Cap=cap,Bounds=bounds});
        }
        public void Ellipsoid(Vector3 center,Vector3 axis,float width,float height,float depth)
        {Section(center-axis.normalized*.01f,center+axis.normalized*.01f,width,width,depth,depth,height);}
        Vector3 Point(int index)
        {int x=index%nx,y=(index/nx)%ny,z=index/(nx*ny);return origin-anchor+new Vector3(x*cell,y*cell,z*cell);}
        int Crossing(int a,int b,List<Vector3> vertices)
        {
            float t=field[a]/(field[a]-field[b]);
            int exact=t<.0001f?a:t>.9999f?b:-1;
            long key=exact>=0?((long)exact<<32)|(uint)exact:((long)Math.Min(a,b)<<32)|(uint)Math.Max(a,b);
            if(crossings.TryGetValue(key,out int found))return found;
            t=exact==a?0:exact==b?1:t;int value=vertices.Count;
            vertices.Add(Vector3.LerpUnclamped(Point(a),Point(b),t));
            Normals.Add(Vector3.LerpUnclamped(Gradient(a),Gradient(b),t).normalized);
            crossings.Add(key,value);return value;
        }
        Vector3 Gradient(int i)
        {
            int x=i%nx,y=(i/nx)%ny,z=i/(nx*ny);
            return new Vector3(field[x<nx-1?i+1:i]-field[x>0?i-1:i],
                field[y<ny-1?i+nx:i]-field[y>0?i-nx:i],
                field[z<nz-1?i+nx*ny:i]-field[z>0?i-nx*ny:i]);
        }
        void Face(int a,int b,int c,Vector3 outward,List<Vector3> vertices,List<int> faces)
        {
            if(a==b||b==c||c==a)return;
            Vector3 normal=Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]);
            if(normal.sqrMagnitude==0)return;
            if(Vector3.Dot(normal,outward)<0){int swap=b;b=c;c=swap;}
            faces.Add(a);faces.Add(b);faces.Add(c);
        }
        public void Build(Vector3 anchor,List<Vector3> vertices,List<int> faces)
        {
            if(binding!=null && boundSections==sections.Count)
            { TopologyChanged=false;Deform(anchor,vertices); return; }
            TopologyChanged=true;
            var timer=System.Diagnostics.Stopwatch.StartNew();
            this.anchor=anchor;
            vertices.Clear();faces.Clear();crossings.Clear();Normals.Clear();
            if(sections.Count==0)return;
            Bounds bounds=sections[0].Bounds;foreach(var section in sections)bounds.Encapsulate(section.Bounds);
            cell=3.5f;
            do
            {
                Vector3 min=bounds.min-anchor;
                origin=anchor+new Vector3(Mathf.Floor(min.x/cell)*cell,Mathf.Floor(min.y/cell)*cell,Mathf.Floor(min.z/cell)*cell);
                Vector3 size=bounds.max-origin;
                nx=Mathf.CeilToInt(size.x/cell)+2;ny=Mathf.CeilToInt(size.y/cell)+2;nz=Mathf.CeilToInt(size.z/cell)+2;
                if((long)nx*ny*nz<=180000)break;
                cell*=1.25f;
            }while(true);
            int count=nx*ny*nz;if(field==null||field.Length<count)field=new float[count];
            for(int i=0;i<count;i++)field[i]=40;
            // Stamp local section bounds, rather than evaluating every body
            // region at every point in the enclosing animated-pose box.
            foreach(var section in sections)
            {
                Vector3 min=(section.Bounds.min-origin)/cell,max=(section.Bounds.max-origin)/cell;
                int x0=Mathf.Max(0,Mathf.FloorToInt(min.x)),x1=Mathf.Min(nx-1,Mathf.CeilToInt(max.x));
                int y0=Mathf.Max(0,Mathf.FloorToInt(min.y)),y1=Mathf.Min(ny-1,Mathf.CeilToInt(max.y));
                int z0=Mathf.Max(0,Mathf.FloorToInt(min.z)),z1=Mathf.Min(nz-1,Mathf.CeilToInt(max.z));
                for(int z=z0;z<=z1;z++)for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
                {
                    int i=x+nx*(y+ny*z);
                    float distance=section.Distance(origin+new Vector3(x*cell,y*cell,z*cell)),previous=field[i];
                    float h=Mathf.Clamp01(.5f+.5f*(distance-previous)/Blend);
                    field[i]=Mathf.Lerp(distance,previous,h)-Blend*h*(1-h);
                }
            }
            for(int z=0;z<nz-1;z++)for(int y=0;y<ny-1;y++)for(int x=0;x<nx-1;x++)
            {
                int baseIndex=x+nx*(y+ny*z),negative=0;
                for(int c=0;c<8;c++){cube[c]=baseIndex+(c&1)+nx*((c>>1)&1)+nx*ny*((c>>2)&1);if(field[cube[c]]<0)negative++;}
                if(negative==0||negative==8)continue;
                for(int tetra=0;tetra<6;tetra++)
                {
                    int ni=0,no=0;Vector3 inward=Vector3.zero,outward=Vector3.zero;
                    for(int c=0;c<4;c++){int index=cube[Tetrahedra[tetra,c]];if(field[index]<0){inside[ni++]=index;inward+=Point(index);}else{outside[no++]=index;outward+=Point(index);}}
                    if(ni==0||no==0)continue;
                    Vector3 direction=outward/no-inward/ni;
                    if(ni==1||no==1)
                    {
                        int solo=ni==1?inside[0]:outside[0];var other=ni==1?outside:inside;
                        Face(Crossing(solo,other[0],vertices),Crossing(solo,other[1],vertices),Crossing(solo,other[2],vertices),direction,vertices,faces);
                    }
                    else
                    {
                        int a=Crossing(inside[0],outside[0],vertices),b=Crossing(inside[0],outside[1],vertices);
                        int c=Crossing(inside[1],outside[0],vertices),d=Crossing(inside[1],outside[1],vertices);
                        Face(a,b,c,direction,vertices,faces);Face(b,d,c,direction,vertices,faces);
                    }
                }
            }
            if(Connected(vertices.Count,faces))
            {
                skinNeighbors=SmoothSeed(vertices,faces);
                skinFaces=faces.ToArray();normalSums=new Vector3[vertices.Count];
                RefreshNormals(vertices);
                Bind(vertices);
            }
            timer.Stop();LastBuildMilliseconds=timer.Elapsed.TotalMilliseconds;
        }
        static bool Connected(int count,List<int> faces)
        {
            var parent=new int[count];for(int i=0;i<count;i++)parent[i]=i;
            int Root(int v){while(parent[v]!=v){parent[v]=parent[parent[v]];v=parent[v];}return v;}
            for(int i=0;i<faces.Count;i+=3)
            {int root=Root(faces[i]);parent[Root(faces[i+1])]=root;parent[Root(faces[i+2])]=root;}
            int component=Root(faces[0]);foreach(int vertex in faces)if(Root(vertex)!=component)return false;
            return true;
        }
        static List<int>[] SmoothSeed(List<Vector3> vertices,List<int> faces)
        {
            // Remove extraction-grid ridges before binding. Alternating steps
            // soften the surface without the shrinkage of repeated averaging.
            var neighbors=new List<int>[vertices.Count];
            for(int i=0;i<neighbors.Length;i++)neighbors[i]=new List<int>(12);
            for(int i=0;i<faces.Count;i+=3)for(int corner=0;corner<3;corner++)
            {
                int a=faces[i+corner],b=faces[i+(corner+1)%3];
                if(!neighbors[a].Contains(b))neighbors[a].Add(b);
                if(!neighbors[b].Contains(a))neighbors[b].Add(a);
            }
            var next=new Vector3[vertices.Count];
            for(int pass=0;pass<4;pass++)
            {
                float amount=pass%2==0?.42f:-.44f;
                for(int i=0;i<vertices.Count;i++)
                {
                    Vector3 mean=Vector3.zero;foreach(int n in neighbors[i])mean+=vertices[n];
                    next[i]=neighbors[i].Count==0?vertices[i]:vertices[i]+amount*(mean/neighbors[i].Count-vertices[i]);
                }
                for(int i=0;i<vertices.Count;i++)vertices[i]=next[i];
            }
            return neighbors;
        }
        void RefreshNormals(List<Vector3> vertices)
        {
            Array.Clear(normalSums,0,normalSums.Length);
            for(int i=0;i<skinFaces.Length;i+=3)
            {
                int a=skinFaces[i],b=skinFaces[i+1],c=skinFaces[i+2];
                Vector3 normal=Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]);
                normalSums[a]+=normal;normalSums[b]+=normal;normalSums[c]+=normal;
            }
            for(int i=0;i<vertices.Count;i++)
                if(normalSums[i].sqrMagnitude>1e-12f)Normals[i]=normalSums[i].normalized;
            // Keep broad sculpted lighting across the extraction triangles,
            // while preserving pronounced changes such as the jaw and nose.
            for(int i=0;i<vertices.Count;i++)
            {
                Vector3 sum=Normals[i];int count=1;
                foreach(int neighbor in skinNeighbors[i])
                    if(Vector3.Dot(Normals[i],Normals[neighbor])>.5f){sum+=Normals[neighbor];count++;}
                normalSums[i]=Vector3.Lerp(Normals[i],sum/count,.65f).normalized;
            }
            for(int i=0;i<vertices.Count;i++)Normals[i]=normalSums[i];
        }
        void Bind(List<Vector3> vertices)
        {
            // Build topology once per native rig, then skin its connected surface.
            // Nearby sections blend at joints; no field rebuild per display frame.
            boundSections=sections.Count;binding=new Influence[vertices.Count*3];
            bindLengths=new float[sections.Count];for(int i=0;i<sections.Count;i++)bindLengths[i]=sections[i].Length;
            var nearest=new int[3];var distances=new float[3];
            for(int v=0;v<vertices.Count;v++)
            {
                Vector3 point=vertices[v]+anchor;
                for(int k=0;k<3;k++){nearest[k]=-1;distances[k]=float.MaxValue;}
                for(int j=0;j<sections.Count;j++)
                {
                    float distance=sections[j].Distance(point);
                    for(int k=0;k<3;k++)if(distance<distances[k])
                    {for(int n=2;n>k;n--){nearest[n]=nearest[n-1];distances[n]=distances[n-1];}nearest[k]=j;distances[k]=distance;break;}
                }
                float sum=0;
                for(int k=0;k<3;k++)if(nearest[k]>=0)
                {float weight=Mathf.Max(0,8-(distances[k]-distances[0]));sum+=weight*weight;}
                for(int k=0;k<3;k++)if(nearest[k]>=0)
                {
                    var section=sections[nearest[k]];var delta=point-section.Start;
                    float weight=Mathf.Max(0,8-(distances[k]-distances[0]));
                    float along=Vector3.Dot(delta,section.Axis);
                    binding[v*3+k]=new Influence{Section=nearest[k],Weight=weight*weight/sum,Along=along,
                        Fraction=section.Length>.001f?Mathf.Clamp01(along/section.Length):0,
                        Across=Vector3.Dot(delta,section.Across),Deep=Vector3.Dot(delta,section.Deep),
                        Normal=new Vector3(Vector3.Dot(Normals[v],section.Across),Vector3.Dot(Normals[v],section.Axis),Vector3.Dot(Normals[v],section.Deep))};
                }
            }
        }
        void Deform(Vector3 currentAnchor,List<Vector3> vertices)
        {
            var timer=System.Diagnostics.Stopwatch.StartNew();
            for(int v=0;v<vertices.Count;v++)
            {
                Vector3 point=Vector3.zero,normal=Vector3.zero;
                for(int k=0;k<3;k++)
                {
                    var influence=binding[v*3+k];if(influence.Weight==0)continue;
                    var section=sections[influence.Section];
                    // Axial offsets outside a section remain rigid cap geometry.
                    // Only the point's interior fraction follows bone length changes.
                    float oldLength=bindLengths[influence.Section];
                    point+=(section.Start-currentAnchor+section.Axis*(influence.Along+(section.Length-oldLength)*influence.Fraction)
                        +section.Across*influence.Across+section.Deep*influence.Deep)*influence.Weight;
                    normal+=(section.Across*influence.Normal.x+section.Axis*influence.Normal.y+section.Deep*influence.Normal.z)*influence.Weight;
                }
                vertices[v]=point;Normals[v]=normal.normalized;
            }
            // Joint blending changes the surface, not just section orientation.
            // Light the actual deformed skin instead of a rotated rest normal.
            RefreshNormals(vertices);
            timer.Stop();LastDeformMilliseconds=timer.Elapsed.TotalMilliseconds;
        }
        float[] bindLengths;

    }
}
