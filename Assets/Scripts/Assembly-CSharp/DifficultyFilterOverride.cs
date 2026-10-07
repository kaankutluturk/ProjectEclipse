public class DifficultyFilterOverride
{
	private bool overrideAllDifficulties;

	public bool OverrideAllDifficulties
	{
		get
		{
			return GetOverrideAllDifficulties();
		}
	}

	public bool GetOverrideAllDifficulties()
	{
		return overrideAllDifficulties;
	}
}
