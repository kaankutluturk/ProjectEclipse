using System.Collections;

public interface IJsonWrapper : IDictionary, IList, IEnumerable, ICollection, IOrderedDictionary
{
	bool IsArray { get; }

	bool IsBoolean { get; }

	bool IsDouble { get; }

	bool IsInt { get; }

	bool IsLong { get; }

	bool IsObject { get; }

	bool IsString { get; }

	bool GetIsArray();

	bool GetIsBoolean();

	bool GetIsDouble();

	bool GetIsInt();

	bool GetIsLong();

	bool GetIsObject();

	bool GetIsString();

	bool GetBoolean();

	double GetDouble();

	int GetInt();

	JsonType GetJsonType();

	long GetLong();

	string GetString();

	void SetBoolean(bool value);

	void SetDouble(double value);

	void SetInt(int value);

	void SetJsonType(JsonType type);

	void SetLong(long value);

	void SetString(string value);

	string ToJson();

	void ToJson(JsonWriter writer);
}
