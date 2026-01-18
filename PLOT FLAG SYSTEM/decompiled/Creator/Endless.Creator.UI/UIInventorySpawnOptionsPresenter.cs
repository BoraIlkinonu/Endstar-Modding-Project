using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInventorySpawnOptionsPresenter : UIBasePresenter<InventorySpawnOptions>
{
	[Header("UIInventorySpawnOptionsPresenter")]
	[SerializeField]
	private UIInventorySpawnOptionsModalView inventorySpawnOptionsModalSource;

	protected override void Start()
	{
		base.Start();
		(base.View.Interface as UIInventorySpawnOptionsView).OnEditPressed += DisplayInventorySpawnOptionsModal;
	}

	private void DisplayInventorySpawnOptionsModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayInventorySpawnOptionsModal");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(inventorySpawnOptionsModalSource, UIModalManagerStackActions.ClearStack, this);
	}
}
