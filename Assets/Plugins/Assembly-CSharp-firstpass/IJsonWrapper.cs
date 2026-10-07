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

	void SetBoolean(bool PKHDLOGJKAD);

	void SetDouble(double PKHDLOGJKAD);

	void SetInt(int PKHDLOGJKAD);

	void SetJsonType(JsonType LFLGCDNKNJI);

	void SetLong(long PKHDLOGJKAD);

	void SetString(string PKHDLOGJKAD);

	string ToJson();

	void ToJson(JsonWriter writer);
}
