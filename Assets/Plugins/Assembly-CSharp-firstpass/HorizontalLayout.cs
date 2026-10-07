using System;
using UnityEngine;

internal class HorizontalLayout : IDisposable
{
	public HorizontalLayout(params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal(options);
	}

	public void Dispose()
	{
		GUILayout.EndHorizontal();
	}
}
