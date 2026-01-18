using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000DF RID: 223
	public class WorldUsableInteractable : InteractableBase
	{
		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060004D3 RID: 1235 RVA: 0x00018D03 File Offset: 0x00016F03
		public WorldUsableController WorldUsableController
		{
			get
			{
				return this.worldUsableController;
			}
		}

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x060004D4 RID: 1236 RVA: 0x00018D0B File Offset: 0x00016F0B
		public WorldUsableDefinition WorldUsableDefinition
		{
			get
			{
				return this.worldUsableController.WorldUsableDefinition;
			}
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x00018D18 File Offset: 0x00016F18
		public override void OnNetworkSpawn()
		{
			if (base.IsServer)
			{
				this.interactionAnimation.Value = this.interactionAnim;
				this.interactionDuration.Value = this.interactionDur;
				this.isHeldInteraction.Value = this.heldInteraction;
				this.hidePromptDuringInteraction.Value = this.hidePromptDuringActiveInteraction;
			}
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x00018D74 File Offset: 0x00016F74
		protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
		{
			if (this.worldUsableController.AttemptInteract(interactor, colliderIndex))
			{
				PlayerInteractor playerInteractor = interactor as PlayerInteractor;
				if (playerInteractor)
				{
					playerInteractor.PlayerReferenceManager.PlayerController.WorldInteractableUseQueue = this;
				}
				return true;
			}
			return false;
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x00018DB3 File Offset: 0x00016FB3
		public override void InteractionStopped(InteractorBase interactor)
		{
			base.InteractionStopped(interactor);
			this.worldUsableController.CancelInteraction(interactor);
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x00018DC8 File Offset: 0x00016FC8
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x00016E27 File Offset: 0x00015027
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x00018DDE File Offset: 0x00016FDE
		protected internal override string __getTypeName()
		{
			return "WorldUsableInteractable";
		}

		// Token: 0x040003E1 RID: 993
		[SerializeField]
		private WorldUsableController worldUsableController;

		// Token: 0x040003E2 RID: 994
		[SerializeField]
		private InteractionAnimation interactionAnim;

		// Token: 0x040003E3 RID: 995
		[SerializeField]
		private float interactionDur;

		// Token: 0x040003E4 RID: 996
		[SerializeField]
		private bool heldInteraction;

		// Token: 0x040003E5 RID: 997
		[SerializeField]
		private bool hidePromptDuringActiveInteraction;
	}
}
