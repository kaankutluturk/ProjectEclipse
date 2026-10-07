using System.Collections.Generic;
using UnityEngine;

public class ModelConditions
{
	public class ModelPositions
	{
		public Vector2 LeftWall = default(Vector2);

		public Vector2 RightWall = default(Vector2);

		public ModelObject Body;

		public void Clear()
		{
			Body = null;
		}
	}

	public KeyData PressedKeys;

	public List<ItemInfo> Items;

	public List<IntervalAnimation> Intervals;

	public List<IntervalAnimation> OtherIntervals;

	public List<IntervalAnimation> ParentIntervals;

	public List<string> CandidateMoveNames = new List<string>();

	public List<string> SelfAnimationNames = new List<string>();

	public List<string> OtherAnimationNames = new List<string>();

	public List<string> ParentAnimationNames = new List<string>();

	public List<string> ChildAnimationNames = new List<string>();

	public Dictionary<string, float> PerkVariables = new Dictionary<string, float>();

	public Dictionary<string, string> PerkStringVariables = new Dictionary<string, string>();

	public string ModelName;
    public string EclipseCharacterId;

	public SceneTypes SceneType;

	public ModelNode SelfNode;

	public ModelNode ParentNode;

	public ModelNode OtherNode;

	public ModelNode ChildNode;

	public EventAnimation CurrentEvent;

	public EndRoundType EndRoundType;

	public int BossAbilityState;

	public int AnimationSign;

	public int PivotPairSelector;

	public int SelfSign;

	public int OtherSign;

	public int ParentSign;

	public int ChildSign;

	public float CurrentHealth;

	public float MaxHealth;

	public int RoundStage;

	public bool IsKeyCheckEnabled;

	public bool IsPlayer;

	public bool HasOther;

	public bool IsWeapon;

	public bool SelfIsPhysics;

	public bool OtherIsPhysics;

	public bool ParentIsPhysics;

	public int CurrentFrame;

	public int ImpulseX;

	public bool RoundEnded;

	public bool IsWinner;

	public object StrikeResult;

	public List<PerkInfoItem> SelfPerks;

	public List<PerkInfoItem> OtherPerks;

	public List<PerksStage.ActionPerk> SelfActionPerks = new List<PerksStage.ActionPerk>();

	public List<PerksStage.ActionPerk> OtherActionPerks = new List<PerksStage.ActionPerk>();

	public List<PerksStage.ActionPerk> SelfExpiredPerks = new List<PerksStage.ActionPerk>();

	public List<PerksStage.ActionPerk> OtherActionPerksSecondary = new List<PerksStage.ActionPerk>();

	public int NoRangedFlag;

	public int MagicCharges;

	public int RaidCharges;

	public ModelPositions ParentPositions = new ModelPositions();

	public ModelPositions SelfPositions = new ModelPositions();

	public ModelPositions OtherPositions = new ModelPositions();

	public ModelPositions ChildPositions = new ModelPositions();

	public void Reset()
	{
		PerkVariables.Clear();
		PerkStringVariables.Clear();
		ParentPositions.Clear();
		SelfPositions.Clear();
		OtherPositions.Clear();
		ChildPositions.Clear();
	}
}
