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

	private void Echo(Hub CGFIJCNNCKP, MethodCallMessage BOPGDKGIGHM)
	{
		typedEchoClientResult = string.Format("{0} #{1} triggered!", BOPGDKGIGHM.GetArguments()[0], BOPGDKGIGHM.GetArguments()[1]);
	}

	public void Echo(string CKEHOEGLMBM)
	{
		Call("echo", OnEchoDone, CKEHOEGLMBM);
	}

	private void OnEchoDone(Hub CGFIJCNNCKP, ClientMessage BKNEELNMDHH, ResultMessage DCJLKCFKCOM)
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
