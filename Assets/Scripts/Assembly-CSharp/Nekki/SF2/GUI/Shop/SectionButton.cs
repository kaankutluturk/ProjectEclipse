using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Shop
{
	public class SectionButton : SFButton
	{
		[SerializeField]
		private ResolutionImage _newItemsCircle;

		[SerializeField]
		private ResolutionImage _newItemsEllipse;

		[SerializeField]
		private LabelAlias _newItemsLabel;

		private bool _spritesResolved;

		private int _newItemsCount;

		public int NewItemsAmount
		{
			get
			{
				return get_NewItemsCount();
			}
			set
			{
				set_NewItemsCount(value);
			}
		}

		public int get_NewItemsCount()
		{
			return _newItemsCount;
		}

		public void set_NewItemsCount(int value)
		{
			_newItemsCount = value;
			UpdateNewItemsIndicator();
		}

		private void UpdateNewItemsIndicator()
		{
			if (_newItemsCircle == null || _newItemsEllipse == null || _newItemsLabel == null)
			{
				GameLog.Error("SectionButton.UpdateNewItemsIndicator some field is null");
				return;
			}
			if (1 > _newItemsCount)
			{
				_newItemsCircle.gameObject.SetActive(false);
				_newItemsEllipse.gameObject.SetActive(false);
				_newItemsLabel.gameObject.SetActive(false);
			}
			else if (10 > _newItemsCount)
			{
				_newItemsCircle.gameObject.SetActive(true);
				_newItemsEllipse.gameObject.SetActive(false);
				_newItemsLabel.gameObject.SetActive(true);
			}
			else
			{
				_newItemsCircle.gameObject.SetActive(false);
				_newItemsEllipse.gameObject.SetActive(true);
				_newItemsLabel.gameObject.SetActive(true);
			}
			_newItemsLabel.set_text(_newItemsCount.ToString());
		}

		private void ResolveStateSprites()
		{
			SpriteState spriteState = base.spriteState;
			Sprite sprite = GetResolutionSprite(spriteState.highlightedSprite);
			if (sprite != null)
			{
				spriteState.highlightedSprite = sprite;
			}
			sprite = GetResolutionSprite(spriteState.disabledSprite);
			if (sprite != null)
			{
				spriteState.disabledSprite = sprite;
			}
			sprite = GetResolutionSprite(spriteState.pressedSprite);
			if (sprite != null)
			{
				spriteState.pressedSprite = sprite;
			}
			_spritesResolved = true;
			base.spriteState = spriteState;
		}

		private Sprite GetResolutionSprite(Sprite GBIOHMNNEJI)
		{
			if (GBIOHMNNEJI != null)
			{
				Sprite sprite = ResolutionImage.GetSprite(string.Empty, GBIOHMNNEJI.name);
				if (sprite != null && GBIOHMNNEJI != sprite)
				{
					return sprite;
				}
			}
			return null;
		}

		protected override void DoStateTransition(SelectionState state, bool PJHFBFHIGNN)
		{
			if (!_spritesResolved)
			{
				ResolveStateSprites();
			}
			base.DoStateTransition(state, PJHFBFHIGNN);
		}
	}
}
