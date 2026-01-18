using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public class WorldUsableInteractable : InteractableBase
{
	[SerializeField]
	private WorldUsableController worldUsableController;

	[SerializeField]
	private InteractionAnimation interactionAnim;

	[SerializeField]
	private float interactionDur;

	[SerializeField]
	private bool heldInteraction;

	[SerializeField]
	private bool hidePromptDuringActiveInteraction;

	public WorldUsableController WorldUsableController => worldUsableController;

	public WorldUsableDefinition WorldUsableDefinition => worldUsableController.WorldUsableDefinition;

	public override void OnNetworkSpawn()
	{
		if (base.IsServer)
		{
			interactionAnimation.Value = interactionAnim;
			interactionDuration.Value = interactionDur;
			isHeldInteraction.Value = heldInteraction;
			hidePromptDuringInteraction.Value = hidePromptDuringActiveInteraction;
		}
	}

	protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
	{
		if (worldUsableController.AttemptInteract(interactor, colliderIndex))
		{
			PlayerInteractor playerInteractor = interactor as PlayerInteractor;
			if ((bool)playerInteractor)
			{
				playerInteractor.PlayerReferenceManager.PlayerController.WorldInteractableUseQueue = this;
			}
			return true;
		}
		return false;
	}

	public override void InteractionStopped(InteractorBase interactor)
	{
		base.InteractionStopped(interactor);
		worldUsableController.CancelInteraction(interactor);
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
		return "WorldUsableInteractable";
	}
}
