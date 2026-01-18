using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000DC RID: 220
	public class ItemInteractable : InteractableBase
	{
		// Token: 0x060004A7 RID: 1191 RVA: 0x00018380 File Offset: 0x00016580
		protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
		{
			PlayerInteractor playerInteractor = (PlayerInteractor)interactor;
			return playerInteractor != null && this.item.IsPickupable && base.AttemptInteract_ServerLogic(interactor, colliderIndex) && playerInteractor.PlayerReferenceManager.Inventory.AttemptPickupItem(this.item, false);
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x000183D8 File Offset: 0x000165D8
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x00016E27 File Offset: 0x00015027
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x000183EE File Offset: 0x000165EE
		protected internal override string __getTypeName()
		{
			return "ItemInteractable";
		}

		// Token: 0x040003D4 RID: 980
		[SerializeField]
		private Item item;
	}
}
