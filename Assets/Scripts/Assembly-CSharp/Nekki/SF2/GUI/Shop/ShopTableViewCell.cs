using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Shop
{
	public class ShopTableViewCell : TableViewCell, IComparable<ShopTableViewCell>
	{
		private string _texturePath = SF2Paths.GetItemsUiPath();

		[SerializeField]
		private ResolutionImage _lockIcon;

		[SerializeField]
		private ResolutionImage _jackdawIcon;

		[SerializeField]
		private ResolutionImage _equppiedIcon;

		[SerializeField]
		private ResolutionImage _levelIcon;

		[SerializeField]
		private Text _levelLabel;

		[SerializeField]
		private ResolutionImage _image;

		[SerializeField]
		private PerksPanel _perksPanel;

		[SerializeField]
		private LayoutElement _iconPanel;

		[SerializeField]
		private LayoutElement _layoutElement;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Vector2 _baseSize;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int _index;

		private ItemInfo _itemInfo;

		private float _incraseHeight;

		public PerksPanel PerksPanelRef
		{
			get
			{
				return get_PerksPanel();
			}
		}

		public LayoutElement LayoutElementRef
		{
			get
			{
				return get_LayoutElement();
			}
		}

		public Vector2 BaseSizeValue
		{
			get
			{
				return get_BaseSize();
			}
			set
			{
				set_BaseSize(value);
			}
		}

		public bool IconPanelVisible
		{
			get
			{
				return get_IconPanelActive();
			}
			set
			{
				set_IconPanelActive(value);
			}
		}

		public Vector3 CenterPoint
		{
			get
			{
				return get_CenterPosition();
			}
		}

		public ItemInfo ItemInfoRef
		{
			get
			{
				return get_ItemInfo();
			}
		}

		public float HeightIncrease
		{
			get
			{
				return get_IncraseHeight();
			}
			set
			{
				set_IncraseHeight(value);
			}
		}

		public PerksPanel get_PerksPanel()
		{
			return _perksPanel;
		}

		public LayoutElement get_LayoutElement()
		{
			return _layoutElement;
		}

		public Vector2 get_BaseSize()
		{
			return _baseSize;
		}

		public void set_BaseSize(Vector2 value)
		{
			_baseSize = value;
		}

		public bool get_IconPanelActive()
		{
			return _iconPanel != null && _iconPanel.gameObject.activeSelf;
		}

		public void set_IconPanelActive(bool value)
		{
			if (_iconPanel != null)
			{
				_iconPanel.gameObject.SetActive(value);
				UpdateLayoutSize();
			}
		}

		public int get_Index()
		{
			return _index;
		}

		public void set_Index(int value)
		{
			_index = value;
		}

		public Vector3 get_CenterPosition()
		{
			Vector3 position = base.transform.position;
			if (_image != null)
			{
				position.y = _image.transform.position.y;
			}
			return position;
		}

		public ItemInfo get_ItemInfo()
		{
			return _itemInfo;
		}

		public float get_IncraseHeight()
		{
			return _incraseHeight;
		}

		public void set_IncraseHeight(float value)
		{
			_incraseHeight = value;
			UpdateLayoutSize();
		}

		public void SetItemInfo(ItemInfo item)
		{
			if (item == null)
			{
				GameLog.Error("ShopTableViewCell.SetItemInfo item is null");
				return;
			}
			base.gameObject.name = string.Format("ShopTableViewCell({0})", item.Name);
			UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
			_itemInfo = ((userItem == null) ? item : userItem.GetCurrentUpgradeItem());
			_lockIcon.gameObject.SetActive(item.ItemLevel > ListSF.GetRoster().GetLevel());
			_jackdawIcon.gameObject.SetActive(userItem != null && item.Type != "Seal");
			_equppiedIcon.gameObject.SetActive(userItem != null && userItem.GetIsEquipped());
			bool active = _itemInfo.ItemLevel > 0;
			Font font = LocalizationManager.GetContentFont();
			if (font != null)
			{
				_levelLabel.font = font;
			}
			_levelLabel.text = _itemInfo.ItemLevel.ToString();
			_levelLabel.gameObject.SetActive(active);
			_levelIcon.gameObject.SetActive(active);
			_image.set_TexturePath((!(item.Type == "Seal")) ? _texturePath : SF2Paths.GetUsersUiPath());
			_image.set_SpriteName(item.FileName);
			if (item.Type == "Seal")
			{
				_image.rectTransform.sizeDelta = Constants.SEAL_SIZE;
			}
			else
			{
				_image.SetNativeSize();
			}
			_perksPanel.SetPerks(ListSF.GetItemEnchantments(_itemInfo));
			UpdateLayoutSize();
			ApplyOpacity();
		}

		private void ApplyOpacity()
		{
		}

		private void UpdateLayoutSize()
		{
			RectTransform rectTransform = (RectTransform)base.transform;
			Vector2 vector = get_BaseSize();
			if (vector == new Vector2(0f, 0f) && _image != null)
			{
				vector = ((RectTransform)_image.transform).sizeDelta;
			}
			vector.y += _incraseHeight;
			if (_iconPanel.gameObject.activeSelf)
			{
				vector.y += _iconPanel.minHeight;
			}
			rectTransform.sizeDelta = vector;
			_layoutElement.minWidth = vector.x;
			_layoutElement.minHeight = vector.y;
		}

		public void UpdateItem()
		{
			SetItemInfo(_itemInfo);
		}

		public int CompareTo(ShopTableViewCell other)
		{
			int num = ((_itemInfo != null) ? _itemInfo.ItemLevel : 0);
			int value = ((other._itemInfo != null) ? other._itemInfo.ItemLevel : 0);
			int num2 = ((_itemInfo != null) ? _itemInfo.Index : 0);
			int value2 = ((other._itemInfo == null) ? 1 : other._itemInfo.Index);
			int num3 = num.CompareTo(value);
			if (num3 != 0)
			{
				return num3;
			}
			return num2.CompareTo(value2);
		}

		public override void SetHighlighted()
		{
		}

		public override void SetSelected()
		{
		}

		public override void Display()
		{
		}
	}
}
