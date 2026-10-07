using System.Collections.Generic;
using System.Reflection;

[DefaultMember("Item")]
public class PlistElementDict : PlistElement
{
	private SortedDictionary<string, PlistElement> m_PrivateValue = new SortedDictionary<string, PlistElement>();

	public IDictionary<string, PlistElement> values
	{
		get
		{
			return GetValues();
		}
	}

	// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
	public PlistElement get_DLKPBAJDHBO(string KGBGENDIMBC)
	{
		return get_Item(KGBGENDIMBC);
	}

	public void set_DLKPBAJDHBO(string KGBGENDIMBC, PlistElement value)
	{
		SetItem(KGBGENDIMBC, value);
	}

	public IDictionary<string, PlistElement> GetValues()
	{
		return m_PrivateValue;
	}

	public new PlistElement get_Item(string KGBGENDIMBC)
	{
		if (GetValues().ContainsKey(KGBGENDIMBC))
		{
			return GetValues()[KGBGENDIMBC];
		}
		return null;
	}

	public new void SetItem(string KGBGENDIMBC, PlistElement value)
	{
		GetValues()[KGBGENDIMBC] = value;
	}

	public void SetInteger(string KGBGENDIMBC, int PKHDLOGJKAD)
	{
		GetValues()[KGBGENDIMBC] = new PlistElementInteger(PKHDLOGJKAD);
	}

	public void SetString(string KGBGENDIMBC, string PKHDLOGJKAD)
	{
		GetValues()[KGBGENDIMBC] = new PlistElementString(PKHDLOGJKAD);
	}

	public void SetBoolean(string KGBGENDIMBC, bool PKHDLOGJKAD)
	{
		GetValues()[KGBGENDIMBC] = new PlistElementBoolean(PKHDLOGJKAD);
	}

	public PlistElementArray CreateArray(string KGBGENDIMBC)
	{
		PlistElementArray gHFPDLCPEBH = new PlistElementArray();
		GetValues()[KGBGENDIMBC] = gHFPDLCPEBH;
		return gHFPDLCPEBH;
	}

	public PlistElementDict CreateDict(string KGBGENDIMBC)
	{
		PlistElementDict jDMGABPEDFI = new PlistElementDict();
		GetValues()[KGBGENDIMBC] = jDMGABPEDFI;
		return jDMGABPEDFI;
	}
}
