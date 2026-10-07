using System.Collections.Generic;
using UnityEngine;

public static class LocationSpriteCache
{
	private static Dictionary<string, Sprite[]> _CachedAtlases = new Dictionary<string, Sprite[]>();

	private static Dictionary<string, Sprite> _CachedSingleSprite = new Dictionary<string, Sprite>();

	public static Sprite[] LoadAtlasSprites(string atlasPath)
	{
		if (!_CachedAtlases.ContainsKey(atlasPath))
		{
			Sprite[] array = ResourcesAndBundles.LoadAllAssets<Sprite>(atlasPath);
			if (array != null)
			{
				_CachedAtlases.Add(atlasPath, array);
			}
			return array;
		}
		return _CachedAtlases[atlasPath];
	}

	private static Sprite LoadSingleSprite(string texturePath, string spriteName)
	{
		string text = string.Format("{0}/{1}", texturePath, spriteName);
		if (_CachedSingleSprite.ContainsKey(text))
		{
			return _CachedSingleSprite[text];
		}
		Sprite sprite;
		if (!Eclipse.Modding.ModAssetBinding.TryLoadSprite(text, out sprite))
		{
			sprite = ResourcesAndBundles.Load<Sprite>(text);
		}
		_CachedSingleSprite[text] = sprite;
		return sprite;
	}

	public static Sprite GetSprite(string texturePath, string spriteName, string atlasName)
	{
		if (Eclipse.Modding.AssetId.TryParse(texturePath, out _))
		{
			// Qualified core/mod location art is addressed per sprite. Legacy atlas sub-assets
			// remain available for installed/core locations through the branch below.
			return LoadSingleSprite(texturePath, spriteName);
		}
        if (Eclipse.Modding.ModRuntime.TryResolveCoreReplacement(texturePath + "/" + spriteName, out var replacement))
            return Eclipse.Modding.ModRuntime.Host.TypedAssets.LoadSprite(replacement);
		if (!string.IsNullOrEmpty(atlasName))
		{
			string atlasSpritePath = string.Format("{0}/{1}", texturePath, atlasName);
			Sprite[] array = LoadAtlasSprites(atlasSpritePath);
			Sprite[] array2 = array ?? new Sprite[0];
			foreach (Sprite sprite in array2)
			{
				if (sprite.name == spriteName)
				{
					return sprite;
				}
			}
		}
		return LoadSingleSprite(texturePath, spriteName);
	}

	public static void Clear()
	{
		_CachedAtlases.Clear();
		_CachedSingleSprite.Clear();
		CocosAnimationData.ClearCache();
	}
}
