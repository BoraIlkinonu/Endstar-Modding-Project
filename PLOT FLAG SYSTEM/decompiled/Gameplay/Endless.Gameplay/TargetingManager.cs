using System.Collections.Generic;
using Endless.Shared;

namespace Endless.Gameplay;

public class TargetingManager : MonoBehaviourSingleton<TargetingManager>
{
	public Dictionary<WorldObject, HittableComponent> TargetableMap { get; } = new Dictionary<WorldObject, HittableComponent>();

	public List<HittableComponent> Targetables { get; } = new List<HittableComponent>();

	public void AddTargetable(HittableComponent targetable)
	{
		TargetableMap.Add(targetable.WorldObject, targetable);
		Targetables.Add(targetable);
	}

	public void RemoveTargetable(HittableComponent targetable)
	{
		TargetableMap.Remove(targetable.WorldObject);
		Targetables.Remove(targetable);
	}
}
