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

	void Verbose(string division, string message);

	void Information(string division, string message);

	void Warning(string division, string message);

	void Error(string division, string message);

	void Exception(string division, string method, Exception ex);
}
