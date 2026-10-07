using System;
using CodeStage.AntiCheat.ObscuredTypes;
using Eclipse.Underworld.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Fight
{
	[Serializable]
	public class ScreenModel : global::EventDispatcher<object>
	{
		public struct ComboChangedEventData
		{
			public ComboModel.ComboChangeInfo Info;
		}

		public enum ScreenModelEvent
		{
			ON_STYLE_CHANGED = 0,
			ON_COMBO_UP = 1,
			ON_CLICK_CHEAT = 2
		}

		public enum ScreenSide
		{
			TYPE_LEFT = 0,
			TYPE_RIGHT = 1
		}

		[SerializeField]
		private ScreenSide _Type;

		[SerializeField]
		private ResolutionImageAvatar _Avatar;

		[SerializeField]
		private PlayerLifeBar _lifeBar;

		private UnderworldRaidShieldBar _raidShields;

		[SerializeField]
		private StylePanel _stylePanel;

		[SerializeField]
		private ResolutionImage _styleName;

		[SerializeField]
		private RoundsPanel _roundsPanel;

		[SerializeField]
		private LabelAlias _name;

		[SerializeField]
		private ComboModel _comboModel;

		[SerializeField]
		private ActivePerkModel _activePerkModel;

		[SerializeField]
		private GameObject _WinBtn;

		private ModelParameters _parameters;

		private bool _showRounds = true;

		public bool IsNoBlock { get; set; }

		public ComboStatistic Statistic
		{
			get
			{
				return (!(_comboModel != null)) ? null : _comboModel.get_ComboStatistic();
			}
			set
			{
				if (_comboModel != null)
				{
					_comboModel.set_ComboStatistic(value);
				}
			}
		}

		public FightStatistics.FightStyle MaxStyle
		{
			get
			{
				return (_stylePanel != null) ? _stylePanel.get_MaximumStyleStrip() : FightStatistics.FightStyle.STYLE_TURTLE;
			}
		}

		public int CurrentStyleStrip
		{
			get
			{
				return (_stylePanel != null) ? _stylePanel.get_CurrentStyleStrip() : 0;
			}
		}

		public string CurrentStyleName
		{
			get
			{
				return (!(_stylePanel != null)) ? string.Empty : _stylePanel.get_CurrentStyleName();
			}
		}

		public float CurrentStyleValue
		{
			get
			{
				return (!(_stylePanel != null)) ? 0f : _stylePanel.GetStyleValue();
			}
		}

		public void Init(ModelParameters parameters, bool showRounds = true)
		{
			_parameters = parameters;
			_showRounds = showRounds;
			if (_roundsPanel != null)
				_roundsPanel.gameObject.SetActive(_showRounds);
			IsNoBlock = true;
			SetupAvatar();
			SetupLifeBar();
			SetupStylePanel();
			if (_showRounds)
			{
				SetupRoundsPanel();
			}
			SetupName();
			SetupComboModel();
			SetupActivePerkModel();
			SetupWinButton();
		}

		private void SetupAvatar()
		{
			_Avatar.set_TexturePath(SF2Paths.GetUsersUiPath());
			_Avatar.set_SpriteName(_parameters.Avatar);
			_Avatar.SetNativeSize();
		}

        internal bool RefreshForm(ModelParameters expected, ModelParameters replacement)
        {
            if (expected == null || replacement == null || _parameters != expected) return false;
            _parameters = replacement;
            SetupAvatar();
            SetupLifeBar();
            SetupName();
            return true;
        }

		private void SetupLifeBar()
		{
			if (_raidShields != null)
			{
				UnityEngine.Object.Destroy(_raidShields.gameObject);
				_raidShields = null;
			}
			_lifeBar.Init(_parameters);
			bool raidBoss = _Type == ScreenSide.TYPE_RIGHT && _parameters != null && _parameters.ShieldTotal > 0;
			_lifeBar.SetRaidStyle(raidBoss);
			if (raidBoss)
			{
				RectTransform lifeBarRect = _lifeBar.get_rectTransform();
				_raidShields = UnderworldRaidShieldBar.Attach(lifeBarRect, _parameters, _name.font);
			}
		}

		private void SetupStylePanel()
		{
			_stylePanel.Init(_styleName);
		}

		private void SetupRoundsPanel()
		{
			if (_parameters != null)
			{
				_roundsPanel.Init(_parameters.RoundTotal);
			}
		}

		private void SetupName()
		{
			if (!(_name == null))
			{
				if (_parameters.DisplayName != null && !_parameters.DisplayName.Equals(string.Empty))
				{
					_name.set_text(_parameters.DisplayName);
				}
				else
				{
					_name.SetAlias(_parameters.FirstName);
					// The fight HUD has one line for the name. Show a {br} two-line name
					// ("SON OF{br}HEAVEN") on one line, shrunk to fit if needed.
					string text = _name.get_text();
					if (text.IndexOf('\n') >= 0)
					{
						_name.set_text(text.Replace("\r", string.Empty).Replace('\n', ' '));
						_name.resizeTextMinSize = Mathf.Max(1, _name.fontSize / 2);
						_name.resizeTextMaxSize = _name.fontSize;
						_name.resizeTextForBestFit = true;
					}
				}
			}
		}

		private void SetupComboModel()
		{
			_comboModel.Init(_Type);
			_comboModel.AddEventListener(0, OnComboChanged);
		}

		private void SetupWinButton()
		{
			_WinBtn.SetActive(SystemProperties.IsDebug());
			_WinBtn.GetComponent<Button>().onClick.AddListener(() =>
			{
				CallEvent(2, _Type);
			});
		}

		private void SetupActivePerkModel()
		{
			_activePerkModel.Init();
		}

		public void UpdateStyle(InfoAnimation animation)
		{
			if (_stylePanel != null)
			{
				_stylePanel.UpdateStyle(animation);
				_comboModel.AddCrazyStyle(_stylePanel.get_CurrentStyleStrip());
				RaiseStyleChanged(true);
			}
		}

		public void IncreaseStyleByValue(float value)
		{
			if (_stylePanel != null)
			{
				_stylePanel.IncreaseStyleStripByValue(value);
				_comboModel.AddCrazyStyle(_stylePanel.get_CurrentStyleStrip());
				RaiseStyleChanged(true);
			}
		}

		public void UpdateVictories()
		{
			if (_showRounds && !(_roundsPanel == null))
			{
				_roundsPanel.UpdateVictories(_parameters.RoundsWon);
			}
		}

		public void SetFightPaused(bool value)
		{
			if (_comboModel != null)
			{
				_comboModel.OnFightPause(value);
			}
		}

		public void SetLifeBarVisible(bool value)
		{
			if (_lifeBar != null)
			{
				_lifeBar.gameObject.SetActive(value);
			}
			if (_raidShields != null)
			{
				_raidShields.SetVisible(value);
			}
		}

		public void SetLifeUpdateLocked(bool value)
		{
			if (_lifeBar != null)
			{
				_lifeBar.set_LockLifeUpdate(value);
			}
		}

		public void NotifyStyleChanged()
		{
			RaiseStyleChanged(true);
		}

		public void RenderComboModel()
		{
			if (_comboModel != null)
			{
				_comboModel.Render();
			}
		}

		public void RenderActivePerks()
		{
			if (_activePerkModel != null)
			{
				_activePerkModel.Render();
			}
		}

		public void Render(bool renderStylePanel)
		{
			if (_lifeBar != null)
			{
				_lifeBar.Render();
			}
			if (_raidShields != null && _parameters != null)
			{
				_raidShields.UpdateBar((float)_parameters.GetCurrentLife());
			}
			if (_stylePanel != null && renderStylePanel)
			{
				_stylePanel.Render();
				RaiseStyleChanged(false);
			}
		}

		public void Reset()
		{
			if (_lifeBar != null)
			{
				_lifeBar.ResetLife();
			}
			if (_stylePanel != null)
			{
				_stylePanel.ResetStyle();
			}
			if (_comboModel != null)
			{
				_comboModel.ResetComboStrike();
			}
			IsNoBlock = true;
		}

		public void AddCriticalCombo()
		{
			if (_comboModel != null)
			{
				_comboModel.CreateCritical();
			}
		}

		public void AddFirstStrikeCombo()
		{
			if (_comboModel != null)
			{
				_comboModel.CreateFirstStrike();
			}
		}

		public void AddHeadStrikeCombo()
		{
			if (_comboModel != null)
			{
				_comboModel.CreateHeadStrike();
			}
		}

		public void AddPerfect()
		{
			if (_comboModel != null)
			{
				_comboModel.AddPerfect();
			}
		}

		public void AddShockCombo()
		{
			if (_comboModel != null)
			{
				_comboModel.CreateShock();
			}
		}

		public void ShowHotGroundTimer(int time)
		{
			if (_comboModel != null)
			{
				_comboModel.UpdateHotGroundTimer(time);
			}
		}

		public void UpdateCombo(int value, int countOffset)
		{
			if (_comboModel != null)
			{
				_comboModel.UpdateCombo(value, countOffset);
			}
		}

		public void RemoveAllCombos()
		{
			if (_comboModel != null)
			{
				_comboModel.RemoveAllCombo();
			}
		}

		public void AddActivePerk(PerksStage.ActionPerk actionPerk)
		{
			if (_activePerkModel != null)
			{
				_activePerkModel.AddActivePerkItem(actionPerk);
			}
		}

		public void AddEffectPerk(PerksStage.ActionPerk actionPerk, PerksStage.ActionPerk otherPerk)
		{
			if (_activePerkModel != null)
			{
				_activePerkModel.AddEffectPerk(actionPerk, otherPerk);
			}
		}

		public void RemoveActivePerk(PerksStage.ActionPerk actionPerk)
		{
			if (_activePerkModel != null)
			{
				_activePerkModel.RemoveActivePerkItem(actionPerk);
			}
		}

		public void HideAllActivePerks()
		{
			if (_activePerkModel != null)
			{
				_activePerkModel.RemoveAllActivePerkItem();
			}
		}

		public void DestroyAllActivePerks()
		{
			if (_activePerkModel != null)
			{
				_activePerkModel.DestroyAllPerkItems();
			}
		}

		private void RaiseStyleChanged(bool isHit)
		{
			ModelStyleChange styleChange = new ModelStyleChange();
			styleChange.Side = _Type;
			styleChange.StyleIndex = CurrentStyleStrip;
			styleChange.StyleName = CurrentStyleName;
			styleChange.StyleGain = CurrentStyleValue;
			styleChange.IsHit = isHit;
			CallEvent(0, styleChange);
		}

		private void OnComboChanged(ComboModel.ComboChangeInfo info)
		{
			CallEvent(1, new ComboChangedEventData
			{
				Info = info
			});
		}
	}
}
