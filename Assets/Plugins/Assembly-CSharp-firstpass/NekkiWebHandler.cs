using UnityEngine.Networking;

public class NekkiWebHandler : DownloadHandlerScript
{
	private const int preallocatedSize = 8192;

	private readonly NekkiUri _uri;

	private bool _isDone;

	private int _downloadedBytes;

	private int _totalBytes;

	private bool _aborted;

	public string Url
	{
		get
		{
			return GetUrl();
		}
	}

	public int TotalBytes
	{
		get
		{
			return GetTotalBytes();
		}
	}

	public int DownloadedBytes
	{
		get
		{
			return GetDownloadedBytes();
		}
	}

	public bool IsDone
	{
		get
		{
			return GetIsDone();
		}
	}

	public NekkiWebHandler(NekkiUri IACLKBNEBDM)
		: base(new byte[8192])
	{
		_uri = IACLKBNEBDM;
		_isDone = false;
		_downloadedBytes = 0;
		_totalBytes = 0;
		_aborted = false;
	}

	public string GetUrl()
	{
		return _uri.OriginalString;
	}

	public int GetTotalBytes()
	{
		return _totalBytes;
	}

	public int GetDownloadedBytes()
	{
		return _downloadedBytes;
	}

	public bool GetIsDone()
	{
		return _isDone;
	}

	public virtual void Abort()
	{
		_aborted = true;
	}

	public virtual void ForceComplete()
	{
		CompleteContent();
	}

	protected override void ReceiveContentLength(int HDIIBKGCCNB)
	{
		_totalBytes = HDIIBKGCCNB;
		OnContentLength(_totalBytes);
	}

	protected override bool ReceiveData(byte[] data, int HIGBAHGOFIJ)
	{
		if (_aborted || data == null || data.Length < 1)
		{
			return false;
		}
		OnDataReceived(data, _downloadedBytes, HIGBAHGOFIJ);
		_downloadedBytes += HIGBAHGOFIJ;
		return true;
	}

	protected override void CompleteContent()
	{
		_totalBytes = _downloadedBytes;
		_isDone = true;
		OnContentComplete();
	}

	protected override float GetProgress()
	{
		return (_downloadedBytes <= 0) ? 0f : ((float)GetTotalBytes() / (float)_downloadedBytes);
	}

	protected virtual void OnContentLength(int HDIIBKGCCNB)
	{
	}

	protected virtual void OnDataReceived(byte[] data, int IAFIGGBIKOD, int HIGBAHGOFIJ)
	{
	}

	protected virtual void OnContentComplete()
	{
	}
}
