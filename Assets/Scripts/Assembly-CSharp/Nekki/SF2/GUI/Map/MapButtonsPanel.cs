using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Map
{
	public class MapButtonsPanel : MonoBehaviour
	{
		[SerializeField]
		private Transform _actionsPanel;

		[SerializeField]
		private GameObject _mapButtonPrefab;

		private List<MapButton> _buttons = new List<MapButton>();
		private bool _storyButtonsVisible = true;

		public void Init()
		{
			MapButtonController.GetInstance().AddEventListener(0, OnButtonAdded);
			MapButtonController.GetInstance().AddEventListener(1, OnButtonRemoved);
			AddButtons();
		}

		private void OnDestroy()
		{
			MapButtonController.GetInstance().RemoveEventListener(0, OnButtonAdded);
			MapButtonController.GetInstance().RemoveEventListener(1, OnButtonRemoved);
		}

		private void OnButtonAdded(MapButtonInfo KLNKEPMAGKF)
		{
			if (KLNKEPMAGKF != null && MapButtonController.GetInstance().IsStoryButton(KLNKEPMAGKF))
			{
				AddButton(KLNKEPMAGKF);
			}
		}

		private void OnButtonRemoved(MapButtonInfo KLNKEPMAGKF)
		{
			if (KLNKEPMAGKF != null)
			{
				RemoveButton(KLNKEPMAGKF);
			}
		}

		public void AddButtons()
		{
			List<MapButtonInfo> list = MapButtonController.GetInstance().GetStoryButtons();
			foreach (MapButtonInfo item in list)
			{
				if (item != null)
				{
					AddButton(item);
				}
			}
		}

		public void AddButton(MapButtonInfo KLNKEPMAGKF)
		{
			Transform kPAICOOKACB = ((!KLNKEPMAGKF.AutoPosition || !(_actionsPanel != null)) ? base.transform : _actionsPanel);
			AddButton(KLNKEPMAGKF, kPAICOOKACB);
		}

		public void AddButton(MapButtonInfo DJDNMAOEFBD, Transform KPAICOOKACB)
		{
			if (_mapButtonPrefab != null)
			{
				MapButton component = Object.Instantiate(_mapButtonPrefab).GetComponent<MapButton>();
				component.gameObject.SetActive(true);
				component.transform.SetParent(KPAICOOKACB, false);
				component.Init(DJDNMAOEFBD);
				component.gameObject.SetActive(_storyButtonsVisible || DJDNMAOEFBD.GetShowType() != MapButtonInfo.MapButtonShowType.Story);
				_buttons.Add(component);
			}
		}

		public void RemoveButtons()
		{
			for (int i = _buttons.Count - 1; i >= 0; i--)
			{
				RemoveButton(_buttons[i]);
			}
		}

		public void SetStoryButtonsVisible(bool visible)
		{
			_storyButtonsVisible = visible;
			foreach (MapButton button in _buttons)
			{
				MapButtonInfo info = button.get_MapButtonInfo();
				bool storyOnly = info != null && info.GetShowType() == MapButtonInfo.MapButtonShowType.Story;
				button.gameObject.SetActive(visible || !storyOnly);
			}
		}

		public void RemoveButton(MapButtonInfo DJDNMAOEFBD)
		{
			MapButton mapButton = _buttons.Find((MapButton DHDMNHCIPEH) => DHDMNHCIPEH.get_MapButtonInfo() == DJDNMAOEFBD);
			if (mapButton != null)
			{
				RemoveButton(mapButton);
			}
		}

		public void RemoveButton(MapButton KLNKEPMAGKF)
		{
			_buttons.Remove(KLNKEPMAGKF);
			KLNKEPMAGKF.gameObject.SetActive(false);
			Object.Destroy(KLNKEPMAGKF.gameObject);
		}
	}
}
