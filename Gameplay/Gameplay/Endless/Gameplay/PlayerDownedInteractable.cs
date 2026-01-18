using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000273 RID: 627
	public class PlayerDownedInteractable : InteractableBase
	{
		// Token: 0x06000D34 RID: 3380 RVA: 0x000481D0 File Offset: 0x000463D0
		protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
		{
			bool flag = this.activeInteraction == null && this.playerReferenceManager && this.playerReferenceManager.HealthComponent.CurrentHealth < 1;
			if (flag)
			{
				this.activeInteraction = interactor;
				this.playerReferenceManager.PlayerDownedComponent.SetReviving(true);
				this.interactionStartTime = base.NetworkManager.ServerTime.Time;
			}
			return flag;
		}

		// Token: 0x06000D35 RID: 3381 RVA: 0x00048242 File Offset: 0x00046442
		public override void InteractionStopped(InteractorBase interactor)
		{
			if (interactor == this.activeInteraction)
			{
				this.playerReferenceManager.PlayerDownedComponent.SetReviving(false);
				this.activeInteraction = null;
			}
			base.InteractionStopped(interactor);
		}

		// Token: 0x06000D36 RID: 3382 RVA: 0x00048274 File Offset: 0x00046474
		private void Update()
		{
			if (this.activeInteraction && base.NetworkManager.ServerTime.Time >= this.interactionStartTime + (double)base.InteractionDuration)
			{
				this.playerReferenceManager.PlayerDownedComponent.HandleReviveInteractionCompleted(this.activeInteraction);
				this.activeInteraction = null;
			}
		}

		// Token: 0x06000D37 RID: 3383 RVA: 0x000482CE File Offset: 0x000464CE
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			base.InteractionDuration = 1f;
			base.IsHeldInteraction = true;
			base.InteractionAnimation = InteractionAnimation.Revive;
		}

		// Token: 0x06000D39 RID: 3385 RVA: 0x000482F0 File Offset: 0x000464F0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000D3A RID: 3386 RVA: 0x00016E27 File Offset: 0x00015027
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000D3B RID: 3387 RVA: 0x00048306 File Offset: 0x00046506
		protected internal override string __getTypeName()
		{
			return "PlayerDownedInteractable";
		}

		// Token: 0x04000C33 RID: 3123
		[SerializeField]
		private GameplayPlayerReferenceManager playerReferenceManager;

		// Token: 0x04000C34 RID: 3124
		private InteractorBase activeInteraction;

		// Token: 0x04000C35 RID: 3125
		private double interactionStartTime;
	}
}
