using System.Collections.Generic;

public class Packs
{
	private List<DownloadPack> _packs = new List<DownloadPack>();

	public List<DownloadPack> PackList
	{
		get
		{
			return GetPacks();
		}
	}

	public List<DownloadPack> GetPacks()
	{
		return _packs;
	}

	public void Reset()
	{
		_packs.Clear();
	}

	public void AddPack(string name, string url, string size, bool reload, string checksum, bool attach)
	{
		DownloadPack downloadPack = new DownloadPack();
		downloadPack.Name = name;
		downloadPack.Url = url;
		downloadPack.Size = size;
		downloadPack.Reload = reload;
		downloadPack.Checksum = checksum;
		downloadPack.Attach = attach;
		int result = 0;
		if (int.TryParse(size, out result))
		{
			downloadPack.Size = ((float)result / 1000000f).ToString("#.#");
		}
		downloadPack.SizeBytes = result;
		_packs.Add(downloadPack);
	}

	public DownloadPack FindPack(string name)
	{
		return _packs.Find((DownloadPack entry) => entry.Name.Equals(name));
	}
}
