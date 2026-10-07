using System.IO;
using UnityEngine;

public class NekkiWebHandlerDownload : NekkiWebHandler
{
	private readonly string _targetPath;

	private readonly string _tempPath;

	private readonly FileMode _fileMode;

	private readonly FileStream _file;

	public NekkiWebHandlerDownload(NekkiUri uri, string targetPath, string tempPath)
		: base(uri)
	{
		_targetPath = targetPath;
		_tempPath = tempPath;
		_file = FileUtils.OpenAppendStream(_tempPath);
	}

	protected override void OnDataReceived(byte[] data, int offset, int length)
	{
		_file.Write(data, 0, length);
	}

	public override void Abort()
	{
		CloseFile();
		base.Abort();
	}

	protected override void OnContentComplete()
	{
		CloseFile();
		if (FileUtils.FileExists(_targetPath) && FileUtils.IsFileLocked(_targetPath))
		{
			Debug.LogError(string.Format("This file is locked: {0} User may have this file open (as a folder or 7z), or there is a bug in the code still holding an open file handler", _targetPath));
			return;
		}
		FileUtils.DeleteFile(_targetPath);
		FileUtils.MoveFile(_tempPath, _targetPath);
	}

	private void CloseFile()
	{
		_file.Close();
	}
}
