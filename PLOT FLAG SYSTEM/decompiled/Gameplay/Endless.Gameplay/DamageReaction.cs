using UnityEngine;

namespace Endless.Gameplay;

public class DamageReaction : MonoBehaviour
{
	[SerializeField]
	private PlayerReferenceManager references;

	private void Start()
	{
	}

	private void HandleHealthChanged(int oldHealth, int newHealth)
	{
		if (oldHealth > newHealth && oldHealth > 0)
		{
			TriggerDamageReaction();
		}
	}

	public void TriggerDamageReaction()
	{
		if ((bool)references.ApperanceController.AppearanceAnimator)
		{
			references.ApperanceController.AppearanceAnimator.Animator.SetTrigger("Flinch");
			references.ApperanceController.AppearanceAnimator.MaterialModifier.StartHurtFlash();
		}
	}
}
