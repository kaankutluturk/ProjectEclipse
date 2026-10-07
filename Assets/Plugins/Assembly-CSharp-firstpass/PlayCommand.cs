using System.Diagnostics;
using UnityEngine;

public class PlayCommand
{
	private float volume;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int chanelId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string sound;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool loop;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool overlap;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isMusic;

	private AudioSettings audioSettings;

	public int ChannelId
	{
		get
		{
			return GetChanelID();
		}
		private set
		{
			set_ChanelID(value);
		}
	}

	public string SoundName
	{
		get
		{
			return GetSound();
		}
		private set
		{
			set_Sound(value);
		}
	}

	public bool Loop
	{
		get
		{
			return GetLoop();
		}
		private set
		{
			SetLoop(value);
		}
	}

	public bool Overlap
	{
		get
		{
			return GetOverlap();
		}
		private set
		{
			SetOverlap(value);
		}
	}

	public bool IsMusicTrack
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

	public float Volume
	{
		get
		{
			return GetVolume();
		}
		set
		{
			SetVolume(value);
		}
	}

	public PlayCommand(int ADNDLGKIJJK, string LGLFOBEIPKB, bool KKHJAJFEPPA, bool HBCDAPJLKOJ, float JIJAJFEJJHK)
	{
		SetVolume(JIJAJFEJJHK);
		SetOverlap(HBCDAPJLKOJ);
		SetLoop(KKHJAJFEPPA);
		set_Sound(LGLFOBEIPKB);
		set_ChanelID(ADNDLGKIJJK);
	}

	public int GetChanelID()
	{
		return chanelId;
	}

	private void set_ChanelID(int value)
	{
		chanelId = value;
	}

	public string GetSound()
	{
		return sound;
	}

	private void set_Sound(string value)
	{
		sound = value;
	}

	public bool GetLoop()
	{
		return loop;
	}

	private void SetLoop(bool value)
	{
		loop = value;
	}

	public bool GetOverlap()
	{
		return overlap;
	}

	private void SetOverlap(bool value)
	{
		overlap = value;
	}

	public bool GetIsMusic()
	{
		return isMusic;
	}

	private void set_IsMusic(bool value)
	{
		isMusic = value;
	}

	public float GetVolume()
	{
		return volume * ((!GetIsMusic()) ? audioSettings.GetSoundsVolume() : audioSettings.GetMusicVolume());
	}

	public void SetVolume(float value)
	{
		volume = Mathf.Clamp01(value);
	}

	internal void SetMusic(bool MHAFPAHIFKP)
	{
		set_IsMusic(MHAFPAHIFKP);
	}

	internal void SetAudioSettings(AudioSettings CCKFFGJGEJE)
	{
		audioSettings = CCKFFGJGEJE;
	}
}
