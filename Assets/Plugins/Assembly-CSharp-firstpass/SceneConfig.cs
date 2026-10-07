using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SceneConfig : MonoBehaviour
{
	[Serializable]
	public class BaseProperty
	{
		public string Name;

		public int Index;

		public BaseProperty()
		{
			Name = "Property";
		}

		public BaseProperty(string name)
		{
			Name = name;
		}
	}

	[Serializable]
	public class NoneProperty : BaseProperty
	{
		public int Value;
	}

	[Serializable]
	public class IntProperty : BaseProperty
	{
		public int Value;
	}

	[Serializable]
	public class FloatProperty : BaseProperty
	{
		public float Value;
	}

	[Serializable]
	public class StringProperty : BaseProperty
	{
		public string Value;
	}

	[Serializable]
	public class BoolProperty : BaseProperty
	{
		public bool Value;
	}

	[Serializable]
	public class IntArrayProperty : BaseProperty
	{
		[SerializeField]
		public List<int> Value;
	}

	[Serializable]
	public class FloatArrayProperty : BaseProperty
	{
		[SerializeField]
		public List<float> Value;
	}

	[Serializable]
	public class StringArrayProperty : BaseProperty
	{
		[SerializeField]
		public List<string> Value;
	}

	[Serializable]
	public class BoolArrayProperty : BaseProperty
	{
		[SerializeField]
		public List<bool> Value;
	}

	[Serializable]
	public class PropertyArrayProperty : BaseProperty
	{
		[SerializeField]
		public List<BaseProperty> Value;
	}

	public string SceneName;

	[SerializeField]
	public List<NoneProperty> NoneProperties;

	[SerializeField]
	public List<IntProperty> IntProperties;

	[SerializeField]
	public List<FloatProperty> FloatProperties;

	[SerializeField]
	public List<StringProperty> StringProperties;

	[SerializeField]
	public List<BoolProperty> BoolProperties;

	[SerializeField]
	public List<IntArrayProperty> IntArrayProperties;

	[SerializeField]
	public List<FloatArrayProperty> FloatArrayProperties;

	[SerializeField]
	public List<StringArrayProperty> StringArrayProperties;

	[SerializeField]
	public List<BoolArrayProperty> BoolArrayProperties;

	[SerializeField]
	public List<PropertyArrayProperty> PropertyArrayProperties;

	public bool IsConfig;

	private static SceneConfig instance;

	private float leftBorderX;

	private float rightBorderX;

	private float centerX;

	private Vector3 spawnPointEnemy;

	private Vector3 spawnPointPlayer;

	private float maxDistBetweenModels;

	private float locationRightBorder;

	private float locationLeftBorder;

	private float camZOffset;

	public static SceneConfig CurrentInstance
	{
		get
		{
			return get_Instance();
		}
	}

	public static bool HasInstance
	{
		get
		{
			return get_IsPresent();
		}
	}

	public static float LeftBorder
	{
		get
		{
			return get_LeftBorderX();
		}
		set
		{
			set_LeftBorderX(value);
		}
	}

	public static float RightBorder
	{
		get
		{
			return get_RightBorderX();
		}
		set
		{
			set_RightBorderX(value);
		}
	}

	public static float Center
	{
		get
		{
			return get_CenterX();
		}
		private set
		{
			SetCenterX(value);
		}
	}

	public static Vector3 EnemySpawnPoint
	{
		get
		{
			return get_SpawnPointEnemy();
		}
		private set
		{
			SetSpawnPointEnemy(value);
		}
	}

	public static Vector3 PlayerSpawnPoint
	{
		get
		{
			return get_SpawnPointPlayer();
		}
		private set
		{
			SetSpawnPointPlayer(value);
		}
	}

	public static float FloorY
	{
		get
		{
			return get_PointFloor();
		}
	}

	public static float MaxModelDistance
	{
		get
		{
			return get_MaxDistBetweenModels();
		}
		private set
		{
			SetMaxDistBetweenModels(value);
		}
	}

	public static float LocationRightEdge
	{
		get
		{
			return get_LocationRightBorder();
		}
		private set
		{
			SetLocationRightBorder(value);
		}
	}

	public static float LocationLeftEdge
	{
		get
		{
			return get_LocationLeftBorder();
		}
		private set
		{
			SetLocationLeftBorder(value);
		}
	}

	public static float CameraZOffset
	{
		get
		{
			return get_CamZOffset();
		}
		private set
		{
			SetCamZOffset(value);
		}
	}

	public int GetInt(string JLCGLCLEGBD)
	{
		for (int i = 0; i < IntProperties.Count; i++)
		{
			if (IntProperties[i].Name.Equals(JLCGLCLEGBD))
			{
				return IntProperties[i].Value;
			}
		}
		return 0;
	}

	public float GetFloat(string JLCGLCLEGBD)
	{
		for (int i = 0; i < FloatProperties.Count; i++)
		{
			if (FloatProperties[i].Name.Equals(JLCGLCLEGBD))
			{
				return FloatProperties[i].Value;
			}
		}
		return 0f;
	}

	public string GetString(string JLCGLCLEGBD)
	{
		for (int i = 0; i < StringProperties.Count; i++)
		{
			if (StringProperties[i].Name.Equals(JLCGLCLEGBD))
			{
				return StringProperties[i].Value;
			}
		}
		return string.Empty;
	}

	public bool GetBool(string JLCGLCLEGBD)
	{
		for (int i = 0; i < BoolProperties.Count; i++)
		{
			if (BoolProperties[i].Name.Equals(JLCGLCLEGBD))
			{
				return BoolProperties[i].Value;
			}
		}
		return false;
	}

	public List<int> GetIntArray(string JLCGLCLEGBD)
	{
		for (int i = 0; i < IntArrayProperties.Count; i++)
		{
			if (IntArrayProperties[i].Name.Equals(JLCGLCLEGBD))
			{
				return IntArrayProperties[i].Value;
			}
		}
		return new List<int>();
	}

	public List<float> GetFloatArray(string JLCGLCLEGBD)
	{
		for (int i = 0; i < FloatArrayProperties.Count; i++)
		{
			if (FloatArrayProperties[i].Name.Equals(JLCGLCLEGBD))
			{
				return FloatArrayProperties[i].Value;
			}
		}
		return new List<float>();
	}

	public List<string> GetStringArray(string JLCGLCLEGBD)
	{
		for (int i = 0; i < StringArrayProperties.Count; i++)
		{
			if (StringArrayProperties[i].Name.Equals(JLCGLCLEGBD))
			{
				return StringArrayProperties[i].Value;
			}
		}
		return new List<string>();
	}

	public List<bool> GetBoolArray(string JLCGLCLEGBD)
	{
		for (int i = 0; i < BoolArrayProperties.Count; i++)
		{
			if (BoolArrayProperties[i].Name.Equals(JLCGLCLEGBD))
			{
				return BoolArrayProperties[i].Value;
			}
		}
		return new List<bool>();
	}

	public static SceneConfig get_Instance()
	{
		return instance;
	}

	public static bool get_IsPresent()
	{
		return instance;
	}

	public static float get_LeftBorderX()
	{
		return instance.leftBorderX;
	}

	public static void set_LeftBorderX(float value)
	{
		instance.leftBorderX = value;
	}

	public static float get_RightBorderX()
	{
		return instance.rightBorderX;
	}

	public static void set_RightBorderX(float value)
	{
		instance.rightBorderX = value;
	}

	public static float get_CenterX()
	{
		return instance.centerX;
	}

	private static void SetCenterX(float value)
	{
		instance.centerX = value;
	}

	public static Vector3 get_SpawnPointEnemy()
	{
		return instance.spawnPointEnemy;
	}

	private static void SetSpawnPointEnemy(Vector3 value)
	{
		instance.spawnPointEnemy = value;
	}

	public static Vector3 get_SpawnPointPlayer()
	{
		return instance.spawnPointPlayer;
	}

	private static void SetSpawnPointPlayer(Vector3 value)
	{
		instance.spawnPointPlayer = value;
	}

	public static float get_PointFloor()
	{
		return instance.spawnPointPlayer.y;
	}

	public static float get_MaxDistBetweenModels()
	{
		return instance.maxDistBetweenModels;
	}

	private static void SetMaxDistBetweenModels(float value)
	{
		instance.maxDistBetweenModels = value;
	}

	public static float get_LocationRightBorder()
	{
		return instance.locationRightBorder;
	}

	private static void SetLocationRightBorder(float value)
	{
		instance.locationRightBorder = value;
	}

	public static float get_LocationLeftBorder()
	{
		return instance.locationLeftBorder;
	}

	private static void SetLocationLeftBorder(float value)
	{
		instance.locationLeftBorder = value;
	}

	public static float get_CamZOffset()
	{
		return instance.camZOffset;
	}

	private static void SetCamZOffset(float value)
	{
		instance.camZOffset = value;
	}

	private void Awake()
	{
		instance = this;
		if (!IsConfig)
		{
			float x = base.transform.Find(GetString("LeftBorder")).position.x;
			set_LeftBorderX(x);
			SetLocationLeftBorder(x);
			x = base.transform.Find(GetString("RightBorder")).position.x;
			set_RightBorderX(x);
			SetLocationRightBorder(x);
			SetCenterX((get_RightBorderX() + get_LeftBorderX()) / 2f);
			SetSpawnPointEnemy(base.transform.Find(GetString("SpawnPointA")).position);
			SetSpawnPointPlayer(base.transform.Find(GetString("SpawnPointB")).position);
			SetMaxDistBetweenModels(GetFloat("MaxDistBetweenModels"));
			SetCamZOffset(GetFloat("CamZOffset"));
		}
	}
}
