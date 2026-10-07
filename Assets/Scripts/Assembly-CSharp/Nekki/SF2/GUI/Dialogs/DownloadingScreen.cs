using UnityEngine;

namespace Nekki.SF2.GUI.Dialogs
{
	public class DownloadingScreen : SFMonoBehaviour<object>
	{
		[SerializeField]
		private LabelAlias _text;

		[SerializeField]
		private ProgressBar _progressBar;

		private static DownloadingScreen _Instance;

		public string TitleLabelText
		{
			get
			{
				return get_TitleText();
			}
			set
			{
				set_TitleText(value);
			}
		}

		public string TitleLabelAlias
		{
			get
			{
				return get_TitleAlias();
			}
			set
			{
				set_TitleAlias(value);
			}
		}

		public float ProgressValue
		{
			get
			{
				return get_Progress();
			}
			set
			{
				set_Progress(value);
			}
		}

		public static DownloadingScreen SharedInstance
		{
			get
			{
				return get_Instance();
			}
		}

		public string get_TitleText()
		{
			return (!(_text != null)) ? null : _text.get_text();
		}

		public void set_TitleText(string value)
		{
			if (_text != null)
			{
				_text.set_text(value);
			}
		}

		public string get_TitleAlias()
		{
			return (!(_text != null)) ? null : _text.get_Alias();
		}

		public void set_TitleAlias(string value)
		{
			if (_text != null)
			{
				_text.set_Alias(value);
			}
		}

		public float get_Progress()
		{
			return (!(_progressBar != null)) ? 0f : _progressBar.GetValue();
		}

		public void set_Progress(float value)
		{
			if (_progressBar != null)
			{
				_progressBar.SetValue(value);
			}
		}

		public static DownloadingScreen get_Instance()
		{
			if (_Instance == null)
			{
				GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("Prefabs/Dialogs/DownloadingScreen"));
				gameObject.name = "[DownloadingScreen]";
				_Instance = gameObject.GetComponent<DownloadingScreen>();
				Object.DontDestroyOnLoad(gameObject);
			}
			return _Instance;
		}

		public static void Destroy()
		{
			if (_Instance != null)
			{
				_Instance.gameObject.SetActive(false);
				Object.Destroy(_Instance.gameObject);
				_Instance = null;
			}
		}
	}
}
