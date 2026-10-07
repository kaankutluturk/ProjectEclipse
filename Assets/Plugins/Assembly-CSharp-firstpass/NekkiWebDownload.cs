using System.IO;
using UnityEngine.Networking;

public class NekkiWebDownload : NekkiWebRequest
{
	private readonly string _targetPath;

	private readonly string _tempPath;

	public NekkiWebDownload(string path, float DGDKHFPEHOG = 5f)
		: base(DGDKHFPEHOG)
	{
		_targetPath = path;
		_tempPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "_download.nekki";
		FileUtils.DeleteFile(_targetPath);
		FileUtils.DeleteFile(_tempPath);
	}

	public void Send(UnityWebRequest DLILAFJFLAI)
	{
		if (FileUtils.FileExists(_tempPath))
		{
			FileInfo fileInfo = new FileInfo(_tempPath);
			DLILAFJFLAI.SetRequestHeader("range-start", fileInfo.Length.ToString());
		}
		Send(DLILAFJFLAI);
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

	protected override void SendError(bool BALCNGAKGKN = false)
	{
		FileUtils.DeleteFile(_targetPath);
		FileUtils.DeleteFile(_tempPath);
		base.SendError();
	}

	protected override NekkiWebHandler CreateHandler(NekkiUri KJHNCLAJMLO)
	{
		return new NekkiWebHandlerDownload(KJHNCLAJMLO, _targetPath, _tempPath);
	}
}
