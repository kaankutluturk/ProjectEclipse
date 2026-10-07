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

		protected void CompleteAction(int BNPIIOAIBGN = 0)
		{
			CallEvent(0, BNPIIOAIBGN);
		}

		protected void SetButtonHighlight(Button KLNKEPMAGKF, bool KOHDJNFJLGH)
		{
			if (!KOHDJNFJLGH)
			{
			}
		}

		protected void DestroyHighlight()
		{
			Object.Destroy(highlightObject);
		}

		protected void HighlightTarget(GameObject target, float KDGOIIIHPCL, float AMKFJMOMNNB, float DOBNKCHMKGE = 0f)
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

		protected void TrackObject(GameObject GBIOHMNNEJI)
		{
			global::Pair<GameObject, int> item = new global::Pair<GameObject, int>(GBIOHMNNEJI, 0);
			trackedObjects.Add(item);
		}
	}
}
