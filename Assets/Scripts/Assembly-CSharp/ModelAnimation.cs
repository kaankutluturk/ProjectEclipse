using System.Collections.Generic;
using UnityEngine;

public class ModelAnimation : global::EventDispatcher<object>
{
	public enum AnimationEventType
	{
		ON_START_ANIMATION_EVENT = 0,
		ON_STOP_ANIMATION_EVENT = 1,
		ON_START_INTERVAL_EVENT = 2,
		ON_STOP_INTERVAL_EVENT = 3,
		ON_ACTION_START = 4
	}

	private ModelAnimation parentAnimation;

	private ModelAnimation otherAnimation;

	private HashSet<IntervalAnimation.IntervalType> intervalTypeFilter;

	private bool isPlaying;

	private bool pendingFinalUpdate;

	private bool frameChangedPending;

	private bool bufferReachedKeyFrame;

	private int sign;

	private int currentNodeId;

	private ModelNode currentNode;

	private int startFrame;

	private int endFrame;

	private int unusedInt;

	private int frameCounter;

	private int frameCursor;

	private int bufferState;

	private int frameInMove;

	private int lastProcessedFrame;

	private bool isMirrored;

	private bool isLooping;

	private bool useInterruptFrames;

	private bool hasParentModel;

	private float pivotTargetX;

	private float startPositionX;

	private float firstStartPositionX;

	private int renderTickCount;

	private bool firstPositionCaptured;

	private float shiftWallDelta;

	private Vector3f shift = new Vector3f();

	private Vector3f accumulatedOffset = new Vector3f();

	private Vector3f velocity = new Vector3f();

	private Vector3f acceleration = new Vector3f();

	private float subFrameScale;

	private int startFrameOffset;

	private int frontAlignMargin;

	private int backAlignMargin;

	private ModelObject _Model;

	private float leftWallX;

	private float rightWallX;

	private KeyFrames _Frames = new KeyFrames();

	private List<List<Vector3f>> bufferedFrames = new List<List<Vector3f>>();

	private int bufferIndex;

	private int interpolationSteps;

	private List<IntervalAnimation> activeIntervals = new List<IntervalAnimation>();

	private List<IntervalAnimation> endedIntervals = new List<IntervalAnimation>();

	private InfoAnimation currentInfo;

	private InfoAnimation firstInfo;

	private List<ModelEdge> attackingEdges = new List<ModelEdge>();

	public ModelNode Heel1Node;

	public ModelNode Heel2Node;

	public ModelNode UnusedNode;

	public InfoAnimation RequiredInfo;

	public ModelAnimation ParentAnimation
	{
		get
		{
			return GetParentAnimation();
		}
		set
		{
			SetParentAnimation(value);
		}
	}

	public ModelAnimation OtherAnimation
	{
		get
		{
			return GetOtherAnimation();
		}
		set
		{
			SetOtherAnimation(value);
		}
	}

	public bool IsPlaying
	{
		get
		{
			return GetIsPlaying();
		}
	}

	public int FacingSign
	{
		get
		{
			return GetSign();
		}
		set
		{
			set_Sign(value);
		}
	}

	public int CurrentNodeId
	{
		get
		{
			return GetCurrentNodeId();
		}
	}

	public ModelNode CurrentNode
	{
		get
		{
			return GetCurrentNode();
		}
	}

	public int StartFrame
	{
		get
		{
			return GetStartFrame();
		}
	}

	public int EndFrame
	{
		get
		{
			return GetEndFrame();
		}
	}

	public bool IsMirrored
	{
		get
		{
			return GetIsMirrored();
		}
	}

	public float StartPosition
	{
		get
		{
			return GetStartPosition();
		}
	}

	public float FirstStartPositionX
	{
		get
		{
			return GetFirstStartPositionX();
		}
	}

	public int RenderTickCount
	{
		get
		{
			return GetRenderTickCount();
		}
	}

	public float WallShiftDelta
	{
		get
		{
			return GetShiftWallDelta();
		}
		set
		{
			set_ShiftWallDelta(value);
		}
	}

	public Vector3f Shift
	{
		get
		{
			return GetShift();
		}
		set
		{
			SetShift(value);
		}
	}

	public int StartFrameOffset
	{
		get
		{
			return GetStartFrameOffset();
		}
	}

	public int FrontAlignMargin
	{
		get
		{
			return GetFrontAlignMargin();
		}
	}

	public int BackAlignMargin
	{
		get
		{
			return GetBackAlignMargin();
		}
	}

	public ModelObject OwnerModel
	{
		get
		{
			return get_Model();
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

	public List<IntervalAnimation> ActiveIntervals
	{
		get
		{
			return GetActiveIntervals();
		}
	}

	public InfoAnimation CurrentInfo
	{
		get
		{
			return GetCurrentInfo();
		}
		set
		{
			SetCurrentInfo(value);
		}
	}

	public InfoAnimation FirstInfo
	{
		get
		{
			return GetFirstInfo();
		}
	}

	public List<ModelEdge> AttackingEdges
	{
		get
		{
			return GetAttackingEdges();
		}
	}

	public int CurrentFrame
	{
		get
		{
			return GetCurrentFrame();
		}
	}

	public int PhysicsFrame
	{
		get
		{
			return GetPhysicsFrame();
		}
	}

	public int ReactionFrame
	{
		get
		{
			return GetReactionFrame();
		}
	}

	public int PlayedFrame
	{
		get
		{
			return GetPlayedFrame();
		}
	}

	public int FrameInMove
	{
		get
		{
			return GetFrameInMove();
		}
	}

	public int FrameStep
	{
		get
		{
			return GetFrameStep();
		}
	}

	public ModelNode PlayingNode
	{
		get
		{
			return GetPlayingNode();
		}
	}

	public ModelAnimation(ModelObject modelObject)
	{
		Heel1Node = null;
		Heel2Node = null;
		otherAnimation = null;
		parentAnimation = null;
		bufferState = -3;
		_Model = modelObject;
		isPlaying = false;
		sign = 1;
		currentNodeId = 0;
		currentNode = null;
		startFrame = 0;
		frameCounter = 0;
		frameCursor = 0;
		lastProcessedFrame = int.MaxValue;
		startPositionX = 0f;
		currentInfo = null;
		firstInfo = null;
		isMirrored = false;
		frontAlignMargin = 0;
		backAlignMargin = 0;
		pendingFinalUpdate = false;
		RequiredInfo = null;
		intervalTypeFilter = null;
		startFrameOffset = 0;
		isLooping = false;
		hasParentModel = modelObject.GetModel().GetParentModel() != null;
		subFrameScale = 1f;
		shiftWallDelta = 0f;
		frameChangedPending = false;
		bufferReachedKeyFrame = false;
		firstStartPositionX = 0f;
		renderTickCount = 0;
		firstPositionCaptured = false;
		Stop();
	}

	public ModelAnimation GetParentAnimation()
	{
		return parentAnimation;
	}

	public void SetParentAnimation(ModelAnimation value)
	{
		parentAnimation = value;
	}

	public ModelAnimation GetOtherAnimation()
	{
		return otherAnimation;
	}

	public void SetOtherAnimation(ModelAnimation value)
	{
		otherAnimation = value;
	}

	public bool GetIsPlaying()
	{
		return isPlaying;
	}

	public int GetSign()
	{
		return sign;
	}

	public void set_Sign(int value)
	{
		if (value < 0)
		{
			sign = -1;
			return;
		}
		if (value > 0)
		{
			sign = 1;
			return;
		}
		sign = 1;
		Debug.LogError("set sign value != -1 or 1");
	}

	public int GetCurrentNodeId()
	{
		return currentNodeId;
	}

	public ModelNode GetCurrentNode()
	{
		return currentNode;
	}

	public int GetStartFrame()
	{
		return startFrame;
	}

	public int GetEndFrame()
	{
		return endFrame;
	}

	public bool GetIsMirrored()
	{
		return isMirrored;
	}

	public float GetStartPosition()
	{
		if (isPlaying)
		{
			return startPositionX;
		}
		return 0f;
	}

	public float GetFirstStartPositionX()
	{
		return firstStartPositionX;
	}

	public int GetRenderTickCount()
	{
		return renderTickCount;
	}

	public float GetShiftWallDelta()
	{
		return shiftWallDelta;
	}

	public void set_ShiftWallDelta(float value)
	{
		shiftWallDelta = value;
	}

	public Vector3f GetShift()
	{
		return shift;
	}

	public void SetShift(Vector3f value)
	{
		shift.Set(value);
	}

	public int GetStartFrameOffset()
	{
		return startFrameOffset;
	}

	public int GetFrontAlignMargin()
	{
		return frontAlignMargin;
	}

	public int GetBackAlignMargin()
	{
		return backAlignMargin;
	}

	public ModelObject get_Model()
	{
		return _Model;
	}

	public float GetLeftWallX()
	{
		return leftWallX;
	}

	public float GetRightWallX()
	{
		return rightWallX;
	}

	public List<IntervalAnimation> GetActiveIntervals()
	{
		return activeIntervals;
	}

	public InfoAnimation GetCurrentInfo()
	{
		return currentInfo;
	}

	public void SetCurrentInfo(InfoAnimation value)
	{
		currentInfo = value;
	}

	public InfoAnimation GetFirstInfo()
	{
		return firstInfo;
	}

	public List<ModelEdge> GetAttackingEdges()
	{
		return attackingEdges;
	}

	public void Render()
	{
		renderTickCount++;
		if (isPlaying)
		{
			if (bufferState != -3)
			{
				bufferState++;
			}
			else if (frameCounter == 0)
			{
				frameInMove = 0;
			}
			ShiftWall();
			int num = _Frames.GetRemainingFrameCount();
			if (isBuffer())
			{
				DrawFrame();
				frameInMove++;
				CheckActionsOnFrame();
				if (hasParentModel && !isBuffer() && frameCursor + 2 >= num)
				{
					if (isLooping)
					{
						SetBufferFrame(frameCursor, startFrame + 1);
						frameCursor = startFrame;
					}
					else
					{
						StopAnimation();
						OnStopAnimation(currentInfo);
						DeleteAnimation();
					}
				}
			}
			else if (!hasParentModel && frameCursor + 2 >= num)
			{
				if (isLooping)
				{
					SetBufferFrame(frameCursor, startFrame + 1);
					frameCursor = startFrame + 1;
					DrawFrame();
					frameInMove++;
				}
				else
				{
					StopAnimation();
					OnStopAnimation(currentInfo);
					DeleteAnimation();
				}
			}
			else
			{
				if (frameCursor + 2 < num && !isBuffer())
				{
					SetBufferFrame();
				}
				if (isBuffer())
				{
					DrawFrame();
				}
				frameInMove++;
				frameCounter++;
				frameCursor++;
				NewFrame();
			}
		}
		else if (pendingFinalUpdate && currentInfo != null)
		{
			frameCursor += 3;
			NewFrame();
			pendingFinalUpdate = false;
		}
	}

	public void RenderPhysics()
	{
		frameCounter++;
		frameCursor++;
		NewFrame();
	}

	public void StopAnimation()
	{
		isPlaying = false;
		Stop();
	}

	public void DeleteAnimation()
	{
		isPlaying = false;
		pendingFinalUpdate = true;
	}

	public int GetCurrentFrame()
	{
		if (!currentInfo.HasPhysics)
		{
			return ((frameCursor > 2) ? (frameCursor - 2) : 0) + startFrame;
		}
		return frameCursor;
	}

	public int GetPhysicsFrame()
	{
		return (!isPlaying) ? frameCounter : (((frameCounter > 2) ? (frameCounter - 2) : 0) + startFrame);
	}

	public int GetReactionFrame()
	{
		return (frameCounter != 0) ? (frameCounter + startFrame - 2) : (-3);
	}

	public int GetPlayedFrame()
	{
		return (frameCounter != 0) ? (frameCounter + startFrame - 2) : (-3);
	}

	// best guess for name
	public int GetFrameInMove()
	{
		if (isPlaying)
		{
			return frameInMove;
		}
		return 0;
	}

	public void Reset()
	{
		isPlaying = false;
		sign = 1;
		currentNodeId = 0;
		currentNode = null;
		startFrame = 0;
		frameCounter = 0;
		frameCursor = 0;
		pendingFinalUpdate = false;
		lastProcessedFrame = int.MaxValue;
		Stop();
	}

	public int GetFrameStep()
	{
		return currentInfo.MidFrames;
	}

	public ModelNode GetNodeById(int nodeId)
	{
		return _Model.GetAllNodes()[nodeId];
	}

	public ModelNode GetPlayingNode()
	{
		if (isPlaying)
		{
			return currentNode;
		}
		return null;
	}

	public ModelNode GetNodeByNameForSign(string name, int sign)
	{
		if (!string.IsNullOrEmpty(name))
		{
			ModelNode node = _Model.GetNodeByName(name);
			if (node != null)
			{
				ModelNode otherNode = node.GetPairNode();
				if (otherNode == null)
				{
					return node;
				}
				char c = name[name.Length - 1];
				switch (sign)
				{
				case 1:
					switch (c)
					{
					case '1':
						if (node.GetStart().GetX() < otherNode.GetStart().GetX())
						{
							return otherNode;
						}
						return node;
					case '2':
						if (node.GetStart().GetX() < otherNode.GetStart().GetX())
						{
							return node;
						}
						return otherNode;
					}
					break;
				case -1:
					switch (c)
					{
					case '1':
						if (node.GetStart().GetX() < otherNode.GetStart().GetX())
						{
							return node;
						}
						return otherNode;
					case '2':
						if (node.GetStart().GetX() < otherNode.GetStart().GetX())
						{
							return otherNode;
						}
						return node;
					}
					break;
				default:
					GameLog.Error("strange sign {0}", sign);
					return node;
				}
			}
		}
		return null;
	}

	public void SetAligns(float leftX, float rightX, int frontMargin, int backMargin)
	{
		frontAlignMargin = frontMargin;
		backAlignMargin = backMargin;
		leftWallX = leftX;
		rightWallX = rightX;
	}

	public void ShiftSequence(float shiftX, float shiftY = 0f, float shiftZ = 0f)
	{
		_Frames.Shift(shiftX, shiftY, shiftZ);
	}

	public void MirrorSequence()
	{
		if (sign == -1)
		{
			_Frames.MirrorHorizontally();
		}
	}

	public void ShiftBuffer(Vector3f offset)
	{
		for (int i = 0; i < bufferedFrames.Count; i++)
		{
			List<Vector3f> list = bufferedFrames[i];
			for (int j = 0; j < list.Count; j++)
			{
				list[j].Add(offset);
			}
		}
	}

	public bool PlayInfo(InfoAnimation animation, int direction, bool useInterrupt = true, bool isFrameShift = false, int frameShift = -1)
	{
		if (animation != null)
		{
			if (RequiredInfo != null && RequiredInfo != animation && RequiredInfo != animation.GetTacticEquivalent())
			{
				GameLog.Error("Animation error need: '{0}' (Priority {1}); Animation played: '{2}' (Priority {3})", RequiredInfo.Name, RequiredInfo.Priority, animation.Name, animation.Priority);
			}
			if (hasParentModel)
			{
				ClearIntervals();
				ClearAttackingEdges();
			}
			int num = animation.FirstFrame;
			if (isFrameShift)
			{
				int num2 = GetCurrentFrame();
				num = num2 + 1 + frameShift;
			}
			else if (-1 < frameShift)
			{
				num = frameShift;
			}
			Stop();
			set_Sign(direction);
			currentInfo = animation;
			startFrame = num;
			endFrame = currentInfo.AnimationEndFrame;
			isLooping = currentInfo.GetIsLooped();
			if (startFrame > endFrame - 1)
			{
				startFrame = endFrame - 1;
			}
			if (currentInfo.GetAnimationFrames().Length == 0)
			{
				GameLog.Error("ModelAnimation::playInfo - empty animation \"{0}\"", currentInfo.Name);
				isPlaying = false;
				return false;
			}
			useInterruptFrames = useInterrupt;
			if (useInterrupt)
			{
				SetInterruptFrames(currentInfo.GetNodesCount());
			}
			// A frame-shifted continuation starts from the previous move's frame, which can pass
			// this animation's end. Load keyframes from the clamped start, not the raw value.
			currentInfo.FillKeyFrames(_Frames, startFrame, !useInterruptFrames);
			PhysicsNodes();
			SetCurrentNode();
			MirrorNodes();
			ShiftPoints();
			int nodesCount = animation.GetNodesCount();
			bufferedFrames.Resize(nodesCount);
			isPlaying = true;
			frameCounter = 0;
			frameCursor = 0;
			lastProcessedFrame = int.MaxValue;
			frameInMove = 0;
			bufferIndex = int.MaxValue;
			bufferState = -3;
			pendingFinalUpdate = false;
			accumulatedOffset.Reset();
			if (!currentInfo.GetSaveVelocity())
			{
				velocity.Set(currentInfo.GetVelocity());
				Vector3f currentVelocity = velocity;
				currentVelocity.SetX(currentVelocity.GetX() * (float)GetSign());
			}
			acceleration.Set(currentInfo.GetAcceleration());
			Vector3f currentAcceleration = acceleration;
			currentAcceleration.SetX(currentAcceleration.GetX() * (float)GetSign());
			SetDistanceAlign();
			OnStartAnimation(currentInfo);
			RequiredInfo = null;
			return true;
		}
		return false;
	}

	public void SetIntervals()
	{
		ClearAttackingEdges();
		int num = GetCurrentFrame();
		currentInfo.GetIntervals(num, activeIntervals, endedIntervals, intervalTypeFilter);
		foreach (IntervalAnimation item in activeIntervals)
		{
			if (item.Type == IntervalAnimation.IntervalType.INTERVAL_ATTACK)
			{
				SetAttackingEdges(item);
			}
			int num2 = ((startFrame > item.Start) ? startFrame : item.Start);
			if (num2 == num)
			{
				OnStartIntervals(item);
			}
		}
		foreach (IntervalAnimation item2 in endedIntervals)
		{
			OnEndIntervals(item2);
		}
	}

	public void ClearIntervals()
	{
		foreach (IntervalAnimation item in activeIntervals)
		{
			OnEndIntervals(item);
		}
	}

	public void ResetIntervals()
	{
		activeIntervals.Clear();
	}

	public void OnStartIntervals(IntervalAnimation interval)
	{
		CallEvent(2, interval);
	}

	public void OnEndIntervals(IntervalAnimation interval)
	{
		CallEvent(3, interval);
	}

	public void OnStartAnimation(InfoAnimation animation)
	{
		CallEvent(0, animation);
	}

	public void OnStopAnimation(InfoAnimation animation)
	{
		if (!hasParentModel)
		{
			ClearIntervals();
			ClearAttackingEdges();
		}
		CallEvent(1, animation);
	}

	public void SetAttackingEdges(IntervalAnimation interval)
	{
		IntervalAttack attackInterval = interval as IntervalAttack;
		List<string> list = attackInterval.GetAttackingParts();
		foreach (string item in list)
		{
			string text = item;
			if (GetIsMirrored())
			{
				char c = text[item.Length - 2];
				char c2 = text[item.Length - 1];
				if (c == '_')
				{
					switch (c2)
					{
					case '1':
						text = text.Remove(text.Length - 1, 1) + "2";
						break;
					case '2':
						text = text.Remove(text.Length - 1, 1) + "1";
						break;
					}
				}
			}
			ModelEdge edge = _Model.GetEdgeByName(text);
			if (edge != null)
			{
				attackingEdges.Add(edge);
			}
		}
	}

	public void ClearAttackingEdges()
	{
		attackingEdges.Clear();
	}

	public IntervalAnimation FindInterval(IntervalAnimation.IntervalType intervalType)
	{
		for (int i = 0; i < activeIntervals.Count; i++)
		{
			if (intervalType == activeIntervals[i].Type)
			{
				return activeIntervals[i];
			}
		}
		return null;
	}

	public IntervalAnimation FindInterval(string intervalName)
	{
		for (int i = 0; i < activeIntervals.Count; i++)
		{
			if (activeIntervals[i].Name == intervalName)
			{
				return activeIntervals[i];
			}
		}
		return null;
	}

	public void RemoveInterval(IntervalAnimation.IntervalType intervalType)
	{
		for (int num = activeIntervals.Count - 1; num >= 0; num--)
		{
			if (intervalType == activeIntervals[num].Type)
			{
				activeIntervals.RemoveAt(num);
			}
		}
	}

	public void RemoveInterval(string name)
	{
		for (int num = activeIntervals.Count - 1; num >= 0; num--)
		{
			if (name == activeIntervals[num].Name)
			{
				activeIntervals.RemoveAt(num);
			}
		}
	}

	public void RemoveIntervals(List<string> intervalNames)
	{
		foreach (string item in intervalNames)
		{
			RemoveInterval(item);
		}
	}

	public bool CheckIntervals(List<string> intervalNames)
	{
		for (int i = 0; i < intervalNames.Count; i++)
		{
			if (FindInterval(intervalNames[i]) != null)
			{
				return true;
			}
		}
		return false;
	}

	public void Init()
	{
		Heel1Node = _Model.GetNodeByName("NHeel_1");
		Heel2Node = _Model.GetNodeByName("NHeel_2");
	}

	public void AddIntervalTypeFilter(IntervalAnimation.IntervalType intervalType)
	{
		if (intervalTypeFilter == null)
		{
			intervalTypeFilter = new HashSet<IntervalAnimation.IntervalType>();
		}
		intervalTypeFilter.Add(intervalType);
	}

	public void ClearIntervalTypeFilter()
	{
		intervalTypeFilter = null;
	}

	public void TriggerActionsForEvent(EventAnimation.EventAnimationType eventType)
	{
		if (currentInfo == null)
		{
			return;
		}
		List<ActionAnimation> actions = currentInfo.MoveData.Actions;
		if (actions.Count <= 0)
		{
			return;
		}
		List<ActionAnimation> list = new List<ActionAnimation>();
		foreach (ActionAnimation item in actions)
		{
			if (item.NeedStart(eventType))
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			CallEvent(4, list);
		}
	}

	public void MoveByVelocity(Vector3f offset)
	{
		List<ModelNode> list = _Model.GetAllNodes();
		foreach (ModelNode item in list)
		{
			item.SetEnd();
			item.GetStart().Add(offset);
		}
	}

	public static bool CalcIsMirror(ModelObject modelObject, string nodeName, int sign, Vector3[] framePositions, bool useIdOrder = true)
	{
		ModelNode node = modelObject.GetNodeByName(nodeName);
		ModelNode pairNode = ((node == null) ? null : node.GetPairNode());
		if (node != null && pairNode != null)
		{
			return CalcIsMirror(node, pairNode, sign, framePositions, useIdOrder);
		}
		GameLog.Error("ModelAnimation::mirrorNodes - nodes not found: \"{0}\"", nodeName);
		return false;
	}

	public static bool CalcIsMirror(ModelNode node, ModelNode pairNode, int sign, Vector3[] framePositions, bool useIdOrder = true)
	{
		if (sign == -1)
		{
			Util.Swap(ref node, ref pairNode);
		}
		int nodeId = node.GetID();
		int pairNodeId = pairNode.GetID();
		if (!useIdOrder && sign == -1)
		{
			Util.Swap(ref nodeId, ref pairNodeId);
		}
		return node.GetStart().GetX() >= pairNode.GetStart().GetX() != framePositions[nodeId].x >= framePositions[pairNodeId].x;
	}

	public static bool CalcIsMirror(ModelObject modelObject, string nodeName, int sign, List<Vector3f> framePositions, bool useIdOrder = true)
	{
		ModelNode node = modelObject.GetNodeByName(nodeName);
		ModelNode pairNode = ((node == null) ? null : node.GetPairNode());
		if (node != null && pairNode != null)
		{
			return CalcIsMirror(node, pairNode, sign, framePositions, useIdOrder);
		}
		GameLog.Error("ModelAnimation::mirrorNodes - nodes not found: \"{0}\"", nodeName);
		return false;
	}

	public static bool CalcIsMirror(ModelNode node, ModelNode pairNode, int sign, List<Vector3f> framePositions, bool useIdOrder = true)
	{
		if (sign == -1)
		{
			Util.Swap(ref node, ref pairNode);
		}
		int nodeId = node.GetID();
		int pairNodeId = pairNode.GetID();
		if (!useIdOrder && sign == -1)
		{
			Util.Swap(ref nodeId, ref pairNodeId);
		}
		return node.GetStart().GetX() >= pairNode.GetStart().GetX() != framePositions[nodeId].GetX() >= framePositions[pairNodeId].GetX();
	}

	private void SetDistanceAlign()
	{
		ModelNode playingNode = GetPlayingNode();
		float startX = pivotTargetX;
		int firstFrame = currentInfo.FirstFrame;
		int num = GetStartFrame();
		startFrameOffset = Eclipse.Runtime.PlaybackTiming.TicksBefore(currentInfo.MidFrames + 1, num - firstFrame, currentInfo.PlaybackRatePermille);
		float num2 = 0f;
		if (0 < startFrameOffset && currentInfo.MoveData.AlignData.PivotObjectType == InfoAnimation.AlignObjectType.ObjectNodes)
		{
			int num3;
			if (GetIsMirrored())
			{
				ModelNode pairNode = playingNode.GetPairNode();
				num3 = ((pairNode == null) ? GetCurrentNodeId() : pairNode.GetID());
			}
			else
			{
				num3 = GetCurrentNodeId();
			}
			num2 = currentInfo.GetAnimationFrames()[num][num3].x - currentInfo.GetAnimationFrames()[firstFrame][num3].x;
		}
		startX = (startPositionX = startX + num2 * (float)GetSign());
		if (!firstPositionCaptured)
		{
			firstStartPositionX = startX;
			firstInfo = currentInfo;
			renderTickCount = -4;
			firstPositionCaptured = true;
		}
	}

	private void Stop()
	{
		_Frames.Reset();
		bufferIndex = int.MaxValue;
	}

	private void DrawFrame()
	{
		List<ModelNode> list = _Model.GetAllNodes();
		ApplyVelocity();
		Vector3f point = new Vector3f();
		for (int i = 0; i < bufferedFrames.Count; i++)
		{
			ModelNode Node = list[i];
			if (!_Model.IsShock() || (_Model.IsShock() && !Node.IsShock()))
			{
				Node.SetEnd();
				point.Set(bufferedFrames[i][bufferIndex]);
				point.Add(accumulatedOffset);
				Node.SetStart(point);
				Node.SetSkipMacroUpdate(true);
			}
		}
		if (currentInfo.GetRotationAngle() != 0f)
		{
			ApplyDrawTransform();
		}
		int count = bufferedFrames[0].Count;
		if (bufferIndex == count - 1)
		{
			bufferIndex++;
		}
		else
		{
			bufferIndex += ((interpolationSteps <= 1) ? 1 : (interpolationSteps / GameUtils.GetSlowMode()));
			if (bufferIndex > count - 1)
			{
				bufferIndex = count - 1;
			}
		}
		if (bufferIndex >= interpolationSteps)
		{
			bufferReachedKeyFrame = true;
		}
		else
		{
			bufferReachedKeyFrame = false;
		}
		ApplyAcceleration();
	}

	private void SetInterruptFrames(int interruptFrameCount)
	{
		_Frames.InterruptFramesSeted(interruptFrameCount);
		int num = (currentInfo.MidFrames + 1) / 2;
		KeyFrames.Frame firstFrame = _Frames.GetFrame(0);
		KeyFrames.Frame secondFrame = _Frames.GetFrame(1);
		List<ModelNode> list = _Model.GetAllNodes();
		for (int i = 0; i < firstFrame.Size; i++)
		{
			Vector3f start = list[i].GetStart();
			Vector3f nodeEnd = list[i].GetEnd();
			float deltaX = (start.GetX() - nodeEnd.GetX()) * (float)num;
			float deltaY = (start.GetY() - nodeEnd.GetY()) * (float)num;
			float deltaZ = (start.GetZ() - nodeEnd.GetZ()) * (float)num;
			firstFrame.Data[i].Set(start);
			firstFrame.Data[i].Subtract(deltaX, deltaY, deltaZ);
			secondFrame.Data[i].Set(start);
			secondFrame.Data[i].Add(deltaX, deltaY, deltaZ);
		}
	}

	private void ShiftPoints()
	{
		InfoAnimation.MovePivot alignData = currentInfo.MoveData.AlignData;
		ModelAnimation pivotAnimation = GetAnimationByModelType(alignData.PivotModelType);
		ModelAnimation alignAnimation = GetAnimationByModelType(alignData.PositionModelType);
		Vector3f pivotPoint = new Vector3f();
		Vector3f shiftPoint = new Vector3f();
		if (alignAnimation == null)
		{
			alignAnimation = this;
		}
		switch (alignData.PivotObjectType)
		{
		case InfoAnimation.AlignObjectType.ObjectPivot:
			pivotPoint.Set(_Frames.GetFrame(2).Data[currentNodeId]);
			break;
		case InfoAnimation.AlignObjectType.ObjectNodes:
		{
			int num = ((!pivotAnimation.GetIsMirrored() || alignData.PivotPairNodeId <= -1) ? alignData.PivotNodeId : alignData.PivotPairNodeId);
			num = ((num >= 0 && num < _Frames.GetFrame(2).Size) ? num : 0);
			pivotPoint.Set(_Frames.GetFrame(2).Data[num]);
			break;
		}
		case InfoAnimation.AlignObjectType.ObjectAnimation:
			pivotPoint.Reset();
			break;
		case InfoAnimation.AlignObjectType.ObjectWall:
			pivotPoint.Reset();
			pivotPoint.SetX((GetSign() == 1 != (alignData.PivotPart == "Back")) ? (0f - rightWallX) : (0f - leftWallX));
			break;
		}
		switch (alignData.PositionObjectType)
		{
		case InfoAnimation.AlignObjectType.ObjectPivot:
			if (alignAnimation.GetCurrentNode() != null)
			{
				shiftPoint.Set(alignAnimation.GetCurrentNode().GetStart());
			}
			break;
		case InfoAnimation.AlignObjectType.ObjectNodes:
		{
			int positionNodeId = ((!alignAnimation.GetIsMirrored() || alignData.PositionPairNodeId <= -1) ? alignData.PositionNodeId : alignData.PositionPairNodeId);
			shiftPoint.Set(alignAnimation.GetNodeById(positionNodeId).GetStart());
			break;
		}
		case InfoAnimation.AlignObjectType.ObjectAnimation:
			shiftPoint.Set(alignAnimation.GetShift());
			break;
		case InfoAnimation.AlignObjectType.ObjectWall:
			shiftPoint.Reset();
			shiftPoint.SetX((GetSign() == 1 != (alignData.PositionPart == "Back")) ? rightWallX : leftWallX);
			break;
		}
		shiftPoint.SetX(shiftPoint.GetX() + (float)GetSign() * alignData.PositionShift.GetX());
		shiftPoint.SetY(shiftPoint.GetY() + alignData.PositionShift.GetY());
		pivotTargetX = shiftPoint.GetX();
		shift.Set(Vector3f.op_Subtraction(shiftPoint, pivotPoint));
		ShiftSequence((!alignData.AlignX) ? 0f : shift.GetX(), (!alignData.AlignY) ? 0f : shift.GetY(), (!alignData.AlignZ) ? 0f : shift.GetZ());
		if (string.IsNullOrEmpty(alignData.ShiftModelNode))
		{
			return;
		}
		ModelNode shiftNode = _Model.GetNodeByName(alignData.ShiftModelNode);
		int index = shiftNode.GetID();
		pivotPoint = _Frames.GetFrame(2).Data[index];
		Vector3f startOffset = new Vector3f(Vector3f.op_Subtraction(pivotPoint, shiftNode.GetStart()));
		Vector3f endOffset = new Vector3f(Vector3f.op_Subtraction(pivotPoint, shiftNode.GetEnd()));
		List<ModelNode> list = _Model.GetAllNodes();
		int count = list.Count;
		foreach (ModelNode item in list)
		{
			item.GetStart().Add(startOffset);
			item.GetEnd().Add(endOffset);
		}
	}

	private float GetNodeOffsetX(List<Vector3f> frame)
	{
		return currentNode.GetStart().GetX() - frame[currentNodeId].GetX();
	}

	private float GetNodeOffsetY(List<Vector3f> frame)
	{
		return currentNode.GetStart().GetY() - frame[currentNodeId].GetY();
	}

	private Vector3f GetNodeOffset(List<Vector3f> frame)
	{
		return Vector3f.op_Subtraction(currentNode.GetStart(), frame[currentNodeId]);
	}

	private Vector3f GetNodeOffset()
	{
		return Vector3f.op_Implicit(default(Vector3));
	}

	private void MirrorNodes()
	{
		MirrorSequence();
		InfoAnimation.MirrorNode mirrorNode = currentInfo.GetMirrorNode();
		if (mirrorNode.GetIsEmpty())
		{
			return;
		}
		isMirrored = CalcIsMirror(_Model, mirrorNode.GetNodeName(), GetSign(), _Frames.GetFrame(2).Data);
		if (isMirrored)
		{
			int num = _Model.GetNodeIDByPairName(currentNodeId);
			if (num > -1)
			{
				currentNodeId = num;
				currentNode = _Model.GetAllNodes()[currentNodeId];
			}
			currentInfo.SwapNodePairs(_Model.GetPairNodeIds(), _Frames, 2);
		}
	}

	private bool isBuffer()
	{
		return 0 < bufferedFrames.Count && bufferIndex < bufferedFrames[0].Count;
	}

	private void SetBufferFrame(int firstFrameIndex = -1, int secondFrameIndex = -1)
	{
		if (firstFrameIndex == -1 || secondFrameIndex == -1)
		{
			firstFrameIndex = frameCursor;
			secondFrameIndex = frameCursor + 1;
		}
		bufferState = 1;
		interpolationSteps = GameUtils.GetSlowMode();
		// Eclipse: a playback rate resizes each keyframe segment; velocity is scaled
		// so a segment still travels its authored distance.
		int rate = currentInfo.PlaybackRatePermille;
		subFrameScale = (float)rate / (1000f * (float)interpolationSteps);
		int steps = Mathf.Max(1, Eclipse.Runtime.PlaybackTiming.SegmentSteps((GetFrameStep() + 1) * interpolationSteps, firstFrameIndex, rate));
		List<Vector3f> startPoints = _Frames.GetFrame(firstFrameIndex).Data;
		List<Vector3f> secondFramePoints = _Frames.GetFrame(secondFrameIndex).Data;
		List<Vector3f> thirdFramePoints = _Frames.GetFrame(secondFrameIndex + 1).Data;
		Bezier bezier = new Bezier(steps);
		for (int i = 0; i < bufferedFrames.Count; i++)
		{
			bezier.BuildCurve(startPoints[i], secondFramePoints[i], thirdFramePoints[i], bufferedFrames[i]);
		}
		bufferIndex = 0;
	}

	private void ShiftWall()
	{
		bool flag = false;
		float num = 0f;
		if (parentAnimation != null && currentInfo.AlignOnParentWallCollision)
		{
			num = parentAnimation.GetShiftWallDelta();
			flag = true;
		}
		int num2 = frameCursor + 2;
		if (num2 > _Frames.GetSize() - 1)
		{
			set_ShiftWallDelta(0f);
			return;
		}
		int num3 = ((sign != -1) ? backAlignMargin : frontAlignMargin);
		int num4 = ((sign != 1) ? backAlignMargin : frontAlignMargin);
		int index = ((_Model.GetPivotNode() == null) ? _Model.GetAllNodes()[0].GetID() : _Model.GetPivotNode().GetID());
		float num5 = _Frames.GetFrame(num2).Data[index].GetX();
		if (currentInfo.NoWallRepulsion && !flag)
		{
			return;
		}
		if (flag)
		{
			num5 = num;
		}
		else
		{
			num5 = ((num5 < leftWallX + (float)num3) ? (num5 - (leftWallX + (float)num3)) : ((!(num5 > rightWallX - (float)num4)) ? 0f : (num5 - (rightWallX - (float)num4))));
			set_ShiftWallDelta(num5);
			if (num5 == 0f)
			{
				return;
			}
		}
		int num6 = ((_Frames.GetSize() >= num2 + 2) ? (num2 + 2) : _Frames.GetSize());
		for (int i = num2; i < num6; i++)
		{
			KeyFrames.Frame frame = _Frames.GetFrame(i);
			for (int j = 0; j < frame.Size; j++)
			{
				Vector3f point = frame.Data[j];
				point.SetX(point.GetX() - num5);
			}
		}
	}

	private void NewFrame()
	{
		if (currentInfo != null)
		{
			int num = GetCurrentFrame();
			if (num != lastProcessedFrame)
			{
				lastProcessedFrame = num;
				SetIntervals();
				frameChangedPending = true;
			}
		}
		CheckActionsOnFrame();
	}

	private void SetCurrentNode()
	{
		if (currentInfo.MoveData.AlignData.PivotNodeId > -1)
		{
			currentNodeId = currentInfo.MoveData.AlignData.PivotNodeId;
			currentNode = _Model.GetAllNodes()[currentNodeId];
		}
		else
		{
			currentNode = null;
			// A move with no alignment pivot is valid; do not log every playback,
			// including rollback re-simulation, on the combat hot path.
		}
	}

	private void PhysicsNodes()
	{
		_Model.RestoreDefaultPhysics();
		List<ModelNode> list = _Model.GetAllNodes();
		int num = currentInfo.GetNodesCount();
		if (list.Count < num)
		{
			GameLog.Error("In {0} animation {1} nodes, but in model only {2}", currentInfo.Name, num, list.Count);
		}
		for (int i = 0; i < num; i++)
		{
			list[i].DisablePhysics();
		}
	}

	private ModelAnimation GetAnimationByModelType(ModelType.ModelTargetType targetType)
	{
		switch (targetType)
		{
		case ModelType.ModelTargetType.MODEL_NULL:
		case ModelType.ModelTargetType.MODEL_THIS:
			return this;
		case ModelType.ModelTargetType.MODEL_PARENT:
			return parentAnimation;
		case ModelType.ModelTargetType.MODEL_OTHER:
			return otherAnimation;
		default:
			GameLog.Error("ModelAnimation::getPlayerAnimation - unknown type: {0}", targetType);
			return null;
		}
	}

	private void CheckActionsOnFrame()
	{
		if (!frameChangedPending || !bufferReachedKeyFrame)
		{
			return;
		}
		frameChangedPending = false;
		List<ActionAnimation> actions = currentInfo.MoveData.Actions;
		if (actions.Count <= 0)
		{
			return;
		}
		int currentFrame = GetCurrentFrame();
		List<ActionAnimation> list = new List<ActionAnimation>();
		foreach (ActionAnimation item in actions)
		{
			if (item.NeedStart(currentFrame))
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			CallEvent(4, list);
		}
	}

	private void ApplyVelocity()
	{
		ApplyVelocity(velocity);
	}

	private void ApplyVelocity(Vector3f velocityStep)
	{
		Vector3f scaledVelocity = new Vector3f(velocityStep);
		if (subFrameScale != 1f)
		{
			scaledVelocity.Multiply(subFrameScale);
		}
		accumulatedOffset.Add(scaledVelocity);
	}

	private void ApplyAcceleration()
	{
		ApplyAcceleration(acceleration);
	}

	private void ApplyAcceleration(Vector3f accelerationStep)
	{
		Vector3f scaledAcceleration = new Vector3f(accelerationStep);
		if (subFrameScale != 1f)
		{
			scaledAcceleration.Multiply(subFrameScale);
		}
		velocity.Add(scaledAcceleration);
	}

	private void NoOpHook()
	{
	}

	private void ApplyDrawTransform()
	{
	}
}
