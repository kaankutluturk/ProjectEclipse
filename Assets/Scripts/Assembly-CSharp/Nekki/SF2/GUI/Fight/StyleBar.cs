using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Fight
{
	public class StyleBar : MonoBehaviour
	{
		[SerializeField]
		private ResolutionImageSkew background;

		private List<StyleBarStrip> styleBarStrips = new List<StyleBarStrip>();

		private const float defaultStripValue = 1f;

		public virtual void Init()
		{
		}

		public void Render()
		{
			for (int i = 0; i < styleBarStrips.Count; i++)
			{
				styleBarStrips[i].Render();
			}
		}

		public void AddStrip(string spriteName, string path = "", float skewAngle = 0f, string stripName = "")
		{
			if (spriteName != null)
			{
				GameObject gameObject = new GameObject();
				if (!stripName.Equals(string.Empty))
				{
					gameObject.name = stripName;
				}
				StyleBarStrip styleBarStrip = gameObject.AddComponent<StyleBarStrip>();
				styleBarStrip.set_TexturePath(path);
				styleBarStrip.set_SpriteName(spriteName);
				styleBarStrip.set_SkewAngle(skewAngle);
				styleBarStrip.SetNativeSize();
				AddStrip(styleBarStrip);
			}
		}

		public void AddStripRange(IEnumerable<StyleBarStrip> collection)
		{
			if (collection == null)
			{
				return;
			}
			foreach (StyleBarStrip item in collection)
			{
				AddStrip(item);
			}
		}

		public void AddStrip(StyleBarStrip strip)
		{
			if (!(strip == null))
			{
				strip.transform.SetParent(base.transform, false);
				strip.rectTransform.anchorMin = new Vector2(0f, 0.5f);
				strip.rectTransform.anchorMax = new Vector2(1f, 0.5f);
				strip.Init(1f);
				styleBarStrips.Add(strip);
			}
		}

		public void SetSkewAngle(float skewAngle)
		{
			SetSkewBackground(skewAngle);
			styleBarStrips.ForEach((StyleBarStrip strip) =>
			{
				strip.set_SkewAngle(skewAngle);
			});
		}

		public void SetSkewAngle(float skewAngle, int index)
		{
			if (styleBarStrips.Count > index)
			{
				styleBarStrips[index].set_SkewAngle(skewAngle);
			}
		}

		public void SetSkewBackground(float skewAngle)
		{
			if (background != null)
			{
				background.set_SkewAngle(skewAngle);
			}
		}

		public void SetValue(float value, int frames)
		{
			styleBarStrips.ForEach((StyleBarStrip strip) =>
			{
				strip.SetValue(value, frames);
			});
		}

		public void SetValue(float value, int frames, int index)
		{
			if (styleBarStrips.Count > index)
			{
				styleBarStrips[index].SetValue(value, frames);
			}
		}

		public float GetValue(int index)
		{
			if (styleBarStrips.Count > index)
			{
				return styleBarStrips[index].fillAmount;
			}
			return 0f;
		}
	}
}
