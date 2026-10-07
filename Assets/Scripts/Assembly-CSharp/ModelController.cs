using System.Collections.Generic;

public class ModelController : global::EventDispatcher<object>
{
	public enum KeyEventType
	{
		onKeyPressed = 0,
		onKeyReleased = 1
	}

	private class KeyInfo
	{
		public int KeyId;

		public int Index;

		public bool Press;
	}

	private const float MCONTROLLER_COMBO_INTERVAL = 0.25f;

	private const int KeyClearInterval = 30;

	private const int MaxPressedKeys = 2;

	private const int KeyCount = 150;

	private List<KeyInfo> keys = new List<KeyInfo>();

	private int keyClearCounter;

	private KeyData currentKeyData = new KeyData();

	private KeyData keyDataSnapshot = new KeyData();

	private int pressAgeCounter;

	public KeyData CurrentKeys
	{
		get
		{
			return GetCurrentKeys();
		}
		set
		{
			SetCurrentKeys(value);
		}
	}

	public ModelController()
	{
		keyClearCounter = 0;
		pressAgeCounter = 0;
		keys.Capacity = 150;
		for (int i = 0; i < 150; i++)
		{
			KeyInfo keyInfo = new KeyInfo();
			keyInfo.KeyId = (keyInfo.Index = i + 1);
			keyInfo.Press = false;
			keys.Add(keyInfo);
		}
	}

	public KeyData GetCurrentKeys()
	{
		return currentKeyData;
	}

    // Move held keys and combo timing without replaying input or exchanging
    // event subscribers, which are bound to each body's animation controller.
    internal void ExchangeFormInput(ModelController other)
    {
        if (other == null || other == this)
            throw new System.ArgumentException("Form input requires distinct controllers.");
        (keys, other.keys) = (other.keys, keys);
        (keyClearCounter, other.keyClearCounter) = (other.keyClearCounter, keyClearCounter);
        (currentKeyData, other.currentKeyData) = (other.currentKeyData, currentKeyData);
        (keyDataSnapshot, other.keyDataSnapshot) = (other.keyDataSnapshot, keyDataSnapshot);
        (pressAgeCounter, other.pressAgeCounter) = (other.pressAgeCounter, pressAgeCounter);
    }

	public void SetCurrentKeys(KeyData value)
	{
		currentKeyData = value.Copy();
		keyDataSnapshot = value.Copy();
	}

	public void Render()
	{
		if (keyClearCounter == 30)
		{
			currentKeyData.Clear();
			keyClearCounter = 0;
		}
		if ((float)pressAgeCounter >= 15f)
		{
			currentKeyData.StarterKeys.Clear();
			pressAgeCounter = 0;
		}
		SetAdditional();
		pressAgeCounter++;
		keyClearCounter++;
	}

	public void Reset()
	{
		keyDataSnapshot.Clear();
		currentKeyData.Clear();
		currentKeyData.StarterKeys.Clear();
		int i = 0;
		for (int count = keys.Count; i < count; i++)
		{
			keys[i].Press = false;
		}
	}

	public void OnPressAnyKey(int keyId)
	{
		KeyInfo keyInfo = GetKey(keyId);
		if (keyInfo != null && !keyInfo.Press)
		{
			keyInfo.Press = true;
			pressAgeCounter = 0;
			currentKeyData.StarterKeys.Add(keyInfo.Index);
			while (currentKeyData.StarterKeys.Count > 2)
			{
				currentKeyData.StarterKeys.RemoveAt(0);
			}
			currentKeyData.Clear();
			SetAdditional();
			currentKeyData.PressTypeMode = GetPressType(currentKeyData.StarterKeys, currentKeyData.AdditionalKeys);
			CallKeyPressed();
		}
	}

	public void OnReleaseAnyKey(int keyId)
	{
		KeyInfo keyInfo = GetKey(keyId);
		if (keyInfo == null)
		{
			return;
		}
		keyInfo.Press = false;
		if (!currentKeyData.StarterKeys.Contains(keyInfo.Index))
		{
			int num = currentKeyData.AdditionalKeys.IndexOf(keyInfo.Index);
			if (currentKeyData.AdditionalKeys.Contains(keyInfo.Index))
			{
				currentKeyData.ReleaseKeys.Add(keyInfo.Index);
				currentKeyData.AdditionalKeys.Remove(keyInfo.Index);
				CallKeyReleased();
			}
		}
	}

	public KeyData GetKeyDataBySign(int sign)
	{
		keyDataSnapshot.Set(currentKeyData);
		return keyDataSnapshot;
	}

	public void CallKeyPressed()
	{
		CallEvent(0, currentKeyData);
	}

	public void CallKeyReleased()
	{
		CallEvent(1, currentKeyData);
	}

	private KeyInfo GetKey(int keyId)
	{
		foreach (KeyInfo item in keys)
		{
			if (item.KeyId == keyId)
			{
				return item;
			}
		}
		return null;
	}

	private void SetAdditional()
	{
		currentKeyData.AdditionalKeys.Clear();
		foreach (KeyInfo item in keys)
		{
			if (item.Press)
			{
				currentKeyData.AdditionalKeys.Add(item.Index);
			}
		}
	}

	private KeyData.PressType GetPressType(List<int> starterKeys, List<int> additionalKeys)
	{
		if (starterKeys.Count == 1)
		{
			return KeyData.PressType.BOTH;
		}
		foreach (int item in starterKeys)
		{
			foreach (int item2 in additionalKeys)
			{
				if (item == item2)
				{
					return KeyData.PressType.BOTH;
				}
			}
		}
		return KeyData.PressType.SEQUENCE;
	}
}
