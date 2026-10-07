using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Eclipse.Modding
{
    /// <summary>
    /// Reads the base-game values the Moveset Lab writes as guards: every move with all mod
    /// move edits (patches, forks, item lock edits) removed for the duration of the read.
    /// Replacements stay, matching what runtime patch guards compare against.
    /// </summary>
    internal static class MovesetBaselineReader
    {
        public static Dictionary<string, MovesetBaselineMove> ReadAll()
        {
            var result = new Dictionary<string, MovesetBaselineMove>(StringComparer.Ordinal);
            void Read()
            {
                foreach (var move in AnimationData.Animations)
                    if (move != null && !string.IsNullOrEmpty(move.Name) && move.MoveData != null && !result.ContainsKey(move.Name))
                        result.Add(move.Name, Describe(move));
            }
            ModRuntime.WithoutMoveOverlay(Read);
            return result;
        }

        private static MovesetBaselineMove Describe(InfoAnimation move)
        {
            var baseline = new MovesetBaselineMove
            {
                Name = move.Name, File = move.FileName ?? string.Empty, Priority = move.Priority, MidFrames = move.MidFrames,
                FrameCount = move.GetFrameCount(), FirstFrame = move.FirstFrame, Looped = move.GetIsLooped(), Physics = move.HasPhysics
            };
            foreach (string name in move.GetTemplateNames()) if (name != move.Name) baseline.Templates.Add(name);
            foreach (var condition in move.MoveData.Locks)
            {
                if (condition is ConditionItemInfo item && !item.IsNot)
                {
                    if (item.get_Type() == "Weapon" && !string.IsNullOrEmpty(item.GetSubType()) && string.IsNullOrEmpty(item.get_Name()))
                        baseline.WeaponGroups.Add(new List<string> { item.GetSubType() });
                    else if (item.get_Type() == "Weapon" && !string.IsNullOrEmpty(item.get_Name())) baseline.WeaponItems.Add(item.get_Name());
                    else if (item.get_Type() == "Skeleton" && item.GetSubType() == "Skeleton") baseline.PlayerSkeleton = true;
                }
                else if (condition is ConditionList list && !list.IsNot && list.get_Type() == ConditionList.OperatorType.OR)
                {
                    var group = new List<string>();
                    foreach (var child in list.GetConditions())
                        if (child is ConditionItemInfo option && !option.IsNot && option.get_Type() == "Weapon" &&
                            !string.IsNullOrEmpty(option.GetSubType()) && string.IsNullOrEmpty(option.get_Name())) group.Add(option.GetSubType());
                    if (group.Count != 0) baseline.WeaponGroups.Add(group);
                }
            }
            foreach (var interval in move.MoveData.Intervals) baseline.Intervals.Add(DescribeInterval(interval));
            return baseline;
        }

        private static string Attribute(XmlNode node, string name) => node?.Attributes?[name]?.Value;
        private static int Integer(XmlNode node, string name, int fallback) =>
            int.TryParse(Attribute(node, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;
        private static double Number(XmlNode node, string name) =>
            float.TryParse(Attribute(node, name), NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? Clean(value) : 0d;
        // Game values are floats; keep their shortest form (0.08, not 0.0799999982) in saved guards.
        private static double Clean(float value) => double.Parse(value.ToString("R", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

        private static MovesetBaselineInterval DescribeInterval(IntervalAnimation interval)
        {
            var node = interval.NodeInterval;
            var result = node != null
                ? new MovesetBaselineInterval { Type = Attribute(node, "Type") ?? string.Empty, Name = Attribute(node, "Name") ?? string.Empty,
                    Start = Integer(node, "Start", 0), End = Attribute(node, "End") == null ? (int?)null : Integer(node, "End", -1) }
                : new MovesetBaselineInterval { Type = interval.AuthoredType ?? string.Empty, Name = interval.Name ?? string.Empty,
                    Start = interval.Start, End = interval.HasAuthoredEnd ? interval.EndFrame : (int?)null };
            if (!(interval is IntervalAttack attack)) return result;
            var info = new MovesetBaselineAttack();
            if (node != null)
            {
                info.Id = Integer(node, "ID", -1);
                var damage = node["Damage"];
                info.Damage = Number(damage, "Value");
                if (damage != null)
                    foreach (XmlNode term in damage.ChildNodes)
                        if (term.Name == "Damage") info.Terms[Attribute(term, "Type") ?? string.Empty] = Number(term, "Shift");
                var parts = node["AttackingParts"];
                if (parts != null) foreach (XmlNode edge in parts.ChildNodes) info.Edges.Add(Attribute(edge, "Name") ?? string.Empty);
                var impulse = node["Impulse"];
                info.Impulse = new[] { Number(impulse, "X"), Number(impulse, "Y"), Number(impulse, "Z") };
                var hits = node.SelectNodes("Hit");
                bool partial = false;
                foreach (XmlNode hit in hits) partial |= Attribute(hit, "Start") != null || Attribute(hit, "End") != null;
                info.HasPartialHits = partial || hits.Count != 1;
                info.Hit = !info.HasPartialHits ? Attribute(hits[0], "Name") : null;
            }
            else
            {
                info.Id = attack.GetAnimationId();
                info.Damage = Clean(attack.GetDamage());
                foreach (var term in attack.GetDamageAttributes()) info.Terms[term.First] = Clean(term.Second);
                info.Edges.AddRange(attack.GetAttackingParts());
                var impulse = attack.GetImpulse();
                info.Impulse = new double[] { Clean(impulse.GetX()), Clean(impulse.GetY()), Clean(impulse.GetZ()) };
                var reactions = attack.HitReactions;
                info.HasPartialHits = reactions.Count != 1 || reactions[0].Start != attack.Start || reactions[0].EndFrameValue != attack.EndFrame;
                info.Hit = !info.HasPartialHits ? reactions[0].Name : null;
            }
            result.Attack = info;
            return result;
        }
    }
}
