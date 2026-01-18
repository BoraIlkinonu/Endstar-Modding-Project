using UnityEngine;

namespace Endless.Gameplay;

public class ItemInteractable : InteractableBase
{
	[SerializeField]
	private Item item;

	protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
	{
		PlayerInteractor playerInteractor = (PlayerInteractor)interactor;
		if (playerInteractor != null && item.IsPickupable)
		{
			if (base.AttemptInteract_ServerLogic(interactor, colliderIndex))
			{
				return playerInteractor.PlayerReferenceManager.Inventory.AttemptPickupItem(item);
			}
			return false;
		}
		return false;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "ItemInteractable";
	}
}
