using System.Collections.Generic;
using UnityEngine;

public class GUIMessageList
{
	private List<string> messages = new List<string>();

	private Vector2 scrollPos;

	public void Draw()
	{
		Draw(Screen.width, 0f);
	}

	public void Draw(float width, float minHeight)
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUILayout.MinHeight(minHeight));
		for (int i = 0; i < messages.Count; i++)
		{
			GUILayout.Label(messages[i], GUILayout.MinWidth(width));
		}
		GUILayout.EndScrollView();
	}

	public void Add(string message)
	{
		messages.Add(message);
		scrollPos = new Vector2(scrollPos.x, float.MaxValue);
	}

	public void Clear()
	{
		messages.Clear();
	}
}
