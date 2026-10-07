using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Shop
{
	public class ParametersPanel : MonoBehaviour
	{
		[SerializeField]
		private GameObject _itemPrefab;

		[SerializeField]
		private VerticalLayoutGroup _verticalLayoutGroup;

		[SerializeField]
		private LayoutElement _layoutElement;

		private List<ParameterScrollItem> _items = new List<ParameterScrollItem>();

		public void Clear()
		{
			foreach (ParameterScrollItem item in _items)
			{
				item.gameObject.SetActive(false);
				Object.Destroy(item.gameObject);
			}
			_items.Clear();
		}

		public void SetParameters(ItemInfo item, ItemInfo upgradeItem, bool showUpgrades)
		{
			Clear();
			List<WarriorAttribute> attributes = GameUtils.WarriorAttributeList.AttributeList;
			float num = 0f;
			foreach (WarriorAttribute warriorItem in attributes)
			{
				int baseValue = 0;
				if (item == null || !item.ItemAttributes.Get(warriorItem.get_Name(), ref baseValue) || warriorItem.IsHidden || warriorItem.IsShopHidden)
				{
					continue;
				}
				int OEMALIFPGPO2 = baseValue;
				if (upgradeItem != null && showUpgrades)
				{
					upgradeItem.ItemAttributes.Get(warriorItem.get_Name(), ref OEMALIFPGPO2);
				}
				if (_itemPrefab != null)
				{
					GameObject gameObject = Object.Instantiate(_itemPrefab);
					gameObject.transform.SetParent(base.gameObject.transform, false);
					ParameterScrollItem component = gameObject.GetComponent<ParameterScrollItem>();
					if (component != null)
					{
						bool isItemLimit = true;
						component.Init(warriorItem.get_Name(), warriorItem.IconName, baseValue, OEMALIFPGPO2, isItemLimit);
						num += component.get_MinHeight();
						_items.Add(component);
					}
				}
			}
			if (_verticalLayoutGroup != null && _items.Count > 0)
			{
				num += (float)(_items.Count - 1) * _verticalLayoutGroup.spacing;
			}
			if (_layoutElement != null)
			{
				_layoutElement.minHeight = num;
			}
		}

		public void SetParameters(ItemInfo item, bool showUpgrades)
		{
			SetParameters(item, null, showUpgrades);
		}
	}
}
