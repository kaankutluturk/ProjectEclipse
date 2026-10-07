using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

public class TacticsArchiver
{
	public static void BuildAllArchives()
	{
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "ComputerSettings.xml");
		XmlNode xmlNode = xmlDocument["Settings"]["OutcomeTables"]["Items"]["Weapons"];
		List<string> list = new List<string>();
		list.Add(string.Empty);
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (childNode.Name == "Weapon")
			{
				string item = childNode.Attributes["TacticWeapon"].GetStringOrDefault(string.Empty);
				list.Add(item);
			}
		}
		foreach (string item2 in list)
		{
			BuildArchive(item2);
			foreach (string item3 in list)
			{
				BuildArchive(item2, item3);
			}
		}
	}

	private static void BuildArchive(string weaponName)
	{
		byte[] archiveBytes = new byte[0];
		AddTable(weaponName, ref archiveBytes);
		if (archiveBytes.Length > 0)
		{
			string archivePath = string.Empty;
			GetFileName(weaponName, ref archivePath);
			File.WriteAllBytes(archivePath, Compressor.Compress(archiveBytes));
		}
	}

	private static void BuildArchive(string firstWeapon, string secondWeapon)
	{
		if (IsWeaponOrder(firstWeapon, secondWeapon))
		{
			byte[] archiveBytes = new byte[0];
			AddTable(firstWeapon, secondWeapon, ref archiveBytes);
			if (firstWeapon != secondWeapon)
			{
				AddTable(secondWeapon, firstWeapon, ref archiveBytes);
			}
			if (archiveBytes.Length > 0)
			{
				string archivePath = string.Empty;
				GetFileName(firstWeapon, secondWeapon, ref archivePath);
				File.WriteAllBytes(archivePath, Compressor.Compress(archiveBytes));
			}
		}
	}

	private static void LoadDefaultArchives()
	{
		List<string> list = new List<string>();
		list.Add(string.Empty);
		list.Add("Fists");
		foreach (string item in list)
		{
			foreach (string item2 in list)
			{
				LoadArchive(item, item2);
			}
		}
	}

	public static void LoadArchive(string weaponName)
	{
		string archivePath = string.Empty;
		if (weaponName == string.Empty)
		{
			weaponName = "default";
		}
		GetFileName(weaponName, ref archivePath);
		byte[] array = ResourceManager.GetBinary(archivePath);
		if (array != null && array.Length > 0)
		{
			byte[] buffer = Compressor.Decompress(array);
			using (MemoryStream input = new MemoryStream(buffer))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
					{
						AiData.TableType tableType = (AiData.TableType)binaryReader.ReadUInt32();
						string text = TacticalTableHolder.ReadNullTerminatedString(binaryReader);
						if (tableType != AiData.TableType.shiftTable)
						{
							uint num = binaryReader.ReadUInt32();
							if (0 < num)
							{
								byte[] tableBytes = binaryReader.ReadBytes((int)num);
								TacticalTableHolder tableHolder = new TacticalTableHolder();
								tableHolder.Load(tableBytes, (int)tableType, weaponName);
								AiData.AddTableHolder(tableHolder, text, text, tableType);
							}
						}
						else
						{
							if (tableType != AiData.TableType.shiftTable)
							{
								continue;
							}
							uint num2 = binaryReader.ReadUInt32();
							if (0 >= num2)
							{
								continue;
							}
							byte[] buffer2 = binaryReader.ReadBytes((int)num2);
							using (MemoryStream input2 = new MemoryStream(buffer2))
							{
								using (BinaryReader binaryReader2 = new BinaryReader(input2))
								{
									uint num3 = binaryReader2.ReadUInt32();
									for (int i = 0; i < num3; i++)
									{
										string animationName = TacticalTableHolder.ReadNullTerminatedString(binaryReader2);
										InfoAnimation animation = AnimationData.GetAnimationByName(animationName);
										if (animation != null)
										{
											animation.ShiftTable.LoadFromFile(animation, binaryReader2);
											continue;
										}
										ModelShiftTable animationShiftTable = new ModelShiftTable();
										animationShiftTable.LoadFromFile(null, binaryReader2);
									}
								}
							}
						}
					}
				}
			}
		}
		else
		{
			GameLog.Write("file {0} not unzip", archivePath);
		}
		// Several archives load together at a round boundary. Do not force a
		// full-heap collection after each one; the runtime schedules collection.
	}

	public static void LoadArchive(string firstWeapon, string secondWeapon)
	{
		if (!IsWeaponOrder(firstWeapon, secondWeapon))
		{
			return;
		}
		string archivePath = string.Empty;
		GetFileName(firstWeapon, secondWeapon, ref archivePath);
		byte[] array = ResourceManager.GetBinary(archivePath);
		if (array != null && array.Length > 0)
		{
			byte[] buffer = Compressor.Decompress(array);
			using (MemoryStream input = new MemoryStream(buffer))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
					{
						AiData.TableType tableType = (AiData.TableType)binaryReader.ReadUInt32();
						if (AiData.CheckIfTableExists(firstWeapon, secondWeapon, tableType))
						{
							GameLog.Write("Skipping - table {0}/{1} !", firstWeapon, secondWeapon);
							return;
						}
						GameLog.Write("Reading - table {0}/{1} !", firstWeapon, secondWeapon);
						string tableName = TacticalTableHolder.ReadNullTerminatedString(binaryReader);
						string text = TacticalTableHolder.ReadNullTerminatedString(binaryReader);
						byte[] tableBytes = new byte[0];
						uint num = binaryReader.ReadUInt32();
						if (0 < num)
						{
							tableBytes = binaryReader.ReadBytes((int)num);
						}
						TacticalTableHolder tableHolder = new TacticalTableHolder();
						tableHolder.Load(tableBytes, (int)tableType, text);
						AiData.AddTableHolder(tableHolder, tableName, text, tableType);
					}
				}
			}
		}
		else
		{
			GameLog.Write("file {0} not unzip", archivePath);
		}
	}

	private static void AddTable(string weaponName, ref byte[] archiveBytes)
	{
		string dodgeTablePath = SF2Paths.GetGameDataPath() + "/tactics/dodge/" + weaponName + ".tbs";
		string resourcePath = SF2Paths.GetGameDataPath() + "/tactics/shiftTables/" + weaponName + ".sts";
		byte[] array = ResourceManager.GetBinary(dodgeTablePath);
		if (array != null && array.Length > 0)
		{
			archiveBytes = Concat(archiveBytes, BitConverter.GetBytes(2u));
			archiveBytes = Concat(archiveBytes, Encoding.ASCII.GetBytes(weaponName));
			archiveBytes = Concat(archiveBytes, array);
		}
		byte[] array2 = ResourceManager.GetBinary(resourcePath);
		if (array2 != null && array2.Length > 0)
		{
			archiveBytes = Concat(archiveBytes, BitConverter.GetBytes(7u));
			archiveBytes = Concat(archiveBytes, Encoding.ASCII.GetBytes(weaponName));
			archiveBytes = Concat(archiveBytes, array2);
		}
	}

	private static void AddTable(string firstWeapon, string secondWeapon, ref byte[] archiveBytes)
	{
		string text = firstWeapon + "_" + secondWeapon + ".tbs";
		string tablePath = SF2Paths.GetGameDataPath() + "/tactics/movements/" + text;
		string resourcePath = SF2Paths.GetGameDataPath() + "/tactics/outcometablesforattack/" + text;
		byte[] array = ResourceManager.GetBinary(tablePath);
		if (array != null && array.Length > 0)
		{
			archiveBytes = Concat(archiveBytes, BitConverter.GetBytes(1u));
			archiveBytes = Concat(archiveBytes, Encoding.ASCII.GetBytes(firstWeapon));
			archiveBytes = Concat(archiveBytes, Encoding.ASCII.GetBytes(secondWeapon));
			archiveBytes = Concat(archiveBytes, array);
		}
		byte[] array2 = ResourceManager.GetBinary(resourcePath);
		if (array2 != null && array2.Length > 0)
		{
			archiveBytes = Concat(archiveBytes, BitConverter.GetBytes(0u));
			archiveBytes = Concat(archiveBytes, Encoding.ASCII.GetBytes(firstWeapon));
			archiveBytes = Concat(archiveBytes, Encoding.ASCII.GetBytes(secondWeapon));
			archiveBytes = Concat(archiveBytes, array2);
		}
	}

	private static bool IsWeaponOrder(string firstWeapon, string secondWeapon)
	{
		return firstWeapon == secondWeapon || firstWeapon == string.Empty || (secondWeapon != string.Empty && string.Compare(firstWeapon, secondWeapon) < 0);
	}

	private static int GetFileName(string weaponName, ref string archivePath)
	{
		archivePath = SF2Paths.GetGameDataPath() + "/tactics_compressed/" + weaponName.ToLower() + ".atf";
		return archivePath.Length;
	}

	private static int GetFileName(string firstWeapon, string secondWeapon, ref string archivePath)
	{
		archivePath = SF2Paths.GetGameDataPath() + "/tactics_compressed/" + firstWeapon.ToLower() + "_" + secondWeapon.ToLower() + ".atf";
		return archivePath.Length;
	}

	private static byte[] Concat(byte[] first, byte[] second)
	{
		byte[] array = new byte[first.Length + second.Length];
		first.CopyTo(array, 0);
		second.CopyTo(array, first.Length);
		return array;
	}
}
