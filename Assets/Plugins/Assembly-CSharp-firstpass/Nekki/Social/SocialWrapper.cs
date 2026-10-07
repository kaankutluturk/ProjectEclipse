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

		internal static SocialWrapper Init(Callbacks EODBKOHACMO, Action<SocialNetworkType> GOLAPDHMKGC)
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
			_instance._callbacks = EODBKOHACMO;
			_onNetworkDetected = GOLAPDHMKGC;
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

		internal void OnSocialNetworkInfo(string EMBBNNBFODN)
		{
			if (!_initDone && EMBBNNBFODN.Contains("|"))
			{
				_initDone = true;
				string[] array = EMBBNNBFODN.Split('|');
				OnNetworkInitialized(array[0], array[1]);
			}
		}

		protected virtual void OnNetworkInitialized(string IDMBNOHJOAH, string AOKMNKOIMHI)
		{
			_currentUserID = AOKMNKOIMHI;
			if (IDMBNOHJOAH != null && IDMBNOHJOAH == "VK")
			{
				_callbacks.OnInitialized(SocialNetworkType.VKontakte, AOKMNKOIMHI);
				_onNetworkDetected(SocialNetworkType.VKontakte);
			}
			else
			{
				_callbacks.OnInitialized(SocialNetworkType.None, AOKMNKOIMHI);
				_onNetworkDetected(SocialNetworkType.None);
			}
		}

		internal void RequestUsersInfo(string[] JAIEEFOCDAA)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < JAIEEFOCDAA.Length; i++)
			{
				stringBuilder.Append(JAIEEFOCDAA[i]);
				if (i < JAIEEFOCDAA.Length - 1)
				{
					stringBuilder.Append(",");
				}
			}
			Application.ExternalCall("RequestUsersInfo", stringBuilder.ToString());
		}

		internal void OnUserInfo(string BBNKIBKPBLO)
		{
			UserInfo jPKEEFNNAAP = new UserInfo(BBNKIBKPBLO);
			if (jPKEEFNNAAP.GetUserId() == _currentUserID)
			{
				SetCurrentUser(jPKEEFNNAAP);
			}
			_callbacks.OnUserInfo(jPKEEFNNAAP);
		}

		internal void RequestFriends()
		{
			Application.ExternalCall("RequestFriends");
		}

		internal void OnFriends(string BBNKIBKPBLO)
		{
			_callbacks.OnFriendsInfo(UserInfo.GetInfos(BBNKIBKPBLO));
		}

		internal void OnAppFriends(string BBNKIBKPBLO)
		{
			_callbacks.OnAppFriendsInfo(UserInfo.GetInfos(BBNKIBKPBLO));
		}

		internal void Invite()
		{
			Application.ExternalCall("Invite");
		}

		internal void RequestBookmark(bool DPJFKNNHONA)
		{
			Application.ExternalCall("RequestBookmark", (!DPJFKNNHONA) ? "false" : "true");
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

		internal void RequestWallPost(string FFHABDMFMMC, string LIOGIBJBHAH, string DMNBDBJNKME)
		{
			Application.ExternalCall("RequestWallPost", FFHABDMFMMC, LIOGIBJBHAH, DMNBDBJNKME);
		}

		internal void OnWallPost(string ADFFKCBJDMP)
		{
			_callbacks.OnWallPostResult(ADFFKCBJDMP);
		}

		internal void Buy(string BGFOJFBFJIA)
		{
			Application.ExternalCall("Buy", BGFOJFBFJIA);
		}

		internal void OnBuy(string DACBPIHLFFD)
		{
			_callbacks.OnBuyResult(DACBPIHLFFD);
		}

		internal void OnExternalFocusGained()
		{
			_callbacks.OnOrderCompleted(true);
		}

		internal void OnExternalFocusLost()
		{
			_callbacks.OnOrderCompleted(false);
		}

		internal void OnResolutionChanged(string BDKHPMOHIMN)
		{
			string[] array = BDKHPMOHIMN.Split('|');
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
