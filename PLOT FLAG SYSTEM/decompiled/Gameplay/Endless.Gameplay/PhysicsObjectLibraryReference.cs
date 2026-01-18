using System;

namespace Endless.Gameplay;

[Serializable]
public class PhysicsObjectLibraryReference : PropLibraryReference
{
	internal override ReferenceFilter Filter => ReferenceFilter.PhysicsObject;
}
