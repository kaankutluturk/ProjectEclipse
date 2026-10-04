using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Eclipse.Modding;

// Imported mesh skinning belongs to Eclipse; archival MacroNode math is unchanged.
public partial class ModelMacroNode
{
    // Foreshortened segments keep at least this share of their 3D length as the
    // frame scale, so absolute offsets shrink smoothly instead of exploding.
    private const float MinimumProjectedShare = .35f;

    private sealed class SkinBinding
    {
        internal ModelNode Start, End;
        // Indices into the skin's proportion rig when the document declares one.
        internal int StartJoint = -1, EndJoint = -1, Frame = -1;
        internal float LocalX, LocalY, LocalZ;
        // Finger phalanx: hand side (0/1) and hand-local pivots of levels 1..n.
        internal int FingerSide = -1;
        internal float[] Pivots;
        internal float Weight, Along, Across, Extend, Offset;
        // Absolute bindings store Extend/Offset in native units instead of a
        // segment-relative Across, so thickness does not follow segment length.
        internal bool Absolute;
    }
    private List<SkinBinding> skinBindings;
    // All bindings are 3D with source frames: blend as dual quaternions (no pinching).
    private bool dualQuaternion;
    private ModelObject skinModel;
    private SkinProportionRig skinRig;

    internal void LoadSkinBindings(ModelObject model, XmlNode node)
    {
        int count = int.Parse(node.Attributes["BonesCount"].Value, CultureInfo.InvariantCulture);
        skinModel = model;
        skinRig = SkinProportionRig.For(model, node);
        if (skinRig != null && skinRig.Leader == null) skinRig.Leader = this;
        skinBindings = new List<SkinBinding>(count);
        dualQuaternion = skinRig != null;
        for (int i = 1; i <= count; i++)
        {
            string suffix = i.ToString(CultureInfo.InvariantCulture);
            string startName = node.Attributes["BoneStart" + suffix].Value, endName = node.Attributes["BoneEnd" + suffix].Value;
            var start = model.FindNodeOrParent(startName);
            var end = model.FindNodeOrParent(endName);
            if (start == null || end == null || start == end)
                throw new System.IO.InvalidDataException("Invalid skin bone on " + GetName());
            if (node.Attributes["LocalX" + suffix] != null)
            {
                // 3D binding: a source-rest point posed in a segment frame, then projected.
                if (skinRig == null || !skinRig.TryFrame(startName, endName, out int frame))
                    throw new System.IO.InvalidDataException("3D skin binding needs Proportions, Rest and a supported segment on " + GetName());
                if (node.Attributes["Dynamic" + suffix] != null &&
                    !skinRig.TryDynamicFrame(int.Parse(node.Attributes["Dynamic" + suffix].Value, CultureInfo.InvariantCulture), out frame))
                    throw new System.IO.InvalidDataException("Skin binding names a missing spring bone on " + GetName());
                var local = new SkinBinding {
                    Start = start, End = end, Frame = frame, Weight = Parse(node, "Weight" + suffix),
                    LocalX = Parse(node, "LocalX" + suffix), LocalY = Parse(node, "LocalY" + suffix), LocalZ = Parse(node, "LocalZ" + suffix)
                };
                if (node.Attributes["Curl" + suffix] != null)
                {
                    local.FingerSide = startName.EndsWith("_2", StringComparison.Ordinal) ? 1 : 0;
                    local.Pivots = ParsePivots(node.Attributes["Pivot" + suffix].Value);
                }
                skinBindings.Add(local);
                continue;
            }
            dualQuaternion = false;
            bool absolute = node.Attributes["Offset" + suffix] != null;
            var binding = new SkinBinding {
                Start = start, End = end, Absolute = absolute,
                Weight = Parse(node, "Weight" + suffix),
                Along = Parse(node, "Along" + suffix),
                Across = absolute ? 0 : Parse(node, "Across" + suffix),
                Offset = absolute ? Parse(node, "Offset" + suffix) : 0,
                Extend = absolute && node.Attributes["Extend" + suffix] != null ? Parse(node, "Extend" + suffix) : 0
            };
            if (skinRig != null && skinRig.TryIndex(startName, out int a) && skinRig.TryIndex(endName, out int b))
            { binding.StartJoint = a; binding.EndJoint = b; }
            skinBindings.Add(binding);
        }
        if (dualQuaternion)
            foreach (var binding in skinBindings)
                if (binding.Frame < 0 || !skinRig.HasSourceFrames(binding.Frame)) { dualQuaternion = false; break; }
    }

    private static float[] ParsePivots(string text)
    {
        var values = new List<float>();
        foreach (string point in text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            foreach (string value in point.Split(','))
                values.Add(float.Parse(value, CultureInfo.InvariantCulture));
        return values.ToArray();
    }

    private static float Parse(XmlNode node, string name) =>
        float.Parse(node.Attributes[name].Value, CultureInfo.InvariantCulture);

    private bool UpdateSkinBindings()
    {
        if (skinBindings == null) return false;
        // Skinned nodes update in load order; the rig's first node refreshes it.
        int sign = skinModel.GetModel()?.FacingSign ?? 1;
        if (skinRig != null && (ReferenceEquals(skinRig.Leader, this) || !skinRig.Updated)) skinRig.Update(sign);
        float x = 0, y = 0;
        if (dualQuaternion)
        {
            var blend = new SkinProportionRig.DualBlend();
            foreach (var binding in skinBindings)
                skinRig.Accumulate(ref blend, binding.Frame, binding.FingerSide, binding.Pivots,
                    binding.LocalX, binding.LocalY, binding.LocalZ, binding.Weight);
            skinRig.Resolve(ref blend, out float bx, out float by);
            _Start.SetX(bx); _Start.SetY(by); _Start.SetZ(0);
            return true;
        }
        foreach (var binding in skinBindings)
        {
            if (binding.Frame >= 0)
            {
                float px, py;
                if (binding.FingerSide >= 0)
                    skinRig.PoseFinger(binding.Frame, binding.FingerSide, binding.Pivots, binding.LocalX, binding.LocalY, binding.LocalZ, out px, out py);
                else skinRig.Pose(binding.Frame, binding.LocalX, binding.LocalY, binding.LocalZ, out px, out py);
                x += binding.Weight * px; y += binding.Weight * py;
                continue;
            }
            float sx, sy, sz, ex, ey, ez;
            if (binding.StartJoint >= 0)
            {
                sx = skinRig.X(binding.StartJoint); sy = skinRig.Y(binding.StartJoint); sz = skinRig.Z(binding.StartJoint);
                ex = skinRig.X(binding.EndJoint); ey = skinRig.Y(binding.EndJoint); ez = skinRig.Z(binding.EndJoint);
            }
            else
            {
                var start = binding.Start.GetStart(); var end = binding.End.GetStart();
                sx = start.GetX(); sy = start.GetY(); sz = start.GetZ();
                ex = end.GetX(); ey = end.GetY(); ez = end.GetZ();
            }
            float dx = ex - sx, dy = ey - sy;
            // Offsets are authored in file coordinates (Y up). Runtime Y is down.
            if (!binding.Absolute)
            {
                float across = binding.Across * sign;
                x += binding.Weight * (sx + binding.Along * dx + across * dy);
                y += binding.Weight * (sy + binding.Along * dy - across * dx);
                continue;
            }
            float dz = ez - sz;
            float projected = (float)Math.Sqrt(dx * dx + dy * dy);
            float scale = Math.Max(projected, MinimumProjectedShare * (float)Math.Sqrt(dx * dx + dy * dy + dz * dz));
            float ux = scale > 1e-6f ? dx / scale : 0, uy = scale > 1e-6f ? dy / scale : 0;
            float offset = binding.Offset * sign;
            x += binding.Weight * (sx + binding.Along * dx + binding.Extend * ux + offset * uy);
            y += binding.Weight * (sy + binding.Along * dy + binding.Extend * uy - offset * ux);
        }
        _Start.SetX(x); _Start.SetY(y); _Start.SetZ(0);
        return true;
    }
}
