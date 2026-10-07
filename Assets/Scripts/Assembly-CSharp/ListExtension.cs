using System;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
	public static int AddIfNotExist<T>(this List<T> list, T item)
	{
		int num = list.IndexOf(item);
		if (num == -1)
		{
			list.Add(item);
			return list.Count - 1;
		}
		return num;
	}

	public static int AddIfNotExist<T>(this List<T> list, List<T> itemsToAdd) where T : class
	{
		int count = list.Count;
		for (int i = 0; i < itemsToAdd.Count; i++)
		{
			list.AddIfNotExist(itemsToAdd[i]);
		}
		return list.Count - count;
	}

	public static bool ContainsAllItems<T>(this List<T> sourceList, List<T> requiredItems) where T : IComparable
	{
		int count = requiredItems.Count;
		int count2 = sourceList.Count;
		if (count <= count2)
		{
			List<bool> list = new List<bool>(count2);
			for (int i = 0; i < count2; i++)
			{
				list.Add(false);
			}
			foreach (T item in requiredItems)
			{
				bool flag = false;
				for (int j = 0; j < sourceList.Count; j++)
				{
					if (!list[j] && item.Equals(sourceList[j]))
					{
						flag = true;
						list[j] = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static T GetRandomElement<T>(this List<T> list) where T : class
	{
		int count = list.Count;
		if (count == 0)
		{
			return (T)null;
		}
		return list[UnityEngine.Random.Range(0, count)];
	}

	public static void Resize<T>(this List<T> list, int size) where T : new()
	{
		int count = list.Count;
		if (count == size)
		{
			return;
		}
		if (count > size)
		{
			for (int num = count - 1; num >= size; num--)
			{
				list.RemoveAt(num);
			}
		}
		else
		{
			for (int i = count; i < size; i++)
			{
				list.Add(new T());
			}
		}
	}

	public static List<T> GetDistinct<T>(this List<T> source)
	{
		List<T> list = new List<T>();
		foreach (T item in source)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}
}
