using System.Collections.Generic;
using UnityEngine;

namespace Eclipse.Underworld.Diagnostics
{
	public static class UnderworldRaidDiagnostics
	{
		// Keep the reference XML's WarriorPower, equipment and AttributesAlign.
		// Replacing them with arbitrary player-relative offsets erased boss tiers
		// and made equipment upgrades ineffective. Alignment is evaluated at hit time.
		public static void LogEnemies(FightList fight, ModelParameters player, List<ModelParameters> enemies)
		{
			Battle battle = fight.Battle;
			Zone zone = battle == null ? null : battle.ParentZone;
			if (!UnderworldZonePolicy.IsRaidZone(zone))
			{
				return;
			}

			foreach (ModelParameters enemy in enemies)
			{
				int weapon = 0;
				int defense = 0;
				int playerWeapon = 0;
				int playerDefense = 0;
				enemy.FinalAttributes.Get("WeaponDamage", ref weapon);
				enemy.FinalAttributes.Get("BodyDefense", ref defense);
				player.FinalAttributes.Get("WeaponDamage", ref playerWeapon);
				player.FinalAttributes.Get("BodyDefense", ref playerDefense);
				Debug.Log("[Underworld] battle=" + battle.get_Name() +
					" warriorPower=" + enemy.WarriorPower + " healthBars=" + enemy.HealthBarCount +
					" bossWeapon=" + weapon + " bossDefense=" + defense +
					" playerWeapon=" + playerWeapon + " playerDefense=" + playerDefense +
					" alignmentRules=" + enemy.AttributeAlignments.Count);
			}
		}
	}
}
