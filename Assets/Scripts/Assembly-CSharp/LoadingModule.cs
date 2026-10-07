using System.Collections.Generic;

public class LoadingModule
{
	private List<LoadingModule> _modules = new List<LoadingModule>();

	private int _currentModuleIndex;

	protected bool isFinished;

	private bool isRunning;

	public LoadingModule()
	{
		isFinished = false;
		isRunning = false;
		_currentModuleIndex = 0;
	}

	public virtual void Start()
	{
		isFinished = false;
		isRunning = true;
		_currentModuleIndex = 0;
	}

	public virtual void Stop()
	{
		isRunning = false;
	}

	public virtual bool IsFinished()
	{
		return isFinished;
	}

	public virtual bool IsLoadingActive()
	{
		return isRunning;
	}

	public virtual bool IsEmpty()
	{
		return _modules.Count == 0;
	}

	public virtual void ProcessStep()
	{
		if (!IsLoadingActive())
		{
			return;
		}
		if (_currentModuleIndex < _modules.Count)
		{
			LoadingModule currentModule = _modules[_currentModuleIndex];
			if (!currentModule.IsLoadingActive())
			{
				currentModule.Start();
			}
			if (!currentModule.IsFinished())
			{
				currentModule.ProcessStep();
			}
			else
			{
				_currentModuleIndex++;
			}
		}
		else
		{
			isFinished = true;
		}
	}

	public virtual void AddModule(LoadingModule module)
	{
		_modules.AddIfNotExist(module);
	}

	public virtual void AddModules(List<LoadingModule> modules)
	{
		_modules.AddIfNotExist(modules);
	}

	public virtual void ClearModules(bool clearActive = false)
	{
		_modules.Clear();
	}
}
