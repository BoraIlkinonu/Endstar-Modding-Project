using System;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;

namespace Endless.Gameplay;

public interface IComponentBase
{
	WorldObject WorldObject { get; }

	Type ComponentReferenceType => null;

	ReferenceFilter Filter => ReferenceFilter.NonStatic;

	NavType NavValue => NavType.Static;

	void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
	}

	void PrefabInitialize(WorldObject worldObject);
}
