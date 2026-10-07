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

	public static void DrawArea(Rect FKAFENMANAB, bool CDKGMDNIECB, Action IBODMPMJELJ)
	{
		Setup();
		GUI.Box(FKAFENMANAB, string.Empty);
		GUILayout.BeginArea(FKAFENMANAB);
		if (CDKGMDNIECB)
		{
			DrawCenteredText(SampleSelector.SelectedSample.GetDisplayName());
			GUILayout.Space(5f);
		}
		if (IBODMPMJELJ != null)
		{
			IBODMPMJELJ();
		}
		GUILayout.EndArea();
	}

	public static void DrawCenteredText(string CKEHOEGLMBM)
	{
		Setup();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(CKEHOEGLMBM, centerAlignedLabel);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	public static void DrawRow(string KGBGENDIMBC, string value)
	{
		Setup();
		GUILayout.BeginHorizontal();
		GUILayout.Label(KGBGENDIMBC);
		GUILayout.FlexibleSpace();
		GUILayout.Label(value, rightAlignedLabel);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
}
