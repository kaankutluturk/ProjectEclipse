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
                            foreach (string field in new[] { "Along", "Across" })
                                if (Math.Abs(Number(node, field + suffix, path, where, true, required: true)) > 100)
                                    Fail(path, where + " @" + field + suffix, "must be in -100..100");
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
            }
            if (nodes.Count > 4096 || expandedEdges > 8192)
                Fail(paths.Count == 0 ? "<composition>" : paths[paths.Count - 1], "/Scene", "composed authored character exceeds 4096 nodes or 8192 expanded edges");
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
