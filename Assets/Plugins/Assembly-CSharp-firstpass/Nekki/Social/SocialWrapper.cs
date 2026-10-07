using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Nekki.Social
{
	public class SocialWrapper : MonoBehaviour
	{
		private string _currentUserID;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static UserInfo _currentUser;

		private static SocialWrapper _instance;

		private static Action<SocialNetworkType> _onNetworkDetected;

		private bool _initDone;

		private Callbacks _callbacks;

		internal static UserInfo CurrentUser
		{
			get
			{
				return GetCurrentUser();
			}
			private set
			{
				SetCurrentUser(value);
			}
		}

		public bool IsInitialized
		{
			get
			{
				return get_Initialized();
			}
		}

		internal static UserInfo GetCurrentUser()
		{
			return _currentUser;
		}

		private static void SetCurrentUser(UserInfo value)
		{
			_currentUser = value;
		}

		internal static SocialWrapper Init(Callbacks callbacks, Action<SocialNetworkType> onNetworkDetected)
		{
			GameObject gameObject = GameObject.Find("_social");
			if (!gameObject)
			{
				gameObject = new GameObject("_social");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			SocialWrapper component = gameObject.GetComponent<SocialWrapper>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			_instance = gameObject.AddComponent<SocialWrapper>();
			_instance._callbacks = callbacks;
			_onNetworkDetected = onNetworkDetected;
			return _instance;
		}

		public bool get_Initialized()
		{
			return _initDone;
		}

		protected virtual void Start()
		{
			UnityEngine.Debug.Log("SocialWrapper Start");
			RequestSocialNetworkInfo();
		}

		private void RequestSocialNetworkInfo()
		{
			Application.ExternalCall("RequestSocialNetworkInfo");
		}

		internal void OnSocialNetworkInfo(string networkInfo)
		{
			if (!_initDone && networkInfo.Contains("|"))
			{
				_initDone = true;
				string[] array = networkInfo.Split('|');
				OnNetworkInitialized(array[0], array[1]);
			}
		}

		protected virtual void OnNetworkInitialized(string networkName, string userId)
		{
			_currentUserID = userId;
			if (networkName != null && networkName == "VK")
			{
				_callbacks.OnInitialized(SocialNetworkType.VKontakte, userId);
				_onNetworkDetected(SocialNetworkType.VKontakte);
			}
			else
			{
				_callbacks.OnInitialized(SocialNetworkType.None, userId);
				_onNetworkDetected(SocialNetworkType.None);
			}
		}

		internal void RequestUsersInfo(string[] userIds)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < userIds.Length; i++)
			{
				stringBuilder.Append(userIds[i]);
				if (i < userIds.Length - 1)
				{
					stringBuilder.Append(",");
				}
			}
			Application.ExternalCall("RequestUsersInfo", stringBuilder.ToString());
		}

		internal void OnUserInfo(string userJson)
		{
			UserInfo userInfo = new UserInfo(userJson);
			if (userInfo.GetUserId() == _currentUserID)
			{
				SetCurrentUser(userInfo);
			}
			_callbacks.OnUserInfo(userInfo);
		}

		internal void RequestFriends()
		{
			Application.ExternalCall("RequestFriends");
		}

		internal void OnFriends(string usersJson)
		{
			_callbacks.OnFriendsInfo(UserInfo.GetInfos(usersJson));
		}

		internal void OnAppFriends(string usersJson)
		{
			_callbacks.OnAppFriendsInfo(UserInfo.GetInfos(usersJson));
		}

		internal void Invite()
		{
			Application.ExternalCall("Invite");
		}

		internal void RequestBookmark(bool bookmark)
		{
			Application.ExternalCall("RequestBookmark", (!bookmark) ? "false" : "true");
		}

		internal void OnBookmarkState(string state)
		{
			_callbacks.OnBookmarkState(state.Equals("true"));
		}

		internal void RequestCheckGroupMembership()
		{
			Application.ExternalCall("RequestCheckGroupMembership");
		}

		internal void OnGroupMembership(string state)
		{
			_callbacks.OnGroupMembership(state.Equals("true"));
		}

		internal void RequestWallPost(string ownerId, string message, string attachments)
		{
			Application.ExternalCall("RequestWallPost", ownerId, message, attachments);
		}

		internal void OnWallPost(string result)
		{
			_callbacks.OnWallPostResult(result);
		}

		internal void Buy(string itemId)
		{
			Application.ExternalCall("Buy", itemId);
		}

		internal void OnBuy(string result)
		{
			_callbacks.OnBuyResult(result);
		}

		internal void OnExternalFocusGained()
		{
			_callbacks.OnOrderCompleted(true);
		}

		internal void OnExternalFocusLost()
		{
			_callbacks.OnOrderCompleted(false);
		}

		internal void OnResolutionChanged(string resolution)
		{
			string[] array = resolution.Split('|');
			int num = int.Parse(array[0]);
			int num2 = int.Parse(array[1]);
			Screen.SetResolution(num, num2, false);
			AdvLog.Log(string.Format("Resolution changed to ({0}x{1})", num, num2));
		}

		private void OnDestroy()
		{
			Delete();
		}

		public static void Delete()
		{
			SetCurrentUser(null);
			_instance = null;
		}
	}
}
