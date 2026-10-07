using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

public class Compressor
{
	public static void Compress(List<string> filePaths, string outputPath)
	{
		if (filePaths == null || filePaths.Count == 0)
		{
			return;
		}
		CustomBinaryWriter writer = new CustomBinaryWriter();
		foreach (string item in filePaths)
		{
			writer.Write(Path.GetFileName(item));
			writer.Write(File.ReadAllText(item));
		}
		byte[] array = writer.ToArray();
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (DeflateStream deflateStream = new DeflateStream(memoryStream, System.IO.Compression.CompressionMode.Compress))
			{
				deflateStream.Write(array, 0, array.Length);
			}
			byte[] bytes = memoryStream.ToArray();
			File.WriteAllBytes(outputPath, bytes);
		}
	}

	public static void Uncompress(string archivePath, string outputDirectory = "")
	{
		using (MemoryStream archiveStream = new MemoryStream(File.ReadAllBytes(archivePath)))
		{
			MemoryStream memoryStream = new MemoryStream();
			using (DeflateStream deflateStream = new DeflateStream(archiveStream, System.IO.Compression.CompressionMode.Decompress))
			{
				byte[] array = new byte[2048];
				int num = 0;
				while ((num = deflateStream.Read(array, 0, array.Length)) > 0)
				{
					memoryStream.Write(array, 0, num);
				}
				memoryStream.Position = 0L;
				StreamReader streamReader = new StreamReader(memoryStream);
				bool flag = false;
				while (!flag)
				{
					string text = streamReader.ReadLine();
					string text2 = streamReader.ReadLine();
					if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
					{
						flag = true;
						continue;
					}
					string path = outputDirectory + text;
					File.WriteAllText(path, text2);
				}
			}
		}
	}

	public static byte[] Compress(byte[] data)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (DeflateStream defl = new DeflateStream(memoryStream, System.IO.Compression.CompressionMode.Compress))
			{
				defl.Write(data, 0, data.Length);
			}
			return memoryStream.ToArray();
		}
	}

	public static byte[] Decompress(byte[] data)
	{
		try
		{
			using (MemoryStream input = new MemoryStream(data))
			using (DeflateStream defl = new DeflateStream(input, System.IO.Compression.CompressionMode.Decompress))
			using (MemoryStream output = new MemoryStream())
			{
				byte[] array = new byte[4096];
				int num;
				while ((num = defl.Read(array, 0, array.Length)) > 0)
				{
					output.Write(array, 0, num);
				}
				return output.ToArray();
			}
		}
		catch
		{
			return data;
		}
	}
}