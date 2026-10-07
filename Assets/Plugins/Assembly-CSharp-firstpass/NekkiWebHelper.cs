using System;

public class NekkiWebHelper
{
	public static NekkiWebRequest Download(string url, string path, Action<NekkiWebRequest> onSuccess, Action<NekkiWebRequest> onError, Action<NekkiWebRequest> onProgress = null, object data = null, float timeoutSeconds = 5f, bool checkCertificate = true)
	{
		NekkiWebDownload download = new NekkiWebDownload(path, timeoutSeconds);
		download.AddOnSuccess(onSuccess);
		download.AddOnError(onError);
		download.AddOnProgress(onProgress);
		download.SetExternalData(data);
		download.Send(url, checkCertificate);
		return download;
	}

	public static NekkiWebRequest Request(string url, Action<NekkiWebRequest> onSuccess, Action<NekkiWebRequest> onError, Action<NekkiWebRequest> onProgress = null, object data = null, float timeoutSeconds = 5f, bool checkCertificate = true)
	{
		NekkiWebRequest request = new NekkiWebRequest(timeoutSeconds);
		request.AddOnSuccess(onSuccess);
		request.AddOnError(onError);
		request.AddOnProgress(onProgress);
		request.SetExternalData(data);
		request.Send(url, checkCertificate);
		return request;
	}
}
