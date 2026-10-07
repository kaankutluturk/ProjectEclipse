using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyFrames
{
	public class Frame
	{
		public List<Vector3f> Data;

		public int Size;
	}

	protected List<Frame> frames = new List<Frame>();

	private bool _IsInterruptFramesSeted;

	protected int frameCount;

	protected int currentFrameIndex;

	public int Size
	{
		get
		{
			return GetSize();
		}
	}

	public int CurrentFrameIndex
	{
		get
		{
			return GetCurrentFrameIndex();
		}
	}

	public int RemainingFrameCount
	{
		get
		{
			return GetRemainingFrameCount();
		}
	}

	public int GetSize()
	{
		return frameCount;
	}

	public int GetCurrentFrameIndex()
	{
		return currentFrameIndex;
	}

	public int GetRemainingFrameCount()
	{
		return frameCount - currentFrameIndex;
	}

	public void AdvanceFrame()
	{
		currentFrameIndex++;
	}

	public void InterruptFramesSeted(int nodeCount)
	{
		frameCount = 2;
		_IsInterruptFramesSeted = true;
		if (frames.Count < frameCount)
		{
			AllocateFrames(2, nodeCount);
		}
		for (int i = 0; i < frameCount; i++)
		{
			Frame frame = frames[i];
			if (frame.Size != nodeCount)
			{
				if (frame.Size < nodeCount)
				{
					frame.Data.Resize(nodeCount);
				}
				frame.Size = nodeCount;
			}
		}
	}

	public Frame GetFrame(int index)
	{
		if (frameCount <= index)
		{
			return null;
		}
		return frames[index];
	}

	public Frame GetFrameRelativeToCurrent(int offset)
	{
		return frames[currentFrameIndex + offset];
	}

	public void Shift(float offsetX, float offsetY = 0f, float offsetZ = 0f)
	{
		for (int i = (_IsInterruptFramesSeted ? 2 : 0); i < frameCount; i++)
		{
			for (int j = 0; j < frames[i].Size; j++)
			{
				frames[i].Data[j].Add(offsetX, offsetY, offsetZ);
			}
		}
	}

	public void MirrorHorizontally()
	{
		for (int i = (_IsInterruptFramesSeted ? 2 : 0); i < frameCount; i++)
		{
			for (int j = 0; j < frames[i].Size; j++)
			{
				Vector3f point = frames[i].Data[j];
				point.SetX(point.GetX() * -1f);
			}
		}
	}

	public void Reset()
	{
		frameCount = 0;
		currentFrameIndex = 0;
		_IsInterruptFramesSeted = false;
	}

	public void SetFramesFromRange(int startFrame, int endFrame, bool repeatFirstFrame, Vector3[][] animation)
	{
		if (repeatFirstFrame)
		{
			int num = Math.Min(animation.Length - 1, startFrame + 2);
			SetFrame(animation[num]);
			SetFrame(animation[num]);
		}
		for (int i = startFrame; i <= endFrame; i++)
		{
			SetFrame(animation[i]);
		}
	}

	public void SetFrame(Vector3[] framePoints)
	{
		frameCount++;
		if (frames.Count < frameCount)
		{
			AllocateFrames(1, framePoints.Length);
		}
		Frame frame = frames[frameCount - 1];
		if (frame.Size != framePoints.Length)
		{
			if (frame.Size < framePoints.Length)
			{
				frame.Data.Resize(framePoints.Length);
			}
			frame.Size = framePoints.Length;
		}
		for (int i = 0; i < framePoints.Length; i++)
		{
			frame.Data[i].Set(framePoints[i]);
		}
	}

	protected void AllocateFrames(int count, int nodeCount)
	{
		for (int i = 0; i < count; i++)
		{
			Frame frame = new Frame();
			frame.Data = new List<Vector3f>(nodeCount);
			frames.Add(frame);
			for (int j = 0; j < nodeCount; j++)
			{
				frame.Data.Add(new Vector3f());
			}
			frame.Size = nodeCount;
		}
	}
}
