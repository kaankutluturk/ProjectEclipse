using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;

#pragma warning disable SYSLIB0050 // Construct a probe without the game's native model constructor.

public static class ThrowRuntimeFixture
{
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic;
    private static int checks;

    // Keep the real named-animation/action path, replacing only model state gates,
    // collision reset and fight events that require a fully constructed game.
    public sealed class Fighter : Model
    {
        private Fighter() : base(null) { }
        public int PlayedSign;
        public bool RefuseAnimation;
        public override bool PlayAnimation(InfoAnimation move, int sign = 0,
            bool frameShift = false, int startFrame = -1)
        {
            if (RefuseAnimation) return false;
            PlayedSign = sign == 0 ? GetFacingSign() : sign;
            return GetAnimationModule().PlayInfo(move, PlayedSign, true, frameShift, startFrame);
        }
    }

    private sealed class LogHandler : ILogHandler
    {
        public void LogFormat(LogType type, UnityEngine.Object context, string format, params object[] args)
        {
            if (type == LogType.Error || type == LogType.Exception)
                throw new Exception(string.Format(format, args));
        }
        public void LogException(Exception exception, UnityEngine.Object context) { throw exception; }
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        checks++;
    }

    private static void Set(object target, Type type, string field, object value)
    {
        type.GetField(field, Instance).SetValue(target, value);
    }

    private static InfoAnimation LoadMove(XmlDocument xml, string name, string root)
    {
        XmlElement source = (XmlElement)xml.SelectSingleNode("/Movesxml/Moves/Move[@Name='" + name + "']");
        var selected = xml.CreateElement("Moves");
        var copy = (XmlElement)source.CloneNode(true);
        copy.SetAttribute("FileName", ""); // Load actual bytes below, without Resources/native APIs.
        selected.AppendChild(copy);
        var templates = new Dictionary<string, TemplateAnimation>();
        typeof(MovesParser).GetMethod("ParseTemplates", Static).Invoke(null,
            new object[] { xml.DocumentElement["Templates"], templates });
        var moves = new List<InfoAnimation>();
        typeof(MovesParser).GetMethod("ParseMoves", Static).Invoke(null,
            new object[] { selected, templates, moves, new List<Trick>() });
        InfoAnimation move = moves[0];
        move.FileName = source.GetAttribute("FileName");
        typeof(InfoAnimation).GetMethod("ReadAnimation", Instance).Invoke(move,
            new object[] { File.ReadAllBytes(Path.Combine(root,
                "Assets/Resources/gamedata/animations/binary", move.FileName)) });
        // The parser already initialized intervals (and released their XML).
        // Native loading was skipped, so fill only the clip's node count here.
        Set(move, typeof(InfoAnimation), "_NodesCount", move.GetAnimationFrames()[0].Length);
        return move;
    }

    private static Fighter MakeFighter(InfoAnimation move, XmlDocument skeleton, int sign, float x)
    {
        var fighter = (Fighter)FormatterServices.GetUninitializedObject(typeof(Fighter));
        var model = new ModelObject();
        model.SetModel(fighter);
        Set(fighter, typeof(Model), "_ModelObject", model);
        Set(fighter, typeof(Model), "_Physics", new ModelPhysics(model));
        Set(fighter, typeof(Model), "_ModelConditions", new ModelConditions());
        Set(fighter, typeof(Model), "availableAnimations", new List<InfoAnimation> { move });
        // Binary clips index the recovered skeleton's nodes; use the real names
        // and pairings, with an initial pose near the requested arena position.
        Vector3[] pose = move.GetAnimationFrames()[move.FirstFrame];
        XmlNodeList definitions = skeleton.SelectNodes("/Scene/Nodes/*");
        float offset = x - sign * pose[18].x; // NPivot, verified below.
        for (int i = 0; i < pose.Length; i++)
        {
            string name = i < definitions.Count ? definitions[i].Name : "Extra" + i;
            Vector3 p = pose[i];
            var node = new ModelNode(name, new Vector3f(sign * p.x + offset, p.y, p.z));
            node.SetID(i);
            model.GetAllNodes().Add(node);
            model.GetPlainNodes().Add(node);
            model.GetNodesByName().Add(name, node);
        }
        Check(model.GetNodeByName("NPivot").GetID() == 18, "Skeleton pivot index changed");
        foreach (ModelNode node in model.GetAllNodes())
        {
            string name = node.GetName();
            if (!name.EndsWith("_1")) continue;
            ModelNode pair = model.GetNodeByName(name.Substring(0, name.Length - 1) + "2");
            if (pair == null) continue;
            node.SetPairNode(pair);
            pair.SetPairNode(node);
            model.GetPairNodeIds().Add(new Pair<int, int>(node.GetID(), pair.GetID()));
        }
        var animation = new ModelAnimation(model);
        animation.set_Sign(sign);
        animation.SetAligns(-2000, 2000, 0, 0);
        Set(fighter, typeof(Model), "_Animation", animation);
        InfoAnimation.MovePivot align = move.MoveData.AlignData;
        if (align.PivotObjectType == InfoAnimation.AlignObjectType.ObjectNodes)
        {
            align.PivotNodeId = model.GetNodeIDByName(align.PivotPart);
            align.PivotPairNodeId = model.GetNodeIDByPairName(align.PivotNodeId);
            Set(animation, typeof(ModelAnimation), "currentNode", model.GetAllNodes()[align.PivotNodeId]);
        }
        return fighter;
    }

    private static void Link(Fighter fighter, Fighter enemy)
    {
        Set(fighter, typeof(Model), "combatTarget", enemy);
        fighter.GetAnimationModule().SetOtherAnimation(enemy.GetAnimationModule());
        ModelConditions conditions = fighter.GetConditions();
        conditions.AnimationSign = conditions.SelfSign = fighter.GetFacingSign();
        conditions.OtherSign = enemy.GetFacingSign();
        conditions.OtherPositions.LeftWall = new Vector2(-2000, 0);
        conditions.OtherPositions.RightWall = new Vector2(2000, 0);
    }

    private static float PivotX(Fighter fighter)
    {
        return fighter.GetBodyObject().GetNodeByName("NPivot").GetStart().GetX();
    }

    private static bool AllowsStrike(Fighter attacker, Fighter victim)
    {
        return (bool)typeof(Model).GetMethod("PairedGrabAllowsStrike", Instance)
            .Invoke(attacker, new object[] { victim });
    }

    public static void Run(string root, string skeletonText)
    {
        ILogHandler previousLog = Debug.unityLogger.logHandler;
        Debug.unityLogger.logHandler = new LogHandler();
        try
        {
            MovesMaps.Clear();
            MovesMaps.Init();
            var xml = new XmlDocument();
            xml.Load(Path.Combine(root, "Assets/vanillaXml/animations/moves.xml"));
            var skeleton = new XmlDocument();
            skeleton.LoadXml(skeletonText);
            foreach (string name in new[] { "ThrowForward", "ThrowThroughTheBack" })
            foreach (int sign in new[] { -1, 1 })
            foreach (float arenaX in new[] { -900f, 0f, 900f })
            foreach (int substeps in new[] { 1, 2, 4 })
            {
                GameUtils.SetSlowMode(substeps);
                InfoAnimation attack = LoadMove(xml, name, root);
                InfoAnimation victim = LoadMove(xml, name + "V", root);
                Fighter a = MakeFighter(attack, skeleton, sign, arenaX);
                Fighter v = MakeFighter(victim, skeleton, -sign, arenaX + sign * 80);
                Link(a, v);
                Link(v, a);
                // Let the attack's real frame event dispatch its PlayAnimation action.
                // Sound and other fight actions are outside this animation fixture.
                a.GetAnimationModule().AddEventListener(4, delegate(object data)
                {
                    foreach (ActionAnimation action in (List<ActionAnimation>)data)
                        if (action is ActionPlayAnimation) action.Visit(a);
                });
                Check(a.PlayAnimation(attack, sign), name + " attacker did not start");
                for (int tick = 0; tick < 32 * substeps && v.PlayedSign == 0; tick++)
                    a.GetAnimationModule().Render();
                Check(v.PlayedSign == sign, name + " victim ignored SetDirection: attacker=" + sign +
                    ", victim=" + v.PlayedSign);
                Check(Math.Abs(a.GetAnimationModule().Shift.GetX() -
                    v.GetAnimationModule().Shift.GetX()) < 0.01f, name + " origins differ");
                // Exercise all buffered poses, including interpolation and landing.
                for (int frame = 0; frame < 220 * substeps; frame++)
                {
                    a.GetAnimationModule().Render();
                    v.GetAnimationModule().Render();
                    float separation = Math.Abs(PivotX(a) - PivotX(v));
                    Check(!float.IsNaN(separation) && separation < 450,
                        name + " separated fighters at frame " + frame + ": " + separation);
                    foreach (ModelNode node in v.GetBodyObject().GetAllNodes())
                    {
                        Vector3f p = node.GetStart();
                        Check(!float.IsNaN(p.GetZ()) && Math.Abs(p.GetX() - arenaX) < 650 &&
                            Math.Abs(p.GetY()) < 600 && Math.Abs(p.GetZ()) < 600,
                            name + " victim escaped the arena");
                    }
                }
                // Explicit caller directions must still override XML.
                Check(v.PlayAnimation(victim.Name, -sign), "Explicit direction failed");
                Check(v.PlayedSign == -sign, "Explicit direction was overridden");
                victim.MoveData.SetDirectionData.IsExists = false;
                Check(v.PlayAnimation(victim.Name), "Move without SetDirection failed");
                Check(v.PlayedSign == -sign, "Move without SetDirection changed facing");
            }
            foreach (string name in new[] { "ThrowForward", "ThrowThroughTheBack" })
            foreach (int sign in new[] { -1, 1 })
            {
                GameUtils.SetSlowMode(1);
                InfoAnimation attack = LoadMove(xml, name, root);
                InfoAnimation victimMove = LoadMove(xml, name + "V", root);
                Fighter attacker = MakeFighter(attack, skeleton, sign, 0);
                Fighter victim = MakeFighter(victimMove, skeleton, -sign, sign * 80);
                Link(attacker, victim);
                Link(victim, attacker);
                victim.RefuseAnimation = true;
                bool dispatched = false;
                attacker.GetAnimationModule().AddEventListener(4, delegate(object data)
                {
                    foreach (ActionAnimation action in (List<ActionAnimation>)data)
                    {
                        if (!(action is ActionPlayAnimation)) continue;
                        dispatched = true;
                        action.Visit(attacker);
                    }
                });
                Check(attacker.PlayAnimation(attack, sign), name + " refused-case attacker did not start");
                for (int tick = 0; tick < 64 && !dispatched; tick++)
                    attacker.GetAnimationModule().Render();
                Check(dispatched, name + " paired action did not dispatch");
                Check(victim.PlayedSign == 0, name + " victim unexpectedly accepted the paired move");
                Check(!AllowsStrike(attacker, victim), name + " struck after the paired move was refused");
                bool attackWindow = false;
                for (int tick = 0; tick < 220; tick++)
                {
                    if (attacker.GetAnimationModule().FindInterval(
                        IntervalAnimation.IntervalType.INTERVAL_ATTACK) is IntervalAttack)
                    {
                        attackWindow = true;
                        Check(victim.GetAnimationModule().FindInterval(
                            IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE) == null,
                            name + " refused victim entered an invulnerable interval");
                        // _Collision is intentionally absent in this detached fixture.
                        // Reaching it would throw, so a false result here proves the
                        // real CheckCollision path returned at its paired-grab gate.
                        Check(!attacker.CheckCollision(victim, true),
                            name + " collision escaped the refused-grab gate");
                        break;
                    }
                    attacker.GetAnimationModule().Render();
                }
                Check(attackWindow, name + " never entered its real strike interval");
                victim.RefuseAnimation = false;
                InfoAnimation next = LoadMove(xml,
                    name == "ThrowForward" ? "ThrowThroughTheBack" : "ThrowForward", root);
                Check(attacker.PlayAnimation(next, sign), name + " next attack did not start");
                Check(AllowsStrike(attacker, victim), name + " retained a stale refused-grab gate");
            }
            Console.WriteLine("PASS: 36 throw playback and four refused-grab scenarios (" + checks +
                " assertions; real frame/action dispatch, vanilla XML, binary clips, both directions, three arena positions and simulation subdivisions).");
        }
        finally { Debug.unityLogger.logHandler = previousLog; }
    }
}
