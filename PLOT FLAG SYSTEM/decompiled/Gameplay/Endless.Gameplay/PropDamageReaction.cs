using System.Linq;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class PropDamageReaction : MaterialModifier, IStartSubscriber
{
	[SerializeField]
	[HideInInspector]
	private HittableComponent hittableComponent;

	private void Awake()
	{
		propBlock = new MaterialPropertyBlock();
	}

	public void Initialize(HittableComponent hittable, HittableReferences references)
	{
		hittableComponent = hittable;
		renderers = references.HitFlashRenderers.ToList();
	}

	private void HandleDamaged(HittableComponent hittable, HealthModificationArgs args)
	{
		StartHurtFlash();
	}

	public void EndlessStart()
	{
		hittableComponent.OnDamaged += HandleDamaged;
	}
}
