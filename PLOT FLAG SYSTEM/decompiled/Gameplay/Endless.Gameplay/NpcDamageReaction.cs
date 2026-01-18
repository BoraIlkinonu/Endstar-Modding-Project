using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay;

public class NpcDamageReaction : NpcComponent, IStartSubscriber
{
	private void HandleHealthChanged(HealthComponent.HealthLostData damageReactionData)
	{
		if (damageReactionData.damageDelta <= 0 && base.NpcEntity.NetworkedDamageMode.Value != DamageMode.IgnoreDamage)
		{
			base.NpcEntity.Components.MaterialModifier?.StartHurtFlash();
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Flinch);
		}
	}

	public void EndlessStart()
	{
		base.NpcEntity.Components.Health.OnHealthLost.AddListener(HandleHealthChanged);
	}
}
