using System;
using UnityEngine;

public sealed class CacheMaintenanceSample : MonoBehaviour
{
	private enum TimeSpans
	{
		Days = 0,
		Hours = 1,
		Mins = 2,
		Secs = 3
	}

	private TimeSpans timespan = TimeSpans.Secs;

	private int value = 10;

	private int maxCacheSize = 5242880;

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Delete cached entities older then");
			GUILayout.Label(value.ToString(), GUILayout.MinWidth(50f));
			value = (int)GUILayout.HorizontalSlider(value, 1f, 60f, GUILayout.MinWidth(100f));
			GUILayout.Space(10f);
			timespan = (TimeSpans)GUILayout.SelectionGrid((int)timespan, new string[4] { "Days", "Hours", "Mins", "Secs" }, 4);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Max Cache Size (bytes): ", GUILayout.Width(150f));
			GUILayout.Label(maxCacheSize.ToString("N0"), GUILayout.Width(70f));
			maxCacheSize = (int)GUILayout.HorizontalSlider(maxCacheSize, 1024f, 10485760f);
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
			if (GUILayout.Button("Maintenance"))
			{
				TimeSpan maintenanceAge = TimeSpan.FromDays(14.0);
				switch (timespan)
				{
				case TimeSpans.Days:
					maintenanceAge = TimeSpan.FromDays(value);
					break;
				case TimeSpans.Hours:
					maintenanceAge = TimeSpan.FromHours(value);
					break;
				case TimeSpans.Mins:
					maintenanceAge = TimeSpan.FromMinutes(value);
					break;
				case TimeSpans.Secs:
					maintenanceAge = TimeSpan.FromSeconds(value);
					break;
				}
				HTTPCacheService.BeginMaintainence(new HTTPCacheMaintananceParams(maintenanceAge, (ulong)maxCacheSize));
			}
		});
	}
}
