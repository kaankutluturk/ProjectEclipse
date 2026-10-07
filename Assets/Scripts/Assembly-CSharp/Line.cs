using UnityEngine.Events;

public interface Line
{
	void StartAnimation();

	void FinishAnimation();

	void AddListener(UnityAction listener);

	void RemoveListener(UnityAction listener);
}
