using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class PerkTreeLines : SFMonoBehaviour<object>
	{
		[SerializeField]
		private GameObject _linesForTwoPerks;

		[SerializeField]
		private GameObject _topLine;

		[SerializeField]
		private GameObject _bottomLine;

		public void Init(bool hasTwoPerks, bool isFirst, bool isLast)
		{
			_linesForTwoPerks.gameObject.SetActive(hasTwoPerks);
			_topLine.gameObject.SetActive(!isFirst);
			_bottomLine.gameObject.SetActive(!isLast);
		}

		private void Start()
		{
		}

		private void Update()
		{
		}
	}
}
