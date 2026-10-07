using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI
{
	[AddComponentMenu("UI_Nekki/ResolutionButton")]
	public class ResolutionButton : SFButton
	{
		protected override void Awake()
		{
			if (base.targetGraphic == null)
			{
				base.targetGraphic = base.gameObject.GetComponent<ResolutionImage>();
			}
			RemapStateSprites();
		}

		private void RemapStateSprites()
		{
			SpriteState spriteState = base.spriteState;
			RemapSprite(spriteState.disabledSprite);
			RemapSprite(spriteState.highlightedSprite);
			RemapSprite(spriteState.pressedSprite);
			base.spriteState = spriteState;
		}

		private void RemapSprite(Sprite sourceSprite)
		{
			if (!(sourceSprite == null))
			{
				string texturePath = ResolutionImage.GetTexturePath(sourceSprite);
				string spriteName = sourceSprite.name;
				sourceSprite = ResolutionImage.GetSprite(texturePath, spriteName);
			}
		}

		public void SetDisabledSprite(Sprite sourceSprite)
		{
			SpriteState spriteState = base.spriteState;
			spriteState.disabledSprite = sourceSprite;
			RemapSprite(spriteState.disabledSprite);
			base.spriteState = spriteState;
		}

		public void SetDisabledSprite(string texturePath, string spriteName)
		{
			SpriteState spriteState = base.spriteState;
			spriteState.disabledSprite = ResolutionImage.GetSprite(texturePath, spriteName);
			base.spriteState = spriteState;
		}

		public void SetHighlightedSprite(Sprite sourceSprite)
		{
			SpriteState spriteState = base.spriteState;
			spriteState.highlightedSprite = sourceSprite;
			RemapSprite(spriteState.highlightedSprite);
			base.spriteState = spriteState;
		}

		public void SetHighlightedSprite(string texturePath, string spriteName)
		{
			SpriteState spriteState = base.spriteState;
			spriteState.highlightedSprite = ResolutionImage.GetSprite(texturePath, spriteName);
			base.spriteState = spriteState;
		}

		public void SetPressedSprite(Sprite sourceSprite)
		{
			SpriteState spriteState = base.spriteState;
			spriteState.pressedSprite = sourceSprite;
			RemapSprite(spriteState.pressedSprite);
			base.spriteState = spriteState;
		}

		public void SetPressedSprite(string texturePath, string spriteName)
		{
			SpriteState spriteState = base.spriteState;
			spriteState.pressedSprite = ResolutionImage.GetSprite(texturePath, spriteName);
			base.spriteState = spriteState;
		}

		public void SetNormalSprite(string texturePath, string spriteName)
		{
			ResolutionImage resolutionImage = base.targetGraphic as ResolutionImage;
			resolutionImage.set_TexturePath(texturePath);
			resolutionImage.set_SpriteName(spriteName);
		}
	}
}
