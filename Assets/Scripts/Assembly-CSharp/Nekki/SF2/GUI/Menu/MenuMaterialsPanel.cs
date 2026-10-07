using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Menu
{
	public class MenuMaterialsPanel : MonoBehaviour
	{
		public enum MenuMaterialsPanelEvent
		{
			onMaterialsBtnClicked = 0
		}

		private enum MenuMaterialsPanelLayer
		{
			zContent = 0
		}

		[SerializeField]
		private GameObject MaterialPrefab;

		private List<MenuMaterSprite> materialSprites = new List<MenuMaterSprite>();

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void Init()
		{
			CreateMaterialSprites();
		}

		private void RemoveListeners()
		{
		}

		public void UpdateView()
		{
			for (int i = 0; i < materialSprites.Count; i++)
			{
				MenuMaterSprite menuMaterSprite = materialSprites[i];
				menuMaterSprite.UpdateView();
			}
		}

		private void CreateMaterialSprites()
		{
			foreach (Transform item in base.transform)
			{
				Object.Destroy(item.gameObject);
			}
			List<GameCurrency> list = GameUtils.GameCurrencies.GetCurrencies();
			if (list.Count == 0)
			{
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				GameCurrency cJJOFMHLFFM = list[i];
				if (cJJOFMHLFFM.Group == GameCurrency.CurrencyGroup.CURRENCY_GROUP_FORGE)
				{
					GameObject gameObject = Object.Instantiate(MaterialPrefab);
					MenuMaterSprite component = gameObject.GetComponent<MenuMaterSprite>();
					// RectTransform.parent preserves world-space scale and position, which
					// makes these UI entries oversized or offset under a scaled Canvas.
					component.transform.SetParent(base.transform, false);
					component.Init(cJJOFMHLFFM);
					materialSprites.Add(component);
				}
			}
		}
	}
}
