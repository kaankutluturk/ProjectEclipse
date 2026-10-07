using UnityEngine;

internal class TypedDemoHub : Hub
{
	private string typedEchoResult = string.Empty;

	private string typedEchoClientResult = string.Empty;

	public TypedDemoHub()
		: base("typeddemohub")
	{
		On("Echo", Echo);
	}

	private void Echo(Hub hub, MethodCallMessage message)
	{
		typedEchoClientResult = string.Format("{0} #{1} triggered!", message.GetArguments()[0], message.GetArguments()[1]);
	}

	public void Echo(string text)
	{
		Call("echo", OnEchoDone, text);
	}

	private void OnEchoDone(Hub hub, ClientMessage originalMessage, ResultMessage resultMessage)
	{
		typedEchoResult = "TypedDemoHub.Echo(string message) invoked!";
	}

	public void Draw()
	{
		GUILayout.Label("Typed callback");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.BeginVertical();
		GUILayout.Label(typedEchoResult);
		GUILayout.Label(typedEchoClientResult);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
	}
}
