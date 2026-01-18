using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

public class HittableMap : EndlessBehaviourSingleton<HittableMap>
{
	private readonly Dictionary<Collider, HittableComponent> hittableMap = new Dictionary<Collider, HittableComponent>();

	private readonly Dictionary<int, HittableComponent> colliderIdMap = new Dictionary<int, HittableComponent>();

	public void AddCollidersToMaps(HittableComponent hittableComponent)
	{
		foreach (Collider hittableCollider in hittableComponent.HittableColliders)
		{
			hittableMap.Add(hittableCollider, hittableComponent);
			colliderIdMap.Add(hittableCollider.GetInstanceID(), hittableComponent);
		}
	}

	public void RemoveCollidersFromMaps(HittableComponent hittableComponent)
	{
		foreach (Collider hittableCollider in hittableComponent.HittableColliders)
		{
			hittableMap.Remove(hittableCollider);
			colliderIdMap.Remove(hittableCollider.GetInstanceID());
		}
	}

	public HittableComponent GetHittableFromMap(Collider hitCollider)
	{
		return hittableMap.GetValueOrDefault(hitCollider);
	}

	public HittableComponent GetHittableFromMap(int instanceId)
	{
		return colliderIdMap.GetValueOrDefault(instanceId);
	}
}
