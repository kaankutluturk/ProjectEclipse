public class CommandForm
{
	public string IDString = string.Empty;

	public bool PostStringMode;

	public CommandForm(string idString, bool postStringMode)
	{
		IDString = idString;
		PostStringMode = postStringMode;
	}
}
