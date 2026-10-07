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

	internal Chanel(int id, bool isMusic, Dictionary<string, AudioClip> clips)
	{
		set_IsMusic(isMusic);
		set_ID(id);
		_clips = clips;
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

	internal void Play(PlayCommand command)
	{
		AudioClip audioClip = ((!_clips.ContainsKey(command.GetSound())) ? null : _clips[command.GetSound()]);
		if (!audioClip)
		{
			return;
		}
		if (!command.GetOverlap())
		{
			StopAll();
		}
		if (_active.ContainsKey(command.GetSound()))
		{
			_active[command.GetSound()].Init(this, command, audioClip);
			_active[command.GetSound()].set_IsMute(IsMute);
			return;
		}
		AudioUnit audioUnit = OverallUnitPool.GetFreeUnit();
		if ((bool)audioUnit)
		{
			audioUnit.Init(this, command, audioClip);
			audioUnit.set_IsMute(IsMute);
			_active.Add(command.GetSound(), audioUnit);
		}
	}

	internal void Pause(bool paused)
	{
		foreach (AudioUnit value in _active.Values)
		{
			if (paused)
			{
				value.Pause();
			}
			else
			{
				value.UnPause();
			}
		}
	}

	internal void Pause(bool paused, string soundName)
	{
		if (_active.ContainsKey(soundName))
		{
			if (paused)
			{
				_active[soundName].Pause();
			}
			else
			{
				_active[soundName].UnPause();
			}
		}
	}

	public void FreeUnit(AudioUnit unit)
	{
		foreach (KeyValuePair<string, AudioUnit> item in _active)
		{
			if (item.Value == unit)
			{
				_active.Remove(item.Key);
				break;
			}
		}
	}

	public void StopAll(bool immediately = false)
	{
		foreach (AudioUnit value in _active.Values)
		{
			value.Stop(immediately);
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
