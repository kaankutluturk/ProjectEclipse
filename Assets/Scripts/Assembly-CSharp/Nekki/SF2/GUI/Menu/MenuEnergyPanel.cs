using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Menu
{
	public class MenuEnergyPanel : SFMonoBehaviour<object>
	{
		public enum MenuEnergyPanelEvent
		{
			onBarClicked = 0
		}

		private enum EnergyViewMode
		{
			InvisibleView = 0,
			NormalView = 1,
			UnlimitedView = 2
		}

		private EnergyViewMode viewMode;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private Image _iconMax;

		[SerializeField]
		private StepBar _bar;

		[SerializeField]
		private Button _dialogButton;

		public void Init()
		{
			int num = GameUtils.GetMaxPower();
			float num2 = 100f / (float)num;
			List<int> list = new List<int>();
			for (int i = 0; i <= num; i++)
			{
				list.Add((int)(num2 * (float)i));
			}
			_bar.Init();
			_bar.SetPercent(list);
			if (ListSF.GetRoster().HasUnlimitedEnergy)
			{
				_bar.gameObject.SetActive(false);
			}
			_dialogButton.onClick.AddListener(() =>
			{
				OnDialogButtonClicked();
			});
			UpdateView();
		}

		private void RemoveListeners()
		{
			_dialogButton.onClick.RemoveListener(() =>
			{
				OnDialogButtonClicked();
			});
		}

		public void UpdateView()
		{
			if (!ListSF.GetRoster().HasUnlimitedEnergy)
			{
				_icon.gameObject.SetActive(true);
				_bar.gameObject.SetActive(true);
				_iconMax.gameObject.SetActive(false);
				UpdateBar();
			}
			else
			{
				_icon.gameObject.SetActive(false);
				_bar.gameObject.SetActive(false);
				_iconMax.gameObject.SetActive(true);
			}
		}

		public void UpdateBar()
		{
			int num = ListSF.GetRoster().GetMaxPower();
			if (_bar.GetValue() != (float)num)
			{
				_bar.SetValue(num);
			}
		}

		public void SetDialogBtnPressType(ButtonStateExtensions.ButtonPressType LFLGCDNKNJI, bool GHJGPAEDIHG)
		{
			_dialogButton.SetPressType(LFLGCDNKNJI, GHJGPAEDIHG);
		}

		public virtual void SetTouchEnabled(bool value)
		{
			_dialogButton.gameObject.SetActive(value);
		}

		private void OnDialogButtonClicked()
		{
			CallEvent(0, 0);
		}
	}
}
