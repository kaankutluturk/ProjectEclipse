using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace Nekki.Audio
{
	public class AudioManager : MonoBehaviour
	{
		private const int DefaultMusicChannel = 0;

		private const int DefaultSoundChannel = 1;

		private static AudioManager _instance;

		private static readonly Dictionary<int, Chanel> _chanels = new Dictionary<int, Chanel>();

		private static List<int> _musicChanels = new List<int>();

		private static Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

		private static Dictionary<string, float> _volumesByClips = new Dictionary<string, float>();

		private static AudioSettings _settings;

		public static void Init(string ALHKHJOJECK, int[] DOBMHKNFHCA, int[] HLGLHKIOPDE)
		{
			if ((bool)_instance)
			{
				AdvLog.LogWarning("AudioManager already exists!");
				return;
			}
			_musicChanels = new List<int>(DOBMHKNFHCA);
			_instance = new GameObject("_audioManager").AddComponent<AudioManager>();
			UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
			Load(ALHKHJOJECK);
			OverallUnitPool.Init(_instance);
			_settings = new AudioSettings();
		}

		public static void Init(string ALHKHJOJECK, int CEDJBBELDLH, int[] HLGLHKIOPDE)
		{
			Init(ALHKHJOJECK, new int[1] { CEDJBBELDLH }, HLGLHKIOPDE);
		}

		public static void Init(string ALHKHJOJECK, int CEDJBBELDLH, int DCMFMCGMMKG)
		{
			Init(ALHKHJOJECK, new int[1] { CEDJBBELDLH }, new int[1] { DCMFMCGMMKG });
		}

		public static void Init(string ALHKHJOJECK)
		{
			Init(ALHKHJOJECK, new int[1], new int[1] { 1 });
		}

		private static void Load(string ALHKHJOJECK)
		{
			if (Directory.Exists(ALHKHJOJECK))
			{
				string[] directories = Directory.GetDirectories(ALHKHJOJECK);
				for (int i = 0; i < directories.Length; i++)
				{
					Load(directories[i]);
				}
				List<string> list = new List<string>(Directory.GetFiles(ALHKHJOJECK, "*.xml"));
				for (int j = 0; j < list.Count; j++)
				{
					LoadSoundsXml(list[j], ALHKHJOJECK);
				}
			}
		}

		private static void LoadSoundsXml(string HIOFDADIEME, string ALHKHJOJECK)
		{
			if (!File.Exists(HIOFDADIEME))
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.LoadXml(File.ReadAllText(HIOFDADIEME));
			}
			catch (Exception ex)
			{
				AdvLog.LogWarning("wrong xml: " + ex.Message);
				return;
			}
			XmlElement xmlElement = xmlDocument["Sounds"];
			if (xmlElement == null)
			{
				return;
			}
			foreach (XmlNode childNode in xmlElement.ChildNodes)
			{
				if (childNode.Attributes == null)
				{
					continue;
				}
				string value = childNode.Attributes["Name"].Value;
				string text = childNode.Attributes["File"].Value.Replace("\\", "/");
				float value2 = ((childNode.Attributes["Volume"] != null) ? float.Parse(childNode.Attributes["Volume"].Value) : 1f);
				if (!string.IsNullOrEmpty(text))
				{
					string aKGGCMGELKH = ALHKHJOJECK + "/" + text;
					LoadClip(value, aKGGCMGELKH);
					if (_volumesByClips.ContainsKey(value))
					{
						_volumesByClips[value] = value2;
					}
					else
					{
						_volumesByClips.Add(value, value2);
					}
				}
			}
		}

		private static void LoadClip(string LGLFOBEIPKB, string AKGGCMGELKH)
		{
			if (!File.Exists(AKGGCMGELKH))
			{
				AdvLog.Log("No" + AKGGCMGELKH);
				return;
			}
			AudioClip audioClip = GeAudioClip(AKGGCMGELKH);
			if ((bool)audioClip)
			{
				if (!_clips.ContainsKey(LGLFOBEIPKB))
				{
					_clips.Add(LGLFOBEIPKB, audioClip);
				}
				else
				{
					_clips[LGLFOBEIPKB] = audioClip;
				}
			}
		}

		private static AudioClip GeAudioClip(string path)
		{
			WWW wWW = new WWW(string.Format("file:///{0}", path));
			while (!wWW.isDone && string.IsNullOrEmpty(wWW.error))
			{
			}
			if (string.IsNullOrEmpty(wWW.error))
			{
				return wWW.GetAudioClip();
			}
			AdvLog.LogError(wWW.error);
			return null;
		}

		public static void AddAudio(AudioClip PIKHEAGHOKB, string name, float JIJAJFEJJHK)
		{
			if (_clips.ContainsKey(name))
			{
				_clips[name] = PIKHEAGHOKB;
			}
			else
			{
				_clips.Add(name, PIKHEAGHOKB);
			}
			if (_volumesByClips.ContainsKey(name))
			{
				_volumesByClips[name] = JIJAJFEJJHK;
			}
			else
			{
				_volumesByClips.Add(name, JIJAJFEJJHK);
			}
		}

		public static void UnloadAudio(string name)
		{
			if (_clips.ContainsKey(name))
			{
				_clips.Remove(name);
			}
			if (_volumesByClips.ContainsKey(name))
			{
				_volumesByClips.Remove(name);
			}
		}

		public static void Play(int ADNDLGKIJJK, string LGLFOBEIPKB, bool KKHJAJFEPPA, bool ENNOPELJKPB, float JIJAJFEJJHK = 1f)
		{
			if (_clips.ContainsKey(LGLFOBEIPKB))
			{
				if (!_chanels.ContainsKey(ADNDLGKIJJK))
				{
					_chanels.Add(ADNDLGKIJJK, new Chanel(ADNDLGKIJJK, IsMusicChanel(ADNDLGKIJJK), _clips));
				}
				PlayCommand iPHFFPCPLDP = new PlayCommand(ADNDLGKIJJK, LGLFOBEIPKB, KKHJAJFEPPA, ENNOPELJKPB, JIJAJFEJJHK * _volumesByClips[LGLFOBEIPKB]);
				iPHFFPCPLDP.SetAudioSettings(_settings);
				_chanels[ADNDLGKIJJK].Play(iPHFFPCPLDP);
			}
		}

		public static void Play(PlayCommand LEKEGLMDAHA)
		{
			if (!_instance)
			{
				AdvLog.LogWarning("you must init AudioManager first!");
				return;
			}
			LEKEGLMDAHA.SetAudioSettings(_settings);
			if (_chanels.ContainsKey(LEKEGLMDAHA.GetChanelID()))
			{
				_chanels.Add(LEKEGLMDAHA.GetChanelID(), new Chanel(LEKEGLMDAHA.GetChanelID(), IsMusicChanel(LEKEGLMDAHA.GetChanelID()), _clips));
			}
			_chanels[LEKEGLMDAHA.GetChanelID()].Play(LEKEGLMDAHA);
		}

		public static void Mute(int ADNDLGKIJJK)
		{
			if (!_chanels.ContainsKey(ADNDLGKIJJK))
			{
				_chanels.Add(ADNDLGKIJJK, new Chanel(ADNDLGKIJJK, IsMusicChanel(ADNDLGKIJJK), _clips));
			}
			_chanels[ADNDLGKIJJK].Mute();
		}

		public static void UnMute(int ADNDLGKIJJK)
		{
			if (!_chanels.ContainsKey(ADNDLGKIJJK))
			{
				_chanels.Add(ADNDLGKIJJK, new Chanel(ADNDLGKIJJK, IsMusicChanel(ADNDLGKIJJK), _clips));
			}
			_chanels[ADNDLGKIJJK].Unmute();
		}

		private static void Pause(bool KCANPMPILKI, int ADNDLGKIJJK, string DPBKBKDCIOI)
		{
			if (_chanels.ContainsKey(ADNDLGKIJJK))
			{
				_chanels[ADNDLGKIJJK].Pause(KCANPMPILKI, DPBKBKDCIOI);
			}
		}

		public static void Pause(bool KCANPMPILKI, int ADNDLGKIJJK)
		{
			if (_chanels.ContainsKey(ADNDLGKIJJK))
			{
				_chanels[ADNDLGKIJJK].Pause(KCANPMPILKI);
			}
		}

		private static void Pause(bool KCANPMPILKI)
		{
			foreach (Chanel value in _chanels.Values)
			{
				value.Pause(KCANPMPILKI);
			}
		}

		public static void Stop(int AHCPPDFEDNJ, bool BJIOMMPCLEA = false)
		{
			if (_chanels.ContainsKey(AHCPPDFEDNJ))
			{
				_chanels[AHCPPDFEDNJ].StopAll(BJIOMMPCLEA);
			}
		}

		public static void SetVolume(float JIJAJFEJJHK, int AHCPPDFEDNJ)
		{
			if (!_chanels.ContainsKey(AHCPPDFEDNJ))
			{
				_chanels.Add(AHCPPDFEDNJ, new Chanel(AHCPPDFEDNJ, IsMusicChanel(AHCPPDFEDNJ), _clips));
			}
			_chanels[AHCPPDFEDNJ].set_MasterVolume(JIJAJFEJJHK);
		}

		public static float GetVolume(int AHCPPDFEDNJ)
		{
			if (_chanels.ContainsKey(AHCPPDFEDNJ))
			{
				return _chanels[AHCPPDFEDNJ].GetMasterVolume();
			}
			return 1f;
		}

		internal void Start()
		{
			if ((bool)_instance && _instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private static bool IsMusicChanel(int LIAILCGJBDK)
		{
			return _musicChanels.Contains(LIAILCGJBDK);
		}

		public static bool IsPlaying(int AHCPPDFEDNJ)
		{
			if (_chanels.ContainsKey(AHCPPDFEDNJ))
			{
				return _chanels[AHCPPDFEDNJ].GetIsPlaying();
			}
			return false;
		}

		public static bool CheckAudioLoaded(string LGLFOBEIPKB)
		{
			return _clips.ContainsKey(LGLFOBEIPKB);
		}
	}
}
