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

	public void Download(string url, string name, string directory, Action<bool> resultCallback, Action<float> progressCallback = null, int expectedSize = 0)
	{
		_resultCallback = resultCallback;
		_progressCallback = progressCallback;
		destinationDirectory = directory;
		_name = name;
		_size = expectedSize;
		NekkiWebHelper.Download(url, string.Format("{0}/{1}", destinationDirectory, _name), OnComplete, OnError, OnProgress, null, _timeout);
	}

	private void OnComplete(NekkiWebRequest request)
	{
		if (_resultCallback != null)
		{
			_resultCallback(true);
		}
	}

	private void OnError(NekkiWebRequest request)
	{
		if (_resultCallback != null)
		{
			_resultCallback(false);
		}
	}

	private void OnProgress(NekkiWebRequest request)
	{
		if (_progressCallback != null)
		{
			if (_size > 0)
			{
				float obj = (float)request.GetDownloadedBytes() / (float)_size;
				_progressCallback(obj);
			}
			else
			{
				_progressCallback(request.GetProgress());
			}
		}
	}
}
