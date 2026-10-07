using System.Collections.Generic;
using UnityEngine;

public class TexturesUtils
{
	private static readonly Dictionary<string, Sprite[]> AtlasesCache = new Dictionary<string, Sprite[]>();

	private static readonly Dictionary<string, string> AtlasesNames = new Dictionary<string, string>();

#if UNITY_6000_0_OR_NEWER
	private static readonly Dictionary<EntityId, int> textureRefCounts = new Dictionary<EntityId, int>();
#else
	private static readonly Dictionary<int, int> textureRefCounts = new Dictionary<int, int>();
#endif

	private static readonly List<Texture> texturesToDestroy = new List<Texture>();

	public static void Init()
	{
		Routiner.AddUpdate(ProcessDestroyQueue);
	}

	public static Sprite CreateSprite(Texture2D texture)
	{
		return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
	}

	public static int GetCountTexture(Texture texture)
	{
		if (texture != null)
		{
#if UNITY_6000_0_OR_NEWER
			EntityId instanceID = texture.GetEntityId();
#else
			int instanceID = texture.GetInstanceID();
#endif
			if (textureRefCounts.ContainsKey(instanceID))
			{
				return textureRefCounts[instanceID];
			}
			if (texturesToDestroy.Contains(texture))
			{
				return 0;
			}
		}
		return -1;
	}

	public static void AddTexture(Texture texture)
	{
		if (texture == null)
		{
			return;
		}
#if UNITY_6000_0_OR_NEWER
		EntityId instanceID = texture.GetEntityId();
#else
		int instanceID = texture.GetInstanceID();
#endif
		if (textureRefCounts.ContainsKey(instanceID))
		{
			textureRefCounts[instanceID]++;
		}
		else
		{
			if (texturesToDestroy.Contains(texture))
			{
				texturesToDestroy.Remove(texture);
			}
			textureRefCounts.Add(instanceID, 1);
		}
		Log("AddTexture " + texture.name + " " + GetCountTexture(texture));
	}

	public static void ReleaseTexture(Texture texture)
	{
		if (texture == null)
		{
			return;
		}
#if UNITY_6000_0_OR_NEWER
		EntityId instanceID = texture.GetEntityId();
#else
		int instanceID = texture.GetInstanceID();
#endif
		if (textureRefCounts.ContainsKey(instanceID))
		{
			if (textureRefCounts[instanceID] > 1)
			{
				textureRefCounts[instanceID]--;
			}
			else
			{
				textureRefCounts.Remove(instanceID);
				texturesToDestroy.Add(texture);
			}
			Log("ReleaseTexture " + texture.name + " " + GetCountTexture(texture));
		}
	}

	private static void ProcessDestroyQueue()
	{
		if (texturesToDestroy.Count <= 0)
		{
			return;
		}
		Texture texture = texturesToDestroy[0];
		if (texture != null)
		{
			if (AtlasesNames.ContainsKey(texture.name))
			{
				AtlasesCache.Remove(AtlasesNames[texture.name]);
				AtlasesNames.Remove(texture.name);
				Log("UnloadAtlas " + texture.name);
			}
			Log("DestroyTexture " + texture.name);
			GlobalLoad.Unload(texture);
		}
		texturesToDestroy.Remove(texture);
	}

	public static Sprite GetSpriteFromAtlas(string atlasPath, string atlasName, string spriteName)
	{
		return GetSpriteFromAtlas(LoadAtlas(atlasPath, atlasName), spriteName);
	}

	public static Sprite GetSpriteFromAtlas(string atlasPath, string spriteName)
	{
		return GetSpriteFromAtlas(LoadAtlas(atlasPath, string.Empty), spriteName);
	}

	private static Sprite GetSpriteFromAtlas(Sprite[] sprites, string spriteName)
	{
		if (sprites != null && sprites.Length > 0)
		{
			foreach (Sprite sprite in sprites)
			{
				if (sprite.name.Equals(spriteName))
				{
					return sprite;
				}
			}
		}
		Log("Sprite From Atlas Not Found  - " + spriteName);
		return GlobalLoad.GetNoImageSprite();
	}

	private static Sprite[] LoadAtlas(string path, string name = "")
	{
		if (!AtlasesCache.ContainsKey(path))
		{
			Sprite[] array = ((!name.IsNullOrEmpty()) ? GlobalLoad.GetLoadObjectsInternal<Sprite>(path, name) : GlobalLoad.GetLoadObjects<Sprite>(path));
			if (array != null && array.Length > 0)
			{
				AtlasesCache.Add(path, array);
				AtlasesNames.Add(array[0].texture.name, path);
				Log("LoadAtlas " + path);
			}
			return array;
		}
		return AtlasesCache[path];
	}

	private static void Log(string message)
	{
	}
}
