using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Eclipse.Modding;

internal static class MoveCombatPatchTests
{
    private static int count;
    private static void Check(bool value, string why) { count++; if (!value) throw new Exception(why); }
    private static XmlElement Node(string xml) { var d = new XmlDocument(); d.LoadXml(xml); return d.DocumentElement; }
    private static readonly ModId Owner = ModId.Parse("fixture.move-patches");
    private static InfoAnimation Move(string name = "Test", bool initialized = false)
    {
        var move = new InfoAnimation { Name = name };
        move.MoveData.Intervals.Add(new IntervalAnimation { NodeInterval = Node("<Interval Name='Uninterrupt' Start='2' End='42'/>") });
        move.MoveData.Intervals.Add(new IntervalAttack { NodeInterval = Node("<Interval Type='Attack' Start='0' End='50'><Hit Name='High'/></Interval>") });
        move.ScheduledActions.Add(new ActionSound("snd_disk",18));
        move.SelectionConditions.Add(new ConditionAnimation());
        if (initialized) foreach (var interval in move.MoveData.Intervals) interval.Init();
        return move;
    }
    private static MoveCombatPatch Patch(string move = "Test") => new MoveCombatPatch(Owner,move,
        new[] { new ModMoveCondition(ModMoveConditionKind.ModExists,"Stun",not:true) },
        new ModMoveFramePatch("Uninterrupt",42,40),new ModMoveHitPatch("High","MiddleShortPlus"),new ModMoveFramePatch("snd_disk",18,16));
    private static MoveCombatPatchRuntime.Lifetime Apply(InfoAnimation[] moves, params MoveCombatPatch[] patches)
        => MoveCombatPatchRuntime.Apply(moves,patches,condition => condition.Kind == ModMoveConditionKind.Keys
            ? new ConditionKeys(condition.Keys.Single().Key) : new ConditionAnimation());
    private static void Reject(InfoAnimation[] moves, MoveCombatPatch[] patches, string reason)
    {
        bool rejected = false; try { Apply(moves,patches); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected,reason);
    }
    private const string AttackXml = "<Interval Type='Attack' Start='6' End='8' ID='3'><AttackingParts><Edge Name='EThigh_2'/><Edge Name='ECalf_2'/></AttackingParts>" +
        "<Damage Value='0.12'><Damage Type='UnarmedDamage' Shift='-10'/><Defense Type='HeadDefense'/></Damage><Impulse X='245' Y='0' Z='350'/><Hit Name='High'/></Interval>";
    private static InfoAnimation ExtrasMove(bool initialized)
    {
        var move = Move("Extras");
        move.MoveData.Intervals.Add(new IntervalAnimation(IntervalAnimation.IntervalType.INTERVAL_BLOCK) { NodeInterval = Node("<Interval Type='Block' Start='45'/>"), AuthoredType = "Block" });
        var attack = new IntervalAttack(); attack.Parse(Node(AttackXml)); move.MoveData.Intervals.Add(attack);
        if (initialized) foreach (var interval in move.MoveData.Intervals) interval.Init();
        return move;
    }
    private static ModMoveAttackEdit FullAttackEdit(double expectedDamage = 0.12) => new ModMoveAttackEdit(3,
        start: new ModMoveGuard<int>(6, 5), end: new ModMoveGuard<int>(8, 10),
        damage: new ModMoveGuard<double>(expectedDamage, 0.2),
        damageTerms: new ModMoveGuard<IReadOnlyDictionary<string, double>>(new Dictionary<string, double> { ["UnarmedDamage"] = -10 },
            new Dictionary<string, double> { ["UnarmedDamage"] = -5, ["WeaponDamage"] = 0 }),
        edges: new ModMoveGuard<IReadOnlyList<string>>(new[] { "EThigh_2", "ECalf_2" }, new[] { "ECalf_2" }),
        impulse: new ModMoveGuard<IReadOnlyList<double>>(new double[] { 245, 0, 350 }, new double[] { 300, 10, 0 }),
        hit: new ModMoveGuard<string>("High", "MiddleShortPlus"));
    private static bool Throws(Action action) { try { action(); return false; } catch (ModContentException) { return true; } }
    private static void ExtrasChecks()
    {
        foreach (bool initialized in new[] { false, true })
        {
            var move = ExtrasMove(initialized);
            var attack = (IntervalAttack)move.MoveData.Intervals[3];
            var block = move.MoveData.Intervals[2];
            var attackNode = attack.NodeInterval;
            var extras = new ModMoveCombatExtras(new[] {
                ModMoveIntervalEdit.Bounds(new ModMoveIntervalSelector("", "Uninterrupt", 2, 42), null, 30),
                ModMoveIntervalEdit.Removal(new ModMoveIntervalSelector("Block", "", 45, null)),
                ModMoveIntervalEdit.Addition("Invulnerable", "Dodge", 0, 4) }, new[] { FullAttackEdit() }, new ModMoveGuard<int>(1000, 1500));
            using (Apply(new[] { move }, new MoveCombatPatch(Owner, "Extras", extras: extras)))
            {
                Check(move.PlaybackRatePermille == 1500, "Playback rate did not apply.");
                Check(!move.MoveData.Intervals.Contains(block), "Open-ended Block interval was not removed.");
                var added = move.MoveData.Intervals.Last();
                if (!initialized) foreach (var interval in move.MoveData.Intervals) if (interval.NodeInterval != null) interval.Init();
                Check(added.Type == IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE && added.Name == "Dodge" && added.Start == 0 && added.EndFrame == 4,
                    "Typed interval addition was not built with its native type.");
                Check(move.MoveData.Intervals[0].Start == 2 && move.MoveData.Intervals[0].EndFrame == 30, "Interval bounds edit did not apply.");
                Check(attack.Start == 5 && attack.EndFrame == 10 && attack.HitReactions.Single().Name == "MiddleShortPlus" &&
                    attack.HitReactions[0].Start == 5 && attack.HitReactions[0].EndFrameValue == 10, "Attack frames or hit did not apply.");
                Check(Math.Abs(attack.GetDamage() - 0.2f) < 1e-6 && attack.GetAttackingParts().SequenceEqual(new[] { "ECalf_2" }) &&
                    attack.GetImpulse().GetX() == 300 && attack.GetImpulse().GetY() == 10 && attack.GetImpulse().GetZ() == 0, "Attack damage, edges or impulse did not apply.");
                var terms = attack.GetDamageAttributes();
                Check(terms.Count == 2 && terms[0].First == "WeaponDamage" && terms[1].First == "UnarmedDamage" && terms[1].Second == -5, "Damage terms were not rewritten in canonical order.");
                if (!initialized) Check(attackNode.Attributes["Start"].Value == "6", "Deferred attack edit mutated the shared source node.");
            }
            // The deferred case parsed the patched nodes above, so both cases restore parsed values.
            {
                Check(attack.NodeInterval == null && attack.Start == 6 && attack.EndFrame == 8 && attack.HitReactions[0].Name == "High" && attack.GetDamage() == 0.12f &&
                    attack.GetAttackingParts().SequenceEqual(new[] { "EThigh_2", "ECalf_2" }) && attack.GetImpulse().GetX() == 245 &&
                    attack.GetDamageAttributes().Single().Second == -10 && attack.HitReactions[0].Start == 6 && attack.HitReactions[0].EndFrameValue == 8,
                    "Parsed attack did not roll back.");
            }
            Check(move.PlaybackRatePermille == 1000 && move.MoveData.Intervals.Count == 4 && ReferenceEquals(move.MoveData.Intervals[2], block),
                "Rate or interval list did not roll back.");
            Check(move.MoveData.Intervals[0].Start == 2 && move.MoveData.Intervals[0].EndFrame == 42, "Interval bounds did not roll back.");
            // Unconsumed deferred XML keeps Defense children and restores the shared source node.
            if (!initialized)
            {
                var fresh = ExtrasMove(false); var freshAttack = (IntervalAttack)fresh.MoveData.Intervals[3]; var freshNode = freshAttack.NodeInterval;
                var freshInterval = fresh.MoveData.Intervals[0].NodeInterval;
                using (Apply(new[] { fresh }, new MoveCombatPatch(Owner, "Extras", extras: new ModMoveCombatExtras(new[] {
                    ModMoveIntervalEdit.Bounds(new ModMoveIntervalSelector("", "Uninterrupt", 2, 42), 1, null) }, new[] { FullAttackEdit() }))))
                    Check(freshAttack.NodeInterval["Damage"].SelectNodes("Defense").Count == 1 && freshAttack.NodeInterval["Damage"].SelectNodes("Damage").Count == 2 &&
                        fresh.MoveData.Intervals[0].NodeInterval.Attributes["Start"].Value == "1", "Deferred edits dropped Defense children or bounds.");
                Check(ReferenceEquals(freshAttack.NodeInterval, freshNode) && ReferenceEquals(fresh.MoveData.Intervals[0].NodeInterval, freshInterval),
                    "Unconsumed deferred edits did not restore the source nodes.");
            }
            Reject(new[] { move }, new[] { new MoveCombatPatch(Owner, "Extras", extras: new ModMoveCombatExtras(attacks: new[] { FullAttackEdit(0.13) })) },
                "Wrong expected damage accepted.");
            Reject(new[] { move }, new[] { new MoveCombatPatch(Owner, "Extras", extras: new ModMoveCombatExtras(new[] {
                ModMoveIntervalEdit.Removal(new ModMoveIntervalSelector("Block", "", 45, 47)) })) }, "Open-ended interval matched an explicit end.");
            Reject(new[] { move }, new[] { new MoveCombatPatch(Owner, "Extras", extras: new ModMoveCombatExtras(attacks: new[] {
                new ModMoveAttackEdit(9, damage: new ModMoveGuard<double>(0.12, 0.2)) })) }, "Missing attack id accepted.");
            var looped = ExtrasMove(initialized); looped.Looped = true;
            Reject(new[] { looped }, new[] { new MoveCombatPatch(Owner, "Extras", extras: new ModMoveCombatExtras(playbackRate: new ModMoveGuard<int>(1000, 1500))) },
                "Rate accepted on a looped move.");
            var still = ExtrasMove(initialized); still.MidFrames = 0;
            Reject(new[] { still }, new[] { new MoveCombatPatch(Owner, "Extras", extras: new ModMoveCombatExtras(playbackRate: new ModMoveGuard<int>(1000, 1500))) },
                "Rate that skips keyframes accepted.");
            // A dry run validates without touching the move.
            var dry = ExtrasMove(initialized);
            MoveCombatPatchRuntime.Apply(new[] { dry }, new[] { new MoveCombatPatch(Owner, "Extras", extras: new ModMoveCombatExtras(attacks: new[] { FullAttackEdit() })) },
                condition => new ConditionAnimation(), null, true);
            var dryAttack = (IntervalAttack)dry.MoveData.Intervals[3];
            Check(initialized ? dryAttack.GetDamage() == 0.12f : dryAttack.NodeInterval.Attributes["Start"].Value == "6", "Dry run changed the move.");
        }
        var clip = Move("Clip"); clip.FileName = "old_clip.bytes";
        using (Apply(new[] { clip }, new MoveCombatPatch(Owner, "Clip", animation: new ModMoveAnimationPatch("old_clip.bytes", "other_clip.bytes"))))
            Check(clip.FileName == "other_clip.bytes", "Native clip swap did not apply.");
        Check(clip.FileName == "old_clip.bytes", "Native clip swap did not roll back.");
        Check(Throws(() => new ModMoveAttackEdit(3, damageTerms: new ModMoveGuard<IReadOnlyDictionary<string, double>>(
            new Dictionary<string, double> { ["UnarmedDamage"] = 0 }, new Dictionary<string, double> { ["MagicDamage"] = 0 }))), "Adding MagicDamage accepted.");
        Check(Throws(() => new MoveCombatPatch(Owner, "Test", hit: new ModMoveHitPatch("High", "Low"),
            extras: new ModMoveCombatExtras(attacks: new[] { FullAttackEdit() }))), "hit and attacks accepted together.");
        Check(Throws(() => new MoveCombatPatch(Owner, "Test", intervalEnd: new ModMoveFramePatch("Uninterrupt", 42, 40),
            extras: new ModMoveCombatExtras(new[] { ModMoveIntervalEdit.Removal(new ModMoveIntervalSelector("", "Uninterrupt", 2, 42)) }))),
            "Single-interval and list edits of one interval accepted.");
        Check(Throws(() => new ModMoveCombatExtras(new[] { ModMoveIntervalEdit.Removal(new ModMoveIntervalSelector("", "Uninterrupt", 2, 42)),
            ModMoveIntervalEdit.Bounds(new ModMoveIntervalSelector("", "Uninterrupt", 2, 42), 3, null) })), "Two edits of one interval accepted.");
        Check(Throws(() => new ModMoveCombatExtras(playbackRate: new ModMoveGuard<int>(1000, 2500))), "Rate above maximum accepted.");
        Check(Throws(() => new ModMoveIntervalSelector("Attack", "", 0, null)), "Attack interval selector accepted.");
    }
    public static void Main()
    {
        foreach (bool initialized in new[] { false,true })
        foreach (bool consume in new[] { false,true })
        {
            var move=Move(initialized:initialized); var baseCondition=move.SelectionConditions[0];
            var interval=move.MoveData.Intervals[0]; var attack=(IntervalAttack)move.MoveData.Intervals[1];
            var source=interval.NodeInterval; var hitSource=attack.NodeInterval;
            var lifetime=Apply(new[]{move},Patch());
            Check(move.SelectionConditions.Count==2 && ReferenceEquals(move.SelectionConditions[0],baseCondition),"Condition order changed.");
            Check(move.ScheduledActions[0].ScheduledFrame==16,"Sound frame unchanged.");
            if (!initialized)
            {
                Check(source.Attributes["End"].Value=="42" && hitSource["Hit"].Attributes["Name"].Value=="High","Shared source nodes mutated.");
                Check(!ReferenceEquals(interval.NodeInterval,source) && !ReferenceEquals(attack.NodeInterval,hitSource),"Pending nodes not isolated.");
            }
            if (consume && !initialized) foreach(var value in move.MoveData.Intervals) value.Init();
            if (initialized || consume) Check(interval.EndFrame==40 && attack.HitReactions.Single().Name=="MiddleShortPlus","Parsed target not patched.");
            else Check(interval.NodeInterval.Attributes["End"].Value=="40" && attack.NodeInterval["Hit"].Attributes["Name"].Value=="MiddleShortPlus","Deferred target not patched.");
            var unrelated=new ConditionAnimation(); move.SelectionConditions.Add(unrelated);
            lifetime.Dispose(); lifetime.Dispose();
            Check(move.SelectionConditions.SequenceEqual(new[]{baseCondition,unrelated}),"Rollback damaged unrelated conditions.");
            Check(move.ScheduledActions[0].ScheduledFrame==18,"Sound rollback failed.");
            if (initialized || consume) Check(interval.EndFrame==42 && attack.HitReactions.Single().Name=="High","Post-init rollback failed.");
            else Check(ReferenceEquals(interval.NodeInterval,source)&&ReferenceEquals(attack.NodeInterval,hitSource),"Pre-init rollback failed.");
        }
        foreach (string reaction in new[] { "Physycal", "HighLong", "NoReaction" })
        foreach (bool initialized in new[] { false, true })
        {
            var move = Move(initialized: initialized);
            var attack = (IntervalAttack)move.MoveData.Intervals[1];
            using (Apply(new[] { move }, new MoveCombatPatch(Owner, "Test", hit: new ModMoveHitPatch("High", reaction))))
            {
                if (!initialized) attack.Init();
                Check(attack.HitReactions.Single().Name == reaction, "Physical-fall patch changed native spelling.");
            }
            Check(attack.HitReactions.Single().Name == "High", "Physical-fall patch rollback failed.");
        }
        var first=Move("First"); var second=Move("Second"); var firstNode=first.MoveData.Intervals[0].NodeInterval;
        Reject(new[]{first,second},new[]{Patch("First"),new MoveCombatPatch(Owner,"Second",intervalEnd:new ModMoveFramePatch("Uninterrupt",41,40))},"Wrong expected value accepted.");
        Check(ReferenceEquals(first.MoveData.Intervals[0].NodeInterval,firstNode) && first.SelectionConditions.Count==1 && first.ScheduledActions[0].ScheduledFrame==18,"Late validation failure partially applied batch.");
        Reject(new[]{Move()},new[]{Patch("Missing")},"Missing target accepted.");
        Reject(new[]{Move(),Move()},new[]{Patch()},"Ambiguous target accepted.");
        Reject(new[]{Move()},new[]{Patch(),Patch()},"Duplicate patch accepted.");
        var duplicate=Move(); duplicate.MoveData.Intervals.Add(duplicate.MoveData.Intervals[0]);
        Reject(new[]{duplicate},new[]{Patch()},"Ambiguous interval accepted.");
        duplicate=Move(); duplicate.MoveData.Intervals.Add(duplicate.MoveData.Intervals[1]);
        Reject(new[]{duplicate},new[]{Patch()},"Ambiguous attack accepted.");
        duplicate=Move(); duplicate.ScheduledActions.Add(new ActionSound("snd_disk",18));
        Reject(new[]{duplicate},new[]{Patch()},"Ambiguous sound accepted.");
        var ranged=Move(); ((XmlElement)ranged.MoveData.Intervals[1].NodeInterval["Hit"]).SetAttribute("Start","3");
        Reject(new[]{ranged},new[]{Patch()},"Partial-range hit accepted.");
        var eventSound=Move(); eventSound.ScheduledActions[0].Frame=null;
        Reject(new[]{eventSound},new[]{Patch()},"Event sound accepted.");
        var low=Move(); Reject(new[]{low},new[]{new MoveCombatPatch(Owner,"Test",intervalEnd:new ModMoveFramePatch("Uninterrupt",42,1))},"End before start accepted.");
        var later=Move(initialized:true);var life=Apply(new[]{later},Patch());
        later.MoveData.Intervals[0].EndFrame=39;((IntervalAttack)later.MoveData.Intervals[1]).HitReactions[0].Name="Low";later.ScheduledActions[0].SetScheduledFrame(20);
        life.Dispose();Check(later.MoveData.Intervals[0].EndFrame==39 && ((IntervalAttack)later.MoveData.Intervals[1]).HitReactions[0].Name=="Low" && later.ScheduledActions[0].ScheduledFrame==20,"Rollback overwrote later field changes.");
        var shared=Move(); var other=Move("Other");other.MoveData.Intervals[0].NodeInterval=shared.MoveData.Intervals[0].NodeInterval;
        life=Apply(new[]{shared,other},Patch());other.MoveData.Intervals[0].Init();Check(other.MoveData.Intervals[0].EndFrame==42,"Patch leaked through shared template node.");life.Dispose();
        // Live tuning (ModRuntime.TryReloadMovePatches) undoes the applied batch, applies the
        // edited one against native values, and restores the previous batch if it is rejected.
        foreach (bool initialized in new[] { false, true })
        {
            var tuned = Move(initialized: initialized);
            int End() => tuned.MoveData.Intervals[0].NodeInterval != null
                ? int.Parse(tuned.MoveData.Intervals[0].NodeInterval.Attributes["End"].Value) : tuned.MoveData.Intervals[0].EndFrame;
            var current = Apply(new[] { tuned }, Patch());
            current.Dispose();
            current = Apply(new[] { tuned }, new MoveCombatPatch(Owner, "Test", intervalEnd: new ModMoveFramePatch("Uninterrupt", 42, 30)));
            Check(End() == 30 && tuned.ScheduledActions[0].ScheduledFrame == 18 && tuned.SelectionConditions.Count == 1, "Swapped batch kept the previous patch.");
            current.Dispose();
            Reject(new[] { tuned }, new[] { new MoveCombatPatch(Owner, "Test", intervalEnd: new ModMoveFramePatch("Uninterrupt", 30, 20)) }, "Edited batch was guarded by previously patched values.");
            current = Apply(new[] { tuned }, Patch());
            Check(End() == 40 && tuned.ScheduledActions[0].ScheduledFrame == 16, "Previous batch could not be restored after a rejected swap.");
            current.Dispose();
            Check(End() == 42 && tuned.ScheduledActions[0].ScheduledFrame == 18 && tuned.SelectionConditions.Count == 1, "Swapped batches did not roll back to native values.");
        }
        var disabled = Move("Disabled"); var original = disabled.SelectionConditions[0];
        using (Apply(new[] { disabled }, new MoveCombatPatch(Owner, "Disabled", disable: true)))
        {
            Check(disabled.SelectionConditions.Count == 2 && ReferenceEquals(disabled.SelectionConditions[0], original), "Disable condition did not append.");
            Check(!disabled.SelectionConditions[1].IsEqual(new ModelConditions()) &&
                !disabled.SelectionConditions[1].IsEqual(new Model(), disabled), "Disabled move remained selectable.");
        }
        Check(disabled.SelectionConditions.SequenceEqual(new[] { original }), "Disabled move rollback changed original conditions.");
        var inputMove = Move("Input"); inputMove.Priority = 1000;
        inputMove.SelectionConditions[0] = new ConditionKeys("Super");
        var originalInput = inputMove.SelectionConditions[0];
        var inputPatch = new MoveCombatPatch(Owner, "Input", input: new ModMoveInputPatch("Super", "RaidCharge"),
            priority: new ModMovePriorityPatch(1000, 200));
        using (Apply(new[] { inputMove }, inputPatch))
        {
            Check(inputMove.Priority == 200 && inputMove.SelectionConditions[0] is ConditionKeys changed &&
                changed.Key == "RaidCharge", "Native input/priority patch did not apply.");
        }
        Check(inputMove.Priority == 1000 && ReferenceEquals(inputMove.SelectionConditions[0], originalInput),
            "Input/priority rollback did not restore the original native move.");
        var inserted = new ConditionAnimation();
        var shiftedInput = Apply(new[] { inputMove }, inputPatch);
        inputMove.SelectionConditions.Insert(0, inserted);
        shiftedInput.Dispose();
        Check(inputMove.SelectionConditions.SequenceEqual(new[] { inserted, originalInput }),
            "Input rollback damaged a later sibling insertion.");
        inputMove.SelectionConditions.Remove(inserted);
        var rebuiltPriorities = new List<int>();
        using (MoveCombatPatchRuntime.Apply(new[] { inputMove }, new[] { inputPatch },
            condition => new ConditionKeys(condition.Keys.Single().Key), () => rebuiltPriorities.Add(inputMove.Priority)))
            Check(rebuiltPriorities.SequenceEqual(new[] { 200 }), "Priority conflicts were not rebuilt after input/priority apply.");
        Check(rebuiltPriorities.SequenceEqual(new[] { 200, 1000 }), "Priority conflicts were not rebuilt after input/priority rollback.");
        int unrelatedRebuilds = 0;
        using (MoveCombatPatchRuntime.Apply(new[] { Move() }, new[] { Patch() },
            condition => new ConditionAnimation(), () => unrelatedRebuilds++)) { }
        Check(unrelatedRebuilds == 0, "Priority conflicts were rebuilt for a patch without input or priority.");
        Reject(new[] { inputMove }, new[] { new MoveCombatPatch(Owner, "Input",
            input: new ModMoveInputPatch("Magic", "RaidCharge")) }, "Wrong expected input accepted.");
        Reject(new[] { inputMove }, new[] { new MoveCombatPatch(Owner, "Input",
            priority: new ModMovePriorityPatch(900, 200)) }, "Wrong expected priority accepted.");
        var ambiguousInput = Move("Input"); ambiguousInput.SelectionConditions[0] = new ConditionKeys("Super");
        ambiguousInput.SelectionConditions.Add(new ConditionKeys("Super"));
        Reject(new[] { ambiguousInput }, new[] { new MoveCombatPatch(Owner, "Input",
            input: new ModMoveInputPatch("Super", "RaidCharge")) }, "Ambiguous native input accepted.");
        var lateInput = Move("LateInput"); lateInput.SelectionConditions[0] = new ConditionKeys("Super");
        Reject(new[] { inputMove, lateInput }, new[] { inputPatch,
            new MoveCombatPatch(Owner, "LateInput", priority: new ModMovePriorityPatch(1000, 200)) },
            "Late priority failure partially applied input patch.");
        Check(inputMove.Priority == 1000 && ReferenceEquals(inputMove.SelectionConditions[0], originalInput),
            "Failed patch batch changed the original input.");
        var clipMove = Move("Clip"); clipMove.FileName = "old_clip.bytes"; clipMove.AnimationEndFrame = 60;
        var clipPatch = new MoveCombatPatch(Owner, "Clip", animation: new ModMoveAnimationPatch(
            "old_clip.bytes", AssetId.Parse("fixture.move-patches:animations/new_clip")));
        using (Apply(new[] { clipMove }, clipPatch))
            Check(clipMove.FileName == "fixture.move-patches:animations/new_clip" && clipMove.AnimationEndFrame == 28,
                "Parsed native clip was not replaced.");
        Check(clipMove.FileName == "old_clip.bytes" && clipMove.AnimationEndFrame == 60,
            "Clip patch did not restore the original parsed animation.");
        Reject(new[] { clipMove }, new[] { new MoveCombatPatch(Owner, "Clip", animation:
            new ModMoveAnimationPatch("wrong_clip.bytes", AssetId.Parse("fixture.move-patches:animations/new_clip"))) },
            "Wrong expected native clip accepted.");
        foreach (bool initialized in new[] { false, true })
        {
            var removeMove = Move("Remove", initialized);
            var evade = new IntervalAnimation { Type = IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE,
                NodeInterval = Node("<Interval Name='Evade' Type='Invulnerable' Start='0' End='47'/>") };
            if (initialized) evade.Init();
            removeMove.MoveData.Intervals.Insert(1, evade);
            var removal = new MoveCombatPatch(Owner, "Remove", removeInterval:
                new ModMoveIntervalRemoval("Evade", "Invulnerable", 0, 47));
            using (Apply(new[] { removeMove }, removal))
                Check(!removeMove.MoveData.Intervals.Contains(evade) && removeMove.MoveData.Intervals.Count == 2,
                    "Guarded native interval removal did not apply.");
            Check(ReferenceEquals(removeMove.MoveData.Intervals[1], evade),
                "Removed interval did not return to its original position.");
            Reject(new[] { removeMove }, new[] { new MoveCombatPatch(Owner, "Remove", removeInterval:
                new ModMoveIntervalRemoval("Evade", "Invulnerable", 0, 46)) },
                "Wrong expected interval bounds accepted.");
            Reject(new[] { removeMove }, new[] { new MoveCombatPatch(Owner, "Remove", removeInterval:
                new ModMoveIntervalRemoval("Evade", "Block", 0, 47)) },
                "Wrong expected interval type accepted.");
        }
        foreach (bool initialized in new[] { false, true })
        {
            var addMove = Move("Add", initialized);
            using (Apply(new[] { addMove }, new MoveCombatPatch(Owner, "Add",
                intervalStart: new ModMoveFramePatch("Uninterrupt", 2, 8),
                addInterval: new ModMoveIntervalAddition("SemiUninterrupt", 0, 7))))
            {
                var added = addMove.MoveData.Intervals.Last();
                Check(addMove.MoveData.Intervals.Count == 3, "Added interval missing.");
                if (initialized)
                    Check(added.NodeInterval == null && added.Name == "SemiUninterrupt" && added.Start == 0 && added.EndFrame == 7,
                        "Added interval was not parsed alongside parsed intervals.");
                else
                {
                    Check(added.NodeInterval.Attributes["Name"].Value == "SemiUninterrupt" &&
                        added.NodeInterval.Attributes["End"].Value == "7", "Added interval was not deferred with its move.");
                    foreach (var value in addMove.MoveData.Intervals) value.Init();
                    Check(added.Start == 0 && added.EndFrame == 7, "Deferred added interval parsed wrong bounds.");
                }
            }
            Check(addMove.MoveData.Intervals.Count == 2, "Added interval was not removed on rollback.");
            Reject(new[] { addMove }, new[] { new MoveCombatPatch(Owner, "Add",
                addInterval: new ModMoveIntervalAddition("Uninterrupt", 0, 7)) }, "Duplicate named interval accepted.");
            bool invalid = false;
            try { new ModMoveIntervalAddition("Evade", 0, 7); } catch (ModContentException) { invalid = true; }
            Check(invalid, "Unsupported added interval name accepted.");
        }
        foreach (bool initialized in new[] { false, true })
        {
            var boundsMove = Move("Bounds", initialized);
            var interval = boundsMove.MoveData.Intervals[0];
            var originalNode = interval.NodeInterval;
            using (Apply(new[] { boundsMove }, new MoveCombatPatch(Owner, "Bounds",
                intervalStart: new ModMoveFramePatch("Uninterrupt", 2, 0),
                intervalEnd: new ModMoveFramePatch("Uninterrupt", 42, 35))))
            {
                if (initialized)
                    Check(interval.Start == 0 && interval.EndFrame == 35, "Parsed interval bounds did not apply together.");
                else
                    Check(interval.NodeInterval.Attributes["Start"].Value == "0" &&
                        interval.NodeInterval.Attributes["End"].Value == "35" &&
                        originalNode.Attributes["Start"].Value == "2", "Deferred interval bounds did not apply together.");
            }
            if (initialized)
                Check(interval.Start == 2 && interval.EndFrame == 42, "Parsed interval bounds did not roll back.");
            else
                Check(ReferenceEquals(interval.NodeInterval, originalNode), "Deferred interval bounds did not roll back.");
        }
        Reject(new[] { Move() }, new[] { new MoveCombatPatch(Owner, "Test",
            intervalStart: new ModMoveFramePatch("Uninterrupt", 9, 0)) }, "Wrong expected start accepted.");
        Reject(new[] { Move() }, new[] { new MoveCombatPatch(Owner, "Test",
            intervalStart: new ModMoveFramePatch("Uninterrupt", 2, 50)) }, "Start beyond end accepted.");
        var batch = Move("Batch");
        Reject(new[] { disabled, batch }, new[] { new MoveCombatPatch(Owner, "Disabled", disable: true),
            new MoveCombatPatch(Owner, "Batch", intervalEnd: new ModMoveFramePatch("Uninterrupt", 41, 40)) },
            "Late validation failure partially disabled a move.");
        Check(disabled.SelectionConditions.SequenceEqual(new[] { original }), "Failed batch left a disabled move.");
        ExtrasChecks();
        Console.WriteLine("PASS: "+count+" production move-patch batch/rollback checks. Controlled native containers exercise deferred and parsed intervals; native Unity acceptance is separate.");
    }
}

public class Model { }
public class ModelConditions { }
public class ConditionAnimation
{
    public enum ConditionType { NONE }
    public ConditionAnimation(ConditionType type = ConditionType.NONE) { }
    public virtual bool IsEqual(ModelConditions conditions) => true;
    public virtual bool IsEqual(Model model, InfoAnimation animation) => true;
}
public class ConditionKeys : ConditionAnimation
{
    public string Key;
    public ConditionKeys(string key) { Key = key; }
    public bool HasSameKeyRequirementAs(ConditionKeys other) => other != null && Key == other.Key;
}
public class InfoAnimation
{
    public string Name,FileName; public int Priority,AnimationEndFrame,MidFrames=2,PlaybackRatePermille=1000; public bool HasPhysics,Looped; public Data MoveData=new Data();
    public bool GetIsLooped() => Looped;
    public List<ConditionAnimation> SelectionConditions=new List<ConditionAnimation>();
    public List<ActionAnimation> ScheduledActions=new List<ActionAnimation>();
    public class Data { public List<IntervalAnimation> Intervals=new List<IntervalAnimation>(); }
    public void ReplaceClip(string fileName,int endFrame)
    { if(fileName.Contains("missing")) throw new InvalidOperationException("Missing clip");FileName=fileName;AnimationEndFrame=endFrame==0?28:endFrame; }
}
public class IntervalAnimation
{
    public enum IntervalType { INTERVAL_NONE, INTERVAL_UNINTERRUPT, INTERVAL_INVULNERABLE, INTERVAL_ATTACK, INTERVAL_BLOCK, INTERVAL_INVISIBLE }
    public IntervalType Type;
    public IntervalAnimation() { }
    public IntervalAnimation(IntervalType type) { Type = type; }
    public XmlNode NodeInterval;public string Name;public int Start,EndFrame,FinishFrame=60;
    public string AuthoredType = "";public bool HasAuthoredEnd = true;private int id = -1;
    public int GetAnimationId() => id;
    public static IntervalType ParseIntervalType(string type) => type == "Attack" ? IntervalType.INTERVAL_ATTACK : type == "Block" ? IntervalType.INTERVAL_BLOCK :
        type == "Invulnerable" ? IntervalType.INTERVAL_INVULNERABLE : type == "Invisible" ? IntervalType.INTERVAL_INVISIBLE : IntervalType.INTERVAL_NONE;
    public virtual void Parse(XmlNode node) { NodeInterval=node;id=node.Attributes["ID"]!=null?int.Parse(node.Attributes["ID"].Value):-1;AuthoredType=node.Attributes["Type"]?.Value??""; }
    public virtual void Init()
    {
        if (NodeInterval.Attributes["ID"] != null) id = int.Parse(NodeInterval.Attributes["ID"].Value);
        if (NodeInterval.Attributes["Type"] != null) AuthoredType = NodeInterval.Attributes["Type"].Value;
        Name=NodeInterval.Attributes["Name"]?.Value;Start=NodeInterval.Attributes["Start"]!=null?int.Parse(NodeInterval.Attributes["Start"].Value):0;
        HasAuthoredEnd=NodeInterval.Attributes["End"]!=null;EndFrame=HasAuthoredEnd?int.Parse(NodeInterval.Attributes["End"].Value):FinishFrame+2;NodeInterval=null;
    }
}
public class IntervalAttack : IntervalAnimation
{
    public class Reaction { public string Name;public int Start,EndFrameValue;public int EndFrame=>EndFrameValue; }
    public List<Reaction> HitReactions=new List<Reaction>();
    private float damage;private readonly List<Pair<string,float>> terms=new List<Pair<string,float>>();
    private readonly List<string> parts=new List<string>();private readonly Vector3f impulse=new Vector3f();
    public IntervalAttack() : base(IntervalType.INTERVAL_ATTACK) { }
    public float GetDamage()=>damage; public List<Pair<string,float>> GetDamageAttributes()=>terms;
    public List<string> GetAttackingParts()=>parts; public Vector3f GetImpulse()=>impulse;
    internal void EclipseSetDamage(float value){damage=value;}
    internal void EclipseSetAttackingParts(IEnumerable<string> value){parts.Clear();parts.AddRange(value);}
    static float F(XmlNode node,string name)=>node?.Attributes[name]==null?0f:float.Parse(node.Attributes[name].Value,System.Globalization.CultureInfo.InvariantCulture);
    public override void Init()
    {
        var node=NodeInterval; base.Init();
        HitReactions.Add(new Reaction{Name=node["Hit"].Attributes["Name"].Value,Start=Start,EndFrameValue=EndFrame});
        var damageNode=node["Damage"];
        if(damageNode!=null){damage=F(damageNode,"Value");foreach(XmlNode child in damageNode.ChildNodes)if(child.Name=="Damage")terms.Add(new Pair<string,float>(child.Attributes["Type"].Value,F(child,"Shift")));}
        if(node["AttackingParts"]!=null)foreach(XmlNode edge in node["AttackingParts"].ChildNodes)parts.Add(edge.Attributes["Name"].Value);
        var impulseNode=node["Impulse"];impulse.SetX(F(impulseNode,"X"));impulse.SetY(F(impulseNode,"Y"));impulse.SetZ(F(impulseNode,"Z"));
    }
}
public class Pair<T1,T2> { public T1 First;public T2 Second;public Pair(T1 first,T2 second){First=first;Second=second;} }
public class Vector3f { float x,y,z;public float GetX()=>x;public float GetY()=>y;public float GetZ()=>z;public void SetX(float v){x=v;}public void SetY(float v){y=v;}public void SetZ(float v){z=v;} }
public class ActionAnimation { public int? Frame;public int? ScheduledFrame=>Frame;public void SetScheduledFrame(int frame){Frame=frame;} }
public class ActionSound : ActionAnimation { private string name;public ActionSound(string value,int frame){name=value;Frame=frame;}public string get_Name()=>name; }
