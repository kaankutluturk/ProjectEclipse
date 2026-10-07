using System.Collections.Generic;
using System.Diagnostics;
using Nekki.Audio;
using UnityEngine;

public static class Sound
{
	private static int MusicChannel = 0;

	private static int FirstSoundChannel = 1;

	private static int LastSoundChannel = 10;

	private static string soundsPath = "sounds/";

	private static string musicPath = "music/";

	private static string soundExtension = ".wav";

	private static string musicExtension = ".ogg";

	private static float soundVolume = 1f;

	private static float musicVolume = 1f;

	private static bool isMuted = false;

	private static bool isSoundMuted = false;

	private static bool isMusicMuted = false;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool isMusicPaused;

	private static List<KeyValuePair<string, uint>> playingSounds = new List<KeyValuePair<string, uint>>();

	private static List<KeyValuePair<string, uint>> loopingSounds = new List<KeyValuePair<string, uint>>();

	private static readonly HashSet<string> MissingAudioWarnings = new HashSet<string>();

	// The migrated stage data uses modern, descriptive music ids while the
	// recovered classic soundtrack keeps its original numbered filenames.
	private static readonly Dictionary<string, string> RecoveredMusicAliases =
		new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
		{
			{ "samurai_spirit", "fight1_samurai_spirit" },
			{ "blade_dance", "fight2_blade_dance" },
			{ "vengeance", "fight3_vengeance" },
			{ "forest_of_death", "fight4_forest_of_death" },
			{ "ninja_in_the_night", "fight5_ninja_in_the_night" },
			{ "ninja_in_the_night_old", "fight5_ninja_in_the_night" },
			{ "sparring", "fight6_sparring" },
			{ "fat_boss", "fight7_fat_boss" },
			{ "final_boss", "fight8_final_boss" },
			{ "master_skills", "fight9_master_skills" },
			{ "black_warrior", "fight10_black_warrior" },
			{ "ronin", "fight11_ronin" },
			{ "deadly_smoke", "fight12_deadly_smoke" },
			{ "deadly_smoke_old", "fight12_deadly_smoke" },
			{ "old_sensei", "fight13_old_sensei" },
			{ "old_sensei_old", "fight13_old_sensei" },
			{ "ship_battle", "fight14_ship_battle" },
			{ "shadow_lady", "fight15_shadow_lady" },
			{ "the_battlefield_flowers", "fight16_the_battlefield_flowers" },
			{ "cave", "fight17_cave" },
			{ "fuji", "fight18_fuji" },
			{ "volcano", "fight19_volcano" },
			{ "bridge_to_the_other_side", "fight20_bridge_to_the_other_side" },
			{ "lesson_in_the_dark_room", "fight21_lesson_in_the_dark_room" },
			{ "heavenly_clouds", "fight22_heavenly_clouds" },
			{ "burning_town", "fight23_burning_town" },
			{ "burning_town_old", "fight23_burning_town" },
			{ "ruins_village", "fight24_ruins_village" },
			{ "hive", "fight25_hive" },
			{ "factory", "fight27_factory" },
			{ "flying_rocks", "fight28_flying_rocks" },
			{ "gates_of_shadows", "fight30_gates_of_shadows" },
			{ "graveyard_ships", "fight31_graveyard_ships" },
			{ "starship", "fight32_starship" },
			{ "stone_forest", "fight33_stone_forest" },
			{ "halls_of_the_dead_heroes", "fight34_halls_of_the_dead_heroes" },
			{ "stardocks", "fight36_stardocks" },
			// These newer ids have no matching clip in the recovered pack. Use the
			// closest location/theme track rather than collapsing them all to fight 1.
			{ "deep", "fight34_halls_of_the_dead_heroes" },
			{ "dao_temple", "fight21_lesson_in_the_dark_room" },
			{ "fight38_sakura_forest", "fight4_forest_of_death" },
			{ "sky_isles", "fight22_heavenly_clouds" },
			{ "spaceship", "fight32_starship" },
			{ "stone_dragon", "fight19_volcano" },
			{ "the_monastery", "fight13_old_sensei" }
		};

	public static uint MaxPlayableSounds = 10u;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string currentMusicName;

	public static string SoundsPath
	{
		get
		{
			return GetSoundsPath();
		}
		private set
		{
			SetSoundsPath(value);
		}
	}

	public static string MusicPath
	{
		get
		{
			return GetMusicPath();
		}
		private set
		{
			SetMusicPath(value);
		}
	}

	public static string SoundExtension
	{
		get
		{
			return GetSoundExtension();
		}
		private set
		{
			SetSoundExtension(value);
		}
	}

	public static string MusicExtension
	{
		get
		{
			return GetMusicExtension();
		}
		private set
		{
			SetMusicExtension(value);
		}
	}

	public static float SoundVolume
	{
		get
		{
			return GetSoundVolume();
		}
		set
		{
			SetSoundVolume(value);
		}
	}

	public static float MusicVolume
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

	public static bool IsMuted
	{
		get
		{
			return GetMuted();
		}
		set
		{
			SetMuted(value);
		}
	}

	public static bool IsSoundMuted
	{
		get
		{
			return GetSoundMuted();
		}
		set
		{
			SetSoundMuted(value);
		}
	}

	public static bool IsMusicMuted
	{
		get
		{
			return GetMusicMuted();
		}
		set
		{
			SetMusicMuted(value);
		}
	}

	public static bool IsMusicPlaying
	{
		get
		{
			return GetMusicPlaying();
		}
	}

	public static bool IsMusicPaused
	{
		get
		{
			return GetMusicPaused();
		}
		private set
		{
			SetMusicPaused(value);
		}
	}

	public static string CurrentMusicName
	{
		get
		{
			return GetCurrentMusicName();
		}
		private set
		{
			SetCurrentMusicName(value);
		}
	}

	public static string GetSoundsPath()
	{
		return soundsPath;
	}

	private static void SetSoundsPath(string value)
	{
		soundsPath = value;
		if (soundsPath[soundsPath.Length - 1] != '/')
		{
			soundsPath += "/";
		}
	}

	public static string GetMusicPath()
	{
		return musicPath;
	}

	private static void SetMusicPath(string value)
	{
		musicPath = value;
		if (musicPath[musicPath.Length - 1] != '/')
		{
			musicPath += "/";
		}
	}

	public static string GetSoundExtension()
	{
		return soundExtension;
	}

	private static void SetSoundExtension(string value)
	{
		soundExtension = value;
		if (soundExtension[0] != '.')
		{
			soundExtension.Insert(0, ".");
		}
	}

	public static string GetMusicExtension()
	{
		return musicExtension;
	}

	private static void SetMusicExtension(string value)
	{
		musicExtension = value;
		if (musicExtension[0] != '.')
		{
			musicExtension.Insert(0, ".");
		}
	}

	public static float GetSoundVolume()
	{
		return soundVolume;
	}

	public static void SetSoundVolume(float value)
	{
		soundVolume = Mathf.Clamp(value, 0f, 1f);
		ApplySoundVolumeToChannels(soundVolume);
	}

	public static float GetMusicVolume()
	{
		return musicVolume;
	}

	public static void SetMusicVolume(float value)
	{
		musicVolume = Mathf.Clamp(value, 0f, 1f);
		SetVolumeToChannel(MusicChannel, value, isMusicMuted);
	}

	public static bool GetMuted()
	{
		return isMuted;
	}

	public static void SetMuted(bool value)
	{
		isMuted = value;
		SetSoundMuted(value);
		SetMusicMuted(value);
	}

	public static bool GetSoundMuted()
	{
		return isSoundMuted;
	}

	public static void SetSoundMuted(bool value)
	{
		if (value != isSoundMuted)
		{
			isSoundMuted = value;
			ApplySoundMuteToChannels(value);
		}
	}

	public static bool GetMusicMuted()
	{
		return isMusicMuted;
	}

	public static void SetMusicMuted(bool value)
	{
		if (value != isMusicMuted)
		{
			isMusicMuted = value;
			SetMuteToChannel(MusicChannel, isMusicMuted);
		}
	}

	public static bool GetMusicPlaying()
	{
		return AudioManager.IsPlaying(MusicChannel);
	}

	public static bool GetMusicPaused()
	{
		return isMusicPaused;
	}

	private static void SetMusicPaused(bool value)
	{
		isMusicPaused = value;
	}

	public static string GetCurrentMusicName()
	{
		return currentMusicName;
	}

	private static void SetCurrentMusicName(string value)
	{
		currentMusicName = value;
	}

	public static void SetPaths(string EGPPPNJHNMF, string ADLELPHJADH)
	{
		SetSoundsPath(EGPPPNJHNMF);
		SetMusicPath(ADLELPHJADH);
	}

	public static void SetExtensions(string IBFNOCDNNDB, string NHAKIADKLPG)
	{
		SetSoundExtension(IBFNOCDNNDB);
		SetMusicExtension(NHAKIADKLPG);
	}

	public static void SetVolume(float MJDCMAEEIPJ, float FEFBNAOBBBE)
	{
		SetSoundVolume(MJDCMAEEIPJ);
		SetMusicVolume(FEFBNAOBBBE);
	}

	private static void ApplySoundVolumeToChannels(float MJDCMAEEIPJ)
	{
		for (int i = FirstSoundChannel; i <= LastSoundChannel; i++)
		{
			SetVolumeToChannel(i, MJDCMAEEIPJ, isSoundMuted);
		}
	}

	private static void SetVolumeToChannel(int LMGPAGINHGD, float ONHAHMIHGJC, bool NGHNGOJHJDE)
	{
		AudioManager.SetVolume(ONHAHMIHGJC, LMGPAGINHGD);
		SetMuteToChannel(LMGPAGINHGD, NGHNGOJHJDE);
	}

	private static void ApplySoundMuteToChannels(bool JFIDKIMPPDH)
	{
		for (int i = FirstSoundChannel; i <= LastSoundChannel; i++)
		{
			SetMuteToChannel(i, JFIDKIMPPDH);
		}
	}

	private static void SetMuteToChannel(int ADNDLGKIJJK, bool JFIDKIMPPDH)
	{
		if (JFIDKIMPPDH)
		{
			AudioManager.Mute(ADNDLGKIJJK);
		}
		else
		{
			AudioManager.UnMute(ADNDLGKIJJK);
		}
	}

	public static int PlaySound(string DPBKBKDCIOI, bool KKHJAJFEPPA = false, float JIJAJFEJJHK = 1f)
	{
		if (Eclipse.Multiplayer.SpectatorInputSource.SuppressAudio && !KKHJAJFEPPA) return -1;
		if (KKHJAJFEPPA)
		{
			var loop = FindLoopingSound(DPBKBKDCIOI);
			if (loop.Key != string.Empty && AudioManager.IsPlaying((int)loop.Value)) return (int)loop.Value;
		}
		if (Fight.GetCurrentFight()?.IsTitleSparring == true)
		{
			Eclipse.UI.EclipseUiAudio.PlayTitleFightSound(DPBKBKDCIOI, KKHJAJFEPPA, JIJAJFEJJHK, LoadSoundClip);
			return -1;
		}
		// A rollback re-simulation replays ticks whose one-shot sounds already played.
		if (!KKHJAJFEPPA && Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
		{
			return -1;
		}
		bool flag = AudioManager.CheckAudioLoaded(DPBKBKDCIOI);
		if (!flag)
		{
			flag = LoadSound(DPBKBKDCIOI, JIJAJFEJJHK);
		}
		int num = -1;
		if (flag)
		{
			num = GetFreeChannel();
			PlayOnChannel(num, DPBKBKDCIOI, KKHJAJFEPPA, isSoundMuted);
			TrackPlayingSound(DPBKBKDCIOI, (uint)num);
			if (KKHJAJFEPPA)
			{
				TrackLoopingSound(DPBKBKDCIOI, (uint)num);
				Eclipse.Multiplayer.Rollback.RollbackObjects.LoopStarted(DPBKBKDCIOI);
			}
		}
		else
		{
			// Newer animation data references a handful of optional sounds that
			// were not present in the recovered client. Missing sound effects must
			// not be treated as gameplay errors (or be logged every animation
			// frame), but retain one useful diagnostic with the real resource name.
			string text = string.IsNullOrEmpty(DPBKBKDCIOI) ? "<empty>" : DPBKBKDCIOI;
			if (MissingAudioWarnings.Add("sound:" + text))
			{
				UnityEngine.Debug.LogWarning("[Audio] Missing optional sound '" + text + "'; skipping it.");
			}
		}
		return num;
	}

	public static int PlaySound(string DPBKBKDCIOI, float JIJAJFEJJHK)
	{
		return PlaySound(DPBKBKDCIOI, false, JIJAJFEJJHK);
	}

	public static void StopAllSounds()
	{
		for (int i = FirstSoundChannel; i <= LastSoundChannel; i++)
		{
			AudioManager.Stop(i);
		}
		loopingSounds.Clear();
		playingSounds.Clear();
	}

	// best guess for name
	public static void StopLoopedSounds()
	{
		if (Fight.GetCurrentFight()?.IsTitleSparring == true)
		{
			Eclipse.UI.EclipseUiAudio.StopTitleFightSounds();
			return;
		}
		foreach (KeyValuePair<string, uint> item in loopingSounds)
		{
			StopSound((int)item.Value);
		}
		loopingSounds.Clear();
	}

	public static void PauseAllSounds()
	{
		for (int i = FirstSoundChannel; i <= LastSoundChannel; i++)
		{
			AudioManager.Pause(true, i);
		}
	}

	public static void ResumeAllSounds()
	{
		for (int i = FirstSoundChannel; i <= LastSoundChannel; i++)
		{
			AudioManager.Pause(false, i);
		}
	}

	private static void StopSound(int LMGPAGINHGD)
	{
		AudioManager.Stop(LMGPAGINHGD);
	}

	public static void StopSound(string path)
	{
		if (Fight.GetCurrentFight()?.IsTitleSparring == true)
		{
			Eclipse.UI.EclipseUiAudio.StopTitleFightSound(path);
			return;
		}
		KeyValuePair<string, uint> keyValuePair = FindLoopingSound(path);
		if (keyValuePair.Key != string.Empty)
		{
			UntrackLoopingSound(path);
			StopSound((int)keyValuePair.Value);
			return;
		}
		foreach (KeyValuePair<string, uint> item in playingSounds)
		{
			if (keyValuePair.Key == path)
			{
				StopSound((int)keyValuePair.Value);
			}
		}
	}

	public static void PlayMusic(string LOJOJHIFCBL, bool KKHJAJFEPPA = true)
	{
		StopMusic();
		if (string.IsNullOrEmpty(LOJOJHIFCBL))
		{
			if (MissingAudioWarnings.Add("music:<empty>"))
			{
				UnityEngine.Debug.LogWarning("[Audio] Fight requested an empty music name; using the recovered default track.");
			}
			LOJOJHIFCBL = "fight1_samurai_spirit";
		}
		AudioClip audioClip;
		if (Eclipse.Modding.ModAssetBinding.TryLoadAudio(LOJOJHIFCBL, out audioClip))
		{
			AudioManager.AddAudio(audioClip, LOJOJHIFCBL, 1f);
			PlayOnChannel(MusicChannel, LOJOJHIFCBL, KKHJAJFEPPA, isMusicMuted);
			SetCurrentMusicName(LOJOJHIFCBL);
			return;
		}
		if (Eclipse.Modding.ModAssetBinding.IsQualified(LOJOJHIFCBL))
		{
			if (MissingAudioWarnings.Add("music-mod-missing:" + LOJOJHIFCBL))
			{
				UnityEngine.Debug.LogWarning("[Audio] Missing mod music '" + LOJOJHIFCBL + "'; continuing without music.");
			}
			return;
		}
		string bLMBLOKPMEC = musicPath;
		string text = LOJOJHIFCBL;
		bool flag = text.EndsWith(".ogg", System.StringComparison.OrdinalIgnoreCase);
		bLMBLOKPMEC = ((!flag && !SF2Paths.UseBundledResources) ? (bLMBLOKPMEC + LOJOJHIFCBL + musicExtension) : (bLMBLOKPMEC + LOJOJHIFCBL));
		audioClip = ResourceManager.GetAudioClip(bLMBLOKPMEC);
		if (audioClip == null)
		{
			string fallback = ResolveRecoveredMusicName(LOJOJHIFCBL);
			audioClip = ResourceManager.GetAudioClip(BuildMusicPath(fallback));
			if (audioClip == null)
			{
				fallback = "fight1_samurai_spirit";
				audioClip = ResourceManager.GetAudioClip(BuildMusicPath(fallback));
			}
			if (audioClip == null)
			{
				if (MissingAudioWarnings.Add("music-missing:" + LOJOJHIFCBL))
				{
					UnityEngine.Debug.LogWarning("[Audio] Missing music '" + LOJOJHIFCBL + "'; continuing without music.");
				}
				return;
			}
			if (MissingAudioWarnings.Add("music-fallback:" + LOJOJHIFCBL))
			{
				UnityEngine.Debug.Log("[Audio] Resolved music '" + LOJOJHIFCBL + "' to recovered track '" + fallback + "'.");
			}
		}
		AudioManager.AddAudio(audioClip, LOJOJHIFCBL, 1f);
		PlayOnChannel(MusicChannel, LOJOJHIFCBL, KKHJAJFEPPA, isMusicMuted);
		SetCurrentMusicName(LOJOJHIFCBL);
	}

	private static string ResolveRecoveredMusicName(string requested)
	{
		string text = (requested ?? string.Empty).Trim();
		if (text.EndsWith(".ogg", System.StringComparison.OrdinalIgnoreCase) ||
			text.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase))
		{
			text = text.Substring(0, text.Length - 4);
		}
		string recovered;
		if (RecoveredMusicAliases.TryGetValue(text, out recovered))
		{
			return recovered;
		}
		return "fight1_samurai_spirit";
	}

	private static string BuildMusicPath(string musicName)
	{
		return musicPath + musicName + ((!SF2Paths.UseBundledResources) ? musicExtension : string.Empty);
	}

	public static void PlayOnChannel(int ADNDLGKIJJK, string DPBKBKDCIOI, bool KKHJAJFEPPA, bool KPCIIDFJCOB)
	{
		AudioManager.Play(ADNDLGKIJJK, DPBKBKDCIOI, KKHJAJFEPPA, true);
		SetMuteToChannel(ADNDLGKIJJK, KPCIIDFJCOB);
	}

	// best guess for name
	public static void StopMusic()
	{
		// Paused sources also need stopping when a fight is left.
		AudioManager.Stop(MusicChannel);
		if (GetCurrentMusicName() != null)
		{
			AudioManager.UnloadAudio(GetCurrentMusicName());
			SetCurrentMusicName(null);
		}
	}

	public static void PauseMusic()
	{
		if (AudioManager.IsPlaying(MusicChannel))
		{
			AudioManager.Pause(true, MusicChannel);
			SetMusicPaused(true);
		}
	}

	public static void ResumeMusic()
	{
		if (AudioManager.IsPlaying(MusicChannel))
		{
			AudioManager.Pause(false, MusicChannel);
			SetMusicPaused(false);
		}
	}

	public static void PreloadSounds(List<string> NAECCPFPEHC)
	{
		foreach (string item in NAECCPFPEHC)
		{
			LoadSound(item, GetSoundVolume());
		}
	}

	public static bool LoadSound(string DPBKBKDCIOI, float JIJAJFEJJHK = 1f)
	{
		AudioClip audioClip = LoadSoundClip(DPBKBKDCIOI);
		if (audioClip != null) AudioManager.AddAudio(audioClip, DPBKBKDCIOI, JIJAJFEJJHK);
		return audioClip != null;
	}

	private static AudioClip LoadSoundClip(string DPBKBKDCIOI)
	{
		AudioClip externalClip;
		if (Eclipse.Modding.ModAssetBinding.TryLoadAudio(DPBKBKDCIOI, out externalClip))
		{
			return externalClip;
		}
		if (Eclipse.Modding.ModAssetBinding.IsQualified(DPBKBKDCIOI))
		{
			return null;
		}
		string text = GetSoundsPath();
		text += DPBKBKDCIOI;
		if (!SF2Paths.UseBundledResources)
		{
			text += GetSoundExtension();
		}
		return ResourceManager.GetAudioClip(text);
	}

	private static void TrackPlayingSound(string path, uint OKNNNLIPODI)
	{
		KeyValuePair<string, uint> item = new KeyValuePair<string, uint>(path, OKNNNLIPODI);
		if (playingSounds.Count == MaxPlayableSounds)
		{
			playingSounds.Remove(playingSounds[0]);
		}
		playingSounds.Add(item);
	}

	private static void TrackLoopingSound(string path, uint OKNNNLIPODI)
	{
		KeyValuePair<string, uint> item = new KeyValuePair<string, uint>(path, OKNNNLIPODI);
		loopingSounds.Add(item);
	}

	private static void UntrackLoopingSound(string path)
	{
		foreach (KeyValuePair<string, uint> item in loopingSounds)
		{
			if (item.Key == path)
			{
				loopingSounds.Remove(item);
				break;
			}
		}
	}

	private static KeyValuePair<string, uint> FindLoopingSound(string path)
	{
		KeyValuePair<string, uint> result = new KeyValuePair<string, uint>(string.Empty, 0u);
		foreach (KeyValuePair<string, uint> item in loopingSounds)
		{
			if (item.Key == path)
			{
				result = item;
				return result;
			}
		}
		return result;
	}

	public static void Init()
	{
		int[] array = new int[LastSoundChannel - FirstSoundChannel + 1];
		for (int i = FirstSoundChannel; i <= LastSoundChannel; i++)
		{
			array[i - FirstSoundChannel] = i;
		}
		AudioManager.Init(null, MusicChannel, array);
	}

	public static int GetFreeChannel()
	{
		for (int i = FirstSoundChannel; i <= LastSoundChannel; i++)
		{
			if (!AudioManager.IsPlaying(i))
			{
				return i;
			}
		}
		return LastSoundChannel;
	}
}
