using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class InfoAnimation
{
	public class MirrorNode
	{
		private KeyValuePair<string, string> _Names;

		private bool _Empty;

		public string NodeName
		{
			get
			{
				return GetNodeName();
			}
		}

		public string MirroredNodeName
		{
			get
			{
				return GetMirroredNodeName();
			}
		}

		public bool IsEmpty
		{
			get
			{
				return GetIsEmpty();
			}
		}

		public MirrorNode()
		{
			_Names = new KeyValuePair<string, string>(string.Empty, string.Empty);
			_Empty = true;
		}

		public MirrorNode(string name)
		{
			_Empty = false;
			SetNodeName(name);
		}

		public string GetNodeName()
		{
			return _Names.Key;
		}

		public string GetMirroredNodeName()
		{
			return _Names.Value;
		}

		public bool GetIsEmpty()
		{
			return _Empty;
		}

		public void SetNodeName(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				string text = name;
				int length = name.Length;
				text.Remove(length - 1, 1);
				text += ((name[length - 1] != '1') ? "1" : "2");
				_Names = new KeyValuePair<string, string>(name, text);
				_Empty = false;
			}
			else
			{
				_Names = new KeyValuePair<string, string>(string.Empty, string.Empty);
				_Empty = true;
			}
		}
	}

	public class CapabilityTable
	{
		// best guess for name
		public List<InfoAnimation> HigherPriorityMoves = new List<InfoAnimation>();

		public bool IsThePriority(InfoAnimation DBOLBEOCEME)
		{
			for (int i = 0; i < HigherPriorityMoves.Count; i++)
			{
				if (HigherPriorityMoves[i] == DBOLBEOCEME)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsThePriority(List<InfoAnimation> MAHEJFLCCHP)
		{
			for (int i = 0; i < MAHEJFLCCHP.Count; i++)
			{
				if (!IsThePriority(MAHEJFLCCHP[i]))
				{
					return false;
				}
			}
			return true;
		}
	}

	public class AnimationContainerStruct
	{
		public Vector3[][] Container;

		public string FileName;
	}

	public enum WeaponKind
	{
		WeaponHand = 0,
		WeaponThrowing = 1,
		AnimationMagic = 2
	}

	public enum PivotSide
	{
		PivotNodeNone = 0,
		PivotNodeFront = 1,
		PivotNodeBack = 2
	}

	public enum AnimationKind
	{
		AnimationNone = 0,
		AnimationMove = 1,
		AnimationAttack = 2
	}

	public enum TutorialKind
	{
		TutorialNone = 0,
		TutorialMove = 1,
		TutorialAttack = 2
	}

	public enum AlignObjectType
	{
		ObjectNone = 0,
		ObjectNodes = 1,
		ObjectWall = 2,
		ObjectAnimation = 3,
		ObjectPivot = 4
	}

	public class MovePivot
	{
		public AlignObjectType PivotObjectType;

		public AlignObjectType PositionObjectType;

		public int PivotNodeId = -1;

		public int PivotPairNodeId = -1;

		public int PositionNodeId = -1;

		public int PositionPairNodeId = -1;

		public PivotSide PivotSideKind;

		public bool AlignX;

		public bool AlignY;

		public bool AlignZ;

		public string PivotPart = string.Empty;

		public string PositionPart = string.Empty;

		public string ShiftModelNode = string.Empty;

		public ModelType.ModelTargetType PivotModelType = ModelType.ModelTargetType.MODEL_THIS;

		public ModelType.ModelTargetType PositionModelType = ModelType.ModelTargetType.MODEL_THIS;

		public Vector2f PositionShift = new Vector2f();

		public bool IsExists;
	}

	public class MoveInside
	{
		public class ShopAnimation
		{
			public bool IsExists;

			public bool RunOnStart;

			public string AnimationName = string.Empty;
		}

		public class Direction
		{
			public bool IsExists;

			public DistancePoint FromPoint = new DistancePoint();

			public DistancePoint ToPoint = new DistancePoint();

			public DistancePoint.ImpulseDirection ImpulseMode;

			public static DistancePoint.ImpulseDirection ParseImpulseMode(XmlNode node)
			{
				DistancePoint.ImpulseDirection result = DistancePoint.ImpulseDirection.IMPULSE_NONE;
				if (node != null && node.Name == "Impulse")
				{
					result = ((0 >= XmlUtils.ParseInt(node.Attributes["Reverse"])) ? DistancePoint.ImpulseDirection.IMPULSE_NOT_REVERSE : DistancePoint.ImpulseDirection.IMPULSE_REVERSE);
				}
				return result;
			}

			public int GetDirectionSign(ModelConditions conditions)
			{
				float num = 0f;
				num = ((ImpulseMode == DistancePoint.ImpulseDirection.IMPULSE_NONE) ? (ToPoint.GetX(conditions) - FromPoint.GetX(conditions)) : ((float)((ImpulseMode != DistancePoint.ImpulseDirection.IMPULSE_NOT_REVERSE) ? (conditions.ImpulseX * -1) : conditions.ImpulseX)));
				return (num >= 0f) ? 1 : (-1);
			}
		}

		public List<EventAnimation> Events = new List<EventAnimation>();

		public List<ConditionAnimation> Conditions = new List<ConditionAnimation>();

		public List<ConditionAnimation> TacticsConditions = new List<ConditionAnimation>();

		public List<IntervalAnimation> Intervals = new List<IntervalAnimation>();

		// best guess for name
		public List<ConditionAnimation> Locks = new List<ConditionAnimation>();

		public List<TransitionAnimation> Transitions = new List<TransitionAnimation>();

		public List<ActionAnimation> Actions = new List<ActionAnimation>();

		public ShopAnimation ShopData = new ShopAnimation();

		public MovePivot AlignData = new MovePivot();

		public Direction SetDirectionData = new Direction();

		public void InitIntervals()
		{
			for (int i = 0; i < Intervals.Count; i++)
			{
				Intervals[i].Init();
			}
		}

		public void DetectPivotSide()
		{
			string bLODCIGDJFK = AlignData.PivotPart;
			if (bLODCIGDJFK != null && bLODCIGDJFK.Length > 2)
			{
				int num = bLODCIGDJFK.Length - 1;
				char c = bLODCIGDJFK[num];
				char c2 = bLODCIGDJFK[num - 1];
				if (c == '1' && c2 == '_')
				{
					AlignData.PivotSideKind = PivotSide.PivotNodeFront;
				}
				else if (c == '2' && c2 == '_')
				{
					AlignData.PivotSideKind = PivotSide.PivotNodeBack;
				}
			}
		}

		public EventAnimation FindEventByType(EventAnimation.EventAnimationType LFLGCDNKNJI)
		{
			for (int i = 0; i < Events.Count; i++)
			{
				if (Events[i].Type == LFLGCDNKNJI)
				{
					return Events[i];
				}
			}
			return null;
		}
	}

	protected List<List<global::Pair<List<GroupTables>, string>>> tacticGroupTables;

	public ModelShiftTable ShiftTable = new ModelShiftTable();

	public AnimationKind Type;

	public TutorialKind TutorialType;

	public int MidFrames;

	public int Priority;

	public int Rank;

	public int Id;

	// best guess for name
	public int FirstFrame;

	// best guess for name
	public int AnimationEndFrame;

	public string Name;

	public string FileName;

	// best guess for name
	public MoveInside MoveData;

	// best guess for name
	public List<ConditionAnimation> SelectionConditions => MoveData.Conditions;

	// best guess for name
	public List<ActionAnimation> ScheduledActions => MoveData.Actions;


	public bool NoWallRepulsion;

	public bool HasPhysics;

	public bool ShowInTricks;

	public bool EndsStage;

	public bool NoInterpolationFrames;

	public float StyleFactor;

	public bool AlignOnParentWallCollision;

	private List<string> _TemplateNames = new List<string>();

	private List<int> _Delays;

	private int _NodesCount;

	private List<string> tacticWeapons = new List<string>();

	private Vector3f velocity;

	private Vector3f acceleration;

	private bool saveVelocity;

	private bool isLooped;

	private DistancePoint rotationPosition;

	private float rotationAngle;

	private bool noMagicRecharge;

	private StageType.Stage cameraStage;

	private MirrorNode mirrorNode = new MirrorNode();

	private Vector3[][] _AnimationContainer;

	// best guess for name
	public CapabilityTable PriorityConflicts = new CapabilityTable();

	private static readonly List<AnimationContainerStruct> animationCache = new List<AnimationContainerStruct>();

	private InfoAnimation _TacticEquivalent;

	public List<List<global::Pair<List<GroupTables>, string>>> TacticGroupTables
	{
		get
		{
			return GetTacticGroupTables();
		}
	}

	public int NodesCount
	{
		get
		{
			return GetNodesCount();
		}
	}

	public Vector3[][] AnimationFrames
	{
		get
		{
			return GetAnimationFrames();
		}
	}

	public string PivotPartName
	{
		get
		{
			return GetPivotPartName();
		}
	}

	public List<IntervalAnimation> AllIntervals
	{
		get
		{
			return GetAllIntervals();
		}
	}

	public List<string> TemplateNames
	{
		get
		{
			return GetTemplateNames();
		}
	}

	public int FrameCount
	{
		get
		{
			return GetFrameCount();
		}
	}

	public uint TotalFramesUnsigned
	{
		get
		{
			return GetTotalFramesUnsigned();
		}
	}

	public ConditionKeys FirstKeysCondition
	{
		get
		{
			return GetFirstKeysCondition();
		}
	}

	public List<ConditionKeys> KeyConditions
	{
		get
		{
			return CollectKeyConditions();
		}
	}

	public InfoAnimation TacticEquivalentAnimation
	{
		get
		{
			return GetTacticEquivalent();
		}
		set
		{
			set_TacticEquivalent(value);
		}
	}

	public List<string> TacticWeapons
	{
		get
		{
			return GetTacticWeapons();
		}
	}

	public bool IsSecondHeelPivot
	{
		get
		{
			return GetIsSecondHeelPivot();
		}
	}

	public bool IsFirstHeelPivot
	{
		get
		{
			return GetIsFirstHeelPivot();
		}
	}

	public Vector3f Velocity
	{
		get
		{
			return GetVelocity();
		}
		set
		{
			SetVelocity(value);
		}
	}

	public Vector3f Acceleration
	{
		get
		{
			return GetAcceleration();
		}
		set
		{
			SetAcceleration(value);
		}
	}

	public bool SaveVelocity
	{
		get
		{
			return GetSaveVelocity();
		}
		set
		{
			SetSaveVelocity(value);
		}
	}

	public DistancePoint RotationPosition
	{
		get
		{
			return GetRotationPosition();
		}
		set
		{
			SetRotationPosition(value);
		}
	}

	public bool NoMagicRecharge
	{
		get
		{
			return GetNoMagicRecharge();
		}
		set
		{
			SetNoMagicRecharge(value);
		}
	}

	public float RotationAngleValue
	{
		get
		{
			return GetRotationAngle();
		}
		set
		{
			set_RotationAngle(value);
		}
	}

	public int TotalFrames
	{
		get
		{
			return GetTotalFrames();
		}
	}

	public int BlendFrameCount
	{
		get
		{
			return GetBlendFrameCount();
		}
	}

	public bool IsLooped
	{
		get
		{
			return GetIsLooped();
		}
		set
		{
			SetIsLooped(value);
		}
	}

	public StageType.Stage CameraStage
	{
		get
		{
			return GetCameraStage();
		}
		set
		{
			SetCameraStage(value);
		}
	}

	public MirrorNode MirrorNodeInfo
	{
		get
		{
			return GetMirrorNode();
		}
		set
		{
			SetMirrorNode(value);
		}
	}

	public Vector3[] FirstFrameNodes
	{
		get
		{
			return GetFirstFrameNodes();
		}
	}

	public static int CachedAnimationCount
	{
		get
		{
			return GetCachedAnimationCount();
		}
	}

	private AnimationContainerStruct CachedContainer
	{
		get
		{
			return FindCachedContainer();
		}
	}

	public InfoAnimation()
	{
		Type = AnimationKind.AnimationNone;
		TutorialType = TutorialKind.TutorialNone;
		MidFrames = 0;
		Priority = 0;
		Id = 0;
		FirstFrame = 0;
		AnimationEndFrame = 0;
		HasPhysics = false;
		EndsStage = false;
		NoInterpolationFrames = false;
		ShowInTricks = true;
		_TacticEquivalent = null;
		saveVelocity = false;
		cameraStage = StageType.Stage.STAGE_NONE;
		noMagicRecharge = false;
		MoveData = new MoveInside();
		NoWallRepulsion = false;
		_NodesCount = 0;
		_AnimationContainer = null;
		isLooped = false;
		rotationAngle = 0f;
		Rank = 0;
		MoveData.AlignData = new MovePivot();
		MoveData.AlignData.IsExists = false;
		MoveData.AlignData.PivotNodeId = -1;
		MoveData.SetDirectionData.IsExists = false;
	}

	public List<List<global::Pair<List<GroupTables>, string>>> GetTacticGroupTables()
	{
		if (tacticGroupTables == null)
		{
			tacticGroupTables = new List<List<global::Pair<List<GroupTables>, string>>>();
			for (int i = 0; i < 3; i++)
			{
				tacticGroupTables.Add(new List<global::Pair<List<GroupTables>, string>>());
			}
		}
		return tacticGroupTables;
	}

	public int GetNodesCount()
	{
		return _NodesCount;
	}

	public Vector3[][] GetAnimationFrames()
	{
		return _AnimationContainer;
	}

	public void SetCurrentNode()
	{
	}

	public void Init()
	{
		MoveData.InitIntervals();
		MoveData.DetectPivotSide();
		if (_AnimationContainer != null && _AnimationContainer.Length > 0)
		{
			_NodesCount = _AnimationContainer[0].Length;
		}
	}

	public bool CheckLockAnimation(string JKBPMGDJIJC, bool LPGLCGMMPHN = true)
	{
		return true;
	}

	public string GetPivotPartName()
	{
		if (MoveData == null)
		{
			GameLog.Error("moveInside is null");
			return string.Empty;
		}
		return MoveData.AlignData.PivotPart;
	}

	public void GetIntervals(int frame, List<IntervalAnimation> NKHPLNBJKLI, List<IntervalAnimation> HLMKBLOHJGC, HashSet<IntervalAnimation.IntervalType> FGBOFDJKLJI = null)
	{
		NKHPLNBJKLI.Clear();
		HLMKBLOHJGC.Clear();
		foreach (IntervalAnimation item in MoveData.Intervals)
		{
			int num = ((item.Start < FirstFrame) ? FirstFrame : item.Start);
			int num2 = ((item.EndFrameValue > AnimationEndFrame) ? AnimationEndFrame : item.EndFrameValue);
			if (num <= frame && frame <= num2)
			{
				if (FGBOFDJKLJI == null || !FGBOFDJKLJI.Contains(item.Type))
				{
					NKHPLNBJKLI.Add(item);
				}
			}
			else if (frame - 1 == num2 && (FGBOFDJKLJI == null || !FGBOFDJKLJI.Contains(item.Type)))
			{
				HLMKBLOHJGC.Add(item);
			}
		}
	}

	public List<IntervalAnimation> GetAllIntervals()
	{
		return MoveData.Intervals;
	}

	public bool HasEvent(EventAnimation p_event)
	{
		foreach (EventAnimation item in MoveData.Events)
		{
			if (item.IsEqual(p_event))
			{
				return true;
			}
		}
		return false;
	}

	public bool AreConditionsMet(ModelConditions conditions, List<ConditionAnimation> JPGMNIFICDM = null, EventAnimation DOANBADPBGH = null)
	{
		List<ConditionAnimation> list = ((JPGMNIFICDM == null) ? MoveData.Conditions : JPGMNIFICDM);
		if (DOANBADPBGH != null)
		{
			conditions.CurrentEvent = DOANBADPBGH;
			DOANBADPBGH.Conditions = conditions;
		}
		foreach (ConditionAnimation item in list)
		{
			if (!item.IsEqual(conditions))
			{
				return false;
			}
		}
		return true;
	}

	public bool AreConditionsMet(Model ACENLMONNPA, List<ConditionAnimation> JPGMNIFICDM = null, EventAnimation DOANBADPBGH = null)
	{
		List<ConditionAnimation> list = ((JPGMNIFICDM == null) ? MoveData.Conditions : JPGMNIFICDM);
		for (int i = 0; i < list.Count; i++)
		{
			ConditionAnimation iIDOLPHMOGA = list[i];
			ModelType.ModelTargetType kEIDBIOIFGA = iIDOLPHMOGA.GetTargetModelType();
			Model fGCODGKLHED = iIDOLPHMOGA.ResolveTargetModel(ACENLMONNPA, kEIDBIOIFGA);
			if (fGCODGKLHED == null)
			{
				return false;
			}
			ModelConditions dGJJDPIAEAO = fGCODGKLHED.GetConditions();
			if (DOANBADPBGH != null)
			{
				dGJJDPIAEAO.CurrentEvent = DOANBADPBGH;
				DOANBADPBGH.Conditions = dGJJDPIAEAO;
			}
			iIDOLPHMOGA.ApplyTargetModelType(ModelType.ModelTargetType.MODEL_THIS);
			bool flag = false;
			if (iIDOLPHMOGA.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = iIDOLPHMOGA as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					flag = eLFKOGJJNMN.EvaluateWithModel(ACENLMONNPA.GetConditions(), ACENLMONNPA, DOANBADPBGH);
				}
			}
			else
			{
				flag = iIDOLPHMOGA.IsEqual(fGCODGKLHED, this);
			}
			if (!flag)
			{
				iIDOLPHMOGA.SetTargetModelType(kEIDBIOIFGA);
				return false;
			}
			iIDOLPHMOGA.SetTargetModelType(kEIDBIOIFGA);
		}
		return true;
	}

	public void SwapNodePairs(List<global::Pair<int, int>> HMKIJOIJNJD, KeyFrames GCDAKGKMJHF, int index)
	{
		if (0 >= HMKIJOIJNJD.Count || index >= GCDAKGKMJHF.GetSize())
		{
			return;
		}
		int dGILPMANFAF = GCDAKGKMJHF.GetFrame(index).Size;
		Vector3f eMAFACPEPDK = new Vector3f();
		for (int i = index; i < GCDAKGKMJHF.GetSize(); i++)
		{
			for (int j = 0; j < HMKIJOIJNJD.Count; j++)
			{
				if (HMKIJOIJNJD[j].First < dGILPMANFAF && HMKIJOIJNJD[j].Second < dGILPMANFAF)
				{
					eMAFACPEPDK.Set(GCDAKGKMJHF.GetFrame(i).Data[HMKIJOIJNJD[j].First]);
					GCDAKGKMJHF.GetFrame(i).Data[HMKIJOIJNJD[j].First].Set(GCDAKGKMJHF.GetFrame(i).Data[HMKIJOIJNJD[j].Second]);
					GCDAKGKMJHF.GetFrame(i).Data[HMKIJOIJNJD[j].Second].Set(eMAFACPEPDK);
				}
			}
		}
	}

	public void FillKeyFrames(KeyFrames frames, int NHEIOIBOPHN, bool HOHEFHKJIOG)
	{
		int num = ((NHEIOIBOPHN <= -1) ? FirstFrame : NHEIOIBOPHN);
		int num2 = _AnimationContainer[num].Length;
		frames.SetFramesFromRange(num, AnimationEndFrame, HOHEFHKJIOG, _AnimationContainer);
	}

	public void LoadAnimationClip()
	{
		if (!string.IsNullOrEmpty(FileName))
		{
			AnimationContainerStruct aGAMDIHPFPF = FindCachedContainer();
			if (aGAMDIHPFPF == null || aGAMDIHPFPF.Container == null)
			{
				string iFKJHHPJPLP = Eclipse.Modding.ModAssetBinding.IsQualified(FileName) ?
					FileName : SF2Paths.GetBinaryAnimationsPath() + "/" + FileName;
				LoadAnimationBinary(iFKJHHPJPLP);
				if (_AnimationContainer != null)
				{
					AddToAnimationCache();
				}
			}
			else
			{
				ApplyCachedContainer(aGAMDIHPFPF);
			}
		}
	}

	// Used by guarded content patches after the native parser has already loaded
	// the original clip. Keep the parsed move identity and its linked actions.
	public void ReplaceClip(string fileName, int endFrame)
	{
		string oldFileName = FileName;
		int oldEndFrame = AnimationEndFrame;
		int oldNodesCount = _NodesCount;
		Vector3[][] oldContainer = _AnimationContainer;
		try
		{
			FileName = fileName;
			AnimationEndFrame = endFrame;
			_AnimationContainer = null;
			_NodesCount = 0;
			// Reload directly: the parser's static cache is keyed only by filename,
			// so an Apply & Restart with changed mod bytes must not reuse old frames.
			string path = Eclipse.Modding.ModAssetBinding.IsQualified(fileName) ?
				fileName : SF2Paths.GetBinaryAnimationsPath() + "/" + fileName;
			LoadAnimationBinary(path);
			if (_AnimationContainer == null || _AnimationContainer.Length == 0 ||
				FirstFrame < 0 || FirstFrame >= _AnimationContainer.Length)
				throw new System.InvalidOperationException("Replacement animation clip is missing or incompatible: " + fileName);
			_NodesCount = _AnimationContainer[FirstFrame].Length;
		}
		catch
		{
			FileName = oldFileName;
			AnimationEndFrame = oldEndFrame;
			_NodesCount = oldNodesCount;
			_AnimationContainer = oldContainer;
			throw;
		}
	}

	public void AddTemplateName(string name)
	{
		_TemplateNames.AddIfNotExist(name);
	}

	public void AddDelay(int value)
	{
		_Delays.Add(value);
	}

	public void MergeMoveData(MoveInside KECIIKEIJBH)
	{
		if (MoveData != null)
		{
			AddEvents(KECIIKEIJBH.Events);
			AddTacticsConditions(KECIIKEIJBH.TacticsConditions);
			AddConditions(KECIIKEIJBH.Conditions);
			AddIntervals(KECIIKEIJBH.Intervals);
			AddLocks(KECIIKEIJBH.Locks);
			AddTransitions(KECIIKEIJBH.Transitions);
			AddActions(KECIIKEIJBH.Actions);
			if (!MoveData.ShopData.IsExists && KECIIKEIJBH.ShopData.IsExists)
			{
				MoveData.ShopData = KECIIKEIJBH.ShopData;
			}
			if (!MoveData.AlignData.IsExists && KECIIKEIJBH.AlignData.IsExists)
			{
				MoveData.AlignData = KECIIKEIJBH.AlignData;
			}
			if (!MoveData.SetDirectionData.IsExists && KECIIKEIJBH.SetDirectionData.IsExists)
			{
				MoveData.SetDirectionData = KECIIKEIJBH.SetDirectionData;
			}
		}
	}

	public void AddLocks(List<ConditionAnimation> value)
	{
		MoveData.Locks.AddRange(value);
	}

	public void AddTransitions(List<TransitionAnimation> value)
	{
		MoveData.Transitions.AddRange(value);
	}

	public void AddActions(List<ActionAnimation> value)
	{
		MoveData.Actions.AddRange(value);
	}

	public void AddEvents(List<EventAnimation> value)
	{
		MoveData.Events.AddRange(value);
	}

	public void AddTacticsConditions(List<ConditionAnimation> value)
	{
		MoveData.TacticsConditions.AddRange(value);
	}

	public void AddConditions(List<ConditionAnimation> value)
	{
		MoveData.Conditions.AddRange(value);
	}

	public void AddIntervals(List<IntervalAnimation> value)
	{
		foreach (IntervalAnimation item in value)
		{
			item.set_AnimationFinishFrame(AnimationEndFrame);
		}
		MoveData.Intervals.AddRange(value);
	}

	// best guess for name
	private void LoadAnimationBinary(string path)
	{
		byte[] array = ResourceManager.GetBinary(path);
		if (array != null && array.Length > 0)
		{
			ReadAnimation(array);
			return;
		}
		GameLog.Error("File {0} not found", path);
	}

	private void ReadAnimation(byte[] data)
	{
		using (BinaryReaderNekki pHAPKCOJMHL = new BinaryReaderNekki(data))
		{
			int num = pHAPKCOJMHL.ReadInt32();
			_AnimationContainer = new Vector3[num][];
			for (int i = 0; i < num; i++)
			{
				pHAPKCOJMHL.ReadByte();
				int num2 = pHAPKCOJMHL.ReadInt32();
				_AnimationContainer[i] = new Vector3[num2];
				for (int j = 0; j < num2; j++)
				{
					_AnimationContainer[i][j] = new Vector3(pHAPKCOJMHL.ReadSingle(), 0f - pHAPKCOJMHL.ReadSingle(), pHAPKCOJMHL.ReadSingle());
				}
			}
			if (AnimationEndFrame == 0)
			{
				AnimationEndFrame = num - 1;
			}
		}
	}

	private void ApplyCachedContainer(AnimationContainerStruct EIJNHOPFLGI)
	{
		_AnimationContainer = EIJNHOPFLGI.Container;
		int num = _AnimationContainer.Length;
		if (AnimationEndFrame == 0)
		{
			AnimationEndFrame = num - 1;
		}
	}

	public void UpdateModelObjects(ModelObject OECPEDPMKCD, bool EKBOGDKIHIH, bool PHADJMAONJG, ModelObject MJCGOJBGFIE = null)
	{
		ModelNode aECCPADGGPG = null;
		if (MoveData.AlignData.PivotNodeId > -1 && MoveData.AlignData.PivotNodeId < OECPEDPMKCD.GetPlainNodes().Count)
		{
			aECCPADGGPG = OECPEDPMKCD.GetPlainNodes()[MoveData.AlignData.PivotNodeId];
		}
		UpdateConditions(MoveData.Conditions, OECPEDPMKCD, EKBOGDKIHIH, PHADJMAONJG, MJCGOJBGFIE, aECCPADGGPG);
		if (0 < MoveData.TacticsConditions.Count)
		{
			UpdateConditions(MoveData.TacticsConditions, OECPEDPMKCD, EKBOGDKIHIH, PHADJMAONJG, MJCGOJBGFIE, aECCPADGGPG);
		}
		MoveData.SetDirectionData.FromPoint.UpdateNode(OECPEDPMKCD, EKBOGDKIHIH, null, PHADJMAONJG, MJCGOJBGFIE);
		MoveData.SetDirectionData.ToPoint.UpdateNode(OECPEDPMKCD, EKBOGDKIHIH, null, PHADJMAONJG, MJCGOJBGFIE);
		if (rotationAngle != 0f)
		{
			rotationPosition.UpdateNode(OECPEDPMKCD, EKBOGDKIHIH, null, PHADJMAONJG, MJCGOJBGFIE);
		}
		foreach (ActionAnimation item in MoveData.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect jFJGGMEJDPG = (ActionEffect)item;
				jFJGGMEJDPG.UpdateNodes(OECPEDPMKCD, EKBOGDKIHIH, null, PHADJMAONJG, MJCGOJBGFIE);
			}
		}
	}

	private void UpdateConditions(List<ConditionAnimation> conditions, ModelObject OECPEDPMKCD, bool EKBOGDKIHIH, bool PHADJMAONJG, ModelObject MJCGOJBGFIE, ModelNode AECCPADGGPG)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.DISTANCE)
			{
				ConditionDistance jNPIBKBDJAN = item as ConditionDistance;
				if (jNPIBKBDJAN != null)
				{
					jNPIBKBDJAN.UpdateNodes(OECPEDPMKCD, EKBOGDKIHIH, AECCPADGGPG, PHADJMAONJG, MJCGOJBGFIE);
				}
				else
				{
					GameLog.Error("subcondition is null");
				}
			}
			if (item.Type == ConditionAnimation.ConditionType.DIRECTION)
			{
				ConditionDirection cFCGJLJBOKI = item as ConditionDirection;
				if (cFCGJLJBOKI != null)
				{
					cFCGJLJBOKI.UpdateNodes(OECPEDPMKCD, EKBOGDKIHIH, AECCPADGGPG, PHADJMAONJG, MJCGOJBGFIE);
				}
				else
				{
					GameLog.Error("subcondition is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					List<ConditionAnimation> kDOGKKGDOBK = eLFKOGJJNMN.GetConditions();
					UpdateConditions(kDOGKKGDOBK, OECPEDPMKCD, EKBOGDKIHIH, PHADJMAONJG, MJCGOJBGFIE, AECCPADGGPG);
				}
				else
				{
					GameLog.Error("subconditions is null");
				}
			}
		}
	}

	private void ResetConditions(List<ConditionAnimation> AIDMEPEKEOL)
	{
		foreach (ConditionAnimation item in AIDMEPEKEOL)
		{
			if (item.Type == ConditionAnimation.ConditionType.DISTANCE)
			{
				ConditionDistance jNPIBKBDJAN = item as ConditionDistance;
				if (jNPIBKBDJAN != null)
				{
					jNPIBKBDJAN.ResetNodes();
				}
				else
				{
					GameLog.Error("conditionDistance is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.DIRECTION)
			{
				ConditionDirection cFCGJLJBOKI = item as ConditionDirection;
				if (cFCGJLJBOKI != null)
				{
					cFCGJLJBOKI.ResetNodes();
				}
				else
				{
					GameLog.Error("conditionDistance is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					List<ConditionAnimation> aIDMEPEKEOL = eLFKOGJJNMN.GetConditions();
					ResetConditions(aIDMEPEKEOL);
				}
				else
				{
					GameLog.Error("conditions is null");
				}
			}
		}
	}

	public void ResetModelBindings()
	{
		ResetConditions(MoveData.Conditions);
		ResetConditions(MoveData.TacticsConditions);
		foreach (ActionAnimation item in MoveData.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect jFJGGMEJDPG = (ActionEffect)item;
				jFJGGMEJDPG.ResetNodes();
			}
		}
		MoveData.SetDirectionData.FromPoint.ClearChildPoints();
		MoveData.SetDirectionData.ToPoint.ClearChildPoints();
		if (rotationPosition != null)
		{
			rotationPosition.ClearChildPoints();
		}
	}

	public List<string> GetTemplateNames()
	{
		return _TemplateNames;
	}

	public int GetDirection(ModelConditions conditions, int CLHNIJGMKBH)
	{
		return (!MoveData.SetDirectionData.IsExists) ? CLHNIJGMKBH : MoveData.SetDirectionData.GetDirectionSign(conditions);
	}

	public int GetFrameCount()
	{
		return AnimationEndFrame - FirstFrame + 1;
	}

	public uint GetTotalFramesUnsigned()
	{
		return (uint)(GetFrameCount() * (MidFrames + 1));
	}

	public int GetLastAttackFrame(bool NPEIEAHIDKH)
	{
		int num = 0;
		foreach (IntervalAnimation item in MoveData.Intervals)
		{
			if (item.Type == IntervalAnimation.IntervalType.INTERVAL_ATTACK && num < item.EndFrameValue)
			{
				num = item.EndFrameValue;
			}
		}
		if (NPEIEAHIDKH)
		{
			num = ToInterpolatedFrame(num + 1) - 1;
		}
		return num;
	}

	public bool IsItemRequired(string LMNNBBKHMEI, string OCOEFJAMFCG)
	{
		List<ConditionAnimation> hIFPHBNGIPO = MoveData.Locks;
		foreach (ConditionAnimation item in hIFPHBNGIPO)
		{
			if (item.Type == ConditionAnimation.ConditionType.ITEM && !item.IsNot)
			{
				ConditionItemInfo kOOGCJOEANH = item as ConditionItemInfo;
				if (LMNNBBKHMEI == kOOGCJOEANH.get_Type() && OCOEFJAMFCG == kOOGCJOEANH.GetSubType())
				{
					return true;
				}
			}
			else
			{
				if (item.Type != ConditionAnimation.ConditionType.LIST || item.IsNot)
				{
					continue;
				}
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN.get_Type() != ConditionList.OperatorType.OR)
				{
					GameLog.Error(string.Empty);
					continue;
				}
				List<ConditionAnimation> list = eLFKOGJJNMN.GetConditions();
				foreach (ConditionAnimation item2 in list)
				{
					if (item2.Type == ConditionAnimation.ConditionType.ITEM && !item2.IsNot)
					{
						ConditionItemInfo kOOGCJOEANH2 = item2 as ConditionItemInfo;
						if (LMNNBBKHMEI == kOOGCJOEANH2.get_Type() && OCOEFJAMFCG == kOOGCJOEANH2.GetSubType())
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public int GetMoveLength(List<string> NFLDEGMEJAK)
	{
		int num = 0;
		foreach (IntervalAnimation item in MoveData.Intervals)
		{
			if (NFLDEGMEJAK.Contains(item.Name) && num < item.EndFrameValue)
			{
				num = item.EndFrameValue;
			}
		}
		if (0 < num)
		{
			return ToInterpolatedFrame(num + 1);
		}
		return 0;
	}

	public int GetMoveLengthStrict()
	{
		List<string> nFLDEGMEJAK = AiData.get_MoveLengthIntervalsStrict();
		return GetMoveLength(nFLDEGMEJAK);
	}

	public int GetMoveLengthExtended()
	{
		List<string> nFLDEGMEJAK = AiData.get_MoveLengthIntervalsExtended();
		return GetMoveLength(nFLDEGMEJAK);
	}

	public ConditionKeys GetFirstKeysCondition()
	{
		return FindKeysCondition(MoveData.Conditions);
	}

	private static ConditionKeys FindKeysCondition(List<ConditionAnimation> conditions)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					List<ConditionAnimation> kDOGKKGDOBK = eLFKOGJJNMN.GetConditions();
					ConditionKeys bHDEBDIHDFM = FindKeysCondition(kDOGKKGDOBK);
					if (bHDEBDIHDFM != null)
					{
						return bHDEBDIHDFM;
					}
				}
				else
				{
					GameLog.Error("conditionList is null");
				}
			}
			else
			{
				ConditionKeys bHDEBDIHDFM2 = AsKeysCondition(item);
				if (bHDEBDIHDFM2 != null)
				{
					return bHDEBDIHDFM2;
				}
			}
		}
		return null;
	}

	// best guess for name
	public List<ConditionKeys> CollectKeyConditions()
	{
		List<ConditionKeys> list = new List<ConditionKeys>();
		CollectKeysConditions(MoveData.Conditions, list);
		return list;
	}

	private static void CollectKeysConditions(List<ConditionAnimation> conditions, List<ConditionKeys> GKHEPKGMEFI)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					List<ConditionAnimation> kDOGKKGDOBK = eLFKOGJJNMN.GetConditions();
					CollectKeysConditions(kDOGKKGDOBK, GKHEPKGMEFI);
				}
				else
				{
					GameLog.Error("conditionList is null");
				}
			}
			else
			{
				ConditionKeys bHDEBDIHDFM = AsKeysCondition(item);
				if (bHDEBDIHDFM != null)
				{
					GKHEPKGMEFI.Add(bHDEBDIHDFM);
				}
			}
		}
	}

	public static ConditionKeys AsKeysCondition(ConditionAnimation IOFGGOCEIAM)
	{
		if (IOFGGOCEIAM.Type == ConditionAnimation.ConditionType.KEYS)
		{
			return IOFGGOCEIAM as ConditionKeys;
		}
		return null;
	}

	public bool HasName(string name)
	{
		return Name == name || HasTemplateName(name);
	}

	public bool HasTemplateName(string IJBOAGICOON)
	{
		foreach (string item in _TemplateNames)
		{
			if (item == IJBOAGICOON)
			{
				return true;
			}
		}
		return false;
	}

	public static bool ComparePrioritets(InfoAnimation LHBNIMGFKIB, InfoAnimation AAOIAEJJINO)
	{
		return LHBNIMGFKIB.Priority < AAOIAEJJINO.Priority;
	}

	public bool CheckAnimationName(List<string> IPFMIJKPABH)
	{
		foreach (string item in _TemplateNames)
		{
			foreach (string item2 in IPFMIJKPABH)
			{
				if (item == item2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public InfoAnimation GetTacticEquivalent()
	{
		return _TacticEquivalent;
	}

	public void set_TacticEquivalent(InfoAnimation value)
	{
		if (this == value)
		{
			GameLog.Error("this animation == tactic equivalent for {0}", Name);
		}
		_TacticEquivalent = value;
	}

	public List<string> GetTacticWeapons()
	{
		return tacticWeapons;
	}

	public void SetTacticWeapons(string INFFOHGHLNG)
	{
		tacticWeapons.Clear();
		if (INFFOHGHLNG != null)
		{
			tacticWeapons.AddRange(INFFOHGHLNG.Split('|'));
		}
	}

	public bool GetIsSecondHeelPivot()
	{
		return MoveData.AlignData.PivotPart == "NHeel_2";
	}

	public bool GetIsFirstHeelPivot()
	{
		return MoveData.AlignData.PivotPart == "NHeel_1";
	}

	public int GetLastUninterruptFrame(bool NPEIEAHIDKH)
	{
		int num = 0;
		foreach (IntervalAnimation item in MoveData.Intervals)
		{
			if ("Uninterrupt" == item.Name && num < item.EndFrameValue)
			{
				num = item.EndFrameValue;
			}
		}
		int lHHAGECFIOL = AnimationEndFrame;
		if (lHHAGECFIOL < num)
		{
			num = lHHAGECFIOL;
		}
		if (NPEIEAHIDKH)
		{
			num = ToInterpolatedFrame(num + 1) - 1;
		}
		return num;
	}

	public int ToInterpolatedFrame(int frame)
	{
		return (frame - FirstFrame + 1) * (MidFrames + 1) + 1;
	}

	public int FromInterpolatedFrame(int IHICCKAOPKG)
	{
		return FirstFrame - 1 + (IHICCKAOPKG - 1) / (MidFrames + 1);
	}

	public void SetVelocity(Vector3f value)
	{
		velocity = value;
	}

	public Vector3f GetVelocity()
	{
		return velocity;
	}

	public void SetAcceleration(Vector3f value)
	{
		acceleration = value;
	}

	public Vector3f GetAcceleration()
	{
		return acceleration;
	}

	public void SetSaveVelocity(bool value)
	{
		saveVelocity = value;
	}

	public bool GetSaveVelocity()
	{
		return saveVelocity;
	}

	public void SetRotationPosition(DistancePoint value)
	{
		rotationPosition = value;
	}

	public DistancePoint GetRotationPosition()
	{
		return rotationPosition;
	}

	public void SetNoMagicRecharge(bool value)
	{
		noMagicRecharge = value;
	}

	public bool GetNoMagicRecharge()
	{
		return noMagicRecharge;
	}

	public void set_RotationAngle(float value)
	{
		rotationAngle = value;
	}

	public float GetRotationAngle()
	{
		return rotationAngle;
	}

	public bool IsIntervalActive(string name, int frame)
	{
		foreach (IntervalAnimation item in MoveData.Intervals)
		{
			if (item.Name == name && item.Start <= frame && frame <= item.EndFrameValue)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsIntervalActiveAtInterpolatedFrame(string name, int IHICCKAOPKG)
	{
		int dBEDGEMEFNB = FromInterpolatedFrame(IHICCKAOPKG);
		return IsIntervalActive(name, dBEDGEMEFNB);
	}

	public bool AreIntervalsActive(List<string> NIKHAICFGNM, int frame, bool FPMGBALCKPI)
	{
		if (FPMGBALCKPI)
		{
			foreach (string item in NIKHAICFGNM)
			{
				bool flag = false;
				foreach (IntervalAnimation item2 in MoveData.Intervals)
				{
					if (item2.Name == item && item2.Start <= frame && frame <= item2.EndFrameValue)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}
		foreach (IntervalAnimation item3 in MoveData.Intervals)
		{
			if (NIKHAICFGNM.Contains(item3.Name) && item3.Start <= frame && frame <= item3.EndFrameValue)
			{
				return true;
			}
		}
		return false;
	}

	public bool AreIntervalsActiveAtInterpolatedFrame(List<string> NIKHAICFGNM, int IHICCKAOPKG, bool FPMGBALCKPI)
	{
		int dBEDGEMEFNB = FromInterpolatedFrame(IHICCKAOPKG);
		return AreIntervalsActive(NIKHAICFGNM, dBEDGEMEFNB, FPMGBALCKPI);
	}

	public EventAnimation FindMoveEventByType(EventAnimation.EventAnimationType LFLGCDNKNJI)
	{
		if (MoveData != null)
		{
			return MoveData.FindEventByType(LFLGCDNKNJI);
		}
		return null;
	}

	public int GetTotalFrames()
	{
		return (MidFrames + 1) * GetFrameCount();
	}

	public int GetBlendFrameCount()
	{
		return 2 * (MidFrames + 1);
	}

	public void PreloadEffects()
	{
		string text = "Textures/Effects/Magic/";
		foreach (ActionAnimation item in MoveData.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect jFJGGMEJDPG = (ActionEffect)item;
				string oNNKJLOGHGH = text + jFJGGMEJDPG.GetSequence();
				LocationSpriteCache.LoadAtlasSprites(oNNKJLOGHGH);
			}
		}
	}

	public void PreloadSounds()
	{
		foreach (ActionAnimation item in MoveData.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.SOUND)
			{
				ActionSound nMLKJLJHCIA = (ActionSound)item;
				Sound.LoadSound(nMLKJLJHCIA.get_Name());
			}
		}
	}

	public bool GetIsLooped()
	{
		return isLooped;
	}

	public void SetIsLooped(bool value)
	{
		isLooped = value;
	}

	public StageType.Stage GetCameraStage()
	{
		return cameraStage;
	}

	public void SetCameraStage(StageType.Stage value)
	{
		cameraStage = value;
	}

	public MirrorNode GetMirrorNode()
	{
		return mirrorNode;
	}

	public void SetMirrorNode(MirrorNode value)
	{
		mirrorNode = value;
	}

	public Vector3[] GetFirstFrameNodes()
	{
		return _AnimationContainer[FirstFrame];
	}

	private void AddToAnimationCache()
	{
		if (_AnimationContainer == null)
		{
			return;
		}
		AnimationContainerStruct aGAMDIHPFPF = new AnimationContainerStruct();
		aGAMDIHPFPF.FileName = FileName;
		aGAMDIHPFPF.Container = _AnimationContainer;
		animationCache.Add(aGAMDIHPFPF);
	}

	public static void ClearAnimationCache()
	{
		foreach (AnimationContainerStruct item in animationCache)
		{
			item.Container = null;
		}
		animationCache.Clear();
	}

	public static int GetCachedAnimationCount()
	{
		return animationCache.Count;
	}

	private AnimationContainerStruct FindCachedContainer()
	{
		return animationCache.FirstOrDefault((AnimationContainerStruct EIJNHOPFLGI) => FileName == EIJNHOPFLGI.FileName);
	}

	public override string ToString()
	{
		return "InfoAnim: Name: " + Name;
	}
}
