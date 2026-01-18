using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIInventorySlotController : UIGameObject
{
	[SerializeField]
	private UIInventorySlotView view;

	[SerializeField]
	private UIItemController itemController;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private InventorySlot Model => view.Model;

	private int Index => view.Index;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		if (MobileUtility.IsMobile)
		{
			itemController.OnSelectUnityEvent.AddListener(Equip);
		}
	}

	private void Equip()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Equip");
		}
		PlayerReferenceManager localPlayerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject();
		int index = Index;
		int inventorySlot = (int)Model.Item.InventorySlot;
		localPlayerObject.Inventory.EquipSlot(index, inventorySlot);
	}
}
