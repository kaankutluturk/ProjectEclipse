using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Eclipse.Modding
{
    /// <summary>
    /// Visual-only joints for a skin that declares &lt;Proportions&gt;: the skin is
    /// drawn with its source character's segment lengths while gameplay keeps the
    /// native skeleton. Torso, neck and head follow native directions at their own
    /// lengths; hands and feet stay on the native wrists, ankles and heels, and
    /// elbows/knees are solved with two-bone IK toward the native bend. Mirrors
    /// Tools/Animation/CharacterPipeline.py visual_joints.
    /// </summary>
    internal sealed class SkinProportionRig
    {
        internal static readonly string[,] Torso =
            { { "NPivot", "NStomach" }, { "NStomach", "NChest" }, { "NChest", "NNeck" }, { "NNeck", "NHead" }, { "NHead", "NTop" } };
        internal static readonly string[,] Side =
        {
            { "NNeck", "NShoulder" }, { "NShoulder", "NElbow" }, { "NElbow", "NWrist" }, { "NWrist", "NFingertips" },
            { "NPivot", "NHip" }, { "NHip", "NKnee" }, { "NKnee", "NAnkle" }, { "NHeel", "NToeTip" }
        };
        private static readonly ConditionalWeakTable<ModelObject, Dictionary<XmlDocument, SkinProportionRig>> Rigs =
            new ConditionalWeakTable<ModelObject, Dictionary<XmlDocument, SkinProportionRig>>();

        private readonly Dictionary<string, int> _index = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<ModelNode> _native = new List<ModelNode>();
        private readonly Dictionary<(string, string), float> _lengths = new Dictionary<(string, string), float>();
        private float[] _x, _y, _z, _nx, _ny, _nz;
        internal object Leader;
        private bool _updated;

        // 3D skin frames in canonical file coordinates (facing right, Y up), present
        // when the document also declares Rest. Mirrors CharacterPipeline.visual_frames.
        private static readonly string[] FrontHelpers =
            { "NPelvisF", "NStomachF", "NChestF", "NHeadF", "NKnuckles_1", "NKnuckles_2", "NKnucklesS_1", "NKnucklesS_2" };
        private static readonly string[,] TorsoFront =
            { { "NPivot", "NPelvisF" }, { "NStomach", "NStomachF" }, { "NChest", "NChestF" }, { "NHead", "NHeadF" }, { "NHead", "NHeadF" } };
        private readonly Dictionary<(string, string), int> _frameIndex = new Dictionary<(string, string), int>();
        private readonly Dictionary<(string, string), float[]> _rest = new Dictionary<(string, string), float[]>();
        private float[] _frames; // per frame: origin xyz, then columns a, b, c
        // Source rest frame per frame (same layout), for dual-quaternion blending.
        private float[] _source;
        private bool[] _sourceSet;
        private int _nativeFrames;

        // Hand twist from NKnucklesS against the import-time native rest roll.
        private const float MaxTwistDegrees = 150f;
        private readonly float[] _restRoll = { float.NaN, float.NaN };
        private readonly float[] _twist = new float[2];

        // Spring bones (coats, hair, cloth): simulated tip per bone, visual only.
        private sealed class Spring
        {
            internal int Parent; // frame index
            internal float Hx, Hy, Hz, Dx, Dy, Dz, Length, Stiffness, Damping;
            internal float Px, Py, Pz, Qx, Qy, Qz, Ox, Oy, Oz; // tip, previous tip, last origin
            internal bool Live;
        }
        private readonly List<Spring> _springs = new List<Spring>();
        private int _springSign;
        // Gravity per step at 60 Hz in native units (about 1200 units/s^2), canonical Y up.
        private const float SpringGravity = .33f;

        // Finger curl (CharacterPipeline.finger_curl / curl_point): full curl at this
        // native knuckle bend, and each phalanx's share of it.
        private const float FullCurlDegrees = 75f;
        private static readonly float[] PhalanxCurlDegrees = { 90f, 100f, 70f };
        private readonly float[] _restCurl = new float[2];
        private readonly float[] _curlShare = new float[2];
        private readonly float[] _curlAxis = new float[6]; // hand-local axis per side
        private int _sign = 1;

        /// <summary>All segments a Proportions element must list, as (start, end).</summary>
        internal static IEnumerable<(string, string)> Segments()
        {
            for (int i = 0; i < Torso.GetLength(0); i++) yield return (Torso[i, 0], Torso[i, 1]);
            foreach (string s in new[] { "1", "2" })
                for (int i = 0; i < Side.GetLength(0); i++)
                {
                    string a = Side[i, 0];
                    yield return (a == "NNeck" || a == "NPivot" ? a : a + "_" + s, Side[i, 1] + "_" + s);
                }
        }

        /// <summary>The rig for a skin document on this fighter, or null without Proportions.</summary>
        internal static SkinProportionRig For(ModelObject model, XmlNode skinNode)
        {
            var document = skinNode.OwnerDocument;
            var element = document?.DocumentElement?["Proportions"];
            if (element == null) return null;
            var byDocument = Rigs.GetOrCreateValue(model);
            if (!byDocument.TryGetValue(document, out var rig))
            {
                rig = new SkinProportionRig(model, element);
                byDocument[document] = rig;
            }
            return rig;
        }

        private SkinProportionRig(ModelObject model, XmlElement element)
        {
            foreach (XmlNode child in element.ChildNodes)
            {
                if (!(child is XmlElement segment)) continue;
                _lengths[(segment.GetAttribute("Start"), segment.GetAttribute("End"))] =
                    float.Parse(segment.GetAttribute("Length"), CultureInfo.InvariantCulture);
            }
            foreach (var (a, b) in Segments())
            {
                if (!_lengths.ContainsKey((a, b))) throw new System.IO.InvalidDataException("Proportions lack " + a + "-" + b);
                Add(model, a); Add(model, b);
            }
            foreach (string s in new[] { "_1", "_2" }) { Add(model, "NAnkle" + s); Add(model, "NHeel" + s); }
            Add(model, "NChestF"); Add(model, "NPelvisF"); // anatomical bend hints
            var rest = element.OwnerDocument.DocumentElement["Rest"];
            if (rest != null)
            {
                foreach (string helper in FrontHelpers) Add(model, helper);
                foreach (var key in FrameKeys()) _frameIndex[key] = _frameIndex.Count;
                _nativeFrames = _frameIndex.Count;
                foreach (XmlNode child in rest.ChildNodes)
                    if (child is XmlElement spring && spring.Name == "Dynamic")
                    {
                        float[] head = Triple(spring.GetAttribute("Head")), direction = Triple(spring.GetAttribute("Dir"));
                        string parent = spring.GetAttribute("Parent");
                        int parentFrame = parent.StartsWith("#", StringComparison.Ordinal)
                            ? _nativeFrames + int.Parse(parent.Substring(1), CultureInfo.InvariantCulture)
                            : _frameIndex[ParseKey(parent)];
                        _springs.Add(new Spring
                        {
                            Parent = parentFrame, Hx = head[0], Hy = head[1], Hz = head[2],
                            Dx = direction[0], Dy = direction[1], Dz = direction[2],
                            Length = Float(spring, "Length", 1), Stiffness = Float(spring, "Stiffness", .15f), Damping = Float(spring, "Damping", .9f)
                        });
                    }
                int frameCount = _nativeFrames + _springs.Count;
                _frames = new float[frameCount * 12];
                _source = new float[frameCount * 12];
                _sourceSet = new bool[frameCount];
                foreach (XmlNode child in rest.ChildNodes)
                    if (child is XmlElement frame && frame.Name == "Frame")
                    {
                        int index = frame.HasAttribute("Dynamic")
                            ? _nativeFrames + int.Parse(frame.GetAttribute("Dynamic"), CultureInfo.InvariantCulture)
                            : _frameIndex[(frame.GetAttribute("Start"), frame.GetAttribute("End"))];
                        float[] origin = Triple(frame.GetAttribute("O")), a = Triple(frame.GetAttribute("A")), b = Triple(frame.GetAttribute("B"));
                        int o = index * 12;
                        _source[o] = origin[0]; _source[o + 1] = origin[1]; _source[o + 2] = origin[2];
                        _source[o + 3] = a[0]; _source[o + 4] = a[1]; _source[o + 5] = a[2];
                        _source[o + 6] = b[0]; _source[o + 7] = b[1]; _source[o + 8] = b[2];
                        _source[o + 9] = a[1] * b[2] - a[2] * b[1]; _source[o + 10] = a[2] * b[0] - a[0] * b[2]; _source[o + 11] = a[0] * b[1] - a[1] * b[0];
                        _sourceSet[index] = true;
                    }
                foreach (XmlNode child in rest.ChildNodes)
                    if (child is XmlElement bone && bone.Name == "Bone")
                    {
                        _rest[(bone.GetAttribute("Start"), bone.GetAttribute("End"))] = new[]
                        {
                            float.Parse(bone.GetAttribute("X"), CultureInfo.InvariantCulture),
                            float.Parse(bone.GetAttribute("Y"), CultureInfo.InvariantCulture),
                            float.Parse(bone.GetAttribute("Z"), CultureInfo.InvariantCulture)
                        };
                        string end = bone.GetAttribute("End");
                        if (bone.HasAttribute("Curl") && end.StartsWith("NFingertips_", StringComparison.Ordinal))
                            _restCurl[end.EndsWith("_2", StringComparison.Ordinal) ? 1 : 0] =
                                float.Parse(bone.GetAttribute("Curl"), CultureInfo.InvariantCulture);
                        if (bone.HasAttribute("Twist") && end.StartsWith("NFingertips_", StringComparison.Ordinal))
                            _restRoll[end.EndsWith("_2", StringComparison.Ordinal) ? 1 : 0] =
                                float.Parse(bone.GetAttribute("Twist"), CultureInfo.InvariantCulture);
                    }
                foreach (var (child, _) in LimbParents())
                    if (!_rest.ContainsKey(child)) throw new System.IO.InvalidDataException("Rest lacks " + child.Item1 + "-" + child.Item2);
            }
            int n = _native.Count;
            _x = new float[n]; _y = new float[n]; _z = new float[n]; _nx = new float[n]; _ny = new float[n]; _nz = new float[n];
        }

        private static float[] Triple(string text)
        {
            var parts = text.Split(',');
            return new[] { float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture),
                           float.Parse(parts[2], CultureInfo.InvariantCulture) };
        }

        private static float Float(XmlElement element, string name, float fallback) =>
            element.HasAttribute(name) ? float.Parse(element.GetAttribute(name), CultureInfo.InvariantCulture) : fallback;

        private static (string, string) ParseKey(string text)
        {
            int dash = text.IndexOf('-');
            return (text.Substring(0, dash), text.Substring(dash + 1));
        }

        internal bool TryDynamicFrame(int spring, out int frame)
        {
            frame = _nativeFrames + spring;
            return spring >= 0 && spring < _springs.Count;
        }

        /// <summary>True when every frame has a source rest frame, so bindings can blend as dual quaternions.</summary>
        internal bool HasSourceFrames(int frame) => _sourceSet != null && frame >= 0 && frame < _sourceSet.Length && _sourceSet[frame];

        private void Add(ModelObject model, string name)
        {
            if (_index.ContainsKey(name)) return;
            var node = model.FindNodeOrParent(name) ?? throw new System.IO.InvalidDataException("Proportions need native point " + name);
            _index[name] = _native.Count;
            _native.Add(node);
        }

        internal bool TryIndex(string name, out int index) => _index.TryGetValue(name, out index);
        internal bool TryFrame(string start, string end, out int frame)
        {
            frame = -1;
            return _frames != null && _frameIndex.TryGetValue((start, end), out frame);
        }

        internal static IEnumerable<((string, string) child, (string, string) parent)> LimbParents()
        {
            foreach (string s in new[] { "_1", "_2" })
            {
                yield return (("NShoulder" + s, "NElbow" + s), ("NChest", "NNeck"));
                yield return (("NElbow" + s, "NWrist" + s), ("NShoulder" + s, "NElbow" + s));
                yield return (("NWrist" + s, "NFingertips" + s), ("NElbow" + s, "NWrist" + s));
                yield return (("NHip" + s, "NKnee" + s), ("NPivot", "NStomach"));
                yield return (("NKnee" + s, "NAnkle" + s), ("NHip" + s, "NKnee" + s));
            }
        }

        /// <summary>Segments that can carry 3D LocalX/Y/Z skin bindings.</summary>
        internal static IEnumerable<(string, string)> FrameKeys()
        {
            for (int i = 0; i < Torso.GetLength(0); i++) yield return (Torso[i, 0], Torso[i, 1]);
            foreach (var (child, _) in LimbParents()) yield return child;
            yield return ("NHeel_1", "NToeTip_1");
            yield return ("NHeel_2", "NToeTip_2");
        }

        /// <summary>Runtime X/Y of a local point in a posed segment frame (orthographic projection).</summary>
        /// <summary>Pose a finger point: fold it about its phalanx pivots (hand-local,
        /// proximal first, xyz triples), distal joint first, then place it in the hand.</summary>
        internal void PoseFinger(int frame, int side, float[] pivots, float lx, float ly, float lz, out float x, out float y)
        {
            float share = _curlShare[side];
            if (share > 0)
            {
                float ax = _curlAxis[side * 3], ay = _curlAxis[side * 3 + 1], az = _curlAxis[side * 3 + 2];
                for (int level = pivots.Length / 3; level >= 1; level--)
                {
                    int q = (level - 1) * 3;
                    float px = lx - pivots[q], py = ly - pivots[q + 1], pz = lz - pivots[q + 2];
                    double angle = share * PhalanxCurlDegrees[level - 1] * Math.PI / 180;
                    float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle), d = (ax * px + ay * py + az * pz) * (1 - c);
                    lx = pivots[q] + px * c + (ay * pz - az * py) * s + ax * d;
                    ly = pivots[q + 1] + py * c + (az * px - ax * pz) * s + ay * d;
                    lz = pivots[q + 2] + pz * c + (ax * py - ay * px) * s + az * d;
                }
            }
            Pose(frame, lx, ly, lz, out x, out y);
        }

        internal void Pose(int frame, float lx, float ly, float lz, out float x, out float y)
        {
            int o = frame * 12;
            float cx = _frames[o] + _frames[o + 3] * lx + _frames[o + 6] * ly + _frames[o + 9] * lz;
            float cy = _frames[o + 1] + _frames[o + 4] * lx + _frames[o + 7] * ly + _frames[o + 10] * lz;
            x = cx * _sign; y = -cy;
        }
        internal float X(int i) => _x[i];
        internal float Y(int i) => _y[i];
        internal float Z(int i) => _z[i];
        internal bool Updated => _updated;

        private int I(string name) => _index[name];
        private float L(string a, string b) => _lengths[(a, b)];

        internal void Update(int sign)
        {
            _sign = sign < 0 ? -1 : 1;
            for (int i = 0; i < _native.Count; i++)
            {
                var p = _native[i].GetStart();
                _nx[i] = p.GetX(); _ny[i] = p.GetY(); _nz[i] = p.GetZ();
            }
            UnswapPairs();
            float nativeLeg = 0, visualLeg = 0;
            foreach (string s in new[] { "_1", "_2" })
            {
                nativeLeg += Distance(I("NHip" + s), I("NKnee" + s)) + Distance(I("NKnee" + s), I("NAnkle" + s));
                visualLeg += L("NHip" + s, "NKnee" + s) + L("NKnee" + s, "NAnkle" + s);
            }
            // Move the pelvis along the body axis so proportioned legs reach the native feet.
            int pivot = I("NPivot"), neck = I("NNeck");
            Direction(neck, pivot, out float dx, out float dy, out float dz);
            float shift = (nativeLeg - visualLeg) / 2;
            Set(pivot, _nx[pivot] + dx * shift, _ny[pivot] + dy * shift, _nz[pivot] + dz * shift);
            for (int i = 0; i < Torso.GetLength(0); i++) Follow(Torso[i, 0], Torso[i, 1], Torso[i, 0]);
            foreach (string s in new[] { "_1", "_2" })
            {
                Follow("NNeck", "NShoulder" + s, "NNeck");
                Pin("NWrist" + s);
                Bend("NShoulder" + s, "NElbow" + s, "NWrist" + s, "NChestF", "NChest"); // elbows point back
                Follow("NWrist" + s, "NFingertips" + s, "NWrist" + s);
                Follow("NPivot", "NHip" + s, "NPivot");
                Pin("NAnkle" + s);
                Bend("NHip" + s, "NKnee" + s, "NAnkle" + s, "NPivot", "NPelvisF"); // knees point forward
                Pin("NHeel" + s);
                Follow("NHeel" + s, "NToeTip" + s, "NHeel" + s);
            }
            if (_frames != null) UpdateFrames();
            _updated = true;
        }

        // Canonical coordinates undo the facing mirror and the runtime Y-down flip, so
        // frames are right-handed like the source rest frames they stand in for.
        private void Canonical(float[] x, float[] y, float[] z, int i, out float cx, out float cy, out float cz)
        {
            cx = x[i] * _sign; cy = -y[i]; cz = z[i];
        }

        private void UpdateFrames()
        {
            for (int i = 0; i < Torso.GetLength(0); i++)
            {
                Canonical(_x, _y, _z, I(Torso[i, 0]), out float ox, out float oy, out float oz);
                Canonical(_x, _y, _z, I(Torso[i, 1]), out float ex, out float ey, out float ez);
                Canonical(_nx, _ny, _nz, I(TorsoFront[i, 0]), out float ax, out float ay, out float az);
                Canonical(_nx, _ny, _nz, I(TorsoFront[i, 1]), out float bx, out float by, out float bz);
                SetFrame(_frameIndex[(Torso[i, 0], Torso[i, 1])], ox, oy, oz, ex - ox, ey - oy, ez - oz, bx - ax, by - ay, bz - az);
            }
            foreach (var (child, parent) in LimbParents())
            {
                int p = _frameIndex[parent] * 12, o = _frameIndex[child] * 12;
                Canonical(_x, _y, _z, I(child.Item1), out float ox, out float oy, out float oz);
                // The hand follows the palm (wrist to knuckles), not the fingertips, so a
                // closing fist does not tilt the whole hand.
                string end = child.Item2.StartsWith("NFingertips", StringComparison.Ordinal)
                    ? "NKnuckles" + child.Item2.Substring("NFingertips".Length) : null;
                float ex, ey, ez;
                if (end != null) Canonical(_nx, _ny, _nz, I(end), out ex, out ey, out ez);
                else Canonical(_x, _y, _z, I(child.Item2), out ex, out ey, out ez);
                float dx = ex - ox, dy = ey - oy, dz = ez - oz;
                float length = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                var rest = _rest[child];
                float qx = rest[0], qy = rest[1], qz = rest[2];
                if (length > 1e-6f)
                {   // current direction in the parent frame
                    qx = (_frames[p + 3] * dx + _frames[p + 4] * dy + _frames[p + 5] * dz) / length;
                    qy = (_frames[p + 6] * dx + _frames[p + 7] * dy + _frames[p + 8] * dz) / length;
                    qz = (_frames[p + 9] * dx + _frames[p + 10] * dy + _frames[p + 11] * dz) / length;
                }
                var swing = Swing(rest[0], rest[1], rest[2], qx, qy, qz);
                _frames[o] = ox; _frames[o + 1] = oy; _frames[o + 2] = oz;
                for (int column = 0; column < 3; column++)
                    for (int row = 0; row < 3; row++)
                        _frames[o + 3 + column * 3 + row] = _frames[p + 3 + row] * swing[column * 3] +
                            _frames[p + 6 + row] * swing[column * 3 + 1] + _frames[p + 9 + row] * swing[column * 3 + 2];
            }
            for (int side = 0; side < 2; side++) UpdateTwist(side);
            for (int side = 0; side < 2; side++) UpdateCurl(side);
            foreach (string s in new[] { "_1", "_2" })
            {
                Canonical(_x, _y, _z, I("NHeel" + s), out float ox, out float oy, out float oz);
                Canonical(_x, _y, _z, I("NToeTip" + s), out float ex, out float ey, out float ez);
                Canonical(_x, _y, _z, I("NAnkle" + s), out float ax, out float ay, out float az);
                SetFrame(_frameIndex[("NHeel" + s, "NToeTip" + s)], ox, oy, oz, ex - ox, ey - oy, ez - oz, ax - ox, ay - oy, az - oz);
            }
            UpdateSprings();
        }

        // Native hand roll about the palm (degrees), measured from NKnucklesS in a hand
        // frame, or NaN when the side point is degenerate. CharacterPipeline.hand_roll.
        private float HandRoll(int frame, float[] restDirection, string s)
        {
            int o = frame * 12;
            float dx = restDirection[0], dy = restDirection[1], dz = restDirection[2];
            float ax = _frames[o + 3] * dx + _frames[o + 6] * dy + _frames[o + 9] * dz;
            float ay = _frames[o + 4] * dx + _frames[o + 7] * dy + _frames[o + 10] * dz;
            float az = _frames[o + 5] * dx + _frames[o + 8] * dy + _frames[o + 11] * dz;
            Normalize(ref ax, ref ay, ref az, 1, 0, 0);
            // Fixed perpendicular of the rest direction (same fallback as frame()).
            float ex, ey, ez;
            if (Math.Abs(dz) < .9f) { ex = dy; ey = -dx; ez = 0; } else { ex = 0; ey = dz; ez = -dy; }
            Normalize(ref ex, ref ey, ref ez, 1, 0, 0);
            float rx = _frames[o + 3] * ex + _frames[o + 6] * ey + _frames[o + 9] * ez;
            float ry = _frames[o + 4] * ex + _frames[o + 7] * ey + _frames[o + 10] * ez;
            float rz = _frames[o + 5] * ex + _frames[o + 8] * ey + _frames[o + 11] * ez;
            Canonical(_nx, _ny, _nz, I("NKnuckles" + s), out float kx, out float ky, out float kz);
            Canonical(_nx, _ny, _nz, I("NKnucklesS" + s), out float sx, out float sy, out float sz);
            sx -= kx; sy -= ky; sz -= kz;
            float d = sx * ax + sy * ay + sz * az; sx -= ax * d; sy -= ay * d; sz -= az * d;
            d = rx * ax + ry * ay + rz * az; rx -= ax * d; ry -= ay * d; rz -= az * d;
            if (Math.Sqrt(sx * sx + sy * sy + sz * sz) < 1 || Math.Sqrt(rx * rx + ry * ry + rz * rz) < 1e-6f) return float.NaN;
            float cx = ry * sz - rz * sy, cy = rz * sx - rx * sz, cz = rx * sy - ry * sx;
            return (float)(Math.Atan2(ax * cx + ay * cy + az * cz, rx * sx + ry * sy + rz * sz) * 180 / Math.PI);
        }

        private void UpdateTwist(int side)
        {
            string s = side == 0 ? "_1" : "_2";
            _twist[side] = 0;
            if (float.IsNaN(_restRoll[side])) return;
            var key = ("NWrist" + s, "NFingertips" + s);
            int frame = _frameIndex[key];
            var rest = _rest[key];
            float roll = HandRoll(frame, rest, s);
            if (float.IsNaN(roll)) return;
            float twist = (float)(((roll - _restRoll[side] + 180.0) % 360.0 + 360.0) % 360.0 - 180.0);
            twist = Math.Max(-MaxTwistDegrees, Math.Min(MaxTwistDegrees, twist));
            _twist[side] = twist;
            int o = frame * 12;
            float ax = _frames[o + 3] * rest[0] + _frames[o + 6] * rest[1] + _frames[o + 9] * rest[2];
            float ay = _frames[o + 4] * rest[0] + _frames[o + 7] * rest[1] + _frames[o + 10] * rest[2];
            float az = _frames[o + 5] * rest[0] + _frames[o + 8] * rest[1] + _frames[o + 11] * rest[2];
            Normalize(ref ax, ref ay, ref az, 1, 0, 0);
            for (int column = 0; column < 3; column++)
                Rotate(_frames, o + 3 + column * 3, ax, ay, az, twist);
        }

        // Rodrigues rotation in place of the vector at values[offset..offset+2].
        private static void Rotate(float[] values, int offset, float ax, float ay, float az, float degrees)
        {
            double angle = degrees * Math.PI / 180;
            float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle);
            float vx = values[offset], vy = values[offset + 1], vz = values[offset + 2];
            float d = (ax * vx + ay * vy + az * vz) * (1 - c);
            values[offset] = vx * c + (ay * vz - az * vy) * s + ax * d;
            values[offset + 1] = vy * c + (az * vx - ax * vz) * s + ay * d;
            values[offset + 2] = vz * c + (ax * vy - ay * vx) * s + az * d;
        }

        // Verlet tip per spring bone: lags behind its target on the parent frame, falls
        // under gravity and keeps its length. Snaps to the target on the first update,
        // when the fighter turns around or after a teleport.
        private void UpdateSprings()
        {
            bool turned = _springSign != _sign;
            _springSign = _sign;
            for (int k = 0; k < _springs.Count; k++)
            {
                var spring = _springs[k];
                int p = spring.Parent * 12, o = (_nativeFrames + k) * 12;
                float ox = _frames[p] + _frames[p + 3] * spring.Hx + _frames[p + 6] * spring.Hy + _frames[p + 9] * spring.Hz;
                float oy = _frames[p + 1] + _frames[p + 4] * spring.Hx + _frames[p + 7] * spring.Hy + _frames[p + 10] * spring.Hz;
                float oz = _frames[p + 2] + _frames[p + 5] * spring.Hx + _frames[p + 8] * spring.Hy + _frames[p + 11] * spring.Hz;
                float tx = ox + (_frames[p + 3] * spring.Dx + _frames[p + 6] * spring.Dy + _frames[p + 9] * spring.Dz) * spring.Length;
                float ty = oy + (_frames[p + 4] * spring.Dx + _frames[p + 7] * spring.Dy + _frames[p + 10] * spring.Dz) * spring.Length;
                float tz = oz + (_frames[p + 5] * spring.Dx + _frames[p + 8] * spring.Dy + _frames[p + 11] * spring.Dz) * spring.Length;
                float jump = (ox - spring.Ox) * (ox - spring.Ox) + (oy - spring.Oy) * (oy - spring.Oy) + (oz - spring.Oz) * (oz - spring.Oz);
                if (!spring.Live || turned || jump > 9 * spring.Length * spring.Length)
                {
                    spring.Px = spring.Qx = tx; spring.Py = spring.Qy = ty; spring.Pz = spring.Qz = tz;
                    spring.Live = true;
                }
                else
                {
                    float vx = (spring.Px - spring.Qx) * spring.Damping, vy = (spring.Py - spring.Qy) * spring.Damping, vz = (spring.Pz - spring.Qz) * spring.Damping;
                    spring.Qx = spring.Px; spring.Qy = spring.Py; spring.Qz = spring.Pz;
                    spring.Px += vx; spring.Py += vy - SpringGravity; spring.Pz += vz;
                    spring.Px += (tx - spring.Px) * spring.Stiffness;
                    spring.Py += (ty - spring.Py) * spring.Stiffness;
                    spring.Pz += (tz - spring.Pz) * spring.Stiffness;
                    float lx = spring.Px - ox, ly = spring.Py - oy, lz = spring.Pz - oz;
                    float length = (float)Math.Sqrt(lx * lx + ly * ly + lz * lz);
                    if (length < 1e-6f) { spring.Px = tx; spring.Py = ty; spring.Pz = tz; }
                    else
                    {
                        float k2 = spring.Length / length;
                        spring.Px = ox + lx * k2; spring.Py = oy + ly * k2; spring.Pz = oz + lz * k2;
                    }
                }
                spring.Ox = ox; spring.Oy = oy; spring.Oz = oz;
                // Frame: parent frame swung from the rest direction to the simulated one.
                float dx = spring.Px - ox, dy = spring.Py - oy, dz = spring.Pz - oz;
                float dl = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                float qx = spring.Dx, qy = spring.Dy, qz = spring.Dz;
                if (dl > 1e-6f)
                {
                    qx = (_frames[p + 3] * dx + _frames[p + 4] * dy + _frames[p + 5] * dz) / dl;
                    qy = (_frames[p + 6] * dx + _frames[p + 7] * dy + _frames[p + 8] * dz) / dl;
                    qz = (_frames[p + 9] * dx + _frames[p + 10] * dy + _frames[p + 11] * dz) / dl;
                }
                var swing = Swing(spring.Dx, spring.Dy, spring.Dz, qx, qy, qz);
                _frames[o] = ox; _frames[o + 1] = oy; _frames[o + 2] = oz;
                for (int column = 0; column < 3; column++)
                    for (int row = 0; row < 3; row++)
                        _frames[o + 3 + column * 3 + row] = _frames[p + 3 + row] * swing[column * 3] +
                            _frames[p + 6 + row] * swing[column * 3 + 1] + _frames[p + 9 + row] * swing[column * 3 + 2];
            }
        }

        // ---- Dual-quaternion skinning (CharacterPipeline.dual_quaternion_point) ----

        internal struct DualBlend
        {
            internal float R0, R1, R2, R3, D0, D1, D2, D3, F0, F1, F2, F3, Vx, Vy, Vz;
            internal bool HasFirst;
        }

        /// <summary>Adds one influence: frame, hand-local curl (side/pivots or null) and rest-frame local point.</summary>
        internal void Accumulate(ref DualBlend blend, int frame, int curlSide, float[] pivots, float lx, float ly, float lz, float weight)
        {
            int o = frame * 12;
            // Columns of the posed frame (R), the curl transform (M, c) and the source frame (S).
            var r = new float[9];
            Array.Copy(_frames, o + 3, r, 0, 9);
            var m = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            float cx = 0, cy = 0, cz = 0;
            if (curlSide >= 0 && pivots != null && _curlShare[curlSide] > 0)
            {
                float ax = _curlAxis[curlSide * 3], ay = _curlAxis[curlSide * 3 + 1], az = _curlAxis[curlSide * 3 + 2];
                for (int level = pivots.Length / 3; level >= 1; level--)
                {
                    int q = (level - 1) * 3;
                    float degrees = _curlShare[curlSide] * PhalanxCurlDegrees[level - 1];
                    for (int column = 0; column < 3; column++) Rotate(m, column * 3, ax, ay, az, degrees);
                    var offset = new[] { cx - pivots[q], cy - pivots[q + 1], cz - pivots[q + 2] };
                    Rotate(offset, 0, ax, ay, az, degrees);
                    cx = offset[0] + pivots[q]; cy = offset[1] + pivots[q + 1]; cz = offset[2] + pivots[q + 2];
                }
            }
            int forearm = ForearmSide(frame);
            if (forearm >= 0 && _twist[forearm] != 0)
            {   // forearm skin takes a growing share of the hand twist toward the wrist
                string s = forearm == 0 ? "_1" : "_2";
                var rest = _rest[("NElbow" + s, "NWrist" + s)];
                float along = (lx * rest[0] + ly * rest[1] + lz * rest[2]) / L("NElbow" + s, "NWrist" + s);
                float ax = r[0] * rest[0] + r[3] * rest[1] + r[6] * rest[2];
                float ay = r[1] * rest[0] + r[4] * rest[1] + r[7] * rest[2];
                float az = r[2] * rest[0] + r[5] * rest[1] + r[8] * rest[2];
                Normalize(ref ax, ref ay, ref az, 1, 0, 0);
                float degrees = _twist[forearm] * Math.Max(0, Math.Min(1, along));
                for (int column = 0; column < 3; column++) Rotate(r, column * 3, ax, ay, az, degrees);
            }
            var rm = Mul(r, m);
            var src = new float[9];
            Array.Copy(_source, o + 3, src, 0, 9);
            // Q = R M S^T ; t = O + R c - Q Os ; rest point v = Os + S L.
            var q9 = MulTransposed(rm, src);
            float sx = _source[o], sy = _source[o + 1], sz = _source[o + 2];
            float tx = _frames[o] + r[0] * cx + r[3] * cy + r[6] * cz - (q9[0] * sx + q9[3] * sy + q9[6] * sz);
            float ty = _frames[o + 1] + r[1] * cx + r[4] * cy + r[7] * cz - (q9[1] * sx + q9[4] * sy + q9[7] * sz);
            float tz = _frames[o + 2] + r[2] * cx + r[5] * cy + r[8] * cz - (q9[2] * sx + q9[5] * sy + q9[8] * sz);
            Quaternion(q9, out float w, out float x, out float y, out float z);
            if (!blend.HasFirst)
            {
                blend.F0 = w; blend.F1 = x; blend.F2 = y; blend.F3 = z; blend.HasFirst = true;
                blend.Vx = sx + src[0] * lx + src[3] * ly + src[6] * lz;
                blend.Vy = sy + src[1] * lx + src[4] * ly + src[7] * lz;
                blend.Vz = sz + src[2] * lx + src[5] * ly + src[8] * lz;
            }
            else if (w * blend.F0 + x * blend.F1 + y * blend.F2 + z * blend.F3 < 0) { w = -w; x = -x; y = -y; z = -z; }
            // Dual part: 0.5 * (0, t) * q
            float d0 = .5f * (-tx * x - ty * y - tz * z);
            float d1 = .5f * (tx * w + ty * z - tz * y);
            float d2 = .5f * (-tx * z + ty * w + tz * x);
            float d3 = .5f * (tx * y - ty * x + tz * w);
            blend.R0 += weight * w; blend.R1 += weight * x; blend.R2 += weight * y; blend.R3 += weight * z;
            blend.D0 += weight * d0; blend.D1 += weight * d1; blend.D2 += weight * d2; blend.D3 += weight * d3;
        }

        /// <summary>Runtime X/Y of the blended point.</summary>
        internal void Resolve(ref DualBlend blend, out float x, out float y)
        {
            float n = (float)Math.Sqrt(blend.R0 * blend.R0 + blend.R1 * blend.R1 + blend.R2 * blend.R2 + blend.R3 * blend.R3);
            if (n < 1e-9f) { x = y = 0; return; }
            float w = blend.R0 / n, qx = blend.R1 / n, qy = blend.R2 / n, qz = blend.R3 / n;
            float e0 = blend.D0 / n, e1 = blend.D1 / n, e2 = blend.D2 / n, e3 = blend.D3 / n;
            // Rotate v by q.
            float vx = blend.Vx, vy = blend.Vy, vz = blend.Vz;
            float ix = w * vx + qy * vz - qz * vy, iy = w * vy + qz * vx - qx * vz, iz = w * vz + qx * vy - qy * vx, iw = -qx * vx - qy * vy - qz * vz;
            float rx = ix * w + iw * -qx + iy * -qz - iz * -qy;
            float ry = iy * w + iw * -qy + iz * -qx - ix * -qz;
            // Translation: 2 * vec(dual * conj(real)).
            float tx = 2 * (-e0 * qx + e1 * w - e2 * qz + e3 * qy);
            float ty = 2 * (-e0 * qy + e1 * qz + e2 * w - e3 * qx);
            float cx = rx + tx, cy = ry + ty;
            x = cx * _sign; y = -cy;
        }

        private int ForearmSide(int frame)
        {
            if (frame >= _nativeFrames) return -1;
            if (_frameIndex.TryGetValue(("NElbow_1", "NWrist_1"), out int left) && left == frame) return 0;
            if (_frameIndex.TryGetValue(("NElbow_2", "NWrist_2"), out int right) && right == frame) return 1;
            return -1;
        }

        private static float[] Mul(float[] a, float[] b)
        {
            var result = new float[9];
            for (int column = 0; column < 3; column++)
                for (int row = 0; row < 3; row++)
                    result[column * 3 + row] = a[row] * b[column * 3] + a[3 + row] * b[column * 3 + 1] + a[6 + row] * b[column * 3 + 2];
            return result;
        }

        // a * b^T for column-major 3x3 matrices.
        private static float[] MulTransposed(float[] a, float[] b)
        {
            var result = new float[9];
            for (int column = 0; column < 3; column++)
                for (int row = 0; row < 3; row++)
                    result[column * 3 + row] = a[row] * b[column] + a[3 + row] * b[3 + column] + a[6 + row] * b[6 + column];
            return result;
        }

        private static void Quaternion(float[] m, out float w, out float x, out float y, out float z)
        {
            // m is column-major: element (row i, column j) = m[j * 3 + i].
            float m00 = m[0], m10 = m[1], m20 = m[2], m01 = m[3], m11 = m[4], m21 = m[5], m02 = m[6], m12 = m[7], m22 = m[8];
            float trace = m00 + m11 + m22;
            if (trace > 0)
            {
                float k = (float)Math.Sqrt(trace + 1) * 2;
                w = .25f * k; x = (m21 - m12) / k; y = (m02 - m20) / k; z = (m10 - m01) / k;
            }
            else if (m00 > m11 && m00 > m22)
            {
                float k = (float)Math.Sqrt(1 + m00 - m11 - m22) * 2;
                w = (m21 - m12) / k; x = .25f * k; y = (m01 + m10) / k; z = (m02 + m20) / k;
            }
            else if (m11 > m22)
            {
                float k = (float)Math.Sqrt(1 + m11 - m00 - m22) * 2;
                w = (m02 - m20) / k; x = (m01 + m10) / k; y = .25f * k; z = (m12 + m21) / k;
            }
            else
            {
                float k = (float)Math.Sqrt(1 + m22 - m00 - m11) * 2;
                w = (m10 - m01) / k; x = (m02 + m20) / k; y = (m12 + m21) / k; z = .25f * k;
            }
        }

        // Native knuckle bend (palm = wrist->knuckles, finger = knuckles->fingertips) as a
        // share of a full fist, with the knuckle-line axis in hand-local coordinates.
        private void UpdateCurl(int side)
        {
            string s = side == 0 ? "_1" : "_2";
            Canonical(_nx, _ny, _nz, I("NWrist" + s), out float wx, out float wy, out float wz);
            Canonical(_nx, _ny, _nz, I("NKnuckles" + s), out float kx, out float ky, out float kz);
            Canonical(_nx, _ny, _nz, I("NFingertips" + s), out float fx, out float fy, out float fz);
            float px = kx - wx, py = ky - wy, pz = kz - wz, qx = fx - kx, qy = fy - ky, qz = fz - kz;
            Normalize(ref px, ref py, ref pz, 0, 0, 0); Normalize(ref qx, ref qy, ref qz, 0, 0, 0);
            float ax = py * qz - pz * qy, ay = pz * qx - px * qz, az = px * qy - py * qx;
            float sine = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            _curlShare[side] = 0;
            if (sine < 1e-6f) return;
            float bend = (float)(Math.Atan2(sine, px * qx + py * qy + pz * qz) * 180 / Math.PI);
            _curlShare[side] = Math.Max(0, Math.Min(1, (bend - _restCurl[side]) / FullCurlDegrees));
            ax /= sine; ay /= sine; az /= sine;
            int h = _frameIndex[("NWrist" + s, "NFingertips" + s)] * 12;
            float lx = _frames[h + 3] * ax + _frames[h + 4] * ay + _frames[h + 5] * az;
            float ly = _frames[h + 6] * ax + _frames[h + 7] * ay + _frames[h + 8] * az;
            float lz = _frames[h + 9] * ax + _frames[h + 10] * ay + _frames[h + 11] * az;
            Normalize(ref lx, ref ly, ref lz, 0, 0, 1);
            _curlAxis[side * 3] = lx; _curlAxis[side * 3 + 1] = ly; _curlAxis[side * 3 + 2] = lz;
        }

        // Right-handed columns (a, b, a x b) from a primary and a secondary direction.
        private void SetFrame(int frame, float ox, float oy, float oz, float px, float py, float pz, float sx, float sy, float sz)
        {
            Normalize(ref px, ref py, ref pz, 0, 1, 0);
            float dot = px * sx + py * sy + pz * sz;
            sx -= px * dot; sy -= py * dot; sz -= pz * dot;
            if (sx * sx + sy * sy + sz * sz < 1e-12f)
            {   // secondary parallel to primary: same fallback as CharacterPipeline.frame
                if (Math.Abs(pz) < .9f) { sx = py; sy = -px; sz = 0; } else { sx = 0; sy = pz; sz = -py; }
            }
            Normalize(ref sx, ref sy, ref sz, 1, 0, 0);
            int o = frame * 12;
            _frames[o] = ox; _frames[o + 1] = oy; _frames[o + 2] = oz;
            _frames[o + 3] = px; _frames[o + 4] = py; _frames[o + 5] = pz;
            _frames[o + 6] = sx; _frames[o + 7] = sy; _frames[o + 8] = sz;
            _frames[o + 9] = py * sz - pz * sy; _frames[o + 10] = pz * sx - px * sz; _frames[o + 11] = px * sy - py * sx;
        }

        private static void Normalize(ref float x, ref float y, ref float z, float fx, float fy, float fz)
        {
            float length = (float)Math.Sqrt(x * x + y * y + z * z);
            if (length < 1e-6f) { x = fx; y = fy; z = fz; return; }
            x /= length; y /= length; z /= length;
        }

        // Shortest-arc rotation taking unit p to unit q, as columns (Rodrigues).
        private static float[] Swing(float px, float py, float pz, float qx, float qy, float qz)
        {
            float ax = py * qz - pz * qy, ay = pz * qx - px * qz, az = px * qy - py * qx;
            float sine = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            float cosine = Math.Max(-1f, Math.Min(1f, px * qx + py * qy + pz * qz));
            if (sine < 1e-6f)
            {
                if (cosine > 0) return new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
                // Opposite: half turn about an axis perpendicular to p.
                if (Math.Abs(pz) < .9f) { ax = py; ay = -px; az = 0; } else { ax = 0; ay = pz; az = -py; }
                Normalize(ref ax, ref ay, ref az, 1, 0, 0);
                sine = 0;
            }
            else { ax /= sine; ay /= sine; az /= sine; }
            float angle = (float)Math.Atan2(sine, cosine), s = (float)Math.Sin(angle), c = (float)Math.Cos(angle), t = 1 - c;
            return new[]
            {
                c + ax * ax * t, ay * ax * t + az * s, az * ax * t - ay * s,
                ax * ay * t - az * s, c + ay * ay * t, az * ay * t + ax * s,
                ax * az * t + ay * s, ay * az * t - ax * s, c + az * az * t
            };
        }

        // Paired limb chains and the torso points (origin, top, front, joint) that tell
        // their true side apart. Mirrors CharacterPipeline.PAIR_GROUPS / unswap_pairs.
        private static readonly string[][] PairChains =
        {
            new[] { "NShoulder", "NElbow", "NWrist", "NFingertips", "NKnuckles", "NKnucklesS" },
            new[] { "NHip", "NKnee", "NAnkle", "NHeel", "NToeTip" }
        };
        private static readonly string[][] PairTests =
        {
            new[] { "NChest", "NNeck", "NChestF", "NShoulder" },
            new[] { "NPivot", "NStomach", "NPelvisF", "NHip" }
        };
        // Outside this dead band the side test decides; inside it the last choice holds,
        // so a chain cannot flicker while a pose passes through edge-on.
        private const float SwapDeadBand = .15f;
        private readonly bool[] _swapped = new bool[2];

        // SF2 keeps _1 on the up x front side of the torso, but some clips are authored
        // with left/right labels reversed and MirrorNodes swaps pairs at runtime. The
        // symmetric native rig does not care; a 3D skin would hang a limb's mesh on the
        // opposite limb. Swap each reversed chain back before posing.
        private void UnswapPairs()
        {
            for (int g = 0; g < PairChains.Length; g++)
            {
                var test = PairTests[g];
                float side = LimbSide(I(test[0]), I(test[1]), I(test[2]), I(test[3] + "_1"), I(test[3] + "_2"));
                if (side < -SwapDeadBand) _swapped[g] = true;
                else if (side > SwapDeadBand || !_updated) _swapped[g] = side < 0;
                if (!_swapped[g]) continue;
                foreach (string joint in PairChains[g])
                {
                    if (!_index.TryGetValue(joint + "_1", out int a) || !_index.TryGetValue(joint + "_2", out int b)) continue;
                    (_nx[a], _nx[b]) = (_nx[b], _nx[a]);
                    (_ny[a], _ny[b]) = (_ny[b], _ny[a]);
                    (_nz[a], _nz[b]) = (_nz[b], _nz[a]);
                }
            }
        }

        // Normalized (up x front) . (left - right) in canonical coordinates.
        private float LimbSide(int origin, int top, int front, int left, int right)
        {
            Canonical(_nx, _ny, _nz, origin, out float ox, out float oy, out float oz);
            Canonical(_nx, _ny, _nz, top, out float tx, out float ty, out float tz);
            Canonical(_nx, _ny, _nz, front, out float fx, out float fy, out float fz);
            Canonical(_nx, _ny, _nz, left, out float lx, out float ly, out float lz);
            Canonical(_nx, _ny, _nz, right, out float rx, out float ry, out float rz);
            float ux = tx - ox, uy = ty - oy, uz = tz - oz; Normalize(ref ux, ref uy, ref uz, 0, 1, 0);
            fx -= ox; fy -= oy; fz -= oz;
            float d = fx * ux + fy * uy + fz * uz; fx -= ux * d; fy -= uy * d; fz -= uz * d;
            Normalize(ref fx, ref fy, ref fz, 0, 0, 1);
            float cx = uy * fz - uz * fy, cy = uz * fx - ux * fz, cz = ux * fy - uy * fx;
            float sx = lx - rx, sy = ly - ry, sz = lz - rz; Normalize(ref sx, ref sy, ref sz, 0, 0, 0);
            return cx * sx + cy * sy + cz * sz;
        }

        private float Distance(int a, int b)
        {
            float x = _nx[b] - _nx[a], y = _ny[b] - _ny[a], z = _nz[b] - _nz[a];
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        private void Direction(int from, int to, out float x, out float y, out float z)
        {
            x = _nx[to] - _nx[from]; y = _ny[to] - _ny[from]; z = _nz[to] - _nz[from];
            float length = (float)Math.Sqrt(x * x + y * y + z * z);
            if (length > 1e-6f) { x /= length; y /= length; z /= length; } else x = y = z = 0;
        }

        private void Set(int i, float x, float y, float z) { _x[i] = x; _y[i] = y; _z[i] = z; }
        private void Pin(string name) { int i = I(name); Set(i, _nx[i], _ny[i], _nz[i]); }

        // Visual child = visual parent + native direction (parent→child) × proportioned length.
        private void Follow(string parent, string child, string from)
        {
            int p = I(parent), c = I(child), f = I(from);
            Direction(f, c, out float x, out float y, out float z);
            float length = L(parent, child);
            Set(c, _x[p] + x * length, _y[p] + y * length, _z[p] + z * length);
        }

        // Share of limb length given to the anatomical bend hint (CharacterPipeline.BEND_HINT_SHARE).
        private const float BendHintShare = .02f;

        // Two-bone IK for the visual elbow/knee. The bend side is the native limb's own
        // bend measured against the native root-end line (not the visual line, which a
        // shifted visual shoulder/hip can put on the far side of the native joint and
        // flip it backward). A nearly straight native limb follows the anatomical hint
        // from hintFrom toward hintTo. Mirrors CharacterPipeline._two_bone.
        private void Bend(string rootName, string midName, string endName, string hintFrom, string hintTo)
        {
            int r = I(rootName), m = I(midName), e = I(endName);
            float upper = L(rootName, midName), lower = L(midName, endName);
            float ax = _x[e] - _x[r], ay = _y[e] - _y[r], az = _z[e] - _z[r];
            float distance = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            if (distance < 1e-6f)
            {
                Direction(r, m, out float px, out float py, out float pz);
                Set(m, _x[r] + px * upper, _y[r] + py * upper, _z[r] + pz * upper);
                return;
            }
            float ux = ax / distance, uy = ay / distance, uz = az / distance;
            if (distance >= upper + lower)
            {
                float t = distance * upper / (upper + lower); // out of reach: straight, stretched
                Set(m, _x[r] + ux * t, _y[r] + uy * t, _z[r] + uz * t);
                return;
            }
            distance = Math.Max(distance, Math.Abs(upper - lower) + 1e-4f);
            float along = (upper * upper - lower * lower + distance * distance) / (2 * distance);
            float height = (float)Math.Sqrt(Math.Max(0, upper * upper - along * along));
            Direction(r, e, out float lx, out float ly, out float lz); // native root-end line
            float bx = _nx[m] - _nx[r], by = _ny[m] - _ny[r], bz = _nz[m] - _nz[r];
            float dot = bx * lx + by * ly + bz * lz;
            bx -= lx * dot; by -= ly * dot; bz -= lz * dot;
            Direction(I(hintFrom), I(hintTo), out float hx, out float hy, out float hz);
            float share = BendHintShare * (upper + lower);
            float sx = bx + hx * share, sy = by + hy * share, sz = bz + hz * share;
            dot = sx * ux + sy * uy + sz * uz;
            sx -= ux * dot; sy -= uy * dot; sz -= uz * dot;
            float side = (float)Math.Sqrt(sx * sx + sy * sy + sz * sz);
            if (side < 1e-6f)
            {
                dot = hx * ux + hy * uy + hz * uz;
                sx = hx - ux * dot; sy = hy - uy * dot; sz = hz - uz * dot;
                side = (float)Math.Sqrt(sx * sx + sy * sy + sz * sz);
            }
            if (side < 1e-6f) { sx = -uy; sy = ux; sz = 0; side = (float)Math.Sqrt(sx * sx + sy * sy); }
            if (side < 1e-6f) { sx = 1; sy = 0; sz = 0; side = 1; }
            Set(m, _x[r] + ux * along + sx / side * height, _y[r] + uy * along + sy / side * height,
                _z[r] + uz * along + sz / side * height);
        }
    }
}
