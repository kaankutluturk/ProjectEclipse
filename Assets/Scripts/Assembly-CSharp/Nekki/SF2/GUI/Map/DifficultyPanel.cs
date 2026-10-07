using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class DifficultyPanel : SFMonoBehaviour<object>
	{
		[SerializeField]
		private ProgressBar _difficultyBar;

		[SerializeField]
		private LabelAlias _difficultyLabel;

		private static List<global::Pair<string, float>> difficultyEvaluation = new List<global::Pair<string, float>>();

		public static List<global::Pair<string, float>> DifficultyLevels
		{
			get
			{
				return get_DifficultyEvaluation();
			}
		}

		public static List<global::Pair<string, float>> get_DifficultyEvaluation()
		{
			return difficultyEvaluation;
		}

		public void Init(float ratio)
		{
			_difficultyBar.SetValueBorders(0f, 100f);
			int num = 0;
			int num2 = 0;
			global::Pair<string, float> bestRating = difficultyEvaluation[0];
			foreach (global::Pair<string, float> item in difficultyEvaluation)
			{
				if (item.Second < ratio && bestRating.Second < item.Second)
				{
					bestRating = item;
					num2 = num;
				}
				num++;
			}
			_difficultyBar.Stripe.set_SpriteName(Constants.DifficultyBarSprites[num2]);
			RestoreTrimmedStripeLayout();
			_difficultyBar.SetValue(100f);
			_difficultyLabel.SetAlias(bestRating.First);
		}

		private void RestoreTrimmedStripeLayout()
		{
			// Inactive map content may be initialized before its images awaken.
			_difficultyBar.Background.set_SpriteName("DifficultyBars.empty");
			Sprite backgroundSprite = _difficultyBar.Background.sprite;
			Sprite stripeSprite = _difficultyBar.Stripe.sprite;
			if (backgroundSprite == null || stripeSprite == null)
			{
				return;
			}

			// The exported difficulty levels are trimmed atlas sprites. Stretching
			// them over the background turns every level into a full-width bar and
			// distorts its arrow caps. Restore their size relative to the untrimmed
			// 501x40 background and pin the colored portion to the left edge.
			float num3 = Mathf.Clamp01(stripeSprite.rect.width / backgroundSprite.rect.width);
			float num4 = Mathf.Clamp01(stripeSprite.rect.height / backgroundSprite.rect.height);
			RectTransform rectTransform = _difficultyBar.Stripe.rectTransform;
			rectTransform.anchorMin = new Vector2(0f, 0.5f - num4 * 0.5f);
			rectTransform.anchorMax = new Vector2(num3, 0.5f + num4 * 0.5f);
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			_difficultyBar.Stripe.type = Image.Type.Simple;
		}

		public static void DifficultyEvaluationParse(XmlNode node)
		{
			difficultyEvaluation.Clear();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				difficultyEvaluation.Add(new global::Pair<string, float>(childNode.Attributes["Name"].GetStringOrDefault(string.Empty), childNode.Attributes["RatingRatioTreshold"].ParseFloat()));
			}
		}
	}
}
