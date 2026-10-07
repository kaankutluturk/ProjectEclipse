using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class NekkiWebRequest
{
	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Action<NekkiWebRequest> OnSuccessful = delegate
	{
	};

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Action<NekkiWebRequest> onErrorField = delegate
	{
	};

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Action<NekkiWebRequest> OnProgress = delegate
	{
	};

	private object _externalData;

	private Coroutine _routine;

	private UnityWebRequest _request;

	private NekkiWebHandler _handler;

	private bool _aborted;

	private bool _timedOut;

	private float _lastProgressTime;

	private readonly float _timeout;

	private int _currentPosition;

	private NekkiUri _uri;

	private bool _checkCertificate;

	private bool _sendBlocked;

	public bool IsDone
	{
		get
		{
			return GetIsDone();
		}
	}

	public bool IsSuccessful
	{
		get
		{
			return GetIsSuccessful();
		}
	}

	public bool HasError
	{
		get
		{
			return GetHasError();
		}
	}

	public string Url
	{
		get
		{
			return GetUrl();
		}
	}

	public string Error
	{
		get
		{
			return GetError();
		}
	}

	public byte[] Bytes
	{
		get
		{
			return GetBytes();
		}
	}

	public string Text
	{
		get
		{
			return GetText();
		}
	}

	public float DownloadProgress
	{
		get
		{
			return GetProgress();
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

	public event Action<NekkiWebRequest> OnSuccess
	{
		add
		{
			AddOnSuccess(value);
		}
		remove
		{
			RemoveOnSuccess(value);
		}
	}

	public event Action<NekkiWebRequest> OnError
	{
		add
		{
			AddOnError(value);
		}
		remove
		{
			RemoveOnError(value);
		}
	}

	public event Action<NekkiWebRequest> OnProgressChanged
	{
		add
		{
			AddOnProgress(value);
		}
		remove
		{
			RemoveOnProgress(value);
		}
	}

	public NekkiWebRequest(float DGDKHFPEHOG = 5f)
	{
		_timeout = DGDKHFPEHOG;
		Reset();
	}

	public bool GetIsDone()
	{
		return _request != null && (_request.isDone || GetHasError());
	}

	public bool GetIsSuccessful()
	{
		return GetIsDone() && !GetHasError();
	}

	public bool GetHasError()
	{
		return _sendBlocked || _request.isNetworkError || _timedOut || (!IsSuccessStatusCode() && !IsPending());
	}

	public string GetUrl()
	{
		return _uri.OriginalString;
	}

	public string GetError()
	{
		return BuildErrorText();
	}

	public byte[] GetBytes()
	{
		return _request.downloadHandler.data;
	}

	public string GetText()
	{
		return _request.downloadHandler.text;
	}

	public float GetProgress()
	{
		return (GetTotalBytes() <= 0) ? 0f : ((float)GetDownloadedBytes() / (float)GetTotalBytes());
	}

	public int GetTotalBytes()
	{
		return (!NekkiUtils.IsEditorNotPlaying()) ? _handler.GetTotalBytes() : 0;
	}

	public int GetDownloadedBytes()
	{
		return (!NekkiUtils.IsEditorNotPlaying()) ? _handler.GetDownloadedBytes() : ((int)_request.downloadedBytes);
	}

	public void AddOnSuccess(Action<NekkiWebRequest> value)
	{
		Action<NekkiWebRequest> action = OnSuccessful;
		Action<NekkiWebRequest> action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref OnSuccessful, (Action<NekkiWebRequest>)Delegate.Combine(action2, value), action);
		}
		while ((object)action != action2);
	}

	public void RemoveOnSuccess(Action<NekkiWebRequest> value)
	{
		Action<NekkiWebRequest> action = OnSuccessful;
		Action<NekkiWebRequest> action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref OnSuccessful, (Action<NekkiWebRequest>)Delegate.Remove(action2, value), action);
		}
		while ((object)action != action2);
	}

	public void AddOnError(Action<NekkiWebRequest> value)
	{
		Action<NekkiWebRequest> action = onErrorField;
		Action<NekkiWebRequest> action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref onErrorField, (Action<NekkiWebRequest>)Delegate.Combine(action2, value), action);
		}
		while ((object)action != action2);
	}

	public void RemoveOnError(Action<NekkiWebRequest> value)
	{
		Action<NekkiWebRequest> action = onErrorField;
		Action<NekkiWebRequest> action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref onErrorField, (Action<NekkiWebRequest>)Delegate.Remove(action2, value), action);
		}
		while ((object)action != action2);
	}

	public void AddOnProgress(Action<NekkiWebRequest> value)
	{
		Action<NekkiWebRequest> action = OnProgress;
		Action<NekkiWebRequest> action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref OnProgress, (Action<NekkiWebRequest>)Delegate.Combine(action2, value), action);
		}
		while ((object)action != action2);
	}

	public void RemoveOnProgress(Action<NekkiWebRequest> value)
	{
		Action<NekkiWebRequest> action = OnProgress;
		Action<NekkiWebRequest> action2;
		do
		{
			action2 = action;
			action = Interlocked.CompareExchange(ref OnProgress, (Action<NekkiWebRequest>)Delegate.Remove(action2, value), action);
		}
		while ((object)action != action2);
	}

	public virtual void Send(string JJEOAIKCKAM, bool GHIGJJCMEDI)
	{
		_checkCertificate = GHIGJJCMEDI;
		Send(UnityWebRequest.Get(JJEOAIKCKAM));
	}

	public virtual void Send(string JJEOAIKCKAM, string MHAANIPLCJD, bool GHIGJJCMEDI)
	{
		_checkCertificate = GHIGJJCMEDI;
		Send(UnityWebRequest.PostWwwForm(JJEOAIKCKAM, MHAANIPLCJD));
	}

	public virtual void Send(string JJEOAIKCKAM, Dictionary<string, string> MHAANIPLCJD, bool GHIGJJCMEDI)
	{
		_checkCertificate = GHIGJJCMEDI;
		Send(UnityWebRequest.Post(JJEOAIKCKAM, MHAANIPLCJD));
	}

	private void Send(UnityWebRequest DLILAFJFLAI)
	{
		_uri = new NekkiUri(DLILAFJFLAI.url);
		_handler = CreateHandler(_uri);
		_lastProgressTime = Time.realtimeSinceStartup;
		_request = DLILAFJFLAI;
		if (NekkiUtils.IsEditorNotPlaying())
		{
			SendBlocking();
			return;
		}
		_request.downloadHandler = _handler;
		_routine = Routiner.Go(SendRoutine());
	}

	private IEnumerator SendRoutine()
	{
		BeginSend();
		while (!GetIsDone())
		{
			yield return new WaitForSecondsRealtime(1f / 60f);
			UpdateProgress();
		}
		FinishRequest();
	}

	private void SendBlocking()
	{
		BeginSend();
		while (!GetIsDone())
		{
			UpdateProgress();
		}
		FinishRequest();
	}

	private void BeginSend()
	{
		_sendBlocked = false;
		if (!OfflineServices.IsLocalContent(GetUrl()))
		{
			_sendBlocked = true;
			return; // Completion/error handling remains in the normal request loop.
		}
		Log("WebRequest Send " + GetUrl());
		if (_checkCertificate && !CertificateValidator.IsTrustedUrl(GetUrl()))
		{
			_sendBlocked = true;
			SendError(true);
		}
		else
		{
			_request.Send();
		}
	}

	private void UpdateProgress()
	{
		if (_currentPosition != GetDownloadedBytes())
		{
			_currentPosition = GetDownloadedBytes();
			_lastProgressTime = Time.realtimeSinceStartup;
		}
		else
		{
			_timedOut = Time.realtimeSinceStartup - _lastProgressTime >= _timeout;
		}
		SendProgress();
	}

	private void FinishRequest()
	{
		if (_handler != null && NekkiUtils.IsEditorNotPlaying())
		{
			_handler.ForceComplete();
		}
		if (GetIsSuccessful())
		{
			SendSuccess();
		}
		else
		{
			SendError();
		}
		Abort();
	}

	protected virtual void SendSuccess()
	{
		Log("WebRequest Successful " + GetUrl());
		OnSuccessful.SafeInvoke(this);
		ClearCallbacks();
	}

	protected virtual void SendError(bool BALCNGAKGKN = false)
	{
		string text = "WebRequest Error " + GetUrl() + " " + GetError();
		if (BALCNGAKGKN)
		{
			UnityEngine.Debug.LogError(text);
		}
		else
		{
			Log(text);
		}
		onErrorField.SafeInvoke(this);
		ClearCallbacks();
	}

	protected virtual void SendProgress()
	{
		OnProgress.SafeInvoke(this);
	}

	protected virtual NekkiWebHandler CreateHandler(NekkiUri KJHNCLAJMLO)
	{
		return new NekkiWebHandlerRequest(KJHNCLAJMLO);
	}

	public void Abort(bool CMBGKNNPACJ = false)
	{
		if (CMBGKNNPACJ)
		{
			SendError();
		}
		if (!_aborted)
		{
			_request.Abort();
			_handler.Abort();
			_aborted = true;
			_externalData = null;
		}
		if (_routine != null)
		{
			Routiner.Stop(_routine);
			_routine = null;
		}
		Reset();
	}

	private void Reset()
	{
		_aborted = false;
		_timedOut = false;
		_lastProgressTime = 0f;
		_currentPosition = 0;
	}

	private void ClearCallbacks()
	{
		OnSuccessful = null;
		onErrorField = null;
		OnSuccessful = null;
	}

	private void Log(string value)
	{
		UnityEngine.Debug.Log(value);
	}

	private bool IsSuccessStatusCode()
	{
		return 200 <= _request.responseCode && _request.responseCode < 300;
	}

	private bool IsPending()
	{
		return !_request.isDone;
	}

	private string BuildErrorText()
	{
		if (_sendBlocked)
		{
			if (!OfflineServices.IsLocalContent(GetUrl())) return OfflineServices.Unavailable;
			return "HTTPS certificate check error";
		}
		if (_timedOut)
		{
			return "Failed with timeout";
		}
		if (!IsSuccessStatusCode())
		{
			return "Failed with responseCode - " + _request.responseCode + " " + _request.error;
		}
		return _request.error;
	}

	public T ParseJson<T>() where T : class
	{
		try
		{
			return JsonConvert.DeserializeObject<T>(GetText());
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("NekkiWeb - Error Parse JSON (" + ex.Message + ")");
		}
		return (T)null;
	}

	public void SetExternalData(object value)
	{
		_externalData = value;
	}

	public T GetExternalData<T>()
	{
		return (T)_externalData;
	}
}
