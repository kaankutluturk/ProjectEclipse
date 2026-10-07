using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFigures
{
	public class UISkewRect : UIFigure
	{
		[SerializeField]
		protected float _SkewAngle;

		protected List<UIVertex> _Vertexes = new List<UIVertex>();

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			base.OnPopulateMesh(vertexHelper);
			Vector2 halfSize = new Vector2(base.rectTransform.rect.width, base.rectTransform.rect.height);
			Draw(vertexHelper, halfSize, base.rectTransform.pivot);
		}

		protected void Draw(VertexHelper vertexHelper, Vector2 halfSize, Vector2 pivot)
		{
			vertexHelper.Clear();
			_Vertexes.Clear();
			float f = (float)Math.PI * _SkewAngle / 180f;
			float num = halfSize.y * Mathf.Tan(f);
			Vector2 vector = new Vector2((0f - halfSize.x) * pivot.x, (0f - halfSize.y) * pivot.y);
			Vector2 vector2 = new Vector2((0f - halfSize.x) * pivot.x + num, halfSize.y * (1f - pivot.y));
			Vector2 vector3 = new Vector2(halfSize.x * (1f - pivot.x), halfSize.y * (1f - pivot.y));
			Vector2 vector4 = new Vector2(halfSize.x * (1f - pivot.x) - num, (0f - halfSize.y) * pivot.y);
			AddVertex(vector);
			AddVertex(vector2);
			AddVertex(vector3);
			AddVertex(vector4);
			vertexHelper.AddUIVertexQuad(_Vertexes.ToArray());
		}

		protected void AddVertex(Vector3 GIAEPIIIMDH)
		{
			UIVertex simpleVert = UIVertex.simpleVert;
			simpleVert.position = GIAEPIIIMDH;
			simpleVert.uv0 = new Vector2(0.5f + GIAEPIIIMDH.x / base.rectTransform.rect.width, 0.5f + GIAEPIIIMDH.y / base.rectTransform.rect.height);
			simpleVert.color = color;
			_Vertexes.Add(simpleVert);
		}
	}
}
