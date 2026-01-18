using System;
using Endless.Data;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000DE RID: 222
	public class PlayerInteractor : InteractorBase
	{
		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x060004C0 RID: 1216 RVA: 0x000188D0 File Offset: 0x00016AD0
		protected override bool IsActive
		{
			get
			{
				return base.IsOwner && this.playerReferenceManager.PlayerDownedComponent != null && !this.playerReferenceManager.PlayerDownedComponent.GetDowned(NetClock.CurrentFrame) && this.playerReferenceManager.PlayerNetworkController.ActiveInputSettings.CanInteract();
			}
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x060004C1 RID: 1217 RVA: 0x00018926 File Offset: 0x00016B26
		protected override global::UnityEngine.Vector3 Position
		{
			get
			{
				return this.playerReferenceManager.ApperanceController.VisualsTransform.position;
			}
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x060004C2 RID: 1218 RVA: 0x0001893D File Offset: 0x00016B3D
		protected override global::UnityEngine.Vector3 Forward
		{
			get
			{
				return this.forward;
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x060004C3 RID: 1219 RVA: 0x00018945 File Offset: 0x00016B45
		public PlayerReferenceManager PlayerReferenceManager
		{
			get
			{
				return this.playerReferenceManager;
			}
		}

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x060004C4 RID: 1220 RVA: 0x0001894D File Offset: 0x00016B4D
		public override Context ContextObject
		{
			get
			{
				return this.playerReferenceManager.PlayerContext;
			}
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060004C5 RID: 1221 RVA: 0x0001895A File Offset: 0x00016B5A
		// (set) Token: 0x060004C6 RID: 1222 RVA: 0x00018962 File Offset: 0x00016B62
		public InteractableBase MostRecentInteraction { get; set; }

		// Token: 0x060004C7 RID: 1223 RVA: 0x0001896B File Offset: 0x00016B6B
		public override void HandleFailedInteraction(InteractableBase interactable)
		{
			base.HandleFailedInteraction(interactable);
			Action<InteractableBase> onInteractionFailed = PlayerInteractor.OnInteractionFailed;
			if (onInteractionFailed != null)
			{
				onInteractionFailed(interactable);
			}
			Debug.Log("Interaction failed");
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x0001898F File Offset: 0x00016B8F
		private void Awake()
		{
			this.playerInputActions = new PlayerInputActions();
			this.OnInteractionChanged.AddListener(new UnityAction<InteractableBase, InteractableBase>(this.HandleInteractionChanged));
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x000189B4 File Offset: 0x00016BB4
		private void OnEnable()
		{
			this.playerInputActions.Player.Interact.Enable();
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x000189DC File Offset: 0x00016BDC
		private void OnDisable()
		{
			this.playerInputActions.Player.Interact.Disable();
			if (this.playerReferenceManager.IsOwner)
			{
				base.CurrentInteractableTarget = null;
				base.AbandonInteraction_ServerRPC();
			}
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x00018A1C File Offset: 0x00016C1C
		public override void HandleInteractionSucceeded(InteractableBase interactableBase)
		{
			base.HandleInteractionSucceeded(interactableBase);
			if (!base.IsOwner)
			{
				return;
			}
			if (interactableBase.HidePromptDuringInteraction)
			{
				base.CurrentInteractableTarget.UnselectLocally();
			}
			if (base.IsOwner && !this.playerInputActions.Player.Interact.IsPressed() && interactableBase.IsHeldInteraction)
			{
				base.AbandonInteraction_ServerRPC();
			}
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x00018A7C File Offset: 0x00016C7C
		private void HandleInteractionChanged(InteractableBase oldValue, InteractableBase newValue)
		{
			if (oldValue != null)
			{
				oldValue.InteractionAnimation.Stop(this.playerReferenceManager.ApperanceController.AppearanceAnimator.Animator);
			}
			if (newValue != null)
			{
				newValue.InteractionAnimation.Start(this.playerReferenceManager.ApperanceController.AppearanceAnimator.Animator);
			}
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x00018ADC File Offset: 0x00016CDC
		protected override void Update()
		{
			if (base.IsOwner)
			{
				if (!this.playerReferenceManager.PlayerNetworkController.ActiveInputSettings.CanInteract())
				{
					base.AbandonInteraction_ServerRPC();
					base.CurrentInteractableTarget = null;
					base.CurrentInteractable = null;
				}
				else if (base.CurrentInteractable && !base.CurrentInteractableTarget)
				{
					base.AbandonInteraction_ServerRPC();
				}
				if (base.CurrentInteractableTarget)
				{
					this.interactInput = this.playerInputActions.Player.Interact.WasPressedThisFrame();
					this.interactInputStop = this.playerInputActions.Player.Interact.WasReleasedThisFrame();
					if ((!base.CurrentInteractable || base.CurrentInteractableTarget.InteractableBase == base.CurrentInteractable) && this.interactInput && !this.playerReferenceManager.PlayerController.CurrentState.GameplayTeleport)
					{
						base.CurrentInteractableTarget.AttemptInteract(this);
					}
					if (base.CurrentInteractable && base.CurrentInteractableTarget.InteractableBase != base.CurrentInteractable)
					{
						base.AbandonInteraction_ServerRPC();
					}
					if (base.CurrentInteractable && base.CurrentInteractable.IsHeldInteraction && this.interactInputStop)
					{
						base.AbandonInteraction_ServerRPC();
						if (base.CurrentInteractableTarget)
						{
							base.CurrentInteractableTarget.SelectLocally();
						}
					}
					this.interactInput = false;
					this.interactInputStop = false;
				}
			}
			global::UnityEngine.Vector3 vector;
			if (this.playerReferenceManager.ApperanceController.AppearanceAnimator.GetAimIKForward(out vector))
			{
				this.forward = new global::UnityEngine.Vector3(vector.x, 0f, vector.z).normalized;
			}
			else
			{
				this.forward = this.playerReferenceManager.ApperanceController.VisualsTransform.forward;
			}
			base.Update();
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x00018CB6 File Offset: 0x00016EB6
		public void ToggleInteract(bool state)
		{
			this.interactInput = state;
			this.interactInputStop = !state;
		}

		// Token: 0x060004D0 RID: 1232 RVA: 0x00018CDC File Offset: 0x00016EDC
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060004D1 RID: 1233 RVA: 0x00018CF2 File Offset: 0x00016EF2
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x00018CFC File Offset: 0x00016EFC
		protected internal override string __getTypeName()
		{
			return "PlayerInteractor";
		}

		// Token: 0x040003D9 RID: 985
		public UnityEvent<bool> OnCanInteractChanged = new UnityEvent<bool>();

		// Token: 0x040003DA RID: 986
		[SerializeField]
		private GameplayPlayerReferenceManager playerReferenceManager;

		// Token: 0x040003DB RID: 987
		private PlayerInputActions playerInputActions;

		// Token: 0x040003DC RID: 988
		private bool interactInput;

		// Token: 0x040003DD RID: 989
		private bool interactInputStop;

		// Token: 0x040003DF RID: 991
		public static Action<InteractableBase> OnInteractionFailed;

		// Token: 0x040003E0 RID: 992
		private global::UnityEngine.Vector3 forward;
	}
}
