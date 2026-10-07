using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nekki.SF2.GUI
{
	[AddComponentMenu("UI/Extensions/TextPic")]
	[ExecuteInEditMode]
	public class TextPic : Text, IPointerClickHandler, IEventSystemHandler, ISelectHandler, IPointerExitHandler, IPointerEnterHandler
	{
		[Serializable]
		public struct IconName
		{
			public string name;

			public Sprite sprite;
		}

		[Serializable]
		public class HrefClickEvent : UnityEvent<string>
		{
		}

		private class HrefInfo
		{
			public int StartIndex;

			public int EndIndex;

			public string name;

			public readonly List<Rect> Boxes = new List<Rect>();
		}

		private readonly List<ResolutionImage> _iconImages = new List<ResolutionImage>();

		private readonly List<GameObject> _imagesPendingDestroy = new List<GameObject>();

		private bool _hasPendingDestroy;

		private UnityEngine.Object _destroyLock = new UnityEngine.Object();

		private readonly List<int> _quadVertexIndices = new List<int>();

		private static readonly Regex _quadRegex = new Regex("<quad name=(.+?) size=(\\d*\\.?\\d+%?) width=(\\d*\\.?\\d+%?) />", RegexOptions.Singleline);

		private string _textWithQuads;

		private string _parsedText;

		public IconName[] inspectorIconList;

		private Dictionary<string, Sprite> _iconSprites = new Dictionary<string, Sprite>();

		public float ImageScalingFactor = 0.5f;

		public string hyperlinkColor = "blue";

		[SerializeField]
		public Vector2 imageOffset = Vector2.zero;

		private Button _button;

		private List<Vector2> _quadPositions = new List<Vector2>();

		private string _lastText = string.Empty;

		public bool isCreating_m_HrefInfos = true;

		private readonly List<HrefInfo> _hrefInfos = new List<HrefInfo>();

		private static readonly StringBuilder _textBuilder = new StringBuilder();

		private static readonly Regex _hrefRegex = new Regex("<a href=([^>\\n\\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

		[SerializeField]
		private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

		public HrefClickEvent HrefClicked
		{
			get
			{
				return get_onHrefClick();
			}
			set
			{
				set_onHrefClick(value);
			}
		}

		public override void SetVerticesDirty()
		{
			base.SetVerticesDirty();
			UpdateQuadImages();
		}

		private new void Start()
		{
			_button = GetComponent<Button>();
			if (inspectorIconList != null && inspectorIconList.Length > 0)
			{
				IconName[] array = inspectorIconList;
				for (int i = 0; i < array.Length; i++)
				{
					IconName iconName = array[i];
					_iconSprites.Add(iconName.name, iconName.sprite);
				}
			}
			ResetHrefInfos();
		}

		protected void UpdateQuadImages()
		{
			_parsedText = GetParsedText();
			_quadVertexIndices.Clear();
			foreach (Match item2 in _quadRegex.Matches(_parsedText))
			{
				string prefix = _parsedText.Substring(0, item2.Index);
                prefix = _quadRegex.Replace(prefix, "\uFFFC");
                prefix = Regex.Replace(prefix, "</?(?:b|i|size|color|material)(?:=[^>]*)?>", "", RegexOptions.IgnoreCase);
                // Eclipse: since Unity 2019.1 the text generator emits no quad for whitespace
                // or line breaks. Counting them pointed past the <quad>, which then drew the
                // font atlas instead of being collapsed under the icon overlay.
                prefix = Regex.Replace(prefix, @"\s", "");
                int item = prefix.Length * 4 + 3;
				_quadVertexIndices.Add(item);
				_iconImages.RemoveAll((ResolutionImage KHPKDMGDMAB) => KHPKDMGDMAB == null);
				if (_iconImages.Count == 0)
				{
					GetComponentsInChildren(_iconImages);
				}
				if (_quadVertexIndices.Count > _iconImages.Count)
				{
					GameObject gameObject = new GameObject("ResolutionImage");
					ResolutionImage resolutionImage = gameObject.AddComponent<ResolutionImage>();
					resolutionImage.raycastTarget = false;
					gameObject.layer = base.gameObject.layer;
					gameObject.layer = base.gameObject.layer;
					RectTransform rectTransform = gameObject.transform as RectTransform;
					if ((bool)rectTransform)
					{
						rectTransform.SetParent(base.rectTransform);
						rectTransform.localPosition = Vector3.zero;
						rectTransform.localRotation = Quaternion.identity;
						rectTransform.localScale = Vector3.one;
						rectTransform.pivot = Vector2.zero;
					}
					_iconImages.Add(resolutionImage);
				}
				string value = item2.Groups[1].Value;
				float num;
                string size = item2.Groups[2].Value;
                bool percent = size.EndsWith("%");
                if (!float.TryParse(size.TrimEnd('%'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out num)) num = fontSize;
                if (percent) num = num * fontSize / 100f;
				ResolutionImage resolutionImage2 = _iconImages[_quadVertexIndices.Count - 1];
				if (resolutionImage2.get_SpriteName() != value)
				{
					resolutionImage2.set_SpriteName(value);
				}
				if (resolutionImage2.sprite == null) { resolutionImage2.enabled = false; continue; }
                resolutionImage2.rectTransform.sizeDelta = new Vector2(num, num * resolutionImage2.sprite.rect.height / resolutionImage2.sprite.rect.width);
				resolutionImage2.enabled = true;
				if (_quadPositions.Count == _iconImages.Count)
				{
					resolutionImage2.transform.SetLocalX(_quadPositions[_quadVertexIndices.Count - 1].x);
					resolutionImage2.transform.SetLocalY(_quadPositions[_quadVertexIndices.Count - 1].y - resolutionImage2.rectTransform.rect.height / 2f + (float)(base.fontSize / 4));
				}
			}
			for (int num2 = _iconImages.Count - 1; num2 >= _quadVertexIndices.Count; num2--)
			{
				if ((bool)_iconImages[num2])
				{
					_iconImages[num2].gameObject.SetActive(false);
					_iconImages[num2].gameObject.hideFlags = HideFlags.HideAndDontSave;
					_imagesPendingDestroy.Add(_iconImages[num2].gameObject);
					_iconImages.Remove(_iconImages[num2]);
				}
			}
			if (_imagesPendingDestroy.Count > 0)
			{
				_hasPendingDestroy = true;
			}
		}

		protected override void OnPopulateMesh(VertexHelper EMOHIIMOAAL)
		{
			string text = m_Text;
			m_Text = _parsedText;
			base.OnPopulateMesh(EMOHIIMOAAL);
			m_Text = text;
			_quadPositions.Clear();
			UIVertex vertex = default(UIVertex);
			for (int i = 0; i < _quadVertexIndices.Count; i++)
			{
				int num = _quadVertexIndices[i];
				RectTransform rectTransform = _iconImages[i].rectTransform;
				Vector2 sizeDelta = rectTransform.sizeDelta;
				if (num < EMOHIIMOAAL.currentVertCount)
				{
					EMOHIIMOAAL.PopulateUIVertex(ref vertex, num);
					_quadPositions.Add(vertex.position);
					EMOHIIMOAAL.PopulateUIVertex(ref vertex, num - 3);
					Vector3 position = vertex.position;
					int num2 = num;
					int num3 = num - 3;
					while (num2 > num3)
					{
						EMOHIIMOAAL.PopulateUIVertex(ref vertex, num);
						vertex.position = position;
						EMOHIIMOAAL.SetUIVertex(vertex, num2);
						num2--;
					}
				}
			}
			if (_quadVertexIndices.Count != 0)
			{
				_quadVertexIndices.Clear();
			}
			foreach (HrefInfo item in _hrefInfos)
			{
				item.Boxes.Clear();
				if (item.StartIndex >= EMOHIIMOAAL.currentVertCount)
				{
					continue;
				}
				EMOHIIMOAAL.PopulateUIVertex(ref vertex, item.StartIndex);
				Vector3 position2 = vertex.position;
				Bounds bounds = new Bounds(position2, Vector3.zero);
				int j = item.StartIndex;
				for (int fBGEOOKNPCF = item.EndIndex; j < fBGEOOKNPCF && j < EMOHIIMOAAL.currentVertCount; j++)
				{
					EMOHIIMOAAL.PopulateUIVertex(ref vertex, j);
					position2 = vertex.position;
					if (position2.x < bounds.min.x)
					{
						item.Boxes.Add(new Rect(bounds.min, bounds.size));
						bounds = new Bounds(position2, Vector3.zero);
					}
					else
					{
						bounds.Encapsulate(position2);
					}
				}
				item.Boxes.Add(new Rect(bounds.min, bounds.size));
			}
			UpdateQuadImages();
		}

		public HrefClickEvent get_onHrefClick()
		{
			return m_OnHrefClick;
		}

		public void set_onHrefClick(HrefClickEvent value)
		{
			m_OnHrefClick = value;
		}

		protected string GetParsedText()
		{
			_textBuilder.Length = 0;
			int num = 0;
			_textWithQuads = text;
			if (inspectorIconList != null && inspectorIconList.Length > 0)
			{
				IconName[] array = inspectorIconList;
				for (int i = 0; i < array.Length; i++)
				{
					IconName iconName = array[i];
					if (iconName.name != null && iconName.name != string.Empty)
					{
						_textWithQuads = _textWithQuads.Replace(iconName.name, "<quad name=" + iconName.name + " size=" + base.fontSize + " width=1 />");
					}
				}
			}
			int num2 = 0;
			foreach (Match item2 in _hrefRegex.Matches(_textWithQuads))
			{
				_textBuilder.Append(_textWithQuads.Substring(num, item2.Index - num));
				_textBuilder.Append("<color=" + hyperlinkColor + ">");
				Group obj = item2.Groups[1];
				if (isCreating_m_HrefInfos)
				{
					HrefInfo lLHOOOEJICC = new HrefInfo();
					lLHOOOEJICC.StartIndex = _textBuilder.Length * 4;
					lLHOOOEJICC.EndIndex = (_textBuilder.Length + item2.Groups[2].Length - 1) * 4 + 3;
					lLHOOOEJICC.name = obj.Value;
					HrefInfo item = lLHOOOEJICC;
					_hrefInfos.Add(item);
				}
				else if (_hrefInfos.Count > 0)
				{
					_hrefInfos[num2].StartIndex = _textBuilder.Length * 4;
					_hrefInfos[num2].EndIndex = (_textBuilder.Length + item2.Groups[2].Length - 1) * 4 + 3;
					num2++;
				}
				_textBuilder.Append(item2.Groups[2].Value);
				_textBuilder.Append("</color>");
				num = item2.Index + item2.Length;
			}
			if (isCreating_m_HrefInfos)
			{
				isCreating_m_HrefInfos = false;
			}
			_textBuilder.Append(_textWithQuads.Substring(num, _textWithQuads.Length - num));
			return _textBuilder.ToString();
		}

		public void OnPointerClick(PointerEventData BHOLFGOGPCP)
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, BHOLFGOGPCP.position, BHOLFGOGPCP.pressEventCamera, out localPoint);
			foreach (HrefInfo item in _hrefInfos)
			{
				List<Rect> eGOEJCBNDIJ = item.Boxes;
				for (int i = 0; i < eGOEJCBNDIJ.Count; i++)
				{
					if (eGOEJCBNDIJ[i].Contains(localPoint))
					{
						m_OnHrefClick.Invoke(item.name);
						return;
					}
				}
			}
		}

		public void OnPointerEnter(PointerEventData BHOLFGOGPCP)
		{
			if (_iconImages.Count < 1)
			{
				return;
			}
			foreach (ResolutionImage item in _iconImages)
			{
				if (_button != null && !_button.isActiveAndEnabled)
				{
				}
			}
		}

		public void OnPointerExit(PointerEventData BHOLFGOGPCP)
		{
			if (_iconImages.Count < 1)
			{
				return;
			}
			foreach (ResolutionImage item in _iconImages)
			{
				if (_button != null && !_button.isActiveAndEnabled)
				{
				}
			}
		}

		public void OnSelect(BaseEventData BHOLFGOGPCP)
		{
			if (_iconImages.Count < 1)
			{
				return;
			}
			foreach (ResolutionImage item in _iconImages)
			{
				if (_button != null && !_button.isActiveAndEnabled)
				{
				}
			}
		}

		private void Update()
		{
			lock (_destroyLock)
			{
				if (_hasPendingDestroy)
				{
					for (int i = 0; i < _imagesPendingDestroy.Count; i++)
					{
						UnityEngine.Object.DestroyImmediate(_imagesPendingDestroy[i]);
					}
					_imagesPendingDestroy.Clear();
					_hasPendingDestroy = false;
				}
			}
			if (_lastText != text)
			{
				ResetHrefInfos();
			}
		}

		private void ResetHrefInfos()
		{
			_lastText = text;
			_hrefInfos.Clear();
			isCreating_m_HrefInfos = true;
		}
	}
}
