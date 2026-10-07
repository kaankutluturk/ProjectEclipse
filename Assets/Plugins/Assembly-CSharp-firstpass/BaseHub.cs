using System.Collections.Generic;
using UnityEngine;

internal class BaseHub : Hub
{
	private string Title;

	private GUIMessageList messages = new GUIMessageList();

	public BaseHub(string name, string PEMOECLNECD)
		: base(name)
	{
		Title = PEMOECLNECD;
		On("joined", OnJoined);
		On("rejoined", OnRejoined);
		On("left", OnLeft);
		On("invoked", OnInvoked);
	}

	private void OnJoined(Hub CGFIJCNNCKP, MethodCallMessage BOPGDKGIGHM)
	{
		Dictionary<string, object> dictionary = BOPGDKGIGHM.GetArguments()[2] as Dictionary<string, object>;
		messages.Add(string.Format("{0} joined at {1}\n\tIsAuthenticated: {2} IsAdmin: {3} UserName: {4}", BOPGDKGIGHM.GetArguments()[0], BOPGDKGIGHM.GetArguments()[1], dictionary["IsAuthenticated"], dictionary["IsAdmin"], dictionary["UserName"]));
	}

	private void OnRejoined(Hub CGFIJCNNCKP, MethodCallMessage BOPGDKGIGHM)
	{
		messages.Add(string.Format("{0} reconnected at {1}", BOPGDKGIGHM.GetArguments()[0], BOPGDKGIGHM.GetArguments()[1]));
	}

	private void OnLeft(Hub CGFIJCNNCKP, MethodCallMessage BOPGDKGIGHM)
	{
		messages.Add(string.Format("{0} left at {1}", BOPGDKGIGHM.GetArguments()[0], BOPGDKGIGHM.GetArguments()[1]));
	}

	private void OnInvoked(Hub CGFIJCNNCKP, MethodCallMessage BOPGDKGIGHM)
	{
		messages.Add(string.Format("{0} invoked hub method at {1}", BOPGDKGIGHM.GetArguments()[0], BOPGDKGIGHM.GetArguments()[1]));
	}

	public void InvokedFromClient()
	{
		Call("invokedFromClient", OnInvokedFromClientSuccess, OnInvokedFromClientFailed);
	}

	private void OnInvokedFromClientSuccess(Hub CGFIJCNNCKP, ClientMessage BKNEELNMDHH, ResultMessage DCJLKCFKCOM)
	{
		AdvLog.Log(CGFIJCNNCKP.get_Name() + " invokedFromClient success!");
	}

	private void OnInvokedFromClientFailed(Hub CGFIJCNNCKP, ClientMessage BKNEELNMDHH, FailureMessage DCJLKCFKCOM)
	{
		AdvLog.LogWarning(CGFIJCNNCKP.get_Name() + " " + DCJLKCFKCOM.GetErrorMessage());
	}

	public void Draw()
	{
		GUILayout.Label(Title);
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		messages.Draw(Screen.width - 20, 100f);
		GUILayout.EndHorizontal();
	}
}
