using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Shop
{
	public class ShopScrollItem : BaseScrollItem, IComparable<ShopScrollItem>
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

		public override Vector3 CenterWorldPosition
		{
			get
			{
				return get_CenterPosition();
			}
		}

		public override float CurrentOpacity
		{
			set
			{
				set_Opacity(value);
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

		public override Vector3 get_CenterPosition()
		{
			Vector3 position = base.transform.position;
			if (_image != null)
			{
				position.y = _image.transform.position.y;
			}
			return position;
		}

		public override void set_Opacity(float value)
		{
			currentOpacity = value;
			if (currentOpacity < minOpacity)
			{
				currentOpacity = minOpacity;
			}
			if (currentOpacity > maxOpacity)
			{
				currentOpacity = maxOpacity;
			}
			ApplyOpacity();
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
				GameLog.Error("ShopScrollItem.SetItemInfo item is null");
				return;
			}
			set_Name(item.Name);
			base.gameObject.name = string.Format("ShopScrollItem({0})", get_Name());
			UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
			_itemInfo = ((userItem == null) ? item : userItem.GetCurrentUpgradeItem());
			set_MaxOpacity((item.ItemLevel <= ListSF.GetRoster().GetLevel()) ? 1f : 0.5f);
			if (_lockIcon != null)
			{
				if (item.ItemLevel > ListSF.GetRoster().GetLevel())
				{
					_lockIcon.gameObject.SetActive(true);
				}
				else
				{
					_lockIcon.gameObject.SetActive(false);
				}
			}
			if (_jackdawIcon != null)
			{
				bool flag = userItem != null;
				_jackdawIcon.gameObject.SetActive(flag && item.Type != "Seal");
			}
			if (_equppiedIcon != null)
			{
				bool active = userItem != null && userItem.GetIsEquipped();
				_equppiedIcon.gameObject.SetActive(active);
			}
			bool active2 = _itemInfo.ItemLevel > 0;
			if (_levelLabel != null)
			{
				Font font = LocalizationManager.GetContentFont();
				if (font != null)
				{
					_levelLabel.font = font;
				}
				_levelLabel.text = _itemInfo.ItemLevel.ToString();
				_levelLabel.gameObject.SetActive(active2);
			}
			if (_levelIcon != null)
			{
				_levelIcon.gameObject.SetActive(active2);
			}
			if (_image != null)
			{
				if (item.Type == "Seal")
				{
					_image.set_TexturePath(SF2Paths.GetUsersUiPath());
				}
				else
				{
					_image.set_TexturePath(_texturePath);
				}
				_image.set_SpriteName(item.FileName);
				_image.SetNativeSize();
			}
			else
			{
				GameLog.Error("ShopScrollItem.SetItemInfo _image is null");
			}
			if (_perksPanel != null)
			{
				_perksPanel.SetPerks(ListSF.GetItemEnchantments(_itemInfo));
			}
			UpdateLayoutSize();
			ApplyOpacity();
		}

		private void ApplyOpacity()
		{
			if (_image != null)
			{
				UIExtensions.SetAlpha(_image, currentOpacity);
			}
			if (_jackdawIcon != null)
			{
				UIExtensions.SetAlpha(_jackdawIcon, currentOpacity);
			}
			if (_equppiedIcon != null)
			{
				UIExtensions.SetAlpha(_equppiedIcon, currentOpacity);
			}
			if (_levelIcon != null)
			{
				UIExtensions.SetAlpha(_levelIcon, currentOpacity);
			}
			if (_levelLabel != null)
			{
				_levelLabel.SetAlpha(currentOpacity);
			}
		}

		private void UpdateLayoutSize()
		{
			if (_layoutElement != null && _iconPanel != null)
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
			else
			{
				GameLog.Error("ShopScrollItem.SetItemInfo _layoutElement or rectTransform is null");
			}
		}

		public void UpdateItem()
		{
			SetItemInfo(_itemInfo);
		}

		public int CompareTo(ShopScrollItem other)
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
	}
}
