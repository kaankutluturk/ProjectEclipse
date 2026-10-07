using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI
{
	public class TextGradient : BaseMeshEffect
	{
		[SerializeField]
		private Color32 topColor = Color.white;

		[SerializeField]
		private Color32 bottomColor = Color.black;

		public override void ModifyMesh(VertexHelper vertexHelper)
		{
			if (!IsActive())
			{
				return;
			}
			List<UIVertex> list = new List<UIVertex>();
			vertexHelper.GetUIVertexStream(list);
			float num = list[0].position.y;
			float num2 = list[0].position.y;
			for (int i = 1; i < vertexHelper.currentVertCount; i++)
			{
				float y = list[i].position.y;
				if (y > num2)
				{
					num2 = y;
				}
				else if (y < num)
				{
					num = y;
				}
			}
			float num3 = num2 - num;
			UIVertex vertex = default(UIVertex);
			for (int j = 0; j < vertexHelper.currentVertCount; j++)
			{
				vertexHelper.PopulateUIVertex(ref vertex, j);
				vertex.color = Color32.Lerp(bottomColor, topColor, (vertex.position.y - num) / num3);
				vertexHelper.SetUIVertex(vertex, j);
			}
		}
	}
}
