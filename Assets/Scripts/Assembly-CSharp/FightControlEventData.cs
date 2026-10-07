using UnityEngine;

public class FightControlEventData
{
	public class KeyboardBinding
	{
		public KeyCode Key;

		public FightCID Index;

		public int count;

		public bool isActive;

		public KeyboardBinding(KeyCode keyCode, FightCID _index, int pressCount, bool active = false)
		{
			Key = keyCode;
			Index = _index;
			count = pressCount;
			isActive = active;
		}
	}

	public FightCID Control;

	public int Index = -1;
}
