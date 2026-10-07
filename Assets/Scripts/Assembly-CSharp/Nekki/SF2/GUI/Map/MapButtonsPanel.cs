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

		private void OnButtonAdded(MapButtonInfo buttonInfo)
		{
			if (buttonInfo != null && MapButtonController.GetInstance().IsStoryButton(buttonInfo))
			{
				AddButton(buttonInfo);
			}
		}

		private void OnButtonRemoved(MapButtonInfo buttonInfo)
		{
			if (buttonInfo != null)
			{
				RemoveButton(buttonInfo);
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

		public void AddButton(MapButtonInfo buttonInfo)
		{
			Transform parentTransform = ((!buttonInfo.AutoPosition || !(_actionsPanel != null)) ? base.transform : _actionsPanel);
			AddButton(buttonInfo, parentTransform);
		}

		public void AddButton(MapButtonInfo buttonInfo, Transform parent)
		{
			if (_mapButtonPrefab != null)
			{
				MapButton component = Object.Instantiate(_mapButtonPrefab).GetComponent<MapButton>();
				component.gameObject.SetActive(true);
				component.transform.SetParent(parent, false);
				component.Init(buttonInfo);
				component.gameObject.SetActive(_storyButtonsVisible || buttonInfo.GetShowType() != MapButtonInfo.MapButtonShowType.Story);
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

		public void RemoveButton(MapButtonInfo buttonInfo)
		{
			MapButton mapButton = _buttons.Find((MapButton button) => button.get_MapButtonInfo() == buttonInfo);
			if (mapButton != null)
			{
				RemoveButton(mapButton);
			}
		}

		public void RemoveButton(MapButton button)
		{
			_buttons.Remove(button);
			button.gameObject.SetActive(false);
			Object.Destroy(button.gameObject);
		}
	}
}
