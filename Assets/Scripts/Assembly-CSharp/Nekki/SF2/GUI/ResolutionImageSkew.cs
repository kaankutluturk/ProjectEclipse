using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI
{
	public class ResolutionImageSkew : ResolutionImage
	{
		[SerializeField]
		protected float _SkewAngle;

		protected List<UIVertex> _Vertexes = new List<UIVertex>();

		public float SkewAngleDegrees
		{
			get
			{
				return get_SkewAngle();
			}
			set
			{
				set_SkewAngle(value);
			}
		}

		public float get_SkewAngle()
		{
			return _SkewAngle;
		}

		public void set_SkewAngle(float value)
		{
			_SkewAngle = value;
			SetVerticesDirty();
		}

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			base.OnPopulateMesh(vertexHelper);
			Vector2 size = new Vector2(base.rectTransform.rect.width, base.rectTransform.rect.height);
			Draw(vertexHelper, size);
		}

		protected void Draw(VertexHelper vertexHelper, Vector2 size)
		{
			float f = (float)Math.PI * _SkewAngle / 180f;
			float num = size.y * Mathf.Tan(f);
			List<UIVertex> list = new List<UIVertex>();
			vertexHelper.GetUIVertexStream(list);
			if (list.Count >= 6)
			{
				UIVertex value = list[0];
				value.position.x -= num;
				list[0] = value;
				value = list[4];
				value.position.x -= num;
				list[4] = value;
				value = list[5];
				value.position.x -= num;
				list[5] = value;
			}
			vertexHelper.Clear();
			vertexHelper.AddUIVertexTriangleStream(list);
		}
	}
}
