using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Shop
{
	public class PropertiesPanelContent : SidePanelContent
	{
		[SerializeField]
		private LabelAlias _notExistText;

		[SerializeField]
		private PropertiesPerksPanel _propertiesPanel;

		public PropertiesPerksPanel PropertiesPanelComponent
		{
			get
			{
				return get_PropertiesPanel();
			}
		}

		public PropertiesPerksPanel get_PropertiesPanel()
		{
			return _propertiesPanel;
		}

		public override void Init()
		{
		}

		public void SetItemInfo(ItemInfo itemInfo)
		{
			if (_propertiesPanel != null && itemInfo != null)
			{
				List<PerkInfoItem> list = ListSF.GetItemEnchantments(itemInfo);
				if (list.Count > 0)
				{
					_propertiesPanel.gameObject.SetActive(true);
					_propertiesPanel.SetPerks(list);
					if (_notExistText != null)
					{
						_notExistText.gameObject.SetActive(false);
					}
					return;
				}
			}
			if (_propertiesPanel != null)
			{
				_propertiesPanel.gameObject.SetActive(false);
			}
			if (_notExistText != null)
			{
				_notExistText.gameObject.SetActive(true);
			}
		}
	}
}
