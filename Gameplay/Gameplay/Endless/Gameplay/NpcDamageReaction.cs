using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000142 RID: 322
	public class NpcDamageReaction : NpcComponent, IStartSubscriber
	{
		// Token: 0x0600078A RID: 1930 RVA: 0x000236F4 File Offset: 0x000218F4
		private void HandleHealthChanged(HealthComponent.HealthLostData damageReactionData)
		{
			if (damageReactionData.damageDelta > 0 || base.NpcEntity.NetworkedDamageMode.Value == DamageMode.IgnoreDamage)
			{
				return;
			}
			MaterialModifier materialModifier = base.NpcEntity.Components.MaterialModifier;
			if (materialModifier != null)
			{
				materialModifier.StartHurtFlash();
			}
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Flinch);
		}

		// Token: 0x0600078B RID: 1931 RVA: 0x00023753 File Offset: 0x00021953
		public void EndlessStart()
		{
			base.NpcEntity.Components.Health.OnHealthLost.AddListener(new UnityAction<HealthComponent.HealthLostData>(this.HandleHealthChanged));
		}
	}
}
