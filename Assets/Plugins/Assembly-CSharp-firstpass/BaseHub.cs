using System.Collections.Generic;
using UnityEngine;

internal class BaseHub : Hub
{
	private string Title;

	private GUIMessageList messages = new GUIMessageList();

	public BaseHub(string name, string title)
		: base(name)
	{
		Title = title;
		On("joined", OnJoined);
		On("rejoined", OnRejoined);
		On("left", OnLeft);
		On("invoked", OnInvoked);
	}

	private void OnJoined(Hub hub, MethodCallMessage methodCall)
	{
		Dictionary<string, object> dictionary = methodCall.GetArguments()[2] as Dictionary<string, object>;
		messages.Add(string.Format("{0} joined at {1}\n\tIsAuthenticated: {2} IsAdmin: {3} UserName: {4}", methodCall.GetArguments()[0], methodCall.GetArguments()[1], dictionary["IsAuthenticated"], dictionary["IsAdmin"], dictionary["UserName"]));
	}

	private void OnRejoined(Hub hub, MethodCallMessage methodCall)
	{
		messages.Add(string.Format("{0} reconnected at {1}", methodCall.GetArguments()[0], methodCall.GetArguments()[1]));
	}

	private void OnLeft(Hub hub, MethodCallMessage methodCall)
	{
		messages.Add(string.Format("{0} left at {1}", methodCall.GetArguments()[0], methodCall.GetArguments()[1]));
	}

	private void OnInvoked(Hub hub, MethodCallMessage methodCall)
	{
		messages.Add(string.Format("{0} invoked hub method at {1}", methodCall.GetArguments()[0], methodCall.GetArguments()[1]));
	}

	public void InvokedFromClient()
	{
		Call("invokedFromClient", OnInvokedFromClientSuccess, OnInvokedFromClientFailed);
	}

	private void OnInvokedFromClientSuccess(Hub hub, ClientMessage clientMessage, ResultMessage resultMessage)
	{
		AdvLog.Log(hub.get_Name() + " invokedFromClient success!");
	}

	private void OnInvokedFromClientFailed(Hub hub, ClientMessage clientMessage, FailureMessage failureMessage)
	{
		AdvLog.LogWarning(hub.get_Name() + " " + failureMessage.GetErrorMessage());
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
