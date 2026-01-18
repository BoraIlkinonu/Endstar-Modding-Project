using UnityEngine;

namespace Endless.Gameplay;

public class WorldTriggerCollider : MonoBehaviour
{
	[field: SerializeField]
	public WorldTrigger WorldTrigger { get; private set; }

	public void Initialize(WorldTrigger worldTrigger)
	{
		WorldTrigger = worldTrigger;
	}
}
