using System.Collections.Generic;

public class PlistElementArray : PlistElement
{
	public List<PlistElement> values = new List<PlistElement>();

	public void AddString(string PKHDLOGJKAD)
	{
		values.Add(new PlistElementString(PKHDLOGJKAD));
	}

	public void AddInteger(int PKHDLOGJKAD)
	{
		values.Add(new PlistElementInteger(PKHDLOGJKAD));
	}

	public void AddBoolean(bool PKHDLOGJKAD)
	{
		values.Add(new PlistElementBoolean(PKHDLOGJKAD));
	}

	public PlistElementArray AddArray()
	{
		PlistElementArray gHFPDLCPEBH = new PlistElementArray();
		values.Add(gHFPDLCPEBH);
		return gHFPDLCPEBH;
	}

	public PlistElementDict AddDict()
	{
		PlistElementDict jDMGABPEDFI = new PlistElementDict();
		values.Add(jDMGABPEDFI);
		return jDMGABPEDFI;
	}
}
