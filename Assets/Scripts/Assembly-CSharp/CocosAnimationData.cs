using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

public class CocosAnimationData
{
	private static readonly HashSet<string> CompatibilityEffectWarnings = new HashSet<string>();

	private static string ResolveCompatibilityEffect(string path)
	{
		string replacement = null;
		if (path.EndsWith("mgc_surge_time_effec_xml") || path.EndsWith("mgc_surge_time_effec_xml.xml"))
			replacement = "mgc_effect_time_bomb_xml";
		else if (path.EndsWith("mgc_effect_prediction_start_xml") ||
			path.EndsWith("mgc_effect_prediction_loop_xml") ||
			path.EndsWith("mgc_effect_prediction_end_xml"))
			replacement = "mgc_effect_green_aura_xml";
		if (replacement == null)
			return path;
		int separator = path.LastIndexOf('/');
		string resolved = (separator < 0 ? string.Empty : path.Substring(0, separator + 1)) + replacement;
		if (CompatibilityEffectWarnings.Add(path))
			Debug.LogWarning("[Effects] Missing newer sequence '" + path + "'; using '" + resolved + "'.");
		return resolved;
	}

	public class SpriteFrameCocos
	{
		private class FrameRect
		{
			public int X;

			public int Y;

			public int Width;

			public int Height;
		}

		private string _Name;

		private Sprite _Sprite;

		private FrameRect _frameRect;

		private Vector2 _offset;

		private bool _Rotated;

		private Vector2 _sourceSize;

		public Sprite FrameSprite
		{
			get
			{
				return GetSprite();
			}
			set
			{
				set_Sprite(value);
			}
		}

		public Vector2 FrameOffset
		{
			get
			{
				return GetOffset();
			}
		}

		public bool IsRotated
		{
			get
			{
				return GetRotated();
			}
			set
			{
				set_Rotated(value);
			}
		}

		public Vector2 SourceSize
		{
			get
			{
				return GetSourceSize();
			}
		}

		public void set_Name(string value)
		{
			_Name = value;
		}

		public string get_Name()
		{
			return _Name;
		}

		public Sprite GetSprite()
		{
			return _Sprite;
		}

		public void set_Sprite(Sprite value)
		{
			_Sprite = value;
		}

		public void SetFrame(string LIAILCGJBDK)
		{
			int num = LIAILCGJBDK.IndexOf('}');
			string[] array = LIAILCGJBDK.Substring(2, num - 2).Split(',');
			string[] array2 = LIAILCGJBDK.Substring(num + 3, LIAILCGJBDK.Length - (num + 5)).Split(',');
			_frameRect = new FrameRect();
			_frameRect.X = int.Parse(array[0]);
			_frameRect.Y = int.Parse(array[1]);
			_frameRect.Width = int.Parse(array2[0]);
			_frameRect.Height = int.Parse(array2[1]);
		}

		public Vector2 GetOffset()
		{
			return _offset;
		}

		public void SetOffset(string LIAILCGJBDK)
		{
			_offset = ParseVector(LIAILCGJBDK);
		}

		public void set_Rotated(bool value)
		{
			_Rotated = value;
		}

		public bool GetRotated()
		{
			return _Rotated;
		}

		public Vector2 GetSourceSize()
		{
			return _sourceSize;
		}

		public void SetSourceSize(string LIAILCGJBDK)
		{
			_sourceSize = ParseVector(LIAILCGJBDK);
		}

		private static Vector2 ParseVector(string value)
		{
			// An earlier recovery tool emitted an extra brace pair. Accept that
			// representation as well as TexturePacker's canonical {x,y}, without
			// letting the machine's decimal separator change the result.
			string[] parts = (value ?? string.Empty).Trim().Trim('{', '}', ' ').Split(',');
			float x, y;
			if (parts.Length != 2 ||
				!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) ||
				!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) ||
				float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y))
			{
				throw new FormatException("Invalid Cocos vector '" + value + "'.");
			}
			return new Vector2(x, y);
		}

		public void ValidateFrameSize()
		{
			if (_frameRect.Height <= 2 && _frameRect.Width <= 2)
			{
			}
		}
	}

	private List<SpriteFrameCocos> _Frames = new List<SpriteFrameCocos>();

	private int _TextureH;

	private string _Path;

	private static Dictionary<string, CocosAnimationData> _cache = new Dictionary<string, CocosAnimationData>();

	public List<SpriteFrameCocos> SpriteFrames
	{
		get
		{
			return GetFrames();
		}
	}

	private CocosAnimationData(XmlDocument GPIBAMAMGKD, string ONEIGMLOGDC)
	{
		_Path = ONEIGMLOGDC.ToLower();
		XmlNode xmlNode = GPIBAMAMGKD["plist"]["dict"];
		string text = null;
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (childNode.Name == "key")
			{
				text = childNode.FirstChild.Value;
				continue;
			}
			if (text == "frames")
			{
				ParseFrames(childNode, _Frames);
			}
			if (!(text == "metadata"))
			{
				continue;
			}
			foreach (XmlNode childNode2 in childNode.ChildNodes)
			{
				if (childNode2.Name == "key")
				{
					text = childNode2.FirstChild.Value;
				}
				else if (text == "size")
				{
					string value = childNode2.FirstChild.Value;
					string[] array = value.Substring(1, value.Length - 2).Split(',');
					_TextureH = int.Parse(array[1]);
				}
			}
		}
	}

	public List<SpriteFrameCocos> GetFrames()
	{
		return _Frames;
	}

	public string GetResourcePath()
	{
		return _Path;
	}

	public static void ClearCache()
	{
		_cache.Clear();
	}

	public static CocosAnimationData Create(string ONEIGMLOGDC, bool MPMHHEMGHOJ = false)
	{
		ONEIGMLOGDC = ResolveCompatibilityEffect(ONEIGMLOGDC);
		if (_cache.ContainsKey(ONEIGMLOGDC))
		{
			return _cache[ONEIGMLOGDC];
		}
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(ONEIGMLOGDC, string.Empty, MPMHHEMGHOJ ? XmlUtils.XmlSourceMode.ForcedResourced : XmlUtils.XmlSourceMode.Normal);
		if (xmlDocument == null)
		{
			return null;
		}
		CocosAnimationData nIHINKFPFLM = new CocosAnimationData(xmlDocument, ONEIGMLOGDC);
		_cache.Add(ONEIGMLOGDC, nIHINKFPFLM);
		return nIHINKFPFLM;
	}

	private void ParseFrames(XmlNode OPPGGBFCIJA, List<SpriteFrameCocos> GFIODDEBNHM)
	{
		string jLEKBBJBLOE = null;
		foreach (XmlNode childNode in OPPGGBFCIJA.ChildNodes)
		{
			if (childNode.Name == "key")
			{
				jLEKBBJBLOE = childNode.FirstChild.Value;
			}
			else
			{
				GFIODDEBNHM.Add(ParseFrame(childNode, jLEKBBJBLOE));
			}
		}
	}

	private SpriteFrameCocos ParseFrame(XmlNode EBBAHEDDHFO, string JLEKBBJBLOE)
	{
		SpriteFrameCocos pBAHNJDFMBO = new SpriteFrameCocos();
		pBAHNJDFMBO.set_Name(JLEKBBJBLOE.Replace(".png", string.Empty));
		string text = null;
		foreach (XmlNode childNode in EBBAHEDDHFO.ChildNodes)
		{
			if (childNode.Name == "key")
			{
				text = childNode.FirstChild.Value;
				continue;
			}
			switch (text)
			{
			case "frame":
				pBAHNJDFMBO.SetFrame(childNode.FirstChild.Value);
				break;
			case "offset":
				pBAHNJDFMBO.SetOffset(childNode.FirstChild.Value);
				break;
			case "rotated":
				pBAHNJDFMBO.set_Rotated(childNode.Name == "true");
				break;
			case "sourceSize":
				pBAHNJDFMBO.SetSourceSize(childNode.FirstChild.Value);
				break;
			}
		}
		pBAHNJDFMBO.ValidateFrameSize();
		return pBAHNJDFMBO;
	}

	public void LoadSprites()
	{
		int num = _Path.IndexOf("resources");
		string oNEIGMLOGDC = ((num != -1) ? _Path.Substring(num) : _Path).Replace("_xml", string.Empty).Replace(".xml", string.Empty);
		Sprite[] array = ResourcesAndBundles.LoadAllAssets<Sprite>(oNEIGMLOGDC);
		Dictionary<string, Sprite> dictionary = new Dictionary<string, Sprite>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				dictionary[array[i].name] = array[i];
			}
		}

		int num2 = oNEIGMLOGDC.LastIndexOf('/');
		string text = (num2 >= 0) ? oNEIGMLOGDC.Substring(0, num2 + 1) : string.Empty;
		for (int j = 0; j < _Frames.Count; j++)
		{
			Sprite value;
			if (!dictionary.TryGetValue(_Frames[j].get_Name(), out value))
			{
				// The exported project stores atlas frames as individual Sprite
				// resources (hit_1.asset, block_1.asset, etc.) rather than as
				// sub-assets of the source PNG. Load those standalone frames when
				// LoadAll cannot find a matching embedded sprite.
				value = ResourcesAndBundles.Load<Sprite>(text + _Frames[j].get_Name());
			}
			_Frames[j].set_Sprite(value);
		}
	}

	public void SortFrames()
	{
		// Atlas dictionaries are commonly emitted in lexical order, which puts
		// frame_10 before frame_2.  The decompiled sorter only recognized a
		// one-character suffix and therefore shuffled every effect over 9 frames.
		// Newer magic effects routinely contain 30-80 frames, so sort by the full
		// trailing integer while retaining a deterministic fallback for names that
		// do not end in a frame number.
		_Frames.Sort(delegate(SpriteFrameCocos left, SpriteFrameCocos right)
		{
			int leftNumber;
			int rightNumber;
			bool leftHasNumber = TryGetFrameNumber(left.get_Name(), out leftNumber);
			bool rightHasNumber = TryGetFrameNumber(right.get_Name(), out rightNumber);
			if (leftHasNumber && rightHasNumber)
			{
				int comparison = leftNumber.CompareTo(rightNumber);
				if (comparison != 0)
					return comparison;
			}
			else if (leftHasNumber != rightHasNumber)
			{
				return leftHasNumber ? -1 : 1;
			}
			return string.CompareOrdinal(left.get_Name(), right.get_Name());
		});
	}

	private static bool TryGetFrameNumber(string name, out int frameNumber)
	{
		frameNumber = 0;
		if (string.IsNullOrEmpty(name))
			return false;
		int separator = name.LastIndexOf('_');
		return separator >= 0 && separator + 1 < name.Length &&
			int.TryParse(name.Substring(separator + 1), out frameNumber);
	}
}
