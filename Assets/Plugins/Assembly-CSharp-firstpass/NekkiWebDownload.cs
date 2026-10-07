using System.IO;
using UnityEngine.Networking;

public class NekkiWebDownload : NekkiWebRequest
{
	private readonly string _targetPath;

	private readonly string _tempPath;

	public NekkiWebDownload(string path, float timeoutSeconds = 5f)
		: base(timeoutSeconds)
	{
		_targetPath = path;
		_tempPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "_download.nekki";
		FileUtils.DeleteFile(_targetPath);
		FileUtils.DeleteFile(_tempPath);
	}

	public void Send(UnityWebRequest webRequest)
	{
		if (FileUtils.FileExists(_tempPath))
		{
			FileInfo fileInfo = new FileInfo(_tempPath);
			webRequest.SetRequestHeader("range-start", fileInfo.Length.ToString());
		}
		Send(webRequest);
	}

	protected override void SendSuccess()
	{
		if (NekkiUtils.IsEditorNotPlaying())
		{
			FileUtils.WriteAllBytes(_targetPath, GetBytes());
			FileUtils.DeleteFile(_tempPath);
		}
		base.SendSuccess();
	}

	protected override void SendError(bool logAsError = false)
	{
		FileUtils.DeleteFile(_targetPath);
		FileUtils.DeleteFile(_tempPath);
		base.SendError();
	}

	protected override NekkiWebHandler CreateHandler(NekkiUri uri)
	{
		return new NekkiWebHandlerDownload(uri, _targetPath, _tempPath);
	}
}
