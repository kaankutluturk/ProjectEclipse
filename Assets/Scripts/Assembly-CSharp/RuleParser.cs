using System.Collections.Generic;
using System.Xml;

public class RuleParser
{
	public static void ParseRules(XmlNode node, List<Rule> OEMALIFPGPO)
	{
		if (node == null)
		{
			return;
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string name = childNode.Name;
			if (name == "RulesWithConditions")
			{
				// This runtime predates conditional rule wrappers. Keep the contained
				// rules instead of rejecting the entire block. Their quest-style
				// Conditions remain in the source for a future conditional-rule port.
				ParseRules(childNode["RuleList"], OEMALIFPGPO);
				continue;
			}
			if (name == "Level")
			{
				ParseLevelRules(childNode, OEMALIFPGPO);
				continue;
			}
			Rule gKAJMMNJBGA = ParseRule(childNode);
			if (gKAJMMNJBGA != null)
			{
				OEMALIFPGPO.Add(gKAJMMNJBGA);
			}
		}
	}

	public static Rule ParseRule(XmlNode node)
	{
		string name = node.Name;
		switch (name)
		{
		case "RequireItem":
			return new ItemRule(node);
		case "EquipItem":
			return new EquipItemRule(node);
		case "RandomAquiredItem":
			return new RandomAquiredItemRule(node);
		case "NoButton":
			return new NoButtonRule(node);
		case "NoAnimation":
			return new NoAnimationRule(node);
		case "Ringout":
			return ParseInFightRule(Rule.RuleType.RuleRingout, node);
		case "HotGround":
			return ParseInFightRule(Rule.RuleType.RuleHotGround, node);
		case "LoseFall":
			return ParseInFightRule(Rule.RuleType.RuleLoseFall, node);
		case "Regeneration":
			return ParseInFightRule(Rule.RuleType.RuleRegeneration, node);
		case "Attributes":
			return ParseInFightRule(Rule.RuleType.RuleAttributes, node);
		case "DamageFactor":
			return ParseInFightRule(Rule.RuleType.RuleDamageFactor, node);
		case "RemoveInterval":
			return ParseInFightRule(Rule.RuleType.RuleRemoveInterval, node);
		case "Crazy":
			return ParseInFightRule(Rule.RuleType.RuleCrazy, node);
		case "Lifesteal":
			return ParseInFightRule(Rule.RuleType.RuleLifeSteal, node);
		case "NoHealthBar":
			return ParseInFightRule(Rule.RuleType.RuleNoHealthBar, node);
		case "TimeOutWin":
			return ParseInFightRule(Rule.RuleType.RuleTimeoutWin, node);
		case "Combo":
			return ParseInFightRule(Rule.RuleType.RuleCombo, node);
		case "Darkness":
			return ParseInFightRule(Rule.RuleType.RuleDarkness, node);
		case "LightInTheDarkness":
			return ParseInFightRule(Rule.RuleType.RuleLightInTheDarkness, node);
		case "Points":
			return ParseInFightRule(Rule.RuleType.RulePoints, node);
		case "NoBulletsReplenishment":
			return ParseInFightRule(Rule.RuleType.RuleNoBulletsReplenishment, node);
		case "RechargeMagicEachRound":
			return ParseInFightRule(Rule.RuleType.RuleRechargeMagicEachRound, node);
		case "Perk":
			return ParseInFightRule(Rule.RuleType.RulePerk, node);
		case "NoPerks":
			return ParseInFightRule(Rule.RuleType.RuleNoPerks, node);
		case "RandomRule":
			return new RandomRule(node);
		case "ComplexRule":
			return new ComplexRule(node);
		case "Description":
			return new DescriptionRule(node);
		case "WinCombo":
			return ParseInFightRule(Rule.RuleType.RuleWinCombo, node);
		case "WinStyle":
			return ParseInFightRule(Rule.RuleType.RuleWinStyle, node);
		case "WinShock":
			return ParseInFightRule(Rule.RuleType.RuleWinShock, node);
		case "ChangeFight":
			return new ChangeFightRule(node);
		case "SetTactic":
			return ParseInFightRule(Rule.RuleType.RuleTactic, node);
		case "InvertJoystick":
			return ParseInFightRule(Rule.RuleType.RuleInvertJoystick, node);
		case "RandomArea":
			return ParseInFightRule(Rule.RuleType.RuleRandomArea, node);
		case "RatingEvaluation":
			return new RatingEvaluationRule(node);
		case "Invulnerability":
			return ParseInFightRule(Rule.RuleType.RuleInvulnerability, node);
		case "CurrencyCost":
			return new CurrencyCostRule(node);
		case "RaidCurrencyCost":
			return new RaidCurrencyCostRule(node);
		case "Resistance":
			return ParseInFightRule(Rule.RuleType.RuleResistance, node);
		case "Avatar":
			return new AvatarRule(node);
		case "Name":
			return new NameRule(node);
		default:
			GameLog.Error("RuleParser::parseRules - unknown node name: " + name);
			return null;
		}
	}

	public static FightStatistics.FightStyle ParseStyleType(XmlNode node)
	{
		string text = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		switch (text)
		{
		case "Turtle":
			return FightStatistics.FightStyle.STYLE_TURTLE;
		case "Hard":
			return FightStatistics.FightStyle.STYLE_HARD;
		case "Brutal":
			return FightStatistics.FightStyle.STYLE_BRUTAL;
		case "Aggressive":
			return FightStatistics.FightStyle.STYLE_AGGRESSIVE;
		case "Crazy":
			return FightStatistics.FightStyle.STYLE_CRAZY;
		case "Fantastic":
			return FightStatistics.FightStyle.STYLE_FANTASTIC;
		default:
			GameLog.Error("RuleParser::parseStyleType - unknown type: " + text);
			return FightStatistics.FightStyle.STYLE_TURTLE;
		}
	}

	protected static InFightRule ParseInFightRule(Rule.RuleType LFLGCDNKNJI, XmlNode node)
	{
		string text = node.Attributes["ApplyTo"].GetStringOrDefault("All");
		RuleAppliance eJPOJJKKICO = RuleAppliance.ApplianceNone;
		switch (text)
		{
		case "Player":
			eJPOJJKKICO = RuleAppliance.AppliancePlayer;
			break;
		case "Bot":
			eJPOJJKKICO = RuleAppliance.ApplianceOpponent;
			break;
		case "All":
			eJPOJJKKICO = RuleAppliance.ApplianceAll;
			break;
		default:
			GameLog.Error("RuleParser::parseInFightRule ERROR - wrong rule applyTo %s", text);
			break;
		}
		switch (LFLGCDNKNJI)
		{
		case Rule.RuleType.RuleHotGround:
			return new HotGroundRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleRingout:
			return new RingOutRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleRegeneration:
			return new RegenerationRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleAttributes:
			return new AttributesRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleDamageFactor:
			return new DamageFactorRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleLoseFall:
			return new LoseFallRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleRemoveInterval:
			return new RemoveIntervalRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleCrazy:
			return new CrazyRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleLifeSteal:
			return new LifeStealRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleNoHealthBar:
			return new NoHealthBarRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleTimeoutWin:
			return new TimeoutWinRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleCombo:
			return new ComboRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleDarkness:
			return new DarknessRule(node, RuleAppliance.AppliancePlayer);
		case Rule.RuleType.RuleLightInTheDarkness:
			return new Eclipse.Combat.LightInTheDarknessRule(node, eJPOJJKKICO);
		case Rule.RuleType.RulePoints:
			return new PointsRule(node, RuleAppliance.ApplianceAll);
		case Rule.RuleType.RuleNoBulletsReplenishment:
			return new NoBulletsReplenishmentRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleRechargeMagicEachRound:
			return new RechargeMagicEachRoundRule(node, eJPOJJKKICO);
		case Rule.RuleType.RulePerk:
			return new PerkRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleNoPerks:
			return new NoPerksRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleWinCombo:
			return new WinComboRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleWinStyle:
			return new WinStyleRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleWinShock:
			return new WinShockRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleTactic:
			return new TacticRule(node);
		case Rule.RuleType.RuleInvertJoystick:
			return new InvertJoystickRule(node);
		case Rule.RuleType.RuleRandomArea:
			return new RandomAreaRule(node);
		case Rule.RuleType.RuleInvulnerability:
			return new InvulnerabilityRule(node, eJPOJJKKICO);
		case Rule.RuleType.RuleResistance:
			return new ResistanceRule(node, eJPOJJKKICO);
		default:
			GameLog.Error("RuleParser::parseInFightRule ERROR - wrong rule type %i", LFLGCDNKNJI);
			return null;
		}
	}

	protected static void ParseLevelRules(XmlNode node, List<Rule> OEMALIFPGPO)
	{
		int kLJOBCIINOF = node.Attributes["Min"].ParseInt();
		int nMPCMFDGOKA = node.Attributes["Max"].ParseInt(int.MaxValue);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			Rule gKAJMMNJBGA = ParseRule(childNode);
			if (gKAJMMNJBGA != null)
			{
				gKAJMMNJBGA.MaxLevel = nMPCMFDGOKA;
				gKAJMMNJBGA.MinLevel = kLJOBCIINOF;
				OEMALIFPGPO.Add(gKAJMMNJBGA);
			}
		}
	}
}
