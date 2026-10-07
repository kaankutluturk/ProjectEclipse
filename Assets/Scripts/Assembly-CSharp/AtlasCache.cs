using System.Collections.Generic;
using UnityEngine;

public static class AtlasCache
{
	private static Dictionary<string, Sprite[]> _CachedAtlases = new Dictionary<string, Sprite[]>();

	public static Sprite[] GetAtlasSprites(string atlasPath)
	{
		if (!_CachedAtlases.ContainsKey(atlasPath))
		{
			Sprite[] array = ResourcesAndBundles.LoadAllAssets<Sprite>(atlasPath);
			if (array != null && array.Length != 0)
			{
				_CachedAtlases.Add(atlasPath, array);
				return array;
			}
			Sprite[] array2 = Resources.LoadAll<Sprite>(atlasPath);
			if (array2 != null && array2.Length != 0)
			{
				_CachedAtlases.Add(atlasPath, array2);
				return array2;
			}
			_CachedAtlases[atlasPath] = array2 ?? new Sprite[0];
            return _CachedAtlases[atlasPath];
		}
		return _CachedAtlases[atlasPath];
	}

	public static Sprite GetSpriteFromAtlas(string atlasPath, string spriteName)
	{
        if (Eclipse.Modding.ModRuntime.TryLoadCoreSpriteReplacement(atlasPath, spriteName, out var replacement)) return replacement;
        // Selected on-screen control texture pack (FightButtons only); sized from the recovered sprite.
        if (Eclipse.UI.ControlTexturePacks.TryGetSprite(atlasPath, spriteName, RecoveredSpriteLoader, out var packed)) return packed;
		return LoadRecoveredSprite(atlasPath, spriteName);
	}

	private static readonly System.Func<string, string, Sprite> RecoveredSpriteLoader = LoadRecoveredSprite;

	private static Sprite LoadRecoveredSprite(string atlasPath, string spriteName)
	{
		if (atlasPath == Eclipse.UI.ControlTexturePacks.AtlasPath)
		{
			Sprite standalone = Resources.Load<Sprite>("ui/atlases/" + spriteName);
			if (standalone != null) return standalone;
		}
		Sprite[] array = GetAtlasSprites(atlasPath);
		if ((array == null || array.Length == 0) && !string.IsNullOrEmpty(atlasPath))
		{
			string text = atlasPath;
			int num = text.IndexOf('/');
			text = (num >= 0) ? text.Substring(num + 1) : text;
			array = GetAtlasSprites(text);
		}
		if (array == null || array.Length == 0)
		{
			return ResourcesAndBundles.Load<Sprite>("ui/atlases/" + spriteName);
		}
		Sprite[] array2 = array;
		foreach (Sprite sprite in array2)
		{
			if (sprite.name == spriteName)
			{
				return sprite;
			}
		}
		return null;
	}

	public static void Clear()
	{
		_CachedAtlases.Clear();
	}
}
