public static class ModelType
{
	public enum ModelTargetType
	{
		MODEL_NULL = 0,
		MODEL_THIS = 1,
		MODEL_OTHER = 2,
		MODEL_PARENT = 3,
		MODEL_CHILD = 4,
		MODEL_BOTH = 5,
		MODEL_OTHER_CHILD = 6
	}

	public static ModelTargetType ParseTargetType(string typeName)
	{
		switch (typeName)
		{
		case "Me":
			return ModelTargetType.MODEL_THIS;
		case "Enemy":
			return ModelTargetType.MODEL_OTHER;
		case "Parent":
			return ModelTargetType.MODEL_PARENT;
		case "Both":
			return ModelTargetType.MODEL_BOTH;
		case "Null":
			return ModelTargetType.MODEL_NULL;
		case "Child":
			return ModelTargetType.MODEL_CHILD;
		case "EnemyChild":
			return ModelTargetType.MODEL_OTHER_CHILD;
		default:
			GameLog.Error("ModelType - parseType - unknownType: " + typeName);
			return ModelTargetType.MODEL_NULL;
		}
	}
}
