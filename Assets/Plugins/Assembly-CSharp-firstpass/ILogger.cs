using System;

public interface ILogger
{
	Loglevels Level { get; set; }

	string FormatVerbose { get; set; }

	string FormatInfo { get; set; }

	string FormatWarn { get; set; }

	string FormatErr { get; set; }

	string FormatEx { get; set; }

	Loglevels GetLevel();

	void SetLevel(Loglevels value);

	string GetFormatVerbose();

	void SetFormatVerbose(string value);

	string GetFormatInfo();

	void SetFormatInfo(string value);

	string GetFormatWarn();

	void SetFormatWarn(string value);

	string GetFormatErr();

	void SetFormatErr(string value);

	string GetFormatEx();

	void SetFormatEx(string value);

	void Verbose(string HMHPCGBCNGI, string POOAFNBCFHM);

	void Information(string HMHPCGBCNGI, string EMBBNNBFODN);

	void Warning(string HMHPCGBCNGI, string EPMNBLHHAHF);

	void Error(string HMHPCGBCNGI, string KEPBNIIECPN);

	void Exception(string HMHPCGBCNGI, string CKEHOEGLMBM, Exception MPFFFAOGBJE);
}
