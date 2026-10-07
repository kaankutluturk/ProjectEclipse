using System;

public class NekkiWebHelper
{
	public static NekkiWebRequest Download(string BEPKJNKCKPH, string path, Action<NekkiWebRequest> LMKFJLKEILL, Action<NekkiWebRequest> onError, Action<NekkiWebRequest> LFAIENNBBMK = null, object data = null, float DGDKHFPEHOG = 5f, bool GHIGJJCMEDI = true)
	{
		NekkiWebDownload iOGFNGLOCHL = new NekkiWebDownload(path, DGDKHFPEHOG);
		iOGFNGLOCHL.AddOnSuccess(LMKFJLKEILL);
		iOGFNGLOCHL.AddOnError(onError);
		iOGFNGLOCHL.AddOnProgress(LFAIENNBBMK);
		iOGFNGLOCHL.SetExternalData(data);
		iOGFNGLOCHL.Send(BEPKJNKCKPH, GHIGJJCMEDI);
		return iOGFNGLOCHL;
	}

	public static NekkiWebRequest Request(string BEPKJNKCKPH, Action<NekkiWebRequest> LMKFJLKEILL, Action<NekkiWebRequest> onError, Action<NekkiWebRequest> LFAIENNBBMK = null, object data = null, float DGDKHFPEHOG = 5f, bool GHIGJJCMEDI = true)
	{
		NekkiWebRequest aHEFDBHFHOM = new NekkiWebRequest(DGDKHFPEHOG);
		aHEFDBHFHOM.AddOnSuccess(LMKFJLKEILL);
		aHEFDBHFHOM.AddOnError(onError);
		aHEFDBHFHOM.AddOnProgress(LFAIENNBBMK);
		aHEFDBHFHOM.SetExternalData(data);
		aHEFDBHFHOM.Send(BEPKJNKCKPH, GHIGJJCMEDI);
		return aHEFDBHFHOM;
	}
}
