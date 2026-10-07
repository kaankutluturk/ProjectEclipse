using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFigures
{
	public class UICircleBorder : UIFigure
	{
		[Range(3f, 360f)]
		[SerializeField]
		private int _Segments = 10;

		[SerializeField]
		private float _Width = 10f;

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			base.OnPopulateMesh(vertexHelper);
			Vector2 center = (_LowerLeft + _UpperRight) * 0.5f;
			Vector2 halfSize = new Vector2(base.rectTransform.rect.width, base.rectTransform.rect.height) * 0.5f;
			DrawFunctions.DrawArcBorder(vertexHelper, center, halfSize, _Width, 0f, (float)Math.PI * 2f, _Segments, color, color);
		}
	}
}
