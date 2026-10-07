using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Nekki.SF2.Core.Fights.Renders.Model;
using UnityEngine;

public class Model : global::EventDispatcher<object>
{
	private readonly Dictionary<string, int> _transientPerkFlags = new Dictionary<string, int>();

	public void AddTransientPerkFlag(string name, int frames)
	{
		if (!string.IsNullOrEmpty(name))
			_transientPerkFlags[name] = Eclipse.Multiplayer.VersusDeterminism.FrameStamp + Mathf.Max(1, frames);
	}

	public bool HasTransientPerkFlag(string name)
	{
		int expires;
		if (!_transientPerkFlags.TryGetValue(name, out expires))
			return false;
		if (Eclipse.Multiplayer.VersusDeterminism.FrameStamp <= expires)
			return true;
		_transientPerkFlags.Remove(name);
		return false;
	}

	public class AnimationEventIndex
	{
		public List<InfoAnimation> AllAnimations = new List<InfoAnimation>();

		public List<InfoAnimation> OnRoundStage = new List<InfoAnimation>();

		public List<InfoAnimation> OnKeyPressed = new List<InfoAnimation>();

		public List<InfoAnimation> OnKeyReleased = new List<InfoAnimation>();

		public List<InfoAnimation> OnAnimationStart = new List<InfoAnimation>();

		public List<InfoAnimation> OnAnimationEnd = new List<InfoAnimation>();

		public List<InfoAnimation> OnIntervalStart = new List<InfoAnimation>();

		public List<InfoAnimation> OnIntervalEnd = new List<InfoAnimation>();

		public List<InfoAnimation> OnHit = new List<InfoAnimation>();

		public List<InfoAnimation> OnStrike = new List<InfoAnimation>();

		public List<InfoAnimation> OnEveryFrame = new List<InfoAnimation>();

		public List<InfoAnimation> OnBirth = new List<InfoAnimation>();

		public List<InfoAnimation> OnModExpires = new List<InfoAnimation>();

		public List<InfoAnimation> ShopAnimations = new List<InfoAnimation>();

		public void Clear()
		{
			AllAnimations.Clear();
			OnRoundStage.Clear();
			OnKeyPressed.Clear();
			OnAnimationStart.Clear();
			OnAnimationEnd.Clear();
			OnIntervalStart.Clear();
			OnIntervalEnd.Clear();
			OnHit.Clear();
			OnStrike.Clear();
			OnEveryFrame.Clear();
			OnBirth.Clear();
			OnModExpires.Clear();
			ShopAnimations.Clear();
		}

		public void AddAnimations(List<InfoAnimation> MAHEJFLCCHP)
		{
			for (int i = 0; i < MAHEJFLCCHP.Count; i++)
			{
				InfoAnimation pJAHIOELGGD = MAHEJFLCCHP[i];
				if (pJAHIOELGGD.MoveData.ShopData.IsExists)
				{
					ShopAnimations.Add(pJAHIOELGGD);
				}
				List<EventAnimation> aJCMBMJGJEG = pJAHIOELGGD.MoveData.Events;
				for (int j = 0; j < aJCMBMJGJEG.Count; j++)
				{
					List<InfoAnimation> list = GetListForEvent(aJCMBMJGJEG[j].Type);
					int count = list.Count;
					if (count == 0 || list[count - 1] != pJAHIOELGGD)
					{
						list.Add(pJAHIOELGGD);
					}
				}
				AllAnimations.Add(pJAHIOELGGD);
			}
		}

		public List<InfoAnimation> GetListForEvent(EventAnimation.EventAnimationType IGABHEMGKKE)
		{
			switch (IGABHEMGKKE)
			{
			case EventAnimation.EventAnimationType.EVENT_ANIMATION_END:
				return OnAnimationEnd;
			case EventAnimation.EventAnimationType.EVENT_ANIMATION_START:
				return OnAnimationStart;
			case EventAnimation.EventAnimationType.EVENT_BIRTH:
				return OnBirth;
			case EventAnimation.EventAnimationType.EVENT_EVERY_FRAME:
				return OnEveryFrame;
			case EventAnimation.EventAnimationType.EVENT_HIT:
				return OnHit;
			case EventAnimation.EventAnimationType.EVENT_INTERVAL_END:
				return OnIntervalEnd;
			case EventAnimation.EventAnimationType.EVENT_INTERVAL_START:
				return OnIntervalStart;
			case EventAnimation.EventAnimationType.EVENT_KEY_PRESSED:
				return OnKeyPressed;
			case EventAnimation.EventAnimationType.EVENT_KEY_RELEASED:
				return OnKeyReleased;
			case EventAnimation.EventAnimationType.EVENT_MOD_EXPIRES:
				return OnModExpires;
			case EventAnimation.EventAnimationType.EVENT_ROUND_STAGE:
				return OnRoundStage;
			case EventAnimation.EventAnimationType.EVENT_STRIKE:
				return OnStrike;
			default:
				return AllAnimations;
			}
		}
	}

	public class TriggerEventIndex
	{
		public List<Trigger> AllTriggers = new List<Trigger>();

		public List<Trigger> OnRoundStage = new List<Trigger>();

		public List<Trigger> OnKeyPressed = new List<Trigger>();

		public List<Trigger> OnKeyReleased = new List<Trigger>();

		public List<Trigger> OnAnimationStart = new List<Trigger>();

		public List<Trigger> OnAnimationEnd = new List<Trigger>();

		public List<Trigger> OnIntervalStart = new List<Trigger>();

		public List<Trigger> OnIntervalEnd = new List<Trigger>();

		public List<Trigger> OnHit = new List<Trigger>();

		public List<Trigger> OnStrike = new List<Trigger>();

		public List<Trigger> OnEveryFrame = new List<Trigger>();

		public List<Trigger> OnBirth = new List<Trigger>();

		public List<Trigger> OnModExpires = new List<Trigger>();

		public void Clear()
		{
			AllTriggers.Clear();
			OnRoundStage.Clear();
			OnKeyPressed.Clear();
			OnKeyReleased.Clear();
			OnAnimationStart.Clear();
			OnAnimationEnd.Clear();
			OnIntervalStart.Clear();
			OnIntervalEnd.Clear();
			OnHit.Clear();
			OnStrike.Clear();
			OnEveryFrame.Clear();
			OnBirth.Clear();
			OnModExpires.Clear();
		}

		public void AddTriggers(List<Trigger> JJAEKPONOBM)
		{
			foreach (Trigger item in JJAEKPONOBM)
			{
				List<EventAnimation> aJCMBMJGJEG = item.Definition.Events;
				foreach (EventAnimation item2 in aJCMBMJGJEG)
				{
					List<Trigger> list = GetListForEvent(item2.Type);
					int count = list.Count;
					if (count == 0 || list[count - 1] != item)
					{
						list.Add(item);
					}
				}
				AllTriggers.Add(item);
			}
		}

		public List<Trigger> GetListForEvent(EventAnimation.EventAnimationType IGABHEMGKKE)
		{
			switch (IGABHEMGKKE)
			{
			case EventAnimation.EventAnimationType.EVENT_ROUND_STAGE:
				return OnRoundStage;
			case EventAnimation.EventAnimationType.EVENT_KEY_PRESSED:
				return OnKeyPressed;
			case EventAnimation.EventAnimationType.EVENT_KEY_RELEASED:
				return OnKeyReleased;
			case EventAnimation.EventAnimationType.EVENT_ANIMATION_START:
				return OnAnimationStart;
			case EventAnimation.EventAnimationType.EVENT_ANIMATION_END:
				return OnAnimationEnd;
			case EventAnimation.EventAnimationType.EVENT_INTERVAL_START:
				return OnIntervalStart;
			case EventAnimation.EventAnimationType.EVENT_INTERVAL_END:
				return OnIntervalEnd;
			case EventAnimation.EventAnimationType.EVENT_HIT:
				return OnHit;
			case EventAnimation.EventAnimationType.EVENT_STRIKE:
				return OnStrike;
			case EventAnimation.EventAnimationType.EVENT_EVERY_FRAME:
				return OnEveryFrame;
			case EventAnimation.EventAnimationType.EVENT_BIRTH:
				return OnBirth;
			case EventAnimation.EventAnimationType.EVENT_MOD_EXPIRES:
				return OnModExpires;
			default:
				return AllTriggers;
			}
		}
	}

	public class EventDelayedModel
	{
		public object data;

		public EventAnimation.EventAnimationType EventType;
	}

	public class EventModel
	{
		public Model KJDFJPBIGJC;

        // best guess for name
        public Model SourceModel => KJDFJPBIGJC;

		public Model Opponent;

		public object Data;

		public string ConditionName;

		public void Clear()
		{
			KJDFJPBIGJC = null;
			Opponent = null;
			Data = null;
		}
	}

	public class DisarmData
	{
		public Model Owner;

		public List<PerkInfoItem> LostPerks;

		public DisarmData(Model _Model, List<PerkInfoItem> NDEOKNAOAKM)
		{
			Owner = _Model;
			LostPerks = NDEOKNAOAKM;
		}
	}

	public class EventActBtnSettings
	{
		public FightCID Button;

		public float Value;

		public int FrameCount;

		public int BulletsCount;

		public EventActBtnSettings(FightCID ONKIPLHLPCO, float _value, int _frames = -1, int PEGMAGCDDDC = -1)
		{
			Button = ONKIPLHLPCO;
			Value = _value;
			FrameCount = _frames;
			BulletsCount = PEGMAGCDDDC;
		}
	}

	public enum ModelEventType
	{
		ON_INTERVAL_START = 0,
		ON_INTERVAL_END = 1,
		ON_ANIMATION_START = 2,
		ON_ANIMATION_END = 3,
		ON_EVERY_FRAME = 4,
		ON_MODEL_DELETE = 5,
		ON_MODEL_CREATE = 6,
		ON_START_EFFECT = 7,
		ON_STOP_EFFECT = 8,
		ON_STOP_FOLLOW_EFFECT = 9,
		ON_KEY_PRESS = 10,
		ON_KEY_RELEASE = 11,
		SETTED_ACT_BTN_PERCENTAGE = 12,
		SETTED_ACT_BTN_COUNT = 18,
		ON_COMBO_CHANGE = 13,
		ON_TRY_ON_END = 14,
		ON_SHAKE_SCREEN = 15,
		ON_DISARM = 16,
		ON_ZOOM_EFFECT = 17
	}

	public class StrikeResult
	{
		public Vector3f Point;

		public Vector3f EdgePoint;

		// best guess for name
		public Vector3f Impulse = new Vector3f();

		public ModelEdge VictimEdge;

		public ModelEdge AttackerEdge;

		// best guess for name
		public InfoAnimation AttackAnimation;

		public float BaseDamage;

		public float FinalDamage;

		public float RawDamage;

		public string DefenceAttribute = string.Empty;

		public int Target;

		public int HitIndex;

		public bool IsFirstStrike;

		public bool IsHeadHit;

		public bool IsBlocked;

		public bool IsCritical;

		public bool IsShock;

		public bool IsDisarm;

		public bool IsOverkill;

		public Model Victim;

		// best guess for name
		public Model AttackerModel;

		public List<int> ProcedPerks = new List<int>();

		public int HitsTakenCount;

		public void AddProcedPerk(int KMEFHNNOLLM)
		{
			ProcedPerks.AddIfNotExist(KMEFHNNOLLM);
		}
	}

	public enum FightPhase
	{
		None = 0,
		Prepare = 1,
		Fight = 2,
		Finish = 3
	}

	public class DelayedStrike
	{
		public InfoAnimation Animation;

		public List<string> Names;

		public bool IsStrikeResult;
	}

	protected class WallBounds
	{
		public float LeftX;

		public float RightX;
	}

	protected class WallAlign
	{
		public int Left;

		public int Right;
	}

	private class HitData
	{
		public Vector3f Point;

		public Vector3f Velocity;

		public float Time;

		public bool DataReady;
	}

	private class DelayedPlayRequest
	{
		public InfoAnimation Animation;

		public int Direction;

		public int Count;

		public bool IsFrameShift;

		public int FrameShift = -1;

		public bool Empty
		{
			get
			{
				return IsEmpty();
			}
		}

		public void Clear()
		{
			Animation = null;
			Direction = 0;
			Count = 0;
			IsFrameShift = false;
			FrameShift = -1;
		}

		public bool IsEmpty()
		{
			return Animation == null;
		}
	}

	private class ActBtnsCooldown
	{
		public bool MagicActive;

		public bool MissileActive;

		public bool PunchActive;

		public bool KickActive;

		public bool RaidChargeActive;

		public float MagicProgress;

		public float MagicMax;

		public int PunchCooldownFrames;

		public float MissileProgress;

		public float MissileMax;

		public int MissileFrames;

		public float PunchProgress;

		public float PunchMax;

		public int PunchRegenFrames;

		public float KickProgress;

		public float KickMax;

		public int KickFrames;

		public float RaidChargeProgress;

		public float RaidChargeMax;

		public int RaidChargeFrames;

		public ActBtnsCooldown()
		{
			MagicActive = false;
			MissileActive = false;
			PunchActive = false;
			KickActive = false;
			RaidChargeActive = false;
			MagicProgress = 0f;
			MagicMax = 1f;
			MissileProgress = 0f;
			MissileMax = 1f;
			PunchProgress = 0f;
			PunchMax = 1f;
			KickProgress = 0f;
			KickMax = 1f;
			RaidChargeProgress = 0f;
			RaidChargeMax = 1f;
			PunchCooldownFrames = 0;
			MissileFrames = 0;
			PunchRegenFrames = 0;
			KickFrames = 0;
			RaidChargeFrames = 0;
		}
	}

	private const int RESET_BTN_COOLDOWN_FRAMES = 30;

	public AnimationEventIndex AnimationEvents = new AnimationEventIndex();

	public TriggerEventIndex TriggerEvents = new TriggerEventIndex();

	public EventModel EventData = new EventModel();

	public StrikeResult LastStrike = new StrikeResult();

	public DelayedStrike DelayedStrikeData = new DelayedStrike();

	protected WallBounds wallBounds = new WallBounds();

	protected WallAlign wallAlign = new WallAlign();

	private HitData hitData = new HitData();

	private DelayedPlayRequest pendingPlayRequest = new DelayedPlayRequest();

	private ActBtnsCooldown buttonCooldowns = new ActBtnsCooldown();

	public int Index;

	private Model combatTarget;

	public int Sign;

	private float distanceToEnemy;

	private float distanceToBackWall;

	// best guess for name
	public ModelParameters Parameters;

	public int LastComboTime;

	public bool ReservedFlag;

	public List<Model> _Enemies = new List<Model>();

	private bool inputEnabled;

	public bool ItemsReloaded;

	public int RoundStage;

	public bool IsChildModel;

	public bool ReceivedCritical;

	public bool EndStageReached;

	public int FrameInRound;

	public int HitCounter;

	public bool IsInitialized;

	public bool LastHitBlocked;

	public bool LastHitCritical;

	public int LegacyCounterA;

	public int LegacyCounterB;

	public int StyleRank;

	public string StyleName;

	public float StyleProgress;

	public float StyleProgressDelta;

	public int PendingKey = -1;

	public EventAnimation.EventAnimationType LastEventType;

	public InfoAnimation.AnimationKind LastAnimationType;

	private List<WeaponModel> weaponModels = new List<WeaponModel>();

	private List<ModelEdge> edges;

	private List<CurrentEffect> currentEffects = new List<CurrentEffect>();

	private string _Name;

	private string _ExplicitBirthAnimation;

	private ModelConditions _ModelConditions = new ModelConditions();

	private ModelObject _ModelObject;

	private ModelController controller = new ModelController();

	private FightStatistics _Statistics = new FightStatistics();

	private FightPhase fightPhase;

	private ItemInfo disarmedItem;

	private Model parentModel;

	private ModelStrike _Strike;

	private ModelPhysics _Physics;

	private ModelCollision _Collision;

	private ModelAnimation _Animation;

	// Compatibility state used by newer perk actions.  It lives on the model so
	// timed modifiers can be applied and reliably undone by InfoPerk.
	private int _perkSlowFactor = 1;
	private int _perkSlowFrame;
	private bool _perkCollisionDisabled;
	private Color? _perkColor;

	private Eclipse.Rendering.ModelPresentation _presentation;

	private ModelAi ai;

	private ModelStatistics modelStats;

	private int disarmCountdown;

	private bool isDisarmed;

	private bool _IsShock;

	private bool insideArea;

	private int decisionDelay;

	private int legacyCounterC;

	private int fixedSign;

	private float pain;

	private bool aiDecisionReady;

	private bool legacyFlag;

	private bool damageImmune;

	private float damageMultiplier;

	private bool slowMotionAllowed;

	private int round;

	private int magicChargesUsed;

	private int raidChargesUsed;

	private int nonComboHitsTaken;

	private ComboCounter comboCounter = new ComboCounter();

	private float powerMultiplier;

	private float bonusModifier;

	private List<InfoAnimation> availableAnimations = new List<InfoAnimation>();

	private List<Trigger> triggers = new List<Trigger>();

	private float magicChargeFraction;

	private int magicCharges;

	private int raidBullets;

	private bool strikePending;

	private DetailedDamages damageLog = new DetailedDamages();

	private int strikesTaken;

	private Vector3f impulseFactor;

	private float hitEffectScale;

	private float additionalDamage;

	private bool cameraAttached;

	private GameObject _UnityObject;

	public MeshRender _MeshRender;

	private bool forceCritical;

	public Model CombatTarget
	{
		get
		{
			return GetCombatTarget();
		}
	}

	public float DistanceToEnemy
	{
		get
		{
			return GetDistanceToEnemy();
		}
	}

	public float DistanceToBackWall
	{
		get
		{
			return GetDistanceToBackWall();
		}
	}

	public float FrontWallX
	{
		get
		{
			return GetFrontWallX();
		}
	}

	public float BackWallX
	{
		get
		{
			return GetBackWallX();
		}
	}

	public bool InputEnabled
	{
		get
		{
			return IsInputEnabled();
		}
		set
		{
			SetInputEnabled(value);
		}
	}

	public List<WeaponModel> WeaponModels
	{
		get
		{
			return GetWeaponModels();
		}
	}

	public ModelConditions Conditions
	{
		get
		{
			return GetConditions();
		}
	}

	public ModelObject Body
	{
		get
		{
			return GetBodyObject();
		}
	}

	public ModelController Controller
	{
		get
		{
			return GetController();
		}
	}

	public FightStatistics Statistics
	{
		get
		{
			return GetStatistics();
		}
	}

	public FightPhase Phase
	{
		get
		{
			return GetPhase();
		}
	}

	public Model ParentModel
	{
		get
		{
			return GetParentModel();
		}
	}

	public ModelPhysics PhysicsModule
	{
		get
		{
			return GetPhysicsModule();
		}
	}

	public ModelCollision CollisionModule
	{
		get
		{
			return GetCollisionModule();
		}
	}

	public ModelAnimation AnimationModule
	{
		get
		{
			return GetAnimationModule();
		}
	}

	public InfoAnimation CurrentAnimation
	{
		get
		{
			return GetCurrentAnimation();
		}
	}

	public List<IntervalAnimation> Intervals
	{
		get
		{
			return GetIntervals();
		}
	}

	public ModelAi Ai
	{
		get
		{
			return GetAi();
		}
	}

	public ModelStatistics ModelStats
	{
		get
		{
			return GetModelStats();
		}
	}

	public bool IsDisarmed
	{
		get
		{
			return WasDisarmed();
		}
		set
		{
			SetDisarmed(value);
		}
	}

	public bool InShock
	{
		get
		{
			return IsInShock();
		}
		set
		{
			set_IsShock(value);
		}
	}

	public bool InsideArea
	{
		get
		{
			return IsInsideArea();
		}
		set
		{
			SetInsideArea(value);
		}
	}

	public float Pain
	{
		get
		{
			return GetPain();
		}
	}

	public bool AiDecisionReady
	{
		get
		{
			return IsAiDecisionReady();
		}
		set
		{
			SetAiDecisionReady(value);
		}
	}

	public bool DamageImmune
	{
		get
		{
			return IsDamageImmune();
		}
		set
		{
			SetDamageImmune(value);
		}
	}

	public float DamageMultiplier
	{
		get
		{
			return GetDamageMultiplier();
		}
		set
		{
			SetDamageMultiplier(value);
		}
	}

	public bool SlowMotionAllowed
	{
		get
		{
			return IsSlowMotionAllowed();
		}
		set
		{
			SetSlowMotionAllowed(value);
		}
	}

	public int MagicChargesUsed
	{
		get
		{
			return GetMagicChargesUsed();
		}
	}

	public int RaidChargesUsed
	{
		get
		{
			return GetRaidChargesUsed();
		}
	}

	public float PowerMultiplier
	{
		get
		{
			return GetPowerMultiplier();
		}
		set
		{
			SetPowerMultiplier(value);
		}
	}

	public float BonusModifier
	{
		get
		{
			return GetBonusModifier();
		}
		set
		{
			SetBonusModifier(value);
		}
	}

	public List<InfoAnimation> AvailableAnimations
	{
		get
		{
			return GetAvailableAnimations();
		}
	}

	public List<Trigger> Triggers
	{
		get
		{
			return GetTriggers();
		}
	}

	public float MagicChargeFraction
	{
		get
		{
			return GetMagicChargeFraction();
		}
		set
		{
			SetMagicChargeFraction(value);
		}
	}

	public int MagicCharges
	{
		get
		{
			return GetMagicCharges();
		}
		set
		{
			SetMagicCharges(value);
		}
	}

	public int RaidBullets
	{
		get
		{
			return GetRaidBullets();
		}
		set
		{
			SetRaidBullets(value);
		}
	}

	public DetailedDamages DamageLog
	{
		get
		{
			return GetDamageLog();
		}
	}

	public int StrikesTaken
	{
		get
		{
			return GetStrikesTaken();
		}
	}

	public float HitEffectScaleValue
	{
		get
		{
			return GetHitEffectScale();
		}
		set
		{
			set_HitEffectScale(value);
		}
	}

	public float AdditionalDamage
	{
		get
		{
			return GetAdditionalDamage();
		}
		set
		{
			set_AdditionalDamageValue(value);
		}
	}

	public bool CameraAttached
	{
		get
		{
			return IsCameraAttached();
		}
		set
		{
			SetCameraAttached(value);
		}
	}

	public GameObject UnityObject
	{
		get
		{
			return GetGameObject();
		}
	}

	public Vector3f Position
	{
		get
		{
			return GetPosition();
		}
	}

	public bool AiControlled
	{
		get
		{
			return IsAiControlled();
		}
	}

	public bool UserControlled
	{
		get
		{
			return IsUserControlled();
		}
	}

	public bool Dummy
	{
		get
		{
			return IsDummy();
		}
	}

	public bool ModelActive
	{
		get
		{
			return IsModelActive();
		}
	}

	public bool EnemiesPresent
	{
		get
		{
			return HasEnemies();
		}
	}

	public bool IsPlayer
	{
		get
		{
			return IsPlayerModel();
		}
	}

	public virtual bool IsWeaponModel
	{
		get
		{
			return IsWeapon();
		}
	}

	// best guess for name
	public int FacingSign
	{
		get
		{
			return GetFacingSign();
		}
	}

	public int NoRangedFlag
	{
		get
		{
			return GetNoRangedFlag();
		}
	}

	public InfoAnimation PendingAnimation
	{
		get
		{
			return GetPendingAnimation();
		}
	}

	public bool IsAnimationPending
	{
		get
		{
			return HasPendingAnimation();
		}
	}

	public bool IsStrikePending
	{
		get
		{
			return HasPendingStrike();
		}
	}

	public KeyData CurrentKeyData
	{
		get
		{
			return GetCurrentKeyData();
		}
	}

	public bool AnimationActive
	{
		get
		{
			return HasCurrentAnimation();
		}
	}

	public bool Finished
	{
		get
		{
			return IsFinished();
		}
	}

	public bool InPhysics
	{
		get
		{
			return IsInPhysics();
		}
	}

	public List<string> PhysicsNames
	{
		get
		{
			return GetPhysicsNames();
		}
	}

	public IntervalAnimation BlockInterval
	{
		get
		{
			return GetBlockInterval();
		}
	}

	public bool Blocking
	{
		get
		{
			return IsBlocking();
		}
	}

	public Model RootModel
	{
		get
		{
			return GetRootModel();
		}
	}

	public float LeftWallX
	{
		get
		{
			return GetLeftWallX();
		}
	}

	public float RightWallX
	{
		get
		{
			return GetRightWallX();
		}
	}

	public bool DisarmedItemPresent
	{
		get
		{
			return HasDisarmedItem();
		}
	}

	public int CurrentFrame
	{
		get
		{
			return GetCurrentFrame();
		}
	}

	public int ReactionFrame
	{
		get
		{
			return GetReactionFrame();
		}
	}

	public bool PastMoveEnd
	{
		get
		{
			return IsPastMoveEnd();
		}
	}

	public bool AnimationFlagged
	{
		get
		{
			return IsAnimationFlagged();
		}
	}

	public int ComboCount
	{
		get
		{
			return GetComboCount();
		}
	}

	public int LastComboCount
	{
		get
		{
			return GetLastComboCount();
		}
	}

	public float Life
	{
		get
		{
			return GetLife();
		}
	}

	public bool Alive
	{
		get
		{
			return IsAlive();
		}
	}

	public bool ComboActive
	{
		get
		{
			return IsComboActive();
		}
	}

	protected float GuardFactor
	{
		get
		{
			return GetGuardFactor();
		}
	}

	public bool ForceCritical
	{
		get
		{
			return IsForceCritical();
		}
		set
		{
			SetForceCritical(value);
		}
	}

	protected float CriticalChance
	{
		get
		{
			return GetCriticalChance();
		}
	}

	public Model(ModelParameters data)
	{
		_UnityObject = new GameObject("Model");
		GameObject gameObject = new GameObject("Mesh");
		gameObject.transform.SetParent(_UnityObject.transform, false);
		_MeshRender = gameObject.AddComponent<MeshRender>();
		_presentation = Eclipse.Rendering.ModelPresentation.Attach(_UnityObject, this);
		Eclipse.Rendering.WeaponTrail.Attach(_UnityObject, this);
		Eclipse.Rendering.FighterParticles.Attach(_UnityObject, this);
		FrameInRound = 0;
		RoundStage = -1;
		IsInitialized = true;
		Index = 0;
		combatTarget = null;
		Sign = 0;
		distanceToEnemy = 0f;
		distanceToBackWall = 0f;
		ReservedFlag = false;
		inputEnabled = false;
		IsChildModel = false;
		ReceivedCritical = false;
		set_IsShock(false);
		LastComboTime = 0;
		slowMotionAllowed = true;
		EndStageReached = false;
		parentModel = null;
		_Strike = null;
		_Physics = null;
		_Collision = null;
		_Animation = null;
		ai = null;
		fixedSign = 0;
		fightPhase = FightPhase.None;
		wallBounds = new WallBounds();
		wallAlign = new WallAlign();
		legacyCounterC = 0;
		legacyFlag = false;
		aiDecisionReady = false;
		decisionDelay = -1;
		Parameters = data;
        _ModelConditions.EclipseCharacterId = data.EclipseCharacterId;
		disarmedItem = null;
		disarmCountdown = -1;
		ItemsReloaded = false;
		pain = 0f;
		HitCounter = 0;
		damageImmune = false;
		damageMultiplier = 1f;
		modelStats = new ModelStatistics(this);
		insideArea = false;
		round = 0;
		magicChargeFraction = 0f;
		magicCharges = 0;
		raidBullets = 0;
		strikesTaken = 0;
		PendingKey = -1;
		strikePending = false;
		buttonCooldowns = new ActBtnsCooldown();
		impulseFactor = new Vector3f(1f, 1f, 1f);
		hitEffectScale = 1f;
		additionalDamage = 0f;
		cameraAttached = true;
		powerMultiplier = 1f;
		bonusModifier = 0f;
		magicChargesUsed = 0;
		raidChargesUsed = 0;
		nonComboHitsTaken = 0;
		comboCounter.AddEventListener(0, OnComboChanged);
	}

	// best guess for name
	public Model GetCombatTarget()
	{
		return combatTarget;
	}

	public float GetDistanceToEnemy()
	{
		return distanceToEnemy;
	}

	public float GetDistanceToBackWall()
	{
		return distanceToBackWall;
	}

	public float GetFrontWallX()
	{
		return (GetFacingSign() != 1) ? wallBounds.LeftX : wallBounds.RightX;
	}

	// best guess for name
	public float GetBackWallX()
	{
		return (GetFacingSign() != 1) ? wallBounds.RightX : wallBounds.LeftX;
	}

	public void SetInputEnabled(bool value)
	{
		inputEnabled = value;
		if (value)
		{
			controller.Reset();
			SetDecisionDelay(0);
		}
	}

	public bool IsInputEnabled()
	{
		return inputEnabled;
	}

	// best guess for name
	public List<WeaponModel> GetWeaponModels()
	{
		return weaponModels;
	}

	public string get_Name()
	{
		return _Name;
	}

	public void set_Name(string value)
	{
		_Name = value;
		_ModelConditions.ModelName = value;
	}

	public ModelConditions GetConditions()
	{
		return _ModelConditions;
	}

	public ModelObject GetBodyObject()
	{
		return _ModelObject;
	}

	// best guess for name
	public ModelObject GetModelObject()
	{
		return GetBodyObject();
	}

    internal System.Action CopyFormModifiersFrom(Model source)
    {
        if (source == null || source == this)
            throw new System.ArgumentException("Form modifiers require distinct models.");
        var impulse = impulseFactor;
        var hitScale = hitEffectScale;
        var addedDamage = additionalDamage;
        var color = _perkColor;
        var slow = _perkSlowFactor;
        var slowFrame = _perkSlowFrame;
        var collision = _perkCollisionDisabled;
        System.Action restore = () =>
        {
            impulseFactor = impulse;
            hitEffectScale = hitScale; additionalDamage = addedDamage;
            _perkSlowFactor = slow; _perkSlowFrame = slowFrame;
            _presentation?.SetSlow(_perkSlowFactor, _perkSlowFrame);
            _perkCollisionDisabled = collision;
            ApplyPerkColor(color);
        };
        try
        {
            impulseFactor = new Vector3f(source.impulseFactor);
            hitEffectScale = source.hitEffectScale; additionalDamage = source.additionalDamage;
            _perkSlowFactor = source._perkSlowFactor; _perkSlowFrame = source._perkSlowFrame;
            _presentation?.SetSlow(_perkSlowFactor, _perkSlowFrame);
            _perkCollisionDisabled = source._perkCollisionDisabled;
            ApplyPerkColor(source._perkColor);
        }
        catch { restore(); throw; }
        return restore;
    }

    // This stage preserves participant history and input, while each body keeps
    // its own animations, physics, AI and event subscriptions. Calling the
    // returned action before another simulation step restores both owners.
    internal System.Action TransferFormCombatState(Model replacement)
    {
        if (replacement == null || replacement == this || modelStats == null || replacement.modelStats == null ||
            _ModelConditions == null || replacement._ModelConditions == null ||
            ReferenceEquals(_ModelConditions, replacement._ModelConditions) ||
            _ModelConditions.PerkVariables == null || replacement._ModelConditions.PerkVariables == null ||
            _ModelConditions.PerkStringVariables == null || replacement._ModelConditions.PerkStringVariables == null ||
            ReferenceEquals(_ModelConditions.PerkVariables, replacement._ModelConditions.PerkVariables) ||
            ReferenceEquals(_ModelConditions.PerkStringVariables, replacement._ModelConditions.PerkStringVariables))
            throw new System.ArgumentException("Form combat state requires initialized distinct models.");
        ExchangeFormCombatState(replacement);
        bool restored = false;
        return () =>
        {
            if (restored) return;
            restored = true;
            ExchangeFormCombatState(replacement);
        };
    }

    private void ExchangeFormCombatState(Model other)
    {
        controller.ExchangeFormInput(other.controller);
        // Variables belong to the ongoing fighter. Exchange their ownership so
        // retiring this body's conditions cannot clear the active form's state.
        // Rig nodes, equipment and animation conditions stay with their bodies.
        (_ModelConditions.PerkVariables, other._ModelConditions.PerkVariables) =
            (other._ModelConditions.PerkVariables, _ModelConditions.PerkVariables);
        (_ModelConditions.PerkStringVariables, other._ModelConditions.PerkStringVariables) =
            (other._ModelConditions.PerkStringVariables, _ModelConditions.PerkStringVariables);
        (_Statistics, other._Statistics) = (other._Statistics, _Statistics);
        (modelStats, other.modelStats) = (other.modelStats, modelStats);
        modelStats.RebindFormOwner(this);
        other.modelStats.RebindFormOwner(other);
        (buttonCooldowns, other.buttonCooldowns) = (other.buttonCooldowns, buttonCooldowns);
        // Charge and the ready cast belong to the fighter, independently of the
        // new magic item. Do not normalize or emit cast/UI events during binding.
        (magicChargeFraction, other.magicChargeFraction) = (other.magicChargeFraction, magicChargeFraction);
        (magicCharges, other.magicCharges) = (other.magicCharges, magicCharges);
        (inputEnabled, other.inputEnabled) = (other.inputEnabled, inputEnabled);
        (RoundStage, other.RoundStage) = (other.RoundStage, RoundStage);
        (round, other.round) = (other.round, round);
        (magicChargesUsed, other.magicChargesUsed) = (other.magicChargesUsed, magicChargesUsed);
        (raidChargesUsed, other.raidChargesUsed) = (other.raidChargesUsed, raidChargesUsed);
        (nonComboHitsTaken, other.nonComboHitsTaken) = (other.nonComboHitsTaken, nonComboHitsTaken);
        (StyleRank, other.StyleRank) = (other.StyleRank, StyleRank);
    }

	public ModelController GetController()
	{
		return controller;
	}

	public FightStatistics GetStatistics()
	{
		return _Statistics;
	}

	public FightPhase GetPhase()
	{
		return fightPhase;
	}

	public Model GetParentModel()
	{
		return parentModel;
	}

	public ModelPhysics GetPhysicsModule()
	{
		return _Physics;
	}

	public ModelCollision GetCollisionModule()
	{
		return _Collision;
	}

	public ModelAnimation GetAnimationModule()
	{
		return _Animation;
	}

	// best guess for name
	public InfoAnimation GetCurrentAnimation()
	{
		return _Animation.GetCurrentInfo();
	}

	public List<IntervalAnimation> GetIntervals()
	{
		return _Animation.GetActiveIntervals();
	}

	public ModelAi GetAi()
	{
		return ai;
	}

	public ModelStatistics GetModelStats()
	{
		return modelStats;
	}

	public bool WasDisarmed()
	{
		return isDisarmed;
	}

	public void SetDisarmed(bool value)
	{
		isDisarmed = value;
	}

	public bool IsInShock()
	{
		return _IsShock;
	}

	public void set_IsShock(bool value)
	{
		_IsShock = value;
	}

	public bool IsInsideArea()
	{
		return insideArea;
	}

	public void SetInsideArea(bool value)
	{
		insideArea = value;
	}

	public float GetPain()
	{
		return pain;
	}

	public bool IsAiDecisionReady()
	{
		return aiDecisionReady;
	}

	public void SetAiDecisionReady(bool value)
	{
		_IsShock = value;
	}

	public void SetDamageImmune(bool value)
	{
		damageImmune = value;
		for (int i = 0; i < weaponModels.Count; i++)
		{
			weaponModels[i].SetDamageImmune(value);
		}
	}

	public bool IsDamageImmune()
	{
		return damageImmune;
	}

	public float GetDamageMultiplier()
	{
		return damageMultiplier;
	}

	public void SetDamageMultiplier(float value)
	{
		damageMultiplier = value;
		for (int i = 0; i < weaponModels.Count; i++)
		{
			weaponModels[i].SetDamageMultiplier(value);
		}
	}

	public bool IsSlowMotionAllowed()
	{
		return (parentModel == null) ? slowMotionAllowed : parentModel.IsSlowMotionAllowed();
	}

	public void SetSlowMotionAllowed(bool value)
	{
		if (parentModel != null)
		{
			parentModel.SetSlowMotionAllowed(value);
		}
		else
		{
			slowMotionAllowed = value;
		}
	}

	public int GetRound()
	{
		return round;
	}

	public void set_Round(int value)
	{
		round = value;
	}

	public int GetMagicChargesUsed()
	{
		return magicChargesUsed;
	}

	public int GetRaidChargesUsed()
	{
		return raidChargesUsed;
	}

	public float GetPowerMultiplier()
	{
		return powerMultiplier;
	}

	public void SetPowerMultiplier(float value)
	{
		powerMultiplier = value;
		for (int i = 0; i < weaponModels.Count; i++)
		{
			weaponModels[i].SetPowerMultiplier(value);
		}
	}

	public float GetBonusModifier()
	{
		return bonusModifier;
	}

	public void SetBonusModifier(float value)
	{
		bonusModifier = value;
		for (int i = 0; i < weaponModels.Count; i++)
		{
			weaponModels[i].SetBonusModifier(value);
		}
	}

	// best guess for name
	public List<InfoAnimation> GetAvailableAnimations()
	{
		return availableAnimations;
	}

	public List<Trigger> GetTriggers()
	{
		return triggers;
	}

	public void SetMagicChargeFraction(float value)
	{
		if (1f < value)
		{
			magicChargeFraction = 1f;
		}
		else if (value < 0f)
		{
			magicChargeFraction = 0f;
		}
		else
		{
			magicChargeFraction = value;
		}
	}

	// best guess for name
	public float GetMagicChargeFraction()
	{
		return magicChargeFraction;
	}

	public void SetMagicCharges(int value)
	{
		if (value != 0 && value != 1)
		{
			GameLog.Error("Wrong magic count {0}", value);
		}
		magicCharges = value;
	}

	// best guess for name
	public int GetMagicCharges()
	{
		return magicCharges;
	}

	public int GetRaidBullets()
	{
		return raidBullets;
	}

	public void SetRaidBullets(int value)
	{
		raidBullets = value;
		UpdateRaidChargeButton();
	}

	public DetailedDamages GetDamageLog()
	{
		return damageLog;
	}

	public int GetStrikesTaken()
	{
		return strikesTaken;
	}

	public float GetHitEffectScale()
	{
		return hitEffectScale;
	}

	public void set_HitEffectScale(float value)
	{
		hitEffectScale = value;
	}

	public float GetAdditionalDamage()
	{
		return additionalDamage;
	}

	public void set_AdditionalDamageValue(float value)
	{
		additionalDamage = value;
	}

	public bool IsCameraAttached()
	{
		return cameraAttached;
	}

	public void SetCameraAttached(bool value)
	{
		cameraAttached = value;
	}

	// Base fighter colour (ViewerModel assigns it when adding the model).
	// Perk tints layer over it for this fighter only; clearing a tint returns
	// to this colour rather than forcing white.
	public void set_color(Color value)
	{
		_presentation?.SetBaseColor(value);
	}

	public void SetPerkColor(Color value)
	{
		ApplyPerkColor(value);
	}

	public void ClearPerkColor()
	{
		ApplyPerkColor(null);
	}

	private void ApplyPerkColor(Color? value)
	{
		_perkColor = value;
		_presentation?.SetTint(value);
	}

	// The pivot as currently presented (render interpolation, including slow-down).
	public Vector3f InterpolatedPivot()
	{
		float alpha = _presentation != null ? _presentation.Alpha : Eclipse.Rendering.Interpolation.FightInterpolation.FightAlpha;
		Vector3f result = new Vector3f();
		Eclipse.Rendering.Interpolation.FightInterpolation.SamplePosition(_ModelObject.GetCenterOfMassNode(), alpha, result);
		return result;
	}

	public Color GetPerkColor()
	{
		return _perkColor ?? Color.white;
	}

	public void SetPerkSlowFactor(int value)
	{
		_perkSlowFactor = Mathf.Max(1, value);
		_perkSlowFrame = 0;
		_presentation?.SetSlow(_perkSlowFactor, _perkSlowFrame);
	}

	public void SetPerkCollisionDisabled(bool value)
	{
		_perkCollisionDisabled = value;
	}

	// Typed rendering seam for Eclipse arena presentation.
	public GameObject GetRenderObject() => _UnityObject;

	public GameObject GetGameObject()
	{
		return _UnityObject;
	}

	public void DestroyModel()
	{
		if (comboCounter != null)
		{
			comboCounter.RemoveAllEventListener();
		}
		if (_UnityObject != null)
		{
			// A rollback may bring this model back, so a speculative tick only hides it.
			if (!Eclipse.Multiplayer.Rollback.RollbackObjects.Hide(_UnityObject))
			{
				_UnityObject.SetActive(false);
				Object.Destroy(_UnityObject);
			}
		}
		if (_ModelObject != null) _ModelObject.Clear();
		Clear();
		if (_ModelConditions != null) _ModelConditions.Reset();
		_ModelConditions = null;
	}

	public void NoOpHookA()
	{
	}

	public static Vector3f GetCameraMidpoint(Model LHBNIMGFKIB, Model AAOIAEJJINO)
	{
		return ModelObject.GetNodesMidpoint(LHBNIMGFKIB._ModelObject.GetCenterOfMassNode(), AAOIAEJJINO._ModelObject.GetCenterOfMassNode());
	}

	public static Vector3f GetCameraMidpoint(ModelObject LHBNIMGFKIB, ModelObject AAOIAEJJINO)
	{
		return Vector3f.Middle(CameraAnchor(LHBNIMGFKIB), CameraAnchor(AAOIAEJJINO));
	}

	// The pivot the camera follows. A slowed fighter only advances once per
	// slow span, so hand the camera its position spread across that span at
	// tick resolution; otherwise its follow velocity jerks on every advance.
	private static Vector3f CameraAnchor(ModelObject body)
	{
		ModelNode pivot = body.GetCenterOfMassNode();
		Model model = body.GetModel();
		if (model == null || model._perkSlowFactor <= 1) return pivot.GetStart();
		Vector3f result = new Vector3f();
		Eclipse.Rendering.Interpolation.FightInterpolation.SamplePosition(pivot,
			(float)model._perkSlowFrame / model._perkSlowFactor, result);
		return result;
	}

	public static float GetDistanceModels(Model LHBNIMGFKIB, Model AAOIAEJJINO)
	{
		return Vector3f.Distance(LHBNIMGFKIB.GetPosition(), AAOIAEJJINO.GetPosition());
	}

	public void ChangeSpeed(float ELDDBMFEFIP)
	{
		_Physics.ChangeSpeed(ELDDBMFEFIP);
	}

	public void RefreshEdgeGeometry()
	{
		List<ModelEdge> list = _Animation.GetAttackingEdges();
		List<ModelEdge> list2 = _ModelObject.GetCollisionEdges();
		foreach (ModelEdge item in list)
		{
			item.UpdateCollisionGeometry();
		}
		foreach (ModelEdge item2 in list2)
		{
			item2.UpdateCollisionGeometry();
		}
	}

	private void SetPoseFromPositions(List<Vector3f> KPLANIHPMED, bool OOPJHIPPCMD = false)
	{
		if (OOPJHIPPCMD)
		{
			int num = GetFacingSign();
			if (num == -1)
			{
				int count = KPLANIHPMED.Count;
				List<Vector3f> list = new List<Vector3f>(count);
				int i = 0;
				for (int num2 = count; i < num2; i++)
				{
					list.Add(new Vector3f(0f - KPLANIHPMED[i].GetX(), KPLANIHPMED[i].GetY(), KPLANIHPMED[i].GetZ()));
				}
				_ModelObject.AlignToFrame(list);
			}
			else
			{
				_ModelObject.AlignToFrame(KPLANIHPMED);
			}
		}
		else
		{
			_ModelObject.AlignToFrame(KPLANIHPMED);
		}
	}

	public void SetPoseFromPositions(List<Vector3f> KPLANIHPMED, int AOJJBKLCHJO, ModelNode AECCPADGGPG)
	{
		if (AOJJBKLCHJO == -1)
		{
			int count = KPLANIHPMED.Count;
			List<Vector3f> list = new List<Vector3f>(count);
			int i = 0;
			for (int num = count; i < num; i++)
			{
				list.Add(new Vector3f(0f - KPLANIHPMED[i].GetX(), KPLANIHPMED[i].GetY(), KPLANIHPMED[i].GetZ()));
			}
			_ModelObject.AlignToFrame(list, AECCPADGGPG);
		}
		else
		{
			_ModelObject.AlignToFrame(KPLANIHPMED, AECCPADGGPG);
		}
	}

	public void Init()
	{
		ReceivedCritical = false;
		set_IsShock(false);
		isDisarmed = false;
		_IsShock = false;
		IsInitialized = true;
		PreInitHook();
		LoadModelComponents(Parameters.ModelDocuments);
		availableAnimations.Clear();
		LoadAnimations(Parameters);
		LoadTriggers(Parameters);
		AnimationEvents.AddAnimations(availableAnimations);
		TriggerEvents.AddTriggers(triggers);
		SetCurrentNode();
		_Animation.Init();
		EventData.KJDFJPBIGJC = this;
		SetModelPosition(Parameters.SpawnPosition);
	}

	public void ResetToStartPosition()
	{
		Reset();
		SetModelPosition(Parameters.SpawnPosition);
	}

	public void Reset()
	{
		_perkSlowFactor = 1;
		_perkSlowFrame = 0;
		_presentation?.SetSlow(1, 0);
		_perkCollisionDisabled = false;
		ClearWeaponModels();
		ApplyItemChange(disarmedItem, false);
		disarmedItem = null;
		disarmCountdown = -1;
		ItemsReloaded = false;
		insideArea = false;
		pain = 0f;
		HitCounter = 0;
		strikesTaken = 0;
		StyleRank = 0;
		StyleName = string.Empty;
		StyleProgress = 0f;
		StyleProgressDelta = 0f;
		_Statistics.Reset();
		_ModelObject.Reset();
		_ModelConditions.Reset();
		if (_Animation != null)
		{
			_Animation.Reset();
		}
	}

	public void PreInitHook()
	{
	}

	public void Render()
	{
		if (_perkSlowFactor > 1)
		{
			_perkSlowFrame = (_perkSlowFrame + 1) % _perkSlowFactor;
			_presentation?.SetSlow(_perkSlowFactor, _perkSlowFrame);
			if (_perkSlowFrame != 0)
				return;
		}
		if (RenderStrikeDelay())
		{
			pendingPlayRequest.Clear();
			strikePending = false;
		}
		else if (RenderAnimationDelay())
		{
			strikePending = false;
		}
		ApplyPendingStrike();
		TickDisarmAndPain();
		UpdateWallShift();
		if (IsModelActive())
		{
			_Animation.Render();
			if (IsUserControlled())
			{
				controller.Render();
			}
			if (!_Animation.GetIsPlaying() && IsInPhysics())
			{
				_Animation.RenderPhysics();
			}
		}
		UpdateCombo();
		_Physics.Render();
		_ModelObject.UpdateCenterOfMass();
		_ModelObject.UpdateMacroNodes();
		_Statistics.Draw();
		TickButtonCooldowns();
		CallEvent(4, EventData);
		ApplyAttributeLifeRegen();
	}

	public bool RenderCollision(bool FHPKEJMDFLK)
	{
		if (!_perkCollisionDisabled && HasEnemies() && !Parameters.RoundEnded && _Animation.GetCurrentInfo() != null && !IsInPhysics())
		{
			return CheckCollision(GetCombatTarget(), FHPKEJMDFLK);
		}
		return false;
	}

	public void RenderAi()
	{
		var fight = Fight.GetCurrentFight();
		// Versus fighters take only player input; a training dummy may be left to the game AI.
		if (fight != null && fight.IsLocalVersus &&
			(this == fight.GetPlayerModel() || this == fight.GetEnemyModel()) &&
			!(Eclipse.Multiplayer.VersusTraining.Active && IsAiControlled())) return;
		if ((!IsAiControlled() && !AiData.get_BothBotEnabled()) || RoundStage != 2)
		{
			return;
		}
		Tactic hBFMBOHLKPJ = Parameters.FightTactic;
		if (hBFMBOHLKPJ != null)
		{
			if (hBFMBOHLKPJ.get_Type() == Tactic.TacticType.TacticRandom)
			{
				TriggerRandomAnimation();
			}
			else if (hBFMBOHLKPJ.get_Type() == Tactic.TacticType.TacticTabular)
			{
				RunTabularTactic();
			}
		}
	}

	public void RunTabularTactic()
	{
		Model fNKFIMEDNLP = GetCombatTarget();
		ModelConditions oADECAPBOND = _ModelConditions;
		InfoAnimation pJAHIOELGGD = null;
		if (oADECAPBOND == null)
		{
			Debug.LogError("modelConditions is Empty");
		}
		else
		{
			pJAHIOELGGD = ai.Render(fNKFIMEDNLP, FrameInRound);
		}
		if (pJAHIOELGGD != null)
		{
			ConditionKeys bHDEBDIHDFM = pJAHIOELGGD.GetFirstKeysCondition();
			if (bHDEBDIHDFM == null)
			{
				GameLog.Error("tactics: conditionKeys is null for {0}", pJAHIOELGGD.Name);
				bHDEBDIHDFM = pJAHIOELGGD.GetFirstKeysCondition();
			}
			else
			{
				KeyData fONEJOKEIEN = bHDEBDIHDFM.RequiredKeys;
				_Animation.RequiredInfo = pJAHIOELGGD;
				fONEJOKEIEN.IsInverted = true;
				PlayAnimation(fONEJOKEIEN);
			}
		}
		else
		{
			controller.Reset();
		}
	}

	public void TriggerRandomAnimation(bool DFILNPNDHHP = true)
	{
		if (DFILNPNDHHP)
		{
			if (decisionDelay > 0)
			{
				decisionDelay--;
				return;
			}
			if (decisionDelay == 0)
			{
				aiDecisionReady = true;
				decisionDelay = -1;
			}
			if (!aiDecisionReady)
			{
				return;
			}
			aiDecisionReady = false;
		}
		EventData.Data = null;
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().NotifyAnimationSelector(EventData);
		}
	}

	public bool CheckCollision(Model HFGPAELCNMF = null, bool FHPKEJMDFLK = false)
	{
		bool result = false;
		IntervalAttack hFIIPNLCIEE = _Animation.FindInterval(IntervalAnimation.IntervalType.INTERVAL_ATTACK) as IntervalAttack;
		if (hFIIPNLCIEE != null)
		{
			if (HFGPAELCNMF == null)
			{
				HFGPAELCNMF = GetCombatTarget();
			}
			if (HFGPAELCNMF == null) return false;
			if (!PairedGrabAllowsStrike(HFGPAELCNMF))
			{
				return false;
			}
			if ((HFGPAELCNMF.GetAnimationModule().FindInterval(IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE) == null || (hFIIPNLCIEE.GetIgnoresInvulnerable() && hFIIPNLCIEE.GetIgnoredInvulnerableNames().Count == 0) || (hFIIPNLCIEE.GetIgnoresInvulnerable() && HFGPAELCNMF.GetAnimationModule().CheckIntervals(hFIIPNLCIEE.GetIgnoredInvulnerableNames()))) && _Collision.Render(HFGPAELCNMF._ModelObject, _Animation.GetAttackingEdges(), hFIIPNLCIEE))
			{
				if (!FHPKEJMDFLK)
				{
					StrikeModel(HFGPAELCNMF, hFIIPNLCIEE);
				}
				result = true;
			}
		}
		return result;
	}

	public void NoOpHookB()
	{
	}

	public void PressAnyKey(FightCID JDDDODIJODK)
	{
		bool inputAccepted = (JDDDODIJODK != FightCID.MagicButton || magicCharges != 0 || magicCharges != 0 || GameUtils.AlwaysMagicMode) && (JDDDODIJODK != FightCID.MissileButton || !buttonCooldowns.MissileActive || buttonCooldowns.MissileProgress == buttonCooldowns.MissileMax) && (JDDDODIJODK != FightCID.Kick || !buttonCooldowns.KickActive || buttonCooldowns.KickProgress == buttonCooldowns.KickMax) && (JDDDODIJODK != FightCID.Punch || !buttonCooldowns.PunchActive || buttonCooldowns.PunchProgress == buttonCooldowns.PunchMax) && (JDDDODIJODK != FightCID.RaidChargeButton || !buttonCooldowns.RaidChargeActive || buttonCooldowns.RaidChargeProgress == buttonCooldowns.RaidChargeMax) && inputEnabled;
		if (JDDDODIJODK == FightCID.MagicButton)
		{
			InfoAnimation current = GetCurrentAnimation();
			Debug.Log("[MagicTrace] request actor=" + get_Name() +
				" player=" + Parameters.IsPlayer +
				" accepted=" + inputAccepted +
				" charge=" + magicCharges +
				" currentAnimation=" + ((current != null) ? current.Name : "<none>") +
				" items=" + GetMagicTraceItems());
		}
		if (inputAccepted)
		{
			controller.OnPressAnyKey((int)JDDDODIJODK);
		}
	}

	public void ReleaseAnyKey(FightCID KJPGKHJNOMC)
	{
		if (inputEnabled)
		{
			controller.OnReleaseAnyKey((int)KJPGKHJNOMC);
		}
	}

	public Vector3f GetPosition()
	{
		return _ModelObject.GetCenterOfMassPosition();
	}

	public void SetModelPosition(Vector3f MGMMDGFPBLP)
	{
		_ModelObject.SetModelPosition(MGMMDGFPBLP);
		_Physics.IterativeProcess();
		_ModelObject.UpdateCenterOfMass();
		_ModelObject.UpdateMacroNodes();
		_ModelObject.ResetNodeVelocities();
	}

	/// <summary>
	/// Eclipse training: moves the fighter so its pivot stands at <paramref name="x"/>. The
	/// running move's keyframes move with it, so a fighter mid-move does not snap back.
	/// </summary>
	internal void TrainingMoveToX(float x)
	{
		ModelNode pivot = _ModelObject.GetPivotNode();
		if (pivot == null) return;
		ShiftModelPosition(new Vector3f(x - pivot.GetStart().GetX()), true);
	}

	public void ShiftModelPosition(Vector3f OPNPKNEOALJ, bool LFFNFGOECLB = false)
	{
		_ModelObject.TranslateAllNodes(OPNPKNEOALJ);
		_Physics.IterativeProcess();
		_ModelObject.UpdateCenterOfMass();
		_ModelObject.UpdateMacroNodes();
		_ModelObject.ResetNodeVelocities();
		if (LFFNFGOECLB)
		{
			_Animation.ShiftSequence(OPNPKNEOALJ.GetX(), OPNPKNEOALJ.GetY(), OPNPKNEOALJ.GetZ());
			_Animation.ShiftBuffer(OPNPKNEOALJ);
		}
	}

	public void ReloadAnimationsForItems(ModelParameters MPBIEICCBMM)
	{
		List<ItemInfo> oJIAKDDCGLB = MPBIEICCBMM.ConditionItems;
		List<PerkInfoItem> mAFPBEFKNGE = MPBIEICCBMM.GetAllPerks();
		List<PerkInfoItem> cFKCGBEONAM = null;
		if (GetCombatTarget() != null && GetCombatTarget().Parameters != null)
		{
			cFKCGBEONAM = GetCombatTarget().Parameters.GetAllPerks();
		}
		ReloadAnimationsForItems(oJIAKDDCGLB, mAFPBEFKNGE, cFKCGBEONAM);
	}

	public void ReloadAnimationsForItems(List<ItemInfo> HELFDCAIJNE, List<PerkInfoItem> MAFPBEFKNGE = null, List<PerkInfoItem> CFKCGBEONAM = null)
	{
		availableAnimations.Clear();
		AnimationData.CollectAvailableAnimations(availableAnimations, HELFDCAIJNE, false, Parameters.ExcludedMoveNames, Parameters.SceneType, MAFPBEFKNGE, CFKCGBEONAM);
		AnimationEvents.Clear();
		AnimationEvents.AddAnimations(availableAnimations);
		SetCurrentNode();
		ItemsReloaded = true;
		Parameters.MovesInitialized = true;
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().UpdateModelAnimationParameters(this);
		}
	}

	public bool IsAiControlled()
	{
		return Parameters.AiControlled;
	}

	public bool IsUserControlled()
	{
		return Parameters.UserControlled;
	}

	public bool IsDummy()
	{
		return Parameters.Armor.Type == "Dummy";
	}

	public bool IsModelActive()
	{
		return Parameters.AnimationEnabled;
	}

	public bool HasEnemies()
	{
		return _Enemies.Count > 0;
	}

	public bool IsPlayerModel()
	{
		return Parameters.IsPlayer;
	}

	public virtual bool IsWeapon()
	{
		return false;
	}

	public void ClearWeaponModels()
	{
		weaponModels.Clear();
	}

	public void SetNearestEnemy()
	{
		combatTarget = ((_Enemies.Count <= 0) ? null : _Enemies[0]);
		if (combatTarget != null && combatTarget.IsWeapon())
		{
			Debug.LogError("Model::setNearestEnemy - enemy is weapon, fix code bug");
		}
		if (EventData != null)
		{
			EventData.Opponent = combatTarget;
		}
	}

	public void RemoveEnemy(Model ACENLMONNPA)
	{
		_Enemies.Remove(ACENLMONNPA);
	}

    // Internal actor integration seam. Call only between simulation passes.
    // Hostile roots are explicit; the selected root is first so the recovered
    // insertion-order fallback retains that selection. Existing owned weapon
    // children must receive the same change. Rollback is synchronous only.
    internal System.Action ReplaceCombatEnemies(IReadOnlyList<Model> roots, Model selected)
    {
        if (roots == null || IsWeapon() || GetParentModel() != null)
            throw new System.ArgumentException("Combat enemies require a root fighter and an explicit root list.");
        var unique = new HashSet<Model>();
        foreach (var root in roots)
            if (root == null || root == this || root.IsWeapon() || root.GetParentModel() != null ||
                root._Animation == null || !unique.Add(root))
                throw new System.ArgumentException("Combat enemies must be distinct live roots other than this fighter.");
        if ((selected == null) != (roots.Count == 0) || selected != null && !unique.Contains(selected))
            throw new System.ArgumentException("The selected target must be a hostile root, or null for an empty list.");
        var next = new List<Model>();
        if (selected != null) AppendCombatEnemy(next, selected);
        foreach (var root in roots) if (root != selected) AppendCombatEnemy(next, root);
        var rollbacks = new List<System.Action>();
        try
        {
            rollbacks.Add(ReplaceCombatEnemyBindings(next, selected));
            foreach (var child in GetWeaponModels())
                rollbacks.Add(child.ReplaceCombatEnemyBindings(next, selected));
        }
        catch
        {
            for (int index = rollbacks.Count - 1; index >= 0; index--) rollbacks[index]();
            throw;
        }
        return () => { for (int index = rollbacks.Count - 1; index >= 0; index--) rollbacks[index](); };
    }

    private static void AppendCombatEnemy(List<Model> result, Model root)
    {
        result.Add(root);
        foreach (var child in root.GetWeaponModels()) if (!result.Contains(child)) result.Add(child);
    }

    internal bool CombatEnemiesMatch(IReadOnlyList<Model> roots, Model selected)
    {
        if (combatTarget != selected || _Animation.GetOtherAnimation() != selected?._Animation) return false;
        var expected = new List<Model>();
        if (selected != null) AppendCombatEnemy(expected, selected);
        foreach (var root in roots) if (root != selected) AppendCombatEnemy(expected, root);
        if (_Enemies.Count != expected.Count) return false;
        for (int i = 0; i < expected.Count; i++) if (_Enemies[i] != expected[i]) return false;
        foreach (var child in GetWeaponModels()) if (!child.CombatEnemiesMatch(roots, selected)) return false;
        return true;
    }

    internal bool CanChangeCombatTarget => GetCurrentAnimation()?.Type != InfoAnimation.AnimationKind.AnimationAttack;

    private System.Action ReplaceCombatEnemyBindings(List<Model> enemies, Model selected)
    {
        if (_Animation == null || ai == null)
            throw new System.InvalidOperationException("Combat target bindings require a live model.");
        var original = _Enemies.ToArray();
        var animation = _Animation.GetOtherAnimation();
        var target = combatTarget;
        var eventTarget = EventData?.Opponent;
        var delay = decisionDelay;
        var restoreAi = ai.ResetCombatTarget(selected?.Parameters.Weapon?.EffectiveTacticSubtype);
        System.Action restore = () =>
        {
            _Enemies.Clear(); _Enemies.AddRange(original);
            _Animation.SetOtherAnimation(animation); combatTarget = target;
            if (EventData != null) EventData.Opponent = eventTarget;
            decisionDelay = delay; restoreAi();
        };
        try
        {
            _Enemies.Clear(); _Enemies.AddRange(enemies);
            _Animation.SetOtherAnimation(selected?._Animation); combatTarget = selected;
            if (EventData != null) EventData.Opponent = selected;
            decisionDelay = 0;
        }
        catch { restore(); throw; }
        return restore;
    }

    // Synchronous form exchange only. The caller must also update each surviving
    // weapon model that targets this fighter and retain the rollback until commit.
    internal System.Action ReplaceEnemyForm(Model expected, Model replacement)
    {
        if (expected == null || replacement == null || expected == replacement || replacement == this)
            throw new System.ArgumentException("Enemy form replacement requires distinct fighters.");
        int index = _Enemies.IndexOf(expected);
        if (index < 0 || _Enemies.Contains(replacement))
            throw new System.InvalidOperationException("Enemy form identity is stale or already registered.");
        var original = _Enemies.ToArray();
        var next = new List<Model>();
        var oldWeapons = expected.GetWeaponModels();
        foreach (var enemy in original)
        {
            if (enemy == expected)
            {
                if (next.Contains(replacement)) continue;
                next.Add(replacement);
                foreach (var weapon in replacement.GetWeaponModels())
                    if (!next.Contains(weapon)) next.Add(weapon);
            }
            else if (!(enemy is WeaponModel oldWeapon && oldWeapons.Contains(oldWeapon)))
                next.Add(enemy);
        }
        var animation = _Animation.GetOtherAnimation();
        var nearest = combatTarget;
        var eventTarget = EventData == null ? null : EventData.Opponent;
        var restoreWeapon = ai.CaptureEnemyWeapon();
        System.Action restore = () =>
        {
            _Enemies.Clear(); _Enemies.AddRange(original);
            _Animation.SetOtherAnimation(animation);
            combatTarget = nearest;
            if (EventData != null) EventData.Opponent = eventTarget;
            restoreWeapon();
        };
        try
        {
            _Enemies.Clear(); _Enemies.AddRange(next);
            if (animation == expected._Animation)
                _Animation.SetOtherAnimation(replacement._Animation);
            if (nearest == expected)
            {
                combatTarget = replacement;
                ai.SetWeaponEnemy(replacement.Parameters.Weapon?.EffectiveTacticSubtype);
            }
            if (EventData != null && eventTarget == expected)
                EventData.Opponent = replacement;
        }
        catch { restore(); throw; }
        return restore;
    }

	public Model FindNearestEnemy()
	{
		Model result = null;
		float num = float.MaxValue;
		float num2 = _ModelObject.GetPivotNode().GetStart().GetX();
		foreach (Model item in _Enemies)
		{
			float num3 = num2 - item._ModelObject.GetPivotNode().GetStart().GetX();
			if (num3 < num)
			{
				num = num3;
				result = item;
			}
		}
		return result;
	}

	public void SetSign()
	{
		if (fixedSign == 0)
		{
			Model fGCODGKLHED = GetCombatTarget();
			if (fGCODGKLHED != null)
			{
				Sign = ((!(_ModelObject.GetPivotNode().GetStart().GetX() > fGCODGKLHED._ModelObject.GetPivotNode().GetStart().GetX())) ? 1 : (-1));
			}
			else
			{
				Sign = 1;
			}
		}
		else
		{
			Sign = fixedSign;
		}
	}

	public int GetFacingSign()
	{
		return (_Animation == null) ? 1 : _Animation.GetSign();
	}

	public void SetDistanceToEnemy(Model HFGPAELCNMF)
	{
		float num = _ModelObject.GetPivotNode().GetStart().GetX() - HFGPAELCNMF._ModelObject.GetPivotNode().GetStart().GetX();
		distanceToEnemy = ((!(num < 0f)) ? num : (0f - num));
	}

	public void SetDistanceToNearestWall()
	{
		float num = _ModelObject.GetPivotNode().GetStart().GetX();
		distanceToBackWall = ((GetFacingSign() != -1) ? (num - wallBounds.LeftX) : (wallBounds.RightX - num));
	}

	private void LoadModelComponents(List<string> NIKHAICFGNM)
	{
		Clear();
		_ModelObject = new ModelObject();
		_ModelObject.SetModel(this);
		ModelLoader.Load(_ModelObject, NIKHAICFGNM);
		_Physics = new ModelPhysics(_ModelObject);
		_Strike = new ModelStrike(_ModelObject);
		_Animation = new ModelAnimation(_ModelObject);
		_Collision = new ModelCollision(_ModelObject);
		ai = new ModelAi(_Animation, _Physics, (Parameters.Weapon == null) ? string.Empty : Parameters.Weapon.EffectiveTacticSubtype, Parameters);
		ai.set_Model(this);
		EventData = new EventModel();
		controller.AddEventListener(0, OnControllerKeyPressed);
		controller.AddEventListener(1, OnControllerKeyReleased);
		_Animation.AddEventListener(0, OnAnimationStarted);
		_Animation.AddEventListener(1, OnStopAnimation);
		_Animation.AddEventListener(2, OnStartInterval);
		_Animation.AddEventListener(3, OnStopInteval);
		_Animation.AddEventListener(4, OnAnimationActions);
	}

	public void SetWalls(float NGHJOCKCCHH, float KCNCLAANGGJ, int CDNFFEFGLKN, int JNKHDFNCOGK)
	{
		wallBounds.LeftX = NGHJOCKCCHH;
		wallBounds.RightX = KCNCLAANGGJ;
		_Physics.SetWallShift(wallBounds.LeftX, wallBounds.RightX);
		_Animation.SetAligns(NGHJOCKCCHH, KCNCLAANGGJ, CDNFFEFGLKN, JNKHDFNCOGK);
	}

	public void SetFixedSign(bool value, int AOJJBKLCHJO = 1)
	{
		if (value)
		{
			fixedSign = AOJJBKLCHJO;
		}
		else
		{
			fixedSign = 0;
		}
	}

	public void AddEnemy(Model HFGPAELCNMF)
	{
		_Animation.SetParentAnimation((parentModel == null) ? null : parentModel._Animation);
		if (HFGPAELCNMF == null)
		{
			return;
		}
		_Enemies.Add(HFGPAELCNMF);
		List<WeaponModel> list = HFGPAELCNMF.GetWeaponModels();
		foreach (WeaponModel item in list)
		{
			_Enemies.Add(item);
		}
		_Animation.SetOtherAnimation(HFGPAELCNMF._Animation);
		if (HFGPAELCNMF.Parameters.Weapon != null)
		{
			ai.SetWeaponEnemy(HFGPAELCNMF.Parameters.Weapon.EffectiveTacticSubtype);
		}
		SetNearestEnemy();
	}

	public void InitializeMagicCharge()
	{
		SetMagicCharges(0);
		SetMagicChargeFraction(GameUtils.MagicConfig.GetInitialCharge(this));
		UpdateMagicButton();
	}

	public void AddMagicChargeFraction(float FOIPKLDNGDL)
	{
		if (magicCharges == 0)
		{
			SetMagicChargeFraction(magicChargeFraction + FOIPKLDNGDL);
		}
	}

	public void ChangeAiTactic(string DIGKODNINPB)
	{
		ai.ChangeTactic(DIGKODNINPB);
	}

	public void ChangeAiTactic(Tactic KHAKOJKLDHO)
	{
		ai.ChangeTactic(KHAKOJKLDHO);
	}

	public void SetTactic(string DIGKODNINPB)
	{
		Tactic kHAKOJKLDHO = AiData.GetTacticByName(DIGKODNINPB);
		SetTactic(kHAKOJKLDHO);
	}

	public void SetTactic(Tactic KHAKOJKLDHO)
	{
		if (KHAKOJKLDHO != null)
		{
			Parameters.FightTactic = KHAKOJKLDHO;
			ChangeAiTactic(KHAKOJKLDHO);
		}
	}

	public void AddMagicCharges(int FOIPKLDNGDL)
	{
		if (FOIPKLDNGDL < 0)
		{
			magicChargesUsed++;
		}
		SetMagicCharges(magicCharges + FOIPKLDNGDL);
	}

	public void UpdateMagicButton()
	{
		Model bFFLLGHDPEB = parentModel;
		if (bFFLLGHDPEB == null)
		{
			float num = magicChargeFraction;
			if (1f <= num)
			{
				AddMagicCharges(1);
				num = 0f;
				SetMagicChargeFraction(num);
			}
			if (1 < magicCharges)
			{
				SetMagicCharges(1);
			}
			if (IsPlayerModel())
			{
				if (magicCharges == 0)
				{
					EventActBtnSettings eHCLMBADLKH = new EventActBtnSettings(FightCID.MagicButton, num);
					CallEvent(12, eHCLMBADLKH);
				}
				else
				{
					float aIEGFACLFKE = 1f;
					EventActBtnSettings eHCLMBADLKH2 = new EventActBtnSettings(FightCID.MagicButton, aIEGFACLFKE);
					CallEvent(12, eHCLMBADLKH2);
				}
			}
		}
		else
		{
			bFFLLGHDPEB.UpdateMagicButton();
		}
	}

	public int GetNoRangedFlag()
	{
		if (Parameters.Ranged != null && Parameters.Ranged.SubType == "NoRanged")
		{
			return 1;
		}
		return -1;
	}

	public InfoAnimation GetPendingAnimation()
	{
		return pendingPlayRequest.Animation;
	}

	public virtual bool PlayAnimation(InfoAnimation CMGIPKIPIPA, int AOJJBKLCHJO = 0, bool HHJGACBCGBP = false, int BADKABIKMBD = -1)
	{
		bool traceMagicAnimation = CMGIPKIPIPA != null &&
			CMGIPKIPIPA.Name != null && CMGIPKIPIPA.Name.IndexOf("Magic") >= 0;
		if (IsModelActive() && !_Physics.IsPhysics())
		{
			if (AOJJBKLCHJO == 0)
			{
				AOJJBKLCHJO = GetFacingSign();
			}
			_Collision.ResetLastStrike();
			bool started = _Animation.PlayInfo(CMGIPKIPIPA, AOJJBKLCHJO, !CMGIPKIPIPA.NoInterpolationFrames, HHJGACBCGBP, BADKABIKMBD);
			if (traceMagicAnimation)
			{
				Debug.Log("[MagicTrace] animation-select actor=" + get_Name() +
					" animation=" + CMGIPKIPIPA.Name +
					" animationFile=" + CMGIPKIPIPA.FileName +
					" started=" + started +
					" direction=" + AOJJBKLCHJO +
					" items=" + GetMagicTraceItems());
			}
			return started;
		}
		if (traceMagicAnimation)
		{
			Debug.Log("[MagicTrace] animation-select actor=" + get_Name() +
				" animation=" + CMGIPKIPIPA.Name +
				" animationFile=" + CMGIPKIPIPA.FileName +
				" started=False blockedByModelState=True" +
				" items=" + GetMagicTraceItems());
		}
		return false;
	}

	public bool PlayAnimation(string name, int AOJJBKLCHJO = 0)
	{
		InfoAnimation cMGIPKIPIPA = null;
		foreach (InfoAnimation item in availableAnimations)
		{
			if (item.Name == name)
			{
				cMGIPKIPIPA = item;
				break;
			}
		}
		if (cMGIPKIPIPA == null)
		{
			Debug.LogWarning("Animation '" + name + "' is unavailable on model '" + get_Name() + "'");
			return false;
		}
		if (AOJJBKLCHJO == 0)
		{
			// Named actions bypass SelectAnimation, which normally resolves SetDirection.
			// Paired throws must use the same facing before sharing an animation origin.
			AOJJBKLCHJO = cMGIPKIPIPA.GetDirection(_ModelConditions, GetFacingSign());
		}
		return PlayAnimation(cMGIPKIPIPA, AOJJBKLCHJO);
	}

	public bool PlayAnimation(KeyData AHBBDGGGEIE)
	{
		controller.Reset();
		controller.SetCurrentKeys(AHBBDGGGEIE);
		controller.CallKeyPressed();
		return true;
	}

	public void PlayAnimationDelay(InfoAnimation CMGIPKIPIPA, int AOJJBKLCHJO = 0, bool HHJGACBCGBP = false, int BADKABIKMBD = -1)
	{
		pendingPlayRequest.Animation = CMGIPKIPIPA;
		pendingPlayRequest.Direction = AOJJBKLCHJO;
		pendingPlayRequest.IsFrameShift = HHJGACBCGBP;
		pendingPlayRequest.FrameShift = BADKABIKMBD;
		Vector3f eMAFACPEPDK = new Vector3f(CMGIPKIPIPA.GetVelocity());
		if (!eMAFACPEPDK.IsEqual(0f, 0f, 0f))
		{
			eMAFACPEPDK.SetX(eMAFACPEPDK.GetX() * (float)AOJJBKLCHJO);
			_Animation.MoveByVelocity(eMAFACPEPDK);
		}
	}

	public void ClearDelayedStrike()
	{
		DelayedStrikeData.Animation = null;
		DelayedStrikeData.Names = null;
		DelayedStrikeData.IsStrikeResult = false;
	}

	public bool HasPendingAnimation()
	{
		return !pendingPlayRequest.IsEmpty();
	}

	public bool HasPendingStrike()
	{
		return DelayedStrikeData.Animation != null;
	}

	public void SetDelayedStrike(InfoAnimation CMGIPKIPIPA, bool CGHPLEOFEFM)
	{
		DelayedStrikeData.Animation = CMGIPKIPIPA;
		DelayedStrikeData.Names = CMGIPKIPIPA.GetTemplateNames();
		DelayedStrikeData.IsStrikeResult = CGHPLEOFEFM;
	}

	public bool RenderAnimationDelay()
	{
		if (HasPendingAnimation())
		{
			if (_Physics.IsPhysics())
			{
				_Physics.Stop();
			}
			if (PlayAnimation(pendingPlayRequest.Animation, pendingPlayRequest.Direction, pendingPlayRequest.IsFrameShift, pendingPlayRequest.FrameShift))
			{
				pendingPlayRequest.Clear();
			}
			return true;
		}
		return false;
	}

	public bool RenderStrikeDelay()
	{
		if (HasPendingStrike())
		{
			StrikePhysics(DelayedStrikeData.Names, DelayedStrikeData.IsStrikeResult);
			_Animation.Reset();
			_Animation.SetCurrentInfo(DelayedStrikeData.Animation);
			ClearDelayedStrike();
			return true;
		}
		return false;
	}

	public void NotifyAnimationEnded()
	{
		InfoAnimation dBOLBEOCEME = GetCurrentAnimation();
		Model fGCODGKLHED = GetCombatTarget();
		if (fGCODGKLHED != null)
		{
			Model fGCODGKLHED2 = fGCODGKLHED.GetRootModel();
			fGCODGKLHED2.modelStats.CommitPendingStatistics(true, dBOLBEOCEME);
		}
		Model fGCODGKLHED3 = GetRootModel();
		fGCODGKLHED3.modelStats.CommitPendingStatistics(false, dBOLBEOCEME);
	}

	public void OnControllerKeyPressed(object data)
	{
		EventData.Data = null;
		CallEvent(10, EventData);
	}

	public void OnControllerKeyReleased(object data)
	{
		EventData.Data = null;
		CallEvent(11, EventData);
	}

	public void OnAnimationStarted(object EMBBNNBFODN)
	{
		_Collision.ResetInterval();
		EventData.Data = EMBBNNBFODN;
		CallEvent(2, EventData);
		if (!IsWeapon())
		{
            // Several roots can target this model. Notify those controllers,
            // rather than inferring an observer from our own selected enemy.
            for (int index = 0; index < _Enemies.Count; index++)
            {
                var observer = _Enemies[index];
                if (observer != null && observer != this && !observer.IsWeapon() &&
                    observer.GetCombatTarget() == this && _Enemies.IndexOf(observer) == index)
                    observer.ObserveEnemyAnimationStarted(this);
            }
            if (IsAiControlled() || AiData.get_BothBotEnabled())
                ai.StartAnimationBot(_Animation.GetCurrentInfo());
		}
	}

	public void OnStopAnimation(object EMBBNNBFODN)
	{
		EventData.Data = EMBBNNBFODN;
		InfoAnimation pJAHIOELGGD = EMBBNNBFODN as InfoAnimation;
		if (pJAHIOELGGD.EndsStage)
		{
			EndStageReached = pJAHIOELGGD.EndsStage;
		}
		NotifyAnimationEnded();
		CallEvent(3, EventData);
	}

	public void OnStartInterval(object EMBBNNBFODN)
	{
		EventData.Data = EMBBNNBFODN;
		CallEvent(0, EventData);
	}

	public void OnStopInteval(object EMBBNNBFODN)
	{
		EventData.Data = EMBBNNBFODN;
		CallEvent(1, EventData);
		IntervalAnimation mNOIEOBBCMI = EMBBNNBFODN as IntervalAnimation;
		if (GameUtils.RandomTactics.IsIntervalByName(mNOIEOBBCMI.Name))
		{
			InfoAnimation pJAHIOELGGD = GetCurrentAnimation();
			if (pJAHIOELGGD != null)
			{
				decisionDelay = GameUtils.RandomTactics.GetDelayByName(pJAHIOELGGD.GetTemplateNames());
			}
		}
		if (mNOIEOBBCMI.Name == "Uninterrupt")
		{
			InfoAnimation dBOLBEOCEME = _Animation.GetCurrentInfo();
			Model fGCODGKLHED = GetCombatTarget();
			if (fGCODGKLHED != null)
			{
				Model fGCODGKLHED2 = fGCODGKLHED.GetRootModel();
				fGCODGKLHED2.modelStats.RecordUse(true, dBOLBEOCEME);
			}
			Model fGCODGKLHED3 = GetRootModel();
			fGCODGKLHED3.modelStats.RecordUse(false, dBOLBEOCEME);
		}
	}

	public void StrikeModel(Model HFGPAELCNMF, IntervalAttack CHCGJBLDPML)
	{
		ModelCollision.StrikeHit dEJLIPMOIHC = _Collision.Strike;
		Vector3f eMAFACPEPDK = new Vector3f(CHCGJBLDPML.GetImpulse());
		eMAFACPEPDK.SetX(eMAFACPEPDK.GetX() * (float)_Animation.GetSign());
		eMAFACPEPDK.SetX(eMAFACPEPDK.GetX() * impulseFactor.GetX());
		eMAFACPEPDK.SetY(eMAFACPEPDK.GetY() * impulseFactor.GetY());
		eMAFACPEPDK.SetZ(eMAFACPEPDK.GetZ() * impulseFactor.GetZ());
		LastComboTime = CHCGJBLDPML.GetComboTime();
		HFGPAELCNMF.Strike(dEJLIPMOIHC.VictimEdge, dEJLIPMOIHC.AttackerEdge, dEJLIPMOIHC.GetPoint(), dEJLIPMOIHC.GetSecondPoint(), this, eMAFACPEPDK);
	}

	public void Strike(ModelEdge GCFJNDJBBOI, ModelEdge AOBJMMHGMPG, Vector3f NAAPALOFBCI, Vector3f GKCGDDBMHNJ, Model HFGPAELCNMF, Vector3f KKIKIDNALOL)
	{
		strikesTaken++;
		IntervalAttack hFIIPNLCIEE = HFGPAELCNMF._Animation.FindInterval(IntervalAnimation.IntervalType.INTERVAL_ATTACK) as IntervalAttack;
		EventData.Data = hFIIPNLCIEE;
		EventData.ConditionName = hFIIPNLCIEE.GetReactionName(HFGPAELCNMF.GetReactionFrame());
		if (hFIIPNLCIEE.GetIgnoresBlock())
		{
			if (hFIIPNLCIEE.GetIgnoredBlockNames().Count == 0)
			{
				RemoveInterval(IntervalAnimation.IntervalType.INTERVAL_BLOCK);
			}
			else
			{
				RemoveIntervals(hFIIPNLCIEE.GetIgnoredBlockNames());
			}
		}
		// Eclipse training: idle stances guard by themselves, so the dummy's block rule
		// drops the guard the same way an unblockable attack does.
		if (Eclipse.Multiplayer.VersusTraining.Active && Eclipse.Multiplayer.VersusTraining.ShouldDropGuard(this))
		{
			RemoveInterval(IntervalAnimation.IntervalType.INTERVAL_BLOCK);
		}
		ai.OnGetHit();
		if (HFGPAELCNMF != null)
		{
			HFGPAELCNMF.ai.OnHitEnemy();
		}
		Parameters.RewardsEnabled = false;
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().LastStrikeResult = LastStrike;
		}
		LastStrike.ProcedPerks.Clear();
		LastStrike.Victim = this;
		LastStrike.AttackerModel = HFGPAELCNMF;
		LastStrike.Target = ((!Parameters.IsPlayer) ? 1 : 0);
		LastStrike.VictimEdge = GCFJNDJBBOI;
		LastStrike.Impulse.Set(KKIKIDNALOL);
		LastStrike.AttackerEdge = AOBJMMHGMPG;
		LastStrike.AttackAnimation = HFGPAELCNMF._Animation.GetCurrentInfo();
		LastStrike.Point = NAAPALOFBCI;
		LastStrike.EdgePoint = GKCGDDBMHNJ;
		LastStrike.BaseDamage = hFIIPNLCIEE.GetDamage();
		LastStrike.IsBlocked = IsBlocking();
		LastStrike.DefenceAttribute = GetDefenseAttribute(hFIIPNLCIEE, LastStrike.IsBlocked, GCFJNDJBBOI);
		if (!LastStrike.IsBlocked)
		{
			if (!HFGPAELCNMF.IsComboActive())
			{
				nonComboHitsTaken++;
			}
			HFGPAELCNMF.RegisterComboHit();
		}
		LastStrike.HitsTakenCount = nonComboHitsTaken;
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().OnModelPreCrit(EventData);
		}
		bool flag = hFIIPNLCIEE.GetNoCritical();
		LastStrike.IsCritical = !LastStrike.IsBlocked && !flag && GameUtils.IsProbality(GetCriticalChance());
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().OnModelPostCrit(EventData);
		}
		LastStrike.RawDamage = GetTotalDamage(hFIIPNLCIEE, LastStrike.IsBlocked, LastStrike.IsCritical, GCFJNDJBBOI);
		LastStrike.FinalDamage = Parameters.ResolveStrikeDamage(
			LastStrike.RawDamage, out LastStrike.IsOverkill);
		string text = "Head";
		string text2 = string.Empty;
		if (!string.IsNullOrEmpty(hFIIPNLCIEE.GetBodyPart()))
		{
			text2 = hFIIPNLCIEE.GetBodyPart();
		}
		else if (GCFJNDJBBOI != null)
		{
			text2 = GCFJNDJBBOI.GetBodyPart();
		}
		LastStrike.IsHeadHit = text2 == text;
		LastStrike.IsShock = ShouldCauseShock(LastStrike, HFGPAELCNMF);
		LastStrike.IsDisarm = LastStrike.IsShock;
		LastStrike.IsFirstStrike = HitCounter == 0;
		strikePending = true;
		HitCounter++;
		RuleAppliance eJPOJJKKICO = ((!IsPlayerModel()) ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		hFIIPNLCIEE.UpdateFactor(eJPOJJKKICO);
		EventData.Opponent = HFGPAELCNMF;
		if (!LastStrike.IsBlocked)
		{
			Parameters.IsUntouched = false;
		}
		LastHitBlocked = LastStrike.IsBlocked;
		LastHitCritical = LastStrike.IsCritical;
		EventData.Data = hFIIPNLCIEE;
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().OnModelHit(EventData);
		}
		EventData.Opponent = GetCombatTarget();
		RecordStrikeStatistics(HFGPAELCNMF, LastStrike.AttackAnimation, LastStrike.FinalDamage, LastStrike);
	}

	public void StrikePhysics(List<string> NIKHAICFGNM, StrikeResult PPIAOBPLGOK)
	{
		_Animation.DeleteAnimation();
		_Physics.Start(NIKHAICFGNM);
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().OnModelPhysicsStart(EventData);
		}
		ApplyStrike(PPIAOBPLGOK);
	}

	public void StrikePhysics(List<string> NIKHAICFGNM, bool CGHPLEOFEFM)
	{
		StrikeResult pPIAOBPLGOK = null;
		if (CGHPLEOFEFM)
		{
			pPIAOBPLGOK = LastStrike;
		}
		StrikePhysics(NIKHAICFGNM, pPIAOBPLGOK);
	}

	public void ApplyStrike(StrikeResult PPIAOBPLGOK)
	{
		if (PPIAOBPLGOK != null)
		{
			_Strike.Strike(LastStrike.VictimEdge, LastStrike.EdgePoint, LastStrike.Impulse);
			_Physics.IterativeProcess();
		}
	}

	public void SetCurrentNode()
	{
		foreach (InfoAnimation item in availableAnimations)
		{
			if (item.MoveData.AlignData.PivotNodeId == -1)
			{
				InfoAnimation.AlignObjectType cKBGFODEBAJ = item.MoveData.AlignData.PivotObjectType;
				if (cKBGFODEBAJ == InfoAnimation.AlignObjectType.ObjectNodes)
				{
					ModelObject oIEODIEHJMH = GetModelObjectByType(item.MoveData.AlignData.PivotModelType);
					int num = oIEODIEHJMH.GetNodeIDByName(item.MoveData.AlignData.PivotPart);
					if (num == -1)
					{
						GameLog.Warning("'Pivot' node '{0}' not found for '{1}' animation", item.MoveData.AlignData.PivotPart, item.Name);
					}
					item.MoveData.AlignData.PivotNodeId = num;
					item.MoveData.AlignData.PivotPairNodeId = oIEODIEHJMH.GetNodeIDByPairName(item.MoveData.AlignData.PivotNodeId);
				}
			}
			if (item.MoveData.AlignData.PositionNodeId != -1 && item.MoveData.AlignData.PositionModelType != ModelType.ModelTargetType.MODEL_OTHER)
			{
				continue;
			}
			InfoAnimation.AlignObjectType hHPAGAOGGLP = item.MoveData.AlignData.PositionObjectType;
			if (hHPAGAOGGLP == InfoAnimation.AlignObjectType.ObjectNodes)
			{
				ModelObject oIEODIEHJMH2 = GetModelObjectByType(item.MoveData.AlignData.PositionModelType);
				if (oIEODIEHJMH2 != null)
				{
					item.MoveData.AlignData.PositionNodeId = oIEODIEHJMH2.GetNodeIDByName(item.MoveData.AlignData.PositionPart);
					item.MoveData.AlignData.PositionPairNodeId = oIEODIEHJMH2.GetNodeIDByPairName(item.MoveData.AlignData.PositionNodeId);
				}
				else
				{
					GameLog.Error("Model::setCurrentNode() m == 0 : {0}", item.Name);
					item.MoveData.AlignData.PositionNodeId = 0;
				}
			}
		}
	}

	public KeyData GetCurrentKeyData()
	{
		return controller.GetCurrentKeys();
	}

	public KeyData GetKeyDataBySign(int KJKCHFALFDD)
	{
		return controller.GetKeyDataBySign(KJKCHFALFDD);
	}

	public bool HasCurrentAnimation()
	{
		return _Animation.GetIsPlaying();
	}

	public bool IsFinished()
	{
		return EndStageReached || !Parameters.AnimationEnabled;
	}

	public bool IsInPhysics()
	{
		return _Physics.IsPhysics();
	}

	public List<string> GetPhysicsNames()
	{
		return _Physics.GetNames();
	}

	public float GetDamageBlockCritical(bool KHOKDADOJCG, string FFLFOELEKIG, float Base)
	{
		if (KHOKDADOJCG)
		{
			int OEMALIFPGPO = 0;
			Parameters.FinalAttributes.Get(FFLFOELEKIG, ref OEMALIFPGPO);
			return Mathf.Pow(2f, (float)OEMALIFPGPO * Base);
		}
		return 1f;
	}

	public float GetBlock(bool OOCLHFGEPML)
	{
		GameUtils.BaseSettigs aMBADLGCMJE = GameUtils.GetBlockDamageFactor();
		return GetDamageBlockCritical(OOCLHFGEPML, aMBADLGCMJE.Attribute, aMBADLGCMJE.Base);
	}

	public float GetCritical(bool OOGIBOBMGJA)
	{
		GameUtils.BaseSettigs aMBADLGCMJE = GameUtils.GetCriticalHitDamage();
		return GetDamageBlockCritical(OOGIBOBMGJA, aMBADLGCMJE.Attribute, aMBADLGCMJE.Base);
	}

	public float GetTotalDamage(IntervalAttack CHCGJBLDPML,
        bool blocked, // best guess for name
        bool OOGIBOBMGJA, ModelEdge GCFJNDJBBOI)
	{
		Model fGCODGKLHED = GetCombatTarget();
		if (fGCODGKLHED == null)
		{
			Debug.LogError("attacker is null");
		}
		if (fGCODGKLHED.IsPlayerModel() && IsPlayerModel())
		{
			Debug.LogError("Both is player! Wat!?");
		}
		List<global::Pair<string, float>> list = CHCGJBLDPML.GetDamageAttributes();
		foreach (global::Pair<string, float> item in list)
		{
			if (item.First == "RaidChargeDamage")
			{
				int raidChargeDamage = 0; // best guess for name
				fGCODGKLHED.Parameters.FinalAttributes.Get(item.First, ref raidChargeDamage);
				return Eclipse.Multiplayer.LocalVersusMatch.ScaleStrikeDamage(Fight.GetCurrentFight(), raidChargeDamage, blocked, fGCODGKLHED, CHCGJBLDPML, this);
			}
		}
		string kLIIDDMHNOL = GetDefenseAttribute(CHCGJBLDPML, blocked, GCFJNDJBBOI);
		float num = GameUtils.GetDamageFactorBase();
		string kGBGENDIMBC = GameUtils.GetDamageFactorAttribute();
		int OEMALIFPGPO2 = 0;
		fGCODGKLHED.Parameters.FinalAttributes.Get(kGBGENDIMBC, ref OEMALIFPGPO2);
		OEMALIFPGPO2 = Mathf.Min(OEMALIFPGPO2, (int)GameUtils.GetDamageFactorMaxValue());
		float num2 = Mathf.Pow(2f, num * (float)OEMALIFPGPO2);
		float num3 = GetBlock(blocked);
		float num4 = fGCODGKLHED.GetCritical(OOGIBOBMGJA);
		float num5 = 0f;
		float num6 = GameUtils.GetAttributesHitMultiplier(fGCODGKLHED.IsPlayerModel(), fGCODGKLHED.Parameters, Parameters, list, kLIIDDMHNOL);
		float num7 = CHCGJBLDPML.GetDamage();
		float num8 = fGCODGKLHED.GetAdditionalDamage();
		float a = (num7 + num8) * num6 * num3 * num4 * num2;
		a = Mathf.Max(a, 0f);
		RuleAppliance eJPOJJKKICO = (fGCODGKLHED.IsPlayerModel() ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		a *= CHCGJBLDPML.GetFactors(eJPOJJKKICO).Factor;
		a *= fGCODGKLHED.GetDamageMultiplier();
		a *= fGCODGKLHED.GetPowerMultiplier();
		if (a < 0f || 100000f < a)
		{
			Debug.LogError("Model::getTotalDamage - wtf so strong");
		}
		// Scale before ResolveStrikeDamage caps lethal hits to remaining health.
		return Eclipse.Multiplayer.LocalVersusMatch.ScaleStrikeDamage(Fight.GetCurrentFight(), a, blocked, fGCODGKLHED, CHCGJBLDPML, this);
	}

	public IntervalAnimation GetBlockInterval()
	{
		return _Animation.FindInterval(IntervalAnimation.IntervalType.INTERVAL_BLOCK);
	}

	public bool IsBlocking()
	{
		return GetBlockInterval() != null;
	}

	public virtual void AttachToParent(Model MDKDAHCNCMC = null)
	{
		parentModel = MDKDAHCNCMC;
		combatTarget = ((MDKDAHCNCMC == null) ? null : MDKDAHCNCMC.GetCombatTarget());
		_Strike = null;
		_Physics = null;
		_Animation = null;
		_Collision = null;
		ai = null;
		LastHitBlocked = false;
		LastHitCritical = false;
		LegacyCounterB = 0;
		StyleRank = 0;
		StyleName = string.Empty;
		StyleProgress = 0f;
		StyleProgressDelta = 0f;
		disarmCountdown = -1;
		ItemsReloaded = false;
		pain = 0f;
		HitCounter = 0;
		disarmedItem = null;
		inputEnabled = false;
		ReservedFlag = false;
		IsChildModel = true;
		EndStageReached = false;
		IsInitialized = true;
		pendingPlayRequest.Clear();
		Init();
		legacyCounterC = 0;
		fightPhase = FightPhase.Prepare;
		Sign = 1;
		if (MDKDAHCNCMC != null)
		{
			RoundStage = MDKDAHCNCMC.RoundStage;
		}
		SetFixedSign(false);
		aiDecisionReady = false;
	}

	// best guess for name
	public Model GetRootModel()
	{
		if (parentModel != null)
		{
			return parentModel.GetRootModel();
		}
		return this;
	}

	public Model GetModelByType(ModelType.ModelTargetType LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case ModelType.ModelTargetType.MODEL_THIS:
			return this;
		case ModelType.ModelTargetType.MODEL_PARENT:
			return GetParentModel();
		case ModelType.ModelTargetType.MODEL_OTHER:
			return GetCombatTarget();
		case ModelType.ModelTargetType.MODEL_CHILD:
			if (weaponModels.Count != 0)
			{
				return weaponModels[weaponModels.Count - 1];
			}
			return null;
		case ModelType.ModelTargetType.MODEL_OTHER_CHILD:
		{
			Model other = GetCombatTarget();
			return (other == null) ? null : other.GetModelByType(ModelType.ModelTargetType.MODEL_CHILD);
		}
		default:
			GameLog.Error("Model::getModelByType ERROR - wrong model type: {0}", LFLGCDNKNJI);
			return null;
		}
	}

	// best guess for name
	public Model GetModelByRole(string role)
	{
		return GetModelByType(ModelType.ParseTargetType(role));
	}

	public int GetMaxAttributeValue(List<global::Pair<string, int>> IBLHIAHECLK)
	{
		int num = 0;
		foreach (global::Pair<string, int> item in IBLHIAHECLK)
		{
			int OEMALIFPGPO = 0;
			if (Parameters.FinalAttributes.Get(item.First, ref OEMALIFPGPO))
			{
				OEMALIFPGPO += item.Second;
				if (num < OEMALIFPGPO)
				{
					num = OEMALIFPGPO;
				}
			}
		}
		return num;
	}

	public float GetLeftWallX()
	{
		return wallBounds.LeftX;
	}

	public float GetRightWallX()
	{
		return wallBounds.RightX;
	}

	public void SetDecisionDelay(int value)
	{
		decisionDelay = value;
	}

	public void NotifyRangedAttack(WeaponModel LGCMGHAFEDD)
	{
		Model fGCODGKLHED = GetCombatTarget();
		if (fGCODGKLHED != null)
		{
			Model fGCODGKLHED2 = fGCODGKLHED.GetRootModel();
			if (fGCODGKLHED2.IsAiControlled())
			{
				fGCODGKLHED2.ai.StartRangedEnemy();
			}
		}
	}

	public void RemoveWeaponModel(WeaponModel GHDEFJAEHLF)
	{
		weaponModels.Remove(GHDEFJAEHLF);
	}

	public void ReleaseWeakNodes()
	{
		List<ModelNode> list = _ModelObject.GetAllNodes();
		foreach (ModelNode item in list)
		{
			if (item.IsWeak())
			{
				item.SetFixed(false);
			}
		}
	}

	public bool SetPain(float CKKFKEIELCP)
	{
		pain += CKKFKEIELCP;
		if (!_ModelObject.IsShock() && pain > GameUtils.ShockSettings.Threshold)
		{
			return true;
		}
		return false;
	}

	public void OnStyleChanged(int JKJCPCCHJJN, string ECJDAIHCDBA, float KPNNGFGBNCI, bool GOAGDIANENH = false)
	{
		if (GOAGDIANENH)
		{
			if (StyleRank == JKJCPCCHJJN)
			{
				StyleProgressDelta = KPNNGFGBNCI - StyleProgress;
			}
			else
			{
				StyleProgressDelta = 1f - StyleProgress + KPNNGFGBNCI + (float)(JKJCPCCHJJN - StyleRank - 1);
			}
		}
		StyleRank = JKJCPCCHJJN;
		StyleName = ECJDAIHCDBA;
		StyleProgress = KPNNGFGBNCI;
	}

	public void OnComboChanged(object data)
	{
		CallEvent(13, this);
	}

	public bool HasDisarmedItem()
	{
		return disarmedItem != null;
	}

	public void SuppressInterval(IntervalAnimation.IntervalType LFLGCDNKNJI)
	{
		if (_Animation != null)
		{
			_Animation.AddIntervalTypeFilter(LFLGCDNKNJI);
		}
	}

	public void ClearSuppressedIntervals()
	{
		if (_Animation != null)
		{
			_Animation.ClearIntervalTypeFilter();
		}
	}

	public void ExecuteActionOnTarget(ActionAnimation IBODMPMJELJ)
	{
		Model fGCODGKLHED = GetModelByType(IBODMPMJELJ.GetTargetPlayer());
		if (fGCODGKLHED != null)
		{
			fGCODGKLHED.StartAction(IBODMPMJELJ);
		}
	}

	public void StartAction(ActionAnimation IBODMPMJELJ)
	{
		GameLog.Error("Model::startAction - unknown action: {0}", IBODMPMJELJ.get_Type());
	}

	public void StartAction(ActionCreateModel IBODMPMJELJ)
	{
		SpawnWeaponModel(IBODMPMJELJ.GetCopyItems(), IBODMPMJELJ.GetModelName(), IBODMPMJELJ.StartAnimation,
            IBODMPMJELJ.EclipseProjectileOwner, IBODMPMJELJ.EclipseProjectileLifetime);
	}

	public void StartAction(ActionDelete IBODMPMJELJ)
	{
		Model eHCLMBADLKH = GetModelByType(IBODMPMJELJ.GetTargetPlayer());
		NotifyAnimationEnded();
		CallEvent(5, eHCLMBADLKH);
	}

	public void StartAction(ActionSound IBODMPMJELJ)
	{
		if (IBODMPMJELJ.SameGender(Parameters.EclipseVoice))
		{
			Sound.PlaySound(IBODMPMJELJ.get_Name(), IBODMPMJELJ.GetIsLooped(), IBODMPMJELJ.GetVolume());
		}
	}

	public void StartAction(ActionStopSound IBODMPMJELJ)
	{
		Sound.StopSound(IBODMPMJELJ.get_Name());
	}

	public void StartAction(ActionRandomSound IBODMPMJELJ)
	{
		if (IBODMPMJELJ.SameGender(Parameters.EclipseVoice))
		{
			Sound.PlaySound(IBODMPMJELJ.get_Name());
		}
	}

	public void StartAction(ActionEffect IBODMPMJELJ)
	{
		IBODMPMJELJ.set_Model(this);
		CallEvent(7, IBODMPMJELJ);
		IBODMPMJELJ.set_Model(null);
	}

	public void StartAction(ActionStopEffect IBODMPMJELJ)
	{
		IBODMPMJELJ.set_Model(this);
		CallEvent(8, IBODMPMJELJ);
		IBODMPMJELJ.set_Model(null);
	}

	public void StartAction(ActionAddBullets IBODMPMJELJ)
	{
		switch (IBODMPMJELJ.GetBulletType())
		{
		case BulletType.MAGIC_BULLET:
		{
			int fOIPKLDNGDL2 = IBODMPMJELJ.GetValue();
			int bulletsBefore = magicCharges;
			InfoAnimation currentMagicAnimation = GetCurrentAnimation();
			AddMagicCharges(fOIPKLDNGDL2);
			Debug.Log("[MagicTrace] cast actor=" + get_Name() +
				" player=" + Parameters.IsPlayer +
				" animation=" + ((currentMagicAnimation != null) ? currentMagicAnimation.Name : "<none>") +
				" animationFile=" + ((currentMagicAnimation != null) ? currentMagicAnimation.FileName : "<none>") +
				" items=" + GetMagicTraceItems() +
				" charge=" + bulletsBefore + "->" + magicCharges +
				" delta=" + fOIPKLDNGDL2);
			UpdateMagicButton();
			break;
		}
		case BulletType.RAID_CHARGE_BULLET:
		{
			int fOIPKLDNGDL = IBODMPMJELJ.GetValue();
			AddRaidBullets(fOIPKLDNGDL);
			UpdateRaidChargeButton();
			break;
		}
		default:
			Debug.LogError("ERROR: Unknown bulletType");
			break;
		}
	}

	public void StartAction(ActionStopFollowEffect IBODMPMJELJ)
	{
		IBODMPMJELJ.set_Model(this);
		CallEvent(9, IBODMPMJELJ);
		IBODMPMJELJ.set_Model(null);
	}

	public void StartAction(ActionTryOnEnd IBODMPMJELJ)
	{
		CallEvent(14, null);
	}

	public void StartAction(ActionShakeScreen IBODMPMJELJ)
	{
		CallEvent(15, IBODMPMJELJ);
	}

	public void StartAction(ActionHitEffect IBODMPMJELJ)
	{
		if (hitData.DataReady && Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().CreateHitEffect(hitData.Point, hitData.Velocity, hitData.Time, IBODMPMJELJ.GetFileName(), hitEffectScale);
		}
	}

	public void StartAction(ActionZoomEffect IBODMPMJELJ)
	{
		CallEvent(17, IBODMPMJELJ);
	}

	public void StartAction(ActionSetCooldown IBODMPMJELJ)
	{
		int pLKFFGILBOP = IBODMPMJELJ.GetDuration();
		string bAINMLLIKOL = IBODMPMJELJ.GetButtonName();
		FightCID eCHINOPKGGI = (FightCID)MovesMaps.GetMappedIndex(MovesMaps.MapType.KEY_TYPE, bAINMLLIKOL);
		ResetButtonCooldown(eCHINOPKGGI, 0);
		bool flag = true;
		if (eCHINOPKGGI == FightCID.RaidChargeButton)
		{
			flag = false;
			RaidModelParameters kAOPLEPILDH = Parameters as RaidModelParameters;
			if (kAOPLEPILDH != null && kAOPLEPILDH.RaidChargeItem != null)
			{
				NoAnimationMove.RaidMoveInfo jNMKPGHOFIF = QuestUtils.GetNoAnimationMoves().GetMoveByName(kAOPLEPILDH.RaidChargeItem.Name);
				if (jNMKPGHOFIF != null)
				{
					pLKFFGILBOP = (int)jNMKPGHOFIF.ChargeCost;
					flag = raidBullets > 0;
				}
			}
		}
		if (flag)
		{
			StartButtonCooldown(eCHINOPKGGI, pLKFFGILBOP);
			if (!SystemProperties.IsDebug())
			{
			}
		}
		else
		{
			ResetButtonCooldown(eCHINOPKGGI, 30);
		}
	}

	public void StartAction(ActionSetEndStage IBODMPMJELJ)
	{
		Model target = GetModelByType(IBODMPMJELJ.GetTargetPlayer());
		if (target == null)
			target = this;
		target.EndStageReached = true;
	}

	public void StartAction(ActionPlayAnimation IBODMPMJELJ)
	{
		Model target = null;
		if (!string.IsNullOrEmpty(IBODMPMJELJ.ChildName))
		{
			foreach (WeaponModel child in GetWeaponModels())
			{
				if (child.get_Name() == IBODMPMJELJ.ChildName)
				{
					target = child;
					break;
				}
			}
		}
		if (target == null)
			target = GetModelByType(IBODMPMJELJ.GetTargetPlayer());
		if (target != null)
		{
			bool paired = IBODMPMJELJ.GetTargetPlayer() == ModelType.ModelTargetType.MODEL_OTHER && string.IsNullOrEmpty(IBODMPMJELJ.ChildName);
			bool started = target.PlayAnimation(IBODMPMJELJ.AnimationName);
			if (paired)
			{
				// A throw pulls the enemy into its paired animation, then strikes by collision.
				// If the enemy refused (in physics, inactive or without that move), the throw
				// still swept through them and dealt damage without a grab ("bluetooth throw").
				_PairedGrab = GetCurrentAnimation();
				_PairedGrabVictim = target;
				_PairedGrabAnimation = IBODMPMJELJ.AnimationName;
				_PairedGrabRefused = !started;
				if (!started)
				{
					Debug.LogWarning("[Throw] " + get_Name() + " '" + (_PairedGrab == null ? "?" : _PairedGrab.Name) +
						"': enemy refused paired animation '" + IBODMPMJELJ.AnimationName + "' (physics=" +
						target._Physics.IsPhysics() + "); its strike is suppressed.");
				}
			}
		}
	}

	private InfoAnimation _PairedGrab;
	private Model _PairedGrabVictim;
	private string _PairedGrabAnimation;
	private bool _PairedGrabRefused;
	private bool _PairedGrabLeftLogged;

	// False when the current move grabbed this enemy into a paired animation that the enemy
	// refused. Logs (without blocking) when the enemy left the paired animation before a strike.
	private bool PairedGrabAllowsStrike(Model victim)
	{
		if (_PairedGrab == null || victim != _PairedGrabVictim) return true;
		if (GetCurrentAnimation() != _PairedGrab)
		{
			_PairedGrab = null;
			_PairedGrabVictim = null;
			_PairedGrabLeftLogged = false;
			return true;
		}
		if (_PairedGrabRefused) return false;
		InfoAnimation current = victim.GetCurrentAnimation();
		if ((current == null || current.Name != _PairedGrabAnimation) && !_PairedGrabLeftLogged)
		{
			_PairedGrabLeftLogged = true;
			if (!Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
				Debug.LogWarning("[Throw] " + get_Name() + " '" + _PairedGrab.Name + "': enemy left paired animation '" +
				_PairedGrabAnimation + "' for '" + (current == null ? "<none>" : current.Name) + "' (physics=" +
				victim._Physics.IsPhysics() + ") before the strike.");
		}
		return true;
	}

	// best guess for name
	public WeaponModel SpawnWeaponModel(List<CopyItemInfo> HELFDCAIJNE = null, string JLHDJLHLGND = "", string startAnimation = "", string projectileOwner = null,
        int lifetimeFrames = Eclipse.Modding.ModProjectileLimits.DefaultLifetimeFrames)
	{
        var ownedFight = string.IsNullOrEmpty(projectileOwner) ? null : Fight.GetCurrentFight();
        if (!string.IsNullOrEmpty(projectileOwner) && (ownedFight == null ||
            !ownedFight.CanSpawnEclipseProjectile(this, projectileOwner, lifetimeFrames))) return null;
		if (HELFDCAIJNE == null)
		{
			HELFDCAIJNE = new List<CopyItemInfo>();
		}
		ModelParameters kIKOGDEPGHB = CreateChildParameters(HELFDCAIJNE);
		kIKOGDEPGHB.IsPlayer = IsPlayerModel();
		WeaponModel gKIANLDJFCH = new WeaponModel(kIKOGDEPGHB);
		gKIANLDJFCH.set_Name(JLHDJLHLGND);
		gKIANLDJFCH.SetExplicitBirthAnimation(startAnimation);
		gKIANLDJFCH.SetDamageImmune(damageImmune);
		gKIANLDJFCH.Parameters.SceneType = SceneTypes.SceneFight;
		gKIANLDJFCH.AttachToParent(this);
		gKIANLDJFCH.AddEnemy(GetCombatTarget());
		gKIANLDJFCH.SetImpulseFactor(impulseFactor);
		gKIANLDJFCH.SetPowerMultiplier(GetPowerMultiplier());
		weaponModels.Add(gKIANLDJFCH);
        ownedFight?.RegisterEclipseProjectile(this, gKIANLDJFCH, projectileOwner, lifetimeFrames);
		CallEvent(6, gKIANLDJFCH);
		return gKIANLDJFCH;
	}

    public bool ExplicitBirthAnimationStarted { get; private set; }

	public void SetExplicitBirthAnimation(string animationName)
	{
		_ExplicitBirthAnimation = animationName;
        ExplicitBirthAnimationStarted = false;
	}

	public bool HasExplicitBirthAnimation()
	{
		return !string.IsNullOrEmpty(_ExplicitBirthAnimation);
	}

	public bool TryPlayExplicitBirthAnimation()
	{
		if (string.IsNullOrEmpty(_ExplicitBirthAnimation))
			return false;

		string animationName = _ExplicitBirthAnimation;
		_ExplicitBirthAnimation = string.Empty;
		InfoAnimation animation = null;
		foreach (InfoAnimation candidate in availableAnimations)
		{
			if (candidate.Name == animationName)
			{
				animation = candidate;
				break;
			}
		}
		// A newer StartAnimation can reference a move excluded from the legacy
		// per-model cache.  It is still a valid parsed move and is safe to play on
		// the helper for which the XML explicitly requested it.
		if (animation == null)
			animation = AnimationData.GetAnimationByName(animationName, false);
		if (animation == null)
		{
			Debug.LogError("[MagicTrace] explicit-start missing actor=" + get_Name() +
				" animation=" + animationName + " items=" + GetMagicTraceItems());
			return false;
		}
		bool started = PlayAnimation(animation);
        ExplicitBirthAnimationStarted = started;
		Debug.Log("[MagicTrace] explicit-start actor=" + get_Name() +
			" animation=" + animationName + " started=" + started +
			" items=" + GetMagicTraceItems());
		return started;
	}

	public bool IsControlKeys(int KGBGENDIMBC)
	{
		if (Parameters.ControlKeys.Count == 0)
		{
			return true;
		}
		foreach (int item in Parameters.ControlKeys)
		{
			int num = item;
			if (num == KGBGENDIMBC)
			{
				return true;
			}
		}
		return false;
	}

	public void AddCurrentEffect(CurrentEffect LLOLBKJMKNC)
	{
		if (!currentEffects.Contains(LLOLBKJMKNC))
		{
			currentEffects.Add(LLOLBKJMKNC);
		}
	}

	public void RemoveCurrentEffect(CurrentEffect LLOLBKJMKNC)
	{
		currentEffects.Remove(LLOLBKJMKNC);
	}

	public void DetachCurrentEffects()
	{
		for (int i = 0; i < currentEffects.Count; i++)
		{
			currentEffects[i].Owner = null;
		}
	}

	public int GetCurrentFrame()
	{
		if (IsInPhysics())
		{
			return _Physics.GetFrame();
		}
		InfoAnimation pJAHIOELGGD = GetCurrentAnimation();
		if (pJAHIOELGGD != null)
		{
			return _Animation.GetCurrentFrame();
		}
		return -1;
	}

	public int GetReactionFrame()
	{
		if (IsInPhysics())
		{
			return _Physics.GetFrame();
		}
		InfoAnimation pJAHIOELGGD = GetCurrentAnimation();
		if (pJAHIOELGGD != null)
		{
			return _Animation.GetReactionFrame();
		}
		return -1;
	}

	public bool IsPastMoveEnd()
	{
		int num = GetCurrentFrame();
		if (num > -1)
		{
			InfoAnimation pJAHIOELGGD = GetCurrentAnimation();
			if (pJAHIOELGGD == null)
			{
				return true;
			}
			return num > _Animation.GetStartFrame();
		}
		return false;
	}

	public bool IsAnimationFlagged()
	{
		return _Animation.GetIsMirrored();
	}

	public void UpdateMagicCharge(float CKKFKEIELCP, Model HFGPAELCNMF, bool OOCLHFGEPML, bool OOGIBOBMGJA, bool FNMEMMLNKJL)
	{
		Model fGCODGKLHED = GetParentModel();
		if (fGCODGKLHED != null)
		{
			fGCODGKLHED.UpdateMagicCharge(CKKFKEIELCP, HFGPAELCNMF, OOCLHFGEPML, OOGIBOBMGJA, FNMEMMLNKJL);
		}
		else if (magicCharges == 0)
		{
			HFGPAELCNMF = HFGPAELCNMF.GetRootModel();
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			if (FNMEMMLNKJL)
			{
				num = GameUtils.MagicConfig.GetDamageRecharge(this);
				num2 = HFGPAELCNMF.GetBlock(OOCLHFGEPML);
				num3 = GetCritical(OOGIBOBMGJA);
			}
			else
			{
				num = GameUtils.MagicConfig.GetPainRecharge(this);
				num2 = GetBlock(OOCLHFGEPML);
				num3 = HFGPAELCNMF.GetCritical(OOGIBOBMGJA);
			}
			float fOIPKLDNGDL = Mathf.Pow(2f, num) * num2 * num3 * CKKFKEIELCP;
			AddMagicChargeFraction(fOIPKLDNGDL);
			UpdateMagicButton();
		}
	}

	public int GetComboCount()
	{
		if (parentModel != null)
		{
			return parentModel.GetComboCount();
		}
		return comboCounter.GetComboCount();
	}

	public void NotifyPerkAction(PerksStage.ActionPerk IBODMPMJELJ, bool CCBEDPIHKAD)
	{
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().UpdatePerkIcon(this, IBODMPMJELJ, CCBEDPIHKAD);
		}
	}

	public void NotifyPerkActionReplaced(PerksStage.ActionPerk CKOEFOCPMGK, PerksStage.ActionPerk IBODMPMJELJ)
	{
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().ReplacePerkIcon(this, CKOEFOCPMGK, IBODMPMJELJ);
		}
	}

	public void RemoveInterval(IntervalAnimation.IntervalType LFLGCDNKNJI)
	{
		_Animation.RemoveInterval(LFLGCDNKNJI);
	}

	public void RemoveInterval(string name)
	{
		_Animation.RemoveInterval(name);
	}

	public void RemoveIntervals(List<string> NFLDEGMEJAK)
	{
		_Animation.RemoveIntervals(NFLDEGMEJAK);
	}

	public void ScheduleDisarm()
	{
		if (disarmCountdown < 0)
		{
			disarmCountdown = GameUtils.ShockSettings.LooseningDelayFrames;
		}
	}

	public int GetLastComboCount()
	{
		return comboCounter.GetLastComboCount();
	}

	public void NextRound(int MHPLDHBGBFO)
	{
		set_Round(MHPLDHBGBFO);
		SetSlowMotionAllowed(true);
		SetDecisionDelay(-1);
		SetAiDecisionReady(false);
		modelStats.ApplyRoundFactor();
	}

	public void SetLife(float DLEDDPFNPOH)
	{
		Parameters.SetCurrentLife(DLEDDPFNPOH);
	}

	public float GetLife()
	{
		return (ObscuredFloat)(Parameters.GetCurrentLife());
	}

	public void ChangeLife(float AACBFABMADJ)
	{
		Parameters.ChangeLife(AACBFABMADJ);
	}

	public bool IsAlive()
	{
		return !Parameters.GetLifeDepleted();
	}

	public static string GetDefenseAttribute(IntervalAttack CHCGJBLDPML, bool OOCLHFGEPML, ModelEdge GCFJNDJBBOI)
	{
		List<string> list = CHCGJBLDPML.GetDefenseTypes();
		if (0 < list.Count)
		{
			return list[0];
		}
		if (OOCLHFGEPML)
		{
			return GameUtils.GetBlockDefenseAttribute();
		}
		if (GCFJNDJBBOI != null && !string.IsNullOrEmpty(GCFJNDJBBOI.GetDefense()))
		{
			return GCFJNDJBBOI.GetDefense();
		}
		return GameUtils.GetSlowMotionDefense();
	}

	public void UpdateAnimationParameters(List<Model> INNLAFHKJNI)
	{
		ModelObject eFALNIGJKLB = _ModelObject;
		bool dPKOKLCJEHI = IsPlayerModel();
		bool eMGNKKHPGCJ = GetParentModel() != null;
		List<InfoAnimation> lNKFKJKLCKP = GetAvailableAnimations();
		List<Trigger> aIPCBIBMFCB = GetTriggers();
		foreach (Model item in INNLAFHKJNI)
		{
			item.ApplyAnimationParameters(lNKFKJKLCKP, eFALNIGJKLB, dPKOKLCJEHI, eMGNKKHPGCJ, aIPCBIBMFCB);
		}
	}

	public void ApplyAnimationParameters(List<InfoAnimation> LNKFKJKLCKP, ModelObject BBGCMFGFMCL, bool DPKOKLCJEHI, bool EMGNKKHPGCJ, List<Trigger> AIPCBIBMFCB)
	{
		List<InfoAnimation> list = GetAvailableAnimations();
		foreach (InfoAnimation item in list)
		{
			item.UpdateModelObjects(BBGCMFGFMCL, DPKOKLCJEHI, EMGNKKHPGCJ, BBGCMFGFMCL);
		}
		List<Trigger> list2 = GetTriggers();
		foreach (Trigger item2 in list2)
		{
			item2.UpdateForObject(BBGCMFGFMCL, DPKOKLCJEHI, EMGNKKHPGCJ, BBGCMFGFMCL);
		}
		ModelObject eFALNIGJKLB = _ModelObject;
		bool eKBOGDKIHIH = IsPlayerModel();
		bool pHADJMAONJG = GetParentModel() != null;
		foreach (InfoAnimation item3 in LNKFKJKLCKP)
		{
			item3.UpdateModelObjects(eFALNIGJKLB, eKBOGDKIHIH, pHADJMAONJG, eFALNIGJKLB);
		}
		foreach (Trigger item4 in AIPCBIBMFCB)
		{
			item4.UpdateForObject(eFALNIGJKLB, eKBOGDKIHIH, pHADJMAONJG, eFALNIGJKLB);
		}
	}

	public void LogDamage(float CKKFKEIELCP, string BBNKIBKPBLO, string target)
	{
		GetDamageLog().Add(CKKFKEIELCP, BBNKIBKPBLO, target);
	}

	public void RunActions(List<ActionAnimation> AFENHJFICNN)
	{
		foreach (ActionAnimation item in AFENHJFICNN)
		{
			bool canVisit = item.CanVisit(this);
			ActionEffect conditionalEffect = item as ActionEffect;
			if (conditionalEffect != null && item.GetConditionCount() > 0)
			{
				InfoAnimation current = GetCurrentAnimation();
				Debug.Log("[MagicTrace] effect-gate actor=" + get_Name() +
					" animation=" + ((current != null) ? current.Name : "<none>") +
					" action=" + conditionalEffect.get_Name() +
					" sequence=" + conditionalEffect.GetSequence() +
					" conditions=" + item.GetConditionCount() +
					" allowed=" + canVisit +
					" items=" + GetMagicTraceItems());
			}
			if (canVisit)
				item.Visit(this);
		}
	}

	public string GetMagicTraceItems()
	{
		List<string> items = new List<string>();
		foreach (ItemInfo item in GetModelConditionItems())
		{
			if (item != null && (item.Type == "Magic" || item.Type == "Weapon" || item.Type == "Ranged"))
				items.Add(item.Type + ":" + item.Name + "/" + item.SubType);
		}
		return (items.Count == 0) ? "<none>" : string.Join(",", items.ToArray());
	}

	private List<ItemInfo> GetModelConditionItems()
	{
		List<ItemInfo> items = Parameters.GetEquippedItems();
		if (Parameters.ConditionItems != null)
		{
			foreach (ItemInfo item in Parameters.ConditionItems)
			{
				if (item != null && !items.Contains(item))
					items.Add(item);
			}
		}
		return items;
	}

	public void SetHitData(Vector3f NAAPALOFBCI, Vector3f KKIKIDNALOL, float time)
	{
		hitData.Point = NAAPALOFBCI;
		hitData.Velocity = KKIKIDNALOL;
		hitData.Time = time;
		hitData.DataReady = true;
	}

	public void ResetHitData()
	{
		hitData.DataReady = false;
	}

	public void ResetButtonCooldown(FightCID DDNBGEJJGMG, int frames)
	{
		switch (DDNBGEJJGMG)
		{
		case FightCID.Punch:
		{
			buttonCooldowns.PunchActive = false;
			buttonCooldowns.PunchProgress = 0f;
			EventActBtnSettings eHCLMBADLKH4 = new EventActBtnSettings(FightCID.Punch, 0f, frames);
			CallEvent(12, eHCLMBADLKH4);
			break;
		}
		case FightCID.Kick:
		{
			buttonCooldowns.KickActive = false;
			buttonCooldowns.KickProgress = 0f;
			EventActBtnSettings eHCLMBADLKH3 = new EventActBtnSettings(FightCID.Kick, 0f, frames);
			CallEvent(12, eHCLMBADLKH3);
			break;
		}
		case FightCID.MissileButton:
		{
			buttonCooldowns.MissileActive = false;
			buttonCooldowns.MissileProgress = 0f;
			EventActBtnSettings eHCLMBADLKH2 = new EventActBtnSettings(FightCID.MissileButton, 0f, frames);
			CallEvent(12, eHCLMBADLKH2);
			break;
		}
		case FightCID.RaidChargeButton:
		{
			buttonCooldowns.RaidChargeActive = false;
			buttonCooldowns.RaidChargeProgress = 0f;
			EventActBtnSettings eHCLMBADLKH = new EventActBtnSettings(FightCID.RaidChargeButton, 0f, frames);
			CallEvent(12, eHCLMBADLKH);
			break;
		}
		case FightCID.MagicButton:
			break;
		}
	}

	public void StartButtonCooldown(FightCID DDNBGEJJGMG, int frames)
	{
		if (frames <= 0)
		{
			frames = 1;
		}
		switch (DDNBGEJJGMG)
		{
		case FightCID.Punch:
			buttonCooldowns.PunchActive = true;
			buttonCooldowns.PunchCooldownFrames = frames;
			break;
		case FightCID.Kick:
			buttonCooldowns.KickActive = true;
			buttonCooldowns.KickFrames = frames;
			break;
		case FightCID.MissileButton:
			buttonCooldowns.MissileActive = true;
			buttonCooldowns.MissileFrames = frames;
			break;
		case FightCID.RaidChargeButton:
			buttonCooldowns.RaidChargeActive = true;
			buttonCooldowns.RaidChargeFrames = frames;
			break;
		case FightCID.MagicButton:
			break;
		}
	}

	public void ReloadTriggers()
	{
		LoadTriggers(Parameters);
		TriggerEvents.Clear();
		TriggerEvents.AddTriggers(triggers);
	}

	public void SetImpulseFactor(Vector3f OLOAPIIOBKK)
	{
		SetImpulseFactor(OLOAPIIOBKK.GetX(), OLOAPIIOBKK.GetY(), OLOAPIIOBKK.GetZ());
	}

	public void SetImpulseFactor(float DHDMNHCIPEH, float BGEEALIPKCC, float LKPCKJOLJDO)
	{
		impulseFactor.Set(DHDMNHCIPEH, BGEEALIPKCC, LKPCKJOLJDO);
	}

	public void ResetImpulseFactor()
	{
		impulseFactor.Set(1f, 1f, 1f);
	}

	public void ResetHitEffectScale()
	{
		hitEffectScale = 1f;
	}

	public void SetAdditionalDamageToOne()
	{
		additionalDamage = 1f;
	}

	public void AddRaidBullets(int FOIPKLDNGDL)
	{
		if (FOIPKLDNGDL < 0)
		{
			raidChargesUsed++;
		}
		raidBullets += FOIPKLDNGDL;
	}

	public void UpdateRaidChargeButton()
	{
		Model fGCODGKLHED = GetParentModel();
		if (fGCODGKLHED == null)
		{
			if (IsPlayerModel())
			{
				if (GetRaidBullets() == 0)
				{
					EventActBtnSettings eHCLMBADLKH = new EventActBtnSettings(FightCID.RaidChargeButton, 0f, 0);
					CallEvent(12, eHCLMBADLKH);
				}
				EventActBtnSettings eHCLMBADLKH2 = new EventActBtnSettings(FightCID.RaidChargeButton, -1f, -1, GetRaidBullets());
				CallEvent(18, eHCLMBADLKH2);
			}
		}
		else
		{
			fGCODGKLHED.UpdateMagicButton();
		}
	}

	public bool IsComboActive()
	{
		if (parentModel != null)
		{
			return parentModel.IsComboActive();
		}
		return comboCounter.GetIsCounting();
	}

	protected virtual void LoadAnimations(ModelParameters data)
	{
		List<PerkInfoItem> mAFPBEFKNGE = Parameters.GetAllPerks();
		List<PerkInfoItem> cFKCGBEONAM = null;
		if (GetCombatTarget() != null && GetCombatTarget().Parameters != null)
		{
			cFKCGBEONAM = GetCombatTarget().Parameters.GetAllPerks();
		}
		List<ItemInfo> fJKCMJNAFJD = Parameters.GetEquippedItems();
		BuildAvailableAnimations(fJKCMJNAFJD, false, Parameters.ExcludedMoveNames, Parameters.SceneType, mAFPBEFKNGE, cFKCGBEONAM);
	}

	protected virtual void LoadTriggers(ModelParameters data)
	{
		List<PerkInfoItem> mAFPBEFKNGE = Parameters.GetAllPerks();
		// Equipped items and action-created items live in separate collections.
		// Newer move actions test the equipped Magic/Weapon subtype directly, so
		// using only OJIAKDDCGLB left shop previews with an empty condition context
		// and made shared templates choose unrelated fallback effects.
		List<ItemInfo> oJIAKDDCGLB = GetModelConditionItems();
		_ModelConditions.Items = oJIAKDDCGLB;
		List<PerkInfoItem> cFKCGBEONAM = null;
		if (GetCombatTarget() != null && GetCombatTarget().Parameters != null)
		{
			cFKCGBEONAM = GetCombatTarget().Parameters.GetAllPerks();
		}
		BuildTriggers(oJIAKDDCGLB, false, Parameters.SceneType, mAFPBEFKNGE, cFKCGBEONAM);
	}

	protected void UpdateWallShift()
	{
		if (_Physics.IsPhysics())
		{
			return;
		}
		float eDCHBILGFLD = wallBounds.LeftX;
		float nNCHJCLKHHA = wallBounds.RightX;
		float num = wallBounds.LeftX;
		float num2 = wallBounds.RightX;
		List<ModelNode> list = _ModelObject.GetAllNodes();
		for (int i = 0; i < list.Count; i++)
		{
			float num3 = list[i].GetStart().GetX();
			if (num3 < num && num3 < eDCHBILGFLD)
			{
				num = num3;
			}
			if (num2 < num3 && nNCHJCLKHHA < num3)
			{
				num2 = num3;
			}
		}
		_Physics.SetWallShift(num, num2);
	}

	protected void Clear()
	{
		RemoveAllEventListener();
		if (_Collision != null)
		{
			controller.RemoveAllEventListener();
		}
		if (_Animation != null)
		{
			_Animation.RemoveAllEventListener();
		}
		_Physics = null;
		_Strike = null;
		_Animation = null;
		_Collision = null;
		ai = null;
		_ModelObject = null;
		if (EventData != null) EventData.Clear();
		EventData = null;
		_Enemies.Clear();
		weaponModels.Clear();
	}

    // best guess for name
	protected void ObserveEnemyAnimationStarted(Model source)
	{
        if (IsAiControlled() || AiData.get_BothBotEnabled())
            ai.StartAnimationEnemy(source);
        SetDecisionDelay(0);
	}

	protected float GetGuardFactor()
	{
		if (_Animation.FindInterval(IntervalAnimation.IntervalType.INTERVAL_BLOCK) != null)
		{
			return 5f;
		}
		return 1f;
	}

	protected void ApplyItemChange(ItemInfo item, bool EPKEEMFHHFM, bool BNDJNLALHKL = true, bool DDMEACNNLJN = true)
	{
		if (item == null)
		{
			return;
		}
		ItemInfo dJKEECEOCJB = Parameters.GetItemByType(item.Type);
		if (dJKEECEOCJB == null || !(dJKEECEOCJB.Name != item.Name))
		{
			return;
		}
		disarmedItem = ((!EPKEEMFHHFM) ? null : dJKEECEOCJB);
		Parameters.SetItemByType(item.Type, item);
		string aPJJEFJHJGK = GameUtils.ShockSettings.SetAttributeName;
		int OEMALIFPGPO = 0;
		item.ItemAttributes.Get(aPJJEFJHJGK, ref OEMALIFPGPO);
		Parameters.FinalAttributes.Set(aPJJEFJHJGK, (!DDMEACNNLJN) ? GameUtils.ShockSettings.SetAttributeValue : OEMALIFPGPO);
		if (BNDJNLALHKL)
		{
			List<ItemInfo> hELFDCAIJNE = Parameters.GetEquippedItems();
			List<PerkInfoItem> mAFPBEFKNGE = Parameters.GetAllPerks();
			List<PerkInfoItem> cFKCGBEONAM = null;
			if (GetCombatTarget() != null && GetCombatTarget().Parameters != null)
			{
				cFKCGBEONAM = GetCombatTarget().Parameters.GetAllPerks();
			}
			ReloadAnimationsForItems(hELFDCAIJNE, mAFPBEFKNGE, cFKCGBEONAM);
		}
	}

	public void SwapPerkItem(ItemInfo item)
	{
		ApplyItemChange(item, false, true, false);
	}

	protected void ApplyDisarm()
	{
		if (_ModelObject.IsShock())
		{
			return;
		}
		ItemInfo jGMLKIPCFII = Parameters.Weapon;
		ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(GameUtils.ShockSettings.WeaponName);
		if (dJKEECEOCJB == null)
		{
			return;
		}
		ApplyItemChange(dJKEECEOCJB, true, true, false);
		_ModelObject.SetShock(true);
		Parameters.DisableActivePerks();
		List<ModelNode> list = _ModelObject.GetAllNodes();
		foreach (ModelNode item in list)
		{
			if (item.IsShock())
			{
				float lHNJJFDIJKK = GameUtils.ShockSettings.Impulse.GetX() / item.GetWeight();
				float fFFHIOALHGM = GameUtils.ShockSettings.Impulse.GetY() / item.GetWeight();
				float pDCENMEKIAP = GameUtils.ShockSettings.Impulse.GetZ() / item.GetWeight();
				item.GetStart().Add(lHNJJFDIJKK, fFFHIOALHGM, pDCENMEKIAP);
			}
		}
		Model fGCODGKLHED = GetCombatTarget();
		if (fGCODGKLHED != null)
		{
			Model fGCODGKLHED2 = fGCODGKLHED.GetRootModel();
			fGCODGKLHED2.ai.SetWeaponEnemy(dJKEECEOCJB.EffectiveTacticSubtype);
		}
		ai.SetWeaponBot(dJKEECEOCJB.EffectiveTacticSubtype);
		DisarmData eHCLMBADLKH = new DisarmData(this, jGMLKIPCFII.InnatePerks);
		CallEvent(16, eHCLMBADLKH);
	}

	protected void TickDisarmAndPain()
	{
		if (!_ModelObject.IsShock() && disarmCountdown >= 0)
		{
			if (disarmCountdown == 0)
			{
				ApplyDisarm();
			}
			disarmCountdown--;
		}
		pain = Mathf.Max(pain - GameUtils.ShockSettings.FrameReduction, 0f);
	}

	protected ModelParameters CreateChildParameters(List<CopyItemInfo> HELFDCAIJNE = null)
	{
		if (HELFDCAIJNE == null)
		{
			HELFDCAIJNE = new List<CopyItemInfo>();
		}
		RaidModelParameters kAOPLEPILDH = new RaidModelParameters();
		kAOPLEPILDH.SetLevel((ObscuredInt)(1));
		kAOPLEPILDH.Dan = 1;
		kAOPLEPILDH.Damage = 1f;
		kAOPLEPILDH.Difficulty = 1f;
		kAOPLEPILDH.FirstName = string.Empty;
		kAOPLEPILDH.LastName = string.Empty;
		kAOPLEPILDH.Avatar = GameUtils.GetDefaultAvatar();
		kAOPLEPILDH.AiControlled = false;
		kAOPLEPILDH.AnimationEnabled = true;
		kAOPLEPILDH.IsPlayer = false;
		kAOPLEPILDH.UserControlled = false;
		kAOPLEPILDH.IsWinner = false;
		kAOPLEPILDH.RoundEnded = false;
		kAOPLEPILDH.IsDead = false;
		kAOPLEPILDH.RoundsWon = 0;
		kAOPLEPILDH.MaxLife = 0f;
		kAOPLEPILDH.Skeleton = null;
		kAOPLEPILDH.Armor = null;
		kAOPLEPILDH.Helm = null;
		kAOPLEPILDH.Weapon = null;
		kAOPLEPILDH.Ranged = null;
		kAOPLEPILDH.Magic = null;
		kAOPLEPILDH.RaidChargeItem = null;
		if (HELFDCAIJNE.Count != 0)
		{
			int i = 0;
			for (int count = HELFDCAIJNE.Count; i < count; i++)
			{
				ApplyCopyItem(HELFDCAIJNE[i], kAOPLEPILDH);
			}
		}
		kAOPLEPILDH.BuildModelDocuments();
		kAOPLEPILDH.CalculateAttributes();
		return kAOPLEPILDH;
	}

	protected ModelObject GetModelObjectByType(ModelType.ModelTargetType HJMMACIELFG)
	{
		ModelObject result = null;
		switch (HJMMACIELFG)
		{
		case ModelType.ModelTargetType.MODEL_NULL:
		case ModelType.ModelTargetType.MODEL_THIS:
			result = _ModelObject;
			break;
		case ModelType.ModelTargetType.MODEL_OTHER:
			if (combatTarget != null)
			{
				result = combatTarget._ModelObject;
			}
			else
			{
				// Shop previews have no opponent. The original diagnostic says this
				// should align to self, but the decompiled body returned null and left
				// projectile nodes unresolved (notably Death Ray and Fire Pillar).
				result = _ModelObject;
				Debug.Log("[MagicTrace] align-fallback actor=" + get_Name() + " requested=Enemy resolved=Self");
			}
			break;
		case ModelType.ModelTargetType.MODEL_PARENT:
			if (parentModel != null)
			{
				result = parentModel._ModelObject;
			}
			break;
		}
		return result;
	}

	public bool IsForceCritical()
	{
		return forceCritical;
	}

	public void SetForceCritical(bool value)
	{
		forceCritical = value;
	}

	protected float GetCriticalChance()
	{
		if (IsForceCritical())
		{
			return 100f;
		}
		Model fGCODGKLHED = GetCombatTarget();
		if (Fight.GetCurrentFight().GetFightType() != BattleType.FightRaid || !fGCODGKLHED.IsPlayerModel() || fGCODGKLHED.modelStats.CheckCritAvailable())
		{
			return GameUtils.CriticalHitDefaults.GetProbability(fGCODGKLHED);
		}
		return 0f;
	}

	protected void ApplyAttributeLifeRegen()
	{
		Fight gDBOMJODDEA = Fight.GetCurrentFight();
		if (gDBOMJODDEA == null)
		{
			return;
		}
		int OEMALIFPGPO = 0;
		if (Parameters.FinalAttributes.Get(GameUtils.GetRegeneration().Attribute, ref OEMALIFPGPO) && GetCombatTarget() != null)
		{
			float num = (float)OEMALIFPGPO * GameUtils.GetRegeneration().Base * GetCombatTarget().GetPowerMultiplier();
			if (num != 0f && (num > 0f || !damageImmune))
			{
				gDBOMJODDEA.UpdateLife(this, num);
			}
		}
	}

	protected void OnAnimationActions(object data)
	{
		List<ActionAnimation> aFENHJFICNN = (List<ActionAnimation>)data;
		RunActions(aFENHJFICNN);
	}

	protected void BuildAvailableAnimations(List<ItemInfo> FJKCMJNAFJD, bool ILMJFHCNLHC, List<string> DANNKMJOOOH = null, SceneTypes NFNJJIGAKNN = SceneTypes.SceneFight, List<PerkInfoItem> MAFPBEFKNGE = null, List<PerkInfoItem> CFKCGBEONAM = null)
	{
		AnimationData.CollectAvailableAnimations(availableAnimations, FJKCMJNAFJD, ILMJFHCNLHC, Parameters.ExcludedMoveNames, NFNJJIGAKNN, MAFPBEFKNGE, CFKCGBEONAM);
		PreloadAnimationAssets();
	}

	protected void BuildTriggers(List<ItemInfo> FJKCMJNAFJD, bool ILMJFHCNLHC, SceneTypes NFNJJIGAKNN = SceneTypes.SceneFight, List<PerkInfoItem> MAFPBEFKNGE = null, List<PerkInfoItem> CFKCGBEONAM = null)
	{
		AnimationData.CollectAvailableTriggers(triggers, FJKCMJNAFJD, ILMJFHCNLHC, NFNJJIGAKNN, MAFPBEFKNGE, CFKCGBEONAM);
		PreloadTriggerAssets();
	}

	protected void PreloadAnimationAssets()
	{
		foreach (InfoAnimation item in availableAnimations)
		{
			item.PreloadEffects();
			item.PreloadSounds();
		}
	}

	protected void PreloadTriggerAssets()
	{
		foreach (Trigger item in triggers)
		{
			item.PreloadEffects();
			item.PreloadSounds();
		}
	}

	protected void ApplyCopyItem(CopyItemInfo item, ModelParameters IHEFAMAFBIA)
	{
		ItemInfo dJKEECEOCJB = null;
		if (!string.IsNullOrEmpty(item.Name))
		{
			dJKEECEOCJB = ListSF.GetItems().GetItemByName(item.Name).Clone();
		}
		else if (!string.IsNullOrEmpty(item.CopyParentType) && Parameters.GetItemByType(item.CopyParentType) != null)
		{
			dJKEECEOCJB = Parameters.GetItemByType(item.CopyParentType).Clone();
		}
		if (dJKEECEOCJB != null)
		{
			dJKEECEOCJB.MergeWithItem(item);
			IHEFAMAFBIA.SetItemByType(dJKEECEOCJB.Type, dJKEECEOCJB);
			ListSF.GetInstance().OnItemGiven(dJKEECEOCJB);
		}
	}

	protected void UpdateCombo()
	{
		if (parentModel != null)
		{
			parentModel.UpdateCombo();
		}
		else
		{
			comboCounter.UpdateCombo();
		}
	}

	public void RegisterComboHit()
	{
		if (parentModel != null)
		{
			parentModel.RegisterComboHit();
		}
		else
		{
			comboCounter.RegisterHit();
		}
	}

	protected void ApplyPendingStrike()
	{
		if (strikePending && _Animation.GetCurrentInfo().HasPhysics)
		{
			ApplyStrike(LastStrike);
		}
		strikePending = false;
	}

	private void RecordStrikeStatistics(Model KKCCDBPOFOC, InfoAnimation DBOLBEOCEME, float CKKFKEIELCP, StrikeResult PPIAOBPLGOK)
	{
		Model fGCODGKLHED = GetRootModel();
		Model fGCODGKLHED2 = KKCCDBPOFOC.GetRootModel();
		fGCODGKLHED.modelStats.RecordDamage(true, DBOLBEOCEME, CKKFKEIELCP);
		fGCODGKLHED2.modelStats.RecordDamage(false, DBOLBEOCEME, CKKFKEIELCP);
		fGCODGKLHED2.modelStats.AddRaidHitInfo(LastStrike.IsBlocked, LastStrike.IsCritical);
	}

	private bool ShouldCauseShock(StrikeResult PPIAOBPLGOK, Model HFGPAELCNMF)
	{
		if (_IsShock)
		{
			return false;
		}
		float num = LastStrike.FinalDamage / HFGPAELCNMF.GetPowerMultiplier();
		bool flag = SetPain((!damageImmune) ? num : 0f);
		ModelParameters kMMJCHDKBDO = HFGPAELCNMF.Parameters;
		float nIPKAAEFMNG = GameUtils.ShockSettings.CriticalHitChanceBase;
		string aDAOLENDOME = GameUtils.ShockSettings.CriticalHitChanceAttribute;
		int OEMALIFPGPO = 0;
		kMMJCHDKBDO.FinalAttributes.Get(aDAOLENDOME, ref OEMALIFPGPO);
		float num2 = nIPKAAEFMNG * (float)OEMALIFPGPO;
		float pAKGFJEEJLD = GameUtils.ShockSettings.HeadHitChanceBase;
		string pOJAOGMJBDC = GameUtils.ShockSettings.HeadHitChanceAttribute;
		int OEMALIFPGPO2 = 0;
		kMMJCHDKBDO.FinalAttributes.Get(pOJAOGMJBDC, ref OEMALIFPGPO2);
		float num3 = pAKGFJEEJLD * (float)OEMALIFPGPO2;
		bool flag2 = false;
		bool flag3 = false;
		if (LastStrike.IsCritical)
		{
			float num4 = num2 * num;
			float num5 = Eclipse.Multiplayer.VersusDeterminism.Range(0f, 1f);
			flag2 = num4 > num5;
		}
		if (LastStrike.IsHeadHit && !LastStrike.IsBlocked)
		{
			float num6 = num3 * num;
			float num7 = Eclipse.Multiplayer.VersusDeterminism.Range(0f, 1f);
			flag3 = num6 > num7;
		}
		return flag || flag3 || flag2;
	}

	private void TickButtonCooldowns()
	{
		if (buttonCooldowns.PunchActive && buttonCooldowns.PunchProgress != buttonCooldowns.PunchMax)
		{
			buttonCooldowns.PunchProgress += buttonCooldowns.PunchMax / (float)(buttonCooldowns.PunchRegenFrames * GameUtils.GetSlowMode());
			if (buttonCooldowns.PunchProgress > buttonCooldowns.PunchMax)
			{
				buttonCooldowns.PunchProgress = buttonCooldowns.PunchMax;
			}
			EventActBtnSettings eHCLMBADLKH = new EventActBtnSettings(FightCID.Punch, buttonCooldowns.PunchProgress, 1);
			CallEvent(12, eHCLMBADLKH);
		}
		if (buttonCooldowns.KickActive && buttonCooldowns.KickProgress != buttonCooldowns.KickMax)
		{
			buttonCooldowns.KickProgress += buttonCooldowns.KickMax / (float)(buttonCooldowns.KickFrames * GameUtils.GetSlowMode());
			if (buttonCooldowns.KickProgress > buttonCooldowns.KickMax)
			{
				buttonCooldowns.KickProgress = buttonCooldowns.KickMax;
			}
			EventActBtnSettings eHCLMBADLKH2 = new EventActBtnSettings(FightCID.Kick, buttonCooldowns.KickProgress, 1);
			CallEvent(12, eHCLMBADLKH2);
		}
		if (buttonCooldowns.MissileActive && buttonCooldowns.MissileProgress != buttonCooldowns.MissileMax)
		{
			buttonCooldowns.MissileProgress += buttonCooldowns.MissileMax / (float)(buttonCooldowns.MissileFrames * GameUtils.GetSlowMode());
			if (buttonCooldowns.MissileProgress > buttonCooldowns.MissileMax)
			{
				buttonCooldowns.MissileProgress = buttonCooldowns.MissileMax;
			}
			EventActBtnSettings eHCLMBADLKH3 = new EventActBtnSettings(FightCID.MissileButton, buttonCooldowns.MissileProgress, 1);
			CallEvent(12, eHCLMBADLKH3);
		}
		if (buttonCooldowns.RaidChargeActive && buttonCooldowns.RaidChargeProgress != buttonCooldowns.RaidChargeMax)
		{
			buttonCooldowns.RaidChargeProgress += buttonCooldowns.RaidChargeMax / (float)(buttonCooldowns.RaidChargeFrames * GameUtils.GetSlowMode());
			if (buttonCooldowns.RaidChargeProgress > buttonCooldowns.RaidChargeMax)
			{
				buttonCooldowns.RaidChargeProgress = buttonCooldowns.RaidChargeMax;
			}
			EventActBtnSettings eHCLMBADLKH4 = new EventActBtnSettings(FightCID.RaidChargeButton, buttonCooldowns.RaidChargeProgress, 1);
			CallEvent(12, eHCLMBADLKH4);
		}
	}
}
