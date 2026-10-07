using SimpleJSON;

public static class JsonUtils
{
	public static int ParseInt(this JSONNode node, int defaultValue = 0)
	{
		if (node == null)
		{
			return defaultValue;
		}
		int result;
		return (!int.TryParse(node.Value, out result)) ? defaultValue : result;
	}

	public static long ParseLong(this JSONNode node, long defaultValue = 0L)
	{
		if (node == null)
		{
			return defaultValue;
		}
		long result;
		return (!long.TryParse(node.Value, out result)) ? defaultValue : result;
	}

	public static uint ParseUint(this JSONNode node, uint defaultValue = 0u)
	{
		if (node == null)
		{
			return defaultValue;
		}
		uint result;
		return (!uint.TryParse(node.Value, out result)) ? defaultValue : result;
	}

	public static float ParseFloat(this JSONNode node, float defaultValue = 0f)
	{
		if (node == null)
		{
			return defaultValue;
		}
		float result;
		return (!float.TryParse(node.Value, out result)) ? defaultValue : result;
	}

	public static bool ParseBool(this JSONNode node, bool defaultValue = false)
	{
		if (node == null)
		{
			return defaultValue;
		}
		int result;
		return (!int.TryParse(node.Value, out result)) ? defaultValue : (result > 0);
	}

	public static string ParseString(JSONNode node, string defaultValue = null)
	{
		if (node == null)
		{
			return defaultValue;
		}
		return node.Value;
	}

	public static string GetString(this JSONNode node, string defaultValue = null)
	{
		if (node == null)
		{
			return defaultValue;
		}
		return node.Value;
	}

	public static JSONNode GetNode(this JSONNode node, string key)
	{
		if (node != null && !string.IsNullOrEmpty(key))
		{
			return node[key];
		}
		return null;
	}
}
