using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Fight
{
	public class EndFightScreen : MonoBehaviour, BackKeyController
	{
		[SerializeField]
		private GameObject _endFightContentPrefab;

		[SerializeField]
		private Transform _content;

		[SerializeField]
		private ResolutionImage _resultHeader;

		[SerializeField]
		private Button _animationFinishButton;

		[SerializeField]
		private GameObject _contentLayout;

		private EndFightContent _endFightContent;

		private FightResult fightResult;

		private string winSpriteName = "FightUI.Label_Win";

		private string loseSpriteName = "FightUI.Label_Lose";

		private string timesUpSpriteName = "FightUI.Label_Timesup";

		public void Init(FightResult result)
		{
			// The recovered shadow is a fixed-width strip scaled 22x. Fill the
			// actual viewport so wide windows do not leave a bright uncovered edge.
			var background = transform.Find("Background") as RectTransform;
			if (background != null)
			{
				background.localScale = Vector3.one;
				background.anchorMin = Vector2.zero;
				background.anchorMax = Vector2.one;
				background.offsetMin = background.offsetMax = Vector2.zero;
			}
			EnableFinishButton(false);
			fightResult = result;
			if (_resultHeader != null)
			{
				bool flag = fightResult.IsWinner();
				bool flag2 = fightResult.IsRaidRoundTimeout();
				_resultHeader.set_SpriteName(flag ? winSpriteName : ((!flag2) ? loseSpriteName : timesUpSpriteName));
				_resultHeader.SetNativeSize();
				Eclipse.UI.UiReveal.Play(_resultHeader.rectTransform, 0f, .45f, new Vector2(0f, 45f), 1.18f);
			}
			if (_endFightContentPrefab != null)
			{
				_endFightContent = Object.Instantiate(_endFightContentPrefab).GetComponent<EndFightContent>();
				_endFightContent.gameObject.SetActive(true);
				Transform parent = ((!(_content != null)) ? base.transform : _content);
				_endFightContent.transform.SetParent(parent, false);
				_endFightContent.Init(fightResult, _contentLayout.GetComponent<VerticalLayoutGroup>(), _animationFinishButton);
				_endFightContent.CloseEvent.AddListener(() =>
				{
					GameUtils.HandleSurrender(fightResult);
				});
				_endFightContent.AnimationEndEvent.AddListener(OnAnimationEnded);
				Eclipse.UI.UiReveal.Play(_endFightContent.transform as RectTransform, .12f, .45f, Vector2.zero, .96f);
			}
		}

		private void EnableFinishButton(bool enabled)
		{
			if (_animationFinishButton != null)
			{
				_animationFinishButton.gameObject.SetActive(enabled);
			}
		}

		private void OnAnimationEnded()
		{
			EnableFinishButton(false);
		}

		public void OnAnimationFinishButton()
		{
			if (_endFightContent != null)
			{
				_endFightContent.FinishAnimation();
			}
			EnableFinishButton(false);
		}

		private void Awake()
		{
			BackKeyManager.get_Instance().AddBackKeyController(this);
		}

		private void OnDestroy()
		{
			BackKeyManager.get_Instance().RemoveBackKeyController(this);
		}

		public void OnBackKeyClicked(object sender)
		{
			GameUtils.HandleSurrender(fightResult);
		}
	}
}
