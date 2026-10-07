using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFigures
{
	public class UILine : UIFigure
	{
		[SerializeField]
		private bool _SingleTexturCord;

		[SerializeField]
		private List<Vector2> _Points;

		[SerializeField]
		private float _Width = 10f;

		public List<Vector2> PointList
		{
			get
			{
				return get_Points();
			}
		}

		public float LineWidth
		{
			get
			{
				return get_Width();
			}
			set
			{
				set_Width(value);
			}
		}

		public List<Vector2> get_Points()
		{
			return _Points;
		}

		public float get_Width()
		{
			return _Width;
		}

		public void set_Width(float value)
		{
			_Width = value;
		}

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			base.OnPopulateMesh(vertexHelper);
			DrawFunctions.DrawLine(vertexHelper, _Points, _Width, color, _SingleTexturCord);
		}
	}
}
