public static class QuestUtils
{
	private static DifficultyFilterOverride difficultyOptions = new DifficultyFilterOverride();

	private static NoAnimationMove noAnimationMoves = new NoAnimationMove();

	public static DifficultyFilterOverride DifficultyOptions
	{
		get
		{
			return GetDifficultyOptions();
		}
	}

	public static NoAnimationMove NoAnimationMoves
	{
		get
		{
			return GetNoAnimationMoves();
		}
	}

	public static DifficultyFilterOverride GetDifficultyOptions()
	{
		return difficultyOptions;
	}

	public static NoAnimationMove GetNoAnimationMoves()
	{
		return noAnimationMoves;
	}
}
