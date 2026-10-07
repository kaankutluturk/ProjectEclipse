using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nekki.Social
{
	public class Avatars : MonoBehaviour
	{
		private class AvatarRequest
		{
			public UserInfo Info;

			public Action<string, Texture> OnDone;
		}

		private static Avatars _instance;

		private static readonly Dictionary<string, Texture> _avatars = new Dictionary<string, Texture>();

		private static readonly Dictionary<string, AvatarRequest> _requests = new Dictionary<string, AvatarRequest>();

		private static string _current;

		private bool _inProcess;

		private static void Init()
		{
			if (!_instance)
			{
				_instance = new GameObject("_avatar").AddComponent<Avatars>();
				UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
			}
		}

		public static void GetAvatar(UserInfo EMBBNNBFODN, Action<string, Texture> onDone)
		{
			Init();
			if (_avatars.ContainsKey(EMBBNNBFODN.GetUserId()))
			{
				onDone(EMBBNNBFODN.GetUserId(), _avatars[EMBBNNBFODN.GetUserId()]);
				return;
			}
			_requests.Add(EMBBNNBFODN.GetUserId(), new AvatarRequest
			{
				Info = EMBBNNBFODN,
				OnDone = onDone
			});
		}

		private void Update()
		{
			if (!_inProcess && _requests.Count > 0)
			{
				StartCoroutine(Load());
			}
		}

		private IEnumerator Load()
		{
			_requests.Clear();
			_inProcess = false;
			yield break;
		}
	}
}
