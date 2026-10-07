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

		public Decision(InfoAnimation DBOLBEOCEME, int JOHDCPNACOC)
		{
			Animation = DBOLBEOCEME;
			Wait = JOHDCPNACOC;
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

	public ModelAi(ModelAnimation MEKLGEGJPFP, ModelPhysics LBELNKIDMIB, string PPIEODBOOJA, ModelParameters JCICKLIMBEF)
	{
		_ModelAnimation = MEKLGEGJPFP;
		_ModelPhysics = LBELNKIDMIB;
		enemyAnimation = null;
		botAnimation = null;
		botWeaponSubtype = AiData.GetItemEquivalent(PPIEODBOOJA);
		distanceError = 0f;
		frameError = 0;
		unusedCounter = 0;
		parameters = JCICKLIMBEF;
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

	public void setAvailableAnimations(List<InfoAnimation> MAHEJFLCCHP)
	{
	}

	public InfoAnimation Render(Model FNKFIMEDNLP, int JLLPJLEDBPG)
	{
		if (!get_IsEnabled() || FNKFIMEDNLP == null || _ModelAnimation.GetCurrentInfo() == null)
		{
			return null;
		}
        // A newly created controller can join while its opponent is already
        // animating, before another animation-start notification arrives.
        if (enemyAnimation == null)
        {
            StartAnimationEnemy(FNKFIMEDNLP);
            if (enemyAnimation == null) return null;
        }
        // A controller joining an ongoing fight may miss its own animation-start
        // notification. Seed that observation in every fight mode; otherwise
        // IsFitIntervalAndMove refuses its decisions despite a running idle move.
        if (botAnimation == null && _ModelAnimation.GetIsPlaying() && Fight.GetCurrentFight() != null)
        {
            StartAnimationBot(_ModelAnimation.GetCurrentInfo());
        }
		ModelAnimation oJIEPADIEDE = FNKFIMEDNLP.GetAnimationModule();
		TacticFactors fJCBLOKOBBD = SetFactors(FNKFIMEDNLP);
		if (oJIEPADIEDE.GetIsPlaying())
		{
			int num = oJIEPADIEDE.GetFrameInMove();
			int num2 = oJIEPADIEDE.GetStartFrameOffset();
			int num3 = GetFrameError(fJCBLOKOBBD);
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
			if (_ModelAnimation.GetIsPlaying() && oJIEPADIEDE.GetIsPlaying())
			{
				InfoAnimation pJAHIOELGGD = _ModelAnimation.GetCurrentInfo();
				InfoAnimation pJAHIOELGGD2 = oJIEPADIEDE.GetCurrentInfo();
				if (pJAHIOELGGD != null && pJAHIOELGGD2 != null)
				{
					int num6 = pJAHIOELGGD.GetMoveLengthStrict();
					switch (waitMode)
					{
					case WaitMode.SetWaitRandAttack:
						decisionWait = pJAHIOELGGD2.GetLastAttackFrame(true) - enemyFrame + 1;
						decisionWait = Mathf.Min(decisionWait, pJAHIOELGGD.GetMoveLengthExtended());
						if (num6 > decisionWait)
						{
							decisionWait = num6;
						}
						decisionWait--;
						break;
					case WaitMode.SetWaitRandUnint:
						decisionWait = pJAHIOELGGD2.GetLastUninterruptFrame(true) - enemyFrame + 1;
						decisionWait = Mathf.Min(decisionWait, pJAHIOELGGD.GetMoveLengthExtended());
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
			WaitMode pLDABIGHHFG = waitMode;
			if (pLDABIGHHFG == WaitMode.SetWaitRandAttack || pLDABIGHHFG == WaitMode.SetWaitRandUnint || pLDABIGHHFG == WaitMode.SetWaitAnimationLength)
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
		InfoAnimation pJAHIOELGGD3 = oJIEPADIEDE.GetCurrentInfo();
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
				RandomizeBehavior(FNKFIMEDNLP);
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
                int? chosen = Eclipse.Modding.ModRuntime.DecideAi(modTactic, this, _Model, FNKFIMEDNLP, decisionFrame, available);
                _modDecisionOwned = chosen.HasValue;
                if (chosen.HasValue) return chosen.Value >= 0 && chosen.Value < available.Count ? available[chosen.Value] : null;
            }
        }
		defenceMode = SelectDefenceMode(FNKFIMEDNLP);
		safeAttackChance = tactic.GetUseSafeAttackChance(fJCBLOKOBBD);
		useSafeAttack = safeAttackRoll < safeAttackChance;
		tableAttackChance = tactic.GetTableAttackChance(fJCBLOKOBBD);
		useTableAttack = tableAttackRoll < tableAttackChance;
		SetQuickAttackChances(fJCBLOKOBBD);
		SetEvadesChances(fJCBLOKOBBD);
		cautiousMovementChance = tactic.GetCautiousMovementsChance(fJCBLOKOBBD);
		useCautiousMovement = cautiousMovementRoll < cautiousMovementChance;
		dodgeMissileChance = tactic.GetDodgeMissileChance(fJCBLOKOBBD);
		dodgeMissile = dodgeMissileRoll < dodgeMissileChance;
		dodgeMagicChance = tactic.GetDodgeMagicChance(fJCBLOKOBBD);
		dodgeMagic = dodgeMagicRoll < dodgeMagicChance;
		int num7 = SetDecisionList(FNKFIMEDNLP, JLLPJLEDBPG);
		if (0 < num7)
		{
			waitElapsed = false;
			GetPlayableAnimations(decisions);
			DecisionsToAimationsList();
			int num8 = SelectAnimationWithWeights(candidateAnimations);
			if (-1 < num8)
			{
				decisionWait = _InterframesList[num8];
				InfoAnimation dBOLBEOCEME = candidateAnimations[num8];
				LogTable(FNKFIMEDNLP.GetAnimationModule(), JLLPJLEDBPG, num7, dBOLBEOCEME, decisionWait);
				return candidateAnimations[num8];
			}
		}
		else if (waitRequested)
		{
			LogTable(FNKFIMEDNLP.GetAnimationModule(), JLLPJLEDBPG - 1, 1, null, 0);
			waitElapsed = false;
		}
		return null;
	}

	public void RandomizeBehavior(Model OGBHDKKOIGH)
	{
		TacticFactors oHKCJDCMOKN = new TacticFactors(_Model.GetModelStats(), _Model.GetNoRangedFlag(), _Model.GetMagicCharges());
		_Model.GetModelStats().GetCountAndDamage(true, enemyAnimation, ref oHKCJDCMOKN.FactorsCount, ref oHKCJDCMOKN.Damage, ref oHKCJDCMOKN.Hits);
		oHKCJDCMOKN.Health = (ObscuredFloat)(_Model.Parameters.GetCurrentLife());
		oHKCJDCMOKN.EnemyHealth = (ObscuredFloat)(OGBHDKKOIGH.Parameters.GetCurrentLife());
		oHKCJDCMOKN.AnimationFrames = OGBHDKKOIGH.GetAnimationModule().GetFrameInMove();
		oHKCJDCMOKN.ChildFrames = ChildMaxModelFrame(OGBHDKKOIGH);
		attackRoll = NekkiMath.randomFloat();
		safeAttackRoll = NekkiMath.randomFloat();
		tableAttackRoll = NekkiMath.randomFloat();
		cautiousMovementRoll = NekkiMath.randomFloat();
		dodgeMissileRoll = NekkiMath.randomFloat();
		dodgeMagicRoll = NekkiMath.randomFloat();
		distanceError = GetDistanceError(oHKCJDCMOKN);
		frameError = GetFrameError(oHKCJDCMOKN);
	}

	private int ChildMaxModelFrame(Model ACENLMONNPA)
	{
		int num = 0;
		int i = 0;
		for (int count = ACENLMONNPA.GetWeaponModels().Count; i < count; i++)
		{
			WeaponModel gKIANLDJFCH = ACENLMONNPA.GetWeaponModels()[i];
			if (gKIANLDJFCH != null)
			{
				int num2 = gKIANLDJFCH.GetAnimationModule().GetRenderTickCount();
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public void StartAnimationBot(InfoAnimation DBOLBEOCEME)
	{
		if (!get_IsEnabled())
		{
			return;
		}
		if (_ModelAnimation.GetIsPlaying() && DBOLBEOCEME != null)
		{
			InfoAnimation pJAHIOELGGD = DBOLBEOCEME.GetTacticEquivalent();
			if (pJAHIOELGGD != null)
			{
				botAnimation = pJAHIOELGGD;
			}
			else
			{
				botAnimation = DBOLBEOCEME;
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

	public void StartAnimationEnemy(Model OGBHDKKOIGH)
	{
		if (!get_IsEnabled())
		{
			return;
		}
		ModelAnimation oJIEPADIEDE = OGBHDKKOIGH.GetAnimationModule();
		InfoAnimation pJAHIOELGGD = oJIEPADIEDE.GetCurrentInfo();
		if (oJIEPADIEDE.GetIsPlaying() && pJAHIOELGGD != null)
		{
			InfoAnimation pJAHIOELGGD2 = pJAHIOELGGD.GetTacticEquivalent();
			if (pJAHIOELGGD2 == null)
			{
				enemyAnimation = pJAHIOELGGD;
			}
			else
			{
				enemyAnimation = pJAHIOELGGD2;
			}
			InfoAnimation cOKFBIJAFLH = enemyAnimation;
			RandomizeBehavior(OGBHDKKOIGH);
			if (!IsIgnoredEnemyAnimation(cOKFBIJAFLH))
			{
				TacticFactors oHKCJDCMOKN = new TacticFactors(_Model.GetModelStats(), _Model.GetNoRangedFlag(), _Model.GetMagicCharges());
				_Model.GetModelStats().GetCountAndDamage(true, enemyAnimation, ref oHKCJDCMOKN.FactorsCount, ref oHKCJDCMOKN.Damage, ref oHKCJDCMOKN.Hits);
				oHKCJDCMOKN.Health = (ObscuredFloat)(_Model.Parameters.GetCurrentLife());
				oHKCJDCMOKN.EnemyHealth = (ObscuredFloat)(OGBHDKKOIGH.Parameters.GetCurrentLife());
				oHKCJDCMOKN.AnimationFrames = OGBHDKKOIGH.GetAnimationModule().GetFrameInMove();
				oHKCJDCMOKN.ChildFrames = ChildMaxModelFrame(OGBHDKKOIGH);
				responseDelay = GetResponseDelay(oHKCJDCMOKN);
			}
		}
	}

	private bool IsIgnoredEnemyAnimation(InfoAnimation DBOLBEOCEME)
	{
		List<string> list = AiData.get_IgnoredEnemyAnimations();
		foreach (string item in list)
		{
			if (DBOLBEOCEME.HasName(item))
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

	public void SetWeaponEnemy(string PPIEODBOOJA)
	{
		enemyWeaponSubtype = AiData.GetItemEquivalent(PPIEODBOOJA);
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

	public void SetWeaponBot(string PPIEODBOOJA)
	{
		botWeaponSubtype = AiData.GetItemEquivalent(PPIEODBOOJA);
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

	public int SelectAnimationWithWeights(List<InfoAnimation> MAHEJFLCCHP)
	{
		Model fGCODGKLHED = _Model.GetCombatTarget();
		if (fGCODGKLHED != null)
		{
			Model fNKFIMEDNLP = fGCODGKLHED.GetRootModel();
			TacticFactors fJCBLOKOBBD = SetFactors(fNKFIMEDNLP);
			return tactic.SelectAnimationWithWeights(MAHEJFLCCHP, botAnimation, fJCBLOKOBBD);
		}
		return -1;
	}

	public void ChangeTactic(string name)
	{
		Tactic bJBIGPGJKIE = AiData.GetTacticByName(name);
		ChangeTactic(bJBIGPGJKIE);
	}

	public void ChangeTactic(Tactic BJBIGPGJKIE)
	{
		if (BJBIGPGJKIE != null)
		{
			tactic = BJBIGPGJKIE;
		}
	}

	private int GetResponseDelay(TacticFactors FJCBLOKOBBD)
	{
		return tactic.GetResponseDelay(FJCBLOKOBBD) + 1;
	}

	private float GetDistanceError(TacticFactors FJCBLOKOBBD)
	{
		return tactic.GetDistanceError(FJCBLOKOBBD);
	}

	private int GetFrameError(TacticFactors FJCBLOKOBBD)
	{
		return tactic.GetFrameError(FJCBLOKOBBD);
	}

	private int GetEnemyResponseDelay(TacticFactors FJCBLOKOBBD)
	{
		return tactic.GetEnemyResponseDelay(FJCBLOKOBBD);
	}

	private static bool GetRandomFlag(float KFJGPCLOMIG)
	{
		float num = NekkiMath.randomFloat();
		return num < KFJGPCLOMIG;
	}

	private void LoadParameters()
	{
		Tactic hBFMBOHLKPJ = parameters.FightTactic;
		if (hBFMBOHLKPJ != null)
		{
			tactic = hBFMBOHLKPJ;
		}
	}

	private void LogTable(ModelAnimation IPNLKNLBLIE, int JLLPJLEDBPG, int count, InfoAnimation DBOLBEOCEME, int JOHDCPNACOC)
	{
	}

	private void LogStart()
	{
	}

	private int GetNearestKeyFrameId(int KKEGODOKGCB)
	{
		if (KKEGODOKGCB % AiData.MovementsStep == 0)
		{
			return KKEGODOKGCB;
		}
		if (0 < KKEGODOKGCB)
		{
			return KKEGODOKGCB - KKEGODOKGCB % AiData.MovementsStep + AiData.MovementsStep;
		}
		return KKEGODOKGCB - KKEGODOKGCB % AiData.MovementsStep;
	}

	private bool IsFitStartAnimation(InfoAnimation DBOLBEOCEME)
	{
		if (_ModelAnimation.GetIsPlaying() && DBOLBEOCEME != null)
		{
			List<string> list = AiData.get_UnexpectedMoves();
			foreach (string item in list)
			{
				if (DBOLBEOCEME.HasName(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsFitBotAnimation(InfoAnimation DBOLBEOCEME, AnimationListType LFLGCDNKNJI = AnimationListType.Standard)
	{
		if (DBOLBEOCEME != null)
		{
			List<string> list = null;
			switch (LFLGCDNKNJI)
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
				if (DBOLBEOCEME.HasName(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsSafeDodges(InfoAnimation DBOLBEOCEME)
	{
		List<string> list = AiData.get_SafeDodgesAnimations();
		foreach (string item in list)
		{
			if (DBOLBEOCEME.HasName(item))
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
			IntervalAnimation mNOIEOBBCMI = _ModelAnimation.FindInterval(item);
			if (mNOIEOBBCMI != null)
			{
				return false;
			}
		}
		if (_ModelAnimation.GetIsPlaying() && botAnimation != null)
		{
			InfoAnimation cGPDPHJIDPA = botAnimation;
			List<string> list2 = AiData.get_NoDecisionMoves();
			foreach (string item2 in list2)
			{
				if (cGPDPHJIDPA.HasName(item2))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private bool IsMissileAnimation(InfoAnimation DBOLBEOCEME)
	{
		List<TemplateAnimation> nNGPIGIMNPD = AiData.get_MissileAnimations();
		return IsGivenTemplateAnimation(DBOLBEOCEME, nNGPIGIMNPD);
	}

	private bool IsMagicAnimation(InfoAnimation DBOLBEOCEME)
	{
		List<TemplateAnimation> nNGPIGIMNPD = AiData.get_MagicAnimations();
		return IsGivenTemplateAnimation(DBOLBEOCEME, nNGPIGIMNPD);
	}

	private bool IsGivenTemplateAnimation(InfoAnimation DBOLBEOCEME, List<TemplateAnimation> NNGPIGIMNPD)
	{
		bool result = false;
		if (DBOLBEOCEME != null)
		{
			foreach (TemplateAnimation item in NNGPIGIMNPD)
			{
				List<InfoAnimation> list = item.GetAnimations();
				if (list.Contains(DBOLBEOCEME))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private bool GetUseChildrenDodge(Model FNKFIMEDNLP, MissileKind LFLGCDNKNJI)
	{
		if (FNKFIMEDNLP.GetWeaponModels().Count == 0 || _Model == null)
		{
			return false;
		}
		bool result = false;
		int num = GetModelDirection(_Model, FNKFIMEDNLP);
		foreach (WeaponModel item in FNKFIMEDNLP.GetWeaponModels())
		{
			if (item.GetAnimationModule() != null && (item.GetAnimationModule() == null || item.GetAnimationModule().GetFirstInfo() != null) && (IsMissileAnimation(item.GetAnimationModule().GetCurrentInfo()) || LFLGCDNKNJI != MissileKind.SimpleMissile) && (IsMagicAnimation(item.GetAnimationModule().GetCurrentInfo()) || LFLGCDNKNJI != MissileKind.MagicMissile))
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

	private DefenceMode SelectDefenceMode(Model FNKFIMEDNLP)
	{
		float num = NekkiMath.randomFloat();
		TacticFactors fJCBLOKOBBD = SetFactors(FNKFIMEDNLP);
		counterAttackChance = tactic.GetCounterAttackChance(fJCBLOKOBBD);
		dodgeChance = tactic.GetDodgeChance(fJCBLOKOBBD);
		blockChance = tactic.GetBlockChance(fJCBLOKOBBD);
		float cJBHLGHFEGC = counterAttackChance;
		if (num < counterAttackChance)
		{
			return DefenceMode.DefenceUseCounterAttack;
		}
		cJBHLGHFEGC += dodgeChance;
		if (num < cJBHLGHFEGC)
		{
			return DefenceMode.DefenceUseDodge;
		}
		cJBHLGHFEGC += blockChance;
		if (num < cJBHLGHFEGC)
		{
			return DefenceMode.DefenceUseBlock;
		}
		return DefenceMode.DefenceUseRandom;
	}

	private float GetNodeX(string IMGCANJHPND, Model ACENLMONNPA, Model FNKFIMEDNLP)
	{
		int aOJJBKLCHJO = GetModelDirection(ACENLMONNPA, FNKFIMEDNLP);
		ModelNode lCDGOCIAIDK = ACENLMONNPA.GetAnimationModule().GetNodeByNameForSign(IMGCANJHPND, aOJJBKLCHJO);
		if (lCDGOCIAIDK != null)
		{
			return lCDGOCIAIDK.GetStart().GetX();
		}
		return float.MaxValue;
	}

	private float GetNodeX(string IMGCANJHPND, List<global::Pair<string, float>> EGMLEFHEBLL)
	{
		foreach (global::Pair<string, float> item in EGMLEFHEBLL)
		{
			if (item.First == IMGCANJHPND)
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

	private int GetPlayableAnimations(List<InfoAnimation> MAHEJFLCCHP, List<int> FIFFFOLGCND = null, bool AEGBKDJEABP = false)
	{
		List<InfoAnimation> list = _Model.GetAvailableAnimations();
		int num = 0;
		int count = MAHEJFLCCHP.Count;
		ModelConditions dGJJDPIAEAO = get_Model().GetConditions();
		if (dGJJDPIAEAO == null)
		{
			GameLog.Error("modelConditions is null");
			return 0;
		}
		dGJJDPIAEAO.IsKeyCheckEnabled = false;
		for (int i = 0; i < count; i++)
		{
			InfoAnimation pJAHIOELGGD = MAHEJFLCCHP[i];
			if (pJAHIOELGGD != null && ((!AEGBKDJEABP) ? IsPlayableAnimations(pJAHIOELGGD) : IsTacticPlayableAnimations(pJAHIOELGGD)))
			{
				MAHEJFLCCHP[num] = MAHEJFLCCHP[i];
				if (FIFFFOLGCND != null)
				{
					FIFFFOLGCND[num] = FIFFFOLGCND[i];
				}
				num++;
			}
		}
		MAHEJFLCCHP.Resize(num);
		if (FIFFFOLGCND != null)
		{
			FIFFFOLGCND.Resize(num);
		}
		return num;
	}

	private int GetPlayableAnimations(List<Decision> PJGOCFKJGJJ)
	{
		List<InfoAnimation> list = _Model.GetAvailableAnimations();
		int num = 0;
		int count = PJGOCFKJGJJ.Count;
		ModelConditions dGJJDPIAEAO = get_Model().GetConditions();
		if (dGJJDPIAEAO == null)
		{
			GameLog.Error("modelConditions is null");
			return 0;
		}
		dGJJDPIAEAO.IsKeyCheckEnabled = false;
		for (int i = 0; i < count; i++)
		{
			InfoAnimation fGICHADOEHF = PJGOCFKJGJJ[i].Animation;
			if (fGICHADOEHF == null || IsPlayableAnimations(fGICHADOEHF))
			{
				PJGOCFKJGJJ[num] = PJGOCFKJGJJ[i];
				num++;
			}
		}
		PJGOCFKJGJJ.Resize(num);
		return num;
	}

	private bool IsPlayableAnimations(InfoAnimation DBOLBEOCEME)
	{
		// AI executes its choices through key input. Event-only animations are
		// started by their runtime events and cannot be selected by this path.
		if (DBOLBEOCEME.GetFirstKeysCondition() == null) return false;
		List<InfoAnimation> list = _Model.GetAvailableAnimations();
		if (!list.Contains(DBOLBEOCEME))
		{
			return false;
		}
		ModelConditions dGJJDPIAEAO = get_Model().GetConditions();
		dGJJDPIAEAO.IsKeyCheckEnabled = false;
		dGJJDPIAEAO.CandidateMoveNames = DBOLBEOCEME.GetTemplateNames();
		dGJJDPIAEAO.AnimationSign = DBOLBEOCEME.GetDirection(dGJJDPIAEAO, _ModelAnimation.GetSign());
		dGJJDPIAEAO.PivotPairSelector = (int)DBOLBEOCEME.MoveData.AlignData.PivotSideKind;
		if (!DBOLBEOCEME.AreConditionsMet(get_Model(), null, DBOLBEOCEME.FindMoveEventByType(EventAnimation.EventAnimationType.EVENT_KEY_PRESSED)))
		{
			return false;
		}
		InfoAnimation.CapabilityTable iCANLHJKKNE = DBOLBEOCEME.PriorityConflicts;
		int count = iCANLHJKKNE.HigherPriorityMoves.Count;
		if (0 < count)
		{
			foreach (InfoAnimation item in iCANLHJKKNE.HigherPriorityMoves)
			{
				if (list.Contains(item) && item.GetFirstKeysCondition() != null)
				{
					dGJJDPIAEAO.CandidateMoveNames = item.GetTemplateNames();
					dGJJDPIAEAO.AnimationSign = item.GetDirection(dGJJDPIAEAO, _ModelAnimation.GetSign());
					dGJJDPIAEAO.PivotPairSelector = (int)item.MoveData.AlignData.PivotSideKind;
					if (item.AreConditionsMet(get_Model(), null, DBOLBEOCEME.FindMoveEventByType(EventAnimation.EventAnimationType.EVENT_KEY_PRESSED)))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool IsTacticPlayableAnimations(InfoAnimation DBOLBEOCEME)
	{
		if (DBOLBEOCEME.GetFirstKeysCondition() == null) return false;
		List<InfoAnimation> list = _Model.GetAvailableAnimations();
		if (!list.Contains(DBOLBEOCEME))
		{
			return false;
		}
		ModelConditions dGJJDPIAEAO = get_Model().GetConditions();
		dGJJDPIAEAO.IsKeyCheckEnabled = false;
		dGJJDPIAEAO.CandidateMoveNames = DBOLBEOCEME.GetTemplateNames();
		dGJJDPIAEAO.AnimationSign = DBOLBEOCEME.GetDirection(dGJJDPIAEAO, _ModelAnimation.GetSign());
		dGJJDPIAEAO.PivotPairSelector = (int)DBOLBEOCEME.MoveData.AlignData.PivotSideKind;
		if (!DBOLBEOCEME.AreConditionsMet(_Model.GetConditions(), DBOLBEOCEME.MoveData.TacticsConditions, DBOLBEOCEME.FindMoveEventByType(EventAnimation.EventAnimationType.EVENT_KEY_PRESSED)))
		{
			return false;
		}
		return true;
	}

	private float GetCurrentDirectionToEnemy(ModelAnimation HFGPAELCNMF)
	{
		ModelNode lCDGOCIAIDK = HFGPAELCNMF.GetPlayingNode();
		ModelNode lCDGOCIAIDK2 = _ModelAnimation.GetPlayingNode();
		if (lCDGOCIAIDK == null || lCDGOCIAIDK2 == null)
		{
			return 0f;
		}
		float num = lCDGOCIAIDK.GetStart().GetX() - lCDGOCIAIDK2.GetStart().GetX();
		if (num >= 0f)
		{
			return 1f;
		}
		return -1f;
	}

	private bool IsFitCondition(InfoAnimation DBOLBEOCEME, bool EMALNKEEEEN)
	{
		ModelConditions dGJJDPIAEAO = get_Model().GetConditions();
		if (dGJJDPIAEAO == null)
		{
			GameLog.Error("modelConditions is null");
			return false;
		}
		dGJJDPIAEAO.CandidateMoveNames = DBOLBEOCEME.GetTemplateNames();
		dGJJDPIAEAO.AnimationSign = DBOLBEOCEME.GetDirection(dGJJDPIAEAO, _ModelAnimation.GetSign());
		dGJJDPIAEAO.PivotPairSelector = (int)DBOLBEOCEME.MoveData.AlignData.PivotSideKind;
		return DBOLBEOCEME.AreConditionsMet(dGJJDPIAEAO, (!EMALNKEEEEN) ? null : DBOLBEOCEME.MoveData.TacticsConditions);
	}

	private int SetDecisionList(Model FNKFIMEDNLP, int JLLPJLEDBPG)
	{
		decisions.Clear();
		ModelAnimation oJIEPADIEDE = FNKFIMEDNLP.GetAnimationModule();
		if (GetCurrentDirectionToEnemy(oJIEPADIEDE) * (float)oJIEPADIEDE.GetSign() > 0f)
		{
			SetRandomAnimation();
			waitMode = WaitMode.SetWaitAnimationLength;
			return 0;
		}
		bool flag = false;
		if (GetUseChildrenDodge(FNKFIMEDNLP, MissileKind.SimpleMissile) && dodgeMissile)
		{
			int num = GetForDodgeMissiles(FNKFIMEDNLP, MissileKind.SimpleMissile);
			resultSource = AiData.TableType.dodgeTable;
			flag = true;
		}
		if (dodgeMagic && GetUseChildrenDodge(FNKFIMEDNLP, MissileKind.MagicMissile))
		{
			int num2 = GetForDodgeMissiles(FNKFIMEDNLP, MissileKind.MagicMissile);
			resultSource = AiData.TableType.dodgeTable;
			flag = true;
		}
		if (flag)
		{
			return decisions.Count;
		}
		int num3 = oJIEPADIEDE.GetFrameInMove();
		if (responseDelay < num3 && !IsUninteruptIntervalEnd(oJIEPADIEDE))
		{
			if (enemyAnimation.IsIntervalActiveAtInterpolatedFrame("Uninterrupt", enemyFrame))
			{
				if (!IsAttackIntervalEnd(oJIEPADIEDE))
				{
					switch (defenceMode)
					{
					case DefenceMode.DefenceUseCounterAttack:
					{
						int num5 = GetFromTablesMove(FNKFIMEDNLP);
						if (0 < num5)
						{
							resultSource = AiData.TableType.movementsTable;
						}
						if (num5 == 0)
						{
							num5 = GetFromTablesDodge(FNKFIMEDNLP);
							if (0 < num5)
							{
								resultSource = AiData.TableType.dodgeTable;
							}
						}
						return num5;
					}
					case DefenceMode.DefenceUseDodge:
					{
						int num4 = GetFromTablesDodge(FNKFIMEDNLP);
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
					int num6 = GetFromTablesMove(FNKFIMEDNLP);
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
					int num7 = GetFromTablesAttack(FNKFIMEDNLP);
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
					InfoAnimation pJAHIOELGGD = FNKFIMEDNLP.GetAnimationModule().GetCurrentInfo();
					int num8 = 0;
					if (pJAHIOELGGD != null && FNKFIMEDNLP.GetAnimationModule().GetIsPlaying())
					{
						num8 = pJAHIOELGGD.GetLastUninterruptFrame(true) - enemyFrame + 1;
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
			ChanceRoll bHDKGLJIOJD = quickAttackRolls[i];
			if (!bHDKGLJIOJD.Flag)
			{
				continue;
			}
			global::Pair<string, TacticValue> cCKLNOPEKHO = list3[i];
			List<InfoAnimation> list4 = new List<InfoAnimation>();
			AnimationData.AddTemplateAnimations(cCKLNOPEKHO.First, list4);
			foreach (InfoAnimation item3 in list4)
			{
				if (item3 != null && IsPlayableAnimations(item3))
				{
					int jOHDCPNACOC = item3.GetMoveLengthStrict();
					decisions.Add(new Decision(item3, jOHDCPNACOC));
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
			ModelAi pCFGKAFOCDO = FNKFIMEDNLP.GetAi();
			if (pCFGKAFOCDO == null)
			{
				continue;
			}
			foreach (InfoAnimation item4 in list6)
			{
				if (item4 != null && pCFGKAFOCDO.IsPlayableAnimations(item4))
				{
					enemyCanEvade = true;
					break;
				}
			}
		}
		TacticFactors fJCBLOKOBBD = SetFactors(FNKFIMEDNLP);
		float num10 = tactic.GetExpectedWait(botAnimation, fJCBLOKOBBD);
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
				num13 = GetFromTablesAttack(FNKFIMEDNLP);
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

	private static bool IsUninteruptIntervalEnd(ModelAnimation MEKLGEGJPFP)
	{
		if (MEKLGEGJPFP.GetIsPlaying())
		{
			InfoAnimation pJAHIOELGGD = MEKLGEGJPFP.GetCurrentInfo();
			if (pJAHIOELGGD != null)
			{
				int num = pJAHIOELGGD.GetLastUninterruptFrame(false);
				int num2 = MEKLGEGJPFP.GetCurrentFrame();
				if (num2 <= num)
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool IsAttackIntervalEnd(ModelAnimation MEKLGEGJPFP)
	{
		if (MEKLGEGJPFP.GetIsPlaying())
		{
			InfoAnimation pJAHIOELGGD = MEKLGEGJPFP.GetCurrentInfo();
			if (pJAHIOELGGD != null)
			{
				int num = pJAHIOELGGD.GetLastAttackFrame(false);
				int num2 = MEKLGEGJPFP.GetCurrentFrame();
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

	private bool IsThrowingState(InfoAnimation DBOLBEOCEME, int IHICCKAOPKG)
	{
		List<string> nIKHAICFGNM = AiData.get_ThrowableIntervals();
		bool fPMGBALCKPI = true;
		return DBOLBEOCEME.AreIntervalsActiveAtInterpolatedFrame(nIKHAICFGNM, IHICCKAOPKG, fPMGBALCKPI);
	}

	private bool IsModelCanThrow(Model ACENLMONNPA)
	{
		candidateAnimations.Clear();
		List<string> nIKHAICFGNM = AiData.get_Throws();
		AnimationData.AddTemplateAnimationsByNames(nIKHAICFGNM, candidateAnimations);
		int num = candidateAnimations.Count;
		if (0 < num)
		{
			num = ACENLMONNPA.GetAi().GetPlayableAnimations(candidateAnimations);
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

	private int GetSafetyAnimations(Model FNKFIMEDNLP)
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
		InfoAnimation pJAHIOELGGD = FNKFIMEDNLP.GetAnimationModule().GetCurrentInfo();
		decisions.Clear();
		foreach (InfoAnimation item3 in candidateAnimations)
		{
			int num = 0;
			if (pJAHIOELGGD != null && FNKFIMEDNLP.GetAnimationModule().GetIsPlaying())
			{
				int num2 = item3.GetMoveLengthStrict();
				num = pJAHIOELGGD.GetLastUninterruptFrame(true) - enemyFrame + 1;
				if (num2 < num)
				{
					num = num2;
				}
			}
			decisions.Add(new Decision(item3, num));
		}
		return decisions.Count;
	}

	private int GetFromTablesMove(Model FNKFIMEDNLP)
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
			int num = GetFromTablesMove(FNKFIMEDNLP, botAnimation, enemyAnimation, _ModelAnimation.GetStartPosition(), FNKFIMEDNLP.GetAnimationModule().GetStartPosition(), ownFrame, enemyFrame, distanceError);
			if (0 < num)
			{
				num = GetPlayableAnimations(decisions);
			}
			if (0 < num)
			{
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					InfoAnimation fGICHADOEHF = decisions[i].Animation;
					if (fGICHADOEHF == null || TestWall(fGICHADOEHF, _Model, FNKFIMEDNLP))
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

	private int GetFromTablesMove(Model FNKFIMEDNLP, InfoAnimation GJAGBKICAGF, InfoAnimation GPHCOBPINCK, float JPPHFIKBFDM, float KACDJBFHICO, int FIPCJOMEPCJ, int FHDLJPNEOLD, float GLFFEFOFCFE, List<global::Pair<string, float>> PJCNHALDPHO = null)
	{
		decisions.Clear();
		GroupTables kMLMHGLLOHM = null;
		List<global::Pair<List<GroupTables>, string>> list = GPHCOBPINCK.GetTacticGroupTables()[1];
		foreach (global::Pair<List<GroupTables>, string> item in list)
		{
			if (!(botWeaponSubtype == item.Second))
			{
				continue;
			}
			List<GroupTables> lLHEDBIEHAA = item.First;
			foreach (GroupTables item2 in lLHEDBIEHAA)
			{
				if (botWeaponSubtype == item2.GroupLabel)
				{
					kMLMHGLLOHM = item2;
					break;
				}
			}
			break;
		}
		if (kMLMHGLLOHM != null)
		{
			ModelAnimation oJIEPADIEDE = FNKFIMEDNLP.GetAnimationModule();
			int num = oJIEPADIEDE.GetSign();
			if (IsFitBotAnimation(GJAGBKICAGF))
			{
				GroupTables cIHPJCIFLHN = kMLMHGLLOHM;
				string mMJNDPGKNPM = GJAGBKICAGF.GetPivotPartName();
				int num2 = FHDLJPNEOLD - FIPCJOMEPCJ;
				int num3 = GetNearestKeyFrameId(num2);
				int num4 = num3 - num2;
				float oIOMNNFMDOO = (float)num * (JPPHFIKBFDM - KACDJBFHICO) + GLFFEFOFCFE;
				decisions.Clear();
				int num5 = GetRow(cIHPJCIFLHN, mMJNDPGKNPM, num3, oIOMNNFMDOO, decisions);
				foreach (Decision item3 in decisions)
				{
					if (GJAGBKICAGF == item3.Animation)
					{
						int num6 = item3.Wait - FIPCJOMEPCJ + num4;
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
			int num7 = ((FHDLJPNEOLD % AiData.MovementsStep == 0) ? FHDLJPNEOLD : (FHDLJPNEOLD + AiData.MovementsStep - FHDLJPNEOLD % AiData.MovementsStep));
			decisions.Clear();
			int num8 = _ModelAnimation.GetSign();
			int num9 = 0;
			int count = kMLMHGLLOHM.Tables.Count;
			for (int i = 0; i < count; i++)
			{
				string hOGFLOLGGOL = kMLMHGLLOHM.Tables[i].Label;
				float num10 = ((PJCNHALDPHO != null) ? GetNodeX(hOGFLOLGGOL, PJCNHALDPHO) : GetNodeX(hOGFLOLGGOL, _Model, FNKFIMEDNLP));
				float num11 = GJAGBKICAGF.ShiftTable.GetDistance(FHDLJPNEOLD, hOGFLOLGGOL);
				float num12 = GJAGBKICAGF.ShiftTable.GetDistance(num7, hOGFLOLGGOL);
				float num13 = num12 - num11;
				float oIOMNNFMDOO2 = (float)num * (num10 + num13 * (float)num8 - KACDJBFHICO) + GLFFEFOFCFE;
				num9 += GetRow(kMLMHGLLOHM, hOGFLOLGGOL, num7, oIOMNNFMDOO2, decisions);
			}
			if (num7 == FHDLJPNEOLD || num9 == 0)
			{
				return num9;
			}
			decisions.Clear();
			decisions.Add(new Decision(null, num7 - FHDLJPNEOLD));
			return 1;
		}
		return decisions.Count;
	}

	private int GetFromTablesAttack(Model FNKFIMEDNLP)
	{
		decisions.Clear();
		if (enemyFrame % AiData.MovementsStep != 0)
		{
			offKeyFrame = true;
			return 0;
		}
		offKeyFrame = false;
		TacticFactors fJCBLOKOBBD = SetFactors(FNKFIMEDNLP);
		int num = GetEnemyResponseDelay(fJCBLOKOBBD);
		GroupTables kMLMHGLLOHM = null;
		List<global::Pair<List<GroupTables>, string>> list = enemyAnimation.GetTacticGroupTables()[0];
		foreach (global::Pair<List<GroupTables>, string> item in list)
		{
			if (!(botWeaponSubtype == item.Second))
			{
				continue;
			}
			List<GroupTables> lLHEDBIEHAA = item.First;
			foreach (GroupTables item2 in lLHEDBIEHAA)
			{
				if (botWeaponSubtype == item2.GroupLabel)
				{
					kMLMHGLLOHM = item2;
					break;
				}
			}
			break;
		}
		if (kMLMHGLLOHM != null)
		{
			ModelAnimation oJIEPADIEDE = FNKFIMEDNLP.GetAnimationModule();
			int mEHOEEIGCEP = enemyFrame;
			int nJPDFMHHIDE = ownFrame;
			float num2 = oJIEPADIEDE.GetStartPosition();
			int num3 = oJIEPADIEDE.GetSign();
			int count = kMLMHGLLOHM.Tables.Count;
			candidateAnimations.Clear();
			_InterframesList.Clear();
			int num4 = enemyFrame + num;
			for (int i = 0; i < count; i++)
			{
				TacticalTable iCLOAGENLJG = kMLMHGLLOHM.Tables[i];
				int num5 = iCLOAGENLJG.GetArrayIndexByFrameIndex(mEHOEEIGCEP);
				if (-1 >= num5)
				{
					continue;
				}
				float num6 = GetNodeX(iCLOAGENLJG.Label, _Model, FNKFIMEDNLP);
				float oIOMNNFMDOO = (float)num3 * (num6 - num2) + distanceError;
				List<IntervalNew> mFFPCMPGEBK = iCLOAGENLJG.IntervalList[num5].Items;
				foreach (IntervalNew item3 in mFFPCMPGEBK)
				{
					int num7 = item3.GetInterframeByDistance(oIOMNNFMDOO);
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
					InfoAnimation pJAHIOELGGD = candidateAnimations[j];
					if (TestWall(pJAHIOELGGD, _Model, FNKFIMEDNLP))
					{
						if (num9 < j)
						{
							candidateAnimations[num9] = pJAHIOELGGD;
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

	private int GetFromTablesDodge(Model FNKFIMEDNLP, AnimationListType NPIOFGMJDKI = AnimationListType.Standard)
	{
		decisions.Clear();
		int num = 0;
		switch (NPIOFGMJDKI)
		{
		case AnimationListType.Standard:
			num = enemyFrame;
			break;
		case AnimationListType.Missile:
			num = FNKFIMEDNLP.GetAnimationModule().GetRenderTickCount();
			break;
		}
		if (num % AiData.MovementsStep != 0)
		{
			switch (NPIOFGMJDKI)
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
		InfoAnimation pJAHIOELGGD = null;
		switch (NPIOFGMJDKI)
		{
		case AnimationListType.Standard:
			pJAHIOELGGD = FNKFIMEDNLP.GetAnimationModule().GetCurrentInfo();
			break;
		case AnimationListType.Missile:
			pJAHIOELGGD = FNKFIMEDNLP.GetAnimationModule().GetFirstInfo();
			break;
		}
		GroupTables kMLMHGLLOHM = null;
		List<global::Pair<List<GroupTables>, string>> list = pJAHIOELGGD.GetTacticGroupTables()[2];
		// Event-only poses/steps can legitimately have no precomputed dodge table.
		if (list.Count == 0 && NPIOFGMJDKI == AnimationListType.Standard && pJAHIOELGGD.GetFirstKeysCondition() == null)
			return 0;
		if (list.Count == 1)
		{
			List<GroupTables> lLHEDBIEHAA = list[0].First;
			if (lLHEDBIEHAA.Count == 1)
			{
				kMLMHGLLOHM = lLHEDBIEHAA[0];
				if (kMLMHGLLOHM != null)
				{
					List<string> nIKHAICFGNM = null;
					List<string> list2 = null;
					switch (NPIOFGMJDKI)
					{
					case AnimationListType.Standard:
						nIKHAICFGNM = AiData.get_MovesFirstIteration();
						list2 = AiData.get_MovesLastIteration();
						break;
					case AnimationListType.Missile:
						nIKHAICFGNM = AiData.get_MissilesFirstIteration();
						list2 = AiData.get_MissilesLastIteration();
						break;
					}
					ModelAnimation oJIEPADIEDE = FNKFIMEDNLP.GetAnimationModule();
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
							string iCBBNJMLDJH = pJAHIOELGGD2.GetPivotPartName();
							TacticalTable iCLOAGENLJG = kMLMHGLLOHM.GetTacticalTableByLabel(iCBBNJMLDJH);
							if (iCLOAGENLJG != null)
							{
								int num4 = num - ownFrame;
								int num5 = GetNearestKeyFrameId(num4);
								int num6 = num5 - num4;
								int num7 = iCLOAGENLJG.GetArrayIndexByFrameIndex(num5);
								if (-1 < num7)
								{
									float num8 = oJIEPADIEDE.GetStartPosition();
									if (NPIOFGMJDKI == AnimationListType.Missile)
									{
										num8 = oJIEPADIEDE.GetFirstStartPositionX();
									}
									int num9 = oJIEPADIEDE.GetSign();
									float num10 = _ModelAnimation.GetStartPosition();
									float oIOMNNFMDOO = (float)num9 * (num10 - num8) + distanceError;
									Intervals gOOGNIPMCEM = iCLOAGENLJG.IntervalList[num7];
									bool flag2 = false;
									foreach (IntervalNew item2 in gOOGNIPMCEM.Items)
									{
										if (item2.Animation == pJAHIOELGGD2)
										{
											int num11 = item2.GetInterframeByDistance(oIOMNNFMDOO);
											if (0 < num11)
											{
												flag2 = true;
												break;
											}
										}
									}
									if (!flag2)
									{
										int num12 = pJAHIOELGGD.GetLastAttackFrame(true);
										int jOHDCPNACOC = num12 - enemyFrame + 1;
										decisions.Add(new Decision(null, jOHDCPNACOC));
										return decisions.Count;
									}
								}
							}
						}
					}
					if (num % AiData.MovementsStep != 0)
					{
						switch (NPIOFGMJDKI)
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
					AnimationData.AddTemplateAnimationsByNames(nIKHAICFGNM, candidateAnimations);
					int count = candidateAnimations.Count;
					int num14 = 0;
					for (int i = 0; i < count; i++)
					{
						InfoAnimation pJAHIOELGGD4 = candidateAnimations[i];
						if (TestWall(pJAHIOELGGD4, _Model, FNKFIMEDNLP))
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
					float num15 = oJIEPADIEDE.GetStartPosition();
					if (NPIOFGMJDKI == AnimationListType.Missile)
					{
						num15 = oJIEPADIEDE.GetFirstStartPositionX();
					}
					int num16 = oJIEPADIEDE.GetSign();
					foreach (TacticalTable item3 in kMLMHGLLOHM.Tables)
					{
						string hOGFLOLGGOL = item3.Label;
						int num17;
						if (NPIOFGMJDKI == AnimationListType.Missile)
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
							float num20 = GetNodeX(hOGFLOLGGOL, _Model, FNKFIMEDNLP.GetRootModel());
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
					IntervalAnimation mNOIEOBBCMI = oJIEPADIEDE.FindInterval(IntervalAnimation.IntervalType.INTERVAL_UNINTERRUPT);
					InfoAnimation pJAHIOELGGD5 = pJAHIOELGGD;
					bool flag3 = oJIEPADIEDE.GetIsPlaying();
					if (mNOIEOBBCMI != null && pJAHIOELGGD5 != null && flag3)
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
					if (candidateAnimations.Count == 0 && NPIOFGMJDKI == AnimationListType.Standard)
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
						int num26 = pJAHIOELGGD.GetLastAttackFrame(true);
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

	private int GetForDodgeMissiles(Model FNKFIMEDNLP, MissileKind OKIFFDGBGDA)
	{
		int num = 0;
		List<Decision> list = new List<Decision>();
		List<Decision> list2 = new List<Decision>(decisions);
		int i = 0;
		for (int count = FNKFIMEDNLP.GetWeaponModels().Count; i < count; i++)
		{
			WeaponModel gKIANLDJFCH = FNKFIMEDNLP.GetWeaponModels()[i];
			List<Decision> jOJBDADJOAP = new List<Decision>(list);
			if ((IsMissileAnimation(gKIANLDJFCH.GetAnimationModule().GetCurrentInfo()) || OKIFFDGBGDA != MissileKind.SimpleMissile) && (IsMagicAnimation(gKIANLDJFCH.GetAnimationModule().GetCurrentInfo()) || OKIFFDGBGDA != MissileKind.MagicMissile) && gKIANLDJFCH.GetAnimationModule().GetCurrentInfo() != null)
			{
				num = GetFromTablesDodge(gKIANLDJFCH, AnimationListType.Missile);
				list = ((list.Count <= 0) ? new List<Decision>(decisions) : Intersection(jOJBDADJOAP, decisions));
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

	private bool IsSafetyAnimations(InfoAnimation DBOLBEOCEME, Model ACENLMONNPA, int IHICCKAOPKG)
	{
		return false;
	}

	private int GetRow(GroupTables CIHPJCIFLHN, string MMJNDPGKNPM, int FMNGLKIGFNA, float OIOMNNFMDOO, List<Decision> OEMALIFPGPO)
	{
		int count = OEMALIFPGPO.Count;
		TacticalTable iCLOAGENLJG = CIHPJCIFLHN.GetTacticalTableByLabel(MMJNDPGKNPM);
		if (iCLOAGENLJG != null)
		{
			Intervals gOOGNIPMCEM = iCLOAGENLJG.GetFrameByFrameIndex(FMNGLKIGFNA);
			if (gOOGNIPMCEM != null)
			{
				foreach (IntervalNew item in gOOGNIPMCEM.Items)
				{
					int num = item.GetInterframeByDistance(OIOMNNFMDOO);
					if (0 < num)
					{
						OEMALIFPGPO.Add(new Decision(item.Animation, num));
						AddModTacticAlternatives(item.Animation, num, OEMALIFPGPO);
					}
				}
			}
		}
		return OEMALIFPGPO.Count - count;
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

	private int GetModelDirection(Model ACENLMONNPA, Model FNKFIMEDNLP)
	{
		int num = 0;
		if (ACENLMONNPA.GetBodyObject().GetPivotNode() == null || FNKFIMEDNLP.GetBodyObject().GetPivotNode() == null)
		{
			return 0;
		}
		return (ACENLMONNPA.GetBodyObject().GetPivotNode().GetStart()
			.GetX() < FNKFIMEDNLP.GetBodyObject().GetPivotNode().GetStart()
			.GetX()) ? 1 : (-1);
	}

	private List<Decision> Intersection(List<Decision> JOJBDADJOAP, List<Decision> DLADGODCJMD)
	{
		List<Decision> list = new List<Decision>();
		foreach (Decision item in JOJBDADJOAP)
		{
			foreach (Decision item2 in DLADGODCJMD)
			{
				if (item2.Animation == item.Animation)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private bool TestBack(InfoAnimation DBOLBEOCEME, Model ACENLMONNPA, Model FNKFIMEDNLP)
	{
		return true && TestBack(DBOLBEOCEME, _Model, FNKFIMEDNLP, "NPivot", "NPivot");
	}

	private bool TestWall(InfoAnimation DBOLBEOCEME, Model ACENLMONNPA, Model FNKFIMEDNLP)
	{
		bool flag = true;
		Model fNKFIMEDNLP = FNKFIMEDNLP.GetRootModel();
		return flag && TestWall(DBOLBEOCEME, _Model, fNKFIMEDNLP, "NPivot");
	}

	private bool TestBack(InfoAnimation DBOLBEOCEME, Model ACENLMONNPA, Model FNKFIMEDNLP, string name, string ODEADGPBDEM)
	{
		float num = ACENLMONNPA.GetBodyObject().GetNodeByName(name).GetStart()
			.GetX();
		float num2 = FNKFIMEDNLP.GetBodyObject().GetNodeByName(ODEADGPBDEM).GetStart()
			.GetX();
		int num3 = DBOLBEOCEME.GetLastUninterruptFrame(true);
		if (num3 < 0)
		{
			num3 = 0;
		}
		float num4 = DBOLBEOCEME.ShiftTable.GetDistance(num3, name);
		float num5 = num + (float)GetModelDirection(ACENLMONNPA, FNKFIMEDNLP) * num4;
		float num8;
		if (FNKFIMEDNLP.GetAnimationModule().GetIsPlaying())
		{
			InfoAnimation pJAHIOELGGD = FNKFIMEDNLP.GetAnimationModule().GetCurrentInfo();
			int mEHOEEIGCEP = enemyFrame;
			int jAPBDIJOKDJ = mEHOEEIGCEP + num3;
			float num6 = pJAHIOELGGD.ShiftTable.GetDistance(jAPBDIJOKDJ, ODEADGPBDEM);
			float num7 = FNKFIMEDNLP.GetAnimationModule().GetStartPosition();
			num8 = num7 + (float)FNKFIMEDNLP.GetFacingSign() * num6;
		}
		else
		{
			num8 = FNKFIMEDNLP.GetBodyObject().GetNodeByName(ODEADGPBDEM).GetStart()
				.GetX();
		}
		if ((num - num2) * (num5 - num8) < 0f)
		{
			return false;
		}
		return true;
	}

	private bool TestWall(InfoAnimation DBOLBEOCEME, Model ACENLMONNPA, Model FNKFIMEDNLP, string name)
	{
		int num = DBOLBEOCEME.GetLastUninterruptFrame(true);
		if (num < 0)
		{
			num = 0;
		}
		float num2 = ACENLMONNPA.GetBodyObject().GetNodeByName(name).GetStart()
			.GetX();
		float num3 = DBOLBEOCEME.ShiftTable.GetDistance(num, name);
		float num4 = num2 + (float)GetModelDirection(ACENLMONNPA, FNKFIMEDNLP) * num3;
		float num5 = ACENLMONNPA.GetAnimationModule().GetLeftWallX();
		float num6 = ACENLMONNPA.GetAnimationModule().GetRightWallX();
		float num7 = ACENLMONNPA.GetAnimationModule().GetFrontAlignMargin();
		float num8 = ACENLMONNPA.GetAnimationModule().GetBackAlignMargin();
		float num9 = ((!(num8 < num7)) ? num8 : num7);
		if (num4 - num9 < num5 || num6 < num4 + num9)
		{
			return false;
		}
		return true;
	}

	private bool IsIncludeIntervalAttack(InfoAnimation DBOLBEOCEME)
	{
		List<IntervalAnimation> cAANBJEPGAA = DBOLBEOCEME.MoveData.Intervals;
		foreach (IntervalAnimation item in cAANBJEPGAA)
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

	private void SetQuickAttackChances(TacticFactors FJCBLOKOBBD)
	{
		List<global::Pair<string, TacticValue>> list = tactic.get_QuickAttacks();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			ChanceRoll bHDKGLJIOJD = quickAttackRolls[i];
			bHDKGLJIOJD.Chance = list[i].Second.GetValue(FJCBLOKOBBD);
			bHDKGLJIOJD.Flag = bHDKGLJIOJD.Roll < bHDKGLJIOJD.Chance;
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

	private void SetEvadesChances(TacticFactors FJCBLOKOBBD)
	{
		List<global::Pair<string, TacticValue>> list = tactic.get_Evades();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			ChanceRoll bHDKGLJIOJD = evadeRolls[i];
			bHDKGLJIOJD.Chance = list[i].Second.GetValue(FJCBLOKOBBD);
			bHDKGLJIOJD.Flag = bHDKGLJIOJD.Roll < bHDKGLJIOJD.Chance;
		}
	}

	private float GetDistanceToEnemy(Model ACENLMONNPA)
	{
		int aOJJBKLCHJO = GetModelDirection(ACENLMONNPA, ACENLMONNPA.GetCombatTarget().GetRootModel());
		int aOJJBKLCHJO2 = GetModelDirection(ACENLMONNPA.GetCombatTarget().GetRootModel(), ACENLMONNPA);
		ModelNode lCDGOCIAIDK = ACENLMONNPA.GetAnimationModule().GetNodeByNameForSign(AiData.get_DistanceNode(), aOJJBKLCHJO);
		ModelNode lCDGOCIAIDK2 = ACENLMONNPA.GetCombatTarget().GetRootModel().GetAnimationModule()
			.GetNodeByNameForSign(AiData.get_DistanceNode(), aOJJBKLCHJO2);
		if (lCDGOCIAIDK != null && lCDGOCIAIDK2 != null)
		{
			float f = lCDGOCIAIDK.GetStart().GetX() - lCDGOCIAIDK2.GetStart().GetX();
			return Mathf.Abs(f);
		}
		return 0f;
	}

	private TacticFactors SetFactors(Model FNKFIMEDNLP)
	{
		TacticFactors oHKCJDCMOKN = new TacticFactors(_Model.GetModelStats(), _Model.GetNoRangedFlag(), _Model.GetMagicCharges());
		_Model.GetModelStats().GetCountAndDamage(true, enemyAnimation, ref oHKCJDCMOKN.FactorsCount, ref oHKCJDCMOKN.Damage, ref oHKCJDCMOKN.Hits);
		oHKCJDCMOKN.Health = (ObscuredFloat)(_Model.Parameters.GetCurrentLife());
		oHKCJDCMOKN.EnemyHealth = (ObscuredFloat)(FNKFIMEDNLP.Parameters.GetCurrentLife());
		oHKCJDCMOKN.AnimationFrames = FNKFIMEDNLP.GetAnimationModule().GetFrameInMove();
		oHKCJDCMOKN.ChildFrames = ChildMaxModelFrame(FNKFIMEDNLP);
		oHKCJDCMOKN.Distance = GetDistanceToEnemy(_Model);
		oHKCJDCMOKN.CurrentAnimation = _Model.GetAnimationModule().GetCurrentInfo();
		oHKCJDCMOKN.EnemyCurrentAnimation = FNKFIMEDNLP.GetAnimationModule().GetCurrentInfo();
		return oHKCJDCMOKN;
	}
}
