using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Menu
{
	public class MenuExpPanel : SFMonoBehaviour<object>
	{
		public enum MenuExpPanelEvent
		{
			onLevelHintBtnClicked = 0
		}

		[SerializeField]
		private Image _iconLevel;

		[SerializeField]
		private Text _labelLevel;

		[SerializeField]
		private ProgressBar _barExp;

		[SerializeField]
		private Image _iconMaxLevel;

		[SerializeField]
		private Button _levelHintButton;

		[SerializeField]
		private Text _hintText;

		[SerializeField]
		private GameObject _hintRootGO;

		private int hintFrameDelay;

		public void ShowHint(string hintText)
		{
			_hintRootGO.SetActive(true);
			_hintText.text = hintText;
			hintFrameDelay = 1;
		}

		public void HideHint()
		{
			hintFrameDelay = 0;
			_hintRootGO.SetActive(false);
		}

		public void Init()
		{
			Font font = LocalizationManager.GetContentFont();
			if (_hintText != null && font != null)
			{
				_hintText.font = font;
			}
			_levelHintButton.onClick.AddListener(() =>
			{
				OnLevelHintClicked();
			});
			_barExp.Init();
			_barExp.SetValueBorders(0f, 1f);
			_barExp.SetValue(0f);
			HideHint();
			UpdateLevel();
		}

		private void Update()
		{
			if (hintFrameDelay <= 0 && Eclipse.Input.EclipseInput.anyKeyDown)
			{
				HideHint();
			}
			else
			{
				hintFrameDelay--;
			}
		}

		private void RemoveListeners()
		{
			_levelHintButton.onClick.RemoveListener(() =>
			{
				OnLevelHintClicked();
			});
		}

		public void UpdateLevel()
		{
			string text = ListSF.GetRoster().GetLevel().ToString();
			_labelLevel.text = text;
		}

		public void UpdateBarExp(float experience, float experienceToNextLevel)
		{
			if (experience != 0f && _barExp.GetValue() == experience)
			{
				return;
			}
			_barExp.SetValueBorders(0f, experienceToNextLevel);
			_barExp.SetValue(experience);
			int num = ListSF.GetRoster().GetLevel();
			int count = GameUtils.LevelThresholdTable.Thresholds.Count;
			if (count != 0)
			{
				global::Pair<int, uint> maxLevelEntry = GameUtils.LevelThresholdTable.Thresholds[count - 1];
				int maxLevel = maxLevelEntry.First;
				if (num >= maxLevel)
				{
					_iconMaxLevel.gameObject.SetActive(true);
					_barExp.gameObject.SetActive(false);
					_levelHintButton.SetPressType(ButtonStateExtensions.ButtonPressType.PressInactive);
				}
				else
				{
					_iconMaxLevel.gameObject.SetActive(false);
					_barExp.gameObject.SetActive(true);
					_levelHintButton.SetPressType(ButtonStateExtensions.ButtonPressType.PressNormal);
				}
			}
		}

		private void OnLevelHintClicked()
		{
			CallEvent(0, 0);
		}

		public void SetHintBtnPressType(ButtonStateExtensions.ButtonPressType pressType, bool isInteractable)
		{
			if (_iconMaxLevel.gameObject.activeSelf)
			{
				_levelHintButton.SetPressType(ButtonStateExtensions.ButtonPressType.PressInactive);
			}
			else
			{
				_levelHintButton.SetPressType(pressType, isInteractable);
			}
		}

		public virtual void SetTouchEnabled(bool value)
		{
			_levelHintButton.gameObject.SetActive(value);
		}
	}
}
