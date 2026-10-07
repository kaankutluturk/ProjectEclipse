using System;
using System.Collections.Generic;
using System.Linq;
using Eclipse.Modding;

internal static class DECombatPerksNativeTests
{
    private static int _checks;
    private static void Check(bool value, string message) { _checks++; if (!value) throw new Exception(message); }

    public static int Main()
    {
        CheckHitClassificationAndLiveDamage();
        CheckReverseSidesAndLocalVersus();
        CheckChildAttackerRouting();
        CheckResolvedChildDamage();
        CheckOutgoingArithmetic();
        CheckStatusIcons();
        Console.WriteLine("DE combat perks native PASS: " + _checks + " checks; extracted current hit-phase/status-icon methods and current hit contract.");
        return 0;
    }

    private static void CheckHitClassificationAndLiveDamage()
    {
        var fight = new FightHarness();
        fight.Player = new Model { Name = "player" };
        fight.Opponent = new Model { Name = "opponent" };
        var animation = new InfoAnimation("Weapon", "Unarmed");
        var strike = new Model.StrikeResult {
            AttackerModel = fight.Player, AttackAnimation = animation, FinalDamage = 0.20f,
            IsBlocked = false, IsCritical = true
        };
        fight.BeginDispatched = false;
        fight.Hit(new Model.EventModel { sourceModel = fight.Opponent, Opponent = fight.Player }, strike, ModEffectEvent.PostHit);
        Check(fight.Events.Count == 0, "Hit phase dispatched before fight-begin initialization.");
        fight.BeginDispatched = true;
        fight.Hit(new Model.EventModel { sourceModel = fight.Opponent, Opponent = fight.Player }, strike, ModEffectEvent.PostHit);
        Check(fight.Events.Count == 2, "PostHit must dispatch attacker and defender once.");
        Check(fight.Events[0].Side == "player" && !fight.Events[0].Hit.HitEvent.Incoming, "Attacker side/target snapshot wrong.");
        Check(fight.Events[1].Side == "opponent" && fight.Events[1].Hit.HitEvent.Incoming, "Defender side/target snapshot wrong.");
        Check(fight.Events.All(e => e.Hit.Blocked == false && e.Hit.Critical), "Block/critical snapshot changed between sides.");
        Check(fight.Events.All(e => e.Hit.HitEvent.Weapon && e.Hit.HitEvent.Unarmed && !e.Hit.HitEvent.Ranged && !e.Hit.HitEvent.Magic),
            "Recovered animation tags were not mapped independently.");
        Check(fight.Events[0].Hit.TryAddOutgoing(0.10, out var error) && error == "", "PostHit additive damage failed.");
        Check(Math.Abs(strike.FinalDamage - 0.30f) < 0.000001 && Math.Abs(fight.Events[1].Hit.Damage - 0.30) < 0.000001,
            "Attacker/defender hit snapshots do not share the pending native strike.");

        fight.Events.Clear();
        animation = new InfoAnimation("RangedMissile", "MagicMissile");
        strike.AttackAnimation = animation; strike.FinalDamage = 0.4f;
        fight.Hit(new Model.EventModel { sourceModel = fight.Opponent, Opponent = fight.Player }, strike, ModEffectEvent.HitPostCrit);
        Check(fight.Events.All(e => e.Hit.HitEvent.Ranged && e.Hit.HitEvent.Magic && !e.Hit.HitEvent.Weapon && !e.Hit.HitEvent.Unarmed),
            "Ranged/magic cancellation tags are not exact recovered predicates.");
    }

    private static void CheckReverseSidesAndLocalVersus()
    {
        var fight = new FightHarness { Player = new Model { Name = "player" }, Opponent = new Model { Name = "opponent" } };
        var strike = new Model.StrikeResult { AttackerModel = fight.Opponent, AttackAnimation = new InfoAnimation("Weapon"), FinalDamage = .2f };
        fight.Hit(new Model.EventModel { sourceModel = fight.Player, Opponent = fight.Opponent }, strike, ModEffectEvent.PostHit);
        Check(fight.Events.Count == 2 && fight.Events[0].Side == "opponent" && !fight.Events[0].Hit.HitEvent.Incoming &&
            fight.Events[1].Side == "player" && fight.Events[1].Hit.HitEvent.Incoming, "Opponent attack side routing is wrong.");
        fight.Events.Clear(); fight.LocalVersus = true;
        fight.Hit(new Model.EventModel { sourceModel = fight.Player, Opponent = fight.Opponent }, strike, ModEffectEvent.PostHit);
        Check(fight.Events.Count == 0, "Local versus must bypass scripted hit phases.");
    }

    private static void CheckOutgoingArithmetic()
    {
        double damage = .25;
        var hit = new ModIncomingHit(() => damage, value => damage = value, hitEvent: new ModHitEvent(false, true, false, false, false));
        Check(hit.TryScaleOutgoing(1.5, out _) && Math.Abs(damage - .375) < 0.000001, "Existing outgoing scale changed.");
        Check(hit.TryAddOutgoing(.10, out _) && Math.Abs(damage - .475) < 0.000001, "Additive normalized damage changed.");
        double before = damage;
        Check(!hit.TryAddOutgoing(-.01, out _) && damage == before, "Negative additive damage was accepted.");
        Check(!hit.TryAddOutgoing(1.01, out _) && damage == before, "Oversized additive damage was accepted.");
    }

    private static void CheckChildAttackerRouting()
    {
        foreach (bool reverse in new[] { false, true })
        {
            var fight = new FightHarness { Player = new Model { Name = "player" }, Opponent = new Model { Name = "opponent" } };
            var root = reverse ? fight.Opponent : fight.Player;
            var victim = reverse ? fight.Player : fight.Opponent;
            var child = new Model { Name = "projectile", Owner = new Model { Owner = root } };
            var strike = new Model.StrikeResult { AttackerModel = child, AttackAnimation = new InfoAnimation("RangedMissile"), FinalDamage = .2f };
            // Strike fallback and event actor both route through the current root.
            foreach (bool fallback in new[] { false, true })
            {
                fight.Events.Clear();
                fight.Hit(new Model.EventModel { sourceModel = victim, Opponent = fallback ? null : child }, strike, ModEffectEvent.PostHit);
                Check(fight.Events.Count == 2 && fight.Events[0].Side == (reverse ? "opponent" : "player"), "Nested child lost main attacker attribution.");
                Check(fight.Events.All(e => e.Hit.HitEvent.Ranged) && !fight.Events[0].Hit.HitEvent.Incoming && fight.Events[1].Hit.HitEvent.Incoming, "Child tags or recipient perspective changed.");
                Check(fight.Events[0].Hit.TryScaleOutgoing(2, out _), "Child outgoing modifier unavailable.");
                Check(Math.Abs(strike.FinalDamage - (fallback ? .8f : .4f)) < .00001 && strike.AttackerModel == child, "Attribution changed native source or pending damage.");
            }
            fight.Events.Clear();
            // Native post-critical processing has not refreshed EventModel's attacker yet.
            fight.Hit(new Model.EventModel { sourceModel = victim, Opponent = victim }, strike, ModEffectEvent.HitPostCrit);
            Check(fight.Events.Count == 2 && fight.Events[0].Side == (reverse ? "opponent" : "player"), "Stale event target overrode current strike attacker.");
            fight.Events.Clear(); child.Owner = new Model { Name = "retired or unrelated" };
            fight.Hit(new Model.EventModel { sourceModel = victim, Opponent = child }, strike, ModEffectEvent.HitPostCrit);
            Check(fight.Events.Count == 1 && fight.Events[0].Hit.HitEvent.Incoming, "Unrelated root impersonated current attacker.");
            fight.Events.Clear(); fight.Hit(new Model.EventModel { sourceModel = new Model(), Opponent = child }, strike, ModEffectEvent.PostHit);
            Check(fight.Events.Count == 0, "Unrelated actor pair delivered main-fighter phase.");
        }
    }

    private static void CheckResolvedChildDamage()
    {
        foreach (bool reverse in new[] { false, true })
        {
            var fight = new FightHarness { Player = new Model(), Opponent = new Model() };
            var root = reverse ? fight.Opponent : fight.Player;
            var victim = reverse ? fight.Player : fight.Opponent;
            var child = new Model { Owner = root };
            var strike = new Model.StrikeResult { AttackerModel = child, FinalDamage = .2f };
            var contact = new Model.EventModel { Opponent = child, sourceModel = victim };
            fight.Outgoing(contact, strike);
            Check(fight.Events.Count == 1 && fight.Events[0].Side == (reverse ? "opponent" : "player") && fight.Events[0].Type == ModEffectEvent.DamageDealing, "Child outgoing damage seam lost owner.");
            Check(fight.Events[0].Hit.TryScaleOutgoing(2, out _) && Math.Abs(strike.FinalDamage - .4f) < .00001, "Outgoing child mutation did not affect native strike.");
            fight.Events.Clear(); victim.Health = .8f; strike.IsBlocked = true; strike.IsCritical = true;
            fight.Resolved(contact, strike, 1);
            Check(fight.Events.Count == 4 && fight.Events.Count(e => e.Type == ModEffectEvent.DamageDealt) == 1 && fight.Events.Count(e => e.Type == ModEffectEvent.DamageReceived) == 1 && fight.Events.Count(e => e.Type == ModEffectEvent.Block) == 1 && fight.Events.Count(e => e.Type == ModEffectEvent.Critical) == 1, "Child resolved damage duplicated or omitted events.");
            Check(fight.Events.Single(e => e.Type == ModEffectEvent.DamageDealt).Side == (reverse ? "opponent" : "player") && fight.Events.All(e => Math.Abs(e.Damage.Damage - .2f) < .00001), "Wrong damage owner or observed amount.");
            fight.Events.Clear(); child.Owner = new Model(); fight.Resolved(contact, strike, .8f);
            Check(fight.Events.Count == 1 && fight.Events[0].Type == ModEffectEvent.Block, "Unrelated source/no health loss credited main attacker.");
            fight.Events.Clear(); contact.sourceModel = new Model { Health = .5f }; child.Owner = root; fight.Resolved(contact, strike, 1);
            Check(fight.Events.Count == 0, "Damage to unrelated NPC credited main-fighter pair.");
        }
    }

    private static void CheckStatusIcons()
    {
        var fight = new FightHarness { Player = new Model { Name = "player" }, Opponent = new Model { Name = "opponent" } };
        object key = "master";
        Check(fight.Show(fight.Player, key, AssetId.Parse("de128:UI/Skills/IconMasterOfStyle_Blue"), 300, 0, out var error) && error == "",
            "Could not show timed status icon.");
        Check(fight.VisibleAdds == 1 && fight.VisibleRemoves == 0, "Status icon did not use recovered add path once.");
        var action = fight.IconAction(fight.Player, key);
        Check(action != null && action.IconPath == "de128:UI/Skills/IconMasterOfStyle_Blue" && action.ShowExpiration && action.DurationFrames == 300,
            "Status icon lost sprite/expiration metadata.");
        fight.Advance(299);
        Check(fight.HasIcon(fight.Player, key) && fight.IconAction(fight.Player, key).ElapsedFrames == 299,
            "Status icon expired early or timer did not advance.");
        fight.Advance(1);
        Check(!fight.HasIcon(fight.Player, key) && fight.VisibleRemoves == 1, "300-frame status icon did not expire exactly.");

        key = "relentless";
        Check(fight.Show(fight.Player, key, AssetId.Parse("de128:UI/Skills/IconCrackedApple_Blue"), 300, 15, out _), "Relentless icon show failed.");
        Check(fight.IconAction(fight.Player, key).EclipseStackCount == 15, "Relentless stack count did not reach the native icon action.");
        Check(fight.Show(fight.Player, key, AssetId.Parse("de128:UI/Skills/IconCrackedApple_Blue"), 120, 7, out _), "Status icon refresh failed.");
        Check(fight.VisibleRemoves == 2 && fight.IconAction(fight.Player, key).DurationFrames == 120 &&
            fight.IconAction(fight.Player, key).EclipseStackCount == 7,
            "Refresh must clear the old recovered icon and replace its timer/stack count.");
        Check(fight.Clear(fight.Player, key, out _) && !fight.HasIcon(fight.Player, key) && fight.VisibleRemoves == 3,
            "Explicit status-icon clear failed.");
        Check(fight.Clear(fight.Player, key, out _) && fight.VisibleRemoves == 3, "Clearing an absent icon must be idempotent.");
    }
}
