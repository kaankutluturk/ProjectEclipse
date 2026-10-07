using UnityEngine;

public static class ResourcesAndBundles
{
	public static T Load<T>(string assetPath) where T : Object
	{
		T modAsset;
			if (Eclipse.Modding.ModRuntime.TryLoadQualified(assetPath, out modAsset))
			{
				return modAsset;
			}
			if (Eclipse.Modding.ModRuntime.TryLoadCore(assetPath, out modAsset))
			{
				return modAsset;
			}

		if (IsLooseLocationAsset(assetPath))
		{
			return Resources.Load<T>(assetPath);
		}

			// Core assets are owned by the reserved core namespace. The provider currently
			// reads TAR/LZ4 through PackagedArtCatalog; callers do not depend on that storage.
			return Resources.Load<T>(assetPath);
	}

	public static T[] LoadAllAssets<T>(string assetPath) where T : Object
	{
		T[] modAssets;
			if (Eclipse.Modding.ModRuntime.TryLoadQualifiedWithSubAssets(assetPath, out modAssets))
			{
				return modAssets;
			}
			if (Eclipse.Modding.ModRuntime.TryLoadCoreWithSubAssets(assetPath, out modAssets))
			{
				return modAssets;
			}

		if (IsLooseLocationAsset(assetPath))
		{
			return Resources.LoadAll<T>(assetPath);
		}

			return Resources.LoadAll<T>(assetPath);
	}

	private static bool IsLooseLocationAsset(string resourcePath)
	{
		if (string.IsNullOrEmpty(resourcePath))
		{
			return false;
		}

		string path = resourcePath.Replace((char)92, '/').TrimStart('/');
		// Location art is packaged like every other runtime visual. Location params stay
		// loose because stage layout/collision/music is part of the moddable config layer.
		return path.StartsWith("gamedata/locations/", System.StringComparison.OrdinalIgnoreCase);
	}
}
