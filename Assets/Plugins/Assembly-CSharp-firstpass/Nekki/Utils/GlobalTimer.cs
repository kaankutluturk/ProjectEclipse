using System;
using UnityEngine;

namespace Nekki.Utils
{
	public class GlobalTimer : ExtentionBehaviour
	{
		public const int TICK = 0;

		private static GlobalTimer _instance;

		private static TimeSpan _serverTimeOffset;

		private static float _syncUnscaledTime;

		private static DateTime _serverTime;

		private static bool _isSynchronized;

		private static Action _onSyncSuccess;

		private static Action _onSyncError;

		private static bool _isRequestInProgress;

		private static bool _isLastRequestSuccessful;

		private static bool _skipServerSync;

		private float _lastTickTime;

		public static GlobalTimer SharedInstance
		{
			get
			{
				return get_Instance();
			}
		}

		public static DateTime LocalNow
		{
			get
			{
				return get_LocalizedNow();
			}
		}

		public static DateTime ServerNow
		{
			get
			{
				return get_Now();
			}
		}

		public static long ServerTimestamp
		{
			get
			{
				return get_GetTime();
			}
		}

		public static long LocalUtcTimestamp
		{
			get
			{
				return get_LocalTimeUTC();
			}
		}

		public static bool Synchronized
		{
			get
			{
				return get_IsSynchronized();
			}
		}

		public static bool IsRequestPending
		{
			get
			{
				return get_IsRequestInProgress();
			}
		}

		public static bool LastRequestSucceeded
		{
			get
			{
				return get_IsLastRequestSuccessful();
			}
		}

		public static GlobalTimer get_Instance()
		{
			if (!_instance)
			{
				Init();
			}
			return _instance;
		}

		public static DateTime get_LocalizedNow()
		{
			return get_Now().ToLocalTime();
		}

		public static DateTime get_Now()
		{
			return _serverTime.AddSeconds(Time.unscaledTime - _syncUnscaledTime);
		}

		public static long get_GetTime()
		{
			return ConvertToUnixTimestamp(get_Now());
		}

		public static long get_LocalTimeUTC()
		{
			return (long)Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
		}

		public static bool get_IsSynchronized()
		{
			return _isSynchronized;
		}

		public static bool get_IsRequestInProgress()
		{
			return _isRequestInProgress;
		}

		public static bool get_IsLastRequestSuccessful()
		{
			return _isLastRequestSuccessful;
		}

		public static long ConvertToUnixTimestamp(DateTime date)
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return (long)Math.Floor((date.ToUniversalTime() - dateTime).TotalSeconds);
		}

		public static void Init(bool skipServerSync = false)
		{
			if (!_instance)
			{
				_instance = new GameObject("_timer").AddComponent<GlobalTimer>();
				UnityEngine.Object.DontDestroyOnLoad(_instance.get_gameObject());
			}
			_serverTimeOffset = default(TimeSpan);
			_isSynchronized = false;
			_isLastRequestSuccessful = false;
			_skipServerSync = skipServerSync;
			if (!_skipServerSync)
			{
				ServerTimeSync();
			}
		}

		public static void ServerTimeSync(Action onSuccess = null, Action onError = null)
		{
			// Local clock only; keep timer callbacks and elapsed-time gameplay working.
			_isRequestInProgress = true;
			_onSyncSuccess = onSuccess;
			_onSyncError = onError;
			OnServerTimeReceived(ConvertToUnixTimestamp(DateTime.UtcNow));
		}

		public static void ServerTimeExtended(long milliseconds)
		{
			DateTime serverTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
			serverTime += timeSpan;
			_serverTime = serverTime;
			_syncUnscaledTime = Time.unscaledTime;
			_serverTimeOffset = _serverTime - DateTime.Now;
			_isSynchronized = true;
			_isRequestInProgress = false;
			_isLastRequestSuccessful = true;
			if (_onSyncSuccess != null)
			{
				_onSyncSuccess();
				_onSyncSuccess = null;
				_onSyncError = null;
			}
		}

		private static void OnServerTimeReceived(long time)
		{
			_serverTime = UnixTimeStampToDateTime(time);
			_syncUnscaledTime = Time.unscaledTime;
			_serverTimeOffset = _serverTime - DateTime.Now;
			_isSynchronized = true;
			_isRequestInProgress = false;
			_isLastRequestSuccessful = true;
			if (_onSyncSuccess != null)
			{
				_onSyncSuccess();
				_onSyncSuccess = null;
				_onSyncError = null;
			}
		}

		private static void OnServerTimeError(object error)
		{
			_serverTime = DateTime.Now;
			_syncUnscaledTime = Time.unscaledTime;
			_serverTimeOffset = _serverTime - DateTime.Now;
			_isSynchronized = false;
			_isRequestInProgress = false;
			_isLastRequestSuccessful = false;
			if (_onSyncError != null)
			{
				_onSyncError();
				_onSyncSuccess = null;
				_onSyncError = null;
			}
			AdvLog.LogError(error);
		}

		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp);
		}

		public static DateTime UnixTimeStampToDateTimeLocal(double unixTimeStamp)
		{
			return UnixTimeStampToDateTime(unixTimeStamp).ToLocalTime();
		}

		private void Update()
		{
			if (_lastTickTime + 1f < Time.unscaledTime)
			{
				_lastTickTime = Time.unscaledTime;
				// Invoke on the live component. During scene/application teardown the
				// static singleton is cleared before Unity delivers the final Update.
				callEvent(0, get_Now());
			}
		}

		private void OnApplicationPause(bool paused)
		{
			if (!paused && !_skipServerSync)
			{
				ServerTimeSync();
			}
		}

		private new void OnDestroy()
		{
			_instance = null;
			StopAllCoroutines();
			base.OnDestroy();
		}
	}
}
