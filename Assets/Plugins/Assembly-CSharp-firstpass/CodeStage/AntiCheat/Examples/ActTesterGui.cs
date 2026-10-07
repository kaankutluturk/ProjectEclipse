using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace CodeStage.AntiCheat.Examples
{
	[AddComponentMenu("")]
	public class ActTesterGui : MonoBehaviour
	{
		private const string RedColor = "#FF4040";

		private const string GreenColor = "#02C85F";

		private const string PrefsString = "name";

		private const string PrefsInt = "money";

		private const string PrefsFloat = "lifeBar";

		private const string PrefsBool = "gameComplete";

		private const string PrefsUint = "demoUint";

		private const string PrefsLong = "demoLong";

		private const string PrefsDouble = "demoDouble";

		private const string PrefsVector2 = "demoVector2";

		private const string PrefsVector3 = "demoVector3";

		private const string PrefsQuaternion = "demoQuaternion";

		private const string PrefsRect = "demoRect";

		private const string PrefsColor = "demoColor";

		private const string PrefsByteArray = "demoByteArray";

		private const string ApiUrlLockToDevice = "http://j.mp/1gxg1tf";

		private const string ApiUrlPreservePrefs = "http://j.mp/1iBK5pz";

		private const string ApiUrlEmergencyMode = "http://j.mp/1FRAL5L";

		private const string ApiUrlReadForeign = "http://j.mp/1LCdpDa";

		private const string ApiUrlUnobscuredMode = "http://j.mp/1KVrpxi";

		private const string ApiUrlPlayerPrefs = "http://docs.unity3d.com/ScriptReference/PlayerPrefs.html";

		[Header("Regular variables")]
		public string regularString = "I'm regular string";

		public int regularInt = 1987;

		public float regularFloat = 2013.0524f;

		public Vector3 regularVector3 = new Vector3(10.5f, 11.5f, 12.5f);

		[Header("Obscured (secure) variables")]
		public ObscuredString obscuredString = (ObscuredString)("I'm obscured string");

		public ObscuredInt obscuredInt = (ObscuredInt)(1987);

		public ObscuredFloat obscuredFloat = (ObscuredFloat)(2013.0524f);

		public ObscuredVector3 obscuredVector3 = (ObscuredVector3)(new Vector3(10.5f, 11.5f, 12.5f));

		public ObscuredBool obscuredBool = (ObscuredBool)(true);

		public ObscuredLong obscuredLong = (ObscuredLong)(945678987654123345L);

		public ObscuredDouble obscuredDouble = (ObscuredDouble)(9.45678987654);

		public ObscuredVector2 obscuredVector2 = (ObscuredVector2)(new Vector2(8.5f, 9.5f));

		[Header("Other")]
		public string prefsEncryptionKey = "change me!";

		private readonly string[] tabs = new string[3] { "Variables protection", "Saves protection", "Cheating detectors" };

		private int currentTab;

		private string allSimpleObscuredTypes;

		private string regularPrefs;

		private string obscuredPrefs;

		private int savesLock;

		private bool savesAlterationDetected;

		private bool foreignSavesDetected;

		private bool injectionDetected;

		private bool speedHackDetected;

		private bool obscuredTypeCheatDetected;

		private bool wallHackCheatDetected;

		private readonly StringBuilder logBuilder = new StringBuilder();

		public void OnSpeedHackDetected()
		{
			speedHackDetected = true;
			Debug.Log("Speed hack Detected!");
		}

		public void OnInjectionDetected()
		{
			injectionDetected = true;
			Debug.Log("Injection Detected!");
		}

		public void OnObscuredTypeCheatingDetected()
		{
			obscuredTypeCheatDetected = true;
			Debug.Log("Obscured Vars Cheating Detected!");
		}

		public void OnWallHackDetected()
		{
			wallHackCheatDetected = true;
			Debug.Log("Wall hack Detected!");
		}

		private void OnValidate()
		{
			if (Application.isPlaying)
			{
				ObscuredPrefs.SetCryptoKey(prefsEncryptionKey);
			}
		}

		private void Awake()
		{
			ObscuredPrefs.SetCryptoKey(prefsEncryptionKey);
			ObscuredPrefs.OnAlterationDetected = OnSavesTampered;
			ObscuredPrefs.OnPossibleForeignSavesDetected = OnForeignSavesDetected;
		}

		private void Start()
		{
			ObscuredStringExample();
			ObscuredIntExample();
			ObscuredFloatExample();
			ObscuredVector3Example();
			Invoke("RandomizeObscuredVars", UnityEngine.Random.Range(1f, 10f));
		}

		private void RandomizeObscuredVars()
		{
			obscuredInt.RandomizeCryptoKey();
			obscuredFloat.RandomizeCryptoKey();
			obscuredString.RandomizeCryptoKey();
			obscuredVector3.RandomizeCryptoKey();
			Invoke("RandomizeObscuredVars", UnityEngine.Random.Range(1f, 10f));
		}

		private void ObscuredStringExample()
		{
			logBuilder.Length = 0;
			logBuilder.AppendLine("[ACTk] <b>[ ObscuredString test ]</b>");
			ObscuredString.SetNewCryptoKey("I LOVE MY GIRLz");
			string text = "the Goscurry is not a lie ;)";
			logBuilder.AppendLine("Original string:\n" + text);
			ObscuredString obscuredString = (ObscuredString)(text);
			logBuilder.AppendLine("How your string is stored in memory when obscured:\n" + obscuredString.GetEncrypted());
			Debug.Log(logBuilder);
		}

		private void ObscuredIntExample()
		{
			logBuilder.Length = 0;
			logBuilder.AppendLine("[ACTk] <b>[ ObscuredInt test ]</b>");
			ObscuredInt.SetNewCryptoKey(434523);
			int num = 5;
			logBuilder.AppendLine("Original lives count: " + num);
			ObscuredInt lives = (ObscuredInt)(num);
			logBuilder.AppendLine("How your lives count is stored in memory when obscured: " + lives.GetEncrypted());
			ObscuredInt.SetNewCryptoKey(666);
			num = (int)(lives);
			lives = (ObscuredInt)((int)(lives) - 2);
			lives = (ObscuredInt)((int)(lives) + num + 10);
			lives = (ObscuredInt)((int)(lives) / 2);
			lives = ObscuredInt.op_Increment(lives);
			ObscuredInt.SetNewCryptoKey(999);
			lives = ObscuredInt.op_Increment(lives);
			lives = ObscuredInt.op_Decrement(lives);
			logBuilder.AppendLine(string.Concat("Lives count after few usual operations: ", lives, " (", lives.ToString("X"), "h)"));
			Debug.Log(logBuilder);
		}

		private void ObscuredFloatExample()
		{
			logBuilder.Length = 0;
			logBuilder.AppendLine("[ACTk] <b>[ ObscuredFloat test ]</b>");
			ObscuredFloat.SetNewCryptoKey(404);
			float num = 99.9f;
			logBuilder.AppendLine("Original health bar: " + num);
			ObscuredFloat healthBar = (ObscuredFloat)(num);
			logBuilder.AppendLine("How your health bar is stored in memory when obscured: " + healthBar.GetEncrypted());
			ObscuredFloat.SetNewCryptoKey(666);
			healthBar = (ObscuredFloat)((float)(healthBar) + 6f);
			healthBar = (ObscuredFloat)((float)(healthBar) - 1.5f);
			healthBar = ObscuredFloat.op_Increment(healthBar);
			healthBar = ObscuredFloat.op_Decrement(healthBar);
			healthBar = ObscuredFloat.op_Decrement(healthBar);
			healthBar = (ObscuredFloat)(num - (float)(healthBar) + 10.5f);
			logBuilder.AppendLine("Health bar after few usual operations: " + healthBar);
			Debug.Log(logBuilder);
		}

		private void ObscuredVector3Example()
		{
			logBuilder.Length = 0;
			logBuilder.AppendLine("[ACTk] <b>[ ObscuredVector3 test ]</b>");
			ObscuredVector3.SetNewCryptoKey(404);
			Vector3 vector = new Vector3(54.1f, 64.3f, 63.2f);
			logBuilder.AppendLine("Original position: " + vector);
			ObscuredVector3 rawObfuscatedVector = (ObscuredVector3)(vector);
			ObscuredVector3.RawEncryptedVector3 rawEncryptedVector = rawObfuscatedVector.GetEncrypted();
			logBuilder.AppendLine("How your position is stored in memory when obscured: (" + rawEncryptedVector.x + ", " + rawEncryptedVector.y + ", " + rawEncryptedVector.z + ")");
			Debug.Log(logBuilder);
		}

		private void OnSavesTampered()
		{
			savesAlterationDetected = true;
		}

		private void OnForeignSavesDetected()
		{
			foreignSavesDetected = true;
		}

		private void OnGUI()
		{
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
			gUIStyle.alignment = TextAnchor.UpperCenter;
			GUILayout.BeginArea(new Rect(10f, 5f, Screen.width - 20, Screen.height - 10));
			GUILayout.Label("<color=\"#0287C8\"><b>Anti-Cheat Toolkit Sandbox</b></color>", gUIStyle);
			GUILayout.Label("Here you can overview common ACTk features and try to cheat something yourself.", gUIStyle);
			GUILayout.Space(5f);
			currentTab = GUILayout.Toolbar(currentTab, tabs);
			if (currentTab == 0)
			{
				GUILayout.Label("ACTk offers own collection of the secure types to let you protect your variables from <b>ANY</b> memory hacking tools (Cheat Engine, ArtMoney, GameCIH, Game Guardian, etc.).");
				GUILayout.Space(5f);
				using (new HorizontalLayout())
				{
					GUILayout.Label("<b>Obscured types:</b>\n<color=\"#75C4EB\">" + GetAllSimpleObscuredTypes() + "</color>", GUILayout.MinWidth(130f));
					GUILayout.Space(10f);
					using (new VerticalLayout(GUI.skin.box))
					{
						GUILayout.Label("Below you can try to cheat few variables of the regular types and their obscured (secure) analogues (you may change initial values from Tester object inspector):");
						GUILayout.Space(10f);
						using (new HorizontalLayout())
						{
							GUILayout.Label("<b>string:</b> " + regularString, GUILayout.Width(250f));
							if (GUILayout.Button("Add random value"))
							{
								regularString += (char)UnityEngine.Random.Range(97, 122);
							}
							if (GUILayout.Button("Reset"))
							{
								regularString = string.Empty;
							}
						}
						using (new HorizontalLayout())
						{
							GUILayout.Label("<b>ObscuredString:</b> " + (string)(obscuredString), GUILayout.Width(250f));
							if (GUILayout.Button("Add random value"))
							{
								obscuredString = (ObscuredString)((string)(obscuredString) + (char)UnityEngine.Random.Range(97, 122));
							}
							if (GUILayout.Button("Reset"))
							{
								obscuredString = (ObscuredString)(string.Empty);
							}
						}
						GUILayout.Space(10f);
						using (new HorizontalLayout())
						{
							GUILayout.Label("<b>int:</b> " + regularInt, GUILayout.Width(250f));
							if (GUILayout.Button("Add random value"))
							{
								regularInt += UnityEngine.Random.Range(1, 100);
							}
							if (GUILayout.Button("Reset"))
							{
								regularInt = 0;
							}
						}
						using (new HorizontalLayout())
						{
							GUILayout.Label("<b>ObscuredInt:</b> " + obscuredInt, GUILayout.Width(250f));
							if (GUILayout.Button("Add random value"))
							{
								obscuredInt = (ObscuredInt)((int)(obscuredInt) + UnityEngine.Random.Range(1, 100));
							}
							if (GUILayout.Button("Reset"))
							{
								obscuredInt = (ObscuredInt)(0);
							}
						}
						GUILayout.Space(10f);
						using (new HorizontalLayout())
						{
							GUILayout.Label("<b>float:</b> " + regularFloat, GUILayout.Width(250f));
							if (GUILayout.Button("Add random value"))
							{
								regularFloat += UnityEngine.Random.Range(1f, 100f);
							}
							if (GUILayout.Button("Reset"))
							{
								regularFloat = 0f;
							}
						}
						using (new HorizontalLayout())
						{
							GUILayout.Label("<b>ObscuredFloat:</b> " + obscuredFloat, GUILayout.Width(250f));
							if (GUILayout.Button("Add random value"))
							{
								obscuredFloat = (ObscuredFloat)((float)(obscuredFloat) + UnityEngine.Random.Range(1f, 100f));
							}
							if (GUILayout.Button("Reset"))
							{
								obscuredFloat = (ObscuredFloat)(0f);
							}
						}
						GUILayout.Space(10f);
						using (new HorizontalLayout())
						{
							GUILayout.Label("<b>Vector3:</b> " + regularVector3, GUILayout.Width(250f));
							if (GUILayout.Button("Add random value"))
							{
								regularVector3 += UnityEngine.Random.insideUnitSphere;
							}
							if (GUILayout.Button("Reset"))
							{
								regularVector3 = Vector3.zero;
							}
						}
						using (new HorizontalLayout())
						{
							GUILayout.Label("<b>ObscuredVector3:</b> " + obscuredVector3, GUILayout.Width(250f));
							if (GUILayout.Button("Add random value"))
							{
								obscuredVector3 = ObscuredVector3.op_Addition(obscuredVector3, UnityEngine.Random.insideUnitSphere);
							}
							if (GUILayout.Button("Reset"))
							{
								obscuredVector3 = (ObscuredVector3)(Vector3.zero);
							}
						}
					}
				}
			}
			else if (currentTab == 1)
			{
				GUILayout.Label("ACTk has secure layer for the PlayerPrefs: <color=\"#75C4EB\">ObscuredPrefs</color>. It protects data from view, detects any cheating attempts, optionally locks data to the current device and supports additional data types.");
				GUILayout.Space(5f);
				using (new HorizontalLayout())
				{
					GUILayout.Label("<b>Supported types:</b>\n" + GetAllObscuredPrefsDataTypes(), GUILayout.MinWidth(130f));
					using (new VerticalLayout(GUI.skin.box))
					{
						GUILayout.Label("Below you can try to cheat both regular PlayerPrefs and secure ObscuredPrefs:");
						using (new VerticalLayout())
						{
							GUILayout.Label("<color=\"#FF4040\"><b>PlayerPrefs:</b></color>\neasy to cheat, only 3 supported types", gUIStyle);
							GUILayout.Space(5f);
							if (string.IsNullOrEmpty(regularPrefs))
							{
								LoadRegularPrefs();
							}
							using (new HorizontalLayout())
							{
								GUILayout.Label(regularPrefs, GUILayout.Width(270f));
								using (new VerticalLayout())
								{
									using (new HorizontalLayout())
									{
										if (GUILayout.Button("Save"))
										{
											SaveRegularPrefs();
										}
										if (GUILayout.Button("Load"))
										{
											LoadRegularPrefs();
										}
									}
									if (GUILayout.Button("Delete"))
									{
										DeleteRegularPrefs();
									}
								}
							}
						}
						GUILayout.Space(5f);
						using (new VerticalLayout())
						{
							GUILayout.Label("<color=\"#02C85F\"><b>ObscuredPrefs:</b></color>\nsecure, lot of additional types and extra options", gUIStyle);
							GUILayout.Space(5f);
							if (string.IsNullOrEmpty(obscuredPrefs))
							{
								LoadObscuredPrefs();
							}
							using (new HorizontalLayout())
							{
								GUILayout.Label(obscuredPrefs, GUILayout.Width(270f));
								using (new VerticalLayout())
								{
									using (new HorizontalLayout())
									{
										if (GUILayout.Button("Save"))
										{
											SaveObscuredPrefs();
										}
										if (GUILayout.Button("Load"))
										{
											LoadObscuredPrefs();
										}
									}
									if (GUILayout.Button("Delete"))
									{
										DeleteObscuredPrefs();
									}
									using (new HorizontalLayout())
									{
										GUILayout.Label("LockToDevice level");
										ShowHelpButton("http://j.mp/1gxg1tf");
									}
									savesLock = GUILayout.SelectionGrid(savesLock, new string[3]
									{
										ObscuredPrefs.DeviceLockLevel.None.ToString(),
										ObscuredPrefs.DeviceLockLevel.Soft.ToString(),
										ObscuredPrefs.DeviceLockLevel.Strict.ToString()
									}, 3);
									ObscuredPrefs.LockToDevice = (ObscuredPrefs.DeviceLockLevel)savesLock;
									GUILayout.Space(5f);
									using (new HorizontalLayout())
									{
										ObscuredPrefs.PreservePlayerPrefs = GUILayout.Toggle(ObscuredPrefs.PreservePlayerPrefs, "preservePlayerPrefs");
										ShowHelpButton("http://j.mp/1iBK5pz");
									}
									using (new HorizontalLayout())
									{
										ObscuredPrefs.EmergencyMode = GUILayout.Toggle(ObscuredPrefs.EmergencyMode, "emergencyMode");
										ShowHelpButton("http://j.mp/1FRAL5L");
									}
									using (new HorizontalLayout())
									{
										ObscuredPrefs.ReadForeignSaves = GUILayout.Toggle(ObscuredPrefs.ReadForeignSaves, "readForeignSaves");
										ShowHelpButton("http://j.mp/1LCdpDa");
									}
									GUILayout.Space(5f);
									GUILayout.Label("<color=\"" + ((!savesAlterationDetected) ? "#02C85F" : "#FF4040") + "\">Saves modification detected: " + savesAlterationDetected + "</color>");
									GUILayout.Label("<color=\"" + ((!foreignSavesDetected) ? "#02C85F" : "#FF4040") + "\">Foreign saves detected: " + foreignSavesDetected + "</color>");
								}
							}
						}
						GUILayout.Space(5f);
						ShowHelpButton("http://docs.unity3d.com/ScriptReference/PlayerPrefs.html", "Visit docs to see where PlayerPrefs are stored", -1);
					}
				}
			}
			else
			{
				GUILayout.Label("ACTk is able to detect some types of cheating to let you take action on the cheating players. This example scene has all possible detectors and all of them are automatically start on scene start.");
				GUILayout.Space(5f);
				using (new VerticalLayout(GUI.skin.box))
				{
					GUILayout.Label("<b>Speed Hack Detector</b>");
					GUILayout.Label("Allows to detect Cheat Engine's speed hack (and maybe some other speed hack tools) usage.");
					GUILayout.Label("<color=\"" + ((!speedHackDetected) ? "#02C85F" : "#FF4040") + "\">Detected: " + speedHackDetected.ToString().ToLower() + "</color>");
					GUILayout.Space(10f);
					GUILayout.Label("<b>Obscured Cheating Detector</b>");
					GUILayout.Label("Detects cheating of any Obscured type (except ObscuredPrefs, it has own detection features) used in project.");
					GUILayout.Label("<color=\"" + ((!obscuredTypeCheatDetected) ? "#02C85F" : "#FF4040") + "\">Detected: " + obscuredTypeCheatDetected.ToString().ToLower() + "</color>");
					GUILayout.Space(10f);
					GUILayout.Label("<b>WallHack Detector</b>");
					GUILayout.Label("Detects common types of wall hack cheating: walking through the walls (Rigidbody and CharacterController modules), shooting through the walls (Raycast module), looking through the walls (Wireframe module).");
					GUILayout.Label("<color=\"" + ((!wallHackCheatDetected) ? "#02C85F" : "#FF4040") + "\">Detected: " + wallHackCheatDetected.ToString().ToLower() + "</color>");
					GUILayout.Space(10f);
					GUILayout.Label("<b>Injection Detector</b>");
					GUILayout.Label("Allows to detect foreign managed assemblies in your application.");
					GUILayout.Label("<color=\"" + ((!injectionDetected) ? "#02C85F" : "#FF4040") + "\">Detected: " + injectionDetected.ToString().ToLower() + "</color>");
				}
			}
			GUILayout.EndArea();
		}

		private string GetAllSimpleObscuredTypes()
		{
			string result = "Can't get the list, sorry :(";
			string typesList = string.Empty;
			if (string.IsNullOrEmpty(allSimpleObscuredTypes))
			{
				IEnumerable<Type> source = from type in Assembly.GetExecutingAssembly().GetTypes()
					where type.IsPublic && type.Namespace == "CodeStage.AntiCheat.ObscuredTypes" && type.Name != "ObscuredPrefs"
					select type;
				source.ToList().ForEach((Type type) =>
				{
					if (typesList.Length > 0)
					{
						typesList = typesList + "\n" + type.Name;
					}
					else
					{
						typesList += type.Name;
					}
				});
				if (!string.IsNullOrEmpty(typesList))
				{
					result = typesList;
					allSimpleObscuredTypes = typesList;
				}
			}
			else
			{
				result = allSimpleObscuredTypes;
			}
			return result;
		}

		private string GetAllObscuredPrefsDataTypes()
		{
			return "int\nfloat\nstring\n<color=\"#75C4EB\">uint\ndouble\nlong\nbool\nbyte[]\nVector2\nVector3\nQuaternion\nColor\nRect</color>";
		}

		private void LoadRegularPrefs()
		{
			regularPrefs = "int: " + PlayerPrefs.GetInt("money", -1) + "\n";
			string prefsText = regularPrefs;
			regularPrefs = prefsText + "float: " + PlayerPrefs.GetFloat("lifeBar", -1f) + "\n";
			regularPrefs = regularPrefs + "string: " + PlayerPrefs.GetString("name", "No saved PlayerPrefs!");
		}

		private void SaveRegularPrefs()
		{
			PlayerPrefs.SetInt("money", 456);
			PlayerPrefs.SetFloat("lifeBar", 456.789f);
			PlayerPrefs.SetString("name", "Hey, there!");
			PlayerPrefs.Save();
		}

		private void DeleteRegularPrefs()
		{
			PlayerPrefs.DeleteKey("money");
			PlayerPrefs.DeleteKey("lifeBar");
			PlayerPrefs.DeleteKey("name");
			PlayerPrefs.Save();
		}

		private void LoadObscuredPrefs()
		{
			byte[] array = ObscuredPrefs.GetByteArray("demoByteArray", 0, 4);
			obscuredPrefs = "int: " + ObscuredPrefs.GetInt("money", -1) + "\n";
			string prefsText = obscuredPrefs;
			obscuredPrefs = prefsText + "float: " + ObscuredPrefs.GetFloat("lifeBar", -1f) + "\n";
			obscuredPrefs = obscuredPrefs + "string: " + ObscuredPrefs.GetString("name", "No saved ObscuredPrefs!") + "\n";
			prefsText = obscuredPrefs;
			obscuredPrefs = prefsText + "bool: " + ObscuredPrefs.GetBool("gameComplete", false) + "\n";
			prefsText = obscuredPrefs;
			obscuredPrefs = prefsText + "uint: " + ObscuredPrefs.GetUInt("demoUint", 0u) + "\n";
			prefsText = obscuredPrefs;
			obscuredPrefs = prefsText + "long: " + ObscuredPrefs.GetLong("demoLong", -1L) + "\n";
			prefsText = obscuredPrefs;
			obscuredPrefs = prefsText + "double: " + ObscuredPrefs.GetDouble("demoDouble", -1.0) + "\n";
			prefsText = obscuredPrefs;
			obscuredPrefs = string.Concat(prefsText, "Vector2: ", ObscuredPrefs.GetVector2("demoVector2", Vector2.zero), "\n");
			prefsText = obscuredPrefs;
			obscuredPrefs = string.Concat(prefsText, "Vector3: ", ObscuredPrefs.GetVector3("demoVector3", Vector3.zero), "\n");
			prefsText = obscuredPrefs;
			obscuredPrefs = string.Concat(prefsText, "Quaternion: ", ObscuredPrefs.GetQuaternion("demoQuaternion", Quaternion.identity), "\n");
			prefsText = obscuredPrefs;
			obscuredPrefs = string.Concat(prefsText, "Rect: ", ObscuredPrefs.GetRect("demoRect", new Rect(0f, 0f, 0f, 0f)), "\n");
			prefsText = obscuredPrefs;
			obscuredPrefs = string.Concat(prefsText, "Color: ", ObscuredPrefs.GetColor("demoColor", Color.black), "\n");
			prefsText = obscuredPrefs;
			obscuredPrefs = prefsText + "byte[]: {" + array[0] + "," + array[1] + "," + array[2] + "," + array[3] + "}";
		}

		private void SaveObscuredPrefs()
		{
			ObscuredPrefs.SetInt("money", 123);
			ObscuredPrefs.SetFloat("lifeBar", 123.456f);
			ObscuredPrefs.SetString("name", "Goscurry is not a lie ;)");
			ObscuredPrefs.SetBool("gameComplete", true);
			ObscuredPrefs.SetUInt("demoUint", 1234567891u);
			ObscuredPrefs.SetLong("demoLong", 1234567891234567890L);
			ObscuredPrefs.SetDouble("demoDouble", 1.234567890123456);
			ObscuredPrefs.SetVector2("demoVector2", Vector2.one);
			ObscuredPrefs.SetVector3("demoVector3", Vector3.one);
			ObscuredPrefs.SetQuaternion("demoQuaternion", Quaternion.Euler(new Vector3(10f, 20f, 30f)));
			ObscuredPrefs.SetRect("demoRect", new Rect(1.5f, 2.6f, 3.7f, 4.8f));
			ObscuredPrefs.SetColor("demoColor", Color.red);
			ObscuredPrefs.SetByteArray("demoByteArray", new byte[4] { 44, 104, 43, 32 });
			ObscuredPrefs.Save();
		}

		private void DeleteObscuredPrefs()
		{
			ObscuredPrefs.DeleteKey("money");
			ObscuredPrefs.DeleteKey("lifeBar");
			ObscuredPrefs.DeleteKey("name");
			ObscuredPrefs.DeleteKey("gameComplete");
			ObscuredPrefs.DeleteKey("demoUint");
			ObscuredPrefs.DeleteKey("demoLong");
			ObscuredPrefs.DeleteKey("demoDouble");
			ObscuredPrefs.DeleteKey("demoVector2");
			ObscuredPrefs.DeleteKey("demoVector3");
			ObscuredPrefs.DeleteKey("demoQuaternion");
			ObscuredPrefs.DeleteKey("demoRect");
			ObscuredPrefs.DeleteKey("demoColor");
			ObscuredPrefs.DeleteKey("demoByteArray");
			ObscuredPrefs.Save();
		}

		private void ShowHelpButton(string url)
		{
			ShowHelpButton(url, 30);
		}

		private void ShowHelpButton(string url, int width)
		{
			ShowHelpButton(url, "?", width);
		}

		private void ShowHelpButton(string url, string label, int width)
		{
			GUILayoutOption[] array = new GUILayoutOption[1];
			if (width != -1)
			{
				array[0] = GUILayout.Width(width);
			}
			else
			{
				array = null;
			}
			if (GUILayout.Button(label, array))
			{
				OfflineServices.OpenExternalUrl(url);
			}
		}

		private void OnApplicationQuit()
		{
			DeleteRegularPrefs();
			DeleteObscuredPrefs();
		}
	}
}
