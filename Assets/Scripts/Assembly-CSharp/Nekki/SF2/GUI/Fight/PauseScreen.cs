using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Fight
{
	public class PauseScreen : MonoBehaviour
	{
		public class PauseEvent : UnityEvent
		{
		}

		[SerializeField]
		private PressButton musicOn;

		[SerializeField]
		private PressButton musicOff;

		[SerializeField]
		private PressButton soundOn;

		[SerializeField]
		private PressButton soundOff;

		public LabelAlias RulesLabel;

		public PauseEvent OnPlay = new PauseEvent();

		public PauseEvent OnSurrender = new PauseEvent();

		public void Init()
		{
			if (musicOn != null)
			{
				musicOn.gameObject.SetActive(!SoundController.GetMusicMuted());
			}
			if (musicOff != null)
			{
				musicOff.gameObject.SetActive(SoundController.GetMusicMuted());
			}
			if (soundOn != null)
			{
				soundOn.gameObject.SetActive(!SoundController.GetSoundMuted());
			}
			if (soundOff != null)
			{
				soundOff.gameObject.SetActive(SoundController.GetSoundMuted());
			}
			Sound.PauseAllSounds();
			UpdateRulesLabel();
			RebuildButtonsLayout();
		}

		private void RebuildButtonsLayout()
		{
			HorizontalLayoutGroup horizontalLayoutGroup = GetComponentInChildren<HorizontalLayoutGroup>(true);
			if (horizontalLayoutGroup != null)
			{
				Canvas.ForceUpdateCanvases();
				LayoutRebuilder.ForceRebuildLayoutImmediate(horizontalLayoutGroup.transform as RectTransform);
			}
		}

		private void UpdateRulesLabel()
		{
			string text = global::Fight.GetCurrentFight().GetFightDefinition().GetDescription();
			bool flag = !string.IsNullOrEmpty(text);
			RulesLabel.gameObject.SetActive(flag);
			if (flag)
			{
				RulesLabel.SetAlias(text);
			}
		}

		public void OnHomeClick()
		{
			Sound.ResumeAllSounds();
			OnSurrender.Invoke();
		}

		public void OnPlayClick()
		{
			Sound.ResumeAllSounds();
			OnPlay.Invoke();
		}

		public void OnMusicOnClick()
		{
			if (musicOn != null)
			{
				musicOn.gameObject.SetActive(false);
			}
			if (musicOff != null)
			{
				musicOff.gameObject.SetActive(true);
			}
			SoundController.SetMusicMuted(true);
			RebuildButtonsLayout();
		}

		public void OnMusicOffClick()
		{
			if (musicOn != null)
			{
				musicOn.gameObject.SetActive(true);
			}
			if (musicOff != null)
			{
				musicOff.gameObject.SetActive(false);
			}
			SoundController.SetMusicMuted(false);
			RebuildButtonsLayout();
		}

		public void OnSoundOnClick()
		{
			if (soundOn != null)
			{
				soundOn.gameObject.SetActive(false);
			}
			if (soundOff != null)
			{
				soundOff.gameObject.SetActive(true);
			}
			SoundController.SetSoundMuted(true);
			RebuildButtonsLayout();
		}

		public void OnSoundOffClick()
		{
			if (soundOn != null)
			{
				soundOn.gameObject.SetActive(true);
			}
			if (soundOff != null)
			{
				soundOff.gameObject.SetActive(false);
			}
			SoundController.SetSoundMuted(false);
			RebuildButtonsLayout();
		}
	}
}
