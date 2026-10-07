using Nekki.Yaml;
using UnityEngine;

public class YamlUtils
{
	public static Vector2 ParseVector2(Sequence sequence)
	{
		Vector2 vector = default(Vector2);
		vector = Vector2.zero;
		if (sequence != null)
		{
			AdvLog.Log(string.Format("<{0}>", sequence.GetType()));
			vector.x = float.Parse(((Nekki.Yaml.Scalar)sequence.nodesInside[0]).text);
			vector.y = float.Parse(((Nekki.Yaml.Scalar)sequence.nodesInside[1]).text);
		}
		return vector;
	}
}
