using UnityEngine;

public class FightControlEventData
{
	public class KeyboardBinding
	{
		public KeyCode Key;

		public FightCID Index;

		public int count;

		public bool isActive;

		public KeyboardBinding(KeyCode MHDOBFNEIAB, FightCID _index, int PGKCCLIHPMN, bool PFOPNLKPBOI = false)
		{
			Key = MHDOBFNEIAB;
			Index = _index;
			count = PGKCCLIHPMN;
			isActive = PFOPNLKPBOI;
		}
	}

	public FightCID Control;

	public int Index = -1;
}
