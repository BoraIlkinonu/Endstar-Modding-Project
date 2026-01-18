using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class StaticProp : MonoBehaviour, IBaseType, IComponentBase
{
	private Context context;

	public Context Context => context ?? (context = new Context(WorldObject));

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public ReferenceFilter Filter => ReferenceFilter.None;

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}
}
