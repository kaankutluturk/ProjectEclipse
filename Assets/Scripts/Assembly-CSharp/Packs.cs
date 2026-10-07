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

	public void AddPack(string name, string BEPKJNKCKPH, string PEEOEOMEBFG, bool LCDCAKLKHMI, string HDPBNCNCMOH, bool AHDLCJFCJMJ)
	{
		DownloadPack jBKAOMLJCEL = new DownloadPack();
		jBKAOMLJCEL.Name = name;
		jBKAOMLJCEL.Url = BEPKJNKCKPH;
		jBKAOMLJCEL.Size = PEEOEOMEBFG;
		jBKAOMLJCEL.Reload = LCDCAKLKHMI;
		jBKAOMLJCEL.Checksum = HDPBNCNCMOH;
		jBKAOMLJCEL.Attach = AHDLCJFCJMJ;
		int result = 0;
		if (int.TryParse(PEEOEOMEBFG, out result))
		{
			jBKAOMLJCEL.Size = ((float)result / 1000000f).ToString("#.#");
		}
		jBKAOMLJCEL.SizeBytes = result;
		_packs.Add(jBKAOMLJCEL);
	}

	public DownloadPack FindPack(string name)
	{
		return _packs.Find((DownloadPack DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(name));
	}
}
