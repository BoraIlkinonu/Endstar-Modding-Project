using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class BasicProp : EndlessBehaviour, IBaseType, IComponentBase
{
	private Context context;

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Context Context => context ?? (context = new Context(WorldObject));

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}
}
