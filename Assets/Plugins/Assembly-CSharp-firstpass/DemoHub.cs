using UnityEngine;

internal class DemoHub : Hub
{
	private float longRunningJobProgress;

	private string longRunningJobStatus = "Not Started!";

	private string fromArbitraryCodeResult = string.Empty;

	private string groupAddedResult = string.Empty;

	private string dynamicTaskResult = string.Empty;

	private string genericTaskResult = string.Empty;

	private string taskWithExceptionResult = string.Empty;

	private string genericTaskWithExceptionResult = string.Empty;

	private string synchronousExceptionResult = string.Empty;

	private string invokingHubMethodWithDynamicResult = string.Empty;

	private string simpleArrayResult = string.Empty;

	private string complexTypeResult = string.Empty;

	private string complexArrayResult = string.Empty;

	private string voidOverloadResult = string.Empty;

	private string intOverloadResult = string.Empty;

	private string readStateResult = string.Empty;

	private string plainTaskResult = string.Empty;

	private string genericTaskWithContinueWithResult = string.Empty;

	private GUIMessageList invokeResults = new GUIMessageList();

	public DemoHub()
		: base("demo")
	{
		On("invoke", Invoke);
		On("signal", Signal);
		On("groupAdded", GroupAdded);
		On("fromArbitraryCode", FromArbitraryCode);
	}

	public void ReportProgress(string EHCLMBADLKH)
	{
		Call("reportProgress", OnLongRunningJob_Done, null, OnLongRunningJob_Progress, EHCLMBADLKH);
	}

	public void OnLongRunningJob_Progress(Hub CGFIJCNNCKP, ClientMessage JBEJKCPHFJP, ProgressMessage progress)
	{
		longRunningJobProgress = (float)progress.GetProgress();
		longRunningJobStatus = progress.GetProgress() + "%";
	}

	public void OnLongRunningJob_Done(Hub CGFIJCNNCKP, ClientMessage BKNEELNMDHH, ResultMessage DCJLKCFKCOM)
	{
		longRunningJobStatus = DCJLKCFKCOM.GetReturnValue().ToString();
		MultipleCalls();
	}

	public void MultipleCalls()
	{
		Call("multipleCalls");
	}

	public void DynamicTask()
	{
		Call("dynamicTask", OnDynamicTask_Done, OnDynamicTask_Failed);
	}

	private void OnDynamicTask_Failed(Hub CGFIJCNNCKP, ClientMessage BKNEELNMDHH, FailureMessage DCJLKCFKCOM)
	{
		dynamicTaskResult = string.Format("The dynamic task failed :( {0}", DCJLKCFKCOM.GetErrorMessage());
	}

	private void OnDynamicTask_Done(Hub CGFIJCNNCKP, ClientMessage BKNEELNMDHH, ResultMessage DCJLKCFKCOM)
	{
		dynamicTaskResult = string.Format("The dynamic task! {0}", DCJLKCFKCOM.GetReturnValue());
	}

	public void AddToGroups()
	{
		Call("addToGroups");
	}

	public void GetValue()
	{
		Call("getValue", (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, ResultMessage DCJLKCFKCOM) =>
		{
			genericTaskResult = string.Format("The value is {0} after 5 seconds", DCJLKCFKCOM.GetReturnValue());
		});
	}

	public void TaskWithException()
	{
		Call("taskWithException", null, (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, FailureMessage JDONBAPIJCG) =>
		{
			taskWithExceptionResult = string.Format("Error: {0}", JDONBAPIJCG.GetErrorMessage());
		});
	}

	public void GenericTaskWithException()
	{
		Call("genericTaskWithException", null, (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, FailureMessage JDONBAPIJCG) =>
		{
			genericTaskWithExceptionResult = string.Format("Error: {0}", JDONBAPIJCG.GetErrorMessage());
		});
	}

	public void SynchronousException()
	{
		Call("synchronousException", null, (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, FailureMessage JDONBAPIJCG) =>
		{
			synchronousExceptionResult = string.Format("Error: {0}", JDONBAPIJCG.GetErrorMessage());
		});
	}

	public void PassingDynamicComplex(object FAFBDKBGDNM)
	{
		Call("passingDynamicComplex", (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, ResultMessage DCJLKCFKCOM) =>
		{
			invokingHubMethodWithDynamicResult = string.Format("The person's age is {0}", DCJLKCFKCOM.GetReturnValue());
		}, FAFBDKBGDNM);
	}

	public void SimpleArray(int[] HFPDMGAEJJE)
	{
		Call("simpleArray", (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, ResultMessage DCJLKCFKCOM) =>
		{
			simpleArrayResult = "Simple array works!";
		}, HFPDMGAEJJE);
	}

	public void ComplexType(object FAFBDKBGDNM)
	{
		Call("complexType", (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, ResultMessage DCJLKCFKCOM) =>
		{
			complexTypeResult = string.Format("Complex Type -> {0}", ((IHub)this).HubConnection.GetJsonEncoder().Encode(GetState()["person"]));
		}, FAFBDKBGDNM);
	}

	public void ComplexArray(object[] OEOJFDNOEAO)
	{
		Call("ComplexArray", (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, ResultMessage DCJLKCFKCOM) =>
		{
			complexArrayResult = "Complex Array Works!";
		}, new object[1] { OEOJFDNOEAO });
	}

	public void Overload()
	{
		Call("Overload", OnVoidOverload_Done);
	}

	private void OnVoidOverload_Done(Hub CGFIJCNNCKP, ClientMessage BKNEELNMDHH, ResultMessage DCJLKCFKCOM)
	{
		voidOverloadResult = "Void Overload called";
		Overload(101);
	}

	public void Overload(int number)
	{
		Call("Overload", OnIntOverload_Done, number);
	}

	private void OnIntOverload_Done(Hub CGFIJCNNCKP, ClientMessage BKNEELNMDHH, ResultMessage DCJLKCFKCOM)
	{
		intOverloadResult = string.Format("Overload with return value called => {0}", DCJLKCFKCOM.GetReturnValue().ToString());
	}

	public void ReadStateValue()
	{
		Call("readStateValue", (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, ResultMessage DCJLKCFKCOM) =>
		{
			readStateResult = string.Format("Read some state! => {0}", DCJLKCFKCOM.GetReturnValue());
		});
	}

	public void PlainTask()
	{
		Call("plainTask", (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, ResultMessage DCJLKCFKCOM) =>
		{
			plainTaskResult = "Plain Task Result";
		});
	}

	public void GenericTaskWithContinueWith()
	{
		Call("genericTaskWithContinueWith", (Hub CGFIJCNNCKP, ClientMessage CKEHOEGLMBM, ResultMessage DCJLKCFKCOM) =>
		{
			genericTaskWithContinueWithResult = DCJLKCFKCOM.GetReturnValue().ToString();
		});
	}

	private void FromArbitraryCode(Hub CGFIJCNNCKP, MethodCallMessage BOPGDKGIGHM)
	{
		fromArbitraryCodeResult = BOPGDKGIGHM.GetArguments()[0] as string;
	}

	private void GroupAdded(Hub CGFIJCNNCKP, MethodCallMessage BOPGDKGIGHM)
	{
		if (!string.IsNullOrEmpty(groupAddedResult))
		{
			groupAddedResult = "Group Already Added!";
		}
		else
		{
			groupAddedResult = "Group Added!";
		}
	}

	private void Signal(Hub CGFIJCNNCKP, MethodCallMessage BOPGDKGIGHM)
	{
		dynamicTaskResult = string.Format("The dynamic task! {0}", BOPGDKGIGHM.GetArguments()[0]);
	}

	private void Invoke(Hub CGFIJCNNCKP, MethodCallMessage BOPGDKGIGHM)
	{
		invokeResults.Add(string.Format("{0} client state index -> {1}", BOPGDKGIGHM.GetArguments()[0], GetState()["index"]));
	}

	public void Draw()
	{
		GUILayout.Label("Arbitrary Code");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(string.Format("Sending {0} from arbitrary code without the hub itself!", fromArbitraryCodeResult));
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Group Added");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(groupAddedResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Dynamic Task");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(dynamicTaskResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Report Progress");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.BeginVertical();
		GUILayout.Label(longRunningJobStatus);
		GUILayout.HorizontalSlider(longRunningJobProgress, 0f, 100f);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Generic Task");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(genericTaskResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Task With Exception");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(taskWithExceptionResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Generic Task With Exception");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(genericTaskWithExceptionResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Synchronous Exception");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(synchronousExceptionResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Invoking hub method with dynamic");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(invokingHubMethodWithDynamicResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Simple Array");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(simpleArrayResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Complex Type");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(complexTypeResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Complex Array");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(complexArrayResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Overloads");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.BeginVertical();
		GUILayout.Label(voidOverloadResult);
		GUILayout.Label(intOverloadResult);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Read State Value");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(readStateResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Plain Task");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(plainTaskResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Generic Task With ContinueWith");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		GUILayout.Label(genericTaskWithContinueWithResult);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("Message Pump");
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		invokeResults.Draw(Screen.width - 40, 270f);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
	}
}
