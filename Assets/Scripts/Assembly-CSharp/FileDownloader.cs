using System;

public class FileDownloader
{
	private static FileDownloader _Instance;

	private Action<bool> _resultCallback;

	private Action<float> _progressCallback;

	private string destinationDirectory;

	private string _name;

	private float _timeout = 120f;

	private int _size;

	public static FileDownloader Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public static FileDownloader GetInstance()
	{
		if (_Instance == null)
		{
			_Instance = new FileDownloader();
		}
		return _Instance;
	}

	public void Download(string BEPKJNKCKPH, string name, string IMFMPLFADCE, Action<bool> HKHNPNNDHFP, Action<float> OODDBFJDGJO = null, int PEEOEOMEBFG = 0)
	{
		_resultCallback = HKHNPNNDHFP;
		_progressCallback = OODDBFJDGJO;
		destinationDirectory = IMFMPLFADCE;
		_name = name;
		_size = PEEOEOMEBFG;
		NekkiWebHelper.Download(BEPKJNKCKPH, string.Format("{0}/{1}", destinationDirectory, _name), OnComplete, OnError, OnProgress, null, _timeout);
	}

	private void OnComplete(NekkiWebRequest DCJLKCFKCOM)
	{
		if (_resultCallback != null)
		{
			_resultCallback(true);
		}
	}

	private void OnError(NekkiWebRequest DCJLKCFKCOM)
	{
		if (_resultCallback != null)
		{
			_resultCallback(false);
		}
	}

	private void OnProgress(NekkiWebRequest DCJLKCFKCOM)
	{
		if (_progressCallback != null)
		{
			if (_size > 0)
			{
				float obj = (float)DCJLKCFKCOM.GetDownloadedBytes() / (float)_size;
				_progressCallback(obj);
			}
			else
			{
				_progressCallback(DCJLKCFKCOM.GetProgress());
			}
		}
	}
}
