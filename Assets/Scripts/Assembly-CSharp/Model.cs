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

		public void AddAnimations(List<InfoAnimation> animations)
		{
			for (int i = 0; i < animations.Count; i++)
			{
				InfoAnimation animation = animations[i];
				if (animation.MoveData.ShopData.IsExists)
				{
					ShopAnimations.Add(animation);
				}
				List<EventAnimation> eventAnimations = animation.MoveData.Events;
				for (int j = 0; j < eventAnimations.Count; j++)
				{
					List<InfoAnimation> list = GetListForEvent(eventAnimations[j].Type);
					int count = list.Count;
					if (count == 0 || list[count - 1] != animation)
					{
						list.Add(animation);
					}
				}
				AllAnimations.Add(animation);
			}
		}

		public List<InfoAnimation> GetListForEvent(EventAnimation.EventAnimationType eventType)
		{
			switch (eventType)
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

		public void AddTriggers(List<Trigger> triggerList)
		{
			foreach (Trigger item in triggerList)
			{
				List<EventAnimation> eventAnimations = item.Definition.Events;
				foreach (EventAnimation item2 in eventAnimations)
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

		public List<Trigger> GetListForEvent(EventAnimation.EventAnimationType eventType)
		{
			switch (eventType)
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
		public Model sourceModel;

        // best guess for name
        public Model SourceModel => sourceModel;

		public Model Opponent;

		public object Data;

		public string ConditionName;

		public void Clear()
		{
			sourceModel = null;
			Opponent = null;
			Data = null;
		}
	}

	public class DisarmData
	{
		public Model Owner;

		public List<PerkInfoItem> LostPerks;

		public DisarmData(Model _Model, List<PerkInfoItem> lostPerks)
		{
			Owner = _Model;
			LostPerks = lostPerks;
		}
	}

	public class EventActBtnSettings
	{
		public FightCID Button;

		public float Value;

		public int FrameCount;

		public int BulletsCount;

		public EventActBtnSettings(FightCID button, float _value, int _frames = -1, int bulletsCount = -1)
		{
			Button = button;
			Value = _value;
			FrameCount = _frames;
			BulletsCount = bulletsCount;
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

		public void AddProcedPerk(int perkId)
		{
			ProcedPerks.AddIfNotExist(perkId);
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

	public static Vector3f GetCameraMidpoint(Model firstModel, Model secondModel)
	{
		return ModelObject.GetNodesMidpoint(firstModel._ModelObject.GetCenterOfMassNode(), secondModel._ModelObject.GetCenterOfMassNode());
	}

	public static Vector3f GetCameraMidpoint(ModelObject firstModel, ModelObject secondModel)
	{
		return Vector3f.Middle(CameraAnchor(firstModel), CameraAnchor(secondModel));
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

	public static float GetDistanceModels(Model firstModel, Model secondModel)
	{
		return Vector3f.Distance(firstModel.GetPosition(), secondModel.GetPosition());
	}

	public void ChangeSpeed(float speed)
	{
		_Physics.ChangeSpeed(speed);
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

	private void SetPoseFromPositions(List<Vector3f> positions, bool mirrorByFacing = false)
	{
		if (mirrorByFacing)
		{
			int num = GetFacingSign();
			if (num == -1)
			{
				int count = positions.Count;
				List<Vector3f> list = new List<Vector3f>(count);
				int i = 0;
				for (int num2 = count; i < num2; i++)
				{
					list.Add(new Vector3f(0f - positions[i].GetX(), positions[i].GetY(), positions[i].GetZ()));
				}
				_ModelObject.AlignToFrame(list);
			}
			else
			{
				_ModelObject.AlignToFrame(positions);
			}
		}
		else
		{
			_ModelObject.AlignToFrame(positions);
		}
	}

	public void SetPoseFromPositions(List<Vector3f> positions, int facingSign, ModelNode alignNode)
	{
		if (facingSign == -1)
		{
			int count = positions.Count;
			List<Vector3f> list = new List<Vector3f>(count);
			int i = 0;
			for (int num = count; i < num; i++)
			{
				list.Add(new Vector3f(0f - positions[i].GetX(), positions[i].GetY(), positions[i].GetZ()));
			}
			_ModelObject.AlignToFrame(list, alignNode);
		}
		else
		{
			_ModelObject.AlignToFrame(positions, alignNode);
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
		EventData.sourceModel = this;
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

	public bool RenderCollision(bool checkOnly)
	{
		if (!_perkCollisionDisabled && HasEnemies() && !Parameters.RoundEnded && _Animation.GetCurrentInfo() != null && !IsInPhysics())
		{
			return CheckCollision(GetCombatTarget(), checkOnly);
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
		Tactic tactic = Parameters.FightTactic;
		if (tactic != null)
		{
			if (tactic.get_Type() == Tactic.TacticType.TacticRandom)
			{
				TriggerRandomAnimation();
			}
			else if (tactic.get_Type() == Tactic.TacticType.TacticTabular)
			{
				RunTabularTactic();
			}
		}
	}

	public void RunTabularTactic()
	{
		Model opponent = GetCombatTarget();
		ModelConditions modelConditions = _ModelConditions;
		InfoAnimation animation = null;
		if (modelConditions == null)
		{
			Debug.LogError("modelConditions is Empty");
		}
		else
		{
			animation = ai.Render(opponent, FrameInRound);
		}
		if (animation != null)
		{
			ConditionKeys keysCondition = animation.GetFirstKeysCondition();
			if (keysCondition == null)
			{
				GameLog.Error("tactics: conditionKeys is null for {0}", animation.Name);
				keysCondition = animation.GetFirstKeysCondition();
			}
			else
			{
				KeyData keyData = keysCondition.RequiredKeys;
				_Animation.RequiredInfo = animation;
				keyData.IsInverted = true;
				PlayAnimation(keyData);
			}
		}
		else
		{
			controller.Reset();
		}
	}

	public void TriggerRandomAnimation(bool useDecisionDelay = true)
	{
		if (useDecisionDelay)
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

	public bool CheckCollision(Model opponent = null, bool checkOnly = false)
	{
		bool result = false;
		IntervalAttack intervalAttack = _Animation.FindInterval(IntervalAnimation.IntervalType.INTERVAL_ATTACK) as IntervalAttack;
		if (intervalAttack != null)
		{
			if (opponent == null)
			{
				opponent = GetCombatTarget();
			}
			if (opponent == null) return false;
			if (!PairedGrabAllowsStrike(opponent))
			{
				return false;
			}
			if ((opponent.GetAnimationModule().FindInterval(IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE) == null || (intervalAttack.GetIgnoresInvulnerable() && intervalAttack.GetIgnoredInvulnerableNames().Count == 0) || (intervalAttack.GetIgnoresInvulnerable() && opponent.GetAnimationModule().CheckIntervals(intervalAttack.GetIgnoredInvulnerableNames()))) && _Collision.Render(opponent._ModelObject, _Animation.GetAttackingEdges(), intervalAttack))
			{
				if (!checkOnly)
				{
					StrikeModel(opponent, intervalAttack);
				}
				result = true;
			}
		}
		return result;
	}

	public void NoOpHookB()
	{
	}

	public void PressAnyKey(FightCID button)
	{
		bool inputAccepted = (button != FightCID.MagicButton || magicCharges != 0 || magicCharges != 0 || GameUtils.AlwaysMagicMode) && (button != FightCID.MissileButton || !buttonCooldowns.MissileActive || buttonCooldowns.MissileProgress == buttonCooldowns.MissileMax) && (button != FightCID.Kick || !buttonCooldowns.KickActive || buttonCooldowns.KickProgress == buttonCooldowns.KickMax) && (button != FightCID.Punch || !buttonCooldowns.PunchActive || buttonCooldowns.PunchProgress == buttonCooldowns.PunchMax) && (button != FightCID.RaidChargeButton || !buttonCooldowns.RaidChargeActive || buttonCooldowns.RaidChargeProgress == buttonCooldowns.RaidChargeMax) && inputEnabled;
		if (button == FightCID.MagicButton)
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
			controller.OnPressAnyKey((int)button);
		}
	}

	public void ReleaseAnyKey(FightCID button)
	{
		if (inputEnabled)
		{
			controller.OnReleaseAnyKey((int)button);
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

	public void ShiftModelPosition(Vector3f shift, bool shiftAnimation = false)
	{
		_ModelObject.TranslateAllNodes(shift);
		_Physics.IterativeProcess();
		_ModelObject.UpdateCenterOfMass();
		_ModelObject.UpdateMacroNodes();
		_ModelObject.ResetNodeVelocities();
		if (shiftAnimation)
		{
			_Animation.ShiftSequence(shift.GetX(), shift.GetY(), shift.GetZ());
			_Animation.ShiftBuffer(shift);
		}
	}

	public void ReloadAnimationsForItems(ModelParameters parameters)
	{
		List<ItemInfo> items = parameters.ConditionItems;
		List<PerkInfoItem> ownPerks = parameters.GetAllPerks();
		List<PerkInfoItem> opponentPerks = null;
		if (GetCombatTarget() != null && GetCombatTarget().Parameters != null)
		{
			opponentPerks = GetCombatTarget().Parameters.GetAllPerks();
		}
		ReloadAnimationsForItems(items, ownPerks, opponentPerks);
	}

	public void ReloadAnimationsForItems(List<ItemInfo> items, List<PerkInfoItem> ownPerks = null, List<PerkInfoItem> opponentPerks = null)
	{
		availableAnimations.Clear();
		AnimationData.CollectAvailableAnimations(availableAnimations, items, false, Parameters.ExcludedMoveNames, Parameters.SceneType, ownPerks, opponentPerks);
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

	public void RemoveEnemy(Model enemy)
	{
		_Enemies.Remove(enemy);
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
			Model opponent = GetCombatTarget();
			if (opponent != null)
			{
				Sign = ((!(_ModelObject.GetPivotNode().GetStart().GetX() > opponent._ModelObject.GetPivotNode().GetStart().GetX())) ? 1 : (-1));
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

	public void SetDistanceToEnemy(Model enemy)
	{
		float num = _ModelObject.GetPivotNode().GetStart().GetX() - enemy._ModelObject.GetPivotNode().GetStart().GetX();
		distanceToEnemy = ((!(num < 0f)) ? num : (0f - num));
	}

	public void SetDistanceToNearestWall()
	{
		float num = _ModelObject.GetPivotNode().GetStart().GetX();
		distanceToBackWall = ((GetFacingSign() != -1) ? (num - wallBounds.LeftX) : (wallBounds.RightX - num));
	}

	private void LoadModelComponents(List<string> components)
	{
		Clear();
		_ModelObject = new ModelObject();
		_ModelObject.SetModel(this);
		ModelLoader.Load(_ModelObject, components);
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

	public void SetWalls(float leftX, float rightX, int frontMargin, int backMargin)
	{
		wallBounds.LeftX = leftX;
		wallBounds.RightX = rightX;
		_Physics.SetWallShift(wallBounds.LeftX, wallBounds.RightX);
		_Animation.SetAligns(leftX, rightX, frontMargin, backMargin);
	}

	public void SetFixedSign(bool value, int sign = 1)
	{
		if (value)
		{
			fixedSign = sign;
		}
		else
		{
			fixedSign = 0;
		}
	}

	public void AddEnemy(Model enemy)
	{
		_Animation.SetParentAnimation((parentModel == null) ? null : parentModel._Animation);
		if (enemy == null)
		{
			return;
		}
		_Enemies.Add(enemy);
		List<WeaponModel> list = enemy.GetWeaponModels();
		foreach (WeaponModel item in list)
		{
			_Enemies.Add(item);
		}
		_Animation.SetOtherAnimation(enemy._Animation);
		if (enemy.Parameters.Weapon != null)
		{
			ai.SetWeaponEnemy(enemy.Parameters.Weapon.EffectiveTacticSubtype);
		}
		SetNearestEnemy();
	}

	public void InitializeMagicCharge()
	{
		SetMagicCharges(0);
		SetMagicChargeFraction(GameUtils.MagicConfig.GetInitialCharge(this));
		UpdateMagicButton();
	}

	public void AddMagicChargeFraction(float fraction)
	{
		if (magicCharges == 0)
		{
			SetMagicChargeFraction(magicChargeFraction + fraction);
		}
	}

	public void ChangeAiTactic(string tacticName)
	{
		ai.ChangeTactic(tacticName);
	}

	public void ChangeAiTactic(Tactic tactic)
	{
		ai.ChangeTactic(tactic);
	}

	public void SetTactic(string tacticName)
	{
		Tactic tactic = AiData.GetTacticByName(tacticName);
		SetTactic(tactic);
	}

	public void SetTactic(Tactic tactic)
	{
		if (tactic != null)
		{
			Parameters.FightTactic = tactic;
			ChangeAiTactic(tactic);
		}
	}

	public void AddMagicCharges(int count)
	{
		if (count < 0)
		{
			magicChargesUsed++;
		}
		SetMagicCharges(magicCharges + count);
	}

	public void UpdateMagicButton()
	{
		Model parent = parentModel;
		if (parent == null)
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
					EventActBtnSettings buttonSettings = new EventActBtnSettings(FightCID.MagicButton, num);
					CallEvent(12, buttonSettings);
				}
				else
				{
					float fullFraction = 1f;
					EventActBtnSettings eHCLMBADLKH2 = new EventActBtnSettings(FightCID.MagicButton, fullFraction);
					CallEvent(12, eHCLMBADLKH2);
				}
			}
		}
		else
		{
			parent.UpdateMagicButton();
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

	public virtual bool PlayAnimation(InfoAnimation animation, int direction = 0, bool isFrameShift = false, int frameShift = -1)
	{
		bool traceMagicAnimation = animation != null &&
			animation.Name != null && animation.Name.IndexOf("Magic") >= 0;
		if (IsModelActive() && !_Physics.IsPhysics())
		{
			if (direction == 0)
			{
				direction = GetFacingSign();
			}
			_Collision.ResetLastStrike();
			bool started = _Animation.PlayInfo(animation, direction, !animation.NoInterpolationFrames, isFrameShift, frameShift);
			if (traceMagicAnimation)
			{
				Debug.Log("[MagicTrace] animation-select actor=" + get_Name() +
					" animation=" + animation.Name +
					" animationFile=" + animation.FileName +
					" started=" + started +
					" direction=" + direction +
					" items=" + GetMagicTraceItems());
			}
			return started;
		}
		if (traceMagicAnimation)
		{
			Debug.Log("[MagicTrace] animation-select actor=" + get_Name() +
				" animation=" + animation.Name +
				" animationFile=" + animation.FileName +
				" started=False blockedByModelState=True" +
				" items=" + GetMagicTraceItems());
		}
		return false;
	}

	public bool PlayAnimation(string name, int direction = 0)
	{
		InfoAnimation animation = null;
		foreach (InfoAnimation item in availableAnimations)
		{
			if (item.Name == name)
			{
				animation = item;
				break;
			}
		}
		if (animation == null)
		{
			Debug.LogWarning("Animation '" + name + "' is unavailable on model '" + get_Name() + "'");
			return false;
		}
		if (direction == 0)
		{
			// Named actions bypass SelectAnimation, which normally resolves SetDirection.
			// Paired throws must use the same facing before sharing an animation origin.
			direction = animation.GetDirection(_ModelConditions, GetFacingSign());
		}
		return PlayAnimation(animation, direction);
	}

	public bool PlayAnimation(KeyData keyData)
	{
		controller.Reset();
		controller.SetCurrentKeys(keyData);
		controller.CallKeyPressed();
		return true;
	}

	public void PlayAnimationDelay(InfoAnimation animation, int direction = 0, bool isFrameShift = false, int frameShift = -1)
	{
		pendingPlayRequest.Animation = animation;
		pendingPlayRequest.Direction = direction;
		pendingPlayRequest.IsFrameShift = isFrameShift;
		pendingPlayRequest.FrameShift = frameShift;
		Vector3f velocity = new Vector3f(animation.GetVelocity());
		if (!velocity.IsEqual(0f, 0f, 0f))
		{
			velocity.SetX(velocity.GetX() * (float)direction);
			_Animation.MoveByVelocity(velocity);
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

	public void SetDelayedStrike(InfoAnimation animation, bool isStrikeResult)
	{
		DelayedStrikeData.Animation = animation;
		DelayedStrikeData.Names = animation.GetTemplateNames();
		DelayedStrikeData.IsStrikeResult = isStrikeResult;
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
		InfoAnimation animation = GetCurrentAnimation();
		Model opponent = GetCombatTarget();
		if (opponent != null)
		{
			Model fGCODGKLHED2 = opponent.GetRootModel();
			fGCODGKLHED2.modelStats.CommitPendingStatistics(true, animation);
		}
		Model fGCODGKLHED3 = GetRootModel();
		fGCODGKLHED3.modelStats.CommitPendingStatistics(false, animation);
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

	public void OnAnimationStarted(object data)
	{
		_Collision.ResetInterval();
		EventData.Data = data;
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

	public void OnStopAnimation(object data)
	{
		EventData.Data = data;
		InfoAnimation animation = data as InfoAnimation;
		if (animation.EndsStage)
		{
			EndStageReached = animation.EndsStage;
		}
		NotifyAnimationEnded();
		CallEvent(3, EventData);
	}

	public void OnStartInterval(object data)
	{
		EventData.Data = data;
		CallEvent(0, EventData);
	}

	public void OnStopInteval(object data)
	{
		EventData.Data = data;
		CallEvent(1, EventData);
		IntervalAnimation interval = data as IntervalAnimation;
		if (GameUtils.RandomTactics.IsIntervalByName(interval.Name))
		{
			InfoAnimation animation = GetCurrentAnimation();
			if (animation != null)
			{
				decisionDelay = GameUtils.RandomTactics.GetDelayByName(animation.GetTemplateNames());
			}
		}
		if (interval.Name == "Uninterrupt")
		{
			InfoAnimation currentInfo = _Animation.GetCurrentInfo();
			Model opponent = GetCombatTarget();
			if (opponent != null)
			{
				Model fGCODGKLHED2 = opponent.GetRootModel();
				fGCODGKLHED2.modelStats.RecordUse(true, currentInfo);
			}
			Model fGCODGKLHED3 = GetRootModel();
			fGCODGKLHED3.modelStats.RecordUse(false, currentInfo);
		}
	}

	public void StrikeModel(Model victim, IntervalAttack intervalAttack)
	{
		ModelCollision.StrikeHit strikeHit = _Collision.Strike;
		Vector3f impulse = new Vector3f(intervalAttack.GetImpulse());
		impulse.SetX(impulse.GetX() * (float)_Animation.GetSign());
		impulse.SetX(impulse.GetX() * impulseFactor.GetX());
		impulse.SetY(impulse.GetY() * impulseFactor.GetY());
		impulse.SetZ(impulse.GetZ() * impulseFactor.GetZ());
		LastComboTime = intervalAttack.GetComboTime();
		victim.Strike(strikeHit.VictimEdge, strikeHit.AttackerEdge, strikeHit.GetPoint(), strikeHit.GetSecondPoint(), this, impulse);
	}

	public void Strike(ModelEdge victimEdge, ModelEdge attackerEdge, Vector3f point, Vector3f edgePoint, Model attacker, Vector3f impulse)
	{
		strikesTaken++;
		IntervalAttack intervalAttack = attacker._Animation.FindInterval(IntervalAnimation.IntervalType.INTERVAL_ATTACK) as IntervalAttack;
		EventData.Data = intervalAttack;
		EventData.ConditionName = intervalAttack.GetReactionName(attacker.GetReactionFrame());
		if (intervalAttack.GetIgnoresBlock())
		{
			if (intervalAttack.GetIgnoredBlockNames().Count == 0)
			{
				RemoveInterval(IntervalAnimation.IntervalType.INTERVAL_BLOCK);
			}
			else
			{
				RemoveIntervals(intervalAttack.GetIgnoredBlockNames());
			}
		}
		// Eclipse training: idle stances guard by themselves, so the dummy's block rule
		// drops the guard the same way an unblockable attack does.
		if (Eclipse.Multiplayer.VersusTraining.Active && Eclipse.Multiplayer.VersusTraining.ShouldDropGuard(this))
		{
			RemoveInterval(IntervalAnimation.IntervalType.INTERVAL_BLOCK);
		}
		ai.OnGetHit();
		if (attacker != null)
		{
			attacker.ai.OnHitEnemy();
		}
		Parameters.RewardsEnabled = false;
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().LastStrikeResult = LastStrike;
		}
		LastStrike.ProcedPerks.Clear();
		LastStrike.Victim = this;
		LastStrike.AttackerModel = attacker;
		LastStrike.Target = ((!Parameters.IsPlayer) ? 1 : 0);
		LastStrike.VictimEdge = victimEdge;
		LastStrike.Impulse.Set(impulse);
		LastStrike.AttackerEdge = attackerEdge;
		LastStrike.AttackAnimation = attacker._Animation.GetCurrentInfo();
		LastStrike.Point = point;
		LastStrike.EdgePoint = edgePoint;
		LastStrike.BaseDamage = intervalAttack.GetDamage();
		LastStrike.IsBlocked = IsBlocking();
		LastStrike.DefenceAttribute = GetDefenseAttribute(intervalAttack, LastStrike.IsBlocked, victimEdge);
		if (!LastStrike.IsBlocked)
		{
			if (!attacker.IsComboActive())
			{
				nonComboHitsTaken++;
			}
			attacker.RegisterComboHit();
		}
		LastStrike.HitsTakenCount = nonComboHitsTaken;
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().OnModelPreCrit(EventData);
		}
		bool flag = intervalAttack.GetNoCritical();
		LastStrike.IsCritical = !LastStrike.IsBlocked && !flag && GameUtils.IsProbality(GetCriticalChance());
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().OnModelPostCrit(EventData);
		}
		LastStrike.RawDamage = GetTotalDamage(intervalAttack, LastStrike.IsBlocked, LastStrike.IsCritical, victimEdge);
		LastStrike.FinalDamage = Parameters.ResolveStrikeDamage(
			LastStrike.RawDamage, out LastStrike.IsOverkill);
		string text = "Head";
		string text2 = string.Empty;
		if (!string.IsNullOrEmpty(intervalAttack.GetBodyPart()))
		{
			text2 = intervalAttack.GetBodyPart();
		}
		else if (victimEdge != null)
		{
			text2 = victimEdge.GetBodyPart();
		}
		LastStrike.IsHeadHit = text2 == text;
		LastStrike.IsShock = ShouldCauseShock(LastStrike, attacker);
		LastStrike.IsDisarm = LastStrike.IsShock;
		LastStrike.IsFirstStrike = HitCounter == 0;
		strikePending = true;
		HitCounter++;
		RuleAppliance appliance = ((!IsPlayerModel()) ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		intervalAttack.UpdateFactor(appliance);
		EventData.Opponent = attacker;
		if (!LastStrike.IsBlocked)
		{
			Parameters.IsUntouched = false;
		}
		LastHitBlocked = LastStrike.IsBlocked;
		LastHitCritical = LastStrike.IsCritical;
		EventData.Data = intervalAttack;
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().OnModelHit(EventData);
		}
		EventData.Opponent = GetCombatTarget();
		RecordStrikeStatistics(attacker, LastStrike.AttackAnimation, LastStrike.FinalDamage, LastStrike);
	}

	public void StrikePhysics(List<string> physicsNames, StrikeResult strikeResult)
	{
		_Animation.DeleteAnimation();
		_Physics.Start(physicsNames);
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().OnModelPhysicsStart(EventData);
		}
		ApplyStrike(strikeResult);
	}

	public void StrikePhysics(List<string> physicsNames, bool useLastStrike)
	{
		StrikeResult strikeResult = null;
		if (useLastStrike)
		{
			strikeResult = LastStrike;
		}
		StrikePhysics(physicsNames, strikeResult);
	}

	public void ApplyStrike(StrikeResult strikeResult)
	{
		if (strikeResult != null)
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
				InfoAnimation.AlignObjectType pivotObjectType = item.MoveData.AlignData.PivotObjectType;
				if (pivotObjectType == InfoAnimation.AlignObjectType.ObjectNodes)
				{
					ModelObject pivotObject = GetModelObjectByType(item.MoveData.AlignData.PivotModelType);
					int num = pivotObject.GetNodeIDByName(item.MoveData.AlignData.PivotPart);
					if (num == -1)
					{
						GameLog.Warning("'Pivot' node '{0}' not found for '{1}' animation", item.MoveData.AlignData.PivotPart, item.Name);
					}
					item.MoveData.AlignData.PivotNodeId = num;
					item.MoveData.AlignData.PivotPairNodeId = pivotObject.GetNodeIDByPairName(item.MoveData.AlignData.PivotNodeId);
				}
			}
			if (item.MoveData.AlignData.PositionNodeId != -1 && item.MoveData.AlignData.PositionModelType != ModelType.ModelTargetType.MODEL_OTHER)
			{
				continue;
			}
			InfoAnimation.AlignObjectType positionObjectType = item.MoveData.AlignData.PositionObjectType;
			if (positionObjectType == InfoAnimation.AlignObjectType.ObjectNodes)
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

	public KeyData GetKeyDataBySign(int sign)
	{
		return controller.GetKeyDataBySign(sign);
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

	public float GetDamageBlockCritical(bool isActive, string attributeName, float Base)
	{
		if (isActive)
		{
			int attributeValue = 0;
			Parameters.FinalAttributes.Get(attributeName, ref attributeValue);
			return Mathf.Pow(2f, (float)attributeValue * Base);
		}
		return 1f;
	}

	public float GetBlock(bool isBlocked)
	{
		GameUtils.BaseSettigs settings = GameUtils.GetBlockDamageFactor();
		return GetDamageBlockCritical(isBlocked, settings.Attribute, settings.Base);
	}

	public float GetCritical(bool isCritical)
	{
		GameUtils.BaseSettigs settings = GameUtils.GetCriticalHitDamage();
		return GetDamageBlockCritical(isCritical, settings.Attribute, settings.Base);
	}

	public float GetTotalDamage(IntervalAttack attackInterval,
        bool blocked, // best guess for name
        bool isCritical, ModelEdge victimEdge)
	{
		Model attacker = GetCombatTarget();
		if (attacker == null)
		{
			Debug.LogError("attacker is null");
		}
		if (attacker.IsPlayerModel() && IsPlayerModel())
		{
			Debug.LogError("Both is player! Wat!?");
		}
		List<global::Pair<string, float>> list = attackInterval.GetDamageAttributes();
		foreach (global::Pair<string, float> item in list)
		{
			if (item.First == "RaidChargeDamage")
			{
				int raidChargeDamage = 0; // best guess for name
				attacker.Parameters.FinalAttributes.Get(item.First, ref raidChargeDamage);
				return Eclipse.Multiplayer.LocalVersusMatch.ScaleStrikeDamage(Fight.GetCurrentFight(), raidChargeDamage, blocked, attacker, attackInterval, this);
			}
		}
		string defenseAttribute = GetDefenseAttribute(attackInterval, blocked, victimEdge);
		float num = GameUtils.GetDamageFactorBase();
		string attributeName = GameUtils.GetDamageFactorAttribute();
		int OEMALIFPGPO2 = 0;
		attacker.Parameters.FinalAttributes.Get(attributeName, ref OEMALIFPGPO2);
		OEMALIFPGPO2 = Mathf.Min(OEMALIFPGPO2, (int)GameUtils.GetDamageFactorMaxValue());
		float num2 = Mathf.Pow(2f, num * (float)OEMALIFPGPO2);
		float num3 = GetBlock(blocked);
		float num4 = attacker.GetCritical(isCritical);
		float num5 = 0f;
		float num6 = GameUtils.GetAttributesHitMultiplier(attacker.IsPlayerModel(), attacker.Parameters, Parameters, list, defenseAttribute);
		float num7 = attackInterval.GetDamage();
		float num8 = attacker.GetAdditionalDamage();
		float a = (num7 + num8) * num6 * num3 * num4 * num2;
		a = Mathf.Max(a, 0f);
		RuleAppliance appliance = (attacker.IsPlayerModel() ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent);
		a *= attackInterval.GetFactors(appliance).Factor;
		a *= attacker.GetDamageMultiplier();
		a *= attacker.GetPowerMultiplier();
		if (a < 0f || 100000f < a)
		{
			Debug.LogError("Model::getTotalDamage - wtf so strong");
		}
		// Scale before ResolveStrikeDamage caps lethal hits to remaining health.
		return Eclipse.Multiplayer.LocalVersusMatch.ScaleStrikeDamage(Fight.GetCurrentFight(), a, blocked, attacker, attackInterval, this);
	}

	public IntervalAnimation GetBlockInterval()
	{
		return _Animation.FindInterval(IntervalAnimation.IntervalType.INTERVAL_BLOCK);
	}

	public bool IsBlocking()
	{
		return GetBlockInterval() != null;
	}

	public virtual void AttachToParent(Model parent = null)
	{
		parentModel = parent;
		combatTarget = ((parent == null) ? null : parent.GetCombatTarget());
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
		if (parent != null)
		{
			RoundStage = parent.RoundStage;
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

	public Model GetModelByType(ModelType.ModelTargetType targetType)
	{
		switch (targetType)
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
			GameLog.Error("Model::getModelByType ERROR - wrong model type: {0}", targetType);
			return null;
		}
	}

	// best guess for name
	public Model GetModelByRole(string role)
	{
		return GetModelByType(ModelType.ParseTargetType(role));
	}

	public int GetMaxAttributeValue(List<global::Pair<string, int>> attributeBonuses)
	{
		int num = 0;
		foreach (global::Pair<string, int> item in attributeBonuses)
		{
			int attributeValue = 0;
			if (Parameters.FinalAttributes.Get(item.First, ref attributeValue))
			{
				attributeValue += item.Second;
				if (num < attributeValue)
				{
					num = attributeValue;
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

	public void NotifyRangedAttack(WeaponModel weaponModel)
	{
		Model opponent = GetCombatTarget();
		if (opponent != null)
		{
			Model fGCODGKLHED2 = opponent.GetRootModel();
			if (fGCODGKLHED2.IsAiControlled())
			{
				fGCODGKLHED2.ai.StartRangedEnemy();
			}
		}
	}

	public void RemoveWeaponModel(WeaponModel weaponModel)
	{
		weaponModels.Remove(weaponModel);
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

	public bool SetPain(float painAmount)
	{
		pain += painAmount;
		if (!_ModelObject.IsShock() && pain > GameUtils.ShockSettings.Threshold)
		{
			return true;
		}
		return false;
	}

	public void OnStyleChanged(int rank, string styleName, float progress, bool updateDelta = false)
	{
		if (updateDelta)
		{
			if (StyleRank == rank)
			{
				StyleProgressDelta = progress - StyleProgress;
			}
			else
			{
				StyleProgressDelta = 1f - StyleProgress + progress + (float)(rank - StyleRank - 1);
			}
		}
		StyleRank = rank;
		StyleName = styleName;
		StyleProgress = progress;
	}

	public void OnComboChanged(object data)
	{
		CallEvent(13, this);
	}

	public bool HasDisarmedItem()
	{
		return disarmedItem != null;
	}

	public void SuppressInterval(IntervalAnimation.IntervalType intervalType)
	{
		if (_Animation != null)
		{
			_Animation.AddIntervalTypeFilter(intervalType);
		}
	}

	public void ClearSuppressedIntervals()
	{
		if (_Animation != null)
		{
			_Animation.ClearIntervalTypeFilter();
		}
	}

	public void ExecuteActionOnTarget(ActionAnimation action)
	{
		Model targetModel = GetModelByType(action.GetTargetPlayer());
		if (targetModel != null)
		{
			targetModel.StartAction(action);
		}
	}

	public void StartAction(ActionAnimation action)
	{
		GameLog.Error("Model::startAction - unknown action: {0}", action.get_Type());
	}

	public void StartAction(ActionCreateModel action)
	{
		SpawnWeaponModel(action.GetCopyItems(), action.GetModelName(), action.StartAnimation,
            action.EclipseProjectileOwner, action.EclipseProjectileLifetime);
	}

	public void StartAction(ActionDelete action)
	{
		Model targetModel = GetModelByType(action.GetTargetPlayer());
		NotifyAnimationEnded();
		CallEvent(5, targetModel);
	}

	public void StartAction(ActionSound action)
	{
		if (action.SameGender(Parameters.EclipseVoice))
		{
			Sound.PlaySound(action.get_Name(), action.GetIsLooped(), action.GetVolume());
		}
	}

	public void StartAction(ActionStopSound action)
	{
		Sound.StopSound(action.get_Name());
	}

	public void StartAction(ActionRandomSound action)
	{
		if (action.SameGender(Parameters.EclipseVoice))
		{
			Sound.PlaySound(action.get_Name());
		}
	}

	public void StartAction(ActionEffect action)
	{
		action.set_Model(this);
		CallEvent(7, action);
		action.set_Model(null);
	}

	public void StartAction(ActionStopEffect action)
	{
		action.set_Model(this);
		CallEvent(8, action);
		action.set_Model(null);
	}

	public void StartAction(ActionAddBullets action)
	{
		switch (action.GetBulletType())
		{
		case BulletType.MAGIC_BULLET:
		{
			int fOIPKLDNGDL2 = action.GetValue();
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
			int raidBulletCount = action.GetValue();
			AddRaidBullets(raidBulletCount);
			UpdateRaidChargeButton();
			break;
		}
		default:
			Debug.LogError("ERROR: Unknown bulletType");
			break;
		}
	}

	public void StartAction(ActionStopFollowEffect action)
	{
		action.set_Model(this);
		CallEvent(9, action);
		action.set_Model(null);
	}

	public void StartAction(ActionTryOnEnd action)
	{
		CallEvent(14, null);
	}

	public void StartAction(ActionShakeScreen action)
	{
		CallEvent(15, action);
	}

	public void StartAction(ActionHitEffect action)
	{
		if (hitData.DataReady && Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().CreateHitEffect(hitData.Point, hitData.Velocity, hitData.Time, action.GetFileName(), hitEffectScale);
		}
	}

	public void StartAction(ActionZoomEffect action)
	{
		CallEvent(17, action);
	}

	public void StartAction(ActionSetCooldown action)
	{
		int cooldown = action.GetDuration();
		string buttonName = action.GetButtonName();
		FightCID button = (FightCID)MovesMaps.GetMappedIndex(MovesMaps.MapType.KEY_TYPE, buttonName);
		ResetButtonCooldown(button, 0);
		bool flag = true;
		if (button == FightCID.RaidChargeButton)
		{
			flag = false;
			RaidModelParameters raidParameters = Parameters as RaidModelParameters;
			if (raidParameters != null && raidParameters.RaidChargeItem != null)
			{
				NoAnimationMove.RaidMoveInfo raidMoveInfo = QuestUtils.GetNoAnimationMoves().GetMoveByName(raidParameters.RaidChargeItem.Name);
				if (raidMoveInfo != null)
				{
					cooldown = (int)raidMoveInfo.ChargeCost;
					flag = raidBullets > 0;
				}
			}
		}
		if (flag)
		{
			StartButtonCooldown(button, cooldown);
			if (!SystemProperties.IsDebug())
			{
			}
		}
		else
		{
			ResetButtonCooldown(button, 30);
		}
	}

	public void StartAction(ActionSetEndStage action)
	{
		Model target = GetModelByType(action.GetTargetPlayer());
		if (target == null)
			target = this;
		target.EndStageReached = true;
	}

	public void StartAction(ActionPlayAnimation action)
	{
		Model target = null;
		if (!string.IsNullOrEmpty(action.ChildName))
		{
			foreach (WeaponModel child in GetWeaponModels())
			{
				if (child.get_Name() == action.ChildName)
				{
					target = child;
					break;
				}
			}
		}
		if (target == null)
			target = GetModelByType(action.GetTargetPlayer());
		if (target != null)
		{
			bool paired = action.GetTargetPlayer() == ModelType.ModelTargetType.MODEL_OTHER && string.IsNullOrEmpty(action.ChildName);
			bool started = target.PlayAnimation(action.AnimationName);
			if (paired)
			{
				// A throw pulls the enemy into its paired animation, then strikes by collision.
				// If the enemy refused (in physics, inactive or without that move), the throw
				// still swept through them and dealt damage without a grab ("bluetooth throw").
				_PairedGrab = GetCurrentAnimation();
				_PairedGrabVictim = target;
				_PairedGrabAnimation = action.AnimationName;
				_PairedGrabRefused = !started;
				if (!started)
				{
					Debug.LogWarning("[Throw] " + get_Name() + " '" + (_PairedGrab == null ? "?" : _PairedGrab.Name) +
						"': enemy refused paired animation '" + action.AnimationName + "' (physics=" +
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
	public WeaponModel SpawnWeaponModel(List<CopyItemInfo> copyItems = null, string modelName = "", string startAnimation = "", string projectileOwner = null,
        int lifetimeFrames = Eclipse.Modding.ModProjectileLimits.DefaultLifetimeFrames)
	{
        var ownedFight = string.IsNullOrEmpty(projectileOwner) ? null : Fight.GetCurrentFight();
        if (!string.IsNullOrEmpty(projectileOwner) && (ownedFight == null ||
            !ownedFight.CanSpawnEclipseProjectile(this, projectileOwner, lifetimeFrames))) return null;
		if (copyItems == null)
		{
			copyItems = new List<CopyItemInfo>();
		}
		ModelParameters childParameters = CreateChildParameters(copyItems);
		childParameters.IsPlayer = IsPlayerModel();
		WeaponModel weaponModel = new WeaponModel(childParameters);
		weaponModel.set_Name(modelName);
		weaponModel.SetExplicitBirthAnimation(startAnimation);
		weaponModel.SetDamageImmune(damageImmune);
		weaponModel.Parameters.SceneType = SceneTypes.SceneFight;
		weaponModel.AttachToParent(this);
		weaponModel.AddEnemy(GetCombatTarget());
		weaponModel.SetImpulseFactor(impulseFactor);
		weaponModel.SetPowerMultiplier(GetPowerMultiplier());
		weaponModels.Add(weaponModel);
        ownedFight?.RegisterEclipseProjectile(this, weaponModel, projectileOwner, lifetimeFrames);
		CallEvent(6, weaponModel);
		return weaponModel;
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

	public bool IsControlKeys(int key)
	{
		if (Parameters.ControlKeys.Count == 0)
		{
			return true;
		}
		foreach (int item in Parameters.ControlKeys)
		{
			int num = item;
			if (num == key)
			{
				return true;
			}
		}
		return false;
	}

	public void AddCurrentEffect(CurrentEffect effect)
	{
		if (!currentEffects.Contains(effect))
		{
			currentEffects.Add(effect);
		}
	}

	public void RemoveCurrentEffect(CurrentEffect effect)
	{
		currentEffects.Remove(effect);
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
		InfoAnimation animation = GetCurrentAnimation();
		if (animation != null)
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
		InfoAnimation animation = GetCurrentAnimation();
		if (animation != null)
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
			InfoAnimation animation = GetCurrentAnimation();
			if (animation == null)
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

	public void UpdateMagicCharge(float damage, Model opponent, bool isBlocked, bool isCritical, bool isDamageRecharge)
	{
		Model parent = GetParentModel();
		if (parent != null)
		{
			parent.UpdateMagicCharge(damage, opponent, isBlocked, isCritical, isDamageRecharge);
		}
		else if (magicCharges == 0)
		{
			opponent = opponent.GetRootModel();
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			if (isDamageRecharge)
			{
				num = GameUtils.MagicConfig.GetDamageRecharge(this);
				num2 = opponent.GetBlock(isBlocked);
				num3 = GetCritical(isCritical);
			}
			else
			{
				num = GameUtils.MagicConfig.GetPainRecharge(this);
				num2 = GetBlock(isBlocked);
				num3 = opponent.GetCritical(isCritical);
			}
			float chargeFraction = Mathf.Pow(2f, num) * num2 * num3 * damage;
			AddMagicChargeFraction(chargeFraction);
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

	public void NotifyPerkAction(PerksStage.ActionPerk perkAction, bool isActive)
	{
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().UpdatePerkIcon(this, perkAction, isActive);
		}
	}

	public void NotifyPerkActionReplaced(PerksStage.ActionPerk oldAction, PerksStage.ActionPerk newAction)
	{
		if (Fight.GetCurrentFight() != null)
		{
			Fight.GetCurrentFight().ReplacePerkIcon(this, oldAction, newAction);
		}
	}

	public void RemoveInterval(IntervalAnimation.IntervalType intervalType)
	{
		_Animation.RemoveInterval(intervalType);
	}

	public void RemoveInterval(string name)
	{
		_Animation.RemoveInterval(name);
	}

	public void RemoveIntervals(List<string> intervalNames)
	{
		_Animation.RemoveIntervals(intervalNames);
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

	public void NextRound(int round)
	{
		set_Round(round);
		SetSlowMotionAllowed(true);
		SetDecisionDelay(-1);
		SetAiDecisionReady(false);
		modelStats.ApplyRoundFactor();
	}

	public void SetLife(float life)
	{
		Parameters.SetCurrentLife(life);
	}

	public float GetLife()
	{
		return (ObscuredFloat)(Parameters.GetCurrentLife());
	}

	public void ChangeLife(float delta)
	{
		Parameters.ChangeLife(delta);
	}

	public bool IsAlive()
	{
		return !Parameters.GetLifeDepleted();
	}

	public static string GetDefenseAttribute(IntervalAttack intervalAttack, bool isBlocked, ModelEdge victimEdge)
	{
		List<string> list = intervalAttack.GetDefenseTypes();
		if (0 < list.Count)
		{
			return list[0];
		}
		if (isBlocked)
		{
			return GameUtils.GetBlockDefenseAttribute();
		}
		if (victimEdge != null && !string.IsNullOrEmpty(victimEdge.GetDefense()))
		{
			return victimEdge.GetDefense();
		}
		return GameUtils.GetSlowMotionDefense();
	}

	public void UpdateAnimationParameters(List<Model> models)
	{
		ModelObject modelObject = _ModelObject;
		bool isPlayer = IsPlayerModel();
		bool hasParent = GetParentModel() != null;
		List<InfoAnimation> animationList = GetAvailableAnimations();
		List<Trigger> triggerList = GetTriggers();
		foreach (Model item in models)
		{
			item.ApplyAnimationParameters(animationList, modelObject, isPlayer, hasParent, triggerList);
		}
	}

	public void ApplyAnimationParameters(List<InfoAnimation> animationList, ModelObject sourceObject, bool isPlayer, bool hasParent, List<Trigger> triggerList)
	{
		List<InfoAnimation> list = GetAvailableAnimations();
		foreach (InfoAnimation item in list)
		{
			item.UpdateModelObjects(sourceObject, isPlayer, hasParent, sourceObject);
		}
		List<Trigger> list2 = GetTriggers();
		foreach (Trigger item2 in list2)
		{
			item2.UpdateForObject(sourceObject, isPlayer, hasParent, sourceObject);
		}
		ModelObject ownObject = _ModelObject;
		bool ownIsPlayer = IsPlayerModel();
		bool ownHasParent = GetParentModel() != null;
		foreach (InfoAnimation item3 in animationList)
		{
			item3.UpdateModelObjects(ownObject, ownIsPlayer, ownHasParent, ownObject);
		}
		foreach (Trigger item4 in triggerList)
		{
			item4.UpdateForObject(ownObject, ownIsPlayer, ownHasParent, ownObject);
		}
	}

	public void LogDamage(float damage, string source, string target)
	{
		GetDamageLog().Add(damage, source, target);
	}

	public void RunActions(List<ActionAnimation> actions)
	{
		foreach (ActionAnimation item in actions)
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

	public void SetHitData(Vector3f point, Vector3f velocity, float time)
	{
		hitData.Point = point;
		hitData.Velocity = velocity;
		hitData.Time = time;
		hitData.DataReady = true;
	}

	public void ResetHitData()
	{
		hitData.DataReady = false;
	}

	public void ResetButtonCooldown(FightCID button, int frames)
	{
		switch (button)
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
			EventActBtnSettings buttonSettings = new EventActBtnSettings(FightCID.RaidChargeButton, 0f, frames);
			CallEvent(12, buttonSettings);
			break;
		}
		case FightCID.MagicButton:
			break;
		}
	}

	public void StartButtonCooldown(FightCID button, int frames)
	{
		if (frames <= 0)
		{
			frames = 1;
		}
		switch (button)
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

	public void SetImpulseFactor(Vector3f factor)
	{
		SetImpulseFactor(factor.GetX(), factor.GetY(), factor.GetZ());
	}

	public void SetImpulseFactor(float factorX, float factorY, float factorZ)
	{
		impulseFactor.Set(factorX, factorY, factorZ);
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

	public void AddRaidBullets(int bulletCount)
	{
		if (bulletCount < 0)
		{
			raidChargesUsed++;
		}
		raidBullets += bulletCount;
	}

	public void UpdateRaidChargeButton()
	{
		Model opponent = GetParentModel();
		if (opponent == null)
		{
			if (IsPlayerModel())
			{
				if (GetRaidBullets() == 0)
				{
					EventActBtnSettings buttonSettings = new EventActBtnSettings(FightCID.RaidChargeButton, 0f, 0);
					CallEvent(12, buttonSettings);
				}
				EventActBtnSettings eHCLMBADLKH2 = new EventActBtnSettings(FightCID.RaidChargeButton, -1f, -1, GetRaidBullets());
				CallEvent(18, eHCLMBADLKH2);
			}
		}
		else
		{
			opponent.UpdateMagicButton();
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
		List<PerkInfoItem> ownPerks = Parameters.GetAllPerks();
		List<PerkInfoItem> opponentPerks = null;
		if (GetCombatTarget() != null && GetCombatTarget().Parameters != null)
		{
			opponentPerks = GetCombatTarget().Parameters.GetAllPerks();
		}
		List<ItemInfo> items = Parameters.GetEquippedItems();
		BuildAvailableAnimations(items, false, Parameters.ExcludedMoveNames, Parameters.SceneType, ownPerks, opponentPerks);
	}

	protected virtual void LoadTriggers(ModelParameters data)
	{
		List<PerkInfoItem> ownPerks = Parameters.GetAllPerks();
		// Equipped items and action-created items live in separate collections.
		// Newer move actions test the equipped Magic/Weapon subtype directly, so
		// using only OJIAKDDCGLB left shop previews with an empty condition context
		// and made shared templates choose unrelated fallback effects.
		List<ItemInfo> items = GetModelConditionItems();
		_ModelConditions.Items = items;
		List<PerkInfoItem> opponentPerks = null;
		if (GetCombatTarget() != null && GetCombatTarget().Parameters != null)
		{
			opponentPerks = GetCombatTarget().Parameters.GetAllPerks();
		}
		BuildTriggers(items, false, Parameters.SceneType, ownPerks, opponentPerks);
	}

	protected void UpdateWallShift()
	{
		if (_Physics.IsPhysics())
		{
			return;
		}
		float wallLeftX = wallBounds.LeftX;
		float wallRightX = wallBounds.RightX;
		float num = wallBounds.LeftX;
		float num2 = wallBounds.RightX;
		List<ModelNode> list = _ModelObject.GetAllNodes();
		for (int i = 0; i < list.Count; i++)
		{
			float num3 = list[i].GetStart().GetX();
			if (num3 < num && num3 < wallLeftX)
			{
				num = num3;
			}
			if (num2 < num3 && wallRightX < num3)
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

	protected void ApplyItemChange(ItemInfo item, bool trackDisarmed, bool reloadAnimations = true, bool applyItemAttributes = true)
	{
		if (item == null)
		{
			return;
		}
		ItemInfo currentItem = Parameters.GetItemByType(item.Type);
		if (currentItem == null || !(currentItem.Name != item.Name))
		{
			return;
		}
		disarmedItem = ((!trackDisarmed) ? null : currentItem);
		Parameters.SetItemByType(item.Type, item);
		string attributeName = GameUtils.ShockSettings.SetAttributeName;
		int attributeValue = 0;
		item.ItemAttributes.Get(attributeName, ref attributeValue);
		Parameters.FinalAttributes.Set(attributeName, (!applyItemAttributes) ? GameUtils.ShockSettings.SetAttributeValue : attributeValue);
		if (reloadAnimations)
		{
			List<ItemInfo> items = Parameters.GetEquippedItems();
			List<PerkInfoItem> ownPerks = Parameters.GetAllPerks();
			List<PerkInfoItem> opponentPerks = null;
			if (GetCombatTarget() != null && GetCombatTarget().Parameters != null)
			{
				opponentPerks = GetCombatTarget().Parameters.GetAllPerks();
			}
			ReloadAnimationsForItems(items, ownPerks, opponentPerks);
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
		ItemInfo originalWeapon = Parameters.Weapon;
		ItemInfo shockWeapon = ListSF.GetItems().GetItemByName(GameUtils.ShockSettings.WeaponName);
		if (shockWeapon == null)
		{
			return;
		}
		ApplyItemChange(shockWeapon, true, true, false);
		_ModelObject.SetShock(true);
		Parameters.DisableActivePerks();
		List<ModelNode> list = _ModelObject.GetAllNodes();
		foreach (ModelNode item in list)
		{
			if (item.IsShock())
			{
				float impulseX = GameUtils.ShockSettings.Impulse.GetX() / item.GetWeight();
				float impulseY = GameUtils.ShockSettings.Impulse.GetY() / item.GetWeight();
				float impulseZ = GameUtils.ShockSettings.Impulse.GetZ() / item.GetWeight();
				item.GetStart().Add(impulseX, impulseY, impulseZ);
			}
		}
		Model opponent = GetCombatTarget();
		if (opponent != null)
		{
			Model fGCODGKLHED2 = opponent.GetRootModel();
			fGCODGKLHED2.ai.SetWeaponEnemy(shockWeapon.EffectiveTacticSubtype);
		}
		ai.SetWeaponBot(shockWeapon.EffectiveTacticSubtype);
		DisarmData disarmData = new DisarmData(this, originalWeapon.InnatePerks);
		CallEvent(16, disarmData);
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

	protected ModelParameters CreateChildParameters(List<CopyItemInfo> copyItems = null)
	{
		if (copyItems == null)
		{
			copyItems = new List<CopyItemInfo>();
		}
		RaidModelParameters raidParameters = new RaidModelParameters();
		raidParameters.SetLevel((ObscuredInt)(1));
		raidParameters.Dan = 1;
		raidParameters.Damage = 1f;
		raidParameters.Difficulty = 1f;
		raidParameters.FirstName = string.Empty;
		raidParameters.LastName = string.Empty;
		raidParameters.Avatar = GameUtils.GetDefaultAvatar();
		raidParameters.AiControlled = false;
		raidParameters.AnimationEnabled = true;
		raidParameters.IsPlayer = false;
		raidParameters.UserControlled = false;
		raidParameters.IsWinner = false;
		raidParameters.RoundEnded = false;
		raidParameters.IsDead = false;
		raidParameters.RoundsWon = 0;
		raidParameters.MaxLife = 0f;
		raidParameters.Skeleton = null;
		raidParameters.Armor = null;
		raidParameters.Helm = null;
		raidParameters.Weapon = null;
		raidParameters.Ranged = null;
		raidParameters.Magic = null;
		raidParameters.RaidChargeItem = null;
		if (copyItems.Count != 0)
		{
			int i = 0;
			for (int count = copyItems.Count; i < count; i++)
			{
				ApplyCopyItem(copyItems[i], raidParameters);
			}
		}
		raidParameters.BuildModelDocuments();
		raidParameters.CalculateAttributes();
		return raidParameters;
	}

	protected ModelObject GetModelObjectByType(ModelType.ModelTargetType targetType)
	{
		ModelObject result = null;
		switch (targetType)
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
		Model opponent = GetCombatTarget();
		if (Fight.GetCurrentFight().GetFightType() != BattleType.FightRaid || !opponent.IsPlayerModel() || opponent.modelStats.CheckCritAvailable())
		{
			return GameUtils.CriticalHitDefaults.GetProbability(opponent);
		}
		return 0f;
	}

	protected void ApplyAttributeLifeRegen()
	{
		Fight fight = Fight.GetCurrentFight();
		if (fight == null)
		{
			return;
		}
		int attributeValue = 0;
		if (Parameters.FinalAttributes.Get(GameUtils.GetRegeneration().Attribute, ref attributeValue) && GetCombatTarget() != null)
		{
			float num = (float)attributeValue * GameUtils.GetRegeneration().Base * GetCombatTarget().GetPowerMultiplier();
			if (num != 0f && (num > 0f || !damageImmune))
			{
				fight.UpdateLife(this, num);
			}
		}
	}

	protected void OnAnimationActions(object data)
	{
		List<ActionAnimation> actions = (List<ActionAnimation>)data;
		RunActions(actions);
	}

	protected void BuildAvailableAnimations(List<ItemInfo> items, bool includeAll, List<string> excludedMoveNames = null, SceneTypes sceneType = SceneTypes.SceneFight, List<PerkInfoItem> ownPerks = null, List<PerkInfoItem> opponentPerks = null)
	{
		AnimationData.CollectAvailableAnimations(availableAnimations, items, includeAll, Parameters.ExcludedMoveNames, sceneType, ownPerks, opponentPerks);
		PreloadAnimationAssets();
	}

	protected void BuildTriggers(List<ItemInfo> items, bool includeAll, SceneTypes sceneType = SceneTypes.SceneFight, List<PerkInfoItem> ownPerks = null, List<PerkInfoItem> opponentPerks = null)
	{
		AnimationData.CollectAvailableTriggers(triggers, items, includeAll, sceneType, ownPerks, opponentPerks);
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

	protected void ApplyCopyItem(CopyItemInfo item, ModelParameters parameters)
	{
		ItemInfo copiedItem = null;
		if (!string.IsNullOrEmpty(item.Name))
		{
			copiedItem = ListSF.GetItems().GetItemByName(item.Name).Clone();
		}
		else if (!string.IsNullOrEmpty(item.CopyParentType) && Parameters.GetItemByType(item.CopyParentType) != null)
		{
			copiedItem = Parameters.GetItemByType(item.CopyParentType).Clone();
		}
		if (copiedItem != null)
		{
			copiedItem.MergeWithItem(item);
			parameters.SetItemByType(copiedItem.Type, copiedItem);
			ListSF.GetInstance().OnItemGiven(copiedItem);
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

	private void RecordStrikeStatistics(Model opponent, InfoAnimation animation, float damage, StrikeResult strikeResult)
	{
		Model rootModel = GetRootModel();
		Model fGCODGKLHED2 = opponent.GetRootModel();
		rootModel.modelStats.RecordDamage(true, animation, damage);
		fGCODGKLHED2.modelStats.RecordDamage(false, animation, damage);
		fGCODGKLHED2.modelStats.AddRaidHitInfo(LastStrike.IsBlocked, LastStrike.IsCritical);
	}

	private bool ShouldCauseShock(StrikeResult strikeResult, Model attacker)
	{
		if (_IsShock)
		{
			return false;
		}
		float num = LastStrike.FinalDamage / attacker.GetPowerMultiplier();
		bool flag = SetPain((!damageImmune) ? num : 0f);
		ModelParameters attackerParameters = attacker.Parameters;
		float criticalChanceBase = GameUtils.ShockSettings.CriticalHitChanceBase;
		string criticalChanceAttribute = GameUtils.ShockSettings.CriticalHitChanceAttribute;
		int attributeValue = 0;
		attackerParameters.FinalAttributes.Get(criticalChanceAttribute, ref attributeValue);
		float num2 = criticalChanceBase * (float)attributeValue;
		float headHitChanceBase = GameUtils.ShockSettings.HeadHitChanceBase;
		string headHitChanceAttribute = GameUtils.ShockSettings.HeadHitChanceAttribute;
		int OEMALIFPGPO2 = 0;
		attackerParameters.FinalAttributes.Get(headHitChanceAttribute, ref OEMALIFPGPO2);
		float num3 = headHitChanceBase * (float)OEMALIFPGPO2;
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
			EventActBtnSettings buttonSettings = new EventActBtnSettings(FightCID.Punch, buttonCooldowns.PunchProgress, 1);
			CallEvent(12, buttonSettings);
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
