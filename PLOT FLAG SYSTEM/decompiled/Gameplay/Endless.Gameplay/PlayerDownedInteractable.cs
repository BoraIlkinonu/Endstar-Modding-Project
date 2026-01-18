using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public class PlayerDownedInteractable : InteractableBase
{
	[SerializeField]
	private GameplayPlayerReferenceManager playerReferenceManager;

	private InteractorBase activeInteraction;

	private double interactionStartTime;

	protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
	{
		int num;
		if (activeInteraction == null && (bool)playerReferenceManager)
		{
			num = ((playerReferenceManager.HealthComponent.CurrentHealth < 1) ? 1 : 0);
			if (num != 0)
			{
				activeInteraction = interactor;
				playerReferenceManager.PlayerDownedComponent.SetReviving(b: true);
				interactionStartTime = base.NetworkManager.ServerTime.Time;
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	public override void InteractionStopped(InteractorBase interactor)
	{
		if (interactor == activeInteraction)
		{
			playerReferenceManager.PlayerDownedComponent.SetReviving(b: false);
			activeInteraction = null;
		}
		base.InteractionStopped(interactor);
	}

	private void Update()
	{
		if ((bool)activeInteraction && base.NetworkManager.ServerTime.Time >= interactionStartTime + (double)base.InteractionDuration)
		{
			playerReferenceManager.PlayerDownedComponent.HandleReviveInteractionCompleted(activeInteraction);
			activeInteraction = null;
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		base.InteractionDuration = 1f;
		base.IsHeldInteraction = true;
		base.InteractionAnimation = InteractionAnimation.Revive;
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
		return "PlayerDownedInteractable";
	}
}
