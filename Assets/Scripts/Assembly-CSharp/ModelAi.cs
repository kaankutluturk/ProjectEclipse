using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class ModelAi
{
	public enum WaitMode
	{
		SetWaitNone = 0,
		SetWaitRandAttack = 1,
		SetWaitRandUnint = 2,
		SetWaitAnimationLength = 3
	}

	public enum AiState
	{
		Nostate = 0,
		Move = 1,
		ConterAttack = 2
	}

	public enum AnimationListType
	{
		Standard = 0,
		Missile = 1
	}

	private enum DefenceMode
	{
		DefenceUseNone = 0,
		DefenceUseRandom = 1,
		DefenceUseCounterAttack = 2,
		DefenceUseDodge = 3,
		DefenceUseBlock = 4
	}

	private enum MissileKind
	{
		SimpleMissile = 0,
		MagicMissile = 1
	}

	private enum AttackMode
	{
		AttackUseRandom = 0,
		AttackUseTableAttack = 1
	}

	private class Decision
	{
		public InfoAnimation Animation;

		public int Wait;

		public bool IsAnimationSet
		{
			get
			{
				return HasAnimation();
			}
		}

		public bool IsWaitSet
		{
			get
			{
				return HasWait();
			}
		}

		public Decision()
		{
			Animation = null;
			Wait = 0;
		}

		public Decision(InfoAnimation animation, int wait)
		{
			Animation = animation;
			Wait = wait;
		}

		public bool HasAnimation()
		{
			return Animation != null;
		}

		public bool HasWait()
		{
			return Wait != -1;
		}
	}

	private class ChanceRoll
	{
		public float Chance;

		public float Roll;

		public bool Flag;
	}

	private WaitMode waitMode;

	private AiState aiState;

	private DefenceMode defenceMode;

	private AttackMode attackMode;

	private static bool aiOn = true;

	private bool unusedFlag;

	private int enemyFrame;

	private int ownFrame;

	private Model _Model;

    private int _modDecisionFrame = -1;
    private bool _modDecisionOwned;
    private string _modDecisionTactic;

	private List<int> _InterframesList = new List<int>();

	private ModelAnimation _ModelAnimation;

	private ModelPhysics _ModelPhysics;

	private ModelParameters parameters;

	private string botWeaponSubtype;

	private string enemyWeaponSubtype;

	private int decisionWait;

	private int responseDelay;

	private bool waitElapsed;

	private bool waitRequested;

	private InfoAnimation enemyAnimation;

	private InfoAnimation botAnimation;

	private float distanceError;

	private int frameError;

	private int unusedCounter;

	private AiData.TableType resultSource;

	private Tactic tactic;

	private bool dodgeMissile;

	private float dodgeMissileChance;

	private bool dodgeMagic;

	private float dodgeMagicChance;

	private bool offKeyFrame;

	private bool useSafeAttack;

	private bool useTableAttack;

	private List<ChanceRoll> quickAttackRolls = new List<ChanceRoll>();

	private List<ChanceRoll> evadeRolls = new List<ChanceRoll>();

	private bool useCautiousMovement;

	private float attackRoll;

	private float safeAttackRoll;

	private float tableAttackRoll;

	private float cautiousMovementRoll;

	private float dodgeMissileRoll;

	private float dodgeMagicRoll;

	private bool enemyCanEvade;

	private List<Decision> decisions = new List<Decision>();

	private List<InfoAnimation> candidateAnimations = new List<InfoAnimation>();

	private float counterAttackChance;

	private float dodgeChance;

	private float blockChance;

	private float safeAttackChance;

	private float tableAttackChance;

	private float cautiousMovementChance;

	public static bool AiEnabled
	{
		get
		{
			return get_AiOn();
		}
		set
		{
			set_AiOn(value);
		}
	}

	public Model OwnerModel
	{
		get
		{
			return get_Model();
		}
		set
		{
			set_Model(value);
		}
	}

	public AiData.TableType ResultTable
	{
		get
		{
			return get_ResultSource();
		}
	}

	public Tactic CurrentTactic
	{
		get
		{
			return get_Tactic();
		}
	}

	private bool IsAiActive
	{
		get
		{
			return get_IsEnabled();
		}
	}

	public ModelAi(ModelAnimation modelAnimation, ModelPhysics modelPhysics, string weaponSubtype, ModelParameters modelParameters)
	{
		_ModelAnimation = modelAnimation;
		_ModelPhysics = modelPhysics;
		enemyAnimation = null;
		botAnimation = null;
		botWeaponSubtype = AiData.GetItemEquivalent(weaponSubtype);
		distanceError = 0f;
		frameError = 0;
		unusedCounter = 0;
		parameters = modelParameters;
		resultSource = AiData.TableType.noneTable;
		unusedFlag = false;
		aiState = AiState.Nostate;
		waitRequested = false;
		responseDelay = 0;
		defenceMode = DefenceMode.DefenceUseRandom;
		attackMode = AttackMode.AttackUseRandom;
		enemyFrame = 0;
		ownFrame = 0;
		enemyCanEvade = false;
		decisionWait = 1;
		waitElapsed = false;
		waitMode = WaitMode.SetWaitNone;
		_Model = null;
		offKeyFrame = false;
		useSafeAttack = false;
		useTableAttack = false;
		useCautiousMovement = false;
		tactic = null;
		attackRoll = 0f;
		safeAttackRoll = 0f;
		tableAttackRoll = 0f;
		cautiousMovementRoll = 0f;
		LoadParameters();
	}

	public static bool get_AiOn()
	{
		return aiOn;
	}

	public static void set_AiOn(bool value)
	{
		aiOn = value;
	}

	public void set_Model(Model value)
	{
		_Model = value;
	}

	public Model get_Model()
	{
		return _Model;
	}

	public AiData.TableType get_ResultSource()
	{
		return resultSource;
	}

	public Tactic get_Tactic()
	{
		return tactic;
	}

	public void setAvailableAnimations(List<InfoAnimation> animations)
	{
	}

	public InfoAnimation Render(Model enemy, int frame)
	{
		if (!get_IsEnabled() || enemy == null || _ModelAnimation.GetCurrentInfo() == null)
		{
			return null;
		}
        // A newly created controller can join while its opponent is already
        // animating, before another animation-start notification arrives.
        if (enemyAnimation == null)
        {
            StartAnimationEnemy(enemy);
            if (enemyAnimation == null) return null;
        }
        // A controller joining an ongoing fight may miss its own animation-start
        // notification. Seed that observation in every fight mode; otherwise
        // IsFitIntervalAndMove refuses its decisions despite a running idle move.
        if (botAnimation == null && _ModelAnimation.GetIsPlaying() && Fight.GetCurrentFight() != null)
        {
            StartAnimationBot(_ModelAnimation.GetCurrentInfo());
        }
		ModelAnimation enemyModelAnimation = enemy.GetAnimationModule();
		TacticFactors factors = SetFactors(enemy);
		if (enemyModelAnimation.GetIsPlaying())
		{
			int num = enemyModelAnimation.GetFrameInMove();
			int num2 = enemyModelAnimation.GetStartFrameOffset();
			int num3 = GetFrameError(factors);
			enemyFrame = num + num2 + num3;
		}
		else
		{
			enemyFrame = -1;
		}
		if (_ModelAnimation.GetIsPlaying())
		{
			int num4 = _ModelAnimation.GetFrameInMove();
			int num5 = _ModelAnimation.GetStartFrameOffset();
			ownFrame = num4 + num5;
		}
		else
		{
			ownFrame = -1;
		}
		if (waitRequested)
		{
			waitRequested = false;
			if (_ModelAnimation.GetIsPlaying() && enemyModelAnimation.GetIsPlaying())
			{
				InfoAnimation animation = _ModelAnimation.GetCurrentInfo();
				InfoAnimation pJAHIOELGGD2 = enemyModelAnimation.GetCurrentInfo();
				if (animation != null && pJAHIOELGGD2 != null)
				{
					int num6 = animation.GetMoveLengthStrict();
					switch (waitMode)
					{
					case WaitMode.SetWaitRandAttack:
						decisionWait = pJAHIOELGGD2.GetLastAttackFrame(true) - enemyFrame + 1;
						decisionWait = Mathf.Min(decisionWait, animation.GetMoveLengthExtended());
						if (num6 > decisionWait)
						{
							decisionWait = num6;
						}
						decisionWait--;
						break;
					case WaitMode.SetWaitRandUnint:
						decisionWait = pJAHIOELGGD2.GetLastUninterruptFrame(true) - enemyFrame + 1;
						decisionWait = Mathf.Min(decisionWait, animation.GetMoveLengthExtended());
						if (num6 > decisionWait)
						{
							decisionWait = num6;
						}
						decisionWait--;
						break;
					case WaitMode.SetWaitAnimationLength:
						decisionWait = num6;
						decisionWait--;
						break;
					}
				}
			}
		}
		else
		{
			WaitMode mode = waitMode;
			if (mode == WaitMode.SetWaitRandAttack || mode == WaitMode.SetWaitRandUnint || mode == WaitMode.SetWaitAnimationLength)
			{
				GameLog.Error("!");
			}
		}
		waitMode = WaitMode.SetWaitNone;
		if (1 < decisionWait)
		{
			decisionWait--;
			return null;
		}
		InfoAnimation pJAHIOELGGD3 = enemyModelAnimation.GetCurrentInfo();
		if (pJAHIOELGGD3 != null)
		{
			bool flag = false;
			List<TemplateAnimation> list = AiData.get_RandomizingEnemyAnimation();
			foreach (TemplateAnimation item in list)
			{
				List<InfoAnimation> list2 = item.GetAnimations();
				if (list2.Contains(pJAHIOELGGD3))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				RandomizeBehavior(enemy);
			}
		}
		SetQuickAttackRnd();
		SetEvadesRnd();
		if (!IsFitIntervalAndMove())
		{
			return null;
		}
        string modTactic = get_Tactic()?.get_Name();
        if (Fight.GetCurrentFight()?.IsTitleSparring != true && Eclipse.Modding.ModRuntime.HasAiHandler(modTactic))
        {
            // The native argument is the controller's last key-input frame. It
            // can remain zero throughout an idle fight, so Lua decisions use
            // the fight's advancing simulation frame instead.
            int decisionFrame = Fight.GetCurrentFight()?.get_FightTimeInFrames() ?? 0;
            if (_modDecisionTactic != modTactic)
            {
                _modDecisionTactic = modTactic;
                _modDecisionFrame = -1;
                _modDecisionOwned = false;
            }
            // At most ten Lua decisions per active simulation second. A requested
            // wait retains control until the next decision; nil uses native AI.
            if (_modDecisionFrame >= 0 && decisionFrame >= _modDecisionFrame && decisionFrame - _modDecisionFrame < 6)
            {
                if (_modDecisionOwned) return null;
            }
            else
            {
                _modDecisionFrame = decisionFrame;
                var available = new List<InfoAnimation>(_Model.GetAvailableAnimations());
                if (GetPlayableAnimations(available) == 0) available.Clear();
                else available.RemoveAll(move => !IsTacticPlayableAnimations(move));
                int? chosen = Eclipse.Modding.ModRuntime.DecideAi(modTactic, this, _Model, enemy, decisionFrame, available);
                _modDecisionOwned = chosen.HasValue;
                if (chosen.HasValue) return chosen.Value >= 0 && chosen.Value < available.Count ? available[chosen.Value] : null;
            }
        }
		defenceMode = SelectDefenceMode(enemy);
		safeAttackChance = tactic.GetUseSafeAttackChance(factors);
		useSafeAttack = safeAttackRoll < safeAttackChance;
		tableAttackChance = tactic.GetTableAttackChance(factors);
		useTableAttack = tableAttackRoll < tableAttackChance;
		SetQuickAttackChances(factors);
		SetEvadesChances(factors);
		cautiousMovementChance = tactic.GetCautiousMovementsChance(factors);
		useCautiousMovement = cautiousMovementRoll < cautiousMovementChance;
		dodgeMissileChance = tactic.GetDodgeMissileChance(factors);
		dodgeMissile = dodgeMissileRoll < dodgeMissileChance;
		dodgeMagicChance = tactic.GetDodgeMagicChance(factors);
		dodgeMagic = dodgeMagicRoll < dodgeMagicChance;
		int num7 = SetDecisionList(enemy, frame);
		if (0 < num7)
		{
			waitElapsed = false;
			GetPlayableAnimations(decisions);
			DecisionsToAimationsList();
			int num8 = SelectAnimationWithWeights(candidateAnimations);
			if (-1 < num8)
			{
				decisionWait = _InterframesList[num8];
				InfoAnimation chosenAnimation = candidateAnimations[num8];
				LogTable(enemy.GetAnimationModule(), frame, num7, chosenAnimation, decisionWait);
				return candidateAnimations[num8];
			}
		}
		else if (waitRequested)
		{
			LogTable(enemy.GetAnimationModule(), frame - 1, 1, null, 0);
			waitElapsed = false;
		}
		return null;
	}

	public void RandomizeBehavior(Model enemy)
	{
		TacticFactors factors = new TacticFactors(_Model.GetModelStats(), _Model.GetNoRangedFlag(), _Model.GetMagicCharges());
		_Model.GetModelStats().GetCountAndDamage(true, enemyAnimation, ref factors.FactorsCount, ref factors.Damage, ref factors.Hits);
		factors.Health = (ObscuredFloat)(_Model.Parameters.GetCurrentLife());
		factors.EnemyHealth = (ObscuredFloat)(enemy.Parameters.GetCurrentLife());
		factors.AnimationFrames = enemy.GetAnimationModule().GetFrameInMove();
		factors.ChildFrames = ChildMaxModelFrame(enemy);
		attackRoll = NekkiMath.randomFloat();
		safeAttackRoll = NekkiMath.randomFloat();
		tableAttackRoll = NekkiMath.randomFloat();
		cautiousMovementRoll = NekkiMath.randomFloat();
		dodgeMissileRoll = NekkiMath.randomFloat();
		dodgeMagicRoll = NekkiMath.randomFloat();
		distanceError = GetDistanceError(factors);
		frameError = GetFrameError(factors);
	}

	private int ChildMaxModelFrame(Model model)
	{
		int num = 0;
		int i = 0;
		for (int count = model.GetWeaponModels().Count; i < count; i++)
		{
			WeaponModel weaponModel = model.GetWeaponModels()[i];
			if (weaponModel != null)
			{
				int num2 = weaponModel.GetAnimationModule().GetRenderTickCount();
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public void StartAnimationBot(InfoAnimation animation)
	{
		if (!get_IsEnabled())
		{
			return;
		}
		if (_ModelAnimation.GetIsPlaying() && animation != null)
		{
			InfoAnimation equivalentAnimation = animation.GetTacticEquivalent();
			if (equivalentAnimation != null)
			{
				botAnimation = equivalentAnimation;
			}
			else
			{
				botAnimation = animation;
			}
		}
		else
		{
			botAnimation = null;
		}
		if (IsFitStartAnimation(botAnimation))
		{
			decisionWait = 1;
		}
	}

	public void StartAnimationEnemy(Model enemy)
	{
		if (!get_IsEnabled())
		{
			return;
		}
		ModelAnimation enemyModelAnimation = enemy.GetAnimationModule();
		InfoAnimation animation = enemyModelAnimation.GetCurrentInfo();
		if (enemyModelAnimation.GetIsPlaying() && animation != null)
		{
			InfoAnimation pJAHIOELGGD2 = animation.GetTacticEquivalent();
			if (pJAHIOELGGD2 == null)
			{
				enemyAnimation = animation;
			}
			else
			{
				enemyAnimation = pJAHIOELGGD2;
			}
			InfoAnimation enemyMove = enemyAnimation;
			RandomizeBehavior(enemy);
			if (!IsIgnoredEnemyAnimation(enemyMove))
			{
				TacticFactors factors = new TacticFactors(_Model.GetModelStats(), _Model.GetNoRangedFlag(), _Model.GetMagicCharges());
				_Model.GetModelStats().GetCountAndDamage(true, enemyAnimation, ref factors.FactorsCount, ref factors.Damage, ref factors.Hits);
				factors.Health = (ObscuredFloat)(_Model.Parameters.GetCurrentLife());
				factors.EnemyHealth = (ObscuredFloat)(enemy.Parameters.GetCurrentLife());
				factors.AnimationFrames = enemy.GetAnimationModule().GetFrameInMove();
				factors.ChildFrames = ChildMaxModelFrame(enemy);
				responseDelay = GetResponseDelay(factors);
			}
		}
	}

	private bool IsIgnoredEnemyAnimation(InfoAnimation animation)
	{
		List<string> list = AiData.get_IgnoredEnemyAnimations();
		foreach (string item in list)
		{
			if (animation.HasName(item))
			{
				return true;
			}
		}
		return false;
	}

	public void StartRangedEnemy()
	{
		if (get_IsEnabled())
		{
			decisionWait = 1;
		}
	}

	public void SetWeaponEnemy(string weaponSubtype)
	{
		enemyWeaponSubtype = AiData.GetItemEquivalent(weaponSubtype);
	}

    internal System.Action CaptureEnemyWeapon()
    {
        var weapon = enemyWeaponSubtype;
        return () => enemyWeaponSubtype = weapon;
    }

    // Changing an actor's target invalidates observations and waits belonging to
    // the previous body. Seed the new observation on the next ordinary AI tick;
    // do not draw random values or invoke tactics inside a targeting transaction.
    internal System.Action ResetCombatTarget(string weaponSubtype)
    {
        var weapon = AiData.GetItemEquivalent(weaponSubtype);
        var oldWeapon = enemyWeaponSubtype;
        var observed = enemyAnimation;
        var response = responseDelay;
        var frame = enemyFrame;
        var wait = decisionWait;
        var deferred = waitRequested;
        var waitKind = waitMode;
        var decisionFrame = _modDecisionFrame;
        var decisionOwned = _modDecisionOwned;
        enemyWeaponSubtype = weapon;
        enemyAnimation = null;
        responseDelay = 0;
        enemyFrame = -1;
        decisionWait = 1;
        waitRequested = false;
        waitMode = WaitMode.SetWaitNone;
        _modDecisionFrame = -1;
        _modDecisionOwned = false;
        return () =>
        {
            enemyWeaponSubtype = oldWeapon; enemyAnimation = observed;
            responseDelay = response; enemyFrame = frame; decisionWait = wait;
            waitRequested = deferred; waitMode = waitKind;
            _modDecisionFrame = decisionFrame; _modDecisionOwned = decisionOwned;
        };
    }

	public void SetWeaponBot(string weaponSubtype)
	{
		botWeaponSubtype = AiData.GetItemEquivalent(weaponSubtype);
	}

	public void OnGetHit()
	{
		if (get_IsEnabled())
		{
		}
	}

	public void OnHitEnemy()
	{
		if (get_IsEnabled())
		{
		}
	}

	public int SelectAnimationWithWeights(List<InfoAnimation> animations)
	{
		Model combatTarget = _Model.GetCombatTarget();
		if (combatTarget != null)
		{
			Model enemyRoot = combatTarget.GetRootModel();
			TacticFactors factors = SetFactors(enemyRoot);
			return tactic.SelectAnimationWithWeights(animations, botAnimation, factors);
		}
		return -1;
	}

	public void ChangeTactic(string name)
	{
		Tactic newTactic = AiData.GetTacticByName(name);
		ChangeTactic(newTactic);
	}

	public void ChangeTactic(Tactic newTactic)
	{
		if (newTactic != null)
		{
			tactic = newTactic;
		}
	}

	private int GetResponseDelay(TacticFactors factors)
	{
		return tactic.GetResponseDelay(factors) + 1;
	}

	private float GetDistanceError(TacticFactors factors)
	{
		return tactic.GetDistanceError(factors);
	}

	private int GetFrameError(TacticFactors factors)
	{
		return tactic.GetFrameError(factors);
	}

	private int GetEnemyResponseDelay(TacticFactors factors)
	{
		return tactic.GetEnemyResponseDelay(factors);
	}

	private static bool GetRandomFlag(float chance)
	{
		float num = NekkiMath.randomFloat();
		return num < chance;
	}

	private void LoadParameters()
	{
		Tactic loadedTactic = parameters.FightTactic;
		if (loadedTactic != null)
		{
			tactic = loadedTactic;
		}
	}

	private void LogTable(ModelAnimation modelAnimation, int frame, int count, InfoAnimation animation, int wait)
	{
	}

	private void LogStart()
	{
	}

	private int GetNearestKeyFrameId(int frame)
	{
		if (frame % AiData.MovementsStep == 0)
		{
			return frame;
		}
		if (0 < frame)
		{
			return frame - frame % AiData.MovementsStep + AiData.MovementsStep;
		}
		return frame - frame % AiData.MovementsStep;
	}

	private bool IsFitStartAnimation(InfoAnimation animation)
	{
		if (_ModelAnimation.GetIsPlaying() && animation != null)
		{
			List<string> list = AiData.get_UnexpectedMoves();
			foreach (string item in list)
			{
				if (animation.HasName(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsFitBotAnimation(InfoAnimation animation, AnimationListType listType = AnimationListType.Standard)
	{
		if (animation != null)
		{
			List<string> list = null;
			switch (listType)
			{
			case AnimationListType.Standard:
				list = AiData.get_MovesLastIteration();
				break;
			case AnimationListType.Missile:
				list = AiData.get_MissilesLastIteration();
				break;
			}
			foreach (string item in list)
			{
				if (animation.HasName(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsSafeDodges(InfoAnimation animation)
	{
		List<string> list = AiData.get_SafeDodgesAnimations();
		foreach (string item in list)
		{
			if (animation.HasName(item))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsFitIntervalAndMove()
	{
		List<string> list = AiData.get_NoDecisionIntervals();
		foreach (string item in list)
		{
			IntervalAnimation interval = _ModelAnimation.FindInterval(item);
			if (interval != null)
			{
				return false;
			}
		}
		if (_ModelAnimation.GetIsPlaying() && botAnimation != null)
		{
			InfoAnimation botMove = botAnimation;
			List<string> list2 = AiData.get_NoDecisionMoves();
			foreach (string item2 in list2)
			{
				if (botMove.HasName(item2))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private bool IsMissileAnimation(InfoAnimation animation)
	{
		List<TemplateAnimation> templateAnimations = AiData.get_MissileAnimations();
		return IsGivenTemplateAnimation(animation, templateAnimations);
	}

	private bool IsMagicAnimation(InfoAnimation animation)
	{
		List<TemplateAnimation> templateAnimations = AiData.get_MagicAnimations();
		return IsGivenTemplateAnimation(animation, templateAnimations);
	}

	private bool IsGivenTemplateAnimation(InfoAnimation animation, List<TemplateAnimation> templateAnimations)
	{
		bool result = false;
		if (animation != null)
		{
			foreach (TemplateAnimation item in templateAnimations)
			{
				List<InfoAnimation> list = item.GetAnimations();
				if (list.Contains(animation))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private bool GetUseChildrenDodge(Model enemy, MissileKind missileKind)
	{
		if (enemy.GetWeaponModels().Count == 0 || _Model == null)
		{
			return false;
		}
		bool result = false;
		int num = GetModelDirection(_Model, enemy);
		foreach (WeaponModel item in enemy.GetWeaponModels())
		{
			if (item.GetAnimationModule() != null && (item.GetAnimationModule() == null || item.GetAnimationModule().GetFirstInfo() != null) && (IsMissileAnimation(item.GetAnimationModule().GetCurrentInfo()) || missileKind != MissileKind.SimpleMissile) && (IsMagicAnimation(item.GetAnimationModule().GetCurrentInfo()) || missileKind != MissileKind.MagicMissile))
			{
				float num2 = _Model.GetBodyObject().GetPivotNode().GetStart()
					.GetX();
				float num3 = item.GetBodyObject().GetAllNodes()[0].GetStart().GetX();
				int num4 = ((num2 - num3 < 0f) ? 1 : (-1));
				int num5 = item.GetAnimationModule().GetSign();
				if (num4 * num5 < 0)
				{
					result = true;
				}
				else if (Mathf.Abs(num3 - num2) < 100f)
				{
					result = true;
				}
			}
		}
		return result;
	}

	private DefenceMode SelectDefenceMode(Model enemy)
	{
		float num = NekkiMath.randomFloat();
		TacticFactors factors = SetFactors(enemy);
		counterAttackChance = tactic.GetCounterAttackChance(factors);
		dodgeChance = tactic.GetDodgeChance(factors);
		blockChance = tactic.GetBlockChance(factors);
		float cumulativeChance = counterAttackChance;
		if (num < counterAttackChance)
		{
			return DefenceMode.DefenceUseCounterAttack;
		}
		cumulativeChance += dodgeChance;
		if (num < cumulativeChance)
		{
			return DefenceMode.DefenceUseDodge;
		}
		cumulativeChance += blockChance;
		if (num < cumulativeChance)
		{
			return DefenceMode.DefenceUseBlock;
		}
		return DefenceMode.DefenceUseRandom;
	}

	private float GetNodeX(string nodeName, Model model, Model enemy)
	{
		int direction = GetModelDirection(model, enemy);
		ModelNode node = model.GetAnimationModule().GetNodeByNameForSign(nodeName, direction);
		if (node != null)
		{
			return node.GetStart().GetX();
		}
		return float.MaxValue;
	}

	private float GetNodeX(string nodeName, List<global::Pair<string, float>> nodePositions)
	{
		foreach (global::Pair<string, float> item in nodePositions)
		{
			if (item.First == nodeName)
			{
				return item.Second;
			}
		}
		return float.MaxValue;
	}

	private void SetRandomAnimation()
	{
		_Model.TriggerRandomAnimation(false);
		resultSource = AiData.TableType.randomAnimation;
		waitRequested = true;
		decisionWait = int.MinValue;
	}

	private int GetPlayableAnimations(List<InfoAnimation> animations, List<int> interframes = null, bool useTacticRules = false)
	{
		List<InfoAnimation> list = _Model.GetAvailableAnimations();
		int num = 0;
		int count = animations.Count;
		ModelConditions modelConditions = get_Model().GetConditions();
		if (modelConditions == null)
		{
			GameLog.Error("modelConditions is null");
			return 0;
		}
		modelConditions.IsKeyCheckEnabled = false;
		for (int i = 0; i < count; i++)
		{
			InfoAnimation animation = animations[i];
			if (animation != null && ((!useTacticRules) ? IsPlayableAnimations(animation) : IsTacticPlayableAnimations(animation)))
			{
				animations[num] = animations[i];
				if (interframes != null)
				{
					interframes[num] = interframes[i];
				}
				num++;
			}
		}
		animations.Resize(num);
		if (interframes != null)
		{
			interframes.Resize(num);
		}
		return num;
	}

	private int GetPlayableAnimations(List<Decision> decisionList)
	{
		List<InfoAnimation> list = _Model.GetAvailableAnimations();
		int num = 0;
		int count = decisionList.Count;
		ModelConditions modelConditions = get_Model().GetConditions();
		if (modelConditions == null)
		{
			GameLog.Error("modelConditions is null");
			return 0;
		}
		modelConditions.IsKeyCheckEnabled = false;
		for (int i = 0; i < count; i++)
		{
			InfoAnimation animation = decisionList[i].Animation;
			if (animation == null || IsPlayableAnimations(animation))
			{
				decisionList[num] = decisionList[i];
				num++;
			}
		}
		decisionList.Resize(num);
		return num;
	}

	private bool IsPlayableAnimations(InfoAnimation animation)
	{
		// AI executes its choices through key input. Event-only animations are
		// started by their runtime events and cannot be selected by this path.
		if (animation.GetFirstKeysCondition() == null) return false;
		List<InfoAnimation> list = _Model.GetAvailableAnimations();
		if (!list.Contains(animation))
		{
			return false;
		}
		ModelConditions modelConditions = get_Model().GetConditions();
		modelConditions.IsKeyCheckEnabled = false;
		modelConditions.CandidateMoveNames = animation.GetTemplateNames();
		modelConditions.AnimationSign = animation.GetDirection(modelConditions, _ModelAnimation.GetSign());
		modelConditions.PivotPairSelector = (int)animation.MoveData.AlignData.PivotSideKind;
		if (!animation.AreConditionsMet(get_Model(), null, animation.FindMoveEventByType(EventAnimation.EventAnimationType.EVENT_KEY_PRESSED)))
		{
			return false;
		}
		InfoAnimation.CapabilityTable capabilities = animation.PriorityConflicts;
		int count = capabilities.HigherPriorityMoves.Count;
		if (0 < count)
		{
			foreach (InfoAnimation item in capabilities.HigherPriorityMoves)
			{
				if (list.Contains(item) && item.GetFirstKeysCondition() != null)
				{
					modelConditions.CandidateMoveNames = item.GetTemplateNames();
					modelConditions.AnimationSign = item.GetDirection(modelConditions, _ModelAnimation.GetSign());
					modelConditions.PivotPairSelector = (int)item.MoveData.AlignData.PivotSideKind;
					if (item.AreConditionsMet(get_Model(), null, animation.FindMoveEventByType(EventAnimation.EventAnimationType.EVENT_KEY_PRESSED)))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool IsTacticPlayableAnimations(InfoAnimation animation)
	{
		if (animation.GetFirstKeysCondition() == null) return false;
		List<InfoAnimation> list = _Model.GetAvailableAnimations();
		if (!list.Contains(animation))
		{
			return false;
		}
		ModelConditions modelConditions = get_Model().GetConditions();
		modelConditions.IsKeyCheckEnabled = false;
		modelConditions.CandidateMoveNames = animation.GetTemplateNames();
		modelConditions.AnimationSign = animation.GetDirection(modelConditions, _ModelAnimation.GetSign());
		modelConditions.PivotPairSelector = (int)animation.MoveData.AlignData.PivotSideKind;
		if (!animation.AreConditionsMet(_Model.GetConditions(), animation.MoveData.TacticsConditions, animation.FindMoveEventByType(EventAnimation.EventAnimationType.EVENT_KEY_PRESSED)))
		{
			return false;
		}
		return true;
	}

	private float GetCurrentDirectionToEnemy(ModelAnimation modelAnimation)
	{
		ModelNode node = modelAnimation.GetPlayingNode();
		ModelNode lCDGOCIAIDK2 = _ModelAnimation.GetPlayingNode();
		if (node == null || lCDGOCIAIDK2 == null)
		{
			return 0f;
		}
		float num = node.GetStart().GetX() - lCDGOCIAIDK2.GetStart().GetX();
		if (num >= 0f)
		{
			return 1f;
		}
		return -1f;
	}

	private bool IsFitCondition(InfoAnimation animation, bool useTacticsConditions)
	{
		ModelConditions modelConditions = get_Model().GetConditions();
		if (modelConditions == null)
		{
			GameLog.Error("modelConditions is null");
			return false;
		}
		modelConditions.CandidateMoveNames = animation.GetTemplateNames();
		modelConditions.AnimationSign = animation.GetDirection(modelConditions, _ModelAnimation.GetSign());
		modelConditions.PivotPairSelector = (int)animation.MoveData.AlignData.PivotSideKind;
		return animation.AreConditionsMet(modelConditions, (!useTacticsConditions) ? null : animation.MoveData.TacticsConditions);
	}

	private int SetDecisionList(Model enemy, int frame)
	{
		decisions.Clear();
		ModelAnimation enemyModelAnimation = enemy.GetAnimationModule();
		if (GetCurrentDirectionToEnemy(enemyModelAnimation) * (float)enemyModelAnimation.GetSign() > 0f)
		{
			SetRandomAnimation();
			waitMode = WaitMode.SetWaitAnimationLength;
			return 0;
		}
		bool flag = false;
		if (GetUseChildrenDodge(enemy, MissileKind.SimpleMissile) && dodgeMissile)
		{
			int num = GetForDodgeMissiles(enemy, MissileKind.SimpleMissile);
			resultSource = AiData.TableType.dodgeTable;
			flag = true;
		}
		if (dodgeMagic && GetUseChildrenDodge(enemy, MissileKind.MagicMissile))
		{
			int num2 = GetForDodgeMissiles(enemy, MissileKind.MagicMissile);
			resultSource = AiData.TableType.dodgeTable;
			flag = true;
		}
		if (flag)
		{
			return decisions.Count;
		}
		int num3 = enemyModelAnimation.GetFrameInMove();
		if (responseDelay < num3 && !IsUninteruptIntervalEnd(enemyModelAnimation))
		{
			if (enemyAnimation.IsIntervalActiveAtInterpolatedFrame("Uninterrupt", enemyFrame))
			{
				if (!IsAttackIntervalEnd(enemyModelAnimation))
				{
					switch (defenceMode)
					{
					case DefenceMode.DefenceUseCounterAttack:
					{
						int num5 = GetFromTablesMove(enemy);
						if (0 < num5)
						{
							resultSource = AiData.TableType.movementsTable;
						}
						if (num5 == 0)
						{
							num5 = GetFromTablesDodge(enemy);
							if (0 < num5)
							{
								resultSource = AiData.TableType.dodgeTable;
							}
						}
						return num5;
					}
					case DefenceMode.DefenceUseDodge:
					{
						int num4 = GetFromTablesDodge(enemy);
						if (0 < num4)
						{
							resultSource = AiData.TableType.dodgeTable;
						}
						return num4;
					}
					case DefenceMode.DefenceUseBlock:
						resultSource = AiData.TableType.block;
						return 0;
					default:
						SetRandomAnimation();
						waitMode = WaitMode.SetWaitRandAttack;
						return 0;
					}
				}
				if (useSafeAttack)
				{
					int num6 = GetFromTablesMove(enemy);
					if (0 < num6)
					{
						resultSource = AiData.TableType.movementsTable;
					}
					if (offKeyFrame || 0 < num6)
					{
						return decisions.Count;
					}
				}
				if (useTableAttack)
				{
					int num7 = GetFromTablesAttack(enemy);
					if (0 < num7)
					{
						resultSource = AiData.TableType.outcometablesforattack;
					}
					if (offKeyFrame || 0 < num7)
					{
						return decisions.Count;
					}
				}
				if (useCautiousMovement)
				{
					List<TemplateAnimation> list = AiData.get_CautiousMovements();
					InfoAnimation animation = enemy.GetAnimationModule().GetCurrentInfo();
					int num8 = 0;
					if (animation != null && enemy.GetAnimationModule().GetIsPlaying())
					{
						num8 = animation.GetLastUninterruptFrame(true) - enemyFrame + 1;
					}
					decisions.Clear();
					foreach (TemplateAnimation item in list)
					{
						List<InfoAnimation> list2 = item.GetAnimations();
						foreach (InfoAnimation item2 in list2)
						{
							int a = num8;
							a = Mathf.Min(a, item2.GetMoveLengthExtended());
							int num9 = item2.GetMoveLengthStrict();
							if (num9 > a)
							{
								a = num9;
							}
							decisions.Add(new Decision(item2, a));
						}
					}
					int count = decisions.Count;
					if (0 < count)
					{
						resultSource = AiData.TableType.safeTable;
					}
					return decisions.Count;
				}
				SetRandomAnimation();
				waitMode = WaitMode.SetWaitRandUnint;
				return 0;
			}
			return 0;
		}
		bool flag2 = false;
		int count2 = quickAttackRolls.Count;
		List<global::Pair<string, TacticValue>> list3 = tactic.get_QuickAttacks();
		for (int i = 0; i < count2; i++)
		{
			ChanceRoll chanceRoll = quickAttackRolls[i];
			if (!chanceRoll.Flag)
			{
				continue;
			}
			global::Pair<string, TacticValue> quickAttackEntry = list3[i];
			List<InfoAnimation> list4 = new List<InfoAnimation>();
			AnimationData.AddTemplateAnimations(quickAttackEntry.First, list4);
			foreach (InfoAnimation item3 in list4)
			{
				if (item3 != null && IsPlayableAnimations(item3))
				{
					int wait = item3.GetMoveLengthStrict();
					decisions.Add(new Decision(item3, wait));
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			resultSource = AiData.TableType.quickAttact;
		}
		enemyCanEvade = false;
		int count3 = evadeRolls.Count;
		List<global::Pair<string, TacticValue>> list5 = tactic.get_Evades();
		for (int j = 0; j < count3; j++)
		{
			if (enemyCanEvade)
			{
				break;
			}
			ChanceRoll bHDKGLJIOJD2 = evadeRolls[j];
			if (!bHDKGLJIOJD2.Flag)
			{
				continue;
			}
			global::Pair<string, TacticValue> cCKLNOPEKHO2 = list5[j];
			List<InfoAnimation> list6 = new List<InfoAnimation>();
			AnimationData.AddTemplateAnimations(cCKLNOPEKHO2.First, list6);
			ModelAi enemyAi = enemy.GetAi();
			if (enemyAi == null)
			{
				continue;
			}
			foreach (InfoAnimation item4 in list6)
			{
				if (item4 != null && enemyAi.IsPlayableAnimations(item4))
				{
					enemyCanEvade = true;
					break;
				}
			}
		}
		TacticFactors factors = SetFactors(enemy);
		float num10 = tactic.GetExpectedWait(botAnimation, factors);
		if (num10 < 1f)
		{
			num10 = 1f;
		}
		float num11 = 1f - 1f / num10;
		float num12 = NekkiMath.randomFloat();
		if (num11 < num12)
		{
			waitElapsed = true;
		}
		if (waitElapsed || enemyCanEvade)
		{
			int num13 = 0;
			if (useTableAttack)
			{
				num13 = GetFromTablesAttack(enemy);
				if (0 < num13)
				{
					resultSource = AiData.TableType.outcometablesforattack;
				}
				if (offKeyFrame && !enemyCanEvade)
				{
					return decisions.Count;
				}
				if (0 < num13)
				{
					return num13;
				}
			}
			if (enemyCanEvade)
			{
				List<TemplateAnimation> list7 = AiData.get_EvadeThrowDodges();
				decisions.Clear();
				foreach (TemplateAnimation item5 in list7)
				{
					List<InfoAnimation> list8 = item5.GetAnimations();
					foreach (InfoAnimation item6 in list8)
					{
						int jOHDCPNACOC2 = item6.GetMoveLengthStrict();
						decisions.Add(new Decision(item6, jOHDCPNACOC2));
					}
				}
				num13 = decisions.Count;
				if (0 < num13)
				{
					resultSource = AiData.TableType.evadeList;
				}
			}
			else if (useCautiousMovement)
			{
				List<TemplateAnimation> list9 = AiData.get_CautiousMovements();
				decisions.Clear();
				foreach (TemplateAnimation item7 in list9)
				{
					List<InfoAnimation> list10 = item7.GetAnimations();
					foreach (InfoAnimation item8 in list10)
					{
						int jOHDCPNACOC3 = item8.GetMoveLengthStrict();
						decisions.Add(new Decision(item8, jOHDCPNACOC3));
					}
				}
				num13 = decisions.Count;
				if (0 < num13)
				{
					resultSource = AiData.TableType.safeTable;
				}
			}
			if (num13 == 0)
			{
				SetRandomAnimation();
				waitMode = WaitMode.SetWaitAnimationLength;
				if (enemyCanEvade)
				{
					RemoveEvadeUnsafeDodgesAnimations();
				}
				return 0;
			}
		}
		return decisions.Count;
	}

	private static bool IsUninteruptIntervalEnd(ModelAnimation modelAnimation)
	{
		if (modelAnimation.GetIsPlaying())
		{
			InfoAnimation animation = modelAnimation.GetCurrentInfo();
			if (animation != null)
			{
				int num = animation.GetLastUninterruptFrame(false);
				int num2 = modelAnimation.GetCurrentFrame();
				if (num2 <= num)
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool IsAttackIntervalEnd(ModelAnimation modelAnimation)
	{
		if (modelAnimation.GetIsPlaying())
		{
			InfoAnimation animation = modelAnimation.GetCurrentInfo();
			if (animation != null)
			{
				int num = animation.GetLastAttackFrame(false);
				int num2 = modelAnimation.GetCurrentFrame();
				if (num2 <= num)
				{
					return false;
				}
			}
		}
		return true;
	}

	private int RemoveEvadeUnsafeDodgesAnimations()
	{
		int num = 0;
		List<string> list = AiData.get_EvadeUnsafeDodgesAnimations();
		foreach (Decision item in decisions)
		{
			bool flag = true;
			foreach (string item2 in list)
			{
				if (item.Animation.HasName(item2))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				decisions[num] = item;
				num++;
			}
		}
		return num;
	}

	private bool IsThrowingState(InfoAnimation animation, int frame)
	{
		List<string> intervalNames = AiData.get_ThrowableIntervals();
		bool useInterpolation = true;
		return animation.AreIntervalsActiveAtInterpolatedFrame(intervalNames, frame, useInterpolation);
	}

	private bool IsModelCanThrow(Model model)
	{
		candidateAnimations.Clear();
		List<string> throwNames = AiData.get_Throws();
		AnimationData.AddTemplateAnimationsByNames(throwNames, candidateAnimations);
		int num = candidateAnimations.Count;
		if (0 < num)
		{
			num = model.GetAi().GetPlayableAnimations(candidateAnimations);
		}
		if (0 < num)
		{
			return true;
		}
		return false;
	}

	private bool get_IsEnabled()
	{
		return get_AiOn() && (parameters.AiControlled || AiData.get_BothBotEnabled());
	}

	private int DecisionsToAimationsList()
	{
		candidateAnimations.Clear();
		_InterframesList.Clear();
		foreach (Decision item in decisions)
		{
			candidateAnimations.Add(item.Animation);
			_InterframesList.Add(item.Wait);
		}
		return 0;
	}

	private int GetSafetyAnimations(Model enemy)
	{
		List<TemplateAnimation> list = AiData.get_CautiousMovements();
		candidateAnimations.Clear();
		foreach (TemplateAnimation item in list)
		{
			List<InfoAnimation> list2 = item.GetAnimations();
			foreach (InfoAnimation item2 in list2)
			{
				candidateAnimations.Add(item2);
			}
		}
		InfoAnimation animation = enemy.GetAnimationModule().GetCurrentInfo();
		decisions.Clear();
		foreach (InfoAnimation item3 in candidateAnimations)
		{
			int num = 0;
			if (animation != null && enemy.GetAnimationModule().GetIsPlaying())
			{
				int num2 = item3.GetMoveLengthStrict();
				num = animation.GetLastUninterruptFrame(true) - enemyFrame + 1;
				if (num2 < num)
				{
					num = num2;
				}
			}
			decisions.Add(new Decision(item3, num));
		}
		return decisions.Count;
	}

	private int GetFromTablesMove(Model enemy)
	{
		if (enemyFrame % AiData.MovementsStep != 0)
		{
			offKeyFrame = true;
		}
		else
		{
			offKeyFrame = false;
		}
		if (botAnimation != null && enemyAnimation != null)
		{
			int num = GetFromTablesMove(enemy, botAnimation, enemyAnimation, _ModelAnimation.GetStartPosition(), enemy.GetAnimationModule().GetStartPosition(), ownFrame, enemyFrame, distanceError);
			if (0 < num)
			{
				num = GetPlayableAnimations(decisions);
			}
			if (0 < num)
			{
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					InfoAnimation animation = decisions[i].Animation;
					if (animation == null || TestWall(animation, _Model, enemy))
					{
						if (num2 < i)
						{
							decisions[num2] = decisions[i];
						}
						num2++;
					}
				}
				num = num2;
				decisions.Resize(num);
			}
			return num;
		}
		return 0;
	}

	private int GetFromTablesMove(Model enemy, InfoAnimation botMove, InfoAnimation enemyMove, float ownStartPosition, float enemyStartPosition, int ownFrameIndex, int enemyFrameIndex, float errorOffset, List<global::Pair<string, float>> nodePositions = null)
	{
		decisions.Clear();
		GroupTables groupTables = null;
		List<global::Pair<List<GroupTables>, string>> list = enemyMove.GetTacticGroupTables()[1];
		foreach (global::Pair<List<GroupTables>, string> item in list)
		{
			if (!(botWeaponSubtype == item.Second))
			{
				continue;
			}
			List<GroupTables> groupTablesList = item.First;
			foreach (GroupTables item2 in groupTablesList)
			{
				if (botWeaponSubtype == item2.GroupLabel)
				{
					groupTables = item2;
					break;
				}
			}
			break;
		}
		if (groupTables != null)
		{
			ModelAnimation enemyModelAnimation = enemy.GetAnimationModule();
			int num = enemyModelAnimation.GetSign();
			if (IsFitBotAnimation(botMove))
			{
				GroupTables botGroup = groupTables;
				string pivotPart = botMove.GetPivotPartName();
				int num2 = enemyFrameIndex - ownFrameIndex;
				int num3 = GetNearestKeyFrameId(num2);
				int num4 = num3 - num2;
				float distance = (float)num * (ownStartPosition - enemyStartPosition) + errorOffset;
				decisions.Clear();
				int num5 = GetRow(botGroup, pivotPart, num3, distance, decisions);
				foreach (Decision item3 in decisions)
				{
					if (botMove == item3.Animation)
					{
						int num6 = item3.Wait - ownFrameIndex + num4;
						decisions.Clear();
						if (0 < num6)
						{
							decisions.Add(new Decision(null, num6));
							return decisions.Count;
						}
						break;
					}
				}
			}
			int num7 = ((enemyFrameIndex % AiData.MovementsStep == 0) ? enemyFrameIndex : (enemyFrameIndex + AiData.MovementsStep - enemyFrameIndex % AiData.MovementsStep));
			decisions.Clear();
			int num8 = _ModelAnimation.GetSign();
			int num9 = 0;
			int count = groupTables.Tables.Count;
			for (int i = 0; i < count; i++)
			{
				string tableLabel = groupTables.Tables[i].Label;
				float num10 = ((nodePositions != null) ? GetNodeX(tableLabel, nodePositions) : GetNodeX(tableLabel, _Model, enemy));
				float num11 = botMove.ShiftTable.GetDistance(enemyFrameIndex, tableLabel);
				float num12 = botMove.ShiftTable.GetDistance(num7, tableLabel);
				float num13 = num12 - num11;
				float oIOMNNFMDOO2 = (float)num * (num10 + num13 * (float)num8 - enemyStartPosition) + errorOffset;
				num9 += GetRow(groupTables, tableLabel, num7, oIOMNNFMDOO2, decisions);
			}
			if (num7 == enemyFrameIndex || num9 == 0)
			{
				return num9;
			}
			decisions.Clear();
			decisions.Add(new Decision(null, num7 - enemyFrameIndex));
			return 1;
		}
		return decisions.Count;
	}

	private int GetFromTablesAttack(Model enemy)
	{
		decisions.Clear();
		if (enemyFrame % AiData.MovementsStep != 0)
		{
			offKeyFrame = true;
			return 0;
		}
		offKeyFrame = false;
		TacticFactors factors = SetFactors(enemy);
		int num = GetEnemyResponseDelay(factors);
		GroupTables groupTables = null;
		List<global::Pair<List<GroupTables>, string>> list = enemyAnimation.GetTacticGroupTables()[0];
		foreach (global::Pair<List<GroupTables>, string> item in list)
		{
			if (!(botWeaponSubtype == item.Second))
			{
				continue;
			}
			List<GroupTables> groupTablesList = item.First;
			foreach (GroupTables item2 in groupTablesList)
			{
				if (botWeaponSubtype == item2.GroupLabel)
				{
					groupTables = item2;
					break;
				}
			}
			break;
		}
		if (groupTables != null)
		{
			ModelAnimation enemyModelAnimation = enemy.GetAnimationModule();
			int frameIndex = enemyFrame;
			int ownFrameIndex = ownFrame;
			float num2 = enemyModelAnimation.GetStartPosition();
			int num3 = enemyModelAnimation.GetSign();
			int count = groupTables.Tables.Count;
			candidateAnimations.Clear();
			_InterframesList.Clear();
			int num4 = enemyFrame + num;
			for (int i = 0; i < count; i++)
			{
				TacticalTable table = groupTables.Tables[i];
				int num5 = table.GetArrayIndexByFrameIndex(frameIndex);
				if (-1 >= num5)
				{
					continue;
				}
				float num6 = GetNodeX(table.Label, _Model, enemy);
				float distance = (float)num3 * (num6 - num2) + distanceError;
				List<IntervalNew> intervals = table.IntervalList[num5].Items;
				foreach (IntervalNew item3 in intervals)
				{
					int num7 = item3.GetInterframeByDistance(distance);
					if (0 < num7 && num7 <= num4)
					{
						candidateAnimations.Add(item3.Animation);
						_InterframesList.Add(item3.Animation.GetMoveLengthStrict());
					}
				}
			}
			int num8 = candidateAnimations.Count;
			if (0 < num8)
			{
				num8 = GetPlayableAnimations(candidateAnimations, _InterframesList);
			}
			if (0 < num8)
			{
				int num9 = 0;
				for (int j = 0; j < num8; j++)
				{
					InfoAnimation animation = candidateAnimations[j];
					if (TestWall(animation, _Model, enemy))
					{
						if (num9 < j)
						{
							candidateAnimations[num9] = animation;
							_InterframesList[num9] = _InterframesList[j];
						}
						num9++;
					}
				}
				num8 = num9;
				candidateAnimations.Resize(num8);
				_InterframesList.Resize(num8);
			}
			for (int k = 0; k < num8; k++)
			{
				decisions.Add(new Decision(candidateAnimations[k], _InterframesList[k]));
			}
		}
		return decisions.Count;
	}

	private int GetFromTablesDodge(Model enemy, AnimationListType listType = AnimationListType.Standard)
	{
		decisions.Clear();
		int num = 0;
		switch (listType)
		{
		case AnimationListType.Standard:
			num = enemyFrame;
			break;
		case AnimationListType.Missile:
			num = enemy.GetAnimationModule().GetRenderTickCount();
			break;
		}
		if (num % AiData.MovementsStep != 0)
		{
			switch (listType)
			{
			case AnimationListType.Standard:
				offKeyFrame = true;
				break;
			case AnimationListType.Missile:
			{
				offKeyFrame = false;
				int num2 = (num / AiData.MovementsStep + 1) * AiData.MovementsStep;
				int num3 = num2 - num;
				break;
			}
			}
		}
		else
		{
			offKeyFrame = false;
		}
		InfoAnimation animation = null;
		switch (listType)
		{
		case AnimationListType.Standard:
			animation = enemy.GetAnimationModule().GetCurrentInfo();
			break;
		case AnimationListType.Missile:
			animation = enemy.GetAnimationModule().GetFirstInfo();
			break;
		}
		GroupTables groupTables = null;
		List<global::Pair<List<GroupTables>, string>> list = animation.GetTacticGroupTables()[2];
		// Event-only poses/steps can legitimately have no precomputed dodge table.
		if (list.Count == 0 && listType == AnimationListType.Standard && animation.GetFirstKeysCondition() == null)
			return 0;
		if (list.Count == 1)
		{
			List<GroupTables> groupTablesList = list[0].First;
			if (groupTablesList.Count == 1)
			{
				groupTables = groupTablesList[0];
				if (groupTables != null)
				{
					List<string> intervalNames = null;
					List<string> list2 = null;
					switch (listType)
					{
					case AnimationListType.Standard:
						intervalNames = AiData.get_MovesFirstIteration();
						list2 = AiData.get_MovesLastIteration();
						break;
					case AnimationListType.Missile:
						intervalNames = AiData.get_MissilesFirstIteration();
						list2 = AiData.get_MissilesLastIteration();
						break;
					}
					ModelAnimation enemyModelAnimation = enemy.GetAnimationModule();
					InfoAnimation pJAHIOELGGD2 = botAnimation;
					if (_ModelAnimation.GetIsPlaying() && pJAHIOELGGD2 != null)
					{
						InfoAnimation pJAHIOELGGD3 = pJAHIOELGGD2.GetTacticEquivalent();
						if (pJAHIOELGGD3 != null)
						{
							pJAHIOELGGD2 = pJAHIOELGGD3;
						}
						bool flag = false;
						foreach (string item in list2)
						{
							if (pJAHIOELGGD2.HasName(item))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							string tableLabel = pJAHIOELGGD2.GetPivotPartName();
							TacticalTable table = groupTables.GetTacticalTableByLabel(tableLabel);
							if (table != null)
							{
								int num4 = num - ownFrame;
								int num5 = GetNearestKeyFrameId(num4);
								int num6 = num5 - num4;
								int num7 = table.GetArrayIndexByFrameIndex(num5);
								if (-1 < num7)
								{
									float num8 = enemyModelAnimation.GetStartPosition();
									if (listType == AnimationListType.Missile)
									{
										num8 = enemyModelAnimation.GetFirstStartPositionX();
									}
									int num9 = enemyModelAnimation.GetSign();
									float num10 = _ModelAnimation.GetStartPosition();
									float distance = (float)num9 * (num10 - num8) + distanceError;
									Intervals frameIntervals = table.IntervalList[num7];
									bool flag2 = false;
									foreach (IntervalNew item2 in frameIntervals.Items)
									{
										if (item2.Animation == pJAHIOELGGD2)
										{
											int num11 = item2.GetInterframeByDistance(distance);
											if (0 < num11)
											{
												flag2 = true;
												break;
											}
										}
									}
									if (!flag2)
									{
										int num12 = animation.GetLastAttackFrame(true);
										int wait = num12 - enemyFrame + 1;
										decisions.Add(new Decision(null, wait));
										return decisions.Count;
									}
								}
							}
						}
					}
					if (num % AiData.MovementsStep != 0)
					{
						switch (listType)
						{
						case AnimationListType.Standard:
							offKeyFrame = true;
							decisions.Clear();
							return 0;
						case AnimationListType.Missile:
						{
							offKeyFrame = false;
							int num13 = (num / AiData.MovementsStep + 1) * AiData.MovementsStep;
							int num3 = num13 - num;
							break;
						}
						}
					}
					else
					{
						offKeyFrame = false;
					}
					candidateAnimations.Clear();
					AnimationData.AddTemplateAnimationsByNames(intervalNames, candidateAnimations);
					int count = candidateAnimations.Count;
					int num14 = 0;
					for (int i = 0; i < count; i++)
					{
						InfoAnimation pJAHIOELGGD4 = candidateAnimations[i];
						if (TestWall(pJAHIOELGGD4, _Model, enemy))
						{
							if (num14 < i)
							{
								candidateAnimations[num14] = pJAHIOELGGD4;
							}
							num14++;
						}
					}
					count = num14;
					candidateAnimations.Resize(count);
					float num15 = enemyModelAnimation.GetStartPosition();
					if (listType == AnimationListType.Missile)
					{
						num15 = enemyModelAnimation.GetFirstStartPositionX();
					}
					int num16 = enemyModelAnimation.GetSign();
					foreach (TacticalTable item3 in groupTables.Tables)
					{
						string nodeName = item3.Label;
						int num17;
						if (listType == AnimationListType.Missile)
						{
							num17 = num;
						}
						else
						{
							int num18 = GetNearestKeyFrameId(num);
							int num19 = num18 - num;
							num17 = item3.GetArrayIndexByFrameIndex(num18);
						}
						if (-1 < num17)
						{
							float num20 = GetNodeX(nodeName, _Model, enemy.GetRootModel());
							float oIOMNNFMDOO2 = (float)num16 * (num20 - num15) + distanceError;
							Intervals gOOGNIPMCEM2 = null;
							gOOGNIPMCEM2 = ((num17 >= item3.IntervalList.Count) ? item3.IntervalList[0] : item3.IntervalList[num17]);
							foreach (IntervalNew item4 in gOOGNIPMCEM2.Items)
							{
								int num21 = item4.GetInterframeByDistance(oIOMNNFMDOO2);
								if (0 >= num21)
								{
									continue;
								}
								for (int j = 0; j < count; j++)
								{
									if (candidateAnimations[j] == item4.Animation)
									{
										count--;
										candidateAnimations[j] = candidateAnimations[count];
										break;
									}
								}
							}
						}
						else
						{
							count = 0;
						}
					}
					candidateAnimations.Resize(count);
					IntervalAnimation interval = enemyModelAnimation.FindInterval(IntervalAnimation.IntervalType.INTERVAL_UNINTERRUPT);
					InfoAnimation pJAHIOELGGD5 = animation;
					bool flag3 = enemyModelAnimation.GetIsPlaying();
					if (interval != null && pJAHIOELGGD5 != null && flag3)
					{
						int num22 = pJAHIOELGGD5.GetLastUninterruptFrame(true);
						int num23 = num22 - enemyFrame;
						for (int k = 0; k < count; k++)
						{
							InfoAnimation pJAHIOELGGD6 = candidateAnimations[k];
							int num24 = pJAHIOELGGD6.GetLastUninterruptFrame(true);
							if (num23 <= num24 && !IsSafeDodges(pJAHIOELGGD6))
							{
								count--;
								candidateAnimations[k] = candidateAnimations[count];
								k--;
							}
						}
						candidateAnimations.Resize(count);
					}
					int count2 = candidateAnimations.Count;
					if (0 < count2)
					{
						count2 = GetPlayableAnimations(candidateAnimations);
					}
					if (candidateAnimations.Count == 0 && listType == AnimationListType.Standard)
					{
						List<TemplateAnimation> list3 = AiData.get_EmergencyDodgesAnimations();
						foreach (TemplateAnimation item5 in list3)
						{
							List<InfoAnimation> list4 = item5.GetAnimations();
							foreach (InfoAnimation item6 in list4)
							{
								candidateAnimations.Add(item6);
							}
						}
						count2 = GetPlayableAnimations(candidateAnimations, null, true);
					}
					int num25 = candidateAnimations.Count;
					if (0 < num25)
					{
						num25 = GetPlayableAnimations(candidateAnimations);
					}
					for (int l = 0; l < num25; l++)
					{
						int num26 = animation.GetLastAttackFrame(true);
						int jOHDCPNACOC2 = num26 - enemyFrame + 1;
						decisions.Add(new Decision(candidateAnimations[l], jOHDCPNACOC2));
					}
				}
				return decisions.Count;
			}
			GameLog.Write("null dodge table");
			return 0;
		}
		GameLog.Write("null dodge table");
		return 0;
	}

	private int GetForDodgeMissiles(Model enemy, MissileKind missileKind)
	{
		int num = 0;
		List<Decision> list = new List<Decision>();
		List<Decision> list2 = new List<Decision>(decisions);
		int i = 0;
		for (int count = enemy.GetWeaponModels().Count; i < count; i++)
		{
			WeaponModel weaponModel = enemy.GetWeaponModels()[i];
			List<Decision> missileDecisions = new List<Decision>(list);
			if ((IsMissileAnimation(weaponModel.GetAnimationModule().GetCurrentInfo()) || missileKind != MissileKind.SimpleMissile) && (IsMagicAnimation(weaponModel.GetAnimationModule().GetCurrentInfo()) || missileKind != MissileKind.MagicMissile) && weaponModel.GetAnimationModule().GetCurrentInfo() != null)
			{
				num = GetFromTablesDodge(weaponModel, AnimationListType.Missile);
				list = ((list.Count <= 0) ? new List<Decision>(decisions) : Intersection(missileDecisions, decisions));
			}
		}
		num = list.Count;
		if (list2.Count != 0)
		{
			decisions = Intersection(list2, list);
		}
		else
		{
			decisions = list;
		}
		return num;
	}

	private bool IsSafetyAnimations(InfoAnimation animation, Model model, int frame)
	{
		return false;
	}

	private int GetRow(GroupTables groupTables, string tableLabel, int frameIndex, float distance, List<Decision> results)
	{
		int count = results.Count;
		TacticalTable table = groupTables.GetTacticalTableByLabel(tableLabel);
		if (table != null)
		{
			Intervals frameIntervals = table.GetFrameByFrameIndex(frameIndex);
			if (frameIntervals != null)
			{
				foreach (IntervalNew item in frameIntervals.Items)
				{
					int num = item.GetInterframeByDistance(distance);
					if (0 < num)
					{
						results.Add(new Decision(item.Animation, num));
						AddModTacticAlternatives(item.Animation, num, results);
					}
				}
			}
		}
		return results.Count - count;
	}

	private List<InfoAnimation> _modTacticSource;

	private int _modTacticSourceCount = -1;

	private readonly Dictionary<InfoAnimation, List<InfoAnimation>> _modTacticAlternatives = new Dictionary<InfoAnimation, List<InfoAnimation>>();

	// The shipped tables have no rows for mod moves. A mod move that names a
	// native TacticEquivalent is also offered wherever a row offers that
	// equivalent, with the same wait; the usual playability checks still apply.
	// Native moves never gain alternatives, so vanilla decisions are unchanged.
	private void AddModTacticAlternatives(InfoAnimation equivalent, int wait, List<Decision> decisions)
	{
		if (equivalent == null)
		{
			return;
		}
		List<InfoAnimation> available = _Model.GetAvailableAnimations();
		if (available == null)
		{
			return;
		}
		if (!ReferenceEquals(available, _modTacticSource) || available.Count != _modTacticSourceCount)
		{
			_modTacticSource = available;
			_modTacticSourceCount = available.Count;
			_modTacticAlternatives.Clear();
			foreach (InfoAnimation move in available)
			{
				InfoAnimation native = move?.GetTacticEquivalent();
				// Mod runtime names are content IDs; native move names never contain ':'.
				if (native == null || move.Name == null || move.Name.IndexOf(':') < 0)
				{
					continue;
				}
				if (!_modTacticAlternatives.TryGetValue(native, out var list))
				{
					_modTacticAlternatives.Add(native, list = new List<InfoAnimation>());
				}
				list.Add(move);
			}
		}
		if (_modTacticAlternatives.TryGetValue(equivalent, out var alternatives))
		{
			foreach (InfoAnimation move in alternatives)
			{
				decisions.Add(new Decision(move, wait));
			}
		}
	}

	private int GetModelDirection(Model model, Model enemy)
	{
		int num = 0;
		if (model.GetBodyObject().GetPivotNode() == null || enemy.GetBodyObject().GetPivotNode() == null)
		{
			return 0;
		}
		return (model.GetBodyObject().GetPivotNode().GetStart()
			.GetX() < enemy.GetBodyObject().GetPivotNode().GetStart()
			.GetX()) ? 1 : (-1);
	}

	private List<Decision> Intersection(List<Decision> firstList, List<Decision> secondList)
	{
		List<Decision> list = new List<Decision>();
		foreach (Decision item in firstList)
		{
			foreach (Decision item2 in secondList)
			{
				if (item2.Animation == item.Animation)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private bool TestBack(InfoAnimation animation, Model model, Model enemy)
	{
		return true && TestBack(animation, _Model, enemy, "NPivot", "NPivot");
	}

	private bool TestWall(InfoAnimation animation, Model model, Model enemy)
	{
		bool flag = true;
		Model enemyRoot = enemy.GetRootModel();
		return flag && TestWall(animation, _Model, enemyRoot, "NPivot");
	}

	private bool TestBack(InfoAnimation animation, Model model, Model enemy, string name, string enemyNodeName)
	{
		float num = model.GetBodyObject().GetNodeByName(name).GetStart()
			.GetX();
		float num2 = enemy.GetBodyObject().GetNodeByName(enemyNodeName).GetStart()
			.GetX();
		int num3 = animation.GetLastUninterruptFrame(true);
		if (num3 < 0)
		{
			num3 = 0;
		}
		float num4 = animation.ShiftTable.GetDistance(num3, name);
		float num5 = num + (float)GetModelDirection(model, enemy) * num4;
		float num8;
		if (enemy.GetAnimationModule().GetIsPlaying())
		{
			InfoAnimation enemyMove = enemy.GetAnimationModule().GetCurrentInfo();
			int currentEnemyFrame = enemyFrame;
			int shiftFrame = currentEnemyFrame + num3;
			float num6 = enemyMove.ShiftTable.GetDistance(shiftFrame, enemyNodeName);
			float num7 = enemy.GetAnimationModule().GetStartPosition();
			num8 = num7 + (float)enemy.GetFacingSign() * num6;
		}
		else
		{
			num8 = enemy.GetBodyObject().GetNodeByName(enemyNodeName).GetStart()
				.GetX();
		}
		if ((num - num2) * (num5 - num8) < 0f)
		{
			return false;
		}
		return true;
	}

	private bool TestWall(InfoAnimation animation, Model model, Model enemy, string name)
	{
		int num = animation.GetLastUninterruptFrame(true);
		if (num < 0)
		{
			num = 0;
		}
		float num2 = model.GetBodyObject().GetNodeByName(name).GetStart()
			.GetX();
		float num3 = animation.ShiftTable.GetDistance(num, name);
		float num4 = num2 + (float)GetModelDirection(model, enemy) * num3;
		float num5 = model.GetAnimationModule().GetLeftWallX();
		float num6 = model.GetAnimationModule().GetRightWallX();
		float num7 = model.GetAnimationModule().GetFrontAlignMargin();
		float num8 = model.GetAnimationModule().GetBackAlignMargin();
		float num9 = ((!(num8 < num7)) ? num8 : num7);
		if (num4 - num9 < num5 || num6 < num4 + num9)
		{
			return false;
		}
		return true;
	}

	private bool IsIncludeIntervalAttack(InfoAnimation animation)
	{
		List<IntervalAnimation> intervals = animation.MoveData.Intervals;
		foreach (IntervalAnimation item in intervals)
		{
			if (item.Type == IntervalAnimation.IntervalType.INTERVAL_ATTACK)
			{
				return true;
			}
		}
		return false;
	}

	private void SetQuickAttackRnd()
	{
		List<global::Pair<string, TacticValue>> list = tactic.get_QuickAttacks();
		int count = list.Count;
		quickAttackRolls.Resize(count);
		foreach (ChanceRoll item in quickAttackRolls)
		{
			item.Roll = NekkiMath.randomFloat();
		}
	}

	private void SetQuickAttackChances(TacticFactors factors)
	{
		List<global::Pair<string, TacticValue>> list = tactic.get_QuickAttacks();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			ChanceRoll chanceRoll = quickAttackRolls[i];
			chanceRoll.Chance = list[i].Second.GetValue(factors);
			chanceRoll.Flag = chanceRoll.Roll < chanceRoll.Chance;
		}
	}

	private void SetEvadesRnd()
	{
		List<global::Pair<string, TacticValue>> list = tactic.get_Evades();
		int count = list.Count;
		evadeRolls.Resize(count);
		foreach (ChanceRoll item in evadeRolls)
		{
			item.Roll = NekkiMath.randomFloat();
		}
	}

	private void SetEvadesChances(TacticFactors factors)
	{
		List<global::Pair<string, TacticValue>> list = tactic.get_Evades();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			ChanceRoll chanceRoll = evadeRolls[i];
			chanceRoll.Chance = list[i].Second.GetValue(factors);
			chanceRoll.Flag = chanceRoll.Roll < chanceRoll.Chance;
		}
	}

	private float GetDistanceToEnemy(Model model)
	{
		int direction = GetModelDirection(model, model.GetCombatTarget().GetRootModel());
		int aOJJBKLCHJO2 = GetModelDirection(model.GetCombatTarget().GetRootModel(), model);
		ModelNode node = model.GetAnimationModule().GetNodeByNameForSign(AiData.get_DistanceNode(), direction);
		ModelNode lCDGOCIAIDK2 = model.GetCombatTarget().GetRootModel().GetAnimationModule()
			.GetNodeByNameForSign(AiData.get_DistanceNode(), aOJJBKLCHJO2);
		if (node != null && lCDGOCIAIDK2 != null)
		{
			float f = node.GetStart().GetX() - lCDGOCIAIDK2.GetStart().GetX();
			return Mathf.Abs(f);
		}
		return 0f;
	}

	private TacticFactors SetFactors(Model enemy)
	{
		TacticFactors factors = new TacticFactors(_Model.GetModelStats(), _Model.GetNoRangedFlag(), _Model.GetMagicCharges());
		_Model.GetModelStats().GetCountAndDamage(true, enemyAnimation, ref factors.FactorsCount, ref factors.Damage, ref factors.Hits);
		factors.Health = (ObscuredFloat)(_Model.Parameters.GetCurrentLife());
		factors.EnemyHealth = (ObscuredFloat)(enemy.Parameters.GetCurrentLife());
		factors.AnimationFrames = enemy.GetAnimationModule().GetFrameInMove();
		factors.ChildFrames = ChildMaxModelFrame(enemy);
		factors.Distance = GetDistanceToEnemy(_Model);
		factors.CurrentAnimation = _Model.GetAnimationModule().GetCurrentInfo();
		factors.EnemyCurrentAnimation = enemy.GetAnimationModule().GetCurrentInfo();
		return factors;
	}
}
