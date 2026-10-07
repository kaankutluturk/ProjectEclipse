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
			LoadingModule pHNHABBBKKL = _modules[_currentModuleIndex];
			if (!pHNHABBBKKL.IsLoadingActive())
			{
				pHNHABBBKKL.Start();
			}
			if (!pHNHABBBKKL.IsFinished())
			{
				pHNHABBBKKL.ProcessStep();
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

	public virtual void AddModule(LoadingModule ENJECLFOHLD)
	{
		_modules.AddIfNotExist(ENJECLFOHLD);
	}

	public virtual void AddModules(List<LoadingModule> CBLIEGAIBLP)
	{
		_modules.AddIfNotExist(CBLIEGAIBLP);
	}

	public virtual void ClearModules(bool LJCIEGKNKGG = false)
	{
		_modules.Clear();
	}
}
