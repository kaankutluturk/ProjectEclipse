using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class IntervalAttack : IntervalAnimation
{
	public class Factors
	{
		public bool IsFactorSet;

		public bool IsMultiplierSet;

		public float Factor;

		public float FactorMultiplier;

		public Factors(float factor = 1f, float factorMultiplier = 1f, bool isFactorSet = false, bool isMultiplierSet = false)
		{
			IsFactorSet = isFactorSet;
			IsMultiplierSet = isMultiplierSet;
			Factor = factor;
			FactorMultiplier = factorMultiplier;
		}

		public void UpdateFactor()
		{
			if (IsFactorSet && IsMultiplierSet)
			{
				Factor *= FactorMultiplier;
			}
		}
	}

	public class Reaction
	{
		public string Name = string.Empty;

		public int Start = -1;

		public int EndFrameValue = -1;

		// best guess for name
		public int EndFrame => EndFrameValue;


		public Reaction()
		{
		}

		public Reaction(string _name, int start, int endFrame)
		{
			Name = _name;
			Start = start;
			EndFrameValue = endFrame;
		}
	}

	private bool ignoresInvulnerable;

	private Factors playerFactors = new Factors();

	private Factors opponentFactors = new Factors();

	private bool hasAttackingParts;

	private bool reservedFlagA;

	private bool reservedFlagB;

	private RuleAppliance appliance;

	private float _Damage;

	private bool noCritical;

	private string _BodyPart;

	private Vector3f impulse = new Vector3f();

	private int _ComboTime;

	private List<Reaction> hitReactions = new List<Reaction>();

	// best guess for name
	public IReadOnlyList<Reaction> HitReactions => hitReactions;


	private bool hasEffect;

	private bool ignoresBlock;

	private List<string> ignoredBlockNames;

	private List<string> ignoredInvulnerableNames;

	private List<string> reservedListA;

	private List<string> reservedListB;

	private List<string> attackingParts = new List<string>();

	private List<global::Pair<string, float>> damageAttributes = new List<global::Pair<string, float>>();

	private List<string> defenseTypes = new List<string>();

	public bool IgnoresInvulnerable
	{
		get
		{
			return GetIgnoresInvulnerable();
		}
	}

	public bool HasAttackingParts
	{
		get
		{
			return GetHasAttackingParts();
		}
	}

	public RuleAppliance Appliance
	{
		get
		{
			return GetAppliance();
		}
		set
		{
			SetAppliance(value);
		}
	}

	public float Damage
	{
		get
		{
			return GetDamage();
		}
	}

	public bool NoCritical
	{
		get
		{
			return GetNoCritical();
		}
	}

	public string BodyPart
	{
		get
		{
			return GetBodyPart();
		}
	}

	public Vector3f Impulse
	{
		get
		{
			return GetImpulse();
		}
	}

	public int ComboTime
	{
		get
		{
			return GetComboTime();
		}
	}

	public bool HasEffect
	{
		get
		{
			return GetHasEffect();
		}
	}

	public bool IgnoresBlock
	{
		get
		{
			return GetIgnoresBlock();
		}
	}

	public List<string> IgnoredBlockNames
	{
		get
		{
			return GetIgnoredBlockNames();
		}
	}

	public List<string> IgnoredInvulnerableNames
	{
		get
		{
			return GetIgnoredInvulnerableNames();
		}
	}

	public List<string> AttackingParts
	{
		get
		{
			return GetAttackingParts();
		}
	}

	public List<global::Pair<string, float>> DamageAttributes
	{
		get
		{
			return GetDamageAttributes();
		}
	}

	public List<string> DefenseTypes
	{
		get
		{
			return GetDefenseTypes();
		}
	}

	public IntervalAttack()
		: base(IntervalType.INTERVAL_ATTACK)
	{
	}

	public bool GetIgnoresInvulnerable()
	{
		return ignoresInvulnerable;
	}

	public bool GetHasAttackingParts()
	{
		return hasAttackingParts;
	}

	public RuleAppliance GetAppliance()
	{
		return appliance;
	}

	public void SetAppliance(RuleAppliance value)
	{
		appliance = value;
	}

	public float GetDamage()
	{
		return _Damage;
	}

	public bool GetNoCritical()
	{
		return noCritical;
	}

	public string GetBodyPart()
	{
		return _BodyPart;
	}

	public Vector3f GetImpulse()
	{
		return impulse;
	}

	public int GetComboTime()
	{
		return _ComboTime;
	}

	public bool GetHasEffect()
	{
		return hasEffect;
	}

	public bool GetIgnoresBlock()
	{
		return ignoresBlock;
	}

	public List<string> GetIgnoredBlockNames()
	{
		return ignoredBlockNames;
	}

	public List<string> GetIgnoredInvulnerableNames()
	{
		return ignoredInvulnerableNames;
	}

	public List<string> GetAttackingParts()
	{
		return attackingParts;
	}

	// best guess for name
	public List<global::Pair<string, float>> GetDamageAttributes()
	{
		return damageAttributes;
	}

	public List<string> GetDefenseTypes()
	{
		return defenseTypes;
	}

	// Eclipse: guarded mod patches edit an already-parsed attack in place and restore it.
	internal void EclipseSetDamage(float value)
	{
		_Damage = value;
	}

	internal void EclipseSetAttackingParts(IEnumerable<string> parts)
	{
		attackingParts.Clear();
		attackingParts.AddRange(parts);
		hasAttackingParts = attackingParts.Count > 0;
	}

	public string GetReactionName(int frame)
	{
		foreach (Reaction item in hitReactions)
		{
			int reactionStart = item.Start;
			int reactionEnd = item.EndFrameValue;
			if (reactionStart <= frame && frame <= reactionEnd)
			{
				return item.Name;
			}
		}
		return string.Empty;
	}

	public static float GetItemFactor(float attributeValue, float baseValue, float doublingRange)
	{
		return Mathf.Pow(2f, (attributeValue - baseValue) / doublingRange);
	}

	public void UpdateFactor(RuleAppliance ruleAppliance)
	{
		switch (ruleAppliance)
		{
		case RuleAppliance.AppliancePlayer:
			playerFactors.UpdateFactor();
			break;
		case RuleAppliance.ApplianceOpponent:
			opponentFactors.UpdateFactor();
			break;
		case RuleAppliance.ApplianceAll:
			playerFactors.UpdateFactor();
			opponentFactors.UpdateFactor();
			break;
		}
	}

	public Factors GetFactors(RuleAppliance ruleAppliance)
	{
		switch (ruleAppliance)
		{
		case RuleAppliance.AppliancePlayer:
			return playerFactors;
		case RuleAppliance.ApplianceOpponent:
			return opponentFactors;
		default:
			return opponentFactors;
		}
	}

	protected override void ParseInside()
	{
		hasEffect = !NodeInterval.Attributes["NoEffect"].ParseBool();
		XmlNode xmlNode = NodeInterval["IgnoresBlock"];
		if (xmlNode != null)
		{
			ignoresBlock = true;
			string text = xmlNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			// Model.Strike uses an empty list to bypass every block interval.
			// Splitting an absent Name into [""] instead targets a nonexistent guard.
			ignoredBlockNames = new List<string>(text.Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries));
		}
		XmlNode xmlNode2 = NodeInterval["IgnoresInvulnerable"];
		if (xmlNode2 != null)
		{
			ignoresInvulnerable = true;
			string text2 = xmlNode2.Attributes["Name"].GetStringOrDefault(string.Empty);
			// An empty native list bypasses every invulnerability interval, just as
			// the adjacent IgnoresBlock parser does for an absent Name attribute.
			ignoredInvulnerableNames = new List<string>(text2.Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries));
		}
		XmlNode xmlNode3 = NodeInterval["AttackingParts"];
		if (xmlNode3 != null)
		{
			foreach (XmlNode childNode in xmlNode3.ChildNodes)
			{
				string item = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				attackingParts.Add(item);
			}
		}
		hasAttackingParts = attackingParts.Count > 0;
		foreach (XmlNode childNode2 in NodeInterval.ChildNodes)
		{
			if (childNode2.Name == "Hit")
			{
				string text3 = childNode2.Attributes["Name"].GetStringOrDefault(string.Empty);
				int num = childNode2.Attributes["Start"].ParseInt(Start);
				int num2 = childNode2.Attributes["End"].ParseInt(EndFrameValue);
				if (num < Start || EndFrameValue < num)
				{
					GameLog.Error("StartFrame ({0}) is outside of attack interval ({1}-{2}) - {3}", num, Start, EndFrameValue, text3);
				}
				if (num2 < Start || EndFrameValue < num2)
				{
					GameLog.Error("EndFrame ({0}) is outside of attack interval ({1}-{2}) - {3}", num2, Start, EndFrameValue, text3);
				}
				Reaction item2 = new Reaction(text3, num, num2);
				hitReactions.Add(item2);
			}
		}
		if (NodeInterval["Impulse"] != null)
		{
			impulse.SetX(NodeInterval["Impulse"].Attributes["X"].ParseFloat());
			impulse.SetY(NodeInterval["Impulse"].Attributes["Y"].ParseFloat());
			impulse.SetZ(NodeInterval["Impulse"].Attributes["Z"].ParseFloat());
		}
		_ComboTime = ((NodeInterval["Combo"] != null) ? NodeInterval["Combo"].Attributes["Time"].ParseInt() : 0);
		XmlNode xmlNode6 = NodeInterval["Damage"];
		_Damage = xmlNode6.Attributes["Value"].ParseFloat();
		noCritical = xmlNode6.Attributes["NoCritical"].ParseBool();
		_BodyPart = xmlNode6.Attributes["BodyPart"].GetStringOrDefault(string.Empty);
		ParseFactorAndDefenseItems(xmlNode6);
	}

	private void ParseFactorAndDefenseItems(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string name = childNode.Name;
			string text = childNode.Attributes["Type"].GetStringOrDefault(string.Empty);
			float shift = childNode.Attributes["Shift"].ParseFloat();
			if (name == "Damage")
			{
				damageAttributes.Add(new global::Pair<string, float>(text, shift));
				// Ranged and magic damage remain unblockable even in cast/follow-up
				// intervals that omit IgnoresBlock. Ordinary melee keeps its XML rules.
				if (!ignoresBlock && (text == "RangedDamage" || text == "MagicDamage"))
				{
					ignoresBlock = true;
					ignoredBlockNames = new List<string>();
				}
				continue;
			}
			if (name == "Defense")
			{
				defenseTypes.Add(text);
				continue;
			}
			GameLog.Error("Strange node name xml {0}", name);
		}
	}
}
