using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Shop
{
	public class ParametersPanelContent : SidePanelContent
	{
		[SerializeField]
		private BaseScrollContent _baseScrollContent;

		[SerializeField]
		private ItemsScroll _itemsScroll;

		[SerializeField]
		private GameObject _scrollItemPrefab;

		private List<BaseScrollItem> _scrollItems = new List<BaseScrollItem>();

		private ModelParameters _previewParameters;

		private EquippedItemsStruct _equippedItems = new EquippedItemsStruct();

		public override void Init()
		{
			CreateItems();
			if (_baseScrollContent != null && _scrollItems != null)
			{
				_baseScrollContent.SetItems(_scrollItems);
			}
			else
			{
				GameLog.Error("ParametersPanelContent.Init _baseScrollContent or _items is null");
			}
			if (_itemsScroll != null)
			{
				_itemsScroll.Init();
			}
			else
			{
				GameLog.Error("ParametersPanelContent.Init _itemsScroll is null");
			}
		}

		private bool IsHiddenAttribute(WarriorAttribute FFLFOELEKIG)
		{
			return FFLFOELEKIG.IsHidden || FFLFOELEKIG.IsShopHidden;
		}

		private void CreateItems()
		{
			_previewParameters = new ModelParameters(ListSF.GetRoster().get_Parameters());
			if (_previewParameters == null)
			{
				GameLog.Error("ParametersPanelContent.CreateItems modelParameters is null");
				return;
			}
			_previewParameters.CalculateAttributes();
			foreach (WarriorAttribute item in GameUtils.WarriorAttributeList.AttributeList)
			{
				if (!IsHiddenAttribute(item))
				{
					GameObject gameObject = Object.Instantiate(_scrollItemPrefab);
					ParameterScrollItem component = gameObject.GetComponent<ParameterScrollItem>();
					if (component != null)
					{
						bool eIAKNKDEEKA = false;
						int OEMALIFPGPO = 0;
						_previewParameters.FinalAttributes.Get(item.get_Name(), ref OEMALIFPGPO);
						component.gameObject.name = string.Format("ParameterScrollItem({0})", item.get_Name());
						component.Init(item.get_Name(), item.IconName, OEMALIFPGPO, OEMALIFPGPO, eIAKNKDEEKA);
						_scrollItems.Add(component);
					}
				}
			}
		}

		public void UpdateParameters(ItemInfo PJDAGCBPLJE)
		{
			ApplyItemParameters(PJDAGCBPLJE, 0f);
		}

		public void UpdateParametersWithDuration(ItemInfo PJDAGCBPLJE)
		{
			ApplyItemParameters(PJDAGCBPLJE, 2f);
		}

		protected void ApplyItemParameters(ItemInfo PJDAGCBPLJE, float _Duration)
		{
			ModelParameters kIKOGDEPGHB = ListSF.GetRoster().get_Parameters();
			kIKOGDEPGHB.CopyEquippedItemsTo(_equippedItems);
			_previewParameters.SetEquippedItemsFrom(_equippedItems);
			_previewParameters.SetItemByType(PJDAGCBPLJE.Type, PJDAGCBPLJE);
			kIKOGDEPGHB.CalculateAttributes();
			_previewParameters.CalculateAttributes();
			foreach (ParameterScrollItem item in _scrollItems)
			{
				string attributeName = item.get_AttributeName();
				int OEMALIFPGPO = 0;
				kIKOGDEPGHB.FinalAttributes.Get(attributeName, ref OEMALIFPGPO);
				int OEMALIFPGPO2 = 0;
				_previewParameters.FinalAttributes.Get(attributeName, ref OEMALIFPGPO2);
				item.SetValue(OEMALIFPGPO, OEMALIFPGPO2, _Duration);
			}
		}
	}
}
