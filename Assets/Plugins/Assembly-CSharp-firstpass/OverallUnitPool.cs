using System.Collections.Generic;
using Nekki.Audio;
using UnityEngine;

internal class OverallUnitPool
{
	public static int MAX_CHANELS = 16;

	private static readonly List<AudioUnit> _sources = new List<AudioUnit>();

	internal static void Init(AudioManager audioManager)
	{
		GameObject gameObject = new GameObject(string.Format("_pool ({0} max)", MAX_CHANELS));
		gameObject.transform.parent = audioManager.transform;
		for (int i = 0; i < MAX_CHANELS; i++)
		{
			_sources.Add(gameObject.AddComponent<AudioUnit>());
		}
	}

	internal static AudioUnit GetFreeUnit()
	{
		for (int i = 0; i < _sources.Count; i++)
		{
			if (_sources[i].GetIsFree())
			{
				_sources[i].ReturnToChanel();
				return _sources[i];
			}
		}
		return null;
	}
}
