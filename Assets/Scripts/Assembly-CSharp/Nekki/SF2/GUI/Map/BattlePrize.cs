using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class BattlePrize : SFMonoBehaviour<object>
	{
		public const float BATTLE_GOLD_SIZE = 40f;

		private List<BattlePrizeElement> prizeElements = new List<BattlePrizeElement>();

		private float maxWidth = -1f;

		private float itemIconHeight = -1f;

		private float unusedWidth;

		private float unusedHeight;

		private bool needsScroll;

		[SerializeField]
		private GameObject _prizeElemPrefab;

		[SerializeField]
		private HorizontalLayoutGroup _layoutGroup;

		[SerializeField]
		private ResolutionImage _itemIcon;

		private float totalContentWidth;

		private float _spacing = 20f;

		private static int scrollFrameCounter = 360;

		public void Init(long GBGNFPNCGED, long PAGGOKFIEOP, RewardPrize DPIIJICBGGA, float HOJOKAOLMGN, float AHKNBOHOOOK, int CFMPJLLNCFF)
		{
			float jPDGMJHNKPK = 1f;
			float jPDGMJHNKPK2 = 1f;
			foreach (BattlePrizeElement item in prizeElements)
			{
				Object.Destroy(item.gameObject);
			}
			prizeElements.Clear();
			_itemIcon.gameObject.SetActive(false);
			maxWidth = HOJOKAOLMGN;
			itemIconHeight = AHKNBOHOOOK;
			string aDONPNOBBDE = "MiscSprites.ruby";
			string aDONPNOBBDE2 = ListSF.GetRoster().GetCoinIcon();
			totalContentWidth = 0f;
			if (GBGNFPNCGED > 0)
			{
				BattlePrizeElement component = Object.Instantiate(_prizeElemPrefab).GetComponent<BattlePrizeElement>();
				component.Init(aDONPNOBBDE2, GBGNFPNCGED, CFMPJLLNCFF, jPDGMJHNKPK);
				AddPrizeElement(component);
			}
			if (PAGGOKFIEOP > 0)
			{
				BattlePrizeElement component2 = Object.Instantiate(_prizeElemPrefab).GetComponent<BattlePrizeElement>();
				component2.Init(aDONPNOBBDE, PAGGOKFIEOP, CFMPJLLNCFF, jPDGMJHNKPK2);
				AddPrizeElement(component2);
			}
			foreach (RewardCurrency item2 in DPIIJICBGGA.currencyRewards)
			{
				if (item2.ShowReward)
				{
					GameCurrency cJJOFMHLFFM = GameUtils.GameCurrencies.GetCurrencyByName(item2.Name);
					if (cJJOFMHLFFM != null)
					{
						string mJBPMLCLMFN = cJJOFMHLFFM.Icon;
						long bAINMLLIKOL = item2.GetMinimumAmount();
						BattlePrizeElement component3 = Object.Instantiate(_prizeElemPrefab).GetComponent<BattlePrizeElement>();
						component3.Init(mJBPMLCLMFN, bAINMLLIKOL, CFMPJLLNCFF);
						AddPrizeElement(component3);
					}
				}
			}
			foreach (RewardResistance item3 in DPIIJICBGGA.resistanceRewards)
			{
				if (item3.ShowReward)
				{
					GameResistance oOJJEOFENBJ = GameUtils.GameResistances.GetResistanceByName(item3.Name);
					if (oOJJEOFENBJ != null)
					{
						string aDONPNOBBDE3 = oOJJEOFENBJ.GetIcon();
						long bAINMLLIKOL2 = item3.Value;
						BattlePrizeElement component4 = Object.Instantiate(_prizeElemPrefab).GetComponent<BattlePrizeElement>();
						component4.Init(aDONPNOBBDE3, bAINMLLIKOL2, CFMPJLLNCFF);
						AddPrizeElement(component4);
					}
				}
			}
			_layoutGroup.spacing = _spacing;
			_layoutGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(totalContentWidth, _layoutGroup.GetComponent<RectTransform>().sizeDelta.y);
			UpdateScrollState();
			foreach (RewardItem item4 in DPIIJICBGGA.items)
			{
				if (item4.ShowReward)
				{
					ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(item4.Name);
					UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(dJKEECEOCJB);
					if (dKCHDHMLKHN == null)
					{
						AddItem(dJKEECEOCJB);
						break;
					}
				}
			}
		}

		public void AddItem(ItemInfo PJDAGCBPLJE)
		{
			if (PJDAGCBPLJE.Type == "Seal")
			{
				_itemIcon.set_TexturePath(SF2Paths.GetUsersUiPath());
			}
			else
			{
				_itemIcon.set_TexturePath(SF2Paths.GetItemsUiPath());
			}
			_itemIcon.set_SpriteName(PJDAGCBPLJE.FileName);
			_itemIcon.gameObject.SetActive(true);
			_itemIcon.preserveAspect = true;
			foreach (BattlePrizeElement item in prizeElements)
			{
				item.gameObject.SetActive(false);
			}
			UpdateScrollState();
		}

		private void Update()
		{
			if (needsScroll)
			{
				ScrollPrizes();
			}
		}

		private void AddPrizeElement(BattlePrizeElement DGNDGHPMPJD)
		{
			if (DGNDGHPMPJD != null)
			{
				totalContentWidth += DGNDGHPMPJD.GetComponent<LayoutElement>().preferredWidth;
				totalContentWidth += _spacing;
				DGNDGHPMPJD.transform.SetParent(_layoutGroup.transform, false);
				prizeElements.Add(DGNDGHPMPJD);
			}
		}

		private void UpdateScrollState()
		{
			needsScroll = _layoutGroup.GetComponent<RectTransform>().rect.width > GetComponent<RectTransform>().rect.width;
			if (null != _itemIcon)
			{
				_itemIcon.rectTransform.sizeDelta = new Vector2(500f, itemIconHeight);
			}
		}

		private void ScrollPrizes()
		{
			scrollFrameCounter++;
			float mNADIKCPPIG = MapGUI.RewardLineOscillation.OscillationPeriod;
			float mIFFMBOIAGC = MapGUI.RewardLineOscillation.OscillationFactor;
			float num = _layoutGroup.GetComponent<RectTransform>().rect.width - GetComponent<RectTransform>().rect.width;
			float num2 = num / (2f * mNADIKCPPIG);
			float bAINMLLIKOL = num2 * mNADIKCPPIG * Mathf.Cos(mIFFMBOIAGC * (float)scrollFrameCounter);
			_layoutGroup.transform.SetLocalX(bAINMLLIKOL);
		}
	}
}
