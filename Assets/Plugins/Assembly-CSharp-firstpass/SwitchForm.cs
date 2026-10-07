public class SwitchForm
{
	public string IDString;

	public SwitchType Type;

	public bool Multi;

	public int MinLen;

	public int MaxLen;

	public string PostCharSet;

	public SwitchForm(string FBEJCDFPDLD, SwitchType LFLGCDNKNJI, bool IJMDFIKBJAG, int GNKCLPKOEBL, int FJLKBBJCLHD, string PHBJBABMEPL)
	{
		IDString = FBEJCDFPDLD;
		Type = LFLGCDNKNJI;
		Multi = IJMDFIKBJAG;
		MinLen = GNKCLPKOEBL;
		MaxLen = FJLKBBJCLHD;
		PostCharSet = PHBJBABMEPL;
	}

	public SwitchForm(string FBEJCDFPDLD, SwitchType LFLGCDNKNJI, bool IJMDFIKBJAG, int GNKCLPKOEBL)
		: this(FBEJCDFPDLD, LFLGCDNKNJI, IJMDFIKBJAG, GNKCLPKOEBL, 0, string.Empty)
	{
	}

	public SwitchForm(string FBEJCDFPDLD, SwitchType LFLGCDNKNJI, bool IJMDFIKBJAG)
		: this(FBEJCDFPDLD, LFLGCDNKNJI, IJMDFIKBJAG, 0)
	{
	}
}
