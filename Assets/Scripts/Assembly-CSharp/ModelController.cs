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
			KeyInfo hFBBKFECOBD = new KeyInfo();
			hFBBKFECOBD.KeyId = (hFBBKFECOBD.Index = i + 1);
			hFBBKFECOBD.Press = false;
			keys.Add(hFBBKFECOBD);
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

	public void OnPressAnyKey(int KJPGKHJNOMC)
	{
		KeyInfo hFBBKFECOBD = GetKey(KJPGKHJNOMC);
		if (hFBBKFECOBD != null && !hFBBKFECOBD.Press)
		{
			hFBBKFECOBD.Press = true;
			pressAgeCounter = 0;
			currentKeyData.StarterKeys.Add(hFBBKFECOBD.Index);
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

	public void OnReleaseAnyKey(int KJPGKHJNOMC)
	{
		KeyInfo hFBBKFECOBD = GetKey(KJPGKHJNOMC);
		if (hFBBKFECOBD == null)
		{
			return;
		}
		hFBBKFECOBD.Press = false;
		if (!currentKeyData.StarterKeys.Contains(hFBBKFECOBD.Index))
		{
			int num = currentKeyData.AdditionalKeys.IndexOf(hFBBKFECOBD.Index);
			if (currentKeyData.AdditionalKeys.Contains(hFBBKFECOBD.Index))
			{
				currentKeyData.ReleaseKeys.Add(hFBBKFECOBD.Index);
				currentKeyData.AdditionalKeys.Remove(hFBBKFECOBD.Index);
				CallKeyReleased();
			}
		}
	}

	public KeyData GetKeyDataBySign(int AOJJBKLCHJO)
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

	private KeyInfo GetKey(int HDKKKCDKFEE)
	{
		foreach (KeyInfo item in keys)
		{
			if (item.KeyId == HDKKKCDKFEE)
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

	private KeyData.PressType GetPressType(List<int> AKFEAJDLIKF, List<int> LFKEMJBCMFL)
	{
		if (AKFEAJDLIKF.Count == 1)
		{
			return KeyData.PressType.BOTH;
		}
		foreach (int item in AKFEAJDLIKF)
		{
			foreach (int item2 in LFKEMJBCMFL)
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
