using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Nekki.Social;
using Newtonsoft.Json;
using SimpleJSON;
using UnityEngine;

public abstract class ServerProviderBase : MonoBehaviour
{
	public class SelectQuery
	{
		private string _table;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string[] fields;

		private QueryCondition[] _conditions;

		private string[] providerOrder;

		private int? _limit;

		public string[] Fields
		{
			get
			{
				return GetFields();
			}
			private set
			{
				SetFields(value);
			}
		}

		public SelectQuery(string table, string[] fields, QueryCondition[] conditions, string[] order, int? limit)
		{
			_table = table;
			SetFields(fields);
			_conditions = conditions;
			providerOrder = order;
			_limit = limit;
		}

		public string[] GetFields()
		{
			return fields;
		}

		private void SetFields(string[] value)
		{
			fields = value;
		}

		private Form ToForm()
		{
			Form form = new Form();
			form.Add("table", _table);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[");
			for (int i = 0; i < GetFields().Length; i++)
			{
				stringBuilder.Append(string.Format("\"{0}\"", GetFields()[i]));
				if (i < GetFields().Length - 1)
				{
					stringBuilder.Append(",");
				}
			}
			stringBuilder.Append("]");
			form.Add("fields", stringBuilder.ToString());
			if (_conditions != null)
			{
				form.Add("where", QueryCondition.ToJson(_conditions));
			}
			if (providerOrder != null)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.Append("[");
				for (int j = 0; j < providerOrder.Length; j++)
				{
					stringBuilder2.Append(string.Format("\"{0}\"", providerOrder[j]));
					if (j < providerOrder.Length - 1)
					{
						stringBuilder2.Append(",");
					}
				}
				stringBuilder2.Append("]");
				form.Add("order", stringBuilder2.ToString());
			}
			int? limitValue = _limit;
			if (limitValue.HasValue)
			{
				form.Add("limit", _limit.Value);
			}
			return form;
		}

		[SpecialName]
		public static WWWForm op_Implicit(SelectQuery query)
		{
			return Form.op_Implicit(query.ToForm());
		}
	}

	public class QueryCondition
	{
		private string field;

		private string op;

		private string value;

		private QueryCondition()
		{
		}

		public static QueryCondition Greater(string fieldName, object value)
		{
			QueryCondition condition = new QueryCondition();
			condition.field = fieldName;
			condition.op = ">";
			condition.value = value.ToString();
			return condition;
		}

		public static QueryCondition Less(string fieldName, object value)
		{
			QueryCondition condition = new QueryCondition();
			condition.field = fieldName;
			condition.op = "<";
			condition.value = value.ToString();
			return condition;
		}

		public static QueryCondition Equals(string fieldName, object value)
		{
			QueryCondition condition = new QueryCondition();
			condition.field = fieldName;
			condition.op = "=";
			condition.value = value.ToString();
			return condition;
		}

		public static QueryCondition Custom(string fieldName, string operatorSymbol, object value)
		{
			QueryCondition condition = new QueryCondition();
			condition.field = fieldName;
			condition.op = operatorSymbol;
			condition.value = value.ToString();
			return condition;
		}

		public string ToJson()
		{
			return string.Format("[\"{0}\",\"{1}\",\"{2}\"]", field, op, value);
		}

		public static string ToJson(QueryCondition[] conditions)
		{
			string text = "[";
			for (int i = 0; i < conditions.Length; i++)
			{
				text += conditions[i].ToJson();
				if (i != conditions.Length - 1)
				{
					text += ",";
				}
			}
			return text + "]";
		}
	}

	public class QueryValue
	{
		private readonly string _value;

		public string AsString
		{
			get
			{
				return GetString();
			}
		}

		public float ParseFloat
		{
			get
			{
				return GetFloat();
			}
		}

		public int ParseInt
		{
			get
			{
				return GetInt();
			}
		}

		public QueryValue(string value)
		{
			_value = value ?? string.Empty;
		}

		public string GetString()
		{
			return _value;
		}

		public float GetFloat()
		{
			return float.Parse(_value);
		}

		public int GetInt()
		{
			return int.Parse(_value);
		}

		public override string ToString()
		{
			return GetString();
		}
	}

	[DefaultMember("Item")]
	public class QueryRow
	{
		private readonly Dictionary<string, QueryValue> values;

		// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
		public QueryValue get_DLKPBAJDHBO(string index)
		{
			return get_Item(index);
		}

		public QueryRow(Dictionary<string, QueryValue> rowValues)
		{
			values = rowValues;
		}

		public QueryValue get_Item(string index)
		{
			if (!values.ContainsKey(index.ToLower()))
			{
				AdvLog.LogWarning("there no data for key " + index.ToLower());
			}
			return values[index.ToLower()];
		}
	}

	[DefaultMember("Item")]
	public class QueryResult
	{
		private readonly List<QueryRow> _data = new List<QueryRow>();

		// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
		public QueryRow get_DLKPBAJDHBO(int index)
		{
			return get_Item(index);
		}

		public int Count
		{
			get
			{
				return GetCount();
			}
		}

		public bool Empty
		{
			get
			{
				return IsEmpty();
			}
		}

		public QueryResult(JSONArray rows, SelectQuery query)
		{
			for (int i = 0; i < rows.Count; i++)
			{
				Dictionary<string, QueryValue> dictionary = new Dictionary<string, QueryValue>();
				for (int j = 0; j < query.GetFields().Length; j++)
				{
					dictionary.Add(query.GetFields()[j].ToLower(), new QueryValue(rows[i].AsArray[j]));
				}
				_data.Add(new QueryRow(dictionary));
			}
		}

		public QueryRow get_Item(int index)
		{
			return _data[index];
		}

		public int GetCount()
		{
			return _data.Count;
		}

		public bool IsEmpty()
		{
			return _data.Count == 0;
		}
	}

	public class FileData
	{
		private string mimeType;

		private string _source;

		private string fileName;

		public string MimeType
		{
			get
			{
				return GetMimeType();
			}
		}

		public string FileName
		{
			get
			{
				return GetFileName();
			}
		}

		public string SourceName
		{
			get
			{
				return GetSource();
			}
		}

		public FileData(string sourcePath, string dataFileName, string dataMimeType)
		{
			_source = sourcePath;
			fileName = dataFileName;
			mimeType = dataMimeType;
		}

		public string GetMimeType()
		{
			return mimeType;
		}

		public string GetFileName()
		{
			return fileName;
		}

		public string GetSource()
		{
			return _source;
		}
	}

	public class Form
	{
		private readonly List<KeyValuePair<string, string>> _data = new List<KeyValuePair<string, string>>();

		private readonly List<KeyValuePair<string, FileData>> files = new List<KeyValuePair<string, FileData>>();

		private static string _key = "DGgim7dg7cbknRCxVOAlXfGVtjOPyZls";

		public static string SigningKey
		{
			get
			{
				return GetKey();
			}
			set
			{
				set_Key(value);
			}
		}

		public Form()
		{
			Add("rand", UnityEngine.Random.Range(0, int.MaxValue));
		}

		public static string GetKey()
		{
			return _key;
		}

		public static void set_Key(string value)
		{
			_key = value;
		}

		public void Add(string key, object value)
		{
			_data.Add(new KeyValuePair<string, string>(key, value.ToString()));
		}

		public void AddFile(string key, FileData fileData)
		{
			KeyValuePair<string, FileData> item = new KeyValuePair<string, FileData>(key, fileData);
			files.Add(item);
		}

		[SpecialName]
		public static WWWForm op_Implicit(Form form)
		{
			form._data.Sort(CompareByKey);
			WWWForm wWWForm = new WWWForm();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < form._data.Count; i++)
			{
				stringBuilder.Append(string.Format("{0}={1}", form._data[i].Key, form._data[i].Value));
				wWWForm.AddField(form._data[i].Key, form._data[i].Value);
			}
			foreach (KeyValuePair<string, FileData> item in form.files)
			{
				string key = item.Key;
				string filePath = item.Value.GetSource();
				byte[] contents = FileUtils.ReadAllBytes(filePath);
				string fileName = item.Value.GetFileName();
				string mimeType = item.Value.GetMimeType();
				wWWForm.AddBinaryData(key, contents, fileName, mimeType);
			}
			stringBuilder.Append(_key);
			MD5 mD = MD5.Create();
			byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
			wWWForm.AddField("sig", BitConverter.ToString(array).Replace("-", string.Empty).ToLower());
			return wWWForm;
		}

		private static int CompareByKey(KeyValuePair<string, string> left, KeyValuePair<string, string> right)
		{
			return string.Compare(left.Key, right.Key, StringComparison.Ordinal);
		}

		public JSONClass ToJsonClass()
		{
			JSONClass jSONClass = new JSONClass();
			for (int i = 0; i < _data.Count; i++)
			{
				jSONClass[_data[i].Key] = _data[i].Value;
			}
			return jSONClass;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < _data.Count; i++)
			{
				stringBuilder.Append(string.Format("{0}={1}\n", _data[i].Key, _data[i].Value));
			}
			return stringBuilder.ToString();
		}
	}

	protected class ServerResponse
	{
		public string GIHDDAKBMHE;

		public string JDONBAPIJCG;

		public static ServerResponse Get(string json)
		{
			return JsonConvert.DeserializeObject<ServerResponse>(json);
		}
	}

	private static GameObject _nestedObject;

	private readonly List<IEnumerator> _holdRoutine = new List<IEnumerator>();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static ServerProviderBase instance;

	protected static GameObject NestedObject
	{
		get
		{
			return GetNestedObject();
		}
	}

	public static ServerProviderBase CurrentInstance
	{
		get
		{
			return get_Instance();
		}
		protected set
		{
			set_Instance(value);
		}
	}

	protected abstract string GetServerUrl();

	protected static GameObject GetNestedObject()
	{
		return _nestedObject;
	}

	protected void OnDestroy()
	{
		_nestedObject = null;
	}

	internal void Update()
	{
		if (_holdRoutine.Count > 0)
		{
			for (int i = 0; i < _holdRoutine.Count; i++)
			{
				StartCoroutine(_holdRoutine[i]);
			}
			_holdRoutine.Clear();
		}
	}

	public static ServerProviderBase get_Instance()
	{
		return instance;
	}

	protected static void set_Instance(ServerProviderBase value)
	{
		instance = value;
	}

	protected static T Init<T>() where T : ServerProviderBase
	{
		_nestedObject = GameObject.Find("_server");
		if (_nestedObject == null)
		{
			_nestedObject = new GameObject("_server");
			UnityEngine.Object.DontDestroyOnLoad(_nestedObject);
		}
		T val = _nestedObject.GetComponent<T>();
		if (val == null)
		{
			val = _nestedObject.AddComponent<T>();
		}
		set_Instance(val);
		return val;
	}

	protected abstract void Init();

	protected bool Check()
	{
		Init();
		if (!_nestedObject)
		{
			AdvLog.LogError("ServerCall terminated. you should call Init<T>() from your Init() method! (where T is your inherited class)");
			return false;
		}
		return true;
	}

	public virtual void Join(Action<bool> onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
	}

	protected virtual IEnumerator JoinRoutine(Action<bool> onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
		yield break;
	}

	public virtual void SaveData(string key, string data, Action onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
	}

	protected virtual IEnumerator SaveDataRoutine(string key, string data, Action onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
		yield break;
	}

	public virtual void LoadData(string key, Action<string> onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
	}

	protected virtual IEnumerator LoadDataRoutine(string key, Action<string> onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
		yield break;
	}

	protected string Unescape(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		return text.Replace("\\/", "/").Replace("\\\"", "\"").Replace("\\r\\n", "\n");
	}

	public virtual void TimeSync(Action<long> onDone, Action<string> onError)
	{
		onDone?.Invoke((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
	}

	protected virtual IEnumerator TimeSyncRoutine(Action<long> onDone, Action<string> onError)
	{
		onDone?.Invoke((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
		yield break;
	}

	public virtual void WipeUser(Action onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
	}

	protected virtual IEnumerator WipeUserRoutine(Action onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
		yield break;
	}

	public virtual QueryResult Query(SelectQuery query)
	{
		throw new NotSupportedException("Remote queries are disabled in the offline build.");
	}

	public virtual void Query(SelectQuery query, Action<QueryResult> onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
	}

	protected virtual IEnumerator QueryRoutine(SelectQuery query, Action<QueryResult> onDone, Action<string> onError)
	{
		onError?.Invoke("offline build");
		yield break;
	}
}
