using System.Collections.Generic;
using Eclipse.Rendering.Interpolation;
using UnityEngine;

public class MeshNode
{
	public int[] Triangles;
    public string[] FigureNames;
    private List<string> _FigureNames = new List<string>();

	public Vector3[] Vertices;

	private List<ModelNode> nodes = new List<ModelNode>();

	private List<int> _TrianglesList = new List<int>();

	// best guess for name
	public void AddTriangle(ModelNode firstNode, ModelNode secondNode, ModelNode thirdNode, string figureName = "")
	{
		int item = GetOrAddNodeIndex(firstNode);
		int item2 = GetOrAddNodeIndex(secondNode);
		int item3 = GetOrAddNodeIndex(thirdNode);
		_TrianglesList.Add(item);
		_TrianglesList.Add(item2);
		_TrianglesList.Add(item3);
        _FigureNames.Add(figureName);
	}

	private int GetOrAddNodeIndex(ModelNode node)
	{
		if (nodes.Contains(node))
		{
			return nodes.IndexOf(node);
		}
		nodes.Add(node);
		return nodes.Count - 1;
	}

	public void Init()
	{
		Triangles = _TrianglesList.ToArray();
        FigureNames = _FigureNames.ToArray();
        _FigureNames = null;
		Vertices = new Vector3[nodes.Count];
		_TrianglesList = null;
	}

	public void Render(float alpha, bool preserveDepth = false)
	{
		for (int i = 0; i < Vertices.Length; i++)
		{
			float x;
			float y;
			float z;
			FightInterpolation.SamplePosition(nodes[i], alpha, out x, out y, out z);
			Vertices[i].Set(x, y, preserveDepth ? z : 0f);
		}
	}
}
