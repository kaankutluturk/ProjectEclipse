using System;
using System.Collections.Generic;
using Eclipse.Rendering.Interpolation;
using UnityEngine;

namespace Eclipse.Rendering
{
    // Standard recovered Skeleton skin. Other rigs keep the connected-surface fallback.
    public sealed class ProceduralFighterBody : MonoBehaviour
    {
        ModelPresentation presentation;
        ModelObject source;
        readonly Dictionary<string, ModelNode> nodes = new Dictionary<string, ModelNode>();
        readonly Dictionary<string, Vector3> pose = new Dictionary<string, Vector3>();
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<int> faces = new List<int>();
        FighterVolume volume;
        int rendered = -1;
        public bool Ready { get; private set; }
        static readonly string[] Anchors = {
            "NNeck", "NHead", "NChest", "NStomach", "NPivot",
            "NShoulder_1", "NElbow_1", "NWrist_1", "NFingertips_1",
            "NShoulder_2", "NElbow_2", "NWrist_2", "NFingertips_2",
            "NHip_1", "NKnee_1", "NAnkle_1", "NHeel_1", "NToeTip_1",
            "NHip_2", "NKnee_2", "NAnkle_2", "NHeel_2", "NToeTip_2" };

        public static bool RenderFor(ModelPresentation owner, Color color)
        {
            if (owner == null || owner.SourceModel == null) return false;
            var body = owner.GetComponent<ProceduralFighterBody>();
            if (body == null) body = owner.gameObject.AddComponent<ProceduralFighterBody>();
            body.presentation = owner;
            return body.Refresh(color);
        }
        public static bool ActiveFor(ModelPresentation owner) => owner != null &&
            owner.GetComponent<ProceduralFighterBody>() is ProceduralFighterBody body && body.Ready;
        public static bool IsBodyOutline(string name)
        {
            // The recovered female body namespaces the same skeleton figures.
            if (name.StartsWith("BODY_WOMAN-", StringComparison.Ordinal)) name = name.Substring(11);
            string edge = name.StartsWith("Capsule_",StringComparison.Ordinal) ? name.Substring(8) :
                name.StartsWith("Capsule-1_",StringComparison.Ordinal) ? name.Substring(10) : null;
            if(edge==null)return false;
            foreach(string prefix in new[]{"EFoot","EToe","EHeel","EHeep","EInstep","ECalf","EThigh","EGroin","EPelvis","EStomach","EArm","EForearm","EHand","EFingers","EChest","EClavicle","ENeck","EHead","Muscle"})
                if(edge.StartsWith(prefix,StringComparison.Ordinal))return true;
            return edge.StartsWith("Edge",StringComparison.Ordinal)&&(edge.EndsWith("_1",StringComparison.Ordinal)||edge.EndsWith("_2",StringComparison.Ordinal));
        }

        bool Refresh(Color color)
        {
            var current = presentation.SourceModel.GetModelObject();
            if (current != source)
            {
                source = current; nodes.Clear(); Ready = source != null;
                if (source != null) foreach (string name in Anchors)
                {
                    var node = source.FindNodeOrParent(name);
                    if (node == null) { Ready = false; break; }
                    nodes.Add(name, node);
                }
            }
            if (volume != null) volume.gameObject.SetActive(Ready);
            if (!Ready) return false;
            if (volume == null) volume = FighterVolume.Create(transform);
            if (volume == null) return false;
            if (rendered == Time.frameCount) return true;
            rendered = Time.frameCount;
            float alpha = ModelPresentation.AlphaFor(presentation);
            foreach (var pair in nodes)
            {
                FightInterpolation.SamplePosition(pair.Value, alpha, out float x, out float y, out float z);
                pose[pair.Key] = new Vector3(x, y, z);
            }
            vertices.Clear(); faces.Clear();
            // Neck/chest/waist/pelvis have dedicated proportions rather than stroke widths.
            Vector3 neck = P("NNeck", .85f), chest = P("NChest", .85f), waist = P("NStomach", .85f), pelvis = P("NPivot", .85f);
            Loft(new[] { pelvis + (pelvis-waist).normalized*8, pelvis, waist, chest, neck },
                new[] { 9f, 20f, 15f, 21f, 7f }, new[] { 8f, 13f, 11f, 14f, 6f });
            Vector3 head = P("NHead", .9f), up = (head-neck).normalized;
            Loft(new[] { neck, head-up*9, head+up*5, head+up*14 },
                new[] { 6f, 10f, 12f, .1f }, new[] { 5f, 9f, 10f, .1f });
            for (int side = 1; side <= 2; side++)
            {
                string suffix = "_"+side;
                Vector3 shoulder=P("NShoulder"+suffix,.65f), elbow=P("NElbow"+suffix,.65f), wrist=P("NWrist"+suffix,.65f);
                // One continuous shoulder/elbow/wrist skin: no spherical elbow or intersecting forearm capsule.
                Loft(new[] { Vector3.Lerp(chest,shoulder,.55f), shoulder, Vector3.Lerp(shoulder,elbow,.48f), elbow,
                    Vector3.Lerp(elbow,wrist,.35f), wrist },
                    new[] { 12f, 13f, 11f, 7f, 9f, 5f }, new[] { 10f, 10f, 8f, 6f, 7f, 4f });
                Vector3 fingers=P("NFingertips"+suffix,.65f), hand=Vector3.Lerp(wrist,fingers,.45f);
                Loft(new[] { wrist, hand, Vector3.Lerp(hand,fingers,.5f), fingers },
                    new[] { 5f, 8f, 7f, 2f }, new[] { 4f, 5f, 4f, 1.5f });
                Vector3 hip=P("NHip"+suffix,.7f), knee=P("NKnee"+suffix,.7f), ankle=P("NAnkle"+suffix,.7f);
                Vector3 heel=P("NHeel"+suffix,.7f), toe=P("NToeTip"+suffix,.7f);
                // The same skin continues through the ankle into a flattened
                // heel/instep/forefoot profile; no disconnected foot primitive.
                Loft(new[] { Vector3.Lerp(pelvis,hip,.6f), hip, Vector3.Lerp(hip,knee,.4f), knee,
                    Vector3.Lerp(knee,ankle,.3f), Vector3.Lerp(knee,ankle,.75f), ankle,
                    heel, Vector3.Lerp(heel,toe,.65f), toe },
                    new[] { 14f, 16f, 15f, 9f, 12f, 8f, 5.5f, 4.5f, 5f, 2f },
                    new[] { 11f, 12f, 11f, 7f, 9f, 6f, 5f, 6f, 9f, 4f });
            }
            volume.Geometry(vertices, faces, color);
            return true;
        }
        Vector3 P(string name, float depth)
        {
            Vector3 value = pose[name]; float anchor=pose["NPivot"].z;
            value.z=anchor+(value.z-anchor)*depth; return value;
        }
        void Loft(Vector3[] path, float[] width, float[] depth)
        {
            const int sides=20, subdivisions=4;
            int start=vertices.Count, rings=(path.Length-1)*subdivisions+1;
            for(int ring=0;ring<rings;ring++)
            {
                int segment=Math.Min(path.Length-2,ring/subdivisions); float t=(ring-segment*subdivisions)/(float)subdivisions;
                Vector3 a=path[segment], b=path[segment+1];
                Vector3 before=path[Math.Max(0,segment-1)], after=path[Math.Min(path.Length-1,segment+2)];
                Vector3 tangentA=(b-before)*.5f,tangentB=(after-a)*.5f;
                float limit=Vector3.Distance(a,b); tangentA=Vector3.ClampMagnitude(tangentA,limit);tangentB=Vector3.ClampMagnitude(tangentB,limit);
                float t2=t*t,t3=t2*t;
                Vector3 center=(2*t3-3*t2+1)*a+(t3-2*t2+t)*tangentA+(-2*t3+3*t2)*b+(t3-t2)*tangentB;
                Vector3 axis=((6*t2-6*t)*a+(3*t2-4*t+1)*tangentA+(-6*t2+6*t)*b+(3*t2-2*t)*tangentB).normalized;
                if(axis.sqrMagnitude<.01f)axis=Vector3.up;
                // Keep the depth ellipse oriented to the recovered presentation plane.
                Vector3 across=Vector3.Cross(axis,Vector3.forward).normalized;
                if(across.sqrMagnitude<.01f)across=Vector3.right;
                Vector3 deep=Vector3.Cross(across,axis).normalized;
                float w=Mathf.Lerp(width[segment],width[segment+1],Mathf.SmoothStep(0,1,t));
                float d=Mathf.Lerp(depth[segment],depth[segment+1],Mathf.SmoothStep(0,1,t));
                for(int side=0;side<sides;side++)
                {
                    float angle=side*Mathf.PI*2/sides;
                    vertices.Add(center+across*(Mathf.Cos(angle)*w)+deep*(Mathf.Sin(angle)*d));
                    if(ring==0)continue;
                    int c=start+ring*sides+side,n=start+ring*sides+(side+1)%sides;
                    faces.Add(c-sides);faces.Add(c);faces.Add(n-sides);
                    faces.Add(c);faces.Add(n);faces.Add(n-sides);
                }
            }
            int bottom=vertices.Count;vertices.Add(path[0]);int top=vertices.Count;vertices.Add(path[path.Length-1]);
            for(int side=0;side<sides;side++)
            {
                faces.Add(bottom);faces.Add(start+side);faces.Add(start+(side+1)%sides);
                faces.Add(top);faces.Add(start+(rings-1)*sides+(side+1)%sides);faces.Add(start+(rings-1)*sides+side);
            }
        }
        void LateUpdate()
        {
            if(volume!=null && !SF2DisplayFrameRate.Experimental3DEnabled) volume.gameObject.SetActive(false);
        }
    }
}
