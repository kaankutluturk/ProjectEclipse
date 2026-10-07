using UnityEngine;
using UnityEngine.UI;

namespace UIFigures
{
	[ExecuteInEditMode]
	public class UIFigure : MaskableGraphic
	{
		public Sprite _Sprite;

		protected Vector2 _LowerLeft;

		protected Vector2 _UpperRight;

		public override Texture mainTexture
		{
			get
			{
				return base.mainTexture;
			}
		}

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			_LowerLeft = new Vector2(0f - base.rectTransform.pivot.x, 0f - base.rectTransform.pivot.y);
			_UpperRight = new Vector2(1f - base.rectTransform.pivot.x, 1f - base.rectTransform.pivot.y);
			_LowerLeft.x *= base.rectTransform.rect.width;
			_LowerLeft.y *= base.rectTransform.rect.height;
			_UpperRight.x *= base.rectTransform.rect.width;
			_UpperRight.y *= base.rectTransform.rect.height;
		}

		public void Refresh()
		{
			SetVerticesDirty();
		}
	}
}
