using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputPoller : global::EventDispatcher<object>
{
	public enum KeyboardEvent
	{
		OnKeyboardKeyPress = 0,
		OnKeyboardKeyRelease = 1
	}

	public List<List<FightControlEventData.KeyboardBinding>> keyBindingsByPlayer = new List<List<FightControlEventData.KeyboardBinding>>();

	public bool IsEnabled;

	public void Init()
	{
		keyBindingsByPlayer.Clear();
		keyBindingsByPlayer.Resize(2);
	}

	public void Render()
	{
		if (!IsEnabled)
		{
			return;
		}
		int i = 0;
		for (int count = keyBindingsByPlayer.Count; i < count; i++)
		{
			foreach (FightControlEventData.KeyboardBinding item in keyBindingsByPlayer[i])
			{
				PollKey(item);
			}
		}
	}

	public void AddKey(KeyCode KGBGENDIMBC, FightCID index, int JAMPAODJGGL = 0)
	{
		FightControlEventData.KeyboardBinding item = new FightControlEventData.KeyboardBinding(KGBGENDIMBC, index, JAMPAODJGGL);
		keyBindingsByPlayer[JAMPAODJGGL].Add(item);
	}

	public void Clear()
	{
		int i = 0;
		for (int count = keyBindingsByPlayer.Count; i < count; i++)
		{
			keyBindingsByPlayer[i].Clear();
		}
		keyBindingsByPlayer.Clear();
	}

	private void PollKey(FightControlEventData.KeyboardBinding KGBGENDIMBC)
	{
		if (Eclipse.Input.EclipseInput.GetKeyDown(KGBGENDIMBC.Key) || Eclipse.Input.EclipseInput.GetKey(KGBGENDIMBC.Key))
		{
			if (!KGBGENDIMBC.isActive)
			{
				KGBGENDIMBC.isActive = true;
				DispatchKeyEvent(0, KGBGENDIMBC);
			}
		}
		else if (KGBGENDIMBC.isActive)
		{
			KGBGENDIMBC.isActive = false;
			DispatchKeyEvent(1, KGBGENDIMBC);
		}
	}

	private void DispatchKeyEvent(int DOPHKKGNAEF, FightControlEventData.KeyboardBinding KGBGENDIMBC)
	{
		FightControlEventData cBBEIGACPPD = new FightControlEventData();
		cBBEIGACPPD.Index = KGBGENDIMBC.count;
		cBBEIGACPPD.Control = KGBGENDIMBC.Index;
		CallEvent(DOPHKKGNAEF, cBBEIGACPPD);
	}
}
