using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class AudioSettings
{
	public delegate void VolumeChangedHandler(float volume);

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private VolumeChangedHandler SoundsVolumeChanged;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private VolumeChangedHandler MusicVolumeChanged;

	private float masterVolume;

	private float soundsVolume;

	private float musicVolume;

	private bool _muted;

	public bool IsMuted
	{
		get
		{
			return GetMuted();
		}
		set
		{
			set_Muted(value);
		}
	}

	public float MasterVolumeLevel
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

	public float SoundsVolume
	{
		get
		{
			return GetSoundsVolume();
		}
		set
		{
			SetSoundsVolume(value);
		}
	}

	public float MusicVolume
	{
		get
		{
			return GetMusicVolume();
		}
		set
		{
			SetMusicVolume(value);
		}
	}

	public event VolumeChangedHandler OnSoundsVolumeChanged
	{
		add
		{
			AddSoundsVolumeChanged(value);
		}
		remove
		{
			RemoveSoundsVolumeChanged(value);
		}
	}

	public event VolumeChangedHandler OnMusicVolumeChanged
	{
		add
		{
			AddMusicVolumeChanged(value);
		}
		remove
		{
			RemoveMusicVolumeChanged(value);
		}
	}

	internal AudioSettings()
	{
		masterVolume = PlayerPrefs.GetFloat("_masterVolume", 1f);
		musicVolume = PlayerPrefs.GetFloat("_musicVolume", 1f);
		soundsVolume = PlayerPrefs.GetFloat("_soundsVolume", 1f);
		_muted = PlayerPrefs.GetInt("_muted", 0) == 1;
	}

	public void AddSoundsVolumeChanged(VolumeChangedHandler value)
	{
		VolumeChangedHandler currentHandler = SoundsVolumeChanged;
		VolumeChangedHandler previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref SoundsVolumeChanged, (VolumeChangedHandler)Delegate.Combine(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public void RemoveSoundsVolumeChanged(VolumeChangedHandler value)
	{
		VolumeChangedHandler currentHandler = SoundsVolumeChanged;
		VolumeChangedHandler previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref SoundsVolumeChanged, (VolumeChangedHandler)Delegate.Remove(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public void AddMusicVolumeChanged(VolumeChangedHandler value)
	{
		VolumeChangedHandler currentHandler = MusicVolumeChanged;
		VolumeChangedHandler previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref MusicVolumeChanged, (VolumeChangedHandler)Delegate.Combine(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public void RemoveMusicVolumeChanged(VolumeChangedHandler value)
	{
		VolumeChangedHandler currentHandler = MusicVolumeChanged;
		VolumeChangedHandler previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref MusicVolumeChanged, (VolumeChangedHandler)Delegate.Remove(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public bool GetMuted()
	{
		return _muted;
	}

	public void set_Muted(bool value)
	{
		if (value != _muted)
		{
			_muted = value;
			RaiseSoundsVolumeChanged(GetSoundsVolume());
			RaiseMusicVolumeChanged(GetSoundsVolume());
			PlayerPrefs.SetInt("_muted", _muted ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	public float GetMasterVolume()
	{
		return (!GetMuted()) ? masterVolume : 0f;
	}

	public void set_MasterVolume(float value)
	{
		value = Mathf.Clamp01(value);
		if (!(Math.Abs(value - masterVolume) < 0.01f))
		{
			masterVolume = value;
			RaiseSoundsVolumeChanged(GetSoundsVolume());
			RaiseMusicVolumeChanged(GetSoundsVolume());
			PlayerPrefs.SetFloat("_masterVolume", masterVolume);
			PlayerPrefs.Save();
		}
	}

	public float GetSoundsVolume()
	{
		return soundsVolume * GetMasterVolume();
	}

	public void SetSoundsVolume(float value)
	{
		value = Mathf.Clamp01(value);
		if (!(Math.Abs(value - soundsVolume) < 0.01f))
		{
			soundsVolume = Mathf.Clamp01(value);
			RaiseSoundsVolumeChanged(GetSoundsVolume());
			PlayerPrefs.SetFloat("_soundsVolume", soundsVolume);
			PlayerPrefs.Save();
		}
	}

	public float GetMusicVolume()
	{
		return musicVolume * GetMasterVolume();
	}

	public void SetMusicVolume(float value)
	{
		value = Mathf.Clamp01(value);
		if (!(Math.Abs(value - musicVolume) < 0.01f))
		{
			musicVolume = Mathf.Clamp01(value);
			RaiseMusicVolumeChanged(GetSoundsVolume());
			PlayerPrefs.SetFloat("_musicVolume", musicVolume);
			PlayerPrefs.Save();
		}
	}

	protected virtual void RaiseSoundsVolumeChanged(float volume)
	{
		if (SoundsVolumeChanged != null)
		{
			SoundsVolumeChanged(volume);
		}
	}

	protected virtual void RaiseMusicVolumeChanged(float volume)
	{
		if (MusicVolumeChanged != null)
		{
			MusicVolumeChanged(volume);
		}
	}
}
