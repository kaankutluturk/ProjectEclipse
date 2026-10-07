using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFigures
{
	public class UIRoundRect : UIRoundRectBorder
	{
		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			base.OnPopulateMesh(vertexHelper);
			Vector2 center = (_LowerLeft + _UpperRight) * 0.5f;
			Vector2 halfSize = new Vector2(base.rectTransform.rect.width, base.rectTransform.rect.height) * 0.5f;
			Draw(vertexHelper, center, halfSize);
		}

		public new void Draw(VertexHelper vertexHelper, Vector2 center, Vector2 halfSize)
		{
			vertexHelper.Clear();
			_Vertexes.Clear();
			AddVertex(center);
			AddArc(new Vector2(halfSize.x - _RadiusUpRight, halfSize.y - _RadiusUpRight), _RadiusUpRight, 0f, false);
			AddArc(new Vector2(0f - halfSize.x + _RadiusUpLeft, halfSize.y - _RadiusUpLeft), _RadiusUpLeft, (float)Math.PI / 2f, false);
			AddArc(new Vector2(0f - halfSize.x + _RadiusBottomLeft, 0f - halfSize.y + _RadiusBottomLeft), _RadiusBottomLeft, (float)Math.PI, false);
			AddArc(new Vector2(halfSize.x - _RadiusBottomRight, 0f - halfSize.y + _RadiusBottomRight), _RadiusBottomRight, 4.712389f, false);
			AddVertex(new Vector2(halfSize.x, halfSize.y - _RadiusUpRight));
			vertexHelper.AddUIVertexStream(_Vertexes, FigureTopology.CreateFanIndices((_Sectors + 1) * 4));
		}
	}
}
