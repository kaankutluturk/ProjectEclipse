using System.Text;

public static class QuestTextResolver
{
	public static string ResolveText(string PMDPPGNJAFE, QuestParameters GFIHPBCEEOB)
	{
		int num = PMDPPGNJAFE.IndexOf('{');
		if (num == -1)
		{
			return PMDPPGNJAFE;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(PMDPPGNJAFE);
		int num2 = PMDPPGNJAFE.LastIndexOf('}');
		if (num2 == -1)
		{
			return stringBuilder.ToString();
		}
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		while (num <= num2)
		{
			string newValue = string.Empty;
			if (PMDPPGNJAFE[num].Equals('{'))
			{
				int num3 = PMDPPGNJAFE.IndexOf('}', num);
				string text = PMDPPGNJAFE.Substring(num + 1, num3 - num - 1);
				if (!text.Equals(string.Empty))
				{
					lNIDLHOIHIM.Clear();
					kKDGLNECFHA.SetValue(text, lNIDLHOIHIM);
					newValue = lNIDLHOIHIM.ToString();
				}
				int startIndex = stringBuilder.ToString().IndexOf(text);
				stringBuilder.Replace(text, newValue, startIndex, text.Length);
				num = num3 + 1;
			}
			else
			{
				num++;
			}
		}
		return stringBuilder.ToString();
	}
}
