using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Eclipse.Modding
{
    // Authored body/skin preflight. Recovered equipment keeps its first-definition
    // binding semantics; only explicitly authored body/skin documents are strict.
    internal static class ModCharacterGeometry
    {
        internal static void Validate(IReadOnlyList<string> paths, IReadOnlyList<XmlDocument> documents, ISet<string> authored)
        {
            if (paths.Count != documents.Count) throw new ArgumentException("Model path/document counts differ.");
            if (authored.Count == 0) return;
            var nodes = new Dictionary<string, double>(StringComparer.Ordinal);
            var edges = new HashSet<string>(StringComparer.Ordinal);
            int expandedEdges = 0;
            for (int index = 0; index < paths.Count; index++)
            {
                string path = paths[index]; bool strict = authored.Contains(path);
                var scene = documents[index]?.DocumentElement;
                if (strict && (scene == null || scene.Name != "Scene" || scene["Figures"] == null))
                    Fail(path, "/Scene", "requires Scene and Figures");
                if (scene == null || scene.Name != "Scene") continue;
                foreach (XmlNode value in scene["Nodes"]?.ChildNodes ?? EmptyNodes())
                {
                    if (!(value is XmlElement node)) continue;
                    string where = "/Scene/Nodes/" + node.Name;
                    string kind = node.GetAttribute("Type");
                    if (strict && nodes.ContainsKey(node.Name)) Fail(path, where, "duplicate node; use a unique authored binding");
                    if (nodes.ContainsKey(node.Name)) continue; // native recovered first-wins rule
                    if (strict && kind != "Node" && kind != "MacroNode" && kind != "CenterOfMass" && kind != "SkinnedNode")
                        Fail(path, where + " @Type", "must be Node, MacroNode, CenterOfMass or SkinnedNode");
                    if (kind != "Node" && kind != "MacroNode" && kind != "CenterOfMass" && kind != "SkinnedNode") continue;
                    double mass = Number(node, "Mass", path, where, strict);
                    foreach (string axis in new[] { "X", "Y", "Z" }) Number(node, axis, path, where, strict);
                    if (strict && mass < 0) Fail(path, where + " @Mass", "cannot be negative");
                    if (strict && kind == "SkinnedNode")
                    {
                        int count = Integer(node, "BonesCount", 0, path, where, 1, 16);
                        double total = 0;
                        for (int bone = 1; bone <= count; bone++)
                        {
                            string suffix = bone.ToString(CultureInfo.InvariantCulture);
                            RequireNode(node, "BoneStart" + suffix, nodes, path, where);
                            RequireNode(node, "BoneEnd" + suffix, nodes, path, where);
                            if (node.GetAttribute("BoneStart" + suffix) == node.GetAttribute("BoneEnd" + suffix))
                                Fail(path, where, "skin bone endpoints must differ");
                            double weight = Number(node, "Weight" + suffix, path, where, true, required: true);
                            if (weight < 0 || weight > 1) Fail(path, where + " @Weight" + suffix, "must be in 0..1");
                            total += weight;
                            if (node.HasAttribute("LocalX" + suffix))
                            {
                                // 3D binding: a source-rest point in a posed segment frame.
                                foreach (string field in new[] { "Along", "Across", "Offset", "Extend" })
                                    if (node.HasAttribute(field + suffix)) Fail(path, where + " @" + field + suffix, "cannot be combined with LocalX" + suffix);
                                var frameKey = (node.GetAttribute("BoneStart" + suffix), node.GetAttribute("BoneEnd" + suffix));
                                if (scene["Proportions"] == null || scene["Rest"] == null || !new List<(string, string)>(SkinProportionRig.FrameKeys()).Contains(frameKey))
                                    Fail(path, where, "3D binding " + suffix + " needs Proportions, Rest and a supported segment");
                                foreach (string axis in new[] { "X", "Y", "Z" })
                                    if (Math.Abs(Number(node, "Local" + axis + suffix, path, where, true, required: true)) > 1000)
                                        Fail(path, where + " @Local" + axis + suffix, "must be in -1000..1000");
                                if (node.HasAttribute("Curl" + suffix) || node.HasAttribute("Pivot" + suffix))
                                    ValidateFinger(node, suffix, frameKey.Item1, path, where);
                                continue;
                            }
                            if (Math.Abs(Number(node, "Along" + suffix, path, where, true, required: true)) > 100)
                                Fail(path, where + " @Along" + suffix, "must be in -100..100");
                            // Relative bindings use Across; absolute bindings use Offset and optional Extend.
                            bool absolute = node.HasAttribute("Offset" + suffix);
                            if (absolute == node.HasAttribute("Across" + suffix))
                                Fail(path, where, "binding " + suffix + " needs exactly one of Across or Offset");
                            if (!absolute && node.HasAttribute("Extend" + suffix))
                                Fail(path, where + " @Extend" + suffix, "requires Offset" + suffix);
                            if (!absolute && Math.Abs(Number(node, "Across" + suffix, path, where, true, required: true)) > 100)
                                Fail(path, where + " @Across" + suffix, "must be in -100..100");
                            if (absolute)
                                foreach (string field in new[] { "Offset", "Extend" })
                                    if (Math.Abs(Number(node, field + suffix, path, where, true, required: field == "Offset")) > 1000)
                                        Fail(path, where + " @" + field + suffix, "must be in -1000..1000");
                        }
                        if (Math.Abs(total - 1) > .0001) Fail(path, where, "skin weights must sum to 1");
                    }
                    if (strict && (kind == "MacroNode" || kind == "CenterOfMass"))
                    {
                        int count = Integer(node, "NodesCount", 0, path, where, 1, 128);
                        double total = 0;
                        for (int child = 1; child <= count; child++)
                        {
                            string suffix = child.ToString(CultureInfo.InvariantCulture);
                            string dependency = node.GetAttribute("ChildNode" + suffix);
                            if (!nodes.TryGetValue(dependency, out double childMass))
                                Fail(path, where + " @ChildNode" + suffix, "unresolved or forward binding '" + dependency + "'; dependencies must appear earlier");
                            total += childMass;
                            if (kind == "MacroNode") Number(node, "LCC" + suffix, path, where, true, required: true);
                        }
                        if (kind == "CenterOfMass" && total <= 0) Fail(path, where, "center of mass dependencies need positive total mass");
                    }
                    nodes[node.Name] = mass;
                }
                foreach (XmlNode value in scene["Edges"]?.ChildNodes ?? EmptyNodes())
                {
                    if (!(value is XmlElement edge)) continue;
                    string where = "/Scene/Edges/" + edge.Name;
                    string edgeKind = edge.GetAttribute("Type");
                    if (!strict && edgeKind != "Edge" && edgeKind != "Muscle") continue;
                    int iterations = strict ? Integer(edge, "Iterations", 1, path, where, 1, 32) : LegacyIterations(edge);
                    if (strict)
                    {
                        string kind = edge.GetAttribute("Type");
                        if (kind != "Edge" && kind != "Muscle") Fail(path, where + " @Type", "must be Edge or Muscle");
                        RequireNode(edge, "End1", nodes, path, where); RequireNode(edge, "End2", nodes, path, where);
                        foreach (string field in new[] { "Length", "Radius", "Margin1", "Margin2" })
                        {
                            double number = Number(edge, field, path, where, true);
                            if ((field == "Length" || field == "Radius") && number < 0) Fail(path, where + " @" + field, "cannot be negative");
                        }
                    }
                    for (int copy = 0; copy < iterations; copy++)
                    {
                        string name = edge.Name + (copy == 0 ? "" : "CI" + copy.ToString(CultureInfo.InvariantCulture));
                        if (strict && !edges.Add(name)) Fail(path, where, "duplicate expanded edge '" + name + "'");
                        if (!strict) edges.Add(name);
                        expandedEdges++;
                    }
                }
                if (!strict) continue;
                var figures = new HashSet<string>(StringComparer.Ordinal);
                foreach (XmlNode value in scene["Figures"].ChildNodes)
                {
                    if (!(value is XmlElement figure)) continue;
                    string where = "/Scene/Figures/" + figure.Name;
                    if (!figures.Add(figure.Name)) Fail(path, where, "duplicate figure");
                    string kind = figure.GetAttribute("Type");
                    if (kind == "Triangle")
                    {
                        foreach (string field in new[] { "Node1", "Node2", "Node3" }) RequireNode(figure, field, nodes, path, where);
                    }
                    else if (kind == "Capsule")
                    {
                        if (!edges.Contains(figure.GetAttribute("Edge"))) Fail(path, where + " @Edge", "unresolved edge '" + figure.GetAttribute("Edge") + "'");
                        foreach (string field in new[] { "Radius1", "Radius2", "Margin1", "Margin2" })
                        {
                            double number = Number(figure, field, path, where, true);
                            if (field.StartsWith("Radius", StringComparison.Ordinal) && number < 0) Fail(path, where + " @" + field, "cannot be negative");
                        }
                    }
                    else Fail(path, where + " @Type", "must be Triangle or Capsule");
                }
                if (nodes.Count == 0) Fail(path, "/Scene/Nodes", "composed model needs at least one node");
                if (scene["Proportions"] != null) ValidateProportions(scene["Proportions"], nodes, path);
                if (scene["Rest"] != null) ValidateRest(scene["Rest"], nodes, path);
            }
            if (nodes.Count > 4096 || expandedEdges > 8192)
                Fail(paths.Count == 0 ? "<composition>" : paths[paths.Count - 1], "/Scene", "composed authored character exceeds 4096 nodes or 8192 expanded edges");
        }
        // Visual proportion rig for a skin: every supported segment exactly once.
        private static void ValidateProportions(XmlElement element, Dictionary<string, double> nodes, string path)
        {
            const string where = "/Scene/Proportions";
            if (element.GetAttribute("Version") != "1") Fail(path, where + " @Version", "must be 1");
            var required = new HashSet<(string, string)>(SkinProportionRig.Segments());
            var seen = new HashSet<(string, string)>();
            foreach (XmlNode value in element.ChildNodes)
            {
                if (!(value is XmlElement segment)) continue;
                var key = (segment.GetAttribute("Start"), segment.GetAttribute("End"));
                string at = where + "/" + segment.Name + "[" + key.Item1 + "-" + key.Item2 + "]";
                if (segment.Name != "Segment" || !required.Contains(key) || !seen.Add(key))
                    Fail(path, at, "must be one unique supported Segment");
                double length = Number(segment, "Length", path, at, true, required: true);
                if (length < .01 || length > 1000) Fail(path, at + " @Length", "must be in 0.01..1000");
            }
            foreach (var key in required)
            {
                if (!seen.Contains(key)) Fail(path, where, "missing Segment " + key.Item1 + "-" + key.Item2);
                if (!nodes.ContainsKey(key.Item1) || !nodes.ContainsKey(key.Item2))
                    Fail(path, where, "needs native points " + key.Item1 + " and " + key.Item2 + " in the body");
            }
            foreach (string hint in new[] { "NChestF", "NPelvisF" }) // elbow/knee bend hints
                if (!nodes.ContainsKey(hint)) Fail(path, where, "needs native point " + hint + " in the body");
        }

        // Finger phalanx binding: Curl 1..3 on a hand segment, one x,y,z pivot per level.
        private static double[] Triple(XmlElement element, string field, string path, string where, double limit)
        {
            var parts = element.GetAttribute(field).Split(',');
            var values = new double[3];
            if (parts.Length != 3) Fail(path, where + " @" + field, "must be x,y,z");
            for (int i = 0; i < 3; i++)
                if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]) ||
                    double.IsNaN(values[i]) || Math.Abs(values[i]) > limit)
                    Fail(path, where + " @" + field, "values must be finite within -" + limit + ".." + limit);
            return values;
        }

        private static void ValidateFinger(XmlElement node, string suffix, string start, string path, string where)
        {
            string curl = node.GetAttribute("Curl" + suffix);
            if (!start.StartsWith("NWrist_", StringComparison.Ordinal) || (curl != "1" && curl != "2" && curl != "3"))
                Fail(path, where + " @Curl" + suffix, "must be 1..3 on a hand (NWrist-NFingertips) binding");
            var points = node.GetAttribute("Pivot" + suffix).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (points.Length != curl[0] - '0') Fail(path, where + " @Pivot" + suffix, "needs one x,y,z point per Curl level");
            foreach (string point in points)
            {
                var values = point.Split(',');
                if (values.Length != 3) Fail(path, where + " @Pivot" + suffix, "entries must be x,y,z");
                foreach (string value in values)
                    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) ||
                        double.IsNaN(number) || Math.Abs(number) > 1000)
                        Fail(path, where + " @Pivot" + suffix, "values must be finite within -1000..1000");
            }
        }

        // Limb rest directions (unit vectors in the parent frame) for 3D skin bindings.
        private static void ValidateRest(XmlElement element, Dictionary<string, double> nodes, string path)
        {
            const string where = "/Scene/Rest";
            if (element.GetAttribute("Version") != "1") Fail(path, where + " @Version", "must be 1");
            var required = new HashSet<(string, string)>();
            foreach (var (child, _) in SkinProportionRig.LimbParents()) required.Add(child);
            var seen = new HashSet<(string, string)>();
            int springs = 0;
            foreach (XmlNode value in element.ChildNodes)
            {
                if (!(value is XmlElement bone)) continue;
                if (bone.Name == "Dynamic")
                {   // spring bone: earlier parent, finite head, unit direction, sane length/coefficients
                    string at2 = where + "/Dynamic[" + springs + "]";
                    if (bone.GetAttribute("Id") != springs.ToString(CultureInfo.InvariantCulture)) Fail(path, at2 + " @Id", "must count up from 0");
                    string parent = bone.GetAttribute("Parent");
                    bool earlier = parent.StartsWith("#", StringComparison.Ordinal) &&
                        int.TryParse(parent.Substring(1), out int parentSpring) && parentSpring >= 0 && parentSpring < springs;
                    int dash = parent.IndexOf('-');
                    bool native = dash > 0 && new List<(string, string)>(SkinProportionRig.FrameKeys()).Contains((parent.Substring(0, dash), parent.Substring(dash + 1)));
                    if (!earlier && !native) Fail(path, at2 + " @Parent", "must be a supported segment or an earlier #spring");
                    var head = Triple(bone, "Head", path, at2, 1000);
                    var direction = Triple(bone, "Dir", path, at2, 1000);
                    if (Math.Abs(Math.Sqrt(direction[0] * direction[0] + direction[1] * direction[1] + direction[2] * direction[2]) - 1) > .01)
                        Fail(path, at2 + " @Dir", "must be a unit vector");
                    double length = Number(bone, "Length", path, at2, true, required: true);
                    double stiffness = Number(bone, "Stiffness", path, at2, true), damping = Number(bone, "Damping", path, at2, true);
                    if (length < .01 || length > 1000 || stiffness < 0 || stiffness > 1 || damping < 0 || damping > 1)
                        Fail(path, at2, "Length 0.01..1000, Stiffness and Damping 0..1");
                    springs++;
                    continue;
                }
                if (bone.Name == "Frame")
                {   // source rest frame: origin and two orthonormal axes
                    string at2 = where + "/Frame";
                    Triple(bone, "O", path, at2, 100000);
                    var a = Triple(bone, "A", path, at2, 1.01); var b = Triple(bone, "B", path, at2, 1.01);
                    double la = Math.Sqrt(a[0] * a[0] + a[1] * a[1] + a[2] * a[2]), lb = Math.Sqrt(b[0] * b[0] + b[1] * b[1] + b[2] * b[2]);
                    if (Math.Abs(la - 1) > .01 || Math.Abs(lb - 1) > .01 || Math.Abs(a[0] * b[0] + a[1] * b[1] + a[2] * b[2]) > .01)
                        Fail(path, at2, "axes must be orthonormal");
                    if (bone.HasAttribute("Dynamic"))
                    {
                        if (!int.TryParse(bone.GetAttribute("Dynamic"), out int spring) || spring < 0 || spring >= springs)
                            Fail(path, at2 + " @Dynamic", "must name an earlier Dynamic");
                    }
                    else if (!new List<(string, string)>(SkinProportionRig.FrameKeys()).Contains((bone.GetAttribute("Start"), bone.GetAttribute("End"))))
                        Fail(path, at2, "must name a supported segment");
                    continue;
                }
                var key = (bone.GetAttribute("Start"), bone.GetAttribute("End"));
                string at = where + "/" + bone.Name + "[" + key.Item1 + "-" + key.Item2 + "]";
                if (bone.HasAttribute("Twist"))
                {
                    double twist = Number(bone, "Twist", path, at, true, required: true);
                    if (!key.Item2.StartsWith("NFingertips_", StringComparison.Ordinal) || twist < -180 || twist > 180)
                        Fail(path, at + " @Twist", "is a hand bone angle in -180..180");
                }
                if (bone.Name != "Bone" || !required.Contains(key) || !seen.Add(key)) Fail(path, at, "must be one unique supported Bone");
                double x = Number(bone, "X", path, at, true, required: true), y = Number(bone, "Y", path, at, true, required: true),
                    z = Number(bone, "Z", path, at, true, required: true);
                if (Math.Abs(Math.Sqrt(x * x + y * y + z * z) - 1) > .01) Fail(path, at, "direction must be a unit vector");
                if (bone.HasAttribute("Curl"))
                {
                    double curl = Number(bone, "Curl", path, at, true, required: true);
                    if (!key.Item2.StartsWith("NFingertips_", StringComparison.Ordinal) || curl < 0 || curl > 180)
                        Fail(path, at + " @Curl", "is a hand bone angle in 0..180");
                }
            }
            foreach (var key in required) if (!seen.Contains(key)) Fail(path, where, "missing Bone " + key.Item1 + "-" + key.Item2);
            foreach (string helper in new[] { "NPelvisF", "NStomachF", "NChestF", "NHeadF", "NKnuckles_1", "NKnuckles_2", "NKnucklesS_1", "NKnucklesS_2" })
                if (!nodes.ContainsKey(helper)) Fail(path, where, "needs native front helper " + helper + " in the body");
            if (element.ParentNode?["Proportions"] == null) Fail(path, where, "requires Proportions");
        }

        private static XmlNodeList EmptyNodes() => new XmlDocument().ChildNodes;
        private static int LegacyIterations(XmlElement edge) => int.TryParse(edge.GetAttribute("Iterations"), out var value) ? Math.Max(0, Math.Min(8193, value)) : 1;
        private static int Integer(XmlElement value, string field, int fallback, string path, string where, int min, int max)
        {
            string text = value.GetAttribute(field);
            int number = fallback;
            if (text.Length != 0 && !int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out number) || number < min || number > max)
                Fail(path, where + " @" + field, "must be an integer in " + min + ".." + max);
            return number;
        }
        private static double Number(XmlElement value, string field, string path, string where, bool strict, bool required = false)
        {
            string text = value.GetAttribute(field);
            if (text.Length == 0 && !required) return 0;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) ||
                double.IsNaN(number) || double.IsInfinity(number) || Math.Abs(number) > 100000)
            {
                if (strict) Fail(path, where + " @" + field, "must be a finite number in -100000..100000");
                return 0;
            }
            return number;
        }
        private static void RequireNode(XmlElement value, string field, Dictionary<string, double> nodes, string path, string where)
        {
            string name = value.GetAttribute(field);
            if (!nodes.ContainsKey(name)) Fail(path, where + " @" + field, "unresolved node '" + name + "'");
        }
        private static void Fail(string path, string where, string message) => throw new InvalidDataException("Authored character model '" + path + "' " + where + ": " + message + ".");
    }
}
