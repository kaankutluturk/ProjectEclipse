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

		public bool IsThePriority(InfoAnimation move)
		{
			for (int i = 0; i < HigherPriorityMoves.Count; i++)
			{
				if (HigherPriorityMoves[i] == move)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsThePriority(List<InfoAnimation> moves)
		{
			for (int i = 0; i < moves.Count; i++)
			{
				if (!IsThePriority(moves[i]))
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
			string pivotPart = AlignData.PivotPart;
			if (pivotPart != null && pivotPart.Length > 2)
			{
				int num = pivotPart.Length - 1;
				char c = pivotPart[num];
				char c2 = pivotPart[num - 1];
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

		public EventAnimation FindEventByType(EventAnimation.EventAnimationType eventType)
		{
			for (int i = 0; i < Events.Count; i++)
			{
				if (Events[i].Type == eventType)
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

	// Eclipse: playback rate in permille (1000 = authored speed). Keyframe-attached
	// timing (intervals, actions, events) follows automatically; tick-based helpers
	// below convert through Eclipse.Runtime.PlaybackTiming.
	public int PlaybackRatePermille = Eclipse.Runtime.PlaybackTiming.Normal;

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

	public bool CheckLockAnimation(string animationName, bool lockFlag = true)
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

	public void GetIntervals(int frame, List<IntervalAnimation> activeIntervals, List<IntervalAnimation> endedIntervals, HashSet<IntervalAnimation.IntervalType> excludedTypes = null)
	{
		activeIntervals.Clear();
		endedIntervals.Clear();
		foreach (IntervalAnimation item in MoveData.Intervals)
		{
			int num = ((item.Start < FirstFrame) ? FirstFrame : item.Start);
			int num2 = ((item.EndFrameValue > AnimationEndFrame) ? AnimationEndFrame : item.EndFrameValue);
			if (num <= frame && frame <= num2)
			{
				if (excludedTypes == null || !excludedTypes.Contains(item.Type))
				{
					activeIntervals.Add(item);
				}
			}
			else if (frame - 1 == num2 && (excludedTypes == null || !excludedTypes.Contains(item.Type)))
			{
				endedIntervals.Add(item);
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

	public bool AreConditionsMet(ModelConditions conditions, List<ConditionAnimation> conditionOverride = null, EventAnimation currentEvent = null)
	{
		List<ConditionAnimation> list = ((conditionOverride == null) ? MoveData.Conditions : conditionOverride);
		if (currentEvent != null)
		{
			conditions.CurrentEvent = currentEvent;
			currentEvent.Conditions = conditions;
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

	public bool AreConditionsMet(Model model, List<ConditionAnimation> conditionOverride = null, EventAnimation currentEvent = null)
	{
		List<ConditionAnimation> list = ((conditionOverride == null) ? MoveData.Conditions : conditionOverride);
		for (int i = 0; i < list.Count; i++)
		{
			ConditionAnimation condition = list[i];
			ModelType.ModelTargetType originalTargetType = condition.GetTargetModelType();
			Model targetModel = condition.ResolveTargetModel(model, originalTargetType);
			if (targetModel == null)
			{
				return false;
			}
			ModelConditions modelConditions = targetModel.GetConditions();
			if (currentEvent != null)
			{
				modelConditions.CurrentEvent = currentEvent;
				currentEvent.Conditions = modelConditions;
			}
			condition.ApplyTargetModelType(ModelType.ModelTargetType.MODEL_THIS);
			bool flag = false;
			if (condition.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList conditionList = condition as ConditionList;
				if (conditionList != null)
				{
					flag = conditionList.EvaluateWithModel(model.GetConditions(), model, currentEvent);
				}
			}
			else
			{
				flag = condition.IsEqual(targetModel, this);
			}
			if (!flag)
			{
				condition.SetTargetModelType(originalTargetType);
				return false;
			}
			condition.SetTargetModelType(originalTargetType);
		}
		return true;
	}

	public void SwapNodePairs(List<global::Pair<int, int>> swapPairs, KeyFrames keyFrames, int index)
	{
		if (0 >= swapPairs.Count || index >= keyFrames.GetSize())
		{
			return;
		}
		int nodeCount = keyFrames.GetFrame(index).Size;
		Vector3f swapBuffer = new Vector3f();
		for (int i = index; i < keyFrames.GetSize(); i++)
		{
			for (int j = 0; j < swapPairs.Count; j++)
			{
				if (swapPairs[j].First < nodeCount && swapPairs[j].Second < nodeCount)
				{
					swapBuffer.Set(keyFrames.GetFrame(i).Data[swapPairs[j].First]);
					keyFrames.GetFrame(i).Data[swapPairs[j].First].Set(keyFrames.GetFrame(i).Data[swapPairs[j].Second]);
					keyFrames.GetFrame(i).Data[swapPairs[j].Second].Set(swapBuffer);
				}
			}
		}
	}

	public void FillKeyFrames(KeyFrames frames, int startFrame, bool repeatFirstFrame)
	{
		int num = ((startFrame <= -1) ? FirstFrame : startFrame);
		int num2 = _AnimationContainer[num].Length;
		frames.SetFramesFromRange(num, AnimationEndFrame, repeatFirstFrame, _AnimationContainer);
	}

	public void LoadAnimationClip()
	{
		if (!string.IsNullOrEmpty(FileName))
		{
			AnimationContainerStruct cachedContainer = FindCachedContainer();
			if (cachedContainer == null || cachedContainer.Container == null)
			{
				string clipPath = Eclipse.Modding.ModAssetBinding.IsQualified(FileName) ?
					FileName : SF2Paths.GetBinaryAnimationsPath() + "/" + FileName;
				LoadAnimationBinary(clipPath);
				if (_AnimationContainer != null)
				{
					AddToAnimationCache();
				}
			}
			else
			{
				ApplyCachedContainer(cachedContainer);
			}
		}
	}

	// Used by guarded content patches after the native parser has already loaded
	// the original clip. Keep the parsed move identity and its linked actions.
	/// <summary>Keyframes in a native clip file (the Moveset Lab's clip picker), or -1 when it is missing.</summary>
	internal static int ReadClipFrameCount(string fileName)
	{
		byte[] data = ResourceManager.GetBinary(SF2Paths.GetBinaryAnimationsPath() + "/" + fileName);
		return data != null && data.Length >= 4 ? System.BitConverter.ToInt32(data, 0) : -1;
	}

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

	public void MergeMoveData(MoveInside sourceMove)
	{
		if (MoveData != null)
		{
			AddEvents(sourceMove.Events);
			AddTacticsConditions(sourceMove.TacticsConditions);
			AddConditions(sourceMove.Conditions);
			AddIntervals(sourceMove.Intervals);
			AddLocks(sourceMove.Locks);
			AddTransitions(sourceMove.Transitions);
			AddActions(sourceMove.Actions);
			if (!MoveData.ShopData.IsExists && sourceMove.ShopData.IsExists)
			{
				MoveData.ShopData = sourceMove.ShopData;
			}
			if (!MoveData.AlignData.IsExists && sourceMove.AlignData.IsExists)
			{
				MoveData.AlignData = sourceMove.AlignData;
			}
			if (!MoveData.SetDirectionData.IsExists && sourceMove.SetDirectionData.IsExists)
			{
				MoveData.SetDirectionData = sourceMove.SetDirectionData;
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
		using (BinaryReaderNekki reader = new BinaryReaderNekki(data))
		{
			int num = reader.ReadInt32();
			_AnimationContainer = new Vector3[num][];
			for (int i = 0; i < num; i++)
			{
				reader.ReadByte();
				int num2 = reader.ReadInt32();
				_AnimationContainer[i] = new Vector3[num2];
				for (int j = 0; j < num2; j++)
				{
					_AnimationContainer[i][j] = new Vector3(reader.ReadSingle(), 0f - reader.ReadSingle(), reader.ReadSingle());
				}
			}
			if (AnimationEndFrame == 0)
			{
				AnimationEndFrame = num - 1;
			}
		}
	}

	private void ApplyCachedContainer(AnimationContainerStruct cachedContainer)
	{
		_AnimationContainer = cachedContainer.Container;
		int num = _AnimationContainer.Length;
		if (AnimationEndFrame == 0)
		{
			AnimationEndFrame = num - 1;
		}
	}

	public void UpdateModelObjects(ModelObject modelObject, bool isPlayer, bool useChildPoints, ModelObject ownerObject = null)
	{
		ModelNode pivotNode = null;
		if (MoveData.AlignData.PivotNodeId > -1 && MoveData.AlignData.PivotNodeId < modelObject.GetPlainNodes().Count)
		{
			pivotNode = modelObject.GetPlainNodes()[MoveData.AlignData.PivotNodeId];
		}
		UpdateConditions(MoveData.Conditions, modelObject, isPlayer, useChildPoints, ownerObject, pivotNode);
		if (0 < MoveData.TacticsConditions.Count)
		{
			UpdateConditions(MoveData.TacticsConditions, modelObject, isPlayer, useChildPoints, ownerObject, pivotNode);
		}
		MoveData.SetDirectionData.FromPoint.UpdateNode(modelObject, isPlayer, null, useChildPoints, ownerObject);
		MoveData.SetDirectionData.ToPoint.UpdateNode(modelObject, isPlayer, null, useChildPoints, ownerObject);
		if (rotationAngle != 0f)
		{
			rotationPosition.UpdateNode(modelObject, isPlayer, null, useChildPoints, ownerObject);
		}
		foreach (ActionAnimation item in MoveData.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect effect = (ActionEffect)item;
				effect.UpdateNodes(modelObject, isPlayer, null, useChildPoints, ownerObject);
			}
		}
	}

	private void UpdateConditions(List<ConditionAnimation> conditions, ModelObject modelObject, bool isPlayer, bool useChildPoints, ModelObject ownerObject, ModelNode pivotNode)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.DISTANCE)
			{
				ConditionDistance distanceCondition = item as ConditionDistance;
				if (distanceCondition != null)
				{
					distanceCondition.UpdateNodes(modelObject, isPlayer, pivotNode, useChildPoints, ownerObject);
				}
				else
				{
					GameLog.Error("subcondition is null");
				}
			}
			if (item.Type == ConditionAnimation.ConditionType.DIRECTION)
			{
				ConditionDirection directionCondition = item as ConditionDirection;
				if (directionCondition != null)
				{
					directionCondition.UpdateNodes(modelObject, isPlayer, pivotNode, useChildPoints, ownerObject);
				}
				else
				{
					GameLog.Error("subcondition is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList conditionList = item as ConditionList;
				if (conditionList != null)
				{
					List<ConditionAnimation> nestedConditions = conditionList.GetConditions();
					UpdateConditions(nestedConditions, modelObject, isPlayer, useChildPoints, ownerObject, pivotNode);
				}
				else
				{
					GameLog.Error("subconditions is null");
				}
			}
		}
	}

	private void ResetConditions(List<ConditionAnimation> conditions)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.DISTANCE)
			{
				ConditionDistance distanceCondition = item as ConditionDistance;
				if (distanceCondition != null)
				{
					distanceCondition.ResetNodes();
				}
				else
				{
					GameLog.Error("conditionDistance is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.DIRECTION)
			{
				ConditionDirection directionCondition = item as ConditionDirection;
				if (directionCondition != null)
				{
					directionCondition.ResetNodes();
				}
				else
				{
					GameLog.Error("conditionDistance is null");
				}
			}
			else if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList conditionList = item as ConditionList;
				if (conditionList != null)
				{
					List<ConditionAnimation> nestedConditions = conditionList.GetConditions();
					ResetConditions(nestedConditions);
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
				ActionEffect effect = (ActionEffect)item;
				effect.ResetNodes();
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

	public int GetDirection(ModelConditions conditions, int defaultDirection)
	{
		return (!MoveData.SetDirectionData.IsExists) ? defaultDirection : MoveData.SetDirectionData.GetDirectionSign(conditions);
	}

	public int GetFrameCount()
	{
		return AnimationEndFrame - FirstFrame + 1;
	}

	public uint GetTotalFramesUnsigned()
	{
		return (uint)GetTotalFrames();
	}

	public int GetLastAttackFrame(bool interpolated)
	{
		int num = 0;
		foreach (IntervalAnimation item in MoveData.Intervals)
		{
			if (item.Type == IntervalAnimation.IntervalType.INTERVAL_ATTACK && num < item.EndFrameValue)
			{
				num = item.EndFrameValue;
			}
		}
		if (interpolated)
		{
			num = ToInterpolatedFrame(num + 1) - 1;
		}
		return num;
	}

	public bool IsItemRequired(string itemType, string itemSubType)
	{
		List<ConditionAnimation> conditions = MoveData.Locks;
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.ITEM && !item.IsNot)
			{
				ConditionItemInfo itemCondition = item as ConditionItemInfo;
				if (itemType == itemCondition.get_Type() && itemSubType == itemCondition.GetSubType())
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
				ConditionList conditionList = item as ConditionList;
				if (conditionList.get_Type() != ConditionList.OperatorType.OR)
				{
					GameLog.Error(string.Empty);
					continue;
				}
				List<ConditionAnimation> list = conditionList.GetConditions();
				foreach (ConditionAnimation item2 in list)
				{
					if (item2.Type == ConditionAnimation.ConditionType.ITEM && !item2.IsNot)
					{
						ConditionItemInfo requiredItem = item2 as ConditionItemInfo;
						if (itemType == requiredItem.get_Type() && itemSubType == requiredItem.GetSubType())
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public int GetMoveLength(List<string> intervalNames)
	{
		int num = 0;
		foreach (IntervalAnimation item in MoveData.Intervals)
		{
			if (intervalNames.Contains(item.Name) && num < item.EndFrameValue)
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
		List<string> intervalNames = AiData.get_MoveLengthIntervalsStrict();
		return GetMoveLength(intervalNames);
	}

	public int GetMoveLengthExtended()
	{
		List<string> intervalNames = AiData.get_MoveLengthIntervalsExtended();
		return GetMoveLength(intervalNames);
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
				ConditionList conditionList = item as ConditionList;
				if (conditionList != null)
				{
					List<ConditionAnimation> nestedConditions = conditionList.GetConditions();
					ConditionKeys keysCondition = FindKeysCondition(nestedConditions);
					if (keysCondition != null)
					{
						return keysCondition;
					}
				}
				else
				{
					GameLog.Error("conditionList is null");
				}
			}
			else
			{
				ConditionKeys keysCondition = AsKeysCondition(item);
				if (keysCondition != null)
				{
					return keysCondition;
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

	private static void CollectKeysConditions(List<ConditionAnimation> conditions, List<ConditionKeys> keysConditions)
	{
		foreach (ConditionAnimation item in conditions)
		{
			if (item.Type == ConditionAnimation.ConditionType.LIST)
			{
				ConditionList conditionList = item as ConditionList;
				if (conditionList != null)
				{
					List<ConditionAnimation> nestedConditions = conditionList.GetConditions();
					CollectKeysConditions(nestedConditions, keysConditions);
				}
				else
				{
					GameLog.Error("conditionList is null");
				}
			}
			else
			{
				ConditionKeys keysCondition = AsKeysCondition(item);
				if (keysCondition != null)
				{
					keysConditions.Add(keysCondition);
				}
			}
		}
	}

	public static ConditionKeys AsKeysCondition(ConditionAnimation condition)
	{
		if (condition.Type == ConditionAnimation.ConditionType.KEYS)
		{
			return condition as ConditionKeys;
		}
		return null;
	}

	public bool HasName(string name)
	{
		return Name == name || HasTemplateName(name);
	}

	public bool HasTemplateName(string templateName)
	{
		foreach (string item in _TemplateNames)
		{
			if (item == templateName)
			{
				return true;
			}
		}
		return false;
	}

	public static bool ComparePrioritets(InfoAnimation first, InfoAnimation second)
	{
		return first.Priority < second.Priority;
	}

	public bool CheckAnimationName(List<string> names)
	{
		foreach (string item in _TemplateNames)
		{
			foreach (string item2 in names)
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

	public void SetTacticWeapons(string weapons)
	{
		tacticWeapons.Clear();
		if (weapons != null)
		{
			tacticWeapons.AddRange(weapons.Split('|'));
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

	public int GetLastUninterruptFrame(bool interpolated)
	{
		int num = 0;
		foreach (IntervalAnimation item in MoveData.Intervals)
		{
			if ("Uninterrupt" == item.Name && num < item.EndFrameValue)
			{
				num = item.EndFrameValue;
			}
		}
		int endFrame = AnimationEndFrame;
		if (endFrame < num)
		{
			num = endFrame;
		}
		if (interpolated)
		{
			num = ToInterpolatedFrame(num + 1) - 1;
		}
		return num;
	}

	public int ToInterpolatedFrame(int frame)
	{
		return Eclipse.Runtime.PlaybackTiming.TicksBefore(MidFrames + 1, frame - FirstFrame + 1, PlaybackRatePermille) + 1;
	}

	public int FromInterpolatedFrame(int interpolatedFrame)
	{
		return FirstFrame - 1 + Eclipse.Runtime.PlaybackTiming.SegmentAt(MidFrames + 1, interpolatedFrame - 1, PlaybackRatePermille);
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

	public bool IsIntervalActiveAtInterpolatedFrame(string name, int interpolatedFrame)
	{
		int frame = FromInterpolatedFrame(interpolatedFrame);
		return IsIntervalActive(name, frame);
	}

	public bool AreIntervalsActive(List<string> intervalNames, int frame, bool requireAll)
	{
		if (requireAll)
		{
			foreach (string item in intervalNames)
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
			if (intervalNames.Contains(item3.Name) && item3.Start <= frame && frame <= item3.EndFrameValue)
			{
				return true;
			}
		}
		return false;
	}

	public bool AreIntervalsActiveAtInterpolatedFrame(List<string> intervalNames, int interpolatedFrame, bool requireAll)
	{
		int frame = FromInterpolatedFrame(interpolatedFrame);
		return AreIntervalsActive(intervalNames, frame, requireAll);
	}

	public EventAnimation FindMoveEventByType(EventAnimation.EventAnimationType eventType)
	{
		if (MoveData != null)
		{
			return MoveData.FindEventByType(eventType);
		}
		return null;
	}

	public int GetTotalFrames()
	{
		return Eclipse.Runtime.PlaybackTiming.TicksBefore(MidFrames + 1, GetFrameCount(), PlaybackRatePermille);
	}

	public int GetBlendFrameCount()
	{
		return Eclipse.Runtime.PlaybackTiming.TicksBefore(MidFrames + 1, 2, PlaybackRatePermille);
	}

	public void PreloadEffects()
	{
		string text = "Textures/Effects/Magic/";
		foreach (ActionAnimation item in MoveData.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.EFFECT)
			{
				ActionEffect effect = (ActionEffect)item;
				string atlasPath = text + effect.GetSequence();
				LocationSpriteCache.LoadAtlasSprites(atlasPath);
			}
		}
	}

	public void PreloadSounds()
	{
		foreach (ActionAnimation item in MoveData.Actions)
		{
			if (item.get_Type() == ActionAnimation.ActionType.SOUND)
			{
				ActionSound sound = (ActionSound)item;
				Sound.LoadSound(sound.get_Name());
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
		AnimationContainerStruct cacheEntry = new AnimationContainerStruct();
		cacheEntry.FileName = FileName;
		cacheEntry.Container = _AnimationContainer;
		animationCache.Add(cacheEntry);
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
		return animationCache.FirstOrDefault((AnimationContainerStruct container) => FileName == container.FileName);
	}

	public override string ToString()
	{
		return "InfoAnim: Name: " + Name;
	}
}
