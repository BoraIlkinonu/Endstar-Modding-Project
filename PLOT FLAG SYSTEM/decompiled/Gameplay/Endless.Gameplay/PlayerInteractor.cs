using System;
using Endless.Data;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class PlayerInteractor : InteractorBase
{
	public UnityEvent<bool> OnCanInteractChanged = new UnityEvent<bool>();

	[SerializeField]
	private GameplayPlayerReferenceManager playerReferenceManager;

	private PlayerInputActions playerInputActions;

	private bool interactInput;

	private bool interactInputStop;

	public static Action<InteractableBase> OnInteractionFailed;

	private UnityEngine.Vector3 forward;

	protected override bool IsActive
	{
		get
		{
			if (base.IsOwner && playerReferenceManager.PlayerDownedComponent != null && !playerReferenceManager.PlayerDownedComponent.GetDowned(NetClock.CurrentFrame))
			{
				return playerReferenceManager.PlayerNetworkController.ActiveInputSettings.CanInteract();
			}
			return false;
		}
	}

	protected override UnityEngine.Vector3 Position => playerReferenceManager.ApperanceController.VisualsTransform.position;

	protected override UnityEngine.Vector3 Forward => forward;

	public PlayerReferenceManager PlayerReferenceManager => playerReferenceManager;

	public override Context ContextObject => playerReferenceManager.PlayerContext;

	public InteractableBase MostRecentInteraction { get; set; }

	public override void HandleFailedInteraction(InteractableBase interactable)
	{
		base.HandleFailedInteraction(interactable);
		OnInteractionFailed?.Invoke(interactable);
		Debug.Log("Interaction failed");
	}

	private void Awake()
	{
		playerInputActions = new PlayerInputActions();
		OnInteractionChanged.AddListener(HandleInteractionChanged);
	}

	private void OnEnable()
	{
		playerInputActions.Player.Interact.Enable();
	}

	private void OnDisable()
	{
		playerInputActions.Player.Interact.Disable();
		if (playerReferenceManager.IsOwner)
		{
			base.CurrentInteractableTarget = null;
			AbandonInteraction_ServerRPC();
		}
	}

	public override void HandleInteractionSucceeded(InteractableBase interactableBase)
	{
		base.HandleInteractionSucceeded(interactableBase);
		if (base.IsOwner)
		{
			if (interactableBase.HidePromptDuringInteraction)
			{
				base.CurrentInteractableTarget.UnselectLocally();
			}
			if (base.IsOwner && !playerInputActions.Player.Interact.IsPressed() && interactableBase.IsHeldInteraction)
			{
				AbandonInteraction_ServerRPC();
			}
		}
	}

	private void HandleInteractionChanged(InteractableBase oldValue, InteractableBase newValue)
	{
		if (oldValue != null)
		{
			oldValue.InteractionAnimation.Stop(playerReferenceManager.ApperanceController.AppearanceAnimator.Animator);
		}
		if (newValue != null)
		{
			newValue.InteractionAnimation.Start(playerReferenceManager.ApperanceController.AppearanceAnimator.Animator);
		}
	}

	protected override void Update()
	{
		if (base.IsOwner)
		{
			if (!playerReferenceManager.PlayerNetworkController.ActiveInputSettings.CanInteract())
			{
				AbandonInteraction_ServerRPC();
				base.CurrentInteractableTarget = null;
				base.CurrentInteractable = null;
			}
			else if ((bool)base.CurrentInteractable && !base.CurrentInteractableTarget)
			{
				AbandonInteraction_ServerRPC();
			}
			if ((bool)base.CurrentInteractableTarget)
			{
				interactInput = playerInputActions.Player.Interact.WasPressedThisFrame();
				interactInputStop = playerInputActions.Player.Interact.WasReleasedThisFrame();
				if ((!base.CurrentInteractable || base.CurrentInteractableTarget.InteractableBase == base.CurrentInteractable) && interactInput && !playerReferenceManager.PlayerController.CurrentState.GameplayTeleport)
				{
					base.CurrentInteractableTarget.AttemptInteract(this);
				}
				if ((bool)base.CurrentInteractable && base.CurrentInteractableTarget.InteractableBase != base.CurrentInteractable)
				{
					AbandonInteraction_ServerRPC();
				}
				if ((bool)base.CurrentInteractable && base.CurrentInteractable.IsHeldInteraction && interactInputStop)
				{
					AbandonInteraction_ServerRPC();
					if ((bool)base.CurrentInteractableTarget)
					{
						base.CurrentInteractableTarget.SelectLocally();
					}
				}
				interactInput = false;
				interactInputStop = false;
			}
		}
		if (playerReferenceManager.ApperanceController.AppearanceAnimator.GetAimIKForward(out var vector))
		{
			forward = new UnityEngine.Vector3(vector.x, 0f, vector.z).normalized;
		}
		else
		{
			forward = playerReferenceManager.ApperanceController.VisualsTransform.forward;
		}
		base.Update();
	}

	public void ToggleInteract(bool state)
	{
		interactInput = state;
		interactInputStop = !state;
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
		return "PlayerInteractor";
	}
}
