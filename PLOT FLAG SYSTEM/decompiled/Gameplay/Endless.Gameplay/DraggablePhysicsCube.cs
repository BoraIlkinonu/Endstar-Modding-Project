using System;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class DraggablePhysicsCube : EndlessNetworkBehaviour, IBaseType, IComponentBase
{
	[SerializeField]
	public PhysicsCubeController PhysicsCubeController;

	private Context context;

	[HideInInspector]
	[SerializeField]
	private DraggablePhysicsCubeReferences references;

	public Context Context => context ?? (context = new Context(WorldObject));

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(DraggablePhysicsCubeReferences);

	public NavType NavValue => NavType.Intangible;

	public GameObject GetAppearanceObject()
	{
		return references.VisualsBaseTransform.gameObject;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (DraggablePhysicsCubeReferences)referenceBase;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "DraggablePhysicsCube";
	}
}
