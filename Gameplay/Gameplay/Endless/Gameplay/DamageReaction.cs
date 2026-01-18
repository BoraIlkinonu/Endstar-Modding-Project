using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000CE RID: 206
	public class DamageReaction : MonoBehaviour
	{
		// Token: 0x06000421 RID: 1057 RVA: 0x00002DB0 File Offset: 0x00000FB0
		private void Start()
		{
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x000169BC File Offset: 0x00014BBC
		private void HandleHealthChanged(int oldHealth, int newHealth)
		{
			if (oldHealth > newHealth && oldHealth > 0)
			{
				this.TriggerDamageReaction();
			}
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x000169CC File Offset: 0x00014BCC
		public void TriggerDamageReaction()
		{
			if (this.references.ApperanceController.AppearanceAnimator)
			{
				this.references.ApperanceController.AppearanceAnimator.Animator.SetTrigger("Flinch");
				this.references.ApperanceController.AppearanceAnimator.MaterialModifier.StartHurtFlash();
			}
		}

		// Token: 0x040003A5 RID: 933
		[SerializeField]
		private PlayerReferenceManager references;
	}
}
