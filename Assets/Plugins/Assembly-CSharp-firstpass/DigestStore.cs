using System;
using System.Collections.Generic;

internal static class DigestStore
{
	private static Dictionary<string, Digest> Digests = new Dictionary<string, Digest>();

	private static object Locker = new object();

	private static string[] SupportedAlgorithms = new string[2] { "digest", "basic" };

	public static Digest Get(Uri requestUri)
	{
		lock (Locker)
		{
			Digest value = null;
			if (Digests.TryGetValue(requestUri.Host, out value) && !value.IsUriProtected(requestUri))
			{
				return null;
			}
			return value;
		}
	}

	public static Digest GetOrCreate(Uri requestUri)
	{
		lock (Locker)
		{
			Digest value = null;
			if (!Digests.TryGetValue(requestUri.Host, out value))
			{
				Digests.Add(requestUri.Host, value = new Digest(requestUri));
			}
			return value;
		}
	}

	public static void Remove(Uri requestUri)
	{
		lock (Locker)
		{
			Digests.Remove(requestUri.Host);
		}
	}

	public static string FindBest(List<string> headerValues)
	{
		if (headerValues == null || headerValues.Count == 0)
		{
			return string.Empty;
		}
		List<string> list = new List<string>(headerValues.Count);
		for (int i = 0; i < headerValues.Count; i++)
		{
			list.Add(headerValues[i].ToLower());
		}
		for (int j = 0; j < SupportedAlgorithms.Length; j++)
		{
			int num = list.FindIndex((string header) => header.StartsWith(SupportedAlgorithms[j]));
			if (num != -1)
			{
				return headerValues[num];
			}
		}
		return string.Empty;
	}
}
