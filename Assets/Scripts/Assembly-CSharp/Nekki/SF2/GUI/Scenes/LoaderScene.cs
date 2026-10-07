using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Scenes
{
	public class LoaderScene : Scene<LoaderScene>
	{
		private static ScreenType previousScene = ScreenType.ModuleNone;

		private static ScreenType nextScene = ScreenType.ModuleNone;

		[SerializeField]
		private GameObject _LoaderType1;

		[SerializeField]
		private GameObject _LoaderType2;

		private Image loadingImage;

		private Text loadingText;

		private bool isPictureChanged;

		public static ScreenType PreviousScene
		{
			get
			{
				return get_PrevScene();
			}
			set
			{
				set_PrevScene(value);
			}
		}

		public static ScreenType NextSceneId
		{
			set
			{
				set_NextScene(value);
			}
		}

		public override ScreenType SceneType
		{
			get
			{
				return get_SceneId();
			}
		}

		public static ScreenType get_PrevScene()
		{
			return previousScene;
		}

		public static void set_PrevScene(ScreenType value)
		{
			previousScene = value;
		}

		public static void set_NextScene(ScreenType value)
		{
			nextScene = value;
		}

		public override ScreenType get_SceneId()
		{
			return ScreenType.Loader;
		}

		protected override void Init(object data)
		{
			base.Init(data);
			if (nextScene != ScreenType.ModulePreloader)
			{
			}
			AtlasCache.Clear();
			// The Single-mode LoadSceneAsync below already unloads unused assets.
			// An extra sweep here scans the same objects twice, followed by a forced
			// full collection, producing long stalls even when nothing is reclaimed.
			if (nextScene == ScreenType.ModuleFight && Eclipse.Multiplayer.LocalVersusMenu.VersusSplashVisible)
			{
				// Multiplayer's persistent VS introduction owns this transition.
				_LoaderType1.SetActive(false);
				_LoaderType2.SetActive(false);
			}
			else if (get_PrevScene() == ScreenType.ModulePreloader)
			{
				_LoaderType1.SetActive(true);
				_LoaderType2.SetActive(false);
			}
			else
			{
				_LoaderType1.SetActive(false);
				_LoaderType2.SetActive(true);
				Eclipse.UI.LoaderArt.ApplyMenuSplash(_LoaderType2, get_PrevScene(), nextScene);
			}
			StartCoroutine(LoadNextSceneAsync());
		}

		private IEnumerator LoadNextSceneAsync()
		{
			yield return SceneManager.LoadSceneAsync((int)nextScene);
		}

		public override void UpdateScene(object data)
		{
		}

		public void UpdateLocalization()
		{
			UpdateLoadingText();
			UpdateLoadingPicture();
		}

		public void ChangePicture()
		{
			isPictureChanged = true;
		}

		public void ScalePicture()
		{
			float scaleY = SystemProperties.ScaleY;
		}

		public void AddLogo()
		{
		}

		public void RemoveLogo()
		{
		}

		public void AddLoadingPic()
		{
			if (loadingImage == null)
			{
				float scaleY = SystemProperties.ScaleY;
			}
		}

		public void RemoveLoadingPic()
		{
		}

		private void UpdateLoadingPicture()
		{
		}

		protected virtual void Draw()
		{
		}

		private void UpdateLoadingText()
		{
		}
	}
}
