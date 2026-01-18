using System;
using Endless.Props.ReferenceComponents;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class ShieldComponent : EndlessBehaviour, IComponentBase
{
	public UnityEvent OnShieldReducedReaction = new UnityEvent();

	public UnityEvent OnShieldLostReaction = new UnityEvent();

	public UnityEvent OnShieldGainedReaction = new UnityEvent();

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(ShieldComponentReferences);

	public int DamageShields(HealthModificationArgs args)
	{
		return args.Delta;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}
}
