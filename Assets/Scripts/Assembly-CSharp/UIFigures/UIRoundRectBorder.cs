using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFigures
{
	public class UIRoundRectBorder : UIFigure
	{
		[SerializeField]
		protected float _Width = 10f;

		[SerializeField]
		protected float _ChangeAllRadius = 10f;

		private float _PreviousChangeAllRadius = 10f;

		[SerializeField]
		protected float _RadiusUpLeft = 10f;

		[SerializeField]
		protected float _RadiusUpRight = 10f;

		[SerializeField]
		protected float _RadiusBottomRight = 10f;

		[SerializeField]
		protected float _RadiusBottomLeft = 10f;

		[Range(1f, 50f)]
		[SerializeField]
		protected int _Sectors = 10;

		protected List<UIVertex> _Vertexes = new List<UIVertex>();

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			base.OnPopulateMesh(vertexHelper);
			Vector2 center = (_LowerLeft + _UpperRight) * 0.5f;
			Vector2 halfSize = new Vector2(base.rectTransform.rect.width, base.rectTransform.rect.height) * 0.5f;
			Draw(vertexHelper, center, halfSize);
		}

		public void Draw(VertexHelper vertexHelper, Vector2 center, Vector2 halfSize)
		{
			vertexHelper.Clear();
			_Vertexes.Clear();
			AddArc(new Vector2(halfSize.x - _RadiusUpRight, halfSize.y - _RadiusUpRight), _RadiusUpRight, 0f);
			AddArc(new Vector2(0f - halfSize.x + _RadiusUpLeft, halfSize.y - _RadiusUpLeft), _RadiusUpLeft, (float)Math.PI / 2f);
			AddArc(new Vector2(0f - halfSize.x + _RadiusBottomLeft, 0f - halfSize.y + _RadiusBottomLeft), _RadiusBottomLeft, (float)Math.PI);
			AddArc(new Vector2(halfSize.x - _RadiusBottomRight, 0f - halfSize.y + _RadiusBottomRight), _RadiusBottomRight, 4.712389f);
			AddVertex(new Vector2(halfSize.x, halfSize.y - _RadiusUpRight));
			AddVertex(new Vector2(halfSize.x - _Width, halfSize.y - _RadiusUpRight));
			vertexHelper.AddUIVertexStream(_Vertexes, FigureTopology.CreateStripIndices((_Sectors + 1) * 8));
		}

		protected void AddArc(Vector3 center, float radius, float startAngle, bool addInnerVertex = true)
		{
			float num = (float)Math.PI / 2f / (float)_Sectors;
			for (int i = 0; i < _Sectors + 1; i++)
			{
				float angle = startAngle + (float)i * num;
				AddSegment(center, radius, angle, addInnerVertex);
			}
		}

		protected void AddSegment(Vector2 center, float radius, float angle, bool addInnerVertex = true)
		{
			float num = Mathf.Cos(angle);
			float num2 = Mathf.Sin(angle);
			float x = num * radius + center.x;
			float y = num2 * radius + center.y;
			AddVertex(new Vector2(x, y));
			if (addInnerVertex)
			{
				x = num * (radius - _Width) + center.x;
				y = num2 * (radius - _Width) + center.y;
				AddVertex(new Vector2(x, y));
			}
		}

		protected void AddVertex(Vector3 GIAEPIIIMDH)
		{
			UIVertex simpleVert = UIVertex.simpleVert;
			simpleVert.position = GIAEPIIIMDH;
			simpleVert.uv0 = new Vector2(0.5f + GIAEPIIIMDH.x / base.rectTransform.rect.width, 0.5f + GIAEPIIIMDH.y / base.rectTransform.rect.height);
			simpleVert.color = color;
			_Vertexes.Add(simpleVert);
		}

		protected void Update()
		{
			if (Math.Abs(_PreviousChangeAllRadius - _ChangeAllRadius) > 0.01f)
			{
				_PreviousChangeAllRadius = _ChangeAllRadius;
				_RadiusBottomLeft = _ChangeAllRadius;
				_RadiusBottomRight = _ChangeAllRadius;
				_RadiusUpLeft = _ChangeAllRadius;
				_RadiusUpRight = _ChangeAllRadius;
			}
		}
	}
}
