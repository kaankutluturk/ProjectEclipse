using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Nekki.SF2.GUI.Fight
{
	public class TextAndMoneyLine : MonoBehaviour, Line
	{
		[SerializeField]
		private LabelAlias textLabel;

		[SerializeField]
		private IconAndText moneyCount;

		[SerializeField]
		private IconAndText rubyCount;

		[SerializeField]
		private Vector2 basePos;

		[SerializeField]
		private float textMoveTime;

		[SerializeField]
		private float moneyAddTime;

		private Vector2 labelTargetPosition;

		private long targetMoney;

		private long displayedMoney;

		private long targetRubies;

		private long displayedRubies;

		private bool needShowLabel;

		private DG.Tweening.Sequence labelSequence;

		private DG.Tweening.Sequence countSequence;

		private UnityEvent endEvent = new UnityEvent();

		public void Init(string HCPNFPMHFCM, long GBGNFPNCGED, long PAGGOKFIEOP, string BBLOBPOCGNM = "")
		{
			needShowLabel = true;
			targetMoney = GBGNFPNCGED;
			displayedMoney = 0L;
			targetRubies = PAGGOKFIEOP;
			displayedRubies = 0L;
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
			numberFormatInfo.NumberGroupSeparator = " ";
			NumberFormatInfo numberFormatInfo2 = numberFormatInfo;
			if (textLabel != null && !string.IsNullOrEmpty(HCPNFPMHFCM))
			{
				if (BBLOBPOCGNM != null)
				{
					HCPNFPMHFCM = string.Format("{0}{1}", HCPNFPMHFCM, "{" + BBLOBPOCGNM + "}");
				}
				textLabel.SetAlias(HCPNFPMHFCM);
				// The recovered label has a zero-width rect. Give the localized text
				// a measurable box before its entrance tween captures the destination.
				textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(1f, textLabel.preferredWidth));
				labelTargetPosition = textLabel.transform.localPosition;
				textLabel.transform.localPosition = basePos;
			}
			if (moneyCount != null)
			{
				moneyCount.SetIcon(ListSF.GetRoster().GetCoinIcon());
				moneyCount.SetText(displayedMoney.ToString("N0", numberFormatInfo2));
				moneyCount.gameObject.SetActive(false);
			}
			if (rubyCount != null)
			{
				rubyCount.SetText(displayedRubies.ToString("N0", numberFormatInfo2));
				rubyCount.gameObject.SetActive(false);
			}
		}

		public void StartAnimation()
		{
			if (needShowLabel)
			{
				PlayLabelAnimation();
				needShowLabel = false;
			}
			else
			{
				PlayCountAnimation();
			}
		}

		private void PlayLabelAnimation()
		{
			labelSequence = DOTween.Sequence();
			if (textLabel != null)
			{
				labelSequence.Append(textLabel.transform.DOLocalMove(labelTargetPosition, textMoveTime));
			}
			labelSequence.AppendCallback(() =>
			{
				if ((targetMoney > 0 || targetRubies < 1) && moneyCount != null)
				{
					moneyCount.gameObject.SetActive(true);
				}
				if (targetRubies > 0 && rubyCount != null)
				{
					rubyCount.gameObject.SetActive(true);
				}
			});
			labelSequence.AppendCallback(() =>
			{
				endEvent.Invoke();
			});
		}

		private void PlayCountAnimation()
		{
			NumberFormatInfo f = new NumberFormatInfo
			{
				NumberGroupSeparator = " "
			};
			countSequence = DOTween.Sequence();
			if (moneyCount != null && rubyCount != null)
			{
				Tweener t = DOTween.To(() => displayedMoney, (long DHDMNHCIPEH) =>
				{
					displayedMoney = DHDMNHCIPEH;
					moneyCount.SetText(displayedMoney.ToString("N0", f));
				}, targetMoney, moneyAddTime);
				countSequence.Append(t);
				if (targetRubies > 0)
				{
					Tweener t2 = DOTween.To(() => displayedRubies, (long DHDMNHCIPEH) =>
					{
						displayedRubies = DHDMNHCIPEH;
						rubyCount.SetText(displayedRubies.ToString("N0", f));
					}, targetRubies, moneyAddTime);
					countSequence.Join(t2);
				}
				countSequence.AppendCallback(() =>
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
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
			numberFormatInfo.NumberGroupSeparator = " ";
			NumberFormatInfo numberFormatInfo2 = numberFormatInfo;
			if (labelSequence != null)
			{
				labelSequence.Kill();
				labelSequence = null;
			}
			if (countSequence != null)
			{
				countSequence.Kill();
				countSequence = null;
			}
			if (textLabel != null)
			{
				textLabel.transform.localPosition = labelTargetPosition;
			}
			if ((targetMoney > 0 || targetRubies < 1) && moneyCount != null)
			{
				moneyCount.gameObject.SetActive(true);
				displayedMoney = targetMoney;
				moneyCount.SetText(displayedMoney.ToString("N0", numberFormatInfo2));
			}
			if (targetRubies > 0 && rubyCount != null)
			{
				rubyCount.gameObject.SetActive(true);
				displayedRubies = targetRubies;
				rubyCount.SetText(displayedRubies.ToString("N0", numberFormatInfo2));
			}
		}
	}
}
