using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Nekki.SF2.GUI.Fight
{
	public class ExpAndMoneyLine : MonoBehaviour, Line
	{
		[SerializeField]
		private IconAndText exp;

		[SerializeField]
		private IconAndText moneyCount;

		[SerializeField]
		private float moneyAddTime;

		private DG.Tweening.Sequence sequence;

		private UnityEvent endEvent = new UnityEvent();

		private long targetExp;

		private long displayedExp;

		private long targetMoney;

		private long displayedMoney;

		private bool needShowExpAndMoney;

		public void Init(long exp, long GBGNFPNCGED)
		{
			needShowExpAndMoney = true;
			targetExp = exp;
			displayedExp = 0L;
			targetMoney = GBGNFPNCGED;
			displayedMoney = 0L;
			if (moneyCount != null)
			{
				moneyCount.SetIcon(ListSF.GetRoster().GetCoinIcon());
			}
			VisibleExpAndMoney(false);
		}

		public void StartAnimation()
		{
			if (needShowExpAndMoney)
			{
				VisibleExpAndMoney(true);
				needShowExpAndMoney = false;
				endEvent.Invoke();
			}
			else
			{
				PlayCountAnimation();
			}
		}

		public void VisibleExpAndMoney(bool KFIECNIMAOA)
		{
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
			numberFormatInfo.NumberGroupSeparator = " ";
			NumberFormatInfo numberFormatInfo2 = numberFormatInfo;
			if (exp != null)
			{
				exp.SetText(displayedExp.ToString("N0", numberFormatInfo2));
				exp.gameObject.SetActive(KFIECNIMAOA);
			}
			if (moneyCount != null)
			{
				moneyCount.SetText(displayedMoney.ToString("N0", numberFormatInfo2));
				moneyCount.gameObject.SetActive(KFIECNIMAOA);
			}
		}

		private void PlayCountAnimation()
		{
			NumberFormatInfo f = new NumberFormatInfo
			{
				NumberGroupSeparator = " "
			};
			sequence = DOTween.Sequence();
			if (moneyCount != null && exp != null)
			{
				Tweener t = DOTween.To(() => displayedMoney, (long DHDMNHCIPEH) =>
				{
					displayedMoney = DHDMNHCIPEH;
					moneyCount.SetText(displayedMoney.ToString("N0", f));
				}, targetMoney, moneyAddTime);
				sequence.Append(t);
				Tweener t2 = DOTween.To(() => displayedExp, (long DHDMNHCIPEH) =>
				{
					displayedExp = DHDMNHCIPEH;
					exp.SetText(displayedExp.ToString("N0", f));
				}, targetExp, moneyAddTime);
				sequence.Join(t2);
				sequence.AppendCallback(() =>
				{
					endEvent.Invoke();
				});
			}
			else
			{
				endEvent.Invoke();
			}
		}

		public void AddListener(UnityAction ODDEOFKLIAG)
		{
			endEvent.AddListener(ODDEOFKLIAG);
		}

		public void RemoveListener(UnityAction ODDEOFKLIAG)
		{
			endEvent.RemoveListener(ODDEOFKLIAG);
		}

		public void FinishAnimation()
		{
			if (sequence != null)
			{
				sequence.Kill();
				sequence = null;
			}
			VisibleExpAndMoney(true);
			if (exp != null)
			{
				displayedExp = targetExp;
				exp.SetText(displayedExp.ToString());
			}
			if (moneyCount != null)
			{
				displayedMoney = targetMoney;
				moneyCount.SetText(displayedMoney.ToString());
			}
		}
	}
}
