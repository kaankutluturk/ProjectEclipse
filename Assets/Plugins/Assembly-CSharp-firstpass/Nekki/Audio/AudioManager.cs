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

		public static void Init(string rootPath, int[] musicChannels, int[] soundChannels)
		{
			if ((bool)_instance)
			{
				AdvLog.LogWarning("AudioManager already exists!");
				return;
			}
			_musicChanels = new List<int>(musicChannels);
			_instance = new GameObject("_audioManager").AddComponent<AudioManager>();
			UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
			Load(rootPath);
			OverallUnitPool.Init(_instance);
			_settings = new AudioSettings();
		}

		public static void Init(string rootPath, int musicChannel, int[] soundChannels)
		{
			Init(rootPath, new int[1] { musicChannel }, soundChannels);
		}

		public static void Init(string rootPath, int musicChannel, int soundChannel)
		{
			Init(rootPath, new int[1] { musicChannel }, new int[1] { soundChannel });
		}

		public static void Init(string rootPath)
		{
			Init(rootPath, new int[1], new int[1] { 1 });
		}

		private static void Load(string directoryPath)
		{
			if (Directory.Exists(directoryPath))
			{
				string[] directories = Directory.GetDirectories(directoryPath);
				for (int i = 0; i < directories.Length; i++)
				{
					Load(directories[i]);
				}
				List<string> list = new List<string>(Directory.GetFiles(directoryPath, "*.xml"));
				for (int j = 0; j < list.Count; j++)
				{
					LoadSoundsXml(list[j], directoryPath);
				}
			}
		}

		private static void LoadSoundsXml(string xmlPath, string rootPath)
		{
			if (!File.Exists(xmlPath))
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.LoadXml(File.ReadAllText(xmlPath));
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
					string clipPath = rootPath + "/" + text;
					LoadClip(value, clipPath);
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

		private static void LoadClip(string clipName, string clipPath)
		{
			if (!File.Exists(clipPath))
			{
				AdvLog.Log("No" + clipPath);
				return;
			}
			AudioClip audioClip = GeAudioClip(clipPath);
			if ((bool)audioClip)
			{
				if (!_clips.ContainsKey(clipName))
				{
					_clips.Add(clipName, audioClip);
				}
				else
				{
					_clips[clipName] = audioClip;
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

		public static void AddAudio(AudioClip clip, string name, float volume)
		{
			if (_clips.ContainsKey(name))
			{
				_clips[name] = clip;
			}
			else
			{
				_clips.Add(name, clip);
			}
			if (_volumesByClips.ContainsKey(name))
			{
				_volumesByClips[name] = volume;
			}
			else
			{
				_volumesByClips.Add(name, volume);
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

		public static void Play(int channelId, string clipName, bool loop, bool overlap, float volume = 1f)
		{
			if (_clips.ContainsKey(clipName))
			{
				if (!_chanels.ContainsKey(channelId))
				{
					_chanels.Add(channelId, new Chanel(channelId, IsMusicChanel(channelId), _clips));
				}
				PlayCommand command = new PlayCommand(channelId, clipName, loop, overlap, volume * _volumesByClips[clipName]);
				command.SetAudioSettings(_settings);
				_chanels[channelId].Play(command);
			}
		}

		public static void Play(PlayCommand command)
		{
			if (!_instance)
			{
				AdvLog.LogWarning("you must init AudioManager first!");
				return;
			}
			command.SetAudioSettings(_settings);
			if (_chanels.ContainsKey(command.GetChanelID()))
			{
				_chanels.Add(command.GetChanelID(), new Chanel(command.GetChanelID(), IsMusicChanel(command.GetChanelID()), _clips));
			}
			_chanels[command.GetChanelID()].Play(command);
		}

		public static void Mute(int channelId)
		{
			if (!_chanels.ContainsKey(channelId))
			{
				_chanels.Add(channelId, new Chanel(channelId, IsMusicChanel(channelId), _clips));
			}
			_chanels[channelId].Mute();
		}

		public static void UnMute(int channelId)
		{
			if (!_chanels.ContainsKey(channelId))
			{
				_chanels.Add(channelId, new Chanel(channelId, IsMusicChanel(channelId), _clips));
			}
			_chanels[channelId].Unmute();
		}

		private static void Pause(bool pause, int channelId, string clipName)
		{
			if (_chanels.ContainsKey(channelId))
			{
				_chanels[channelId].Pause(pause, clipName);
			}
		}

		public static void Pause(bool pause, int channelId)
		{
			if (_chanels.ContainsKey(channelId))
			{
				_chanels[channelId].Pause(pause);
			}
		}

		private static void Pause(bool pause)
		{
			foreach (Chanel value in _chanels.Values)
			{
				value.Pause(pause);
			}
		}

		public static void Stop(int channelId, bool fadeOut = false)
		{
			if (_chanels.ContainsKey(channelId))
			{
				_chanels[channelId].StopAll(fadeOut);
			}
		}

		public static void SetVolume(float volume, int channelId)
		{
			if (!_chanels.ContainsKey(channelId))
			{
				_chanels.Add(channelId, new Chanel(channelId, IsMusicChanel(channelId), _clips));
			}
			_chanels[channelId].set_MasterVolume(volume);
		}

		public static float GetVolume(int channelId)
		{
			if (_chanels.ContainsKey(channelId))
			{
				return _chanels[channelId].GetMasterVolume();
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

		private static bool IsMusicChanel(int channelId)
		{
			return _musicChanels.Contains(channelId);
		}

		public static bool IsPlaying(int channelId)
		{
			if (_chanels.ContainsKey(channelId))
			{
				return _chanels[channelId].GetIsPlaying();
			}
			return false;
		}

		public static bool CheckAudioLoaded(string clipName)
		{
			return _clips.ContainsKey(clipName);
		}
	}
}
