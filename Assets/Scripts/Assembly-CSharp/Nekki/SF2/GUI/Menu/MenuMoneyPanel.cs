using System;
using System.Collections.Generic;
using Eclipse.UI.TopBar;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Menu
{
	public class MenuMoneyPanel : SFMonoBehaviour<object>
	{
		public enum MenuMoneyPanelEvent
		{
			onRubyBtnClicked = 0
		}

		public enum ValuesSource
		{
			LocalValues = 0,
			ServerValues = 1
		}

		private enum MoneyViewMode
		{
			NormalView = 0,
			ForgeView = 1
		}

		public const float SALE_ANIM_TIME = 0.03f;

		public const float SALE_ANIM_PAUSE = 5f;

		private MoneyViewMode viewMode;

		private long rubyCount = -1L;

		private float rubyChangeStep;

		private float rubyChangeRemaining;

		[SerializeField]
		private Text _infoCoins;

		[SerializeField]
		private ResolutionImage _iconCoins;

		[SerializeField]
		private Text _infoBonus;

		[SerializeField]
		private ResolutionImage _iconBonus;

		[SerializeField]
		private Button _btnRuby;

		[SerializeField]
		private Button _btnServerValues;

		[SerializeField]
		private Button _btnGoToShop;

		[SerializeField]
		private ImageAnimation _picRubySale;

		private long saleEndTime = -1L;

		private ValuesSource valuesSource;

		public void Init()
		{
			InitRubyButtons();
			bool flag = SystemProperties.IsDebug();
			bool flag2 = false;
			if (flag || flag2)
			{
				_btnServerValues.onClick.AddListener(() =>
				{
					ToggleValuesSource();
				});
			}
			UpdateCoinsIcon();
		}

		public void ConfigureCompactTopBar()
		{
			DesktopTopBarLayout.ConfigureMoneyPanel(this, _btnRuby, _btnGoToShop, _picRubySale,
				_iconCoins.rectTransform, _infoCoins.rectTransform, _iconBonus.rectTransform, _infoBonus.rectTransform);
		}

		private void RemoveListeners()
		{
			_btnServerValues.onClick.RemoveListener(() =>
			{
				ToggleValuesSource();
			});
		}

		private void UpdateCoinsIcon()
		{
			if (_iconCoins != null && _iconCoins.get_SpriteName() != ListSF.GetRoster().GetCoinIcon())
			{
				_iconCoins.set_SpriteName(ListSF.GetRoster().GetCoinIcon());
			}
		}

		public void UpdateValues()
		{
			UpdateCoinsIcon();
			long num = ListSF.GetRoster().GetMoney();
			_infoCoins.text = num.ToString();
			long previousRubyCount = rubyCount;
			rubyCount = ListSF.GetRoster().GetBonus();
			if (valuesSource != ValuesSource.ServerValues)
			{
				_infoBonus.text = rubyCount.ToString();
			}
		}

		public void UpdateRuby()
		{
			string empty = string.Empty;
			if (valuesSource == ValuesSource.LocalValues)
			{
				if (rubyChangeRemaining != 0f)
				{
					if (Math.Abs(rubyChangeRemaining) > Math.Abs(rubyChangeStep))
					{
						rubyChangeRemaining -= rubyChangeStep;
					}
					else
					{
						rubyChangeRemaining = 0f;
					}
					empty = ((int)((float)rubyCount - rubyChangeRemaining)/*cast due to constrained. prefix*/).ToString();
				}
				else
				{
					empty = rubyCount.ToString();
				}
			}
			else
			{
				bool flag = false;
				int num = 0;
				empty = ((!flag) ? (-1) : num).ToString();
			}
			if (_infoBonus.text != empty)
			{
				_infoBonus.text = empty;
			}
		}

		public void UpdateRubySale()
		{
			List<ItemInfo> list = new List<ItemInfo>();
			long saleEndTime = -1L;
			for (int i = 0; i < list.Count; i++)
			{
				ItemInfo itemInfo = list[i];
			}
			SetSaleEndTime(saleEndTime);
		}

		public void SetRubyBtnPressType(ButtonStateExtensions.ButtonPressType pressType, bool isInteractable)
		{
			_btnRuby.SetPressType(pressType, isInteractable);
		}

		public void SetServerValuesBtnPressType(ButtonStateExtensions.ButtonPressType pressType, bool isInteractable)
		{
			_btnServerValues.SetPressType(pressType, isInteractable);
		}

		public void SetNormalViewMode()
		{
			viewMode = MoneyViewMode.NormalView;
			_iconCoins.gameObject.SetActive(true);
			_infoCoins.gameObject.SetActive(true);
		}

		public void SetForgeViewMode()
		{
			viewMode = MoneyViewMode.ForgeView;
			_iconCoins.gameObject.SetActive(false);
			_infoCoins.gameObject.SetActive(false);
		}

		public float GetRubyIconLeftEdgePosX()
		{
			float x = _iconBonus.transform.position.x;
			float x2 = base.transform.position.x;
			float num = x2 + x;
			float width = _iconBonus.rectTransform.rect.width;
			return num - width / 2f;
		}

		public Button GetRubyBtn()
		{
			return _btnRuby;
		}

		public Button GetServerValuesBtn()
		{
			return _btnServerValues;
		}

		public ValuesSource GetValuesSource()
		{
			return valuesSource;
		}

		public void SetValuesSource(ValuesSource newSource)
		{
			valuesSource = newSource;
			UpdateValues();
		}

		private void InitRubyButtons()
		{
			_btnRuby.onClick.AddListener(OnRubyButtonClicked);
			_btnGoToShop.onClick.AddListener(OnRubyButtonClicked);
		}

		private void OnRubyButtonClicked()
		{
			_btnRuby.enabled = false;
			MainMenu.MenuButtonType buttonType = MainMenu.MenuButtonType.MENU_MONEY;
			CallEvent(0, buttonType);
		}

		private void ToggleValuesSource()
		{
			if (valuesSource == ValuesSource.LocalValues)
			{
				SetValuesSource(ValuesSource.ServerValues);
			}
			else
			{
				SetValuesSource(ValuesSource.LocalValues);
			}
			UpdateRubySale();
		}

		private void SetSaleEndTime(long value)
		{
			if (saleEndTime != value)
			{
				saleEndTime = value;
				bool flag = false;
			}
		}

		private void OnSaleAnimationUpdate(float data)
		{
		}
	}
}
