using System.Collections.Generic;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nekki.SF2.Core.Tutorials
{
	public class TutorialAction : SFMonoBehaviour<object>
	{
		public enum TutorialActionEvent
		{
			ACTION_EVENT_ON_COMPLETE = 0
		}

		protected Scene currentScene;

		private GameObject highlightObject;

		private List<GameObject> spawnedObjects = new List<GameObject>();

		private List<global::Pair<GameObject, int>> trackedObjects;

		public virtual void Run()
		{
			currentScene = Module.GetInstance().GetCurrentScene();
		}

		protected virtual bool CanRun()
		{
			return true;
		}

		protected void CompleteAction(int resultCode = 0)
		{
			CallEvent(0, resultCode);
		}

		protected void SetButtonHighlight(Button button, bool highlight)
		{
			if (!highlight)
			{
			}
		}

		protected void DestroyHighlight()
		{
			Object.Destroy(highlightObject);
		}

		protected void HighlightTarget(GameObject target, float offsetX, float offsetY, float offsetZ = 0f)
		{
		}

		protected void DestroySpawnedObjects()
		{
			foreach (GameObject item in spawnedObjects)
			{
				Object.Destroy(item);
			}
			spawnedObjects.Clear();
		}

		protected void TrackObject(GameObject trackedObject)
		{
			global::Pair<GameObject, int> item = new global::Pair<GameObject, int>(trackedObject, 0);
			trackedObjects.Add(item);
		}
	}
}
