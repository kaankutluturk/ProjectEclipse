using System.Text;

public static class QuestTextResolver
{
	public static string ResolveText(string template, QuestParameters parameters)
	{
		int num = template.IndexOf('{');
		if (num == -1)
		{
			return template;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(template);
		int num2 = template.LastIndexOf('}');
		if (num2 == -1)
		{
			return stringBuilder.ToString();
		}
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		while (num <= num2)
		{
			string newValue = string.Empty;
			if (template[num].Equals('{'))
			{
				int num3 = template.IndexOf('}', num);
				string text = template.Substring(num + 1, num3 - num - 1);
				if (!text.Equals(string.Empty))
				{
					result.Clear();
					condition.SetValue(text, result);
					newValue = result.ToString();
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
