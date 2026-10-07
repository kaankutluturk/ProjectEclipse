using System.Collections;
using System.Collections.Generic;

internal class OrderedDictionaryEnumerator : IEnumerator, IDictionaryEnumerator
{
	private IEnumerator<KeyValuePair<string, JsonData>> listEnumerator;


	public OrderedDictionaryEnumerator(IEnumerator<KeyValuePair<string, JsonData>> GEJJPNMHBJO)
	{
		listEnumerator = GEJJPNMHBJO;
	}

	public object Current
	{
		get
		{
			return Entry;
		}
	}

	public DictionaryEntry Entry
	{
		get
		{
			KeyValuePair<string, JsonData> current = listEnumerator.Current;
			return new DictionaryEntry(current.Key, current.Value);
		}
	}

	object IDictionaryEnumerator.Key
	{
		get
		{
			return listEnumerator.Current.Key;
		}
	}

	object IDictionaryEnumerator.Value
	{
		get
		{
			return listEnumerator.Current.Value;
		}
	}

	public bool MoveNext()
	{
		return listEnumerator.MoveNext();
	}

	public void Reset()
	{
		listEnumerator.Reset();
	}
}
