using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class DrawFunctions
{
	public static void DrawArc(VertexHelper vertexHelper, Vector2 center, Vector2 radius, float startAngle, float endAngle, int segments, Color color)
	{
		vertexHelper.Clear();
		List<UIVertex> list = new List<UIVertex>();
		float num = (endAngle - startAngle) / (float)segments;
		list.Add(CreateVertex(center, new Vector2(0.5f, 0.5f), color));
		for (int i = 0; i <= segments; i++)
		{
			float f = startAngle + (float)i * num;
			float num2 = Mathf.Cos(f);
			float num3 = Mathf.Sin(f);
			float x = num2 * radius.x + center.x;
			float y = num3 * radius.y + center.y;
			list.Add(CreateVertex(uv: new Vector2(num2 * 0.5f + 0.5f, num3 * 0.5f + 0.5f), GIAEPIIIMDH: new Vector2(x, y), color: color));
		}
		vertexHelper.AddUIVertexStream(list, FigureTopology.CreateFanIndices(segments));
	}

	public static void DrawArcBorder(VertexHelper vertexHelper, Vector2 center, Vector2 radius, float thickness, float startAngle, float endAngle, int segments, Color startColor, Color endColor)
	{
		vertexHelper.Clear();
		List<UIVertex> list = new List<UIVertex>();
		float num = (endAngle - startAngle) / (float)segments;
		Vector2 vector = new Vector2(radius.x - thickness, radius.y - thickness);
		for (int i = 0; i <= segments; i++)
		{
			float f = startAngle + (float)i * num;
			float num2 = Mathf.Cos(f);
			float num3 = Mathf.Sin(f);
			Color vertexColor = Color.Lerp(startColor, endColor, (float)i / (float)segments);
			Vector2 gIAEPIIIMDH = new Vector2(num2 * radius.x + center.x, num3 * radius.y + center.y);
			Vector2 uv = new Vector2(num2 * 0.5f + 0.5f, num3 * 0.5f + 0.5f);
			list.Add(CreateVertex(gIAEPIIIMDH, uv, vertexColor));
			Vector2 gIAEPIIIMDH2 = new Vector2(num2 * vector.x + center.x, num3 * vector.y + center.y);
			Vector2 vector2 = new Vector2(1f - thickness / radius.x, 1f - thickness / radius.y);
			Vector2 fGFOGDLPAIC2 = new Vector2(num2 * vector2.x * 0.5f + 0.5f, num3 * vector2.y * 0.5f + 0.5f);
			list.Add(CreateVertex(gIAEPIIIMDH2, fGFOGDLPAIC2, vertexColor));
		}
		vertexHelper.AddUIVertexStream(list, FigureTopology.CreateStripIndices(segments * 2));
	}

	public static void DrawLine(VertexHelper vertexHelper, List<Vector2> points, float thickness, Color color, bool useDistanceUv = false)
	{
		vertexHelper.Clear();
		List<UIVertex> list = new List<UIVertex>();
		List<Vector2> list2 = new List<Vector2>();
		if (points.Count < 2)
		{
			return;
		}
		float num = points[points.Count - 1].x - points[0].x;
		for (int i = 1; i < points.Count; i++)
		{
			list2.Add(points[i] - points[i - 1]);
			int index = i - 1;
			Vector2 vector = new Vector2(0f - list2[i - 1].y, list2[i - 1].x);
			list2[index] = vector.normalized;
			if (list2[i - 1].magnitude == 0f && i > 1)
			{
				list2[i - 1] = list2[i - 2];
			}
		}
		for (int num2 = points.Count - 2; num2 > 0; num2--)
		{
			if (list2[num2 - 1].magnitude == 0f && num2 > 0)
			{
				list2[num2 - 1] = list2[num2];
			}
		}
		int num3 = 1;
		for (int j = 0; j < points.Count; j++)
		{
			Vector2 vector2 = list2[(j <= 0) ? j : (j - 1)];
			Vector2 vector3 = list2[(j >= points.Count - 1) ? (j - 1) : j];
			Vector2 vector4 = points[j];
			float x = ((!useDistanceUv) ? ((float)((j % 2 == 0) ? 1 : 0)) : ((vector4.x - points[0].x) / num));
			Vector2 uv = new Vector2(x, 0f);
			float num4 = Mathf.Abs(Mathf.Cos((float)Math.PI / 180f * Vector2.Angle(vector2, vector3) / 2f));
			Vector2 vector5 = num3 * (vector2 + vector3).normalized / num4 * thickness;
			if (vector5.magnitude == 0f)
			{
				vector5 = num3 * vector2 * thickness;
				num3 *= -1;
			}
			Vector2 fGFOGDLPAIC2 = new Vector2(x, 1f);
			list.Add(CreateVertex(vector4 - vector5 / 2f, uv, color));
			list.Add(CreateVertex(vector4 + vector5 / 2f, fGFOGDLPAIC2, color));
		}
		vertexHelper.AddUIVertexStream(list, FigureTopology.CreateStripIndices((points.Count - 1) * 2));
	}

	private static UIVertex CreateVertex(Vector2 GIAEPIIIMDH, Vector2 uv, Color color)
	{
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.position = GIAEPIIIMDH;
		simpleVert.uv0 = uv;
		simpleVert.color = color;
		return simpleVert;
	}
}
