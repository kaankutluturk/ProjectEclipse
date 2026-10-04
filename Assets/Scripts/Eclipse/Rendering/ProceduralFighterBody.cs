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
        readonly SculptedFighterSkin skin = new SculptedFighterSkin();
        int rendered = -1;
        public bool Ready { get; private set; }
        public double LastBuildMilliseconds => skin.LastBuildMilliseconds;
        public double LastDeformMilliseconds => skin.LastDeformMilliseconds;
        public Mesh SurfaceMesh => volume != null ? volume.GetComponent<MeshFilter>().sharedMesh : null;
        public float DepthAnchor => Ready && pose.TryGetValue("NPivot",out var pivot) ? pivot.z : 0;
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
                source = current; nodes.Clear();pose.Clear();vertices.Clear();skin.Reset(); Ready = source != null;
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
            bool changed=vertices.Count==0;
            foreach (var pair in nodes)
            {
                FightInterpolation.SamplePosition(pair.Value, alpha, out float x, out float y, out float z);
                var sample=new Vector3(x,y,z);
                changed |= !pose.TryGetValue(pair.Key,out var previous) || previous != sample;
                pose[pair.Key] = sample;
            }
            volume.UpdateTint(color);
            if(!changed)return true;
            skin.Clear();
            // Neck/chest/waist/pelvis have dedicated proportions rather than stroke widths.
            Vector3 neck = P("NNeck", .85f), chest = P("NChest", .85f), waist = P("NStomach", .85f), pelvis = P("NPivot", .85f);
            Loft(new[] { pelvis + (pelvis-waist).normalized*8, pelvis, waist, chest, neck },
                new[] { 9f, 20f, 15f, 21f, 7f }, new[] { 8f, 13f, 11f, 14f, 6f });
            Vector3 head = P("NHead", .9f), up = (head-neck).normalized;
            skin.Section(neck,head-up*5,6,7,5,6,4);
            skin.Ellipsoid(head+up*4,up,11,14,9.5f);
            skin.Ellipsoid(head-up*6,up,8.5f,8,8);
            for (int side = 1; side <= 2; side++)
            {
                string suffix = "_"+side;
                Vector3 shoulder=P("NShoulder"+suffix,.65f), elbow=P("NElbow"+suffix,.65f), wrist=P("NWrist"+suffix,.65f);
                // One continuous shoulder/elbow/wrist skin: no spherical elbow or intersecting forearm capsule.
                Loft(new[] { chest, shoulder, Vector3.Lerp(shoulder,elbow,.48f), elbow,
                    Vector3.Lerp(elbow,wrist,.35f), wrist },
                    new[] { 12f, 13f, 11f, 7f, 9f, 5f }, new[] { 10f, 10f, 8f, 6f, 7f, 4f });
                Vector3 fingers=P("NFingertips"+suffix,.65f), hand=Vector3.Lerp(wrist,fingers,.45f);
                Loft(new[] { wrist, hand, Vector3.Lerp(hand,fingers,.65f), fingers },
                    new[] { 4.5f, 6.5f, 6f, 3.5f }, new[] { 3.5f, 4.5f, 4f, 3f });
                Vector3 hip=P("NHip"+suffix,.7f), knee=P("NKnee"+suffix,.7f), ankle=P("NAnkle"+suffix,.7f);
                Vector3 heel=P("NHeel"+suffix,.7f), toe=P("NToeTip"+suffix,.7f);
                // The same skin continues through the ankle into a flattened
                // heel/instep/forefoot profile; no disconnected foot primitive.
                Loft(new[] { pelvis, hip, Vector3.Lerp(hip,knee,.4f), knee,
                    Vector3.Lerp(knee,ankle,.3f), Vector3.Lerp(knee,ankle,.75f), ankle,
                    heel, Vector3.Lerp(heel,toe,.65f), toe },
                    new[] { 14f, 16f, 15f, 9f, 12f, 8f, 5.5f, 4.5f, 5f, 2f },
                    new[] { 11f, 12f, 11f, 7f, 9f, 6f, 5f, 6f, 9f, 4f });
            }
            skin.Build(P("NPivot",1),vertices,faces);
            volume.transform.localPosition=P("NPivot",1);
            volume.Geometry(vertices, faces, color,skin.Normals,skin.TopologyChanged);
            return true;
        }
        Vector3 P(string name, float depth)
        {
            Vector3 value = pose[name]; float anchor=pose["NPivot"].z;
            value.z=anchor+(value.z-anchor)*depth; return value;
        }
        void Loft(Vector3[] path, float[] width, float[] depth)
        {
            // Cross sections contribute to one blended field. Shoulders, hips,
            // elbows and ankles no longer carry separate intersecting end caps.
            for(int segment=0;segment<path.Length-1;segment++)
                skin.Section(path[segment],path[segment+1],width[segment],width[segment+1],depth[segment],depth[segment+1],4);
        }
        void LateUpdate()
        {
            if(volume!=null && !SF2DisplayFrameRate.Experimental3DEnabled) volume.gameObject.SetActive(false);
        }
    }
}
