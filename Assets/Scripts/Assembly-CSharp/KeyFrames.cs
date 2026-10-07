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

	public void InterruptFramesSeted(int FPDMCHPHFAJ)
	{
		frameCount = 2;
		_IsInterruptFramesSeted = true;
		if (frames.Count < frameCount)
		{
			AllocateFrames(2, FPDMCHPHFAJ);
		}
		for (int i = 0; i < frameCount; i++)
		{
			Frame cJMFONMNFBI = frames[i];
			if (cJMFONMNFBI.Size != FPDMCHPHFAJ)
			{
				if (cJMFONMNFBI.Size < FPDMCHPHFAJ)
				{
					cJMFONMNFBI.Data.Resize(FPDMCHPHFAJ);
				}
				cJMFONMNFBI.Size = FPDMCHPHFAJ;
			}
		}
	}

	public Frame GetFrame(int DCHCFFFFLLK)
	{
		if (frameCount <= DCHCFFFFLLK)
		{
			return null;
		}
		return frames[DCHCFFFFLLK];
	}

	public Frame GetFrameRelativeToCurrent(int OCDKOFPGCHH)
	{
		return frames[currentFrameIndex + OCDKOFPGCHH];
	}

	public void Shift(float HLBMDDOPKKL, float ELAKEOGEDPN = 0f, float PIIFLHIBODE = 0f)
	{
		for (int i = (_IsInterruptFramesSeted ? 2 : 0); i < frameCount; i++)
		{
			for (int j = 0; j < frames[i].Size; j++)
			{
				frames[i].Data[j].Add(HLBMDDOPKKL, ELAKEOGEDPN, PIIFLHIBODE);
			}
		}
	}

	public void MirrorHorizontally()
	{
		for (int i = (_IsInterruptFramesSeted ? 2 : 0); i < frameCount; i++)
		{
			for (int j = 0; j < frames[i].Size; j++)
			{
				Vector3f eMAFACPEPDK = frames[i].Data[j];
				eMAFACPEPDK.SetX(eMAFACPEPDK.GetX() * -1f);
			}
		}
	}

	public void Reset()
	{
		frameCount = 0;
		currentFrameIndex = 0;
		_IsInterruptFramesSeted = false;
	}

	public void SetFramesFromRange(int AMNCLCPADOO, int IFIOLDFCLIE, bool HOHEFHKJIOG, Vector3[][] GHDPPHAAPCA)
	{
		if (HOHEFHKJIOG)
		{
			int num = Math.Min(GHDPPHAAPCA.Length - 1, AMNCLCPADOO + 2);
			SetFrame(GHDPPHAAPCA[num]);
			SetFrame(GHDPPHAAPCA[num]);
		}
		for (int i = AMNCLCPADOO; i <= IFIOLDFCLIE; i++)
		{
			SetFrame(GHDPPHAAPCA[i]);
		}
	}

	public void SetFrame(Vector3[] GHDPPHAAPCA)
	{
		frameCount++;
		if (frames.Count < frameCount)
		{
			AllocateFrames(1, GHDPPHAAPCA.Length);
		}
		Frame cJMFONMNFBI = frames[frameCount - 1];
		if (cJMFONMNFBI.Size != GHDPPHAAPCA.Length)
		{
			if (cJMFONMNFBI.Size < GHDPPHAAPCA.Length)
			{
				cJMFONMNFBI.Data.Resize(GHDPPHAAPCA.Length);
			}
			cJMFONMNFBI.Size = GHDPPHAAPCA.Length;
		}
		for (int i = 0; i < GHDPPHAAPCA.Length; i++)
		{
			cJMFONMNFBI.Data[i].Set(GHDPPHAAPCA[i]);
		}
	}

	protected void AllocateFrames(int GNDPBMIJEMH, int DGHIGGGFNLP)
	{
		for (int i = 0; i < GNDPBMIJEMH; i++)
		{
			Frame cJMFONMNFBI = new Frame();
			cJMFONMNFBI.Data = new List<Vector3f>(DGHIGGGFNLP);
			frames.Add(cJMFONMNFBI);
			for (int j = 0; j < DGHIGGGFNLP; j++)
			{
				cJMFONMNFBI.Data.Add(new Vector3f());
			}
			cJMFONMNFBI.Size = DGHIGGGFNLP;
		}
	}
}
