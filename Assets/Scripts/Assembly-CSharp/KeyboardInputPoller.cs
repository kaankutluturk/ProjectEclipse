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

	public void AddKey(KeyCode keyCode, FightCID index, int playerIndex = 0)
	{
		FightControlEventData.KeyboardBinding item = new FightControlEventData.KeyboardBinding(keyCode, index, playerIndex);
		keyBindingsByPlayer[playerIndex].Add(item);
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

	private void PollKey(FightControlEventData.KeyboardBinding binding)
	{
		if (Eclipse.Input.EclipseInput.GetKeyDown(binding.Key) || Eclipse.Input.EclipseInput.GetKey(binding.Key))
		{
			if (!binding.isActive)
			{
				binding.isActive = true;
				DispatchKeyEvent(0, binding);
			}
		}
		else if (binding.isActive)
		{
			binding.isActive = false;
			DispatchKeyEvent(1, binding);
		}
	}

	private void DispatchKeyEvent(int eventType, FightControlEventData.KeyboardBinding binding)
	{
		FightControlEventData eventData = new FightControlEventData();
		eventData.Index = binding.count;
		eventData.Control = binding.Index;
		CallEvent(eventType, eventData);
	}
}
