using System;
using UnityEngine;

public static class GUIHelper
{
	private static GUIStyle centerAlignedLabel;

	private static GUIStyle rightAlignedLabel;

	public static Rect ClientArea;

	private static void Setup()
	{
		if (centerAlignedLabel == null)
		{
			centerAlignedLabel = new GUIStyle(GUI.skin.label);
			centerAlignedLabel.alignment = TextAnchor.MiddleCenter;
			rightAlignedLabel = new GUIStyle(GUI.skin.label);
			rightAlignedLabel.alignment = TextAnchor.MiddleRight;
		}
	}

	public static void DrawArea(Rect area, bool showTitle, Action drawContent)
	{
		Setup();
		GUI.Box(area, string.Empty);
		GUILayout.BeginArea(area);
		if (showTitle)
		{
			DrawCenteredText(SampleSelector.SelectedSample.GetDisplayName());
			GUILayout.Space(5f);
		}
		if (drawContent != null)
		{
			drawContent();
		}
		GUILayout.EndArea();
	}

	public static void DrawCenteredText(string text)
	{
		Setup();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(text, centerAlignedLabel);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	public static void DrawRow(string label, string value)
	{
		Setup();
		GUILayout.BeginHorizontal();
		GUILayout.Label(label);
		GUILayout.FlexibleSpace();
		GUILayout.Label(value, rightAlignedLabel);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
}
