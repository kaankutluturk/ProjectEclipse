using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Fight
{
	public class EndFightContent : MonoBehaviour
	{
		[SerializeField]
		private string goldPrizeAlias = "goldPrize";

		[SerializeField]
		private string goldPerfectAlias = "goldPerfect";

		[SerializeField]
		private string goldFirstStrikeAlias = "goldFirstStrike";

		[SerializeField]
		private string goldComboAlias = "goldCombo";

		[SerializeField]
		private string goldShockAlias = "goldShock";

		[SerializeField]
		private GameObject _rewardItemPrefab;

		[SerializeField]
		private GameObject _textAndMoneyLinePrefab;

		[SerializeField]
		private GameObject _expAndMoneyLinePrefab;

		[SerializeField]
		private LabelButton _buttonOk;

		[SerializeField]
		private Transform _buttonPanel;

		private VerticalLayoutGroup ownLayout;

		private VerticalLayoutGroup parentLayout;

		private FightResult fightResult;

		private ItemRewardHardmode _itemReward;

		private List<Line> lines = new List<Line>();

		private List<ItemInfo> _items = new List<ItemInfo>();

		private int _currentLine;

		private const float RewardSpacing = 300f;

		private float savedParentSpacing;

		private float savedOwnSpacing;

		public UnityEvent AnimationEndEvent = new UnityEvent();

		public UnityEvent CloseEvent = new UnityEvent();

		private Button _animationFinishButton;

		public void Init(FightResult HEIADONEACH, VerticalLayoutGroup KPAICOOKACB, Button OBMBALDIBEB)
		{
			if (_buttonOk != null)
				((RectTransform)_buttonOk.transform).anchoredPosition = Vector2.zero;
			_animationFinishButton = OBMBALDIBEB;
			fightResult = HEIADONEACH;
			_currentLine = 0;
			_items = fightResult.Prize.GetItems(true);
			bool flag = _items.Count > 0;
			parentLayout = KPAICOOKACB;
			ownLayout = GetComponent<VerticalLayoutGroup>();
			if (!flag)
			{
				ShowResultLines();
			}
			else
			{
				ShowItemReward();
			}
		}

		private void ShowItemReward()
		{
			if (_animationFinishButton != null)
			{
				_animationFinishButton.gameObject.SetActive(false);
			}
			if ((bool)parentLayout)
			{
				savedParentSpacing = parentLayout.spacing;
				parentLayout.spacing = 300f;
			}
			if ((bool)ownLayout)
			{
				savedOwnSpacing = ownLayout.spacing;
				ownLayout.spacing = 300f;
			}
			if (_items.Count != 0)
			{
				_itemReward = Object.Instantiate(_rewardItemPrefab).GetComponent<ItemRewardHardmode>();
				_itemReward.gameObject.SetActive(true);
				_itemReward.transform.SetParent(base.transform, false);
				_itemReward.setIcon(_items[0]);
			}
			if (_buttonOk != null)
			{
				_buttonOk.onClick.AddListener(OnRewardOkClicked);
				_buttonOk.gameObject.SetActive(true);
			}
			if (_buttonPanel != null)
			{
				_buttonPanel.SetAsLastSibling();
			}
		}

		private void ShowResultLines()
		{
			if (_animationFinishButton != null)
			{
				_animationFinishButton.gameObject.SetActive(true);
			}
			if (_textAndMoneyLinePrefab != null)
			{
				lines.Add(CreateLine(goldPrizeAlias, fightResult.PlayerStatistics.Prize.BaseGold, fightResult.PlayerStatistics.Prize.Experience, string.Empty));
				lines.Add(CreateLine(goldPerfectAlias, fightResult.PlayerStatistics.Prize.PerfectGold, 0L, fightResult.PlayerStatistics.PerfectCount.ToString()));
				lines.Add(CreateLine(goldFirstStrikeAlias, fightResult.PlayerStatistics.Prize.FirstStrikeGold, 0L, fightResult.PlayerStatistics.FirstStrikeCount.ToString()));
				lines.Add(CreateLine(goldComboAlias, fightResult.PlayerStatistics.Prize.ComboGold, 0L, fightResult.PlayerStatistics.MaxCombo.ToString()));
				lines.Add(CreateLine(goldShockAlias, fightResult.PlayerStatistics.Prize.ShockGold, 0L, fightResult.PlayerStatistics.ShockCount.ToString()));
				lines.Add(CreateLine(fightResult.PlayerStatistics.StatisticCrazyStyleToString, fightResult.PlayerStatistics.Prize.StyleGold, 0L, string.Empty));
			}
			if (_expAndMoneyLinePrefab != null)
			{
				lines.Add(CreateLine((long)fightResult.ExpReward, fightResult.PlayerStatistics.Prize.TotalGold));
			}
			if (_buttonOk != null)
			{
				_buttonOk.onClick.AddListener(() =>
				{
					CloseEvent.Invoke();
				});
				_buttonOk.gameObject.SetActive(false);
			}
			if (_buttonPanel != null)
			{
				_buttonPanel.SetAsLastSibling();
			}
			StartNextLine();
		}

		private void OnRewardOkClicked()
		{
			if (_buttonOk != null)
			{
				_buttonOk.onClick.RemoveListener(OnRewardOkClicked);
			}
			if ((bool)parentLayout)
			{
				parentLayout.spacing = savedParentSpacing;
			}
			if ((bool)ownLayout)
			{
				ownLayout.spacing = savedOwnSpacing;
			}
			Object.DestroyObject(_itemReward.gameObject);
			ShowResultLines();
		}

		private void StartNextLine()
		{
			if (_currentLine < lines.Count)
			{
				Line fCMOHBLGJFP = lines[_currentLine];
				fCMOHBLGJFP.AddListener(OnLineFinished);
				fCMOHBLGJFP.AddListener(fCMOHBLGJFP.StartAnimation);
				fCMOHBLGJFP.StartAnimation();
				return;
			}
			Line fCMOHBLGJFP2 = ((lines.Count <= 0) ? null : lines[lines.Count - 1]);
			if (fCMOHBLGJFP2 != null)
			{
				fCMOHBLGJFP2.AddListener(OnAllLinesFinished);
			}
			foreach (Line item in lines)
			{
				item.RemoveListener(OnLineFinished);
				item.RemoveListener(item.StartAnimation);
			}
		}

		private void OnLineFinished()
		{
			StartCoroutine(AdvanceLineCoroutine());
		}

		private void OnAllLinesFinished()
		{
			AnimationEndEvent.Invoke();
			if (_buttonOk != null)
			{
				_buttonOk.gameObject.SetActive(true);
			}
		}

		private IEnumerator AdvanceLineCoroutine()
		{
			yield return new WaitForEndOfFrame();
			_currentLine++;
			StartNextLine();
		}

		public Line CreateLine(string HCPNFPMHFCM, long GBGNFPNCGED, long PAGGOKFIEOP = 0L, string BBLOBPOCGNM = "")
		{
			TextAndMoneyLine component = Object.Instantiate(_textAndMoneyLinePrefab).GetComponent<TextAndMoneyLine>();
			component.gameObject.SetActive(true);
			component.transform.SetParent(base.transform, false);
			component.Init(HCPNFPMHFCM, GBGNFPNCGED, PAGGOKFIEOP, BBLOBPOCGNM);
			return component;
		}

		public Line CreateLine(long exp, long GBGNFPNCGED)
		{
			ExpAndMoneyLine component = Object.Instantiate(_expAndMoneyLinePrefab).GetComponent<ExpAndMoneyLine>();
			component.gameObject.SetActive(true);
			component.transform.SetParent(base.transform, false);
			component.Init(exp, GBGNFPNCGED);
			return component;
		}

		public void FinishAnimation()
		{
			foreach (Line item in lines)
			{
				item.FinishAnimation();
			}
			OnAllLinesFinished();
		}
	}
}
