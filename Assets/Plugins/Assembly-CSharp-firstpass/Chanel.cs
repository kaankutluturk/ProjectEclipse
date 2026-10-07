using System.Collections.Generic;
using System.Diagnostics;
using Nekki.Audio;
using UnityEngine;

internal class Chanel
{
	private readonly Dictionary<string, AudioUnit> _active = new Dictionary<string, AudioUnit>();

	private Dictionary<string, AudioClip> _clips;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isMusic;

	public bool IsMute;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int id;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float masterVolume;

	public bool IsMusicChannel
	{
		get
		{
			return GetIsMusic();
		}
		private set
		{
			set_IsMusic(value);
		}
	}

	public bool IsSoundChannel
	{
		get
		{
			return GetIsSound();
		}
	}

	public int ChannelId
	{
		get
		{
			return GetChannelId();
		}
		private set
		{
			set_ID(value);
		}
	}

	public bool IsPlaying
	{
		get
		{
			return GetIsPlaying();
		}
	}

	public float Volume
	{
		get
		{
			return GetMasterVolume();
		}
		set
		{
			set_MasterVolume(value);
		}
	}

	internal Chanel(int OKNNNLIPODI, bool MHAFPAHIFKP, Dictionary<string, AudioClip> OCEMOHJPDLK)
	{
		set_IsMusic(MHAFPAHIFKP);
		set_ID(OKNNNLIPODI);
		_clips = OCEMOHJPDLK;
		IsMute = false;
		set_MasterVolume(1f);
	}

	public bool GetIsMusic()
	{
		return isMusic;
	}

	private void set_IsMusic(bool value)
	{
		isMusic = value;
	}

	public bool GetIsSound()
	{
		return !GetIsMusic();
	}

	public int GetChannelId()
	{
		return id;
	}

	private void set_ID(int value)
	{
		id = value;
	}

	public bool GetIsPlaying()
	{
		return _active.Count != 0;
	}

	public float GetMasterVolume()
	{
		return masterVolume;
	}

	public void set_MasterVolume(float value)
	{
		masterVolume = value;
	}

	internal void Play(PlayCommand LEKEGLMDAHA)
	{
		AudioClip audioClip = ((!_clips.ContainsKey(LEKEGLMDAHA.GetSound())) ? null : _clips[LEKEGLMDAHA.GetSound()]);
		if (!audioClip)
		{
			return;
		}
		if (!LEKEGLMDAHA.GetOverlap())
		{
			StopAll();
		}
		if (_active.ContainsKey(LEKEGLMDAHA.GetSound()))
		{
			_active[LEKEGLMDAHA.GetSound()].Init(this, LEKEGLMDAHA, audioClip);
			_active[LEKEGLMDAHA.GetSound()].set_IsMute(IsMute);
			return;
		}
		AudioUnit audioUnit = OverallUnitPool.GetFreeUnit();
		if ((bool)audioUnit)
		{
			audioUnit.Init(this, LEKEGLMDAHA, audioClip);
			audioUnit.set_IsMute(IsMute);
			_active.Add(LEKEGLMDAHA.GetSound(), audioUnit);
		}
	}

	internal void Pause(bool KCANPMPILKI)
	{
		foreach (AudioUnit value in _active.Values)
		{
			if (KCANPMPILKI)
			{
				value.Pause();
			}
			else
			{
				value.UnPause();
			}
		}
	}

	internal void Pause(bool KCANPMPILKI, string LGLFOBEIPKB)
	{
		if (_active.ContainsKey(LGLFOBEIPKB))
		{
			if (KCANPMPILKI)
			{
				_active[LGLFOBEIPKB].Pause();
			}
			else
			{
				_active[LGLFOBEIPKB].UnPause();
			}
		}
	}

	public void FreeUnit(AudioUnit PNJCPKNCLCP)
	{
		foreach (KeyValuePair<string, AudioUnit> item in _active)
		{
			if (item.Value == PNJCPKNCLCP)
			{
				_active.Remove(item.Key);
				break;
			}
		}
	}

	public void StopAll(bool BJIOMMPCLEA = false)
	{
		foreach (AudioUnit value in _active.Values)
		{
			value.Stop(BJIOMMPCLEA);
		}
		_active.Clear();
	}

	public void Mute()
	{
		IsMute = true;
		foreach (AudioUnit value in _active.Values)
		{
			value.set_IsMute(true);
		}
	}

	public void Unmute()
	{
		IsMute = false;
		foreach (AudioUnit value in _active.Values)
		{
			value.set_IsMute(false);
		}
	}
}
